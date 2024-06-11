using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Makeshift;

internal sealed class CannonBall : MWeapon
{
    internal CannonBall()
    {
        MWeaponProperties weaponProperties = new(73, "Cannonball", 6f, 14f, "MeleeSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "CannonBall00", false, WeaponCategory.Melee, true)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            WeaponMaterial = MaterialDatabase.Get("stone"),
            DurabilityLossOnHitObjects = 30f,
            DurabilityLossOnHitPlayers = 60f,
            DurabilityLossOnHitBlockingPlayers = 40f,
            ThrownDurabilityLossOnHitPlayers = 80f,
            ThrownDurabilityLossOnHitBlockingPlayers = 40f,
            DeflectionDuringBlock =
            {
                DeflectType = DeflectBulletType.Absorb,
                DurabilityLoss = 70f
            },
            DeflectionOnAttack =
            {
                DeflectType = DeflectBulletType.Absorb,
                DurabilityLoss = 70f
            },
            VisualText = "Cannonball"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee",
            AnimMeleeAttack1 = "UpperMelee1H1",
            AnimMeleeAttack2 = "UpperMelee1H2",
            AnimMeleeAttack3 = "UpperMelee1H3",
            AnimFullJumpAttack = "FullJumpAttackMelee",
            AnimDraw = "UpperDrawMelee",
            AnimCrouchUpper = "UpperCrouchMelee",
            AnimIdleUpper = "UpperIdleMelee",
            AnimJumpKickUpper = "UpperJumpKickMelee",
            AnimJumpUpper = "UpperJumpMelee",
            AnimJumpUpperFalling = "UpperJumpFallingMelee",
            AnimKickUpper = "UpperKickMelee",
            AnimStaggerUpper = "UpperStagger",
            AnimRunUpper = "UpperRunMelee",
            AnimWalkUpper = "UpperWalkMelee",
            AnimFullLand = "FullLandMelee",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("CannonBall00");
        weaponVisuals.SetDrawnTexture("CannonBall00");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private CannonBall(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
    }

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        if (player.LastDirectionX > 0)
        {
            thrownWeaponItem.Body.SetTransform(thrownWeaponItem.Body.Position, thrownWeaponItem.Body.Rotation - 1.57079637f);
        }
        else
        {
            thrownWeaponItem.Body.SetTransform(thrownWeaponItem.Body.Position, thrownWeaponItem.Body.Rotation + 1.57079637f);
        }

        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity *= 0.25f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        var unitX = Vector2.UnitX;
        SFDMath.ProjectUonV(ref linearVelocity, ref unitX, out var x);
        float num = 2f * (x.CalcSafeLength() / linearVelocity.CalcSafeLength());
        thrownWeaponItem.Body.SetAngularVelocity(-(float)Math.Sign(linearVelocity.X) * num);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override MWeapon Copy() => new CannonBall(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };
}