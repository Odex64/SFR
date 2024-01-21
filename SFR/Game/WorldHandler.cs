using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using SFD;
using SFD.Objects;
using SFR.Bootstrap;
using SFR.Fighter;
using SFR.Helper;
using SFR.Sync;
using Constants = SFR.Misc.Constants;

namespace SFR.Game;

/// <summary>
///     This class contain patches that affect all the rounds, such as how the game is supposed to dispose objects.
/// </summary>
[HarmonyPatch]
internal static class WorldHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectDestructible), nameof(ObjectDestructible.OnDestroyObject))]
    private static void DestroyObject(ObjectDestructible __instance)
    {
        if (Vanilla.Active) return;

        if (__instance.GameOwner != GameOwnerEnum.Client)
        {
            if (__instance.MapObjectID is "CRATE00" or "CRATE01" && Constants.Random.NextDouble() <= 0.02)
            {
                __instance.GameWorld.SpawnDebris(__instance, __instance.GetWorldPosition(), 0f, [Constants.Random.NextBool() ? "BeachBall00" : "Monkey00"], 1, false);
            }
        }
    }

    /// <summary>
    ///     For unknown reasons players tempt to crash when joining a game.
    ///     This is caused because a collection is being modified during its iteration.
    ///     Therefore we iterate the collection backwards so it can be modified without throwing an exception.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.FinalizeProperties))]
    private static bool FinalizeProperties(GameWorld __instance)
    {
        __instance.b2_settings.timeStep = 0f;
        __instance.Step(__instance.b2_settings);

        for (int i = __instance.DynamicObjects.Count - 1; i >= 0; i--)
        {
            __instance.DynamicObjects.ElementAt(i).Value.FinalizeProperties();
        }

        for (int i = __instance.StaticObjects.Count - 1; i >= 0; i--)
        {
            __instance.StaticObjects.ElementAt(i).Value.FinalizeProperties();
        }

        return false;
    }

    /// <summary>
    ///     This class will be called at the end of every round.
    ///     Use it to dispose your collections or reset some data.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.DisposeAllObjects))]
    private static void DisposeData()
    {
        SyncHandler.Attempts.Clear();
        ExtendedPlayer.ExtendedPlayersTable.Clear();
        ExtendedModifiers.ExtendedModifiersTable.Clear();
    }
}