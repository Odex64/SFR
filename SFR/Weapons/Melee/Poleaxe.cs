using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Poleaxe : MWeapon, ISharpMelee, IMedievalMelee
{
    internal Poleaxe()
    {
        MWeaponProperties weaponProperties = new(80, "Poleaxe", 14f, 16f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "KatanaDraw", "WpnPoleaxe", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            Handling = MeleeHandlingType.Custom,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 4f,
            DurabilityLossOnHitPlayers = 8f,
            DurabilityLossOnHitBlockingPlayers = 4f,
            ThrownDurabilityLossOnHitPlayers = 20f,
            ThrownDurabilityLossOnHitBlockingPlayers = 10f,
            DamageObjects = 50,
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
            BreakDebris = new[] { "MetalDebris00A", "PoleaxeDebris1", "PoleaxeDebris2" },
            AI_DamageOutput = DamageOutputType.High
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("PoleaxeM");
        weaponVisuals.SetDrawnTexture("PoleaxeD");
        weaponVisuals.SetSheathedTexture("PoleaxeS");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee2HVerySlow";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee2H1Slow";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee2H2Slow";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee2H3Slow";
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
        weaponProperties.VisualText = "Poleaxe";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Poleaxe(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public float GetPoise() => 1.2f;

    public bool CanParry() => false;

    public float GetDecapitationChance() => 0.66f;

    public override MWeapon Copy() => new Poleaxe(Properties, Visuals)
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
        linearVelocity.X *= 0.65f;
        linearVelocity.Y *= 0.65f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroyMetal", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "MetalDebris00A", "PoleaxeDebris1", "PoleaxeDebris2" });
    }
}