using SFD.Projectiles;
using SFD.Tiles;

namespace SFR.Projectiles;

internal sealed class ProjectileDoubleBarrel : Projectile
{
    internal ProjectileDoubleBarrel()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("BulletBarrett"), Textures.GetTexture("BulletBarrettSlowmo"));
        Properties = new ProjectileProperties(97, 1200f, 10f, 3.7f, 8f, 0f, 3.7f, 9f, 0.2f)
        {
            PowerupFireIgniteValue = 5.94f
        };
    }

    private ProjectileDoubleBarrel(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals)
    {
    }

    public override Projectile Copy()
    {
        Projectile projectile = new ProjectileDoubleBarrel(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }
}