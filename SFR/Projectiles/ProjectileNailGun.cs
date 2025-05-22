using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Objects;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Tiles;

namespace SFR.Projectiles;

internal sealed class ProjectileNailGun : Projectile, IExtendedProjectile
{
    private float _gravity;
    private float _velocity;

    internal ProjectileNailGun()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("ProjectileNailgun"), Textures.GetTexture("ProjectileNailgun"));
        Properties = new ProjectileProperties(70, 400f, 10f, 6f, 6f, 0.01f, 8f, 10f, 0.35f)
        {
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Default,
            PowerupTotalBounces = 4,
            PowerupFireIgniteValue = 4f
        };
    }

    private ProjectileNailGun(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals)
    {
    }

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
        ProjectileNailGun projectile = new(Properties, Visuals);
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

                EffectHandler.PlayEffect("TR_F", Position, GameWorld);
                _gravity = Constants.EFFECT_LEVEL_FULL ? 10f : 20f;
            }
        }
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        if (GameOwner != GameOwnerEnum.Client)
        {
            player.TakeProjectileDamage(this);
            Material material = player.GetPlayerHitMaterial() ?? playerObjectData.Tile.Material;
            SoundHandler.PlaySound(material.Hit.Projectile.HitSound, GameWorld);
            EffectHandler.PlayEffect(material.Hit.Projectile.HitEffect, Position, GameWorld);
            SoundHandler.PlaySound("MeleeHitSharp", GameWorld);
        }
    }
}