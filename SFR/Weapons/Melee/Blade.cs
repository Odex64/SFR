using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Blade : MWeapon, ISharpMelee
{
    internal Blade()
    {
        MWeaponProperties weaponProperties = new(74, "Blade", 9.5f, 13f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "KatanaDraw", "WpnBlade", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
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
            BreakDebris =
            [
                "MetalDebris00A",
                "BladeDebris1"
            ],
            AI_DamageOutput = DamageOutputType.Standard,
            VisualText = "Blade"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee2H",
            AnimMeleeAttack1 = "UpperMelee2H1",
            AnimMeleeAttack2 = "UpperMelee2H2",
            AnimMeleeAttack3 = "UpperMelee2H3",
            AnimFullJumpAttack = "FullJumpAttackMelee",
            AnimDraw = "UpperDrawMeleeSheathed",
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

        weaponVisuals.SetModelTexture("BladeM");
        weaponVisuals.SetDrawnTexture("BladeD");
        weaponVisuals.SetSheathedTexture("BladeS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Blade(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public float GetDecapitationChance() => 0.33f;

    public override MWeapon Copy() => new Blade(Properties, Visuals)
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
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MetalDebris00A", "BladeDebris1"]);
    }
}