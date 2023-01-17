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

namespace SFR.Projectiles;

internal sealed class ProjectileCrossbow : Projectile, IExtendedProjectile
{
    private ObjectCrossbowBolt _data;
    private float _gravity;
    private float _velocity;

    internal ProjectileCrossbow()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("CrossbowBolt00"), Textures.GetTexture("CrossbowBolt00"));
        Properties = new ProjectileProperties(96, 700f, 50f, 30f, 40f, 0f, 30f, 40f, 0.5f)
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
        }

        if (GameOwner != GameOwnerEnum.Server)
        {
            var data = (ObjectCrossbowBolt)GameWorld.IDCounter.NextLocalObjectData("CrossbowBolt01");
            SpawnObjectInformation spawnObject = new(data, Position, -GetAngle(), 1, Vector2.Zero, 0);
            GameWorld.CreateTile(spawnObject);
            data.ChangeBodyType(BodyType.Static);
            data.Timer = GameWorld.ElapsedTotalGameTime + 10000;
            data.ApplyPlayerBolt(player);
            data.EnableUpdateObject();
        }
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        base.HitObject(objectData, e);
        if (GameOwner != GameOwnerEnum.Client && !PowerupBounceActive && !objectData.IsPlayer && objectData.GetCollisionFilter().AbsorbProjectile)
        {
            _data = (ObjectCrossbowBolt)ObjectData.CreateNew(new ObjectDataStartParams(GameWorld.IDCounter.NextID(), 0, 0, "CrossbowBolt00", GameOwner));
            GameWorld.CreateTile(new SpawnObjectInformation(_data, Position, -GetAngle(), 1, objectData.LocalRenderLayer, objectData.GetLinearVelocity(), 0));
            _data.Timer = GameWorld.ElapsedTotalGameTime + 15000;
            _data.EnableUpdateObject();
            _data.FilterObjectId = objectData.BodyID;

            if (objectData.IsStatic)
            {
                _data.ChangeBodyType(BodyType.Static);
            }
            // else 
            // {
            //     ObjectWeldJoint joint = (ObjectWeldJoint)ObjectData.CreateNew(new ObjectDataStartParams(GameWorld.IDCounter.NextID(), 0, 0, "WeldJoint", GameOwner));
            //     GameWorld.CreateTile(new SpawnObjectInformation(joint, Position));
            //     joint.AddObjectToProperty(_data, ObjectPropertyID.JointBodies);
            //     joint.AddObjectToProperty(objectData, ObjectPropertyID.JointBodies);
            //     joint.FinalizeProperties();
            // }
        }
    }
}