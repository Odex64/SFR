using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFD.Weapons;

namespace SFR.Weapons.Melee
{
    internal sealed class Scythe : MWeapon
    {
        public Scythe()
        {
            MWeaponProperties mweaponProperties = new(108, "Scythe", 8.5f, 20f, "MeleeSlash", "MeleeHitSharp", "HIT_S", "MeleeBlock", "HIT", "MeleeDrawMetal", "WpnScythe", false, WeaponCategory.Melee, false)
            {
                MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
                WeaponMaterial = MaterialDatabase.Get("metal"),
                DurabilityLossOnHitObjects = 8f,
                DurabilityLossOnHitPlayers = 8f,
                DurabilityLossOnHitBlockingPlayers = 4f,
                ThrownDurabilityLossOnHitPlayers = 20f,
                ThrownDurabilityLossOnHitBlockingPlayers = 10f
            };
            mweaponProperties.DeflectionDuringBlock.DeflectType = DeflectBulletType.Deflect;
            mweaponProperties.DeflectionDuringBlock.DurabilityLoss = 4f;
            mweaponProperties.DeflectionOnAttack.DeflectType = DeflectBulletType.Deflect;
            mweaponProperties.DeflectionOnAttack.DurabilityLoss = 4f;
            mweaponProperties.BreakDebris = new string[] { "ScytheDebris1", "MetalDebris00A" };
            mweaponProperties.Handling = MeleeHandlingType.Custom;
            MWeaponVisuals mweaponVisuals = new();
            mweaponVisuals.SetModelTexture("ScytheM");
            mweaponVisuals.SetDrawnTexture("ScytheD");
            mweaponVisuals.SetSheathedTexture("ScytheS");
            mweaponVisuals.SetHolsterTexture("ScytheH");
            mweaponVisuals.AnimBlockUpper = "UpperBlockMelee2H";
            mweaponVisuals.AnimMeleeAttack1 = "UpperMelee2H1";
            mweaponVisuals.AnimMeleeAttack2 = "UpperMelee2H2";
            mweaponVisuals.AnimMeleeAttack3 = "UpperMelee2H3";
            mweaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
            mweaponVisuals.AnimDraw = "UpperDrawMelee";
            mweaponVisuals.AnimCrouchUpper = "UpperCrouchMelee2H";
            mweaponVisuals.AnimIdleUpper = "UpperIdleMelee2H";
            mweaponVisuals.AnimJumpKickUpper = "UpperJumpKickMelee";
            mweaponVisuals.AnimJumpUpper = "UpperJumpMelee2H";
            mweaponVisuals.AnimJumpUpperFalling = "UpperJumpFallingMelee2H";
            mweaponVisuals.AnimKickUpper = "UpperKickMelee2H";
            mweaponVisuals.AnimStaggerUpper = "UpperStagger";
            mweaponVisuals.AnimRunUpper = "UpperRunMelee2H";
            mweaponVisuals.AnimWalkUpper = "UpperWalkMelee2H";
            mweaponVisuals.AnimFullLand = "FullLandMelee";
            mweaponVisuals.AnimToggleThrowingMode = "UpperToggleThrowing";
            mweaponProperties.VisualText = "Scythe";
            SetPropertiesAndVisuals(mweaponProperties, mweaponVisuals);
            CacheDrawnTextures(new string[] { "Extended", "Curved" });
        }

        public Scythe(MWeaponProperties rwp, MWeaponVisuals rwv)
        {
            SetPropertiesAndVisuals(rwp, rwv);
        }

        public override MWeapon Copy()
        {
            return new Scythe(Properties, Visuals)
            {
                Durability =
                {
                    CurrentValue = Durability.CurrentValue
                }
            };
        }

        public override bool CustomHandlingOnAttackKey(Player player, bool onKeyEvent)
        {
            return onKeyEvent && (player.CurrentAction == PlayerAction.MeleeAttack1 || player.CurrentAction == PlayerAction.MeleeAttack2 || player.CurrentAction == PlayerAction.MeleeAttack3);
        }

        public override void Destroyed(Player ownerPlayer)
        {
            SoundHandler.PlaySound("DestroySmall", ownerPlayer.GameWorld);
            EffectHandler.PlayEffect("DestroyWood", ownerPlayer.Position, ownerPlayer.GameWorld);
            new Vector2(ownerPlayer.Position.X, ownerPlayer.Position.Y + 16f);
        }
    }
}
