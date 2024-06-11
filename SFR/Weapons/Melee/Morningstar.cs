using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Morningstar : MWeapon
{
    internal Morningstar()
    {
        MWeaponProperties weaponProperties = new(78, "Morningstar", 14f, 15f, "ChainSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "WpnMorningstar", false, WeaponCategory.Melee, false)
        {
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
            AI_DamageOutput = DamageOutputType.High,
            VisualText = "Morningstar"
        };

        MWeaponVisuals weaponVisuals = new()
        {
            AnimBlockUpper = "UpperBlockMelee",
            AnimMeleeAttack1 = "UpperMelee1H1",
            AnimMeleeAttack2 = "UpperMelee1H2",
            AnimMeleeAttack3 = "UpperMelee1H3Chain",
            AnimFullJumpAttack = "FullJumpAttackMeleeChain",
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

        weaponVisuals.SetModelTexture("MorningstarM");
        weaponVisuals.SetDrawnTexture("MorningstarD");
        weaponVisuals.SetSheathedTexture("MorningstarS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

        CacheDrawnTextures(["Extended", "Curved"]);
    }

    private Morningstar(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

    public override MWeapon Copy() => new Morningstar(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, ["MorningstarDebris1", "MorningstarDebris2"]);
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        string subAnimation;
        if ((subAnimation = args.SubAnimation) is not null)
        {
            if (subAnimation is not "UpperMelee1H1" and not "UpperMelee1H2" and not "UpperMelee1H3")
            {
                if (subAnimation == "UpperBlockMelee")
                {
                    args.Postfix = "Curved";
                }
            }
            else if (args.SubFrame >= 2)
            {
                args.Postfix = "Extended";
            }
        }

        return base.GetDrawnTexture(ref args);
    }
}