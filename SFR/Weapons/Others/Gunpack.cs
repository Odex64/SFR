using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Sync.Generic;

namespace SFR.Weapons.Others;

internal sealed class Gunpack : HItem
{
    internal Gunpack()
    {
        HItemProperties itemProperties = new(106, "Gunpack", "ItemGunpack", false, WeaponCategory.Supply)
        {
            GrabSoundID = "GetHealthSmall"
        };
        HItemVisuals visuals = new(Textures.GetTexture("Pills"));
        itemProperties.VisualText = "Gunpack";
        Properties = itemProperties;
        Visuals = visuals;
    }

    private Gunpack(HItemProperties itemProperties, HItemVisuals itemVisuals) : base(itemProperties, itemVisuals) { }

    public override void OnPickup(Player player, HItem instantPickupItem)
    {
        if (player.GameOwner != GameOwnerEnum.Client)
        {
            SoundHandler.PlaySound(instantPickupItem.Properties.GrabSoundID, player.Position, player.GameWorld);

            var extendedPlayer = player.GetExtension();
            extendedPlayer.JetpackType = JetpackType.Gunpack;
            extendedPlayer.GenericJetpack = new Fighter.Jetpacks.Gunpack();
            if (player.GameOwner == GameOwnerEnum.Server)
            {
                GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, new SyncFlag[] { }, player.ObjectID, extendedPlayer.GetStates()));
            }
        }
    }

    public override bool CheckDoPickup(Player player, HItem instantPickupItem) => true;

    public override HItem Copy() => new Gunpack(Properties, Visuals);
}