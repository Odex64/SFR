using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Editor.Colors;

public class ColorPalette
{
    public const int TotalColorLevels = 3;
    private string _firstKey = string.Empty;

    public ColorPalette(string key, string name, IReadOnlyList<string[]> colors)
    {
        Key = key.ToUpperInvariant();
        Name = name;
        for (int i = 0; i < 3; i++)
        {
            if (colors[i] != null)
            {
                foreach (string text in colors[i])
                {
                    if (!Colors[i].Contains(text))
                    {
                        Colors[i].Add(text);
                    }
                }
            }
        }
    }

    private ColorPalette() { }

    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < 3; i++)
            {
                if (Colors[i].Count > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public List<string>[] Colors { get; private set; } =
    {
        new(),
        new(),
        new()
    };

    public string Key { get; private set; }
    public string Name { get; private set; }

    public bool IsSame(ColorPalette otherColorPalette)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Colors[i].Count != otherColorPalette.Colors[i].Count)
            {
                return false;
            }

            for (int j = 0; j < Colors[i].Count; j++)
            {
                if (Colors[i][j] != otherColorPalette.Colors[i][j])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void AdjustColorFields(bool[,] colorFields)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = Colors[i].Count - 1; j >= 0; j--)
            {
                var color = ColorDatabase.GetColor(Colors[i][j]);
                bool flag = false;
                for (int k = 0; k < color.Length; k++)
                {
                    if (colorFields[i, k])
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    Colors[i].RemoveAt(j);
                }
            }
        }
    }

    public string GetFirstColorFromLevel(int level) => Colors[level].Count > 0 ? Colors[level][0] : string.Empty;

    // public string GetRandomColorFromLevel(int level)
    // {
    //     if (this.colors[level].Count > 0)
    //     {
    //         return this.colors[level][Constants.RANDOM.Next(0, this.colors[level].Count)];
    //     }
    //
    //     return "";
    // }

    public string[] GetFirstColorFromLevels()
    {
        string[] array = new string[3];
        for (int i = 0; i < 3; i++)
        {
            array[i] = GetFirstColorFromLevel(i);
        }

        return array;
    }

    // public string[] GetRandomColorsFromLevels(bool random1, bool random2, bool random3)
    // {
    //     return new string[]
    //     {
    //         random1 ? this.GetRandomColorFromLevel(0) : this.GetFirstColorFromLevel(0),
    //         random2 ? this.GetRandomColorFromLevel(1) : this.GetFirstColorFromLevel(1),
    //         random3 ? this.GetRandomColorFromLevel(2) : this.GetFirstColorFromLevel(2)
    //     };
    // }

    public bool ContainsColorPackageForLevel(int level, string colorPackage) => Colors != null && level >= 0 && level < Colors.Length && Colors[level] != null && Colors[level].Contains(colorPackage);

    public List<string> GetColorNamesFromLevel(int level)
    {
        if (Colors[level] == null)
        {
            return null;
        }

        return Colors[level];
    }

    public List<Color[]> GetColorsFromLevel(int level)
    {
        if (Colors[level] == null)
        {
            return null;
        }

        var list = new List<Color[]>();
        foreach (string text in Colors[level])
        {
            var color = ColorDatabase.GetColor(text);
            if (color != null)
            {
                list.Add(color);
            }
        }

        return list;
    }

    public void Dispose()
    {
        Colors = null;
    }

    public ColorPalette Intersect(ColorPalette colorPalette)
    {
        var colorPalette2 = new ColorPalette();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < Colors[i].Count; j++)
            {
                for (int k = 0; k < colorPalette.Colors[i].Count; k++)
                {
                    if (!string.IsNullOrEmpty(Colors[i][j]) && Colors[i][j] == colorPalette.Colors[i][k])
                    {
                        colorPalette2.Colors[i].Add(Colors[i][j]);
                        break;
                    }
                }
            }
        }

        return colorPalette2;
    }

    public ColorPalette Union(ColorPalette colorPalette)
    {
        var colorPalette2 = colorPalette.Copy();
        for (int i = 0; i < 3; i++)
        {
            foreach (string text in colorPalette.Colors[i])
            {
                if (!colorPalette2.Colors[i].Contains(text))
                {
                    colorPalette2.Colors[i].Add(text);
                }
            }
        }

        return colorPalette2;
    }

    public void CombineFrom(ColorPalette colorPalette)
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (string text in colorPalette.Colors[i])
            {
                if (!Colors[i].Contains(text))
                {
                    Colors[i].Add(text);
                }
            }
        }
    }

    private ColorPalette Copy()
    {
        var colorPalette = new ColorPalette
        {
            Name = Name,
            _firstKey = _firstKey,
            Key = Key
        };

        for (int i = 0; i < 3; i++)
        {
            foreach (string text in Colors[i])
            {
                colorPalette.Colors[i].Add(text);
            }
        }

        return colorPalette;
    }
}