using Celeste.Mod;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry;

public class ThinkTwiceBeforeRetryModuleSettings : EverestModuleSettings
{
    public enum MenuEnableType
    {
        ALWAYS_DISABLED, IMPORTANT_CARRIED, ALWAYS_ENABLED
    }

    [SettingName("TTBR_setting_menu_enable_type")]
    public MenuEnableType EnableType { get; set; } = MenuEnableType.IMPORTANT_CARRIED;

    [SettingName("TTBR_setting_delay")]
    [SettingRange(0, 10)]
    public int DefaultDelay { get; set; } = 3;

    [SettingName("TTBR_setting_cancel_delay")]
    [SettingRange(0, 10)]
    public int CancelDelay { get; set; } = 1;
}