using HarmonyLib;
using SFD;
using SFD.Objects;
using SFD.Projectiles;

namespace SFR.Projectiles;

/// <summary>
///     Since some projectiles need special interaction, we need to override and call special methods upon specific actions.
///     However all projectiles already inherit from a class, therefore we use an interface and through polymorphism
///     we call overridden methods from hooked harmony patches. The following interface defines all the extra methods a projectile can override.
/// </summary>
internal interface IExtendedProjectile
{
    abstract bool OnHit(Projectile projectile, ProjectileHitEventArgs e, ObjectData objectData);
    abstract bool OnExplosiveHit(Projectile projectile, ProjectileHitEventArgs e, ObjectExplosive objectData);
    abstract bool OnExplosiveBarrelHit(Projectile projectile, ProjectileHitEventArgs e, ObjectBarrelExplosive objectData);
}

[HarmonyPatch]
internal static class ExtendedProjectile
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.ProjectileHit))]
    private static bool OnProjectileHit(Projectile projectile, ProjectileHitEventArgs e, ObjectData __instance) => projectile is not IExtendedProjectile proj || proj.OnHit(projectile, e, __instance);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectExplosive), nameof(ObjectExplosive.ProjectileHit))]
    private static bool OnProjectileExplosiveHit(Projectile projectile, ProjectileHitEventArgs e, ObjectExplosive __instance) => projectile is not IExtendedProjectile proj || proj.OnExplosiveHit(projectile, e, __instance);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectBarrelExplosive), nameof(ObjectBarrelExplosive.ProjectileHit))]
    private static bool OnProjectileExplosiveHit(Projectile projectile, ProjectileHitEventArgs e, ObjectBarrelExplosive __instance) => projectile is not IExtendedProjectile proj || proj.OnExplosiveBarrelHit(projectile, e, __instance);
}