using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry
{
    public static class SpeedrunToolHooks
    {
        private static readonly EverestModuleMetadata SpeedrunToolModuleMetadata = new()
        {
            Name = "SpeedrunTool",
            VersionString = "3.27.14"
        };

        private static EverestModule SpeedrunToolModule = null;

        private static bool ConsumeAndGetDelayedOpenDebugMap(bool value)
        {
            if (ThinkTwiceBeforeRetryModuleSettings.ShouldDisableDebug())
            {
                return false;
            }

            return value;
        }

        private static ILHook betterMapEditorEngineOnUpdateHook = null;
        private static void ModBetterMapEditorEngineOnUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);
            var assembly = SpeedrunToolModule.GetType().Assembly;

            var betterMapEditorType = assembly.GetType("Celeste.Mod.SpeedrunTool.Other.BetterMapEditor");
            if (betterMapEditorType is null)
            {
                Logger.Error("TTBR", "Cannot find type Celeste.Mod.SpeedrunTool.Other.BetterMapEditor!");
                return;
            }
            var delayedOpenDebugMapField = betterMapEditorType.GetField("delayedOpenDebugMap", BindingFlags.NonPublic | BindingFlags.Static);
            if (delayedOpenDebugMapField is null)
            {
                Logger.Error("TTBR", "Cannot find field BetterMapEditor.delayedOpenDebugMap!");
                return;
            }

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(betterMapEditorType, "delayedOpenDebugMap")))
            {
                cursor.EmitDelegate(ConsumeAndGetDelayedOpenDebugMap);
                cursor.EmitDup();
                cursor.EmitStsfld(delayedOpenDebugMapField);
            }
            else
            {
                Logger.Error("TTBR", "Cannot find hook point BetterMapEditor.delayedOpenDebugMap!");
            }
        }

        internal static void Hook()
        {
            if (!Everest.Loader.TryGetDependency(SpeedrunToolModuleMetadata, out SpeedrunToolModule))
            {
                return;
            }

            var assembly = SpeedrunToolModule.GetType().Assembly;
            var betterMapEditorType = assembly.GetType("Celeste.Mod.SpeedrunTool.Other.BetterMapEditor");
            if (betterMapEditorType is null)
            {
                Logger.Error("TTBR", "Cannot find type Celeste.Mod.SpeedrunTool.Other.BetterMapEditor!");
                return;
            }
            var engineOnUpdateMethod = betterMapEditorType.GetMethod("EngineOnUpdate", BindingFlags.NonPublic | BindingFlags.Static);
            if (engineOnUpdateMethod is null)
            {
                Logger.Error("TTBR", "Cannot find method EngineOnUpdate!");
                return;
            }
            betterMapEditorEngineOnUpdateHook = new ILHook(engineOnUpdateMethod, ModBetterMapEditorEngineOnUpdate);
        }

        internal static void Unhook()
        {
            betterMapEditorEngineOnUpdateHook?.Dispose();
            betterMapEditorEngineOnUpdateHook = null;

            SpeedrunToolModule = null;
        }
    }
}
