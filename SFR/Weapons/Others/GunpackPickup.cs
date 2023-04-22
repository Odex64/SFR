using SFD;
using SFD.Effects;
using SFD.Tiles;
using SFD.Weapons;

namespace SFR.Weapons.Others;

internal sealed class GunpackPickup : HItem
{
    internal GunpackPickup()
    {
        HItemProperties itemProperties = new(89, "Minigunpack", "ItemGunpack", false, WeaponCategory.Supply)
        {
            GrabSoundID = "GetHealthSmall"
        };
        HItemVisuals visuals = new(Textures.GetTexture("Pills"));
        itemProperties.VisualText = "Minigunpack";
        Properties = itemProperties;
        Visuals = visuals;
    }

    private GunpackPickup(HItemProperties itemProperties, HItemVisuals itemVisuals) : base(itemProperties, itemVisuals) { }

    public override void OnPickup(Player player, HItem instantPickupItem)
    {
        /*
        SoundHandler.PlaySound(instantPickupItem.Properties.GrabSoundID, player.Position, player.GameWorld);
        Logger.LogDebug("Adding gunpack: " + player.GameOwner);
        new GadgetHandler.JetpackPlayer(player, true, 5000, GadgetHandler.JetpackType.Minigun, true);
        */
        EffectHandler.PlayEffect("CFTXT", player.Position, player.GameWorld, "Not implemented yet");
    }

    public override bool CheckDoPickup(Player player, HItem instantPickupItem) => true;

    public override HItem Copy() => new GunpackPickup(Properties, Visuals);
}