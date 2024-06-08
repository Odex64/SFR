using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class RiotShield : MWeapon
{
    internal RiotShield()
    {
        MWeaponProperties weaponProperties = new(82, "RiotShield", 15f, 22f, "MeleeSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "WpnRiotShield", true, WeaponCategory.Melee, false)
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
            BreakDebris = ["MetalDebris00A", "RiotShieldDebris1"],
            AI_DamageOutput = DamageOutputType.High,
            VisualText = "Riot Shield"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee2HSlow",
            AnimFullJumpAttack = "FullJumpAttackMeleeSlow",
            AnimCrouchUpper = "UpperCrouchMelee2H",
            AnimJumpKickUpper = "UpperJumpKickMelee",

            AnimRunUpper = "UpperRunMelee",
            AnimMeleeAttack3 = "UpperMelee2H3Slow",
            AnimDraw = "UpperDrawMeleeSheathed",
            AnimIdleUpper = "UpperIdleRiotShield",
            AnimJumpUpper = "UpperIdleRiotShield",
            AnimJumpUpperFalling = "UpperIdleRiotShield",
            AnimKickUpper = "UpperIdleRiotShield",
            AnimStaggerUpper = "UpperStagger",
            AnimWalkUpper = "UpperIdleRiotShield",
            AnimFullLand = "FullLandMelee",
            AnimToggleThrowingMode = "UpperToggleThrowing",
        };
        weaponVisuals.SetModelTexture("RiotShieldM");
        weaponVisuals.SetDrawnTexture("RiotShieldD");
        weaponVisuals.SetSheathedTexture("RiotShieldS");
        weaponVisuals.SetThrowingTexture("RiotShieldD");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private RiotShield(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override MWeapon Copy() => new RiotShield(Properties, Visuals)
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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MetalDebris00A", "GreatswordDebris1"]);
    }

    public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
    {
        if (onKeyEvent && player.CurrentAction is PlayerAction.Idle && !player.InAir)
        {
            player.CurrentAction = PlayerAction.MeleeAttack3;
        }

        return true;
    }
}