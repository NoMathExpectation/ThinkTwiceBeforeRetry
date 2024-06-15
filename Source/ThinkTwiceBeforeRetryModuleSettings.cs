using Celeste.Mod;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry;

public class ThinkTwiceBeforeRetryModuleSettings : EverestModuleSettings
{
    [SettingName("TTBR_setting_delay")]
    [SettingRange(0, 10)]
    public int DefaultDelay { get; set; } = 3;
}