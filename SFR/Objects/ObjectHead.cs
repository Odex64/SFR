﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using SFD.Tiles;
using SFR.Helper;
using Player = SFD.Player;

namespace SFR.Objects;

internal sealed class ObjectHead : ObjectGiblet
{
    private static readonly Texture2D _backupTexture = null;
    private static readonly string[] _disallowedAccessories = ["swing", "armband", "dogtag", "scarf"];
    internal Texture2D ReplaceTexture;

    internal ObjectHead(ObjectDataStartParams startParams) : base(startParams)
    {
    }

    public override void SetProperties() => Properties.Add(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);

    public override void Initialize()
    {
        base.Initialize();
        if (Properties.Exists(290) && GameOwner != GameOwnerEnum.Client)
        {
            if (Properties.Get(290).Value is not null)
            {
                List<ObjectPlayerProfileInfo> targetPlayerProfileInfoObjects = GetObjectsFromProperty<ObjectPlayerProfileInfo>(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);
                if (targetPlayerProfileInfoObjects.Count > 0)
                {
                    Profile profile = targetPlayerProfileInfoObjects[0].GetProfile();
                    Equipment equipment = new();
                    equipment.Equip(profile.EquippedItems[0]);
                    equipment.Equip(profile.EquippedItems[6]);
                    equipment.Equip(profile.EquippedItems[8]);
                    // GenericData.SendGenericDataToClients(new GenericData(DataType.Head, new SyncFlags[] { }, ObjectID, EquipmentToString(equipment)));
                    // SyncedMethod(new ObjectDataSyncedMethod(ObjectDataSyncedMethod.Methods.AnimationSetFrame, GameWorld.ElapsedTotalGameTime, EquipmentToString(equipment)));
                }
            }
        }

        GameWorld.PortalsObjectsToKeepTrackOf.Add(this);
    }

    public override void Dispose()
    {
        _ = GameWorld.PortalsObjectsToKeepTrackOf.Remove(this);
        base.Dispose();
    }

    private static Texture2D GetTextureFromEquipment(Equipment equipment)
    {
        Texture2D skin = equipment.m_parts[0][0]?.Textures[0];
        if (skin is not null)
        {
            Task<Texture2D> task = Task.Run(() => equipment.m_parts[0][0]?.GetTexture(0, equipment.GetItemColors(0), equipment.GetItemColorsKey(0)));
            if (task.Wait(5))
            {
                skin = task.Result;
            }
            else
            {
                Logger.LogWarn("Timed out for Skin!");
            }
        }

        Texture2D accessory = equipment.m_parts[6][0]?.Textures[0];
        if (accessory is not null)
        {
            Task<Texture2D> task1 = Task.Run(() => equipment.m_parts[6][0]?.GetTexture(0, equipment.GetItemColors(6), equipment.GetItemColorsKey(6)));
            if (task1.Wait(5))
            {
                accessory = task1.Result;
            }
            else
            {
                Logger.LogWarn("Timed out for Accessory!");
            }
        }

        Texture2D head = equipment.m_parts[8][0]?.Textures[0];
        if (head is not null)
        {
            Task<Texture2D> task2 = Task.Run(() => equipment.m_parts[8][0]?.GetTexture(0, equipment.GetItemColors(8), equipment.GetItemColorsKey(8)));
            if (task2.Wait(5))
            {
                head = task2.Result;
            }
            else
            {
                Logger.LogWarn("Timed out for Head!");
            }
        }


        // Filter
        if (equipment.GetItem(6) is not null)
        {
            foreach (string str in _disallowedAccessories)
            {
                if (equipment.GetItem(6).Filename.ToLower().Contains(str))
                {
                    accessory = null;
                }
            }
        }

        if (skin is not null && (skin.Width != 16 || skin.Height != 16))
        {
            skin = null;
        }

        if (accessory is not null && (accessory.Width != 16 || accessory.Height != 16))
        {
            accessory = null;
        }

        if (head is not null && (head.Width != 16 || head.Height != 16))
        {
            head = null;
        }

        // Get Colors
        Texture2D texture = new(skin?.GraphicsDevice, 16, 16);
        Color[] data = new Color[16 * 16];
        texture.GetData(data);
        Color[] skinData = new Color[16 * 16];
        skin?.GetData(skinData);

        Color[] accData = new Color[16 * 16];
        accessory?.GetData(accData);

        Color[] headData = new Color[16 * 16];
        head?.GetData(headData);

        // Draw
        for (int i = 0; i < data.Length; i++)
        {
            Color color = Color.Transparent;
            if (skin is not null && skinData[i].A == 255)
            {
                color = skinData[i];
            }

            if (accessory is not null && accData[i].A == 255)
            {
                color = accData[i];
            }

            if (head is not null && headData[i].A == 255)
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
        if (ReplaceTexture is not null)
        {
            Texture = ReplaceTexture;
            ClearDecals();
            AddDecal(new ObjectDecal(ReplaceTexture));
        }
        else
        {
            ReplaceTexture = _backupTexture ?? Textures.GetTexture("Giblet04");
        }

        base.Draw(spriteBatch, ms);
    }

    public override void PlayerMeleeHit(Player player, PlayerHitEventArgs e) => ObjectDataMethods.DefaultPlayerHitBaseballEffect(this, player, e);

    internal static string EquipmentToString(Equipment eq)
    {
        string str = eq.GetItem(0) is null
            ? "NONE:0,0,0"
            : eq.GetItem(0).Filename + ":" + eq.GetItemColors(0)[0] + "," + eq.GetItemColors(0)[1] + "," + eq.GetItemColors(0)[2];

        str += "|";
        str += eq.GetItem(6) is null
            ? "NONE:0,0,0"
            : eq.GetItem(6).Filename + ":" + eq.GetItemColors(6)[0] + "," + eq.GetItemColors(6)[1] + "," + eq.GetItemColors(6)[2];

        str += "|";
        str += eq.GetItem(8) is null
            ? "NONE:0,0,0"
            : eq.GetItem(8).Filename + ":" + eq.GetItemColors(8)[0] + "," + eq.GetItemColors(8)[1] + "," + eq.GetItemColors(8)[2];

        return str;
    }

    internal static Texture2D TextureFromString(string str)
    {
        Equipment equipment = new();
        string[] fullItems = str.Split('|');
        Item skin = Items.GetItem(fullItems[0].Split(':')[0]);
        string[] skinColors = fullItems[0].Split(':')[1].Split(',');
        Item accessory = Items.GetItem(fullItems[1].Split(':')[0]);
        string[] accColors = fullItems[1].Split(':')[1].Split(',');
        Item head = Items.GetItem(fullItems[2].Split(':')[0]);
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
        List<ObjectPlayerProfileInfo> targetPlayerProfileInfoObjects = GetObjectsFromProperty<ObjectPlayerProfileInfo>(ObjectPropertyID.ScriptPlayerSpawnProfileInfoTarget);
        if (targetPlayerProfileInfoObjects is { Count: > 0 })
        {
            foreach (ObjectPlayerProfileInfo objectPlayerProfileInfo in targetPlayerProfileInfoObjects)
            {
                GameWorld.DrawEditArrowLine(spriteBatch, this, objectPlayerProfileInfo, Color.White, 1.5f);
                GameWorld.EditHighlightObjectsOnce.Add(objectPlayerProfileInfo);
            }
        }

        base.EditDrawExtraData(spriteBatch);
    }
}