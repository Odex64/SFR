using System;
using System.IO;
using System.Linq;

namespace SFDX;

internal static class Program
{
    private const string Data = @"C:\Users\odex6\Desktop\Modding\SFR\SFDX\Data";
    private static readonly string[] TargetFolders = { @"Images\Objects", @"Images\Tiles" };
    private static readonly string[] OriginalTiles = { "objects.sfdx", "tiles.sfdx", "tilesBG.sfdx", "tilesE.sfdx", "tilesFarBG.sfdx", "tilesS.sfdx" };

    public static void Main(string[] args)
    {
        File.WriteAllText(Path.Combine(Data, "sorted.sfdx"), SortTiles(File.ReadAllText(Path.Combine(Data, "sfr_tilesFarBg.sfdx"))));


        // string data = string.Empty;

        // foreach (string file in Directory.GetFiles(Data, "*.sfdx", SearchOption.TopDirectoryOnly))
        // {
        //     data += File.ReadAllText(file) + "\r\n";
        // }

        // FindMissingTiles(data);
    }

    private static string SortTiles(string data)
    {
        string[] entries = data.Split(new[] { "defaultTile", "\r\nTile" }, StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(entries);
        return string.Join("\r\nTile", entries);
    }

    private static void FindMissingTiles(string data)
    {
        foreach (string file in Directory.GetFiles(Data, "*.xnb", SearchOption.AllDirectories)
                     .Where(d => TargetFolders.Any(e => Path.Combine(Data, Path.GetDirectoryName(d)!).Contains(e))))
        {
            if (!data.Contains(ToTile(file), StringComparison.OrdinalIgnoreCase))
                // if (file.Contains("Bg"))
            {
                LogError("Tile(" + Path.GetFileNameWithoutExtension(file) + ")" + "{}");
            }
        }
    }

    private static void ResetColor()
    {
        if (Console.ForegroundColor != ConsoleColor.Gray)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    private static void LogSuccess(string message)
    {
        if (Console.ForegroundColor != ConsoleColor.Green)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        Console.WriteLine(message);
    }

    private static void LogError(string message)
    {
        if (Console.ForegroundColor != ConsoleColor.Red)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine(message);
    }

    private static string ToTile(string value) => $"Tile({Path.GetFileNameWithoutExtension(value)})";

    private static bool Contains(this string source, string toCheck, StringComparison comp) => source.IndexOf(toCheck, comp) >= 0;
}