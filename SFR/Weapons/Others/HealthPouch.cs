using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;

namespace SFR.Weapons.Others;

internal sealed class HealthPouch : PItem
{
    internal HealthPouch()
    {
        PItemProperties itemProperties = new(92, "Health_Pouch", "ItemHealthPouch", false, WeaponCategory.Supply)
        {
            PickupSoundID = "GetSlomo",
            ActivateSoundID = "GetHealthSmall",
            VisualText = "Health Pouch"
        };

        PItemVisuals visuals = new(Textures.GetTexture("HealthPouch"));

        SetPropertiesAndVisuals(itemProperties, visuals);
    }

    private HealthPouch(PItemProperties itemProperties, PItemVisuals itemVisuals) => SetPropertiesAndVisuals(itemProperties, itemVisuals);

    public override void OnActivation(Player player, PItem powerupItem)
    {
        SoundHandler.PlaySound(powerupItem.Properties.ActivateSoundID, player.Position, player.GameWorld);
        player.HealAmount(25f);
        EffectHandler.PlayEffect("PWT", player.Position, player.GameWorld, "13");
        player.RemovePowerup();
    }

    public override PItem Copy() => new HealthPouch(Properties, Visuals);
}