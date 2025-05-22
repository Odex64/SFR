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
            BreakDebris = ["CueStick00Debris"],
            VisualText = "Broom"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee2H",
            AnimMeleeAttack1 = "UpperMelee2H1",
            AnimMeleeAttack2 = "UpperMelee2H2",
            AnimMeleeAttack3 = "UpperMelee2H3",
            AnimFullJumpAttack = "FullJumpAttackMelee",
            AnimDraw = "UpperDrawMelee",
            AnimCrouchUpper = "UpperCrouchMelee2H",
            AnimIdleUpper = "UpperIdleMelee2H",
            AnimJumpKickUpper = "UpperJumpKickMelee",
            AnimJumpUpper = "UpperJumpMelee2H",
            AnimJumpUpperFalling = "UpperJumpFallingMelee2H",
            AnimKickUpper = "UpperKickMelee2H",
            AnimStaggerUpper = "UpperStagger",
            AnimRunUpper = "UpperRunMelee2H",
            AnimWalkUpper = "UpperWalkMelee2H",
            AnimFullLand = "FullLandMelee",
            AnimToggleThrowingMode = "UpperToggleThrowing"
        };

        weaponVisuals.SetModelTexture("Broom00");
        weaponVisuals.SetDrawnTexture("BroomD");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Broom(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["CueStick00Debris"]);
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

        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        Vector2 unitX = Vector2.UnitX;
        SFDMath.ProjectUonV(ref linearVelocity, ref unitX, out Vector2 x);
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