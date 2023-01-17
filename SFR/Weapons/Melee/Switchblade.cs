using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee;

internal sealed class Switchblade : MWeapon
{
    internal Switchblade()
    {
        MWeaponProperties weaponProperties = new(84, "Switchblade", 9f, 16f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "MeleeDrawMetal", "WpnSwitchblade", true, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.OneHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 8f,
            DurabilityLossOnHitPlayers = 16f,
            DurabilityLossOnHitBlockingPlayers = 8f,
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
            BreakDebris = new[] { "MetalDebris00A", "KnifeDebris1" }
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("SwitchbladeM");
        weaponVisuals.SetDrawnTexture("SwitchbladeD");
        weaponVisuals.SetSheathedTexture("SwitchbladeS");
        weaponVisuals.SetThrowingTexture("SwitchbladeThrowing");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H1";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee1H2";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee1H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMelee";
        weaponVisuals.AnimCrouchUpper = "UpperCrouch";
        weaponVisuals.AnimIdleUpper = "UpperIdle";
        weaponVisuals.AnimJumpKickUpper = "UpperJumpKickMelee";
        weaponVisuals.AnimJumpUpper = "UpperJump";
        weaponVisuals.AnimJumpUpperFalling = "UpperJumpFalling";
        weaponVisuals.AnimKickUpper = "UpperKick";
        weaponVisuals.AnimStaggerUpper = "UpperStagger";
        weaponVisuals.AnimRunUpper = "UpperRun";
        weaponVisuals.AnimWalkUpper = "UpperWalk";
        weaponVisuals.AnimFullLand = "FullLand";
        weaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
        weaponProperties.VisualText = "Switchblade";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
        CacheDrawnTextures(new[] { "Open" });
    }

    private Switchblade(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public override Texture2D GetDrawnTexture(ref GetDrawnTextureArgs args)
    {
        if (args.Player is { CurrentAction: PlayerAction.MeleeAttack1 or PlayerAction.MeleeAttack2 or PlayerAction.MeleeAttack3 or PlayerAction.Block or PlayerAction.JumpAttack })
        {
            args.Postfix = "Open";
        }

        return base.GetDrawnTexture(ref args);
    }

    public override MWeapon Copy() => new Switchblade(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    public override void OnThrowWeaponItem(Player player, ObjectWeaponItem thrownWeaponItem)
    {
        thrownWeaponItem.Body.SetAngularVelocity(thrownWeaponItem.Body.GetAngularVelocity() * 2f);
        var linearVelocity = thrownWeaponItem.Body.GetLinearVelocity();
        linearVelocity.X *= 1.5f;
        linearVelocity.Y *= 1.25f;
        thrownWeaponItem.Body.SetLinearVelocity(linearVelocity);
        base.OnThrowWeaponItem(player, thrownWeaponItem);
    }

    public override void Destroyed(Player ownerPlayer)
    {
        SoundHandler.PlaySound("DestroyMetal", ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", ownerPlayer.Position, ownerPlayer.GameWorld);
        Vector2 center = new(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        ownerPlayer.GameWorld.SpawnDebris(ownerPlayer.ObjectData, center, 8f, new[] { "MetalDebris00A", "KnifeDebris1" });
    }
}