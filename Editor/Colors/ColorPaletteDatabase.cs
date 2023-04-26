using System;
using System.Collections.Generic;
using Editor.Data;

namespace Editor.Colors;

public static class ColorPaletteDatabase
{
    private static readonly Dictionary<string, ColorPalette> Palettes = new();

    public static void Load(string path)
    {
        Reader.ReadDataFromFile(path);
    }

    public static void ConstructColorPalette(List<DataNode> tileNodeProperties, string tileName)
    {
        string text = tileName.ToUpperInvariant();
        string[][] array = new string[3][];
        foreach (var dataNode in tileNodeProperties)
        {
            try
            {
                string text2 = dataNode.Property.ToUpperInvariant();
                if (text2 != "COLORS1")
                {
                    if (text2 != "COLORS2")
                    {
                        if (text2 == "COLORS3")
                        {
                            array[2] = dataNode.Value.Split(',');
                        }
                    }
                    else
                    {
                        array[1] = dataNode.Value.Split(',');
                    }
                }
                else
                {
                    array[0] = dataNode.Value.Split(',');
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Error: Colors.ConstructColorPalette failed for property '", dataNode.Property, "' with value '", dataNode.Value, "'\r\n", ex.ToString()));
            }
        }

        if (Palettes.ContainsKey(text))
        {
            throw new Exception($"Error: Color '{text}' already exist");
        }

        Palettes.Add(text, new ColorPalette(text, tileName, array));
    }


    public static ColorPalette GetColorPalette(string colorPaletteName)
    {
        colorPaletteName = colorPaletteName.ToUpperInvariant();
        if (Palettes.ContainsKey(colorPaletteName))
        {
            return Palettes[colorPaletteName];
        }

        return null;
    }
}