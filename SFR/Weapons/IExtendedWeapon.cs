using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFR.Fighter;
using SFR.Helper;
using SFR.Weapons.Melee;

namespace SFR.Weapons;

/// <summary>
/// Since some weapons need special interaction, we need to override and call special methods upon specific actions.
/// However all weapons already inherit from a class, therefore we use an interface and through polymorphism
/// we call overridden methods from hooked harmony patches. The following interface defines all the extra methods a weapon can override.
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
/// Here we hook for specific methods. If an action occurs we try to cast the current weapon to an IExtendedWeapon
/// and call its overridden method from here.
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.HitByMelee))]
    private static bool HitByMelee(Player hitBy, Player __instance)
    {
        object currentWeapon = __instance.GetCurrentWeapon();
        bool canHit = __instance.LastDirectionX != hitBy.LastDirectionX || Math.Abs(__instance.Position.X - hitBy.Position.X) < 4f;

        hitBy.GetAABBMeleeAttack(out var aabb, false);
        var effectPosition = Converter.Box2DToWorld(new Vector2((hitBy.LastDirectionX == 1) ? aabb.upperBound.X : aabb.lowerBound.X, aabb.GetCenter().Y));
        if (hitBy.LastDirectionX == 1 && effectPosition.X > __instance.Position.X)
        {
            effectPosition.X = __instance.Position.X;
        }
        else if (hitBy.LastDirectionX == -1 && effectPosition.X < __instance.Position.X)
        {
            effectPosition.X = __instance.Position.X;
        }

        if (canHit && currentWeapon is RiotShield && !hitBy.IsPerformingGrabAction)
        {
            EffectHandler.PlayEffect("Block", effectPosition, __instance.GameWorld);

            var playerHitMaterial = __instance.GetPlayerHitMaterial();
            var material = playerHitMaterial ?? __instance.GetCurrentMeleeWeaponInUse(false).Properties.WeaponMaterial;
            Material.HandleMeleeVsMelee(hitBy.GetCurrentMeleeWeaponInUse(false).Properties.WeaponMaterial, material, PlayerHitAction.Punch, effectPosition, __instance.GameWorld);

            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.HitByKick))]
    private static bool HitByKick(Player hitBy, Player __instance)
    {
        object currentWeapon = __instance.GetCurrentWeapon();
        bool canHit = __instance.LastDirectionX != hitBy.LastDirectionX || Math.Abs(__instance.Position.X - hitBy.Position.X) < 4f;

        if (canHit && currentWeapon is RiotShield)
        {
            SoundHandler.PlaySound("MeleeBlock", __instance.Position, __instance.GameWorld);
            return false;
        }

        return true;
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
}