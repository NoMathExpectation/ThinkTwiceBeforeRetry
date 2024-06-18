using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Linq;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry
{
    public static class LevelExtension
    {
        private static void EndLevel(this Level level)
        {
            Engine.TimeRate = 1f;
            level.Session.InArea = false;
            Audio.SetMusic(null);
            Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
            level.DoScreenWipe(wipeIn: false, () =>
            {
                Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session, level.HiresSnow);
            }, hiresSnow: true);
            foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
            {
                if (component.OnEnd != null)
                {
                    component.OnEnd();
                }
            }
        }

        private static void RestartLevel(this Level level)
        {
            Engine.TimeRate = 1f;
            level.Session.InArea = false;
            Audio.SetMusic(null);
            Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
            level.DoScreenWipe(wipeIn: false, delegate
            {
                Engine.Scene = new LevelExit(LevelExit.Mode.Restart, level.Session);
            });
            foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
            {
                if (component.OnEnd != null)
                {
                    component.OnEnd();
                }
            }
        }

        private static void RetryLevel(this Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null && !player.Dead)
            {
                Engine.TimeRate = 1f;
                Distort.GameRate = 1f;
                Distort.Anxiety = 0f;
                level.InCutscene = (level.SkippingCutscene = false);
                level.RetryPlayerCorpse = player.Die(Vector2.Zero, evenIfInvincible: true);
                foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
                {
                    if (component.OnEnd != null)
                    {
                        component.OnEnd();
                    }
                }
            }

            level.Paused = false;
            level.PauseMainMenuOpen = false;
            level.EndPauseEffects();
        }

        private static IEnumerator DisableCoroutine(TextMenu.Button button, float delay, string label)
        {
            button.Disabled = true;
            button.Label = label + string.Format("({0})", ((int)delay) + 1);

            yield return delay - Math.Floor(delay);
            for (var i = (int)delay; i > 0; i--)
            {
                button.Label = label + string.Format("({0})", i);
                yield return 1f;
            }

            button.Label = label;
            button.Disabled = false;
        }

        public static void GiveUpGolden(this Level level, int returnIndex, bool minimal, string operation, Action<TextMenu> onConfirm, float? delay0 = null, float cancelDelay = 0f)
        {
            var delay = delay0 ?? ThinkTwiceBeforeRetryModule.Settings.DefaultDelay;

            level.Paused = true;

            var menu = new TextMenu();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);

            var header = new TextMenu.Header("TTBR_giveup_header".DialogClean());
            var subHeader = new TextMenu.SubHeader("TTBR_giveup_subheader".DialogClean());
            menu.Add(header);
            menu.Add(subHeader);

            var cancelButton = new TextMenu.Button("TTBR_giveup_cancel".DialogClean());
            cancelButton.Pressed(() =>
            {
                menu.OnCancel();
            });
            var confirmButton = new TextMenu.Button(operation);
            confirmButton.Pressed(() =>
            {
                onConfirm(menu);
            });


            if (delay > 0f)
            {
                confirmButton.Disabled = true;
                confirmButton.Label += string.Format("({0})", ((int)delay) + 1);

                var coroutine = new Coroutine(DisableCoroutine(confirmButton, delay, operation));
                coroutine.UseRawDeltaTime = true;
                menu.Add(coroutine);
            }
            if (cancelDelay > 0f)
            {
                cancelButton.Disabled = true;
                cancelButton.Label += string.Format("({0})", ((int)delay) + 1);

                var coroutine = new Coroutine(DisableCoroutine(cancelButton, cancelDelay, "TTBR_giveup_cancel".DialogClean()));
                coroutine.UseRawDeltaTime = true;
                menu.Add(coroutine);
            }

            menu.Add(cancelButton);
            menu.Add(confirmButton);

            menu.OnPause = (menu.OnESC = () =>
            {
                menu.RemoveSelf();
                level.Paused = false;
                level.unpauseTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = () =>
            {
                if (cancelButton.Disabled)
                {
                    return;
                }

                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                level.Pause(returnIndex, minimal);
            };
            level.Add(menu);
        }

        public static bool PlayerHasImportantCollectible(this Level level)
        {
            return level.Tracker.GetEntity<Player>().HasImportantCollectible();
        }

        private static void QuickRestartReplacement(this Level level, bool minimal = false)
        {
            // hooked just before the original give up method
            if (level.PlayerHasImportantCollectible())
            {
                level.GiveUpGolden(0, minimal, "menu_pause_restartarea".DialogClean(), menu =>
                {
                    menu.Focused = false;
                    level.RestartLevel();
                }, cancelDelay: 1f);
            }
            else
            {
                level.GiveUp(0, true, minimal, false);
            }
        }

        private static bool IsLevelOrigPauseHookPoint(Instruction instr)
        {
            if (!instr.MatchLdarg0())
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.MatchLdcI4(0))
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.MatchLdcI4(1))
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.MatchLdloc0())
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.Match(OpCodes.Ldfld))
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.MatchLdcI4(0))
            {
                return false;
            }
            instr = instr.Next;
            if (!instr.MatchCallvirt<Level>("GiveUp"))
            {
                return false;
            }
            return true;
        }

        private static ILHook levelOrigPauseHook = null;
        private static void ModLevelOrigPause(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.Before, IsLevelOrigPauseHookPoint))
            {
                cursor.EmitLdarg0();
                cursor.EmitLdarg2();
                cursor.EmitDelegate(QuickRestartReplacement);
                cursor.EmitRet();
            }
        }

        private static void OnCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
        {
            if (!level.PlayerHasImportantCollectible())
            {
                return;
            }

            foreach (var button in menu.Items.OfType<TextMenu.Button>())
            {
                var label = button.Label;
                if (label == "menu_pause_retry".DialogClean())
                {
                    var origPressed = button.OnPressed;
                    button.Pressed(() =>
                    {
                        menu.RemoveSelf();
                        level.PauseMainMenuOpen = false;
                        level.GiveUpGolden(menu.IndexOf(button), minimal, label, m2 =>
                        {
                            m2.RemoveSelf();
                            origPressed();
                        });
                    });
                    continue;
                }

                if (label == "menu_pause_savequit".DialogClean())
                {
                    var origPressed = button.OnPressed;
                    button.Pressed(() =>
                    {
                        menu.RemoveSelf();
                        level.PauseMainMenuOpen = false;
                        level.GiveUpGolden(menu.IndexOf(button), minimal, label, m2 =>
                        {
                            m2.Focused = false;
                            origPressed();
                        });
                    });
                    continue;
                }

                if (label == "menu_pause_restartarea".DialogClean())
                {
                    button.Pressed(() =>
                    {
                        menu.RemoveSelf();
                        level.PauseMainMenuOpen = false;
                        level.GiveUpGolden(menu.IndexOf(button), minimal, label, m2 =>
                        {
                            m2.Focused = false;
                            level.RestartLevel();
                        });
                    });
                    continue;
                }

                if (label == "menu_pause_return".DialogClean())
                {
                    button.Pressed(() =>
                    {
                        menu.RemoveSelf();
                        level.PauseMainMenuOpen = false;
                        level.GiveUpGolden(menu.IndexOf(button), minimal, label, m2 =>
                        {
                            m2.Focused = false;
                            level.EndLevel();
                        });
                    });
                    continue;
                }
            }
        }

        internal static void Hook()
        {
            levelOrigPauseHook = new ILHook(typeof(Level).GetMethod("orig_Pause"), ModLevelOrigPause);
            Everest.Events.Level.OnCreatePauseMenuButtons += OnCreatePauseMenuButtons;
        }

        internal static void Unhook()
        {
            levelOrigPauseHook?.Dispose();
            levelOrigPauseHook = null;
            Everest.Events.Level.OnCreatePauseMenuButtons -= OnCreatePauseMenuButtons;
        }
    }
}
