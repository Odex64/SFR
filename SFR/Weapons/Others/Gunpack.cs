using SFD;
using SFD.Sounds;
using SFD.Tiles;
using SFD.Weapons;
using SFR.Fighter.Jetpacks;
using SFR.Helper;
using SFR.Sync.Generic;
using Player = SFD.Player;

namespace SFR.Weapons.Others;

internal sealed class Gunpack : HItem
{
    internal Gunpack()
    {
        HItemProperties itemProperties = new(106, "Gunpack", "ItemGunpack", false, WeaponCategory.Supply)
        {
            GrabSoundID = "GetHealthSmall",
            VisualText = "Gunpack"
        };
        HItemVisuals visuals = new(Textures.GetTexture("Pills"));

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
                GenericData.SendGenericDataToClients(new(DataType.ExtraClientStates, [], player.ObjectID, extendedPlayer.GetStates()));
            }
        }
    }

    public override bool CheckDoPickup(Player player, HItem instantPickupItem) => true;

    public override HItem Copy() => new Gunpack(Properties, Visuals);
}