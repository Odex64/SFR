using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Sync.Generic;

namespace SFR.Weapons.Others;

internal sealed class JetpackPickup : HItem
{
    internal JetpackPickup()
    {
        HItemProperties itemProperties = new(104, "Jetpack", "ItemJetpack", false, WeaponCategory.Supply)
        {
            GrabSoundID = "GetHealthSmall"
        };
        HItemVisuals visuals = new(Textures.GetTexture("Pills"));
        itemProperties.VisualText = "Jetpack";
        Properties = itemProperties;
        Visuals = visuals;
    }

    private JetpackPickup(HItemProperties itemProperties, HItemVisuals itemVisuals) : base(itemProperties, itemVisuals) { }

    public override void OnPickup(Player player, HItem instantPickupItem)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            SoundHandler.PlaySound(instantPickupItem.Properties.GrabSoundID, player.Position, player.GameWorld);

            var extendedPlayer = player.GetExtension();
            extendedPlayer.JetpackType = JetpackType.Jetpack;
            extendedPlayer.PrepareJetpack = true;
            if (player.GameOwner == GameOwnerEnum.Local) // Offline or map editor
            {
                extendedPlayer.GenericJetpack = new Jetpack();
            }
            else
            {
                GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, player.ObjectID, extendedPlayer.GetStates()));
            }
        }
        // if (ExtendedPlayer.GetExtendedPlayer(player, typeof(Jetpack), player) is GenericJetpack jetPlayer)
        // {
        //     jetPlayer.Active = true;
        // }
    }

    public override bool CheckDoPickup(Player player, HItem instantPickupItem) => true;

    public override HItem Copy() => new JetpackPickup(Properties, Visuals);
}