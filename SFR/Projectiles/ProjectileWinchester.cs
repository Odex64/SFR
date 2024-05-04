using SFD.Projectiles;

namespace SFR.Projectiles;

internal sealed class ProjectileWinchester : Projectile
{
    internal ProjectileWinchester()
    {
        Visuals = new(ProjectileDatabase.BulletCommonTexture, ProjectileDatabase.BulletCommonSlowmoTexture);
        Properties = new(101, 1300f, 50f, 22f, 22f, 0.33f, 32f, 12f, 0.55f)
        {
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Default,
            PowerupFireIgniteValue = 50f
        };
    }

    private ProjectileWinchester(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals) { }

    public override Projectile Copy()
    {
        ProjectileWinchester projectile = new(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }
}