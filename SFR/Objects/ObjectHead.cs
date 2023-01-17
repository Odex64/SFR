using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Tiles;
using SFR.Helper;
using SFR.Sync.Generic;

namespace SFR.Objects;

internal sealed class ObjectHead : ObjectGiblet
{
    private static readonly Texture2D BackupTexture = null;
    private static readonly string[] DisallowedAccessories = { "swing", "armband", "dogtag", "scarf" };
    internal Texture2D ReplaceTexture;

    internal ObjectHead(ObjectDataStartParams startParams) : base(startParams) { }

    public override void SetProperties()
    {
        Properties.Add(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);
    }

    public override void Initialize()
    {
        base.Initialize();
        if (Properties.Exists(290) && GameOwner != GameOwnerEnum.Client)
        {
            if (Properties.Get(290).Value != null)
            {
                var targetPlayerProfileInfoObjects = GetObjectsFromProperty<ObjectPlayerProfileInfo>(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);
                Logger.LogDebug(targetPlayerProfileInfoObjects.Count);
                if (targetPlayerProfileInfoObjects.Count > 0)
                {
                    var profile = targetPlayerProfileInfoObjects[0].GetProfile();
                    Equipment equipment = new();
                    equipment.Equip(profile.EquippedItems[0]);
                    equipment.Equip(profile.EquippedItems[6]);
                    equipment.Equip(profile.EquippedItems[8]);
                    GenericData.SendGenericDataToClients(new GenericData(DataType.Head, ObjectID, EquipmentToString(equipment)));
                    // SyncedMethod(new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame, GameWorld.ElapsedTotalGameTime, EquipmentToString(equipment)));
                }
            }
        }

        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
    }

    public override void Dispose()
    {
        GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
        base.Dispose();
    }

    private static Texture2D GetTextureFromEquipment(Equipment equipment)
    {
        var skin = equipment.m_parts[0][0]?.GetTexture(0);
        if (skin != null)
        {
            var task = Task.Run(() =>
                equipment.m_parts[0][0]?.GetTexture(0, equipment.GetItemColors(0))
            );
            if (task.Wait(5))
            {
                skin = task.Result;
            }
            else
            {
                Logger.LogDebug("Timed out for Skin!");
            }
        }

        var accessory = equipment.m_parts[6][0]?.GetTexture(0);
        if (accessory != null)
        {
            var task1 = Task.Run(() =>
                equipment.m_parts[6][0]?.GetTexture(0, equipment.GetItemColors(6))
            );
            if (task1.Wait(5))
            {
                accessory = task1.Result;
            }
            else
            {
                Logger.LogDebug("Timed out for Accessory!");
            }
        }

        var head = equipment.m_parts[8][0]?.GetTexture(0);
        Logger.LogDebug("Getting head color: " + equipment.GetItemColors(8));
        if (head != null)
        {
            var task2 = Task.Run(() => equipment.m_parts[8][0]?.GetTexture(0, equipment.GetItemColors(8)));
            if (task2.Wait(5))
            {
                head = task2.Result;
            }
            else
            {
                Logger.LogDebug("Timed out for Head!");
            }
        }


        //Filter
        if (equipment.GetItem(6) != null)
        {
            foreach (string str in DisallowedAccessories)
            {
                if (equipment.GetItem(6).Filename.ToLower().Contains(str))
                {
                    accessory = null;
                }
            }
        }

        if (skin != null && (skin.Width != 16 || skin.Height != 16))
        {
            skin = null;
        }

        if (accessory != null && (accessory.Width != 16 || accessory.Height != 16))
        {
            accessory = null;
        }

        if (head != null && (head.Width != 16 || head.Height != 16))
        {
            head = null;
        }

        //Get Colors
        Texture2D texture = new(skin?.GraphicsDevice, 16, 16);
        var data = new Color[16 * 16];
        texture.GetData(data);
        var skinData = new Color[16 * 16];
        skin?.GetData(skinData);

        var accData = new Color[16 * 16];
        accessory?.GetData(accData);

        var headData = new Color[16 * 16];
        head?.GetData(headData);

        //Draw
        for (int i = 0; i < data.Length; i++)
        {
            var color = Color.Transparent;
            if (skin != null && skinData[i].A == 255)
            {
                color = skinData[i];
            }

            if (accessory != null && accData[i].A == 255)
            {
                color = accData[i];
            }

            if (head != null && headData[i].A == 255)
            {
                color = headData[i];
            }

            data[i] = color;
        }

        texture.SetData(data);
        return texture;
    }

    public override void Draw(SpriteBatch spriteBatch, float ms)
    {
        if (ReplaceTexture != null)
        {
            Texture = ReplaceTexture;
            ClearDecals();
            AddDecal(new ObjectDecal(ReplaceTexture));
        }
        else
        {
            ReplaceTexture = BackupTexture ?? Textures.GetTexture("Giblet04");
        }

        base.Draw(spriteBatch, ms);
    }

    public override void PlayerMeleeHit(Player player, PlayerHitEventArgs e)
    {
        ObjectDataMethods.DefaultPlayerHitBaseballEffect(this, player, e);
    }

    internal static string EquipmentToString(Equipment eq)
    {
        string str = "";
        str += eq.GetItem(0) == null
            ? "NONE:0,0,0"
            : eq.GetItem(0).Filename + ":" + eq.GetItemColors(0)[0] + "," + eq.GetItemColors(0)[1] + "," + eq.GetItemColors(0)[2];
        str += "|";
        str += eq.GetItem(6) == null
            ? "NONE:0,0,0"
            : eq.GetItem(6).Filename + ":" + eq.GetItemColors(6)[0] + "," + eq.GetItemColors(6)[1] + "," + eq.GetItemColors(6)[2];
        str += "|";
        str += eq.GetItem(8) == null
            ? "NONE:0,0,0"
            : eq.GetItem(8).Filename + ":" + eq.GetItemColors(8)[0] + "," + eq.GetItemColors(8)[1] + "," + eq.GetItemColors(8)[2];
        return str;
    }

    internal static Texture2D TextureFromString(string str)
    {
        Equipment equipment = new();
        string[] fullItems = str.Split('|');
        var skin = Items.GetItem(fullItems[0].Split(':')[0]);
        string[] skinColors = fullItems[0].Split(':')[1].Split(',');
        var accessory = Items.GetItem(fullItems[1].Split(':')[0]);
        string[] accColors = fullItems[1].Split(':')[1].Split(',');
        var head = Items.GetItem(fullItems[2].Split(':')[0]);
        string[] headColors = fullItems[2].Split(':')[1].Split(',');
        equipment.Equip(skin);
        equipment.Equip(accessory);
        equipment.Equip(head);
        equipment.SetItemColors(0, skinColors);
        equipment.SetItemColors(6, accColors);
        equipment.SetItemColors(8, headColors);
        return GetTextureFromEquipment(equipment);
    }

    public override void EditDrawExtraData(SpriteBatch spriteBatch)
    {
        var targetPlayerProfileInfoObjects = GetObjectsFromProperty<ObjectPlayerProfileInfo>(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);
        if (targetPlayerProfileInfoObjects is { Count: > 0 })
        {
            foreach (var objectPlayerProfileInfo in targetPlayerProfileInfoObjects)
            {
                GameWorld.DrawEditArrowLine(spriteBatch, this, objectPlayerProfileInfo, Color.White, 1.5f);
                GameWorld.EditHighlightObjectsOnce.Add(objectPlayerProfileInfo);
            }
        }

        base.EditDrawExtraData(spriteBatch);
    }
}