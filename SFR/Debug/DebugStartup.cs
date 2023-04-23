using HarmonyLib;
using SFD;
using SFD.MapEditor;
using SFD.States;
using Constants = SFR.Misc.Constants;

namespace SFR.Debug;

[HarmonyPatch]
internal static class DebugStartup
{
    private static double _timer;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.ChangeState))]
    private static void DoLoad(ref State newState, GameSFD __instance)
    {
        if (Constants.FastStart && newState != State.Loading)
        {
            newState = State.Editor;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateEditor), nameof(StateEditor.Update))] // Load
    private static void LoadMap(double elapsedGameTime, StateEditor __instance)
    {
        if (Constants.FastStart)
        {
            _timer += elapsedGameTime;
            if (_timer > 600)
            {
                Constants.FastStart = false;
                __instance.LoadEditMap(Constants.DebugMap);
                __instance.TestRunStart();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EditorMapTestData), nameof(EditorMapTestData.Reset))]
    private static void AddTestUsers(EditorMapTestData __instance)
    {
        if (Constants.FastStart)
        {
            for (int i = 2; i < 4; i++)
            {
                __instance.TestUsers[i].Team = Team.Independent;
                __instance.TestUsers[i].UpdateProfileTeamHeadband();
            }
        }
    }
}