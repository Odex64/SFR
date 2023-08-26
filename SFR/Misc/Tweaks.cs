using System;
using HarmonyLib;
using SFD;
using SFDGameScriptInterface;
using SFR.Weapons;

namespace SFR.Misc;

/// <summary>
///     Here we tweak some system methods (non-SFD) in order to fix some bugs.
/// </summary>
[HarmonyPatch]
internal static class Tweaks
{
    /// <summary>
    ///     When patching enums they return its raw value (number) instead of their name.
    ///     Here we return their correct title based on the object type and enum value.
    ///     E.g: Metrolaw track used to return 42 instead of its name.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), "InternalFormat")]
    private static bool PatchEnums(Type eT, object value, ref string __result)
    {
        if (eT == typeof(MusicHandler.MusicTrackID) && value is int track)
        {
            switch (track)
            {
                case 42:
                    __result = "Metrolaw";
                    return false;

                case 43:
                    __result = "FrozenBlood";
                    return false;
            }
        }
        else if (eT == typeof(ObjectPropertyID) && value is int objectProperty)
        {
            switch (objectProperty)
            {
                case 500:
                    __result = "JumpHeightModifier";
                    return false;
                case 501:
                    __result = "BulletDodgeChance";
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     This method is used to return an enum of all the new weapons.
    ///     This fixed an issue regarding supply crates
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
    private static bool GetEnumValues(Type enumType, ref Array __result)
    {
        if (enumType == typeof(WeaponItem))
        {
            __result = Enum.GetValues(typeof(Database.CustomWeaponItem));
            return false;
        }

        return true;
    }

    /// <summary>
    ///     This patch allows us to override comparison operators without having access to the source code.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(object), nameof(object.Equals), typeof(object), typeof(object))]
    private static bool PatchEquals(object objA, object objB, ref bool __result)
    {
        if (objA is Player playerA && objB is Player playerB)
        {
            __result = playerA.ObjectID == playerB.ObjectID;
            return false;
        }

        return true;
    }
}