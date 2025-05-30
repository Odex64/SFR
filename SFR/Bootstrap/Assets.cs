using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SFD;
using SFD.Code;
using SFD.Colors;
using SFD.Parser;
using SFD.Sounds;
using SFD.Tiles;
using SFR.Fighter;
using SFR.Helper;
using SFR.Misc;
using Player = SFD.Player;

namespace SFR.Bootstrap;

/// <summary>
/// This is where SFR starts.
/// This class handles and loads all the new textures, music, sounds, tiles, colors etc...
/// This class is also used to tweak some game code on startup, such as window title.
/// </summary>
[HarmonyPatch]
internal static class Assets
{
    private static readonly string _contentPath = Path.Combine(Program.GameDirectory, @"SFR\Content");
    private static readonly string _officialsMapsPath = Path.Combine(_contentPath, @"Data\Maps\Official");

    ///// <summary>
    ///// Some items like Armband are locked by default.
    ///// Here we unlock all items & prevent specific ones from being equipped.
    ///// </summary>
    //[HarmonyTranspiler]
    //[HarmonyPatch(typeof(Challenges), nameof(Challenges.Load))]
    //private static IEnumerable<CodeInstruction> UnlockItems(IEnumerable<CodeInstruction> instructions)
    //{
    //    List<CodeInstruction> list = instructions.ToList();

    //    // Remove the following line:
    //    // Items.GetItems("FrankenbearSkin", "MechSkin", "HazmatMask", "Armband", "Armband_fem", "OfficerHat", "OfficerJacket", "OfficerJacket_fem", "GermanHelmet", "FLDisguise", "Robe", "Robe_fem").ForEach((Action<Item>) (x => x.Locked = true));
    //    list.RemoveRange(859, 61);

    //    return list;
    //}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicHandler), nameof(MusicHandler.Initialize))]
    private static void LoadMusic()
    {
        Logger.LogInfo("LOADING: Music");
        MusicHandler.m_trackPaths.Add((MusicHandler.MusicTrackID)42, Path.Combine(_contentPath, @"Data\Sounds\Music\Metrolaw.mp3"));
        MusicHandler.m_trackPaths.Add((MusicHandler.MusicTrackID)43, Path.Combine(_contentPath, @"Data\Sounds\Music\FrozenBlood.mp3"));
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(MusicHandler), nameof(MusicHandler.PlayTitleTrack))]
    private static IEnumerable<CodeInstruction> PlayTitleMusic(IEnumerable<CodeInstruction> instructions)
    {
        CodeInstruction musicId = instructions.ElementAt(0);
        musicId.opcode = OpCodes.Ldc_I4_S;
        musicId.operand = 42;
        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SoundHandler), nameof(SoundHandler.Load))]
    private static void LoadSounds(GameSFD game)
    {
        Logger.LogInfo("LOADING: Sounds");
        foreach (string data in Directory.GetFiles(Path.Combine(_contentPath, @"Data\Sounds"), "*.sfds"))
        {
            List<string> soundsData = SFDSimpleReader.Read(data);
            foreach (string soundData in soundsData)
            {
                string[] soundFields = [.. SFDSimpleReader.Interpret(soundData)];
                if (soundFields.Length < 3)
                {
                    continue;
                }

                SoundEffect[] sound = new SoundEffect[soundFields.Length - 2];
                float pitch = SFDXParser.ParseFloat(soundFields[1]);

                for (int i = 0; i < sound.Length; i++)
                {
                    string loadPath = Path.Combine(_contentPath, @"Data\Sounds", soundFields[i + 2]);
                    sound[i] = Content.Load<SoundEffect>(loadPath);
                }

                int count = sound.Count(t => t is null);

                if (count > 0)
                {
                    SoundEffect[] extraSounds = new SoundEffect[sound.Length - count];
                    int field = 0;
                    foreach (SoundEffect soundEffect in sound)
                    {
                        if (soundEffect is not null)
                        {
                            extraSounds[field] = soundEffect;
                            field++;
                        }
                    }

                    sound = extraSounds;
                }

                SoundHandler.SoundEffectGroup finalSound = new(soundFields[0], pitch, sound);
                SoundHandler.soundEffects.Add(finalSound);
            }
        }
    }

    /// <summary>
    /// This method is executed whenever we close the game or it crash.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.OnExiting))]
    private static void Dispose() => Logger.LogError("Disposing");

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    private static void LoadFonts() => Logger.LogInfo("LOADING: Fonts");

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    private static IEnumerable<CodeInstruction> LoadFonts(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.operand is null)
            {
                continue;
            }

            if (instruction.operand.Equals("Data\\Fonts\\"))
            {
                instruction.operand = Path.Combine(_contentPath, @"Data\Fonts");
            }
        }

        return instructions;
    }


    /// <summary>
    /// Fix for loading SFR and SFD textures from both paths.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TitleContainer), nameof(TitleContainer.OpenStream))]
    private static bool StreamPatch(string name, ref Stream __result)
    {
        if (name.Contains(@"Content\Data"))
        {
            if (name.EndsWith(".xnb.xnb"))
            {
                name = name.Substring(0, name.Length - 4);
            }
            else
            {
                string wav = Path.ChangeExtension(name, ".wav");
                if (File.Exists(wav))
                {
                    name = wav;
                }

                string png = Path.ChangeExtension(name, ".png");
                if (File.Exists(png))
                {
                    name = png;
                }

                string item = Path.ChangeExtension(name, ".item");
                if (File.Exists(item))
                {
                    name = item;
                }
            }

            __result = File.OpenRead(name);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TileDatabase), nameof(TileDatabase.Load))]
    private static void LoadTiles() => Logger.LogInfo("LOADING: Tiles");

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(TileDatabase), nameof(TileDatabase.Load))]
    private static IEnumerable<CodeInstruction> LoadTiles(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.operand is null)
            {
                continue;
            }

            if (instruction.operand.Equals("Data\\Tiles\\"))
            {
                instruction.operand = Path.Combine(_contentPath, @"Data\Tiles");
            }
            else if (instruction.operand.Equals("Data\\Weapons\\"))
            {
                instruction.operand = Path.Combine(_contentPath, @"Data\Weapons");
                break;
            }
        }

        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorDatabase), nameof(ColorDatabase.Load))]
    private static bool LoadColors(GameSFD game)
    {
        Logger.LogInfo("LOADING: Colors");
        ColorDatabase.LoadColors(game, Path.Combine(_contentPath, @"Data\Colors\Colors"));
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorPaletteDatabase), nameof(ColorDatabase.Load))]
    private static bool LoadColorsPalette(GameSFD game)
    {
        Logger.LogInfo("LOADING: Palettes");
        ColorPaletteDatabase.LoadColorPalettes(game, Path.Combine(_contentPath, @"Data\Colors\Palettes"));
        return false;
    }

    /// <summary>
    /// Load SFR maps into the officials category.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapHandler), nameof(MapHandler.ReadMapInfoFromStorages), [])]
    private static bool LoadMaps(ref List<MapInfo> __result)
    {
        Logger.LogInfo("LOADING: Maps");
        Constants.SetThreadCultureInfo(Thread.CurrentThread);
        List<MapInfo> list = new();
        string[] array =
        [
            Constants.Paths.ContentOfficialMapsPath,
            Constants.Paths.UserDocumentsCustomMapsPath,
            Constants.Paths.UserDocumentsDownloadedMapsPath,
            Constants.Paths.ContentCustomMapsPath,
            Constants.Paths.ContentDownloadedMapsPath,
            _officialsMapsPath
        ];
        HashSet<Guid> loadedMaps = new();
        foreach (string t in array)
        {
            MapHandler.ReadMapInfoFromStorages(list, t, loadedMaps, true);
        }

        if (!string.IsNullOrEmpty(Constants.STEAM_WORKSHOP_FOLDER))
        {
            MapHandler.ReadMapInfoFromStorages(list, Constants.STEAM_WORKSHOP_FOLDER, loadedMaps, false);
        }

        __result = [.. list.OrderBy(m => m.Name)];
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapInfo), nameof(MapInfo.SetFilePathData))]
    private static bool LoadMaps(string pathToFile, MapInfo __instance)
    {
        if (string.IsNullOrEmpty(pathToFile))
        {
            __instance.Folder = "Other";
            return false;
        }

        __instance.FullPathToFile = Path.GetFullPath(pathToFile);
        __instance.FileName = Path.GetFileName(__instance.FullPathToFile);
        string directoryName = Path.GetDirectoryName(__instance.FullPathToFile);
        __instance.SaveDate = DateTime.MinValue;
        __instance.IsSteamSubscription = !string.IsNullOrEmpty(Constants.STEAM_WORKSHOP_FOLDER) && pathToFile.StartsWith(Constants.STEAM_WORKSHOP_FOLDER, StringComparison.InvariantCultureIgnoreCase);
        if (directoryName!.StartsWith(_officialsMapsPath, true, null))
        {
            __instance.Folder = "Official" + directoryName.Substring(_officialsMapsPath.Length);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Textures), nameof(Textures.Load), [])]
    private static void LoadTextures()
    {
        Logger.LogInfo("LOADING: Textures");
        Textures.Load(Path.Combine(_contentPath, @"Data\Images"));
    }

    /// <summary>
    /// Fix for loading SFR and SFD textures from both paths.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants.Paths), nameof(Constants.Paths.GetContentAssetPathFromFullPath))]
    private static bool GetContentAssetPathFromFullPath(string path, ref string __result)
    {
        __result = path;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Items), nameof(Items.Load))]
    private static bool LoadItems(GameSFD game)
    {
        Logger.LogInfo("LOADING: Items");

        Items.m_allItems = [];
        Items.m_allFemaleItems = [];
        Items.m_allMaleItems = [];
        Items.m_slotAllItems = new List<Item>[10];
        Items.m_slotFemaleItems = new List<Item>[10];
        Items.m_slotMaleItems = new List<Item>[10];

        for (int i = 0; i < Items.m_slotAllItems.Length; i++)
        {
            Items.m_slotAllItems[i] = [];
            Items.m_slotFemaleItems[i] = [];
            Items.m_slotMaleItems[i] = [];
        }

        List<string> files = Directory.GetFiles(Path.Combine(_contentPath, @"Data\Items"), "*.item", SearchOption.AllDirectories).ToList();
        List<string> originalItems = Directory.GetFiles(Constants.Paths.GetContentFullPath(@"Data\Items"), "*.item", SearchOption.AllDirectories).ToList();
        foreach (string item in originalItems)
        {
            if (files.TrueForAll(f => Path.GetFileNameWithoutExtension(f) != Path.GetFileNameWithoutExtension(item)))
            {
                files.Add(item);
            }
        }

        foreach (string file in files)
        {
            if (GameSFD.Closing)
            {
                return false;
            }

            Item item = Content.Load<Item>(file);
            if (Items.m_allItems.Any(item2 => item2.ID == item.ID))
            {
                throw new Exception("Can't load items");
            }

            item.PostProcess();
            Items.m_allItems.Add(item);
            Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        }

        Items.PostProcessGenders();
        Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        Player.HurtLevel2 = Items.GetItem("HurtLevel2") ?? Player.HurtLevel1;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor, [])]
    private static void Init(GameSFD __instance) => __instance.Window.Title = $"Superfighters Redux {Globals.SFRVersion}";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static bool LoadCustomAnimations(ref bool __result)
    {
        Logger.LogInfo("LOADING: Custom Animations");
        Animations.Data = AnimationHandler.LoadAnimationsDataPipeline(Path.Combine(_contentPath, @"Data\Animations"));
        __result = true;

        return false;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static void EditAnimations()
    {
        AnimationsData data = Animations.Data;
        AnimationData[] anims = data.Animations;

        List<AnimationData> customData = AnimHandler.GetAnimations(data);
        Array.Resize(ref anims, data.Animations.Length + customData.Count);
        for (int i = 0; i < customData.Count; i++)
        {
            anims[anims.Length - 1 - i] = customData[i];
        }

        data.Animations = anims;
        AnimationsData animData = new(data.Animations);
        Animations.Data = animData;
    }
}