using HarmonyLib;
using SFD;
using SFD.Objects;
using SFDGameScriptInterface;
using SFR.Helper;
using Player = SFD.Player;

namespace SFR.Fighter;

[HarmonyPatch]
internal class ModifiersHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectProperties), nameof(ObjectProperties.Load), [])]
    private static void AddExtraProperties()
    {
        foreach (var objectPropertyId in new[]
                 {
                     (ObjectPropertyID)500, // Jump Height Modifier.
                     (ObjectPropertyID)501 // Bullet Dodge Chance.
                 })
        {
            string text = objectPropertyId.ToString();
            string description = objectPropertyId switch
            {
                (ObjectPropertyID)500 => "[0, 2]",
                (ObjectPropertyID)501 => "[0, 1]",
                _ => string.Empty
            };
            ObjectProperties.AddProperty(new ObjectPropertyItem((int)objectPropertyId, text, -1f)
            {
                Description = description,
                SyncType = ObjectPropertySyncType.Unsynced,
                LimitToMinValue = true,
                MinValue = -2
            });
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetProperties))]
    private static void LoadExtraModifiers(ObjectPlayerModifierInfo __instance)
    {
        __instance.Properties.Add(500);
        __instance.Properties.Add(501);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetModifiers))]
    private static bool SetExtraModifiers(ObjectPlayerModifierInfo __instance, PlayerModifiers value)
    {
        if (value == null)
        {
            return false;
        }

        var extendedModifiers = value.GetExtension();
        __instance.Properties.Get(500).Value = extendedModifiers.JumpHeightModifier;
        __instance.Properties.Get(501).Value = extendedModifiers.BulletDodgeChance;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.GetModifiers))]
    private static void GetExtraModifiers(ObjectPlayerModifierInfo __instance, ref PlayerModifiers __result)
    {
        var extendedModifiers = __result.GetExtension();
        extendedModifiers.JumpHeightModifier = (float)__instance.Properties.Get(500).Value;
        extendedModifiers.BulletDodgeChance = (float)__instance.Properties.Get(501).Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerModifiersExtension), nameof(PlayerModifiersExtension.SanitizeInput))]
    private static void SanitizeInputPatch(PlayerModifiers modifiers)
    {
        var extendedModifiers = modifiers.GetExtension();
        extendedModifiers.JumpHeightModifier = PlayerModifiersExtension.ValidateFloatInput(extendedModifiers.JumpHeightModifier, 0f, 2f);
        extendedModifiers.BulletDodgeChance = PlayerModifiersExtension.ValidateFloatInput(extendedModifiers.BulletDodgeChance, 0f, 1f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerModifiersExtension), nameof(PlayerModifiersExtension.DefaultValues))]
    private static void DefaultValuesPatch(PlayerModifiers modifiers)
    {
        var extendedModifiers = modifiers.GetExtension();
        if (extendedModifiers.JumpHeightModifier == -1f)
        {
            extendedModifiers.JumpHeightModifier = 1f;
        }

        if (extendedModifiers.BulletDodgeChance == -1f)
        {
            extendedModifiers.BulletDodgeChance = 0f;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.SetModifiers))]
    private static void SetModifiersPatch(Player __instance, PlayerModifiers value)
    {
        if (value != null)
        {
            var extendedModifiers = value.GetExtension();
            var currentExtendedModifiers = __instance.m_modifiers.GetExtension();
            if (extendedModifiers.JumpHeightModifier != -1f)
            {
                __instance.ModifiersUpdated |= currentExtendedModifiers.JumpHeightModifier != extendedModifiers.JumpHeightModifier;
                currentExtendedModifiers.JumpHeightModifier = extendedModifiers.JumpHeightModifier;
            }

            if (extendedModifiers.BulletDodgeChance != -1f)
            {
                __instance.ModifiersUpdated |= currentExtendedModifiers.BulletDodgeChance != extendedModifiers.BulletDodgeChance;
                currentExtendedModifiers.BulletDodgeChance = extendedModifiers.BulletDodgeChance;
            }
        }
    }
}