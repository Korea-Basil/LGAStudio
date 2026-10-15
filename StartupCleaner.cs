using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LGAStudio;

internal enum StartupItemKind
{
    Registry,
    ScheduledTask
}

internal sealed record StartupItem(
    StartupItemKind Kind,
    string Name,
    string Location,
    string Command,
    string Assessment,
    RegistryHive? Hive = null,
    RegistryView? View = null,
    string? ValueName = null,
    string? TaskFolder = null);

internal sealed record StartupScanResult(
    IReadOnlyList<StartupItem> Items,
    IReadOnlyList<string> Warnings);

// Scans the two standard Run keys and recurring scheduled tasks; nothing is removed during scanning.
internal static class StartupCleaner
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private static readonly string[] ScriptOrProxyExecutables =
        ["powershell.exe", "pwsh.exe", "wscript.exe", "cscript.exe", "mshta.exe", "rundll32.exe", "cmd.exe"];
    private static readonly string[] BrowserExecutables =
        ["chrome.exe", "msedge.exe", "brave.exe", "vivaldi.exe", "opera.exe", "firefox.exe", "chromium.exe"];

    public static StartupScanResult Scan()
    {
        var items = new List<StartupItem>();
        var warnings = new List<string>();
        ScanRegistry(RegistryHive.CurrentUser, items, warnings);
        ScanRegistry(RegistryHive.LocalMachine, items, warnings);
        ScanScheduledTasks(items, warnings);
        return new StartupScanResult(items, warnings);
    }

    public static void Remove(StartupItem item)
    {
        if (item.Kind == StartupItemKind.Registry)
        {
            if (item.Hive is null || item.View is null || item.ValueName is null)
                throw new InvalidOperationException("The selected registry item is missing its registry identity.");

            using RegistryKey baseKey = RegistryKey.OpenBaseKey(item.Hive.Value, item.View.Value);
            using RegistryKey? runKey = baseKey.OpenSubKey(RunKeyPath, writable: true);
            if (runKey is null)
                throw new InvalidOperationException($"The startup registry key no longer exists: {item.Location}");
            runKey.DeleteValue(item.ValueName, throwOnMissingValue: true);
            return;
        }

        if (item.TaskFolder is null)
            throw new InvalidOperationException("The selected scheduled task is missing its folder.");

        DeleteScheduledTask(item.TaskFolder, item.Name);
    }

    private static void ScanRegistry(RegistryHive hive, ICollection<StartupItem> items, ICollection<string> warnings)
    {
        RegistryView[] views = Environment.Is64BitOperatingSystem
            ? [RegistryView.Registry64, RegistryView.Registry32]
            : [RegistryView.Default];

        foreach (RegistryView view in views)
        {
            string displayHive = hive == RegistryHive.CurrentUser ? "HKCU" : "HKLM";
            string location = $"{displayHive}\\{RunKeyPath} ({view})";
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                using RegistryKey? runKey = baseKey.OpenSubKey(RunKeyPath);
                if (runKey is null)
                    continue;

                foreach (string valueName in runKey.GetValueNames())
                {
                    try
                    {
                        object? rawValue = runKey.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        if (rawValue is null)
                            continue;

                        string command = rawValue is byte[] bytes
                            ? Convert.ToHexString(bytes)
                            : Convert.ToString(rawValue, System.Globalization.CultureInfo.InvariantCulture) ?? "";
                        if (string.IsNullOrWhiteSpace(command))
                            continue;

                        string assessment = AssessExecutable(command);
                        items.Add(new StartupItem(
                            StartupItemKind.Registry,
                            valueName,
                            location,
                            command,
                            assessment,
                            hive,
                            view,
                            valueName));
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
                    {
                        warnings.Add($"Could not read {location}\\{valueName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                warnings.Add($"Could not inspect {location}: {ex.Message}");
            }
        }
    }

    private static string AssessExecutable(string command)
    {
        string expanded = Environment.ExpandEnvironmentVariables(command.Trim());
        string executable = ExtractExecutable(expanded);
        string arguments = expanded.Length > executable.Length ? expanded[executable.Length..].Trim() : "";
        string normalizedPath = executable.Replace('/', '\\');
        string lowerPath = normalizedPath.ToLowerInvariant();
        string lowerArgs = arguments.ToLowerInvariant();

        bool inTemp = lowerPath.Contains(@"\appdata\local\temp\", StringComparison.Ordinal) ||
                      lowerPath.Contains(@"\windows\temp\", StringComparison.Ordinal);
        bool inDownloads = lowerPath.Contains(@"\downloads\", StringComparison.Ordinal);
        bool inUserProfile = lowerPath.StartsWith(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).TrimEnd('\\').ToLowerInvariant() + "\\",
            StringComparison.Ordinal);
        bool suspiciousArguments = HasSuspiciousArguments(lowerArgs);
        string executableName = Path.GetFileName(lowerPath);

        if (inTemp)
            return "Suspicious: executable launches from a temporary directory.";
        if (inDownloads)
            return "Suspicious: executable launches from Downloads.";
        if (inUserProfile && suspiciousArguments)
            return "Suspicious: user-profile executable uses potentially concealed or remote arguments.";
        if (ScriptOrProxyExecutables.Contains(executableName, StringComparer.OrdinalIgnoreCase) && suspiciousArguments)
            return "Suspicious: script/proxy executable uses potentially concealed or remote arguments.";
        if (inUserProfile)
            return "Review: executable is stored in the user profile; this can be legitimate.";
        if (ScriptOrProxyExecutables.Contains(executableName, StringComparer.OrdinalIgnoreCase))
            return "Review: startup uses a script or proxy executable.";
        return "No obvious suspicious path or argument pattern detected.";
    }

    private static bool HasSuspiciousArguments(string arguments)
    {
        return arguments.Contains("http://", StringComparison.Ordinal) ||
               arguments.Contains("https://", StringComparison.Ordinal) ||
               arguments.Contains("javascript:", StringComparison.Ordinal) ||
               arguments.Contains("-encodedcommand", StringComparison.Ordinal) ||
               arguments.Contains(" -enc ", StringComparison.Ordinal) ||
               arguments.Contains("-windowstyle hidden", StringComparison.Ordinal) ||
               arguments.Contains("-w hidden", StringComparison.Ordinal) ||
               arguments.Contains(" --headless", StringComparison.Ordinal) ||
               arguments.Contains(" -e ", StringComparison.Ordinal);
    }

    private static string ExtractExecutable(string command)
    {
        if (command.Length == 0)
            return "";

        if (command[0] == '"')
        {
            int closingQuote = command.IndexOf('"', 1);
            return closingQuote > 1 ? command[1..closingQuote] : command.Trim('"');
        }

        int exeSuffix = command.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exeSuffix >= 0)
            return command[..(exeSuffix + 4)].Trim();

        int whitespace = command.IndexOfAny([' ', '\t']);
        return whitespace < 0 ? command : command[..whitespace];
    }

    private static void ScanScheduledTasks(ICollection<StartupItem> items, ICollection<string> warnings)
    {
        Type? schedulerType = Type.GetTypeFromProgID("Schedule.Service");
        if (schedulerType is null)
        {
            warnings.Add("Task Scheduler COM interface is unavailable; scheduled tasks were not inspected.");
            return;
        }

        object? serviceObject = null;
        object? rootObject = null;
        try
        {
            serviceObject = Activator.CreateInstance(schedulerType);
            if (serviceObject is null)
                throw new InvalidOperationException("Could not create the Task Scheduler service.");

            dynamic service = serviceObject;
            service.Connect();
            rootObject = service.GetFolder("\\");
            ScanTaskFolder(rootObject, "\\", items, warnings);
        }
        catch (Exception ex)
        {
            warnings.Add($"Could not inspect scheduled tasks: {ex.Message}");
        }
        finally
        {
            if (rootObject is not null && Marshal.IsComObject(rootObject))
                Marshal.FinalReleaseComObject(rootObject);
            if (serviceObject is not null && Marshal.IsComObject(serviceObject))
                Marshal.FinalReleaseComObject(serviceObject);
        }
    }

    private static void ScanTaskFolder(
        object folderObject,
        string folderPath,
        ICollection<StartupItem> items,
        ICollection<string> warnings)
    {
        dynamic folder = folderObject;
        object? tasksObject = null;
        object? foldersObject = null;

        try
        {
            tasksObject = folder.GetTasks(1);
            dynamic tasks = tasksObject;
            int taskCount = Convert.ToInt32(tasks.Count, System.Globalization.CultureInfo.InvariantCulture);
            for (int index = 1; index <= taskCount; index++)
            {
                object? taskObject = null;
                object? definitionObject = null;
                try
                {
                    taskObject = tasks.Item(index);
                    dynamic task = taskObject;
                    string taskName = Convert.ToString(task.Name, System.Globalization.CultureInfo.InvariantCulture) ?? "(unnamed task)";
                    definitionObject = task.Definition;
                    string? reason = FindPeriodicBrowserLaunch(definitionObject);
                    if (reason is null)
                        continue;

                    items.Add(new StartupItem(
                        StartupItemKind.ScheduledTask,
                        taskName,
                        folderPath,
                        reason,
                        "Suspicious: recurring task launches a browser or URL.",
                        TaskFolder: folderPath));
                }
                catch (Exception ex)
                {
                    warnings.Add($"Could not inspect scheduled task in {folderPath}: {ex.Message}");
                }
                finally
                {
                    if (definitionObject is not null && Marshal.IsComObject(definitionObject))
                        Marshal.FinalReleaseComObject(definitionObject);
                    if (taskObject is not null && Marshal.IsComObject(taskObject))
                        Marshal.FinalReleaseComObject(taskObject);
                }
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Could not inspect tasks in {folderPath}: {ex.Message}");
        }
        finally
        {
            if (tasksObject is not null && Marshal.IsComObject(tasksObject))
                Marshal.FinalReleaseComObject(tasksObject);
        }

        try
        {
            foldersObject = folder.GetFolders(0);
            dynamic folders = foldersObject;
            int folderCount = Convert.ToInt32(folders.Count, System.Globalization.CultureInfo.InvariantCulture);
            for (int index = 1; index <= folderCount; index++)
            {
                object? childObject = null;
                try
                {
                    childObject = folders.Item(index);
                    dynamic child = childObject;
                    string childName = Convert.ToString(child.Name, System.Globalization.CultureInfo.InvariantCulture) ?? "";
                    ScanTaskFolder(childObject, folderPath == "\\" ? "\\" + childName : folderPath + "\\" + childName, items, warnings);
                }
                catch (Exception ex)
                {
                    warnings.Add($"Could not inspect a scheduled-task folder beneath {folderPath}: {ex.Message}");
                }
                finally
                {
                    if (childObject is not null && Marshal.IsComObject(childObject))
                        Marshal.FinalReleaseComObject(childObject);
                }
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Could not enumerate scheduled-task folders beneath {folderPath}: {ex.Message}");
        }
        finally
        {
            if (foldersObject is not null && Marshal.IsComObject(foldersObject))
                Marshal.FinalReleaseComObject(foldersObject);
        }
    }

    private static string? FindPeriodicBrowserLaunch(dynamic definition)
    {
        if (!HasRecurringTrigger(definition))
            return null;

        object? actionsObject = definition.Actions;
        try
        {
            dynamic actions = actionsObject;
            int count = Convert.ToInt32(actions.Count, System.Globalization.CultureInfo.InvariantCulture);
            for (int index = 1; index <= count; index++)
            {
                object? actionObject = null;
                try
                {
                    actionObject = actions.Item(index);
                    dynamic action = actionObject;
                    if (Convert.ToInt32(action.Type, System.Globalization.CultureInfo.InvariantCulture) != 0)
                        continue;

                    string executable = Convert.ToString(action.Path, System.Globalization.CultureInfo.InvariantCulture) ?? "";
                    string arguments = Convert.ToString(action.Arguments, System.Globalization.CultureInfo.InvariantCulture) ?? "";
                    string executableName = Path.GetFileName(executable);
                    bool browser = BrowserExecutables.Contains(executableName, StringComparer.OrdinalIgnoreCase);
                    bool url = LooksLikeUrl(executable) || LooksLikeUrl(arguments);
                    if (browser || url)
                        return $"Executable: {executable}; arguments: {arguments}";
                }
                finally
                {
                    if (actionObject is not null && Marshal.IsComObject(actionObject))
                        Marshal.FinalReleaseComObject(actionObject);
                }
            }

            return null;
        }
        finally
        {
            if (actionsObject is not null && Marshal.IsComObject(actionsObject))
                Marshal.FinalReleaseComObject(actionsObject);
        }
    }

    private static bool HasRecurringTrigger(dynamic definition)
    {
        object? triggersObject = definition.Triggers;
        try
        {
            dynamic triggers = triggersObject;
            int count = Convert.ToInt32(triggers.Count, System.Globalization.CultureInfo.InvariantCulture);
            for (int index = 1; index <= count; index++)
            {
                object? triggerObject = null;
                try
                {
                    triggerObject = triggers.Item(index);
                    dynamic trigger = triggerObject;
                    int type = Convert.ToInt32(trigger.Type, System.Globalization.CultureInfo.InvariantCulture);
                    if (type is 2 or 3 or 4 or 5)
                        return true;

                    string interval = Convert.ToString(trigger.Repetition.Interval, System.Globalization.CultureInfo.InvariantCulture) ?? "";
                    if (!string.IsNullOrWhiteSpace(interval))
                        return true;
                }
                finally
                {
                    if (triggerObject is not null && Marshal.IsComObject(triggerObject))
                        Marshal.FinalReleaseComObject(triggerObject);
                }
            }

            return false;
        }
        finally
        {
            if (triggersObject is not null && Marshal.IsComObject(triggersObject))
                Marshal.FinalReleaseComObject(triggersObject);
        }
    }

    private static bool LooksLikeUrl(string value)
    {
        string lower = value.ToLowerInvariant();
        return lower.Contains("http://", StringComparison.Ordinal) ||
               lower.Contains("https://", StringComparison.Ordinal) ||
               lower.Contains("www.", StringComparison.Ordinal) ||
               lower.Contains("javascript:", StringComparison.Ordinal);
    }

    private static void DeleteScheduledTask(string folderPath, string taskName)
    {
        Type? schedulerType = Type.GetTypeFromProgID("Schedule.Service");
        if (schedulerType is null)
            throw new InvalidOperationException("Task Scheduler COM interface is unavailable.");

        object? serviceObject = null;
        object? folderObject = null;
        try
        {
            serviceObject = Activator.CreateInstance(schedulerType)
                ?? throw new InvalidOperationException("Could not create the Task Scheduler service.");
            dynamic service = serviceObject;
            service.Connect();
            folderObject = service.GetFolder(folderPath);
            dynamic folder = folderObject;
            folder.DeleteTask(taskName, 0);
        }
        finally
        {
            if (folderObject is not null && Marshal.IsComObject(folderObject))
                Marshal.FinalReleaseComObject(folderObject);
            if (serviceObject is not null && Marshal.IsComObject(serviceObject))
                Marshal.FinalReleaseComObject(serviceObject);
        }
    }
}
