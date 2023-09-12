using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SFD;
using SFD.Objects;
using SFD.Weapons;
using SFR.Weapons.Handguns;
using SFR.Weapons.Makeshift;
using SFR.Weapons.Melee;
using SFR.Weapons.Others;
using SFR.Weapons.Rifles;
using SFR.Weapons.Thrown;

namespace SFR.Weapons;

/// <summary>
///     Load all the new weapons.
/// </summary>
[HarmonyPatch]
internal static class Database
{
    private static List<WeaponItem> _weapons;

    /// <summary>
    ///     Fix an issue that prevents new weapons from spawning correctly.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.CreateNew))]
    private static bool FixDrop(ObjectDataStartParams startParams, ref ObjectData __result)
    {
        var weapon = _weapons.Find(w => startParams.MapObjectID == w.BaseProperties.ModelID.ToUpper());
        if (weapon != null)
        {
            __result = new ObjectWeaponItem(startParams, weapon.BaseProperties.WeaponID);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.Load))]
    private static void LoadWeapons()
    {
        WeaponDatabase.m_weapons = new WeaponItem[110];

        _weapons ??= new List<WeaponItem>
        {
            // Makeshift
            new(WeaponItemType.Melee, new Brick()), // 71
            new(WeaponItemType.Melee, new Broom()), // 72
            new(WeaponItemType.Melee, new CannonBall()), // 73

            // Melee
            new(WeaponItemType.Melee, new Blade()), // 74
            new(WeaponItemType.Melee, new Caber()), // 75
            new(WeaponItemType.Melee, new Crowbar()), // 76
            new(WeaponItemType.Melee, new Greatsword()), // 77
            new(WeaponItemType.Melee, new Morningstar()), // 78
            new(WeaponItemType.Melee, new ParryingDagger()), // 79
            new(WeaponItemType.Melee, new Poleaxe()), // 80
            new(WeaponItemType.Melee, new Rapier()), // 81
            // new(WeaponItemType.Melee, new RiotShield()), // 82
            new(WeaponItemType.Melee, new Sledgehammer()), // 83
            new(WeaponItemType.Melee, new Switchblade()), // 84
            new(WeaponItemType.Melee, new Scythe()), // 108

            // Handgun
            new(WeaponItemType.Handgun, new Flintlock()), // 69
            new(WeaponItemType.Handgun, new NailGun()), // 70
            new(WeaponItemType.Handgun, new UnkemptHarold()), // 85
            new(WeaponItemType.Handgun, new StickyLauncher()), // 86
            new(WeaponItemType.Handgun, new Anaconda()), // 109

            // Throwable
            new(WeaponItemType.Thrown, new Claymore()), // 87
            new(WeaponItemType.Thrown, new FragGrenade()), // 88
            new(WeaponItemType.Thrown, new ImpactGrenade()), // 89
            new(WeaponItemType.Thrown, new Snowball()), // 90
            new(WeaponItemType.Thrown, new StickyBomb()), // 91

            // Rifle
            new(WeaponItemType.Rifle, new AA12()), // 93
            new(WeaponItemType.Rifle, new Barrett()), // 94
            new(WeaponItemType.Rifle, new Blunderbuss()), // 95
            new(WeaponItemType.Rifle, new Crossbow()), // 96
            new(WeaponItemType.Rifle, new DoubleBarrel()), // 97
            new(WeaponItemType.Rifle, new Musket()), // 98
            new(WeaponItemType.Rifle, new QuadLauncher()), // 99
            new(WeaponItemType.Rifle, new RCM()), // 100
            new(WeaponItemType.Rifle, new Winchester()), // 101
            new(WeaponItemType.Rifle, new Minigun()), // 102
            new(WeaponItemType.Rifle, new AK47()), // 107

            // Pickup
            new(WeaponItemType.Powerup, new HealthPouch()), // 92
            new(WeaponItemType.Powerup, new AdrenalineBoost()), // 103
            new(WeaponItemType.InstantPickup, new Jetpack()), // 104
            new(WeaponItemType.InstantPickup, new JetpackEditor()), // 105
            new(WeaponItemType.InstantPickup, new Gunpack()) // 106
        };

        foreach (var weapon in _weapons)
        {
            WeaponDatabase.m_weapons[weapon.BaseProperties.WeaponID] = weapon;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WeaponDatabase), nameof(WeaponDatabase.Load))]
    private static IEnumerable<CodeInstruction> Load(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions.ToList();
        codeInstructions.RemoveRange(0, 3);
        return codeInstructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponItem.ID), nameof(WeaponItem.ID.DefaultWeaponSpawnChance))]
    private static bool SpawnChance(ref Dictionary<int, int> __result)
    {
        WeaponItem.ID.m_wpns ??= new Dictionary<int, int>
        {
            { 1, 12 }, // Handgun
            { 2, 14 }, // Shotgun
            { 3, 8 }, // Katana
            { 4, 25 }, // Pipe wrench
            { 5, 8 }, // Tommygun
            { 6, 5 }, // M60
            // 7, Fists
            { 8, 20 }, // Machete
            { 9, 5 }, // Sniper
            { 10, 12 }, // SawedOff shotgun
            { 11, 25 }, // Bat
            { 12, 25 }, // Uzi
            { 13, 35 }, // Pills
            { 14, 20 }, // Medkit
            { 15, 15 }, // Slowmo 5
            { 16, 5 }, // Slowmo 10
            { 17, 5 }, // Bazooka
            { 18, 10 }, // Axe
            { 19, 15 }, // Assault rifle
            { 20, 25 }, // Grenades
            { 21, 20 }, // Lazer
            { 23, 15 }, // Carabine
            { 24, 30 }, // Pistol
            { 25, 25 }, // Molotovs
            { 26, 10 }, // Flamethrower
            { 27, 10 }, // Flaregun
            { 28, 30 }, // Revolver
            { 29, 5 }, // GrenadeLauncher
            { 30, 12 }, // SMG
            { 31, 20 }, // Hammer
            // 32, Chair
            // 33, Chair leg
            // 34, Bottle
            // 35, Broken bottle
            // 36, Cue stick
            // 37, Cue stick shaft
            // 38, Suitcase
            { 39, 15 }, // Silenced pistol
            { 40, 10 }, // Silenced uzi
            { 41, 25 }, // Baton
            { 42, 20 }, // C4
            // 43, C4 detonator
            { 44, 22 }, // Mines
            { 45, 22 }, // Shuriken
            { 46, 25 }, // Chain
            // 47, Pillow
            // 48, Flagpole
            { 49, 20 }, // Knife
            // 50, Teapot
            // 51, Trashcan lid,
            // 52, Trash bag
            { 53, 20 }, // Machine pistol
            { 54, 10 }, // Dark shotgun
            { 55, 12 }, // MP50
            { 56, 25 }, // Lead pipe
            { 57, 8 }, // Shock baton
            // 58, Baseball
            { 59, 4 }, // Chainsaw
            // 60, None
            { 61, 20 }, // Pistol45
            { 62, 10 }, // Strength boost
            { 63, 10 }, // Speed boost
            { 64, 15 }, // Bow
            { 65, 8 }, // Whip
            { 66, 12 }, // Bouncing ammo
            { 67, 8 }, // Fire ammo
            { 68, 13 }, // Drone
            { 69, 13 }, // Flintlock
            { 70, 14 }, // NailGun
            // 71, Brick
            // 72, Broom
            // 73, Cannon ball
            { 74, 13 }, // Blade
            { 75, 10 }, // Caber
            { 76, 18 }, // Crowbar
            { 77, 6 }, // GreatSword
            { 78, 10 }, // Morningstar
            { 79, 8 }, //ParryingDagger
            { 80, 8 }, // Poleaxe
            { 81, 15 }, // Rapier
            // { 82, 16 }, // RiotShield,
            { 83, 16 }, // Sledgehammer
            { 84, 18 }, // Switchblade
            // { 85, 4 }, // UnkemptHarold
            { 86, 9 }, // StickyLauncher
            { 87, 15 }, // Claymore
            { 88, 16 }, // Frag grenade
            { 89, 12 }, // Impact grenade
            // { 90, 1 }, // Snowball
            { 91, 11 }, // Sticky bomb
            { 92, 22 }, // Health pouch
            { 93, 12 }, // AA12
            { 94, 8 }, // Barrett
            { 95, 9 }, // Blunderbuss
            { 96, 15 }, // Crossbow
            { 97, 8 }, // Double barrel
            { 98, 18 }, // Musket
            { 99, 7 }, // Quad launcher
            { 100, 8 }, // RCM
            { 101, 12 }, // Winchester
            { 102, 6 }, // Minigun
            { 103, 12 }, // Adrenaline boost
            { 104, 17 }, // Jetpack
            // 105, Jetpack editor
            { 107, 12 }, // AK47
            { 108, 10 }, // Scythe
            { 109, 12 } // Anaconda
        };

        __result = WeaponItem.ID.m_wpns;
        return false;
    }

    internal enum CustomWeaponItem
    {
        None = -1,
        Pistol = 24,
        Magnum = 1,
        Shotgun,
        Katana,
        Pipe,
        Tommygun,
        M60,
        Machete = 8,
        Sniper,
        SawedOff,
        Bat,
        Uzi,
        Pills,
        Medkit,
        Slowmo5,
        Slowmo10,
        Bazooka,
        Axe,
        Assault,
        Grenades,
        Lazer,
        Carbine = 23,
        Molotovs = 25,
        Flamethrower,
        Flaregun,
        Revolver,
        GrenadeLauncher,
        Smg,
        SubMachinegun = 30,
        Hammer,
        Chair,
        ChairLeg,
        Bottle,
        BrokenBottle,
        Cuestick,
        CuestickShaft,
        Suitcase,
        Silencedpistol,
        Silenceduzi,
        Baton,
        C4,
        C4Detonator,
        Mines,
        Shuriken,
        Chain,
        Pillow,
        Flagpole,
        Knife,
        Teapot,
        TrashcanLid,
        TrashBag,
        MachinePistol,
        DarkShotgun,
        Mp50,
        LeadPipe,
        ShockBaton,
        Baseball,
        Chainsaw,
        Pistol45 = 61,
        Strengthboost,
        Speedboost,
        Bow,
        Whip,
        Bouncingammo,
        Fireammo,
        Streetsweeper,
        Flintlock,
        NailGun,
        Brick,
        Broom,
        CannonBall,
        Blade,
        Caber,
        Crowbar,
        Greatsword,
        Morningstar,
        ParryingDagger,
        Poleaxe,
        Rapier,
        Sledgehammer = 83,
        Switchblade,
        UnkemptHarold,
        StickyLauncher,
        Claymore,
        FragGrenade,
        ImpactGrenade,
        Snowball,
        StickyBomb,
        HealthPouch,
        AA12,
        Barrett,
        Blunderbuss,
        Crossbow,
        DoubleBarrel,
        Musket,
        QuadLauncher,
        RCM,
        Winchester,
        Minigun,
        AdrenalineBoost,
        AK47,
        Scythe,
        Anaconda
        Jetpack,
        JetpackEditor,
        Gunpack
    }
}