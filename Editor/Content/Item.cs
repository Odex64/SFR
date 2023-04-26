using System;
using System.Collections.Generic;
using Editor.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Content;

public class Item
{
    public enum GenderType
    {
        Unisex,
        Female,
        Male
    }

    private bool[] _itemColorLevels;

    public Item(ItemPart[] parts, string gameName, string fileName, int equipmentLayer, string id, bool jacketUnderBelt, bool canEquip, bool canScript, string colorPalette)
    {
        Parts = parts;
        GameName = gameName;
        Filename = fileName;
        EquipmentLayer = equipmentLayer;
        ID = id;
        JacketUnderBelt = jacketUnderBelt;
        CanEquip = canEquip;
        CanScript = canScript;
        ColorPalette = colorPalette;
        OtherGenderItem = null;
        Gender = GenderType.Unisex;
        Locked = false;
        if (Filename.ToLowerInvariant().EndsWith("_fem"))
        {
            FileIsFem = true;
            FilenameWithoutFem = Filename.Substring(0, Filename.Length - 4);
            return;
        }

        FileIsFem = false;
        FilenameWithoutFem = Filename;
    }

    internal bool FileIsFem { get; }
    internal string FilenameWithoutFem { get; }
    public GenderType Gender { get; internal set; }
    public string GameName { get; }
    public string Filename { get; }

    // [Obsolete("This is not used and its value will always be null")]
    // private Texture2D Image { get; set; }
    public ItemPart[] Parts { get; }
    public Item OtherGenderItem { get; internal set; }
    public string ID { get; }
    public int EquipmentLayer { get; }
    public bool JacketUnderBelt { get; }
    public bool CanEquip { get; }

    public bool CanScript { get; }

    public string ColorPalette { get; }

    public bool Locked { get; set; }

    public bool CanEquipAndUnlocked => CanEquip && !Locked;

    public ColorPalette GetColorPalette() => ColorPaletteDatabase.GetColorPalette(ColorPalette);

    public override string ToString() => string.Concat("Item[GameName:", GameName, ",FileName:", Filename, ",ID:", ID, "]");

    public void Dispose()
    {
        OtherGenderItem = null;
        for (int i = 0; i < Parts.Length; i++)
        {
            for (int j = 0; j < Parts[i].Textures.Length; j++)
            {
                if (Parts[i].Textures[j] != null)
                {
                    Parts[i].Textures[j].Dispose();
                }
            }
        }
    }

    internal void PostProcess()
    {
        _itemColorLevels = new bool[3];
        _itemColorLevels[0] = false;
        _itemColorLevels[1] = false;
        _itemColorLevels[2] = false;
        var colorPalette = ColorPaletteDatabase.GetColorPalette(ColorPalette);
        if (colorPalette != null)
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    for (int j = 0; j < Parts[i].Textures.Length; j++)
                    {
                        // Texture2D texture2D = this.Parts[i].Textures[j];
                        var texture2D = Parts[i].Textures[j];
                        if (texture2D != null)
                        {
                            // bool[,] textureColorableFields = Textures.GetTextureColorableFields(texture2D, colorPalette);
                            bool[,] textureColorableFields = GetTextureColorableFields(texture2D, colorPalette);
                            for (int k = 0; k < 5; k++)
                            {
                                if (textureColorableFields[0, k])
                                {
                                    _itemColorLevels[0] = true;
                                }

                                if (textureColorableFields[1, k])
                                {
                                    _itemColorLevels[1] = true;
                                }

                                if (textureColorableFields[2, k])
                                {
                                    _itemColorLevels[2] = true;
                                }
                            }
                        }

                        if (_itemColorLevels[0] && _itemColorLevels[1] && _itemColorLevels[2])
                        {
                            return;
                        }
                    }
                }
            }
        }
    }

    public static bool[,] GetTextureColorableFields(Texture2D texture, ColorPalette colorPalette)
    {
        bool[,] array = new bool[3, 5];
        for (int i = 0; i < 5; i++)
        {
            array[0, i] = false;
            array[1, i] = false;
            array[2, i] = false;
        }

        if (texture == null || colorPalette == null)
        {
            return array;
        }

        var array2 = new Color[texture.Width * texture.Height];
        // var array2 = texture.GetData();
        int[] array3 = { 255, 192, 128, 64, 32 };
        for (int j = 0; j < 3; j++)
        {
            var list = new List<Color[]>();
            for (int k = 0; k < colorPalette.Colors[j].Count; k++)
            {
                list.Add(ColorDatabase.GetColor(colorPalette.Colors[j][k]));
            }

            int num = 0;
            for (int l = 0; l < list.Count; l++)
            {
                num = Math.Max(num, list[l].Length);
            }

            for (int m = 0; m < num; m++)
            {
                foreach (var color in array2)
                {
                    if (j == 0)
                    {
                        if (color.R == array3[m] && color.G == 0 && color.B == 0)
                        {
                            array[j, m] = true;
                            break;
                        }
                    }
                    else if (j == 1)
                    {
                        if (color.R == 0 && color.G == array3[m] && color.B == 0)
                        {
                            array[j, m] = true;
                            break;
                        }
                    }
                    else if (j == 2 && color.R == 0 && color.G == 0 && color.B == array3[m])
                    {
                        array[j, m] = true;
                        break;
                    }
                }
            }

            list.Clear();
        }

        return array;
    }

    public bool CanRecolorLevelPossible(int colorLevel) => _itemColorLevels[colorLevel];
}