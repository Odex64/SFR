using System;
using System.Collections.Generic;
using Editor.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Content;

public class ItemPart
{
    public const int TotalRowsInItems = 6;
    public static int CachedTextureCount;

    public ItemPart(Texture2D[] textures, int type)
    {
        Textures = textures;
        Type = type;
        IsDisposed = false;
    }

    public int Type { get; }

    // public Texture2D[] Textures { get; private set; }
    public Texture2D[] Textures { get; }

    public bool IsDisposed { get; private set; }

    public Dictionary<string, Texture2D[]> ColoredTextures { get; } = new(24);

    public static int GlobalIndexToType(int globalPartIndex)
    {
        if (globalPartIndex >= 0)
        {
            return globalPartIndex / 50;
        }

        return -(-globalPartIndex / 50 + 1);
    }

    public static int GlobalIndexToLocalIndex(int globalPartIndex) => Math.Abs(globalPartIndex % 50);

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }

    public static Texture2D RecolorTexture(Texture2D texture, string[] colors, bool waitForGraphicsDevice = true)
    {
        var color = ColorDatabase.GetColor(colors[0]);
        var color2 = ColorDatabase.GetColor(colors[1]);
        var color3 = ColorDatabase.GetColor(colors[2]);
        Color[][] array = { color, color2, color3 };
        // var array2 = texture.GetData();
        // var array3 = texture.GetData();
        var array2 = new Color[texture.Width * texture.Height];
        var array3 = new Color[texture.Width * texture.Height];
        texture.GetData(array2);
        texture.GetData(array3);
        int[] array4 = { 255, 192, 128, 64, 32 };
        for (int i = 0; i < array.Length; i++)
        {
            var array5 = array[i];
            if (array5 != null)
            {
                int num = Math.Min(array5.Length, array4.Length);
                for (int j = 0; j < num; j++)
                {
                    for (int k = 0; k < array2.Length; k++)
                    {
                        var color4 = array3[k];
                        if (i == 0)
                        {
                            if (color4.R == array4[j] && color4.G == 0 && color4.B == 0)
                            {
                                array2[k] = array5[j];
                            }
                        }
                        else if (i == 1)
                        {
                            if (color4.R == 0 && color4.G == array4[j] && color4.B == 0)
                            {
                                array2[k] = array5[j];
                            }
                        }
                        else if (i == 2 && color4.R == 0 && color4.G == 0 && color4.B == array4[j])
                        {
                            array2[k] = array5[j];
                        }
                    }
                }
            }
        }

        // if (waitForGraphicsDevice)
        // {
        //     Utils.WaitForGraphicsDevice();
        // }

        // Texture2D texture2D = Utils.NewTexture2D(GameSFD.Handle.GraphicsDevice, texture.Width, texture.Height);
        // texture2D.SetData<Color>(array2);
        texture.SetData(array2);
        return texture;
    }

    // public Bitmap GetTexture(int index, string[] colors, bool waitForGraphicsDevice = true)
    // {
    //     string text = string.Concat(index.ToString(), "_", colors[0], "_", colors[1], "_", colors[2]);
    //     if (!coloredTextures.ContainsKey(text))
    //     {
    //         var array = new Bitmap[Textures.Length];
    //         int i = 0;
    //         while (i < Textures.Length)
    //         {
    //             if (Textures[i] != null)
    //             {
    //                 lock (GameSFD.SpriteBatchResourceObject)
    //                 {
    //                     array[i] = SFD.Tiles.Textures.RecolorTexture(Textures[i], colors, waitForGraphicsDevice);
    //                     goto IL_A9;
    //                 }
    //
    //                 goto IL_A5;
    //             }
    //
    //             goto IL_A5;
    //             IL_A9:
    //             i++;
    //             continue;
    //             IL_A5:
    //             array[i] = null;
    //             goto IL_A9;
    //         }
    //
    //         Interlocked.Increment(ref CachedTextureCount);
    //         coloredTextures.Add(text, array);
    //     }
    //
    //     return coloredTextures[text][index];
    // }

    public Texture2D GetTexture(int localPartIndex) => Textures[localPartIndex];

    public int GetGlobalIndex(int localPartIndex) => Type * 50 + localPartIndex;

    public struct TYPE
    {
        public const int SHEATHED_MELEE = 9;
        public const int SHEATHED_RIFLE = 8;
        public const int SHEATHED_HANDGUN = 7;
        public const int WPN_MAINHAND = 6;
        public const int WPN_OFFHAND = 5;
        public const int TAIL = 4;
        public const int SUBANIMATION = 10;
        public const int PART_RANGE = 50;
    }
}