using System.Collections.Generic;
using SFD;
using SFD.Objects;
using SFD.Weapons;
using Constants = SFR.Misc.Constants;

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
            var player = GameWorld.GetPlayer(sender.ObjectID);

            if (_type == "BALL")
            {
                var wpn = WeaponDatabase.GetWeapon(93);
                player.GrabWeaponItem(wpn);
            }
            else
            {
                var weapons = new List<WeaponItem>();
                switch (Constants.Random.Next(4))
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

                foreach (var wep in weapons)
                {
                    player.GrabWeaponItem(wep);
                }
            }
        }
    }
}