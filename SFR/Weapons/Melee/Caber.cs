using Box2D.XNA;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFD.Weapons;
using SFDGameScriptInterface;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SFR.Weapons.Melee;

internal sealed class Caber : MWeapon, IExtendedWeapon
{
    private const float CaberRadius = 32f;
    private const float CaberDamage = 50f;
    private const float CaberSelfDamage = 25f;
    private const float CaberSelfBoost = 6f;
    private const float CaberBoost = 4f;
    private const float CaberMaxSpeed = 8f;
    private const float CaberMinSpeed = 2f;
    private bool _destroyed;
    private float _fallBackDamage;

    internal Caber()
    {
        MWeaponProperties weaponProperties = new(75, "Caber", 7.5f, 1f, "MeleeSwing", "MeleeHitBlunt", "HIT_B", "MeleeBlock", "HIT", "MeleeDraw", "WpnCaber", false, WeaponCategory.Melee, false)
        {
            MeleeWeaponType = MeleeWeaponTypeEnum.TwoHanded,
            WeaponMaterial = MaterialDatabase.Get("metal"),
            DurabilityLossOnHitObjects = 110f,
            DurabilityLossOnHitPlayers = 100f,
            DurabilityLossOnHitBlockingPlayers = 100f,
            ThrownDurabilityLossOnHitPlayers = 0f,
            ThrownDurabilityLossOnHitBlockingPlayers = 0f,
            DeflectionDuringBlock =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 100f
            },
            DeflectionOnAttack =
            {
                DeflectType = DeflectBulletType.Deflect,
                DurabilityLoss = 100f
            },
            BreakDebris = new[]
            {
                "CaberDebris1",
                "CaberDebris2"
            },
            AI_DamageOutput = DamageOutputType.High
        };

        MWeaponVisuals weaponVisuals = new();
        weaponVisuals.SetModelTexture("CaberM");
        weaponVisuals.SetDrawnTexture("CaberD");
        weaponVisuals.SetSheathedTexture("CaberS");
        weaponVisuals.AnimBlockUpper = "UpperBlockMelee";
        weaponVisuals.AnimMeleeAttack1 = "UpperMelee1H1";
        weaponVisuals.AnimMeleeAttack2 = "UpperMelee1H2";
        weaponVisuals.AnimMeleeAttack3 = "UpperMelee1H3";
        weaponVisuals.AnimFullJumpAttack = "FullJumpAttackMelee";
        weaponVisuals.AnimDraw = "UpperDrawMelee";
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
        weaponProperties.VisualText = "Caber";

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Caber(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals)
    {
        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    public void GetDealtDamage(Player player, float damage)
    {
        if (_destroyed)
        {
            return;
        }

        if (player.InfiniteAmmo || Cheat.InfiniteAmmo)
        {
            _fallBackDamage += damage;
        }

        if (_fallBackDamage >= 100f)
        {
            TriggerExplosion(player);
            _fallBackDamage = 0f;
        }
    }

    public void OnHit(Player player, Player target) { }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj) { }

    public void Update(Player player, float ms, float realMs) { }
    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms) { }

    public void BeforeHit(Player player, Player target) { }

    public override MWeapon Copy() => new Caber(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    private static void TriggerExplosion(Player ownerPlayer)
    {
        var position = ownerPlayer.Position + new Vector2(8f * ownerPlayer.LastDirectionX, ownerPlayer.Crouching ? 8f : 0f);
        ownerPlayer.Position += new Vector2(0f, 2f);

        // Create a fake explosion instead
        SoundHandler.PlaySound("Explosion", position, ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("EXP", position, ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("CAM_S", position, ownerPlayer.GameWorld, 8f, 250f, false);

        // Move the owner player and damage him
        ownerPlayer.TakeMiscDamage(CaberSelfDamage, sourceID: ownerPlayer.ObjectID);
        ownerPlayer.SetNewLinearVelocity(new Vector2(ownerPlayer.CurrentVelocity.X, CaberSelfBoost));

        // Move all the nearby players and damage them
        AABB.Create(out var area, ownerPlayer.Position, ownerPlayer.Position, CaberRadius);
        foreach (var obj in ownerPlayer.GameWorld.GetObjectDataByArea(area, false, PhysicsLayer.Active))
        {
            //Damage objects
            if (obj.InternalData is not Player && obj.Destructable)
            {
                obj.DealScriptDamage(100);
            }

            //What does this even mean
            if (obj.InternalData is not Player player || player == ownerPlayer)
            {
                continue;
            }

            //Have to use TakeMiscDamage, to actually kill the players + attach a source id for scripters
            player.TakeMiscDamage(CaberDamage, sourceID: ownerPlayer.ObjectID);
            var direction = player.Position - (position + new Vector2(0f, -12f));
            float distance = Vector2.Distance(player.Position, position);
            var boost = direction / distance * CaberBoost;
            player.Position += new Vector2(0f, 2f);

            //Limit target velocity
            if (boost.Length() > CaberMaxSpeed)
            {
                boost.Normalize();
                boost *= CaberMaxSpeed;
            }
            else if (boost.Length() < CaberMinSpeed)
            {
                boost.Normalize();
                boost *= CaberMinSpeed;
            }

            //This will make them ragdoll
            player.SimulateFallWithSpeed(boost + new Vector2(0f, 4f));
        }
    }

    public override void Destroyed(Player player)
    {
        _destroyed = true;
        SoundHandler.PlaySound("DestroyMetal", player.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", player.Position, player.GameWorld);
        Vector2 center = new(player.Position.X, player.Position.Y + 16f);
        player.GameWorld.SpawnDebris(player.ObjectData, center, 8f, new[] { "CaberDebris1", "CaberDebris2" });
        TriggerExplosion(player);
    }
}