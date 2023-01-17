using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Greatsword : MWeapon, ISharpMelee, IMedievalMelee
{
    private bool _doLanding;

    internal Greatsword()
    {
        MWeaponProperties weaponProperties = new(77, "Greatsword", 15f, 22f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "KatanaDraw", "WpnGreatsword", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            Handling = MeleeHandlingType.Custom,
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
            BreakDebris = new[] { "MetalDebris00A", "GreatswordDebris1" },
            AI_DamageOutput = DamageOutputType.High
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("GreatswordM");
        weaponVisuals.SetDrawnTexture("GreatswordD");
        weaponVisuals.SetSheathedTexture("GreatswordS");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee2HVerySlow";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee2H1VerySlow";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee2H2VerySlow";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee2H3VerySlow";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMeleeVerySlow";
        weaponVisuals.AnimDraw = "UpperDrawMeleeSheathed";
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
        weaponProperties.VisualText = "Greatsword";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Greatsword(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public float GetPoise() => 2f;

    public bool CanParry() => false;

    public float GetDecapitationChance() => 1f;

    public override MWeapon Copy() => new Greatsword(Properties, Visuals)
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
        linearVelocity.X *= 0.6f;
        linearVelocity.Y *= 0.6f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroyMetal", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "MetalDebris00A", "GreatswordDebris1" });
    }

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
    {
        if (onKeyEvent)
        {
            if (player.InAir || player.InFreeAir)
            {
                _doLanding = true;
            }
        }

        return false;
    }

    public override void CustomHandlingPostUpdate(Player player, float totalMs)
    {
        if (_doLanding && player.GameOwner != GameOwnerEnum.Server)
        {
            if (player.LayingOnGround || player.IsRecoveryKneeling)
            {
                _doLanding = false;
                EffectHandler.PlayEffect("CAM_S", Vector2.Zero, player.GameWorld, 0.8f, 150f, false);
                EffectHandler.PlayEffect("STM", player.Position, player.GameWorld);
                EffectHandler.PlayEffect("F_S", player.Position, player.GameWorld);
            }
        }
    }
}