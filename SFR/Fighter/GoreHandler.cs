using System;
using HarmonyLib;
using SFD;
using SFD.Effects;
using SFD.Objects;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.Weapons;
using SFDGameScriptInterface;
using SFR.Helper;
using SFR.Objects;
using SFR.Sync.Generic;
using SFR.Weapons;
using Constants = SFR.Misc.Constants;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using WeaponItemType = SFD.Weapons.WeaponItemType;

namespace SFR.Fighter;

[HarmonyPatch]
internal static class GoreHandler
{
    private const float HeadThresholdStanding = 5f;
    private const float HeadThresholdCrouching = 5f;
    private const float HeadThresholdLaying = 3f;
    private const int MaxDamageChance = 40;

    /// <summary>
    ///     Spawn more giblets on player dead.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Gib))]
    private static void OnGib(Player __instance)
    {
        if (!__instance.IsRemoved && !__instance.m_removalRunning && __instance.PlayerHitEffect == PlayerHitEffect.Default)
        {
            string[] giblets = { "Organ00", "Organ01", "Organ02", "Organ03", "Organ04", "Organ05" };
            foreach (string giblet in giblets)
            {
                var value = Converter.ConvertBox2DToWorld(__instance.WorldBody.GetPosition());
                SpawnObjectInformation spawnObjectInformation = new(__instance.GameWorld.IDCounter.NextObjectData(giblet), value, 1f, 1, new Vector2(Constants.Random.NextFloat(-4.5f, 4.5f), Constants.Random.NextFloat(4f, 4f)), Constants.Random.NextFloat(-0.3f, 0.3f))
                {
                    FireBurning = __instance.ObjectData.Fire.IsBurning,
                    FireSmoking = __instance.ObjectData.Fire.IsSmoking
                };
                __instance.GameWorld.CreateTile(spawnObjectInformation);
            }
        }
    }

    /// <summary>
    ///     Check the damage and where the player has been hit.
    ///     Decapitate it accordingly.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.RunScriptOnProjectileHitCallbacks))]
    private static void ProjectileHit(Projectile projectile, ObjectData hitObject, Vector2 deflectionNormal, GameWorld __instance)
    {
        if (hitObject.IsPlayer)
        {
            var player = __instance.GetPlayer(hitObject.BodyID);
            var profile = player.GetProfile().ToSFDProfile();
            if (profile.Skin.Name.Contains("Headless") || (!profile.Skin.Name.Contains("Zombie") && !player.IsDead))
            {
                return;
            }

            bool headShot = false;
            float finalDamage = projectile.HitDamageValue * player.GetModifiers().ProjectileDamageTakenModifier;

            // Shotguns have higher chance
            int[] shotgunRounds = { 2, 10, 54 };
            foreach (int i in shotgunRounds)
            {
                if (i == projectile.Properties.ProjectileID)
                {
                    finalDamage *= 6;
                }
            }

            // Alive zombies can lose head, but much more rare
            if (player.GetProfile().ToSFDProfile().Skin.Name.Contains("Zombie") && !player.IsDead)
            {
                finalDamage /= 4;
            }

            if (Constants.Random.Next(MaxDamageChance) > finalDamage)
            {
                return;
            }

            var aabb = hitObject.GetWorldAABB();
            var position = projectile.Position;
            int facingDirection = player.LastDirectionX * (player.GetAnimation().ToString() == "LayOnGroundB" ? -1 : 1);

            if ((player.Crouching || player.FullLanding) && position.Y > aabb.GetCenter().Y + HeadThresholdCrouching)
            {
                headShot = true;
            }
            else if ((player.Diving && facingDirection == 1 && aabb.GetCenter().X < position.X) || (facingDirection == -1 && aabb.GetCenter().X > position.X))
            {
                headShot = true;
            }
            else if ((player.LayingOnGround && facingDirection == 1 && aabb.GetCenter().X + HeadThresholdLaying < position.X) || (facingDirection == -1 && aabb.GetCenter().X - HeadThresholdLaying > position.X))
            {
                headShot = true;
            }
            else if (position.Y > aabb.GetCenter().Y + HeadThresholdStanding)
            {
                headShot = true;
            }

            if (headShot)
            {
                ApplyHeadshot(player, position);
            }
        }
    }

    /// <summary>
    ///     Some melee weapons may affect gore.
    /// </summary>
    internal static void MeleeHit(Player source, Player target)
    {
        var weapon = source.CurrentMeleeWeapon;
        if (source.CurrentWeaponDrawn == WeaponItemType.Melee && source.CurrentMeleeMakeshiftWeapon == null)
        {
            if (weapon is WpnKatana or WpnMachete or WpnChainsaw or WpnAxe or ISharpMelee && !target.GetProfile().ToSFDProfile().Skin.Name.Contains("Headless"))
            {
                if (target.IsDead && (source.Position.Y >= target.Position.Y || target.DeathKneeling || target.Crouching || target.LayingOnGround))
                {
                    if (weapon is WpnChainsaw && source.Position.Y < target.Position.Y + 8 && target.LayingOnGround && source.Crouching)
                    {
                        return;
                    }

                    float decapitationChance = 0.34f;
                    if (weapon is ISharpMelee melee)
                    {
                        decapitationChance = melee.GetDecapitationChance();
                    }

                    if (Constants.Random.NextFloat() < decapitationChance)
                    {
                        ApplyHeadshot(target, target.Position + new Vector2(0, 18), "Melee_Sharp");
                    }
                }
            }
        }

        if (source.GrabAttacking && target.IsDead && Constants.Random.Next(5) == 0)
        {
            ApplyHeadshot(target, target.Position + new Vector2(0, 18), "Bazinga");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Equipment), nameof(Equipment.EnsureHurtLevelEquipped))]
    private static bool ClearHurtLevel(int hurtLevel, Equipment __instance)
    {
        if (__instance.m_equippedItems[0] != null && __instance.m_equippedItems[0].Filename.Contains("Headless"))
        {
            if (__instance.m_equippedItems[9] != null)
            {
                __instance.Unequip(9);
            }

            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CheckBurnedProfile))]
    internal static bool CheckBurned(Player __instance)
    {
        if (__instance.Burned)
        {
            var item = Items.GetItem(__instance.Gender == Player.GenderType.Male ? "Burnt" : "Burnt_fem");
            if (__instance.GetProfile().ToSFDProfile().Skin.Name.Contains("Burnt"))
            {
                return false;
            }

            if (__instance.GetProfile().ToSFDProfile().Skin.Name.Contains("Headless"))
            {
                item = Items.GetItem(GetCorrespondingSkin(item.Filename));
            }

            if (item != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    __instance.Equipment.Unequip(i);
                }

                __instance.Equipment.Equip(item);
            }
        }

        return false;
    }

    private static void ApplyHeadshot(Player player, Vector2 position, string headshotType = "Normal")
    {
        // Check for special skins
        if (player.GetProfile().ToSFDProfile().Skin.Name.Contains("Headless") || GetCorrespondingSkin(player.GetProfile().ToSFDProfile().Skin.Name) == null)
        {
            return;
        }

        // Spawn gibs
        if (headshotType == "Bazinga")
        {
            player.GameWorld.SpawnDebris(player.ObjectData, position, 4, new[] { "HeadDebris00A", "HeadDebris00B", "HeadDebris00C" });
            var spine = ObjectData.CreateNew(new ObjectDataStartParams(player.GameWorld.IDCounter.NextID(), 100, 0, "Giblet05", player.GameWorld.GameOwner));
            player.GameWorld.CreateTile(new SpawnObjectInformation(spine, player.Position + new Vector2(0, 13), 0, (short)player.LastDirectionX, new Vector2(Constants.Random.NextFloat(-0.5f, 0.5f), Constants.Random.NextFloat(6, 10)), Constants.Random.NextFloat(-1f, 1f)));

            var skull = ObjectData.CreateNew(new ObjectDataStartParams(player.GameWorld.IDCounter.NextID(), 100, 0, "Giblet04", player.GameWorld.GameOwner));
            player.GameWorld.CreateTile(new SpawnObjectInformation(skull, player.Position + new Vector2(2, 18), 0, (short)player.LastDirectionX, spine.GetLinearVelocity(), Constants.Random.NextFloat(-1f, 1f)));

            var revJoint = (ObjectRevoluteJoint)ObjectData.CreateNew(new ObjectDataStartParams(player.GameWorld.IDCounter.NextID(), 100, 0, "RevoluteJoint", player.GameWorld.GameOwner));
            player.GameWorld.CreateTile(new SpawnObjectInformation(revJoint, player.Position + new Vector2(0, 16)));
            revJoint.AddObjectToProperty(spine, ObjectPropertyID.JointBodyA);
            revJoint.AddObjectToProperty(skull, ObjectPropertyID.JointBodyB);
            revJoint.FinalizeProperties();
        }

        if (headshotType == "Normal")
        {
            player.GameWorld.SpawnDebris(player.ObjectData, position, 4, new[] { "HeadDebris00A", "HeadDebris00B", "HeadDebris00C" });
        }

        if (headshotType == "Melee_Sharp")
        {
            // var head = (ObjectHead)ObjectData.CreateNew(new ObjectDataStartParams(player.GameWorld.IDCounter.NextID(), 100, 0, "Head00", player.GameWorld.GameOwner));
            var head = (ObjectHead)player.GameWorld.CreateObjectData("Head00");
            player.GameWorld.CreateTile(new SpawnObjectInformation(head, player.Position + new Vector2(0, 16), 0, (short)player.LastDirectionX, new Vector2(Constants.Random.NextFloat(-0.5f, 0.5f), Constants.Random.NextFloat(6, 10)), Constants.Random.NextFloat(-6f, 6f)));
            switch (player.GameOwner)
            {
                case GameOwnerEnum.Server:
                    GenericData.SendGenericDataToClients(new GenericData(DataType.Head, new[] { SyncFlag.MustSyncNewObjects }, head.ObjectID, ObjectHead.EquipmentToString(player.Equipment)));
                    // head.SyncedMethod(new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame, player.GameWorld.ElapsedTotalGameTime, ObjectHead.EquipmentToString(player.Equipment)));
                    break;
                case GameOwnerEnum.Local:
                    head.ReplaceTexture = ObjectHead.TextureFromString(ObjectHead.EquipmentToString(player.Equipment));
                    break;
            }
        }

        // Apply profile
        var profile = player.GetProfile().ToSFDProfile();
        profile.Head = GetCorrespondingHat(profile.Skin, headshotType);
        profile.Accesory = null;
        profile.Skin = new IProfileClothingItem(GetCorrespondingSkin(profile.Skin.Name), profile.Skin.Color1, profile.Skin.Color2, profile.Skin.Color3);
        player.ApplyScriptProfile(profile);
        player.MetaDataUpdated = true;
        // Items.SetGoreScriptable(false);

        // Play effects
        EffectHandler.PlayEffect("BLD", position, player.GameWorld);
        EffectHandler.PlayEffect("BLD", position, player.GameWorld);
        EffectHandler.PlayEffect("Smack", position, player.GameWorld);
        SoundHandler.PlaySound("ImpactFlesh", position, player.GameWorld);
    }

    private static string GetCorrespondingSkin(string skin)
    {
        return skin switch
        {
            "Normal" => "NormalHeadless",
            "Normal_fem" => "NormalHeadless_fem",
            "Zombie" => "ZombieHeadless",
            "Zombie_fem" => "ZombieHeadless_fem",
            "Tattoos" => "TattoosHeadless",
            "Tattoos_fem" => "TattoosHeadless_fem",
            "Burnt" => "BurntHeadless",
            "Burnt_fem" => "BurntHeadless_fem",
            "Warpaint" => "WarpaintHeadless",
            "Warpaint_fem" => "WarpaintHeadless_fem",
            "NormalHeadless" => "Normal",
            "NormalFemHeadless" => "Normal_fem",
            "ZombieHeadless" => "Zombie",
            "ZombieFemHeadless" => "Zombie_fem",
            "TattoosHeadless" => "Tattoos",
            "TattoosFemHeadless" => "Tattoos_fem",
            "BurntHeadless" => "Burnt",
            "BurntFemHeadless" => "Burnt_fem",
            "WarpaintHeadless" => "Warpaint",
            "WarpaintFemHeadless" => "Warpaint_fem",
            _ => null
        };
    }

    private static IProfileClothingItem GetCorrespondingHat(IProfileClothingItem skin, string headshotType = "")
    {
        bool isZombie = skin.Name.Contains("Zombie");
        string color1 = isZombie ? "SkinZombie" : skin.Color1;
        string color2 = isZombie || skin.Name.Contains("Burnt") ? "Meat2" : "Meat1";
        const string color3 = "Bone1";
        string hat;

        if (isZombie)
        {
            string[] zHats = { "Headless", "ExposedBrain", "HeadShot", "HeadShot2", "HeadShot3" };
            hat = zHats[new Random().Next(zHats.Length)];
            if (hat.Contains("Brain"))
            {
                color2 = "Brain";
            }
        }
        else
        {
            string[] nHats = { "Headless", "HeadShot", "HeadShot2" };
            hat = nHats[new Random().Next(nHats.Length)];
        }

        return headshotType switch
        {
            "Bazinga" => new IProfileClothingItem("ChestCavity", color1, color2, color3),
            "Melee_Sharp" => new IProfileClothingItem("Headless", color1, color2, color3),
            _ => hat.Contains("HeadShot") ? new IProfileClothingItem(hat, color1, color3, color2) : new IProfileClothingItem(hat, color1, color2, color3)
        };
    }
}