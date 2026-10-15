namespace LGAStudio;

internal enum UiLanguage
{
    English,
    Korean
}

// Centralized UI strings keep buttons, prompts, statuses, and diagnostics synchronized by language.
internal static class UiText
{
    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        ["app.title"] = "LGA Studio",
        ["languageError"] = "The language could not be changed. The previous language has been restored.",
        ["languageErrorTitle"] = "Language change error",
        ["language"] = "Language:",
        ["english"] = "English",
        ["korean"] = "한국어",
        ["oneClick"] = "One-Click Scan (safe)",
        ["intro"] = "This scan only looks for items. It does not change or delete anything.",
        ["step"] = "Step 1: Scan  →  Step 2: Review the list  →  Step 3: Select items and choose a cleanup action",
        ["browserTab"] = "Browser notifications",
        ["startupTab"] = "Startup items",
        ["browserInstructions"] = "Review sites allowed to send notifications. Block prevents notifications; Reset removes the saved permission.",
        ["startupInstructions"] = "Review startup registry entries and recurring browser/URL tasks. Detection is advisory; unknown items may be legitimate.",
        ["scanBrowsers"] = "Scan browsers",
        ["blockSelected"] = "Block selected",
        ["resetSelected"] = "Reset selected",
        ["scanStartup"] = "Scan startup items",
        ["removeSelected"] = "Remove selected",
        ["select"] = "Select",
        ["browser"] = "Browser",
        ["profile"] = "Profile",
        ["domain"] = "Allowed domain / origin",
        ["database"] = "Source / permission file",
        ["type"] = "Type",
        ["name"] = "Name",
        ["location"] = "Location",
        ["command"] = "Command / task action",
        ["assessment"] = "Assessment",
        ["browserHint"] = "Step 1: Scan browsers. Step 2: Review the allowed sites. Step 3: Select rows, then Block or Reset.",
        ["startupHint"] = "Step 1: Scan startup items. Step 2: Review names, commands, and assessments. Step 3: Select only items you recognize as unwanted, then Remove.",
        ["browserScanStatus"] = "Scan complete: {0} allowed notification permission(s) found.",
        ["startupScanStatus"] = "Scan complete: {0} startup item(s) listed. Review all items before removing any.",
        ["scanFailed"] = "The scan could not be completed.",
        ["scanErrorTitle"] = "Scan error",
        ["browserWarningsTitle"] = "Browser scan details",
        ["startupWarningsTitle"] = "Startup scan details",
        ["warningHeader"] = "Some items could not be inspected:",
        ["selectBrowser"] = "Select one or more browser permissions first.",
        ["selectStartup"] = "Select one or more startup items first.",
        ["nothingSelected"] = "Nothing selected",
        ["closeBrowsers"] = "Please close these browsers normally before changing permission files, then try again:\n\n{0}\n\nThis app will not close browsers for you.",
        ["closeBrowsersTitle"] = "Close browsers first",
        ["browserProcessUnavailable"] = "{0} (process status unavailable)",
        ["browserConfirm"] = "You are about to {0} {1} selected notification permission(s). Continue?",
        ["blockAction"] = "block",
        ["resetAction"] = "reset",
        ["browserConfirmTitle"] = "Confirm permission change",
        ["removeConfirm"] = "Permanently remove {0} selected startup item(s)? Review each item carefully. This cannot be undone.",
        ["removeConfirmTitle"] = "Confirm startup removal",
        ["yes"] = "Yes",
        ["no"] = "No",
        ["completed"] = "{0} completed for {1} item(s).",
        ["completedErrors"] = "{0} completed for {1} item(s), but {2} item(s) could not be changed:",
        ["operationErrorTitle"] = "Operation completed with errors",
        ["completedTitle"] = "Completed",
        ["technicalDetail"] = "Details: {0}",
        ["registry"] = "Registry",
        ["scheduledTask"] = "Scheduled task",
        ["suspiciousTemp"] = "Suspicious: executable launches from a temporary folder.",
        ["suspiciousDownloads"] = "Suspicious: executable launches from Downloads.",
        ["suspiciousUserArgs"] = "Suspicious: a user-profile executable uses potentially hidden or remote arguments.",
        ["suspiciousProxyArgs"] = "Suspicious: a script/proxy executable uses potentially hidden or remote arguments.",
        ["reviewUser"] = "Review: executable is stored in the user profile; this can be legitimate.",
        ["reviewProxy"] = "Review: startup uses a script or proxy executable.",
        ["noPattern"] = "No obvious suspicious path or argument pattern detected.",
        ["taskAssessment"] = "Suspicious: recurring task launches a browser or URL.",
        ["diagCouldNotInspect"] = "Could not inspect",
        ["diagCouldNotEnumerate"] = "Could not enumerate",
        ["diagCouldNotRead"] = "Could not read",
        ["diagTaskUnavailable"] = "Task Scheduler is unavailable; scheduled tasks were not inspected.",
        ["diagTaskCreate"] = "Could not connect to Task Scheduler",
        ["diagTaskInspect"] = "Could not inspect scheduled tasks",
        ["diagTaskFolder"] = "Could not inspect tasks in",
        ["diagTaskChildren"] = "Could not inspect a scheduled-task folder beneath",
        ["diagTaskEnumerate"] = "Could not enumerate scheduled-task folders beneath",
        ["diagTaskSingle"] = "Could not inspect scheduled task in",
        ["oneClickResult"] = "Safe scan complete. No items were changed or deleted.",
        ["oneClickTitle"] = "Scan complete",
        ["languageTip"] = "Change the display language at any time.",
        ["oneClickTip"] = "Scans browsers and startup locations. This is read-only and does not clean anything.",
        ["scanTip"] = "Step 1: Scan for items. Scanning does not change your computer.",
        ["reviewTip"] = "Review every row carefully. Suspicious labels are estimates and can be wrong.",
        ["blockTip"] = "Changes selected allowed sites to Block after confirmation.",
        ["resetTip"] = "Removes the selected saved notification permissions after confirmation.",
        ["removeTip"] = "Permanently removes selected startup entries or tasks after confirmation."
    };

    private static readonly IReadOnlyDictionary<string, string> Korean = new Dictionary<string, string>
    {
        ["app.title"] = "LGA Studio",
        ["languageError"] = "언어를 변경하지 못했습니다. 이전 언어로 복원했습니다.",
        ["languageErrorTitle"] = "언어 변경 오류",
        ["language"] = "언어:",
        ["english"] = "English",
        ["korean"] = "한국어",
        ["oneClick"] = "안전 진단 한 번에 실행",
        ["intro"] = "진단은 항목을 확인만 합니다. 설정을 바꾸거나 삭제하지 않습니다.",
        ["step"] = "1단계: 검사  →  2단계: 목록 확인  →  3단계: 항목을 선택하고 정리 작업 선택",
        ["browserTab"] = "브라우저 알림",
        ["startupTab"] = "시작 프로그램",
        ["browserInstructions"] = "알림을 허용한 사이트를 확인합니다. 차단은 알림을 막고, 초기화는 저장된 허용 설정을 삭제합니다.",
        ["startupInstructions"] = "시작 레지스트리 항목과 반복 실행 브라우저/URL 작업을 확인합니다. 의심 표시는 참고용이며 정상 항목일 수도 있습니다.",
        ["scanBrowsers"] = "브라우저 검사",
        ["blockSelected"] = "선택 항목 차단",
        ["resetSelected"] = "선택 항목 초기화",
        ["scanStartup"] = "시작 항목 검사",
        ["removeSelected"] = "선택 항목 삭제",
        ["select"] = "선택",
        ["browser"] = "브라우저",
        ["profile"] = "프로필",
        ["domain"] = "알림 허용 도메인 / 출처",
        ["database"] = "출처 / 권한 파일",
        ["type"] = "종류",
        ["name"] = "이름",
        ["location"] = "위치",
        ["command"] = "명령 / 작업 동작",
        ["assessment"] = "검사 의견",
        ["browserHint"] = "1단계: 브라우저 검사. 2단계: 허용 사이트 확인. 3단계: 행을 선택한 뒤 차단 또는 초기화.",
        ["startupHint"] = "1단계: 시작 항목 검사. 2단계: 이름, 명령, 검사 의견 확인. 3단계: 원치 않는 항목임을 확인한 경우에만 선택 후 삭제.",
        ["browserScanStatus"] = "검사 완료: 알림 허용 항목 {0}개를 찾았습니다.",
        ["startupScanStatus"] = "검사 완료: 시작 항목 {0}개를 표시했습니다. 삭제 전에 모든 항목을 확인하세요.",
        ["scanFailed"] = "검사를 완료할 수 없습니다.",
        ["scanErrorTitle"] = "검사 오류",
        ["browserWarningsTitle"] = "브라우저 검사 세부 정보",
        ["startupWarningsTitle"] = "시작 항목 검사 세부 정보",
        ["warningHeader"] = "일부 항목을 검사하지 못했습니다:",
        ["selectBrowser"] = "먼저 브라우저 권한 항목을 하나 이상 선택하세요.",
        ["selectStartup"] = "먼저 시작 항목을 하나 이상 선택하세요.",
        ["nothingSelected"] = "선택 항목 없음",
        ["closeBrowsers"] = "권한 파일을 변경하기 전에 다음 브라우저를 정상적으로 종료한 후 다시 시도하세요:\n\n{0}\n\n앱이 브라우저를 강제 종료하지 않습니다.",
        ["closeBrowsersTitle"] = "브라우저를 먼저 종료하세요",
        ["browserProcessUnavailable"] = "{0} (프로세스 상태 확인 불가)",
        ["browserConfirm"] = "선택한 알림 권한 {1}개를 {0}합니다. 계속할까요?",
        ["blockAction"] = "차단",
        ["resetAction"] = "초기화",
        ["browserConfirmTitle"] = "권한 변경 확인",
        ["removeConfirm"] = "선택한 시작 항목 {0}개를 영구 삭제합니다. 각 항목을 주의해서 확인하세요. 삭제는 되돌릴 수 없습니다.",
        ["removeConfirmTitle"] = "시작 항목 삭제 확인",
        ["yes"] = "예",
        ["no"] = "아니요",
        ["completed"] = "{0} 완료: {1}개 항목을 처리했습니다.",
        ["completedErrors"] = "{0} 완료: {1}개 항목을 처리했지만 {2}개 항목을 변경하지 못했습니다:",
        ["operationErrorTitle"] = "일부 작업 오류",
        ["completedTitle"] = "완료",
        ["technicalDetail"] = "세부 정보: {0}",
        ["registry"] = "레지스트리",
        ["scheduledTask"] = "예약 작업",
        ["suspiciousTemp"] = "의심: 임시 폴더에서 실행되는 프로그램입니다.",
        ["suspiciousDownloads"] = "의심: 다운로드 폴더에서 실행되는 프로그램입니다.",
        ["suspiciousUserArgs"] = "의심: 사용자 프로필의 프로그램에 숨김 또는 원격 실행 인수가 있습니다.",
        ["suspiciousProxyArgs"] = "의심: 스크립트/중계 프로그램에 숨김 또는 원격 실행 인수가 있습니다.",
        ["reviewUser"] = "확인 필요: 사용자 프로필에 저장된 프로그램입니다. 정상 항목일 수도 있습니다.",
        ["reviewProxy"] = "확인 필요: 시작 시 스크립트 또는 중계 프로그램을 사용합니다.",
        ["noPattern"] = "뚜렷하게 의심스러운 경로 또는 인수가 발견되지 않았습니다.",
        ["taskAssessment"] = "의심: 반복 예약 작업이 브라우저 또는 URL을 실행합니다.",
        ["diagCouldNotInspect"] = "검사할 수 없음",
        ["diagCouldNotEnumerate"] = "목록을 가져올 수 없음",
        ["diagCouldNotRead"] = "읽을 수 없음",
        ["diagTaskUnavailable"] = "작업 스케줄러를 사용할 수 없어 예약 작업을 검사하지 못했습니다.",
        ["diagTaskCreate"] = "작업 스케줄러에 연결할 수 없음",
        ["diagTaskInspect"] = "예약 작업을 검사할 수 없음",
        ["diagTaskFolder"] = "폴더의 작업을 검사할 수 없음",
        ["diagTaskChildren"] = "아래 예약 작업 폴더를 검사할 수 없음",
        ["diagTaskEnumerate"] = "아래 예약 작업 폴더 목록을 가져올 수 없음",
        ["diagTaskSingle"] = "예약 작업을 검사할 수 없음",
        ["oneClickResult"] = "안전 검사가 완료되었습니다. 항목을 변경하거나 삭제하지 않았습니다.",
        ["oneClickTitle"] = "검사 완료",
        ["languageTip"] = "언제든지 표시 언어를 변경할 수 있습니다.",
        ["oneClickTip"] = "브라우저와 시작 위치를 검사합니다. 읽기 전용이며 정리 작업은 하지 않습니다.",
        ["scanTip"] = "1단계: 항목을 검사합니다. 검사는 컴퓨터 설정을 변경하지 않습니다.",
        ["reviewTip"] = "각 항목을 주의해서 확인하세요. 의심 표시는 추정이며 틀릴 수 있습니다.",
        ["blockTip"] = "확인 후 선택한 허용 사이트를 차단합니다.",
        ["resetTip"] = "확인 후 선택한 알림 권한 설정을 삭제합니다.",
        ["removeTip"] = "확인 후 선택한 시작 항목 또는 예약 작업을 영구 삭제합니다."
    };

    public static UiLanguage Language { get; set; } = UiLanguage.English;

    public static string Get(string key) =>
        (Language == UiLanguage.Korean ? Korean : English).TryGetValue(key, out string? value)
            ? value
            : key;

    public static string Format(string key, params object[] values) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Get(key), values);

    public static string Assessment(string value)
    {
        string key = value switch
        {
            "Suspicious: executable launches from a temporary directory." => "suspiciousTemp",
            "Suspicious: executable launches from Downloads." => "suspiciousDownloads",
            "Suspicious: user-profile executable uses potentially concealed or remote arguments." => "suspiciousUserArgs",
            "Suspicious: script/proxy executable uses potentially concealed or remote arguments." => "suspiciousProxyArgs",
            "Review: executable is stored in the user profile; this can be legitimate." => "reviewUser",
            "Review: startup uses a script or proxy executable." => "reviewProxy",
            "No obvious suspicious path or argument pattern detected." => "noPattern",
            "Suspicious: recurring task launches a browser or URL." => "taskAssessment",
            _ => ""
        };
        return key.Length == 0 ? value : Get(key);
    }

    public static string Diagnostic(string value)
    {
        string[] prefixes =
        [
            "Could not inspect scheduled task in",
            "Could not inspect a scheduled-task folder beneath",
            "Could not enumerate scheduled-task folders beneath",
            "Could not inspect tasks in",
            "Could not inspect scheduled tasks",
            "Could not connect to Task Scheduler",
            "Could not enumerate",
            "Could not inspect",
            "Could not read"
        ];

        foreach (string prefix in prefixes)
        {
            if (!value.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string remainder = value[prefix.Length..];
            string localizedPrefix;
            if (prefix.StartsWith("Could not inspect scheduled task in", StringComparison.Ordinal))
                localizedPrefix = Get("diagTaskSingle");
            else if (prefix.StartsWith("Could not inspect a scheduled-task folder beneath", StringComparison.Ordinal))
                localizedPrefix = Get("diagTaskChildren");
            else if (prefix.StartsWith("Could not enumerate scheduled-task folders beneath", StringComparison.Ordinal))
                localizedPrefix = Get("diagTaskEnumerate");
            else if (prefix.StartsWith("Could not inspect tasks in", StringComparison.Ordinal))
                localizedPrefix = Get("diagTaskFolder");
            else if (prefix == "Could not inspect scheduled tasks")
                localizedPrefix = Get("diagTaskInspect");
            else if (prefix == "Could not connect to Task Scheduler")
                localizedPrefix = Get("diagTaskCreate");
            else if (prefix == "Could not enumerate")
                localizedPrefix = Get("diagCouldNotEnumerate");
            else if (prefix == "Could not read")
                localizedPrefix = Get("diagCouldNotRead");
            else
                localizedPrefix = Get("diagCouldNotInspect");

            return localizedPrefix + remainder;
        }

        if (value.StartsWith("Task Scheduler COM interface is unavailable;", StringComparison.Ordinal))
            return Get("diagTaskUnavailable");

        return value;
    }
}
