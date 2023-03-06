using HarmonyLib;
using SFD;
using SFD.Weapons;
using SFR.Game;
using SFR.Helper;
using SFR.Objects;
using SFR.Weapons.Rifles;
using System;

namespace SFR.Fighter;

/// <summary>
///     This class contains all patches regarding players movements, delays etc...
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CheckLedgeGrab))]
    private static bool CheckLedgeGrab(Player __instance)
    {
        if (__instance.VirtualKeyboardLastMovement is PlayerMovement.Right or PlayerMovement.Left)
        {
            var data = __instance.LedgeGrabData?.ObjectData;
            if (data is ObjectDoor { IsOpen: true })
            {
                __instance.ClearLedgeGrab();
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanActivateSprint))]
    private static bool ActivateSprint(Player __instance, ref bool __result)
    {
        if (__instance is { CurrentWeaponDrawn: WeaponItemType.Rifle, CurrentRifleWeapon: Barrett, StrengthBoostActive: false })
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Jump), new Type[] { })]
    private static bool JumpModifier(Player __instance)
    {
        //Logger.LogDebug($"Returned ExtendedModifiers with value {__instance.m_modifiers.GetExtension().JumpHeightModifier} for JumpHeightModifier.");
        __instance.Jump(7.55f * __instance.m_modifiers.GetExtension().JumpHeightModifier, false);
        return false;
    }
}