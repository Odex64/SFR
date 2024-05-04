using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Makeshift;

internal sealed class Brick : MWeapon
{
    internal Brick()
    {
        MWeaponProperties weaponProperties = new(71, "Brick", 6f, 14f, "MeleeSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "Brick00", false, WeaponCategory.Melee, true)
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
            }
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("Brick00");
        weaponVisuals.SetDrawnTexture("Brick00D");
        weaponVisuals.SetSheathedTexture(string.Empty);
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H1";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee1H2";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee1H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMelee";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchMelee";
        weaponVisuals.AnimIdleUpper = "UpperIdleMelee";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickMelee";
        weaponVisuals.AnimJumpUpper = "UpperJumpMelee";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingMelee";
        weaponVisuals.AnimKickUpper = "UpperKickMelee";
        weaponVisuals.AnimStaggerUpper = "UpperStagger";
        weaponVisuals.AnimRunUpper = "UpperRunMelee";
        weaponVisuals.AnimWalkUpper = "UpperWalkMelee";
        weaponVisuals.AnimFullLand = "FullLandMelee";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Brick";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Brick(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

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
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        var unitX = Vector2.UnitX;
        SFDMath.ProjectUonV(ref linearVelocity, ref unitX, out var x);
        float num = 2f * (x.CalcSafeLength() / linearVelocity.CalcSafeLength());
        thrownWeaponItem.Body.SetAngularVelocity(-(float)Math.Sign(linearVelocity.X) * num);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override MWeapon Copy() => new Brick(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };
}