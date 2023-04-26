using System;
using System.IO;
using Editor.Colors;
using Editor.Misc;
using Microsoft.Xna.Framework;

namespace Editor;

internal class Program : Game
{
    public static bool HasRedux;
    public static string GameDirectory = string.Empty;

    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    public Program()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);

        // Typically you would load a config here...
        _graphicsDeviceManager.PreferredBackBufferWidth = 1280;
        _graphicsDeviceManager.PreferredBackBufferHeight = 720;
        _graphicsDeviceManager.IsFullScreen = false;
        _graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
    }

    [STAThread]
    private static void Main()
    {
        if (!File.Exists("config.ini"))
        {
            throw new Exception("No config.ini found!");
        }

        string[] lines = File.ReadAllLines("config.ini");
        foreach (string line in lines)
        {
            int splitPosition = line.IndexOf('=');
            string key = line.Substring(0, splitPosition);
            switch (key)
            {
                case "SFD_DIRECTORY":
                    GameDirectory = line.Substring(splitPosition + 1);
                    break;
            }
        }

        if (!Directory.Exists(GameDirectory))
        {
            throw new Exception("SFD directory not found!");
        }

        HasRedux = Directory.Exists(Path.Combine(GameDirectory, "SFR"));

        // Harmony.PatchAll();

        // var form = new Form();
        // var game = new Game(form);

        // form.Show();
        // game.Run();
        // var game 
        // Application.SetCompatibleTextRenderingDefault(false);
        // Application.Run(new Form());
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        ColorDatabase.Load(HasRedux ? Path.Combine(GameDirectory, "SFR/Content/Data/Colors/Colors/ItemColors.sfdx") : Path.Combine(GameDirectory, "Content/Data/Colors/Colors/ItemColors.sfdx"));
        ColorPaletteDatabase.Load(HasRedux ? Path.Combine(GameDirectory, "SFR/Content/Data/Colors/Palettes/ItemPalettes.sfdx") : Path.Combine(GameDirectory, "Content/Data/Colors/Palettes/ItemPalettes.sfdx"));
        Items.Load(this, GameDirectory);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}