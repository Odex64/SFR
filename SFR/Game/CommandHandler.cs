using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.States;

namespace SFR.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    private static bool _useHostMouse;
    private static readonly float[] _debugVar = new float[10];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void DebugMouse(float chunkMs, float totalMs, bool isLast, bool isFirst, GameWorld __instance)
    {
        //if ( isLast && Program.IsGame && __instance.GameOwner != GameOwnerEnum.Client && __instance.m_game.CurrentState is State.EditorTestRun or State.MainMenu) return; //Already handled

        if (isLast && SFD.Program.IsGame && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu && __instance.GameOwner != GameOwnerEnum.Client)
        {
            if (_useHostMouse)
            {
                __instance.UpdateDebugMouse();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static void HandleCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        if (__instance.GameOwner != GameOwnerEnum.Client)
        {
            if (args.HostPrivileges)
            {
                if (args.IsCommand("MOUSE", "M"))
                {
                    if (args.Parameters.Count > 0)
                    {
                        if (args.Parameters[0] == "1" || args.Parameters[0].ToUpperInvariant() == "TRUE")
                        {
                            _useHostMouse = true;
                        }

                        if (args.Parameters[0] == "0" || args.Parameters[0].ToUpperInvariant() == "FALSE")
                        {
                            _useHostMouse = false;
                        }

                        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Mouse Control: " + _useHostMouse));
                    }
                }

#if DEBUG
                if (args.IsCommand("DEBUG", "D"))
                {
                    if (args.Parameters.Count == 2)
                    {
                        if (int.TryParse(args.Parameters[0], out int index))
                        {
                            if (float.TryParse(args.Parameters[1], out float num))
                            {
                                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Debug float: " + num));
                                _debugVar[index] = num;
                            }
                        }
                    }
                }
#endif
            }

#if DEBUG
            if (args.ModeratorPrivileges)
            {
                if (args.IsCommand("NUKE"))
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, args.SenderGameUser.GetProfileName() + " has set the world on fire", Color.Red));
                    NukeHandler.CreateNuke(__instance.GameWorld);
                }
            }
#endif

            if (args.IsCommand("HELP"))
            {
                Color color = new(159, 255, 64);
                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "SFR Commands: ", color, args.SenderGameUser));
                if (args.HostPrivileges)
                {
#if DEBUG
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/DEBUG [INDEX] [VALUE]' debug purposes", color, args.SenderGameUser));
#endif
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/MOUSE [1/0]' Drag stuff with mouse", color, args.SenderGameUser));
                }

                // if (args.ModeratorPrivileges)
                // {
                //     args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "'/NUKE' You're a terrible person.", color, args.SenderGameUser));
                // }
            }
        }
    }
}