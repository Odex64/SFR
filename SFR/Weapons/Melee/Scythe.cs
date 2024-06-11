using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Scythe : MWeapon
{
    internal Scythe()
    {
        MWeaponProperties weaponProperties = new(108, "Scythe", 8.5f, 20f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "MeleeDrawMetal", "WpnScythe", false, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 8f,
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
            BreakDebris = ["ScytheDebris1", "MetalDebris00A"],
            Handling = MeleeHandlingType.Custom,
            VisualText = "Scythe"
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

        weaponVisuals.SetModelTexture("ScytheM");
        weaponVisuals.SetDrawnTexture("ScytheD");
        weaponVisuals.SetSheathedTexture("ScytheS");
        weaponVisuals.SetHolsterTexture("ScytheH");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Extended", "Curved"]);
    }

    private Scythe(MWeaponProperties rwp, MWeaponVisuals rwv) => SetPropertiesAndVisuals(rwp, rwv);

    public override MWeapon Copy() => new Scythe(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent) => onKeyEvent && player.CurrentAction is PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 or PlayerAction.MeleeAttack3;

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
    }
}