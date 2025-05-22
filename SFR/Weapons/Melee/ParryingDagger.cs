using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class ParryingDagger : MWeapon
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
            BreakDebris = ["MetalDebris00A", "ParryingdaggerDebris1"],
            AI_DamageOutput = DamageOutputType.Low,
            VisualText = "Parrying Dagger"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMeleeFast",
            AnimMeleeAttack1 = "UpperMelee1H1Fast",
            AnimMeleeAttack2 = "UpperMelee1H2Fast",
            AnimMeleeAttack3 = "UpperMelee1H3Fast",
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

        weaponVisuals.SetModelTexture("ParryingdaggerM");
        weaponVisuals.SetDrawnTexture("ParryingdaggerD");
        weaponVisuals.SetSheathedTexture("ParryingdaggerS");
        weaponVisuals.SetHolsterTexture("ParryingdaggerH");
        weaponVisuals.SetThrowingTexture("ParryingdaggerThrowing");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private ParryingDagger(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 2f);
        Vector2 linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MetalDebris00A", "ParryingdaggerDebris1"]);
    }
}