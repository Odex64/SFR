using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Makeshift;

internal sealed class Broom : MWeapon
{
    internal Broom()
    {
        MWeaponProperties weaponProperties = new(72, "Broom", 13f, 17f, "MeleeSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "Broom00", false, WeaponCategory.Melee, true)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            WeaponMaterial = MaterialDatabase.Get("wood"),
            DurabilityLossOnHitObjects = 35f,
            DurabilityLossOnHitPlayers = 75f,
            DurabilityLossOnHitBlockingPlayers = 35f,
            ThrownDurabilityLossOnHitPlayers = 100f,
            ThrownDurabilityLossOnHitBlockingPlayers = 35f,
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
            BreakDebris = new[]
            {
                "CueStick00Debris"
            }
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("Broom00");
        weaponVisuals.SetDrawnTexture("BroomD");
        weaponVisuals.SetSheathedTexture(string.Empty);
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee2H";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee2H1";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee2H2";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee2H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMelee";
        weaponVisuals.AnimCrouchUpper = "UpperCrouchMelee2H";
        weaponVisuals.AnimIdleUpper = "UpperIdleMelee2H";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickMelee";
        weaponVisuals.AnimJumpUpper = "UpperJumpMelee2H";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingMelee2H";
        weaponVisuals.AnimKickUpper = "UpperKickMelee2H";
        weaponVisuals.AnimStaggerUpper = "UpperStagger";
        weaponVisuals.AnimRunUpper = "UpperRunMelee2H";
        weaponVisuals.AnimWalkUpper = "UpperWalkMelee2H";
        weaponVisuals.AnimFullLand = "FullLandMelee";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Broom";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Broom(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "CueStick00Debris" });
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

    public override MWeapon Copy() => new Broom(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };
}