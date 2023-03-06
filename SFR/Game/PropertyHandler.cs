using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SFD;
using SFDGameScriptInterface;
using SFR.Helper;

namespace SFR.Game;

[HarmonyPatch]
internal static class PropertyHandler
{
    // Adding new property items.

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectProperties), nameof(ObjectProperties.Load), new Type[] { })]
    private static void AddCustomProperties()
    {
        foreach (ExtendedPropertyID extObjectPropertyID in new ExtendedPropertyID[]
        {
            ExtendedPropertyID.ScriptPlayerModifier_JumpHeightModifier,
        })
        {
            string text = extObjectPropertyID.ToString();
            text = "properties.string.playerModifier." + text.Substring(text.IndexOf("_") + 1);
            ObjectProperties.AddProperty(new ObjectPropertyItem((int)extObjectPropertyID, LanguageHelper.GetText(text), -1f)
            {
                Description = LanguageHelper.GetText(text + ".description"),
                SyncType = ObjectPropertySyncType.Unsynced,
                LimitToMinValue = true,
                MinValue = -2f
            });
        }
    }

    // Patching the modifier extension methodsd.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerModifiersExtension), nameof(PlayerModifiersExtension.SanitizeInput))]
    private static void SanitizeInputExtended(PlayerModifiers modifiers)
    {
        modifiers.GetExtension().JumpHeightModifier = PlayerModifiersExtension.ValidateFloatInput(modifiers.GetExtension().JumpHeightModifier, 0.2f, 2f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerModifiersExtension), nameof(PlayerModifiersExtension.DefaultValues))]
    private static void DefaultValuesExtended(PlayerModifiers modifiers)
    {
        if (modifiers.GetExtension().JumpHeightModifier == -2f)
        {
            modifiers.GetExtension().JumpHeightModifier = 1f;
        }
    }
}

public enum ExtendedPropertyID
{
    ScriptPlayerModifier_JumpHeightModifier = 370,
}
