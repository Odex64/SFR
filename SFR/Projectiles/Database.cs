using System;
using HarmonyLib;
using SFD.Projectiles;

namespace SFR.Projectiles;

/// <summary>
/// Load all the new projectiles.
/// </summary>
[HarmonyPatch]
internal static class Database
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProjectileDatabase), nameof(ProjectileDatabase.Load))]
    private static void LoadProjectiles()
    {
        Array.Resize(ref ProjectileDatabase.projectiles, 103);

        ProjectileDatabase.projectiles[69] = new ProjectileFlintlock();
        ProjectileDatabase.projectiles[70] = new ProjectileNailGun();
        ProjectileDatabase.projectiles[85] = new ProjectileUnkemptHarold();
        ProjectileDatabase.projectiles[94] = new ProjectileBarrett();
        ProjectileDatabase.projectiles[95] = new ProjectileBlunderbuss();
        ProjectileDatabase.projectiles[96] = new ProjectileCrossbow();
        ProjectileDatabase.projectiles[97] = new ProjectileDoubleBarrel();
        ProjectileDatabase.projectiles[98] = new ProjectileMusket();
        ProjectileDatabase.projectiles[99] = new ProjectileQuad();
        ProjectileDatabase.projectiles[100] = new ProjectileRCM();
        ProjectileDatabase.projectiles[101] = new ProjectileWinchester();
        ProjectileDatabase.projectiles[102] = new ProjectileMinigun();
    }
}