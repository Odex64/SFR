using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFR.Fighter;
using SFR.Helper;

namespace SFR.Weapons;

/// <summary>
///     Since some weapons need special interaction, we need to override and call special methods upon specific actions.
///     However all weapons already inherit from a class, therefore we use an interface and through polymorphism
///     we call overridden methods from hooked harmony patches. The following interface defines all the extra methods a weapon can override.
/// </summary>
internal interface IExtendedWeapon
{
    abstract void GetDealtDamage(Player player, float damage);
    abstract void OnHit(Player player, Player target);
    abstract void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj);
    abstract void Update(Player player, float ms, float realMs);
    abstract void DrawExtra(SpriteBatch spritebatch, Player player, float ms);
}

/// <summary>
///     Here we hook for specific methods. If an action occurs we try to cast the current weapon to an IExtendedWeapon
///     and call its overridden method from here.
/// </summary>
[HarmonyPatch]
internal static class ExtendedWeapon
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateMeleeWeaponDurabilityOnHitObjects))]
    private static void GetDealtDamage(IEnumerable<ObjectData> objectsHitInMelee, Player __instance)
    {
        var weapon = __instance.GetCurrentMeleeWeaponInUse();
        if (weapon is IExtendedWeapon wep)
        {
            foreach (var objectData in objectsHitInMelee)
            {
                if (!objectData.IsPlayer)
                {
                    wep.GetDealtDamage(__instance, weapon.Properties.DurabilityLossOnHitObjects);
                }
                else
                {
                    var player = (Player)objectData.InternalData;
                    wep.GetDealtDamage(__instance, player.CurrentAction != PlayerAction.Block ? weapon.Properties.DurabilityLossOnHitPlayers : weapon.Properties.DurabilityLossOnHitBlockingPlayers);
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.HitByMelee))]
    private static void OnPlayerHit(Player hitBy, Player __instance)
    {
        GoreHandler.MeleeHit(hitBy, __instance);

        var weapon = hitBy.GetCurrentMeleeWeaponInUse();
        if (weapon is IExtendedWeapon wep)
        {
            wep.OnHit(hitBy, __instance);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.PlayerMeleeHit))]
    private static void OnHitObject(Player player, PlayerHitEventArgs e, ObjectData __instance)
    {
        var weapon = player.GetCurrentMeleeWeaponInUse();
        if (weapon is IExtendedWeapon wep)
        {
            wep.OnHitObject(player, e, __instance);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    private static void Update(float ms, float realMs, Player __instance)
    {
        object weapon = __instance.GetCurrentWeapon();
        if (weapon is IExtendedWeapon wep)
        {
            wep.Update(__instance, ms, realMs);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static void DrawExtra(SpriteBatch spriteBatch, float ms, Player __instance)
    {
        object weapon = __instance.GetCurrentWeapon();
        if (weapon is IExtendedWeapon wep)
        {
            wep.DrawExtra(spriteBatch, __instance, ms);
        }
    }
}