using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Content;
using Microsoft.Xna.Framework;

namespace Editor.Misc;

internal static class Items
{
    private static readonly List<Item> AllItems = new();
    private static readonly List<Item> AllFemaleItems = new();
    private static readonly List<Item> AllMaleItems = new();
    private static readonly List<Item>[] SlotAllItems = new List<Item>[10];
    private static readonly List<Item>[] SlotFemaleItems = new List<Item>[10];
    private static readonly List<Item>[] SlotMaleItems = new List<Item>[10];

    public static readonly List<Color> DynamicColorTable = new();

    public static void Load(Game game, string path)
    {
        // ContentManager content = game.Content;
        // Items.m_allItems = new List<Item>();
        // Items.m_allFemaleItems = new List<Item>();
        // Items.m_allMaleItems = new List<Item>();
        // Items.m_slotAllItems = new List<Item>[10];
        // Items.m_slotFemaleItems = new List<Item>[10];
        // Items.m_slotMaleItems = new List<Item>[10];
        for (int i = 0; i < SlotAllItems.Length; i++)
        {
            SlotAllItems[i] = new List<Item>();
            SlotFemaleItems[i] = new List<Item>();
            SlotMaleItems[i] = new List<Item>();
        }

        var files = Directory.GetFiles(Path.Combine(path, "Content/Data/Items"), "*.xnb", SearchOption.AllDirectories).ToList();
        if (Program.HasRedux)
        {
            files.AddRange(Directory.GetFiles(Path.Combine(path, "SFR/Content/Data/Items"), "*.xnb", SearchOption.AllDirectories));
        }

        // files.ToArray();
        foreach (string itemFile in files)
        {
            // string text = itemFile.Substring(0, itemFile.Length - 4);
            // text = text.Remove(0, Constants.Paths.ExecutablePath.Length + "Content\\".Length);
            // while (text.StartsWith("\\") || text.StartsWith("/"))
            // {
            //     text = text.Remove(0, 1);
            // }

            // ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, "Loading equipment '" + text + "'");
            // text = Constants.GetLoadPath(text);
            // Item item = content.Load<Item>(text);
            // var test = (itemFile);
            var item = game.Content.Load<Item>(itemFile.Substring(0, itemFile.LastIndexOf('.')));
            foreach (var checkItem in AllItems)
            {
                if (checkItem.ID == item.ID)
                {
                    continue;
                    throw new Exception($"Error: Item ID collision between item {checkItem}, and {item},  while loading  {itemFile}");
                }
            }

            item.PostProcess();
            AllItems.Add(item);
            SlotAllItems[item.EquipmentLayer].Add(item);
            // Items.m_allItems.Add(item);
            // Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        }

        PostProcessGenders();
    }


    private static void PostProcessGenders()
    {
        for (int i = 0; i < AllItems.Count; i++)
        {
            var item = AllItems[i];
            for (int j = i + 1; j < AllItems.Count; j++)
            {
                var item2 = AllItems[j];
                if (item.FilenameWithoutFem == item2.FilenameWithoutFem)
                {
                    item.OtherGenderItem = item2;
                    item2.OtherGenderItem = item;
                    if (item.FileIsFem)
                    {
                        item.Gender = Item.GenderType.Female;
                        item2.Gender = Item.GenderType.Male;
                    }
                    else
                    {
                        item.Gender = Item.GenderType.Male;
                        item2.Gender = Item.GenderType.Female;
                    }
                }
            }
        }

        foreach (var item in AllItems)
        {
            switch (item.Gender)
            {
                case Item.GenderType.Unisex:
                    AllFemaleItems.Add(item);
                    SlotFemaleItems[item.EquipmentLayer].Add(item);
                    AllMaleItems.Add(item);
                    SlotMaleItems[item.EquipmentLayer].Add(item);
                    break;
                case Item.GenderType.Female:
                    AllFemaleItems.Add(item);
                    SlotFemaleItems[item.EquipmentLayer].Add(item);
                    break;
                case Item.GenderType.Male:
                    AllMaleItems.Add(item);
                    SlotMaleItems[item.EquipmentLayer].Add(item);
                    break;
            }
        }
    }
}