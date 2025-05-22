﻿using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Fighter;
using SFR.Helper;
using Player = SFD.Player;

namespace SFR.Weapons.Others;

internal class AdrenalineBoost : PItem
{
    internal AdrenalineBoost()
    {
        PItemProperties itemProperties = new(103, "AdrenalineBoost", "ItemAdrenalineBoost", false, WeaponCategory.Supply)
        {
            PickupSoundID = "GetSlomo",
            ActivateSoundID = "",
            VisualText = "Adrenaline Boost"
        };

        PItemVisuals itemVisuals = new(Textures.GetTexture("AdrenalineBoost"), Textures.GetTexture("AdrenalineBoostD"));

        SetPropertiesAndVisuals(itemProperties, itemVisuals);
    }

    private AdrenalineBoost(PItemProperties properties, PItemVisuals visuals) => SetPropertiesAndVisuals(properties, visuals);

    public override void OnActivation(Player player, PItem powerupItem)
    {
        if (player.StrengthBoostPrepare())
        {
            SoundHandler.PlaySound(powerupItem.Properties.ActivateSoundID, player.Position, player.GameWorld);
        }
    }

    internal void OnEffectStart(Player player)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            SoundHandler.PlaySound("Syringe", player.Position, player.GameWorld);
            SoundHandler.PlaySound("StrengthBoostStart", player.Position, player.GameWorld);

            ExtendedPlayer extendedPlayer = player.GetExtension();
            extendedPlayer.ApplyAdrenalineBoost();
            if (!player.InfiniteAmmo)
            {
                player.RemovePowerup();
            }
        }
    }

    public override PItem Copy() => new AdrenalineBoost(Properties, Visuals);
}