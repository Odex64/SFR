using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Crowbar : MWeapon
{
    internal Crowbar()
    {
        MWeaponProperties weaponProperties = new(76, "Crowbar", 9.5f, 10f, "MeleeSlash", "MeleeHitBlunt", "HIT_S", "MeleeBlock", "HIT", "MeleeDraw", "WpnCrowbar", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 2f,
            DurabilityLossOnHitPlayers = 8f,
            DurabilityLossOnHitBlockingPlayers = 4f,
            ThrownDurabilityLossOnHitPlayers = 20f,
            ThrownDurabilityLossOnHitBlockingPlayers = 10f,
            DamageObjects = 50f,
            DamagePlayers = 10f,
            Range = 9f,
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
            BreakDebris = ["MetalDebris00A", "CrowbarDebris1"],
            VisualText = "Crowbar"
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

        weaponVisuals.SetModelTexture("CrowbarM");
        weaponVisuals.SetDrawnTexture("CrowbarD");
        weaponVisuals.SetSheathedTexture("CrowbarS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Crowbar(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override MWeapon Copy() => new Crowbar(Properties, Visuals)
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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MetalDebris00A", "CrowbarDebris1"]);
    }
}