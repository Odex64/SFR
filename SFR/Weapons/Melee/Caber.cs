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
    private const float _caberRadius = 32f;
    private const float _caberDamage = 50f;
    private const float _caberSelfDamage = 25f;
    private const float _caberSelfBoost = 6f;
    private const float _caberBoost = 4f;
    private const float _caberMaxSpeed = 8f;
    private const float _caberMinSpeed = 2f;
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
            BreakDebris =
            [
                "CaberDebris1",
                "CaberDebris2"
            ],
            AI_DamageOutput = DamageOutputType.High,
            VisualText = "Caber"
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

        weaponVisuals.SetModelTexture("CaberM");
        weaponVisuals.SetDrawnTexture("CaberD");
        weaponVisuals.SetSheathedTexture("CaberS");

        SetPropertiesAndVisuals(weaponProperties, weaponVisuals);
    }

    private Caber(MWeaponProperties weaponProperties, MWeaponVisuals weaponVisuals) => SetPropertiesAndVisuals(weaponProperties, weaponVisuals);

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

    public void OnHit(Player player, Player target)
    {
    }

    public void OnHitObject(Player player, PlayerHitEventArgs args, ObjectData obj)
    {
    }

    public void Update(Player player, float ms, float realMs)
    {
    }

    public void DrawExtra(SpriteBatch spritebatch, Player player, float ms)
    {
    }

    public override MWeapon Copy() => new Caber(Properties, Visuals)
    {
        Durability =
        {
            CurrentValue = Durability.CurrentValue
        }
    };

    private static void TriggerExplosion(Player ownerPlayer)
    {
        Vector2 position = ownerPlayer.Position + new Vector2(8f * ownerPlayer.LastDirectionX, ownerPlayer.Crouching ? 8f : 0f);
        ownerPlayer.Position += new Vector2(0f, 2f);

        // Create a fake explosion instead
        SoundHandler.PlaySound("Explosion", position, ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("EXP", position, ownerPlayer.GameWorld);
        EffectHandler.PlayEffect("CAM_S", position, ownerPlayer.GameWorld, 8f, 250f, false);

        // Move the owner player and damage him
        ownerPlayer.TakeMiscDamage(_caberSelfDamage, sourceID: ownerPlayer.ObjectID);
        ownerPlayer.SetNewLinearVelocity(new Vector2(ownerPlayer.CurrentVelocity.X, _caberSelfBoost));

        // Move all the nearby players and damage them
        AABB.Create(out AABB area, ownerPlayer.Position, ownerPlayer.Position, _caberRadius);
        foreach (ObjectData obj in ownerPlayer.GameWorld.GetObjectDataByArea(area, false, PhysicsLayer.Active))
        {
            // Damage objects
            if (obj.InternalData is not Player && obj.Destructable)
            {
                obj.DealScriptDamage(100);
            }

            // Damage other players but attacker
            if (obj.InternalData is not Player player || player == ownerPlayer)
            {
                continue;
            }

            // Have to use TakeMiscDamage, to actually kill the players + attach a source id for scripters
            player.TakeMiscDamage(_caberDamage, sourceID: ownerPlayer.ObjectID);
            Vector2 direction = player.Position - (position + new Vector2(0f, -12f));
            float distance = Vector2.Distance(player.Position, position);
            Vector2 boost = direction / distance * _caberBoost;
            player.Position += new Vector2(0f, 2f);

            // Limit target velocity
            if (boost.Length() > _caberMaxSpeed)
            {
                boost.Normalize();
                boost *= _caberMaxSpeed;
            }
            else if (boost.Length() < _caberMinSpeed)
            {
                boost.Normalize();
                boost *= _caberMinSpeed;
            }

            // This will make them ragdoll
            player.SimulateFallWithSpeed(boost + new Vector2(0f, 4f));
        }
    }

    public override void Destroyed(Player player)
    {
        _destroyed = true;
        SoundHandler.PlaySound("DestroyMetal", player.GameWorld);
        EffectHandler.PlayEffect("DestroyMetal", player.Position, player.GameWorld);
        Vector2 center = new(player.Position.X, player.Position.Y + 16f);
        player.GameWorld.SpawnDebris(player.ObjectData, center, 8f, ["CaberDebris1", "CaberDebris2"]);
        TriggerExplosion(player);
    }
}