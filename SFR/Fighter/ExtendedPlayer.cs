using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using SFD;
using SFD.Projectiles;
using SFD.Sounds;
using SFDGameScriptInterface;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Sync.Generic;

namespace SFR.Fighter;

/// <summary>
///     Since we need to save additional data into the player instance
///     we use this file to "extend" the player class.
/// </summary>
internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly ConditionalWeakTable<Player, ExtendedPlayer> ExtendedPlayers = new();
    internal readonly Player Player;
    internal readonly TimeSequence Time = new();
    internal GenericJetpack GenericJetpack;
    internal JetpackType JetpackType = JetpackType.None;

    internal ExtendedPlayer(Player player) => Player = player;

    internal bool AdrenalineBoost
    {
        get => Time.AdrenalineBoost > 0f;
        set => Time.AdrenalineBoost = value ? TimeSequence.AdrenalineBoostTime : 0f;
    }

    public bool Equals(Player other) => other?.ObjectID == Player.ObjectID;

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void ApplyAdrenalineBoost()
    {
        // var modifiers = new PlayerModifiers(true)
        // {
        //     SprintSpeedModifier = 1.3f,
        //     RunSpeedModifier = 1.3f,
        //     SizeModifier = 1.05f,
        //     MeleeForceModifier = 1.2f,
        //     CurrentHealth = Player.Health.CurrentValue
        // };
        // Player.SetModifiers(modifiers);
        AdrenalineBoost = true;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, new SyncFlag[] { }, Player.ObjectID, GetStates()));
    }

    internal object[] GetStates()
    {
        object[] states = new object[3];
        states[0] = AdrenalineBoost;
        states[1] = (int)JetpackType;
        states[2] = GenericJetpack?.Fuel?.CurrentValue ?? 0f;

        return states;
    }

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void DisableAdrenalineBoost()
    {
        // SoundHandler.PlaySound("StrengthBoostStop", Player.Position, Player.GameWorld);
        // var modifiers = new PlayerModifiers(true)
        // {
        //     CurrentHealth = Player.Health.CurrentValue
        // };
        // Player.SetModifiers(modifiers);
        AdrenalineBoost = false;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, new SyncFlag[] { }, Player.ObjectID, GetStates()));
    }

    internal class TimeSequence
    {
        internal const float AdrenalineBoostTime = 20000f;
        internal float AdrenalineBoost;
    }
}

[HarmonyPatch]
internal static class ModifierApplication
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Jump), new Type[] { })]
    private static bool JumpPatch(Player __instance)
    {
        if (__instance.m_modifiers.GetExtension().JumpHeightModifier != 1f)
        {
            float jumpForce = 7.55f * __instance.m_modifiers.GetExtension().JumpHeightModifier;
            __instance.Jump(jumpForce, false);
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.TestProjectileHit))]
    private static void PatchProjectileHit(ref bool __result, Player __instance, Projectile projectile)
    {
        if (!__result)
        {
            return;
        }
        else
        {
            if (Constants.RANDOM.NextDouble() < __instance.m_modifiers.GetExtension().BulletDodgeChance)
            {
                __result = false;
            }
        }
    }
}