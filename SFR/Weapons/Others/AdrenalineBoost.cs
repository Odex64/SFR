using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Helper;
using Player = SFD.Player;

namespace SFR.Weapons.Others;

internal class AdrenalineBoost : PItem
{
    internal AdrenalineBoost()
    {
        var itemProperties = new PItemProperties(103, "AdrenalineBoost", "ItemAdrenalineBoost", false, WeaponCategory.Supply)
        {
            PickupSoundID = "GetSlomo",
            ActivateSoundID = ""
        };

        var itemVisuals = new PItemVisuals(Textures.GetTexture("AdrenalineBoost"), Textures.GetTexture("AdrenalineBoostD"));
        itemProperties.VisualText = "Adrenaline Boost";

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
            // player.StrengthBoostApply(15000f);
            var extendedPlayer = player.GetExtension();
            extendedPlayer.ApplyAdrenalineBoost();
            if (!player.InfiniteAmmo)
            {
                player.RemovePowerup();
            }
        }
    }

    public override PItem Copy() => new AdrenalineBoost(Properties, Visuals);
}