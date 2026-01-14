using Celeste;
using Celeste.Mod;
using Monocle;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry;

public class ThinkTwiceBeforeRetryModuleSettings : EverestModuleSettings
{
    public enum MenuEnableType
    {
        ALWAYS_DISABLED, IMPORTANT_CARRIED, ALWAYS_ENABLED
    }

    [SettingName("TTBR_setting_menu_enable_type")]
    [SettingSubText("TTBR_setting_menu_enable_type_description")]
    public MenuEnableType EnableType { get; set; } = MenuEnableType.IMPORTANT_CARRIED;

    [SettingName("TTBR_setting_disable_debug")]
    [SettingSubText("TTBR_setting_disable_debug_description")]
    public bool DisableDebug { get; set; } = true;

    [SettingName("TTBR_setting_delay")]
    [SettingSubText("TTBR_setting_delay_description")]
    [SettingRange(0, 10)]
    public int DefaultDelay { get; set; } = 3;

    [SettingName("TTBR_setting_cancel_delay")]
    [SettingSubText("TTBR_setting_cancel_delay_description")]
    [SettingRange(0, 10)]
    public int CancelDelay { get; set; } = 1;

    public static bool ShouldDisableDebug()
    {
        var scene = Engine.Scene;
        if (scene is not Level level)
        {
            return false;
        }
        return ThinkTwiceBeforeRetryModule.Settings.DisableDebug && level.PlayerHasImportantCollectible();
    }
}