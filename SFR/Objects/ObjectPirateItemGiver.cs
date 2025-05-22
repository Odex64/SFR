using System.Collections.Generic;
using SFD;
using SFD.Objects;
using SFD.Weapons;
using SFR.Misc;

namespace SFR.Objects;

internal class ObjectPirateItemGiver : ObjectButtonTrigger
{
    private readonly string _type;

    internal ObjectPirateItemGiver(string str, ObjectDataStartParams startParams) : base(startParams) => _type = str;

    public override void Activate(ObjectData sender)
    {
        base.Activate(sender);
        if (sender is { IsPlayer: true })
        {
            Player player = GameWorld.GetPlayer(sender.ObjectID);

            if (_type == "BALL")
            {
                WeaponItem wpn = WeaponDatabase.GetWeapon(93);
                _ = player.GrabWeaponItem(wpn);
            }
            else
            {
                List<WeaponItem> weapons = new();
                switch (Globals.Random.Next(4))
                {
                    case 0:
                        weapons.Add(WeaponDatabase.GetWeapon(94));
                        weapons.Add(WeaponDatabase.GetWeapon(49));
                        break;
                    case 1:
                        weapons.Add(WeaponDatabase.GetWeapon(92));
                        weapons.Add(WeaponDatabase.GetWeapon(8));
                        break;
                    case 2:
                        weapons.Add(WeaponDatabase.GetWeapon(70));
                        weapons.Add(WeaponDatabase.GetWeapon(73));
                        break;
                    case 3:
                        weapons.Add(WeaponDatabase.GetWeapon(72));
                        weapons.Add(WeaponDatabase.GetWeapon(73));
                        break;
                }

                foreach (WeaponItem wep in weapons)
                {
                    _ = player.GrabWeaponItem(wep);
                }
            }
        }
    }
}