using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Rapier : MWeapon
{
    private const float LungeSpeed = 4f;
    private const int LungeDuration = 200;
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
            BreakDebris = new[] { "MetalDebris00A", "RapierDebris1" }
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("RapierM");
        weaponVisuals.SetDrawnTexture("RapierD");
        weaponVisuals.SetSheathedTexture("RapierS");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H2";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee2H2";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee1H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMeleeSheathed";
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
        weaponProperties.VisualText = "Rapier";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Rapier(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

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
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "MetalDebris00A", "RapierDebris1" });
    }

    public override void CustomHandlingPostUpdate(Player player, float totalMs)
    {
        if (player.CurrentAction == PlayerAction.MeleeAttack3)
        {
            if ((player.GetTopSpeed() == 2.25f || player.SpeedBoostActive) && !_lungeDone)
            {
                player.CurrentSpeed = new Vector2(player.AimVector().X > 0 ? LungeSpeed : -LungeSpeed, 0f);
                _lungeTimer += totalMs;
            }

            if ((player.GetTopSpeed() == 1f || _lungeTimer >= LungeDuration) && !_lungeDone)
            {
                _lungeDone = true;
            }
        }
        else
        {
            _lungeDone = false;
            _lungeTimer = 0f;
        }

        base.CustomHandlingPostUpdate(player, totalMs);
    }

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
    {
        if (!onKeyEvent)
        {
            if (player.CurrentAction == PlayerAction.JumpAttack)
            {
                player.CurrentSpeed = new Vector2(player.AimVector().X > 0 ? LungeSpeed * 1.4f : -LungeSpeed * 1.4f, player.CurrentSpeed.Y);
            }
        }

        base.CustomHandlingOnAttackKey(player, onKeyEvent);
        return false;
    }
}