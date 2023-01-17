using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class ParryingDagger : MWeapon, IMedievalMelee
{
    internal ParryingDagger()
    {
        MWeaponProperties weaponProperties = new(79, "Parrying_Dagger", 10f, 9f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "MeleeDrawMetal", "WpnParryingDagger", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.OneHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 2f,
            DurabilityLossOnHitPlayers = 4f,
            DurabilityLossOnHitBlockingPlayers = 0f,
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
            BreakDebris = new[] { "MetalDebris00A", "ParryingdaggerDebris1" },
            AI_DamageOutput = DamageOutputType.Low
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("ParryingdaggerM");
        weaponVisuals.SetDrawnTexture("ParryingdaggerD");
        weaponVisuals.SetSheathedTexture("ParryingdaggerS");
        weaponVisuals.SetHolsterTexture("ParryingdaggerH");
        weaponVisuals.SetThrowingTexture("ParryingdaggerThrowing");
        weaponVisuals.AnimBlockUpper = "UpperBlockMeleeFast";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H1Fast";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee1H2Fast";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee1H3Fast";
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
        weaponProperties.VisualText = "Parrying Dagger";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private ParryingDagger(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public float GetPoise() => 0.2f;

    public bool CanParry() => true;

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 2f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 1.35f;
        linearVelocity.Y *= 1.15f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override MWeapon Copy() => new ParryingDagger(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroyMetal", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "MetalDebris00A", "ParryingdaggerDebris1" });
    }
}