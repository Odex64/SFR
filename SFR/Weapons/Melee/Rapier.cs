﻿using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Rapier : MWeapon
{
    private const float _lungeSpeed = 4f;
    private const float _lungeDuration = 200f;
    private bool _lungeDone;

    private float _lungeTimer;

    internal Rapier()
    {
        MWeaponProperties weaponProperties = new(81, "Rapier", 10.5f, 10.5f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "KatanaDraw", "WpnRapier", false, WeaponCategory.Melee, false)
        {
            Handling = MeleeHandlingType.Custom,
            MeleeWeaponType = MeleeWeaponTypeEnum.OneHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 4f,
            DurabilityLossOnHitPlayers = 8f,
            DurabilityLossOnHitBlockingPlayers = 4f,
            ThrownDurabilityLossOnHitPlayers = 20f,
            ThrownDurabilityLossOnHitBlockingPlayers = 10f,
            DeflectionDuringBlock =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 4f
            },
            DeflectionOnAttack =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 4f
            },
            BreakDebris = ["MetalDebris00A", "RapierDebris1"],
            VisualText = "Rapier"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee",
            AnimMeleeAttack1 = "UpperMelee1H2",
            AnimMeleeAttack2 = "UpperMelee2H2",
            AnimMeleeAttack3 = "UpperMelee1H3",
            AnimFullJumpAttack = "FullJumpAttackMelee",
            AnimDraw = "UpperDrawMeleeSheathed",
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

        weaponVisuals.SetModelTexture("RapierM");
        weaponVisuals.SetDrawnTexture("RapierD");
        weaponVisuals.SetSheathedTexture("RapierS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Rapier(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override MWeapon Copy() => new Rapier(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 0.9f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 0.9f;
        linearVelocity.Y *= 0.9f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroyMetal", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MetalDebris00A", "RapierDebris1"]);
    }

    public override void CustomHandlingPostUpdate(Player player, float totalMs)
    {
        if (player.CurrentAction == PlayerAction.MeleeAttack3)
        {
            if (!_lungeDone)
            {
                if (player.GetTopSpeed() == 1f || _lungeTimer >= _lungeDuration)
                {
                    _lungeDone = true;
                }
                else if (player.GetTopSpeed() == 2.25f || player.SpeedBoostActive)
                {
                    player.CurrentSpeed = new Vector2(player.AimVector().X > 0 ? _lungeSpeed : -_lungeSpeed, 0f);
                    player.ImportantUpdate = true;
                    _lungeTimer += totalMs;
                }
            }
        }
        else
        {
            _lungeDone = false;
            _lungeTimer = 0f;
        }
    }

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
    {
        if (!onKeyEvent)
        {
            if (player.CurrentAction == PlayerAction.JumpAttack)
            {
                player.CurrentSpeed = new Vector2(player.AimVector().X > 0 ? _lungeSpeed * 1.4f : -_lungeSpeed * 1.4f, player.CurrentSpeed.Y);
                player.ImportantUpdate = true;
            }
        }

        return false;
    }
}