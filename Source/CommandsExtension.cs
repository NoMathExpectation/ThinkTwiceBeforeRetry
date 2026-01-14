using Celeste.Mod;
using Celeste.Mod.Core;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry
{
    public static class CommandsExtension
    {
        private static ILHook updateClosedHook = null;
        private static void ModUpdateClosed(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCall<CoreModule>("get_Settings")))
            {
                Logger.Error("TTBR", "Cannot find hook point get_Settings!");
                return;
            }

            var cursor2 = cursor.Clone();
            if (!cursor2.TryGotoNext(MoveType.After, instr => instr.MatchStfld<Monocle.Commands>("printedInfoMessage")))
            {
                Logger.Error("TTBR", "Cannot find hook point printedInfoMessage!");
                return;
            }

            cursor2.MoveAfterLabels();
            var jumpLabel = cursor2.MarkLabel();

            cursor.EmitDelegate(ThinkTwiceBeforeRetryModuleSettings.ShouldDisableDebug);
            cursor.EmitBrtrue(jumpLabel);

            cursor = cursor2.Clone();
            if (!cursor2.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchRet()))
            {
                Logger.Error("TTBR", "Cannot find hook point return!");
                return;
            }

            jumpLabel = cursor2.MarkLabel();

            cursor.EmitDelegate(ThinkTwiceBeforeRetryModuleSettings.ShouldDisableDebug);
            cursor.EmitBrtrue(jumpLabel);
        }

        internal static void Hook()
        {
            updateClosedHook = new ILHook(typeof(Monocle.Commands).GetMethod("UpdateClosed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), ModUpdateClosed);
        }

        internal static void Unhook()
        {
            updateClosedHook?.Dispose();
            updateClosedHook = null;
        }
    }
}
