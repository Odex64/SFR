using SFD.Projectiles;

namespace SFR.Projectiles;

internal sealed class ProjectileMinigun : Projectile
{
    internal ProjectileMinigun()
    {
        Visuals = new(ProjectileDatabase.BulletCommonTexture, ProjectileDatabase.BulletCommonSlowmoTexture);
        Properties = new(102, 1200f, 10f, 3f, 20f, 0.01f, 5f, 10f, 0.1f)
        {
            PowerupBounceRandomAngle = 0.2f,
            PowerupFireType = ProjectilePowerupFireType.Default
        };
    }

    private ProjectileMinigun(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override Projectile Copy()
    {
        ProjectileMinigun projectile = new(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }
}