using SFDGameScriptInterface;
using SFR.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SFD;
using SFD.Objects;

namespace SFR.Helper;

[HarmonyPatch]
internal static class FighterModifiers
{
    internal static Dictionary<PlayerModifiers, ExtendedModifiers> ModifierExtensionsLink = new();

    /// <summary>
    /// Finds the corresponding <see cref="ExtendedModifiers"/> for this <see cref="PlayerModifiers"/>
    /// </summary>
    /// <returns>An <see cref="ExtendedModifiers"/> corresponding to this.</returns>
    internal static ExtendedModifiers GetExtension(this PlayerModifiers playerModifiers)
    {
        ExtendedModifiers result;
        if (ModifierExtensionsLink.TryGetValue(playerModifiers, out result))
        {
            Logger.LogDebug($"Found Extended Modifiers!");
            return result;
        }
        else
        {
            // If there is no matching ExtendedModifiers, make a new one and link it immediately.
            Logger.LogDebug($"Could not find Extended Modifiers!");
            var extendedModifiers = new ExtendedModifiers(playerModifiers);
            ExtendedModifiers.ExtendedModifiersList.Add(extendedModifiers);
            return extendedModifiers;
        }

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerModifiers), MethodType.Constructor, new Type[] { typeof(bool) })]
    private static void ExtendedConstructor(bool defaultValues, PlayerModifiers __instance)
    {
        int num = (defaultValues ? (-2) : (-1));

        // Insert the new values while automatically creating an extension for the PlayerModifiers.
        ExtendedModifiers instanceExtension = __instance.GetExtension();
        instanceExtension.JumpHeightModifier = (float)num;

        // Create a provisory link.
        ModifierExtensionsLink.Add(__instance, instanceExtension);
    }

    // Read modifiers by extension.

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetProperties))]
    private static void SetExtendedProperties(ObjectPlayerModifierInfo __instance)
    {
        __instance.Properties.Add((ObjectPropertyID)370, ObjectPropertyID.NONE); // Jump height modifier.
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.GetModifiers))]
    private static PlayerModifiers GetExtendedModifiers(PlayerModifiers value, ObjectPlayerModifierInfo __instance)
    {
        value.GetExtension().JumpHeightModifier = (float)__instance.Properties.Get((ObjectPropertyID)370).Value;

        return value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetModifiers))]
    private static void SetExtendedModifiers(PlayerModifiers value, ObjectPlayerModifierInfo __instance)
    {
        if (value == null)
        {
            return;
        }
        __instance.Properties.Get((ObjectPropertyID)370).Value = value.GetExtension().JumpHeightModifier;
    }
}
