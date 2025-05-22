using System;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Materials;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Tiles;

namespace SFR.Projectiles;

internal sealed class ProjectileMusket : Projectile
{
    private float _gravity;
    private float _velocity;

    internal ProjectileMusket()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("ProjectileFlintlock"), Textures.GetTexture("ProjectileFlintlock"));
        Properties = new ProjectileProperties(98, 900f, 100f, 40f, 50f, 0.5f, 40f, 45f, 0.5f)
        {
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Fireplosion,
            PowerupTotalBounces = 16,
            PowerupFireIgniteValue = 30f
        };
    }

    private ProjectileMusket(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals)
    {
    }

    public override float SlowmotionFactor => 1f - (1f - GameWorld.SlowmotionHandler.SlowmotionModifier) * 0.5f;

    public override Projectile Copy()
    {
        ProjectileMusket projectile = new(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }

    public override void Update(float ms)
    {
        _velocity += ms;
        float scaleFactor = Math.Min(_velocity / 500f, 1f);
        Velocity -= Vector2.UnitY * ms * 0.66f * scaleFactor;
        if (GameOwner != GameOwnerEnum.Server)
        {
            _gravity -= ms;
            if (_gravity <= 0f)
            {
                if (Constants.EFFECT_LEVEL_FULL)
                {
                    EffectHandler.PlayEffect("CSW", Position, GameWorld); //CSW is ok
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
            Material material = player.GetPlayerHitMaterial() ?? playerObjectData.Tile.Material;
            SoundHandler.PlaySound(material.Hit.Projectile.HitSound, GameWorld);
            EffectHandler.PlayEffect(material.Hit.Projectile.HitEffect, Position, GameWorld);
            SoundHandler.PlaySound("MeleeHitSharp", GameWorld);
        }
    }
}