using System;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Tiles;
using SFR.Objects;
using SFR.Sync.Generic;

namespace SFR.Projectiles;

internal sealed class ProjectileCrossbow : Projectile, IExtendedProjectile
{
    private float _gravity;
    private float _velocity;

    internal ProjectileCrossbow()
    {
        Visuals = new(Textures.GetTexture("CrossbowBolt00"), Textures.GetTexture("CrossbowBolt00"));
        Properties = new(96, 700f, 50f, 30f, 40f, 0f, 30f, 40f, 0.5f)
        {
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Fireplosion,
            PowerupTotalBounces = 8,
            PowerupFireIgniteValue = 56f
        };
    }

    private ProjectileCrossbow(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override float SlowmotionFactor => 1f - (1f - GameWorld.SlowmotionHandler.SlowmotionModifier) * 0.5f;

    public bool OnHit(Projectile projectile, ProjectileHitEventArgs e, ObjectData objectData) => true;

    public bool OnExplosiveHit(Projectile projectile, ProjectileHitEventArgs e, ObjectExplosive objectData)
    {
        ObjectDataMethods.ApplyProjectileHitImpulse(objectData, projectile, e);
        return false;
    }

    public bool OnExplosiveBarrelHit(Projectile projectile, ProjectileHitEventArgs e, ObjectBarrelExplosive objectData)
    {
        ObjectDataMethods.ApplyProjectileHitImpulse(objectData, projectile, e);
        return false;
    }

    public override Projectile Copy()
    {
        Projectile projectile = new ProjectileCrossbow(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }

    public override void Update(float ms)
    {
        _velocity += ms;
        float scaleFactor = Math.Min(_velocity / 500f, 1f);
        Velocity -= Vector2.UnitY * ms * 0.66f * scaleFactor;
        if (GameOwner != GameOwnerEnum.Server && PowerupFireActive)
        {
            _gravity -= ms;
            if (_gravity <= 0f)
            {
                if (Constants.EFFECT_LEVEL_FULL)
                {
                    EffectHandler.PlayEffect("TR_S", Position, GameWorld);
                }

                if (PowerupFireActive)
                {
                    EffectHandler.PlayEffect("TR_F", Position, GameWorld);
                }

                _gravity = Constants.EFFECT_LEVEL_FULL ? 15f : 30f;
            }
        }
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            player.TakeProjectileDamage(this);
            var material = player.GetPlayerHitMaterial() ?? playerObjectData.Tile.Material;

            SoundHandler.PlaySound(material.Hit.Projectile.HitSound, GameWorld);
            EffectHandler.PlayEffect(material.Hit.Projectile.HitEffect, Position, GameWorld);
            SoundHandler.PlaySound("MeleeHitSharp", GameWorld);

            Remove();
            var data = (ObjectCrossbowBolt)GameWorld.IDCounter.NextObjectData("CrossbowBolt01");
            SpawnObjectInformation spawnObject = new(data, Position, -GetAngle(), 1, Vector2.Zero, 0);
            data.Timer = GameWorld.ElapsedTotalGameTime + 10000;
            var body = GameWorld.CreateTile(spawnObject);
            body.SetType(BodyType.Static);

            data.ApplyPlayerBolt(player);
            data.EnableUpdateObject();
            if (GameOwner == GameOwnerEnum.Server)
            {
                GenericData.SendGenericDataToClients(new(DataType.Crossbow, [SyncFlag.NewObjects], data.ObjectID, player.ObjectID, data.Timer));
            }
        }
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        base.HitObject(objectData, e);
        if (GameOwner != GameOwnerEnum.Client && !PowerupBounceActive && !objectData.IsPlayer && objectData.GetCollisionFilter().AbsorbProjectile)
        {
            Remove();
            var data = (ObjectCrossbowBolt)ObjectData.CreateNew(new(GameWorld.IDCounter.NextID(), 0, 0, "CrossbowBolt00", GameOwner));
            _ = GameWorld.CreateTile(new(data, Position, -GetAngle(), 1, objectData.LocalRenderLayer, objectData.GetLinearVelocity(), 0));
            data.Timer = GameWorld.ElapsedTotalGameTime + 15000;
            data.EnableUpdateObject();
            data.FilterObjectId = objectData.BodyID;

            if (objectData.IsStatic)
            {
                data.ChangeBodyType(BodyType.Static);
            }
        }
    }
}