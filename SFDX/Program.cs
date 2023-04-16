using System;
using System.IO;
using System.Linq;

namespace SFDX;

internal static class Program
{
    private const string Data = @"C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe\SFR\Content\Data";
    private static readonly string[] TargetFolders = { @"Images\Objects", @"Images\Tiles" };

    public static void Main(string[] args)
    {
        string data = string.Empty;

        foreach (string file in Directory.GetFiles(Path.Combine(Data, "Tiles"), "*.sfdx", SearchOption.TopDirectoryOnly))
        {
            data += File.ReadAllText(file);
        }

        foreach (string file in Directory.GetFiles(Data, "*.xnb", SearchOption.AllDirectories)
                     .Where(d => TargetFolders.Any(e => Path.Combine(Data, Path.GetDirectoryName(d)!).Contains(e))))
        {
            if (!data.Contains(ToTile(file), StringComparison.OrdinalIgnoreCase))
            {
                // if (file.Contains("Bg"))
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