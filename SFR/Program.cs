using System;
using System.IO;
using System.IO.Compression;
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
    private static readonly Harmony Harmony = new("superfightersredux.tk");
    private static WebClient _webClient;

    private static int Main(string[] args)
    {
#if (!DEBUG)
        if (Choice("Check for updates? (Y/n)"))
        {
            if (CheckUpdate())
            {
                return 0;
            }
        }
#endif

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

        if (remoteVersion != Constants.SFRVersion)
        {
            _gameURI = _gameURI.Replace("GAMEVERSION", remoteVersion);
            return Update();
        }

        _webClient.Dispose();

        Logger.LogInfo("No updates found. Starting");
        return false;
    }

    private static bool Choice(string message)
    {
        Logger.LogWarn(message, false);
        return Console.ReadLine() is "y" or "Y";
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