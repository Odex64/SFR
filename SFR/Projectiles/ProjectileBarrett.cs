using System.Collections.Generic;
using SFD;
using SFD.Projectiles;
using SFD.Tiles;
using SFR.Misc;

namespace SFR.Projectiles;

internal sealed class ProjectileBarrett : Projectile
{
    private readonly List<int> _ignoreIds = [];

    internal ProjectileBarrett()
    {
        Visuals = new ProjectileVisuals(Textures.GetTexture("BulletBarrett"), Textures.GetTexture("BulletBarrettSlowmo"));
        Properties = new ProjectileProperties(94, 1500f, 2000f, 70f, 2000f, 1f, 70f, 120f, 0.0f)
        {
            PowerupBounceRandomAngle = 0f,
            PowerupFireType = ProjectilePowerupFireType.Default,
            PowerupFireIgniteValue = 100f,
            PowerupTotalBounces = 25,
            CanBeAbsorbedOrBlocked = false,
            DodgeChance = 0
        };
    }

    private ProjectileBarrett(ProjectileProperties projectileProperties, ProjectileVisuals projectileVisuals) : base(projectileProperties, projectileVisuals)
    {
    }

    public override Projectile Copy()
    {
        ProjectileBarrett projectile = new(Properties, Visuals);
        projectile.CopyBaseValuesFrom(this);
        return projectile;
    }

    public override void HitPlayer(Player player, ObjectData playerObjectData)
    {
        if (!_ignoreIds.Contains(player.ObjectID) || PowerupBounceActive)
        {
            base.HitPlayer(player, playerObjectData);
            if (player.GameOwner != GameOwnerEnum.Client)
            {
                if (player is { IsRemoved: false, IsDead: true })
                {
                    if (Globals.Random.Next(3) == 1)
                    {
                        player.Gib();
                    }
                }

                _ignoreIds.Add(player.ObjectID);
            }
        }

        HitFlag = false;
    }

    public override void HitObject(ObjectData objectData, ProjectileHitEventArgs e)
    {
        base.HitObject(objectData, e);
        if (objectData.Destructable)
        {
            HitFlag = false;
            e.CustomHandled = true;
            e.ReflectionStatus = ProjectileReflectionStatus.None;
        }
    }
}