using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SFDX;

internal static class Program
{
    private const string DataSFD = @"C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe\Content\Data";
    private static string _data = Path.GetFullPath(Assembly.GetExecutingAssembly().Location + @"\..\..\..\..\Content\Data");

    public static void Main(string[] args)
    {
        if (!Directory.Exists(_data))
        {
            _data = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "SFR", "Content", "Data"));
        }

        // Check(@"Tiles\objects.sfdx", @"Images\Objects");
        // return;

        LogSuccess("Select which category to check\n1. objects\n2. tiles\n3. backgrounds\n4. fbg");
        switch (Console.ReadKey(true).Key)
        {
            case ConsoleKey.D1:
                Check(@"Tiles\objects.sfdx", @"Images\Objects");
                break;

            case ConsoleKey.D2:
                Check(@"Tiles\tiles.sfdx", @"Images\Tiles\Solid");
                break;

            case ConsoleKey.D3:
                Check(@"Tiles\tilesBG.sfdx", @"Images\Tiles\Background");
                break;

            case ConsoleKey.D4:
                Check(@"Tiles\tilesFarBG.sfdx", @"Images\Tiles\FarBG");
                break;

            default:
                LogError("None selected. Closing.");
                break;
        }
    }

    private static void Check(string sfdx, string path)
    {
        LogSuccess("Missing entries in sfdx");
        // string fileNameSFR = Path.Combine(sfdx.Substring(0, sfdx.LastIndexOf(@"\", StringComparison.Ordinal)), $"SFR{Path.GetFileName(sfdx)}");
        string objects = File.ReadAllText(Path.Combine(_data, sfdx));
        // string objectsSFR = File.ReadAllText(Path.Combine(_data, fileNameSFR));

        var files = Directory.GetFiles(Path.Combine(DataSFD, path), "*.xnb", SearchOption.AllDirectories).ToList();
        files.AddRange(Directory.GetFiles(Path.Combine(_data, path), "*.xnb", SearchOption.AllDirectories));
        foreach (string file in files)
        {
            if (!objects.Contains(ToTile(file), StringComparison.OrdinalIgnoreCase))
            {
                LogError($"{Path.GetFileNameWithoutExtension(file)} not found in sfdx!");
            }
        }

        // string[] filesSFR = Directory.GetFiles(Path.Combine(_data, path), "*.xnb", SearchOption.AllDirectories);
        // foreach (string file in filesSFR)
        // {
        //     if (!objectsSFR.Contains(ToTile(file), StringComparison.OrdinalIgnoreCase))
        //     {
        //         LogError($"[SFR] {Path.GetFileNameWithoutExtension(file)} not found in sfdx!");
        //     }
        // }

        LogSuccess("\nDuplicate entries in sfdx and missing files in folder");
        LogDuplicatesAndMissing(objects.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList(), files.ToList());
        // LogDuplicatesAndMissing(objectsSFR.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList(), filesSFR.ToList(), true);
    }

    private static void LogDuplicatesAndMissing(List<string> lines, List<string> files)
    {
        var duplicates = new List<string>();
        foreach (string line in lines)
        {
            if (!duplicates.Contains(line) && line.StartsWith("Tile", StringComparison.OrdinalIgnoreCase) && lines.Count(e => e == line) > 1)
            {
                string item = line.Substring(5, line.IndexOf(')') - 5);
                duplicates.Add(line);
                LogWarn($"Duplicate entry for: {item} in sfdx");
            }
            else if (line.StartsWith("Tile", StringComparison.OrdinalIgnoreCase))
            {
                string item = line.Substring(5, line.IndexOf(')') - 5);
                string tile = files.Find(e => e.Contains(item));
                if (tile == null)
                {
                    LogError($"Found {item} entry in sfdx but missing in folder");
                }
            }
            else if (line.StartsWith("//Tile", StringComparison.OrdinalIgnoreCase))
            {
                string item = line.Substring(7, line.IndexOf(')') - 7);
                string tile = files.Find(e => e.Contains(item));
                LogWarn(tile == null ? $"Found COMMENTED {item} entry in sfdx but missing in folder" : $"Found file {item} but COMMENTED in sfdx");
            }
        }
    }

    private static Dictionary<string, int?> CheckDuplicates(List<string> list)
    {
        var duplicates = new Dictionary<string, int?>();
        foreach (string item in list)
        {
            if (duplicates[item] != null)
            {
                duplicates[item]++;
            }
            else
            {
                duplicates.Add(item, 1);
            }
        }

        return duplicates;
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

    private static void LogWarn(string message)
    {
        if (Console.ForegroundColor != ConsoleColor.Yellow)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
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