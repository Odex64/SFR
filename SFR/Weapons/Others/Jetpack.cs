using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Sync.Generic;
using Player = SFD.Player;

namespace SFR.Weapons.Others;

internal sealed class Jetpack : HItem
{
    internal Jetpack()
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

    private Jetpack(HItemProperties itemProperties, HItemVisuals itemVisuals) : base(itemProperties, itemVisuals) { }

    public override void OnPickup(Player player, HItem instantPickupItem)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            SoundHandler.PlaySound(instantPickupItem.Properties.GrabSoundID, player.Position, player.GameWorld);

            var extendedPlayer = player.GetExtension();
            extendedPlayer.JetpackType = JetpackType.Jetpack;
            extendedPlayer.GenericJetpack = new Fighter.Jetpacks.Jetpack();
            if (player.GameOwner == GameOwnerEnum.Server)
            {
                GenericData.SendGenericDataToClients(new(DataType.ExtraClientStates, [], player.ObjectID, extendedPlayer.GetStates()));
            }
        }
    }

    public override bool CheckDoPickup(Player player, HItem instantPickupItem) => true;

    public override HItem Copy() => new Jetpack(Properties, Visuals);
}