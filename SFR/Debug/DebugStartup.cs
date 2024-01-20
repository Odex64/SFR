using HarmonyLib;
using SFD;
using SFD.MapEditor;
using SFD.States;
using Constants = SFR.Misc.Constants;

namespace SFR.Debug;

/// <summary>
///     This class is used for a fast startup of the game for debug purposes.
///     It will load directly inside a map with some bots, skipping the intro and map editor load times.
/// </summary>
[HarmonyPatch]
internal static class DebugStartup
{
    private static double _timer;

    /// <summary>
    ///     Change the state of the game from the main menu to the map editor.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.ChangeState))]
    private static void DoLoad(ref State newState, GameSFD __instance)
    {
        if (Constants.FastStart && newState != State.Loading)
        {
            newState = State.Editor;
        }
    }

    /// <summary>
    ///     Play the map as soon as possible when in map editor.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateEditor), nameof(StateEditor.Update))]
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

    /// <summary>
    ///     Add some testing bots.
    /// </summary>
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