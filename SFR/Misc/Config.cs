using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using HarmonyLib;
using SFD;
using Color = Microsoft.Xna.Framework.Color;

namespace SFR.Misc;

[HarmonyPatch()]
internal static class Config
{
    private static Dictionary<string, string> _content;
    public static Color MenuBorderColor = Constants.COLORS.MENU_BLUE;

    public static void Init()
    {
        if (_content is not null) return;

        string configPath = Path.Combine(Constants.Paths.UserDocumentsSFDUserDataPath, "sfr.ini");
        bool mustReset = !File.Exists(configPath);
        if (mustReset)
        {
            using (_ = File.CreateText(configPath)) { };
        }

        _content = GetConfigData(configPath);

        if (!mustReset)
        {
            mustReset = !_content.ContainsKey("MenuColor");
        }

        if (mustReset)
        {
            DefaultSettings(configPath);
            return;
        }

        var menuBorderColor = ColorTranslator.FromHtml(_content["MenuColor"]);
        MenuBorderColor = new Color(menuBorderColor.R, menuBorderColor.G, menuBorderColor.B, menuBorderColor.A);

        ApplySettings();
    }

    private static void ApplySettings() => Constants.COLORS.MENU_BLUE = MenuBorderColor;

    private static void DefaultSettings(string path)
    {
        _content = new Dictionary<string, string>()
        {
            {
                "MenuColor", MenuBorderColor.ToHex()
            }
        };

        WriteConfigData(path, _content);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFDConfig), nameof(SFDConfig.LoadConfig))]
    private static void LoadConfig() => Init();

    private static Dictionary<string, string> GetConfigData(string fileName)
    {
        Dictionary<string, string> data = [];
        foreach (string line in File.ReadLines(fileName))
        {
            if (string.IsNullOrEmpty(line)) continue;

            string[] lineData = line.Split('=');
            data.Add(lineData[0], lineData[1]);
        }

        return data;
    }

    private static void WriteConfigData(string fileName, Dictionary<string, string> data) => File.WriteAllLines(fileName, data.Select(z => $"{z.Key}={z.Value}{Environment.NewLine}"));
}
