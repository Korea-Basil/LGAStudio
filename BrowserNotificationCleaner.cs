using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;

namespace LGAStudio;

internal sealed record BrowserPermission(
    string Browser,
    string Profile,
    string Domain,
    string SourcePath,
    string PermissionKey,
    bool IsFirefox);

internal sealed record BrowserScanResult(
    IReadOnlyList<BrowserPermission> Permissions,
    IReadOnlyList<string> Warnings);

// Discovers known browser profile locations and reads only explicit ALLOW notification entries.
internal static class BrowserNotificationCleaner
{
    private const int ChromiumAllow = 1;
    private const int ChromiumBlock = 2;
    private const int FirefoxAllow = 1;
    private const int FirefoxBlock = 2;

    private static readonly (string Name, string Root)[] ChromiumRoots =
    [
        ("Chrome", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data")),
        ("Edge", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data")),
        ("Brave", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "User Data")),
        ("Vivaldi", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi", "User Data")),
        ("Chromium", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chromium", "User Data")),
        ("Opera", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software", "Opera Stable"))
    ];

    public static BrowserScanResult Scan()
    {
        var permissions = new List<BrowserPermission>();
        var warnings = new List<string>();

        foreach ((string browser, string root) in ChromiumRoots)
        {
            if (!Directory.Exists(root))
                continue;

            try
            {
                IEnumerable<string> profiles = Directory.EnumerateDirectories(root)
                    .Where(path => File.Exists(Path.Combine(path, "Preferences")));
                if (File.Exists(Path.Combine(root, "Preferences")))
                    profiles = profiles.Prepend(root);

                foreach (string profilePath in profiles)
                {
                    string preferencesPath = Path.Combine(profilePath, "Preferences");
                    try
                    {
                        ScanChromiumPreferences(browser, profilePath, preferencesPath, permissions);
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
                    {
                        warnings.Add($"Could not inspect {preferencesPath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add($"Could not enumerate {root}: {ex.Message}");
            }
        }

        string firefoxProfiles = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Mozilla", "Firefox", "Profiles");
        if (Directory.Exists(firefoxProfiles))
        {
            try
            {
                foreach (string profilePath in Directory.EnumerateDirectories(firefoxProfiles))
                {
                    string databasePath = Path.Combine(profilePath, "permissions.sqlite");
                    if (!File.Exists(databasePath))
                        continue;

                    try
                    {
                        ScanFirefoxDatabase(profilePath, databasePath, permissions);
                    }
                    catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
                    {
                        warnings.Add($"Could not inspect {databasePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add($"Could not enumerate {firefoxProfiles}: {ex.Message}");
            }
        }

        return new BrowserScanResult(permissions, warnings);
    }

    public static int Apply(BrowserPermission permission, bool block)
    {
        return permission.IsFirefox
            ? ApplyFirefox(permission, block)
            : ApplyChromium(permission, block);
    }

    private static void ScanChromiumPreferences(
        string browser,
        string profilePath,
        string preferencesPath,
        ICollection<BrowserPermission> results)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(preferencesPath));
        if (!document.RootElement.TryGetProperty("profile", out JsonElement profile) ||
            !profile.TryGetProperty("content_settings", out JsonElement contentSettings) ||
            !contentSettings.TryGetProperty("exceptions", out JsonElement exceptions) ||
            !exceptions.TryGetProperty("notifications", out JsonElement notifications) ||
            notifications.ValueKind != JsonValueKind.Object)
            return;

        foreach (JsonProperty entry in notifications.EnumerateObject())
        {
            if (entry.Value.ValueKind != JsonValueKind.Object ||
                !entry.Value.TryGetProperty("setting", out JsonElement setting) ||
                setting.ValueKind != JsonValueKind.Number ||
                setting.GetInt32() != ChromiumAllow)
                continue;

            string displayName = GetProfileName(profilePath, profile);
            results.Add(new BrowserPermission(
                browser,
                displayName,
                entry.Name,
                preferencesPath,
                entry.Name,
                IsFirefox: false));
        }
    }

    private static void ScanFirefoxDatabase(
        string profilePath,
        string databasePath,
        ICollection<BrowserPermission> results)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT origin FROM moz_perms WHERE type = 'desktop-notification' AND permission = $allow";
        command.Parameters.AddWithValue("$allow", FirefoxAllow);

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string origin = reader.GetString(0);
            results.Add(new BrowserPermission(
                "Firefox",
                Path.GetFileName(profilePath),
                origin,
                databasePath,
                origin,
                IsFirefox: true));
        }
    }

    private static int ApplyChromium(BrowserPermission permission, bool block)
    {
        JsonNode? root = JsonNode.Parse(File.ReadAllText(permission.SourcePath));
        JsonObject? notifications = root?["profile"]?["content_settings"]?["exceptions"]?["notifications"] as JsonObject;
        if (notifications is null || !notifications.ContainsKey(permission.PermissionKey))
            throw new InvalidDataException($"The notification entry was not found in {permission.SourcePath}. Scan again and retry.");

        JsonObject? entry = notifications[permission.PermissionKey] as JsonObject;
        if (entry is null ||
            entry["setting"] is not JsonValue settingNode ||
            !settingNode.TryGetValue<int>(out int currentSetting) ||
            currentSetting != ChromiumAllow)
            throw new InvalidDataException($"The notification permission changed after scanning in {permission.SourcePath}. Scan again and retry.");

        if (block)
        {
            entry["setting"] = ChromiumBlock;
        }
        else
        {
            notifications.Remove(permission.PermissionKey);
        }

        string temporaryPath = permission.SourcePath + ".lga-tmp";
        try
        {
            File.WriteAllText(temporaryPath, root!.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));
            File.Move(temporaryPath, permission.SourcePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return 1;
    }

    private static int ApplyFirefox(BrowserPermission permission, bool block)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = permission.SourcePath,
            Mode = SqliteOpenMode.ReadWrite
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        if (block)
        {
            command.CommandText =
                "UPDATE moz_perms SET permission = $block WHERE origin = $origin AND type = 'desktop-notification' AND permission = $allow";
            command.Parameters.AddWithValue("$block", FirefoxBlock);
        }
        else
        {
            command.CommandText =
                "DELETE FROM moz_perms WHERE origin = $origin AND type = 'desktop-notification' AND permission = $allow";
        }

        command.Parameters.AddWithValue("$origin", permission.PermissionKey);
        command.Parameters.AddWithValue("$allow", FirefoxAllow);
        int affected = command.ExecuteNonQuery();
        transaction.Commit();
        if (affected == 0)
            throw new InvalidDataException($"The notification entry was not found in {permission.SourcePath}. Scan again and retry.");
        return affected;
    }

    private static string GetProfileName(string profilePath, JsonElement profile)
    {
        if (profile.TryGetProperty("name", out JsonElement name) && name.ValueKind == JsonValueKind.String)
            return name.GetString() ?? Path.GetFileName(profilePath);
        return Path.GetFileName(profilePath);
    }
}
