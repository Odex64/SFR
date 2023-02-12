using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using HarmonyLib;
using SFR.Helper;
using SFR.Misc;

namespace SFR;

/// <summary>
///     Entry point of SFR. This class will simply check for available updates, patch SFD and start the game.
/// </summary>
internal static class Program
{
    private const string VersionURI = "https://raw.githubusercontent.com/Odex64/SFR/master/version";
    private static string _gameURI = "https://github.com/Odex64/SFR/releases/download/GAMEVERSION/SFR.zip";
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    private static readonly Harmony Harmony = new("github.com/Odex64/SFR");
    private static WebClient _webClient;

    private static int Main(string[] args)
    {
        if (args.Contains("-HELP", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("#Command-line parameters", false);
            Logger.LogWarn("-HELP            Show help dialog.\n-SFD             Directly start SFD.\n-SFR             Directly start SFR.\n-SKIP            Skip 'check updates' dialog.\n-SLOTS <amount>  Set slots amount for dedicated server.\n", false);
            Logger.LogInfo("Example command to skip updates and start SFR server with 16 slots", false);
            Logger.LogWarn("SFR.exe -sfr -skip -server -slots 16", false);
            return 0;
        }

        if (args.Contains("-SFD", StringComparer.OrdinalIgnoreCase))
        {
            string gameFile = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
            if (!File.Exists(gameFile))
            {
                Logger.LogError("Superfighters Deluxe.exe not found!");
                return -1;
            }

            Process.Start(gameFile, string.Join(" ", args));
            return 0;
        }

        if (!args.Contains("-SFR", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("Start SFR or SFD: \n1. SFR\n2. SFD", false);
            Console.SetCursorPosition("Start SFD or SFD: ".Length, Console.CursorTop - 3);
            var key = Console.ReadKey().Key;
            Console.SetCursorPosition(0, Console.CursorTop + 4);
            if (key == ConsoleKey.D2)
            {
                string gameFile = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
                if (!File.Exists(gameFile))
                {
                    Logger.LogError("Superfighters Deluxe.exe not found!");
                    return -1;
                }

                Process.Start(gameFile, string.Join(" ", args));
                return 0;
            }
        }

        if (!(args.Length > 0 && args.Contains("-SKIP", StringComparer.OrdinalIgnoreCase)) && !Constants.SFRVersion.EndsWith("_dev") && Choice("Check for updates? (Y/n)"))
        {
            if (CheckUpdate())
            {
                return 0;
            }
        }

        bool isServer = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-server")
            {
                isServer = true;
            }
            else if (isServer && args[i].Equals("-SLOTS", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int slots))
                {
                    Constants.Slots = slots;
                }
            }
        }

        Logger.LogWarn("Patching");
        Harmony.PatchAll();
        Logger.LogError("Starting SFR");
        SFD.Program.Main(args);

        return 0;
    }

    private static bool CheckUpdate()
    {
        string remoteVersion;
        try
        {
            _webClient = new WebClient();
            remoteVersion = _webClient.DownloadString(VersionURI);
        }
        catch (WebException)
        {
            Logger.LogError("Couldn't fetch updates - Starting the game without updating!");
            _webClient.Dispose();
            return false;
        }

        string[] versionInfo = remoteVersion.Split('+');
        _gameURI = _gameURI.Replace("GAMEVERSION", versionInfo[0]);

        switch (string.CompareOrdinal(Constants.SFRVersion, versionInfo[0]))
        {
            // New version
            case < 0:
                return Update();
            
            // Same version but hotfix if present
            case 0 when versionInfo.Length > 1 && int.TryParse(versionInfo[1], out int result) && result > Constants.Build:
                return Update();
        }

        _webClient.Dispose();

        Logger.LogInfo("No updates found. Starting");
        return false;
    }

    private static bool Choice(string message)
    {
        Logger.LogWarn(message, false);
        return (Console.ReadLine() ?? string.Empty).Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    private static void ReplaceOldFile(string file)
    {
        string newExtension = Path.ChangeExtension(file, "old");
        if (File.Exists(newExtension))
        {
            File.Delete(newExtension);
        }

        File.Move(file, newExtension);
    }

    private static bool Update()
    {
        string contentDirectory = Path.Combine(GameDirectory, @"SFR");
        if (Choice($"All files in {contentDirectory} will be erased. Proceed? (Y/n):"))
        {
            Logger.LogInfo("Downloading files...");
            string archivePath = Path.Combine(GameDirectory, "SFR.zip");

            try
            {
                _webClient.DownloadFile(_gameURI, archivePath);
            }
            catch (WebException)
            {
                Logger.LogError("Couldn't fetch updates - Starting the game without updating!");
                return false;
            }
            finally
            {
                _webClient.Dispose();
            }

            ReplaceOldFile(Assembly.GetExecutingAssembly().Location);
            ReplaceOldFile(Path.Combine(GameDirectory, "SFR.exe.config"));

            foreach (string file in Directory.GetFiles(contentDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                ReplaceOldFile(file);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(contentDirectory, "Content"), "*.*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                archive.ExtractToDirectory(GameDirectory);
                archive.Dispose();
            }

            File.Delete(archivePath);

            Logger.LogInfo("SFR has been updated to the latest version.");
            return true;
        }

        _webClient.Dispose();

        Logger.LogWarn("Ignoring update.");
        return false;
    }
}