using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Helper;

namespace SFR.Weapons.Others;

internal class RageBoost : PItem
{
    internal RageBoost()
    {
        var itemProperties = new PItemProperties(103, "RageBoost", "ItemRageBoost", false, WeaponCategory.Supply)
        {
            PickupSoundID = "GetSlomo",
            ActivateSoundID = ""
        };

        var itemVisuals = new PItemVisuals(Textures.GetTexture("RageBoost"), Textures.GetTexture("RageBoostD"));
        itemProperties.VisualText = "Rage Boost";

        SetPropertiesAndVisuals(itemProperties, itemVisuals);
    }

    private RageBoost(PItemProperties properties, PItemVisuals visuals)
    {
        SetPropertiesAndVisuals(properties, visuals);
    }

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
            // player.StrengthBoostApply(15000f);
            var extendedPlayer = player.GetExtension();
            extendedPlayer.ApplyRageBoost();
            if (!player.InfiniteAmmo)
            {
                player.RemovePowerup();
            }
        }
    }

    public override PItem Copy() => new RageBoost(Properties, Visuals);
}