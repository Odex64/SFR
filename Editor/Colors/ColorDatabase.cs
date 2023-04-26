using System;
using System.Collections.Generic;
using Editor.Data;
using Microsoft.Xna.Framework;

namespace Editor.Colors;

public static class ColorDatabase
{
    private static readonly Dictionary<string, Color[]> Colors = new();

    public static void Load(string path)
    {
        Reader.ReadDataFromFile(path);
    }

    public static void ConstructColor(List<DataNode> tileNodeProperties, string tileName)
    {
        string text = tileName.ToUpperInvariant();
        Color[] array = null;
        foreach (var dataNode in tileNodeProperties)
        {
            try
            {
                string text2 = dataNode.Property.ToUpperInvariant();
                if (text2 != "KEY")
                {
                    if (text2 == "C")
                    {
                        string text3 = dataNode.Value.ToUpperInvariant();
                        text3 = text3.Replace("(", "");
                        text3 = text3.Replace(")", "");
                        string[] array2 = text3.Split(',');
                        if (array2.Length == 0 || array2.Length % 3 != 0)
                        {
                            throw new Exception(string.Concat("Error: Colors.ConstructColor failed for property '", dataNode.Property, "' with value '", dataNode.Value, "'. Incorrect color construction, must be in the format (r,g,b)"));
                        }

                        try
                        {
                            array = new Color[array2.Length / 3];
                            for (int i = 0; i < array2.Length; i += 3)
                            {
                                byte b = byte.Parse(array2[i]);
                                byte b2 = byte.Parse(array2[i + 1]);
                                byte b3 = byte.Parse(array2[i + 2]);
                                array[i / 3] = new Color(b, b2, b3);
                            }
                        }
                        catch
                        {
                            throw new Exception(string.Concat("Error: Colors.ConstructColor failed for property '", dataNode.Property, "' with value '", dataNode.Value, "'. Could not parse values"));
                        }
                    }
                }
                else
                {
                    text = dataNode.Value.ToUpperInvariant();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Error: Colors.ConstructColor failed for property '", dataNode.Property, "' with value '", dataNode.Value, "'\r\n", ex.ToString()));
            }
        }

        array ??= new[] { Color.White };

        if (array.Length > 5)
        {
            throw new Exception($"Error: Color '{text}' defines too many colors. Max 5 is allowed.");
        }

        if (Colors.ContainsKey(text))
        {
            Console.WriteLine($@"Error: Color '{text}' already exist. Will be replaced with new values.");
            Colors[text] = array;
            return;
        }

        Colors.Add(text, array);
    }

    public static Color[] GetColor(string colorKey)
    {
        colorKey = colorKey.ToUpperInvariant();
        return Colors.ContainsKey(colorKey) ? Colors[colorKey] : null;
    }
}