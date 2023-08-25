using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD;
using HarmonyLib;
using SFD.Objects;
using SFDGameScriptInterface;
using SFD.MenuControls;
using System.Runtime.CompilerServices;
using SFR.Helper;

namespace SFR.Fighter
{
    /// <summary>
    ///     This class is used to extend the Player modifiers and save new data to them.
    ///     Basically clones the base class and appends it as a modifier extension to them.
    /// </summary>
    //public class ExtendedModifiers : IEquatable<ExtendedModifiers>
    //{
    //    internal static readonly ConditionalWeakTable<PlayerModifiers, ExtendedModifiers> ExtendedModifiersTable = new();
    //    public PlayerModifiers parentModifiers;
    //    public float JumpHeightModifier;
    //    public float BulletDodgeChance;

    //    public ExtendedModifiers(PlayerModifiers parent, bool defaultValues = false)
    //    {
    //        parentModifiers = parent;
    //        int num = defaultValues ? (-2) : (-1);
    //        JumpHeightModifier = num;
    //        BulletDodgeChance = num;
    //    }

    //    public bool Equals(ExtendedModifiers other)
    //    {
    //        return JumpHeightModifier == other.JumpHeightModifier &&
    //            BulletDodgeChance == other.BulletDodgeChance &&
    //            parentModifiers.Equals(other.parentModifiers);
    //    }
    //}

    //[HarmonyPatch]
    //internal static class ModifierExtension
    //{
    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(PlayerModifiers), MethodType.Constructor)]
    //    private static void GenerateExtension(PlayerModifiers __instance, bool defaultValues = false)
    //    {
    //        ExtendedModifiers.ExtendedModifiersTable.Add(__instance, new ExtendedModifiers(__instance, defaultValues));
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(ObjectProperties), nameof(ObjectProperties.Load), new Type[] { })]
    //    private static void AddExtraProperties()
    //    {
    //        foreach (ObjectPropertyID objectPropertyID in new ObjectPropertyID[]
    //        {
    //            (ObjectPropertyID)500, // Jump Height Modifier.
    //            (ObjectPropertyID)501  // Bullet Dodge Chance.
    //        })
    //        {
    //            string text = objectPropertyID.ToString();
    //            text = "properties.script.playerModifier." + text.Substring(text.IndexOf("_") + 1);
    //            ObjectProperties.AddProperty(new ObjectPropertyItem((int)objectPropertyID, LanguageHelper.GetText(text), -1f)
    //            {
    //                Description = LanguageHelper.GetText(text + ".description"),
    //                SyncType = ObjectPropertySyncType.Unsynced,
    //                LimitToMinValue = true,
    //                MinValue = -2f
    //            });
    //        }
    //    }

    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetProperties))]
    //    private static bool LoadExtraModifiers(ObjectPlayerModifierInfo __instance)
    //    {
    //        __instance.Properties.Add(500, (int)ObjectPropertyID.NONE);
    //        __instance.Properties.Add(501, (int)ObjectPropertyID.NONE);
    //        return true;
    //    }

    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.SetModifiers))]
    //    private static bool SetExtraModifiers(ObjectPlayerModifierInfo __instance, PlayerModifiers value)
    //    {
    //        if (value == null)
    //        {
    //            return false;
    //        }
    //        __instance.Properties.Get(500).Value = value.GetExtension().JumpHeightModifier;
    //        __instance.Properties.Get(501).Value = value.GetExtension().BulletDodgeChance;
    //        return true;
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(ObjectPlayerModifierInfo), nameof(ObjectPlayerModifierInfo.GetModifiers))]
    //    private static void GetExtraModifiers(ObjectPlayerModifierInfo __instance, ref PlayerModifiers __result)
    //    {
    //        __result.GetExtension().JumpHeightModifier = (float)__instance.Properties.Get(500).Value;
    //        __result.GetExtension().BulletDodgeChance = (float)__instance.Properties.Get(501).Value;
    //    }
    //}
}
