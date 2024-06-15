using Celeste.Mod;
using System;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry;

public class ThinkTwiceBeforeRetryModule : EverestModule
{
    public static ThinkTwiceBeforeRetryModule Instance { get; private set; }

    public override Type SettingsType => typeof(ThinkTwiceBeforeRetryModuleSettings);
    public static ThinkTwiceBeforeRetryModuleSettings Settings => (ThinkTwiceBeforeRetryModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(ThinkTwiceBeforeRetryModuleSession);
    public static ThinkTwiceBeforeRetryModuleSession Session => (ThinkTwiceBeforeRetryModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(ThinkTwiceBeforeRetryModuleSaveData);
    public static ThinkTwiceBeforeRetryModuleSaveData SaveData => (ThinkTwiceBeforeRetryModuleSaveData)Instance._SaveData;

    public ThinkTwiceBeforeRetryModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ThinkTwiceBeforeRetryModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ThinkTwiceBeforeRetryModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        LevelExtension.Hook();
    }

    public override void Unload()
    {
        LevelExtension.Unhook();
    }
}