using Editor.Content;
using Editor.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Pipeline;

public class ItemContentReader : ContentTypeReader<Item>
{
    protected override Item Read(ContentReader input, Item existingInstance)
    {
        var graphicsDevice = GetGraphicsDevice(input.ContentManager);
        // GraphicsDevice graphicsDevice = Game.GetGraphicsDevice();
        string text = input.ReadString();
        string text2 = input.ReadString();
        int num = input.ReadInt32();
        string text3 = input.ReadString();
        bool flag = input.ReadBoolean();
        bool flag2 = input.ReadBoolean();
        bool flag3 = input.ReadBoolean();
        string text4 = input.ReadString();
        int num2 = input.ReadInt32();
        int num3 = input.ReadInt32();
        int num4 = input.ReadByte();
        for (int i = 0; i < num4; i++)
        {
            Items.DynamicColorTable.Add(new Color
            {
                PackedValue = input.ReadUInt32()
            });
        }

        int num5 = input.ReadInt32();
        input.ReadChar();
        var array = new ItemPart[num5];
        for (int j = 0; j < num5; j++)
        {
            int num6 = input.ReadInt32();
            int num7 = input.ReadInt32();
            int num8 = num2 * num3;
            var array2 = new Texture2D[num7];
            for (int k = 0; k < num7; k++)
            {
                bool flag4 = input.ReadBoolean();
                if (flag4)
                {
                    var color = default(Color);
                    var array3 = new Color[num8];
                    for (int l = 0; l < num8; l++)
                    {
                        if (input.ReadBoolean())
                        {
                            array3[l] = new Color(color.R, color.G, color.B, color.A);
                        }
                        else
                        {
                            byte b = input.ReadByte();
                            // array3[l] = dynamicColorTable.GetColor((int)b);
                            array3[l] = Items.DynamicColorTable[b];
                            color = array3[l];
                        }
                    }

                    input.ReadChar();
                    // array2[k] = Utils.NewTexture2D(graphicsDevice, num2, num3);
                    array2[k] = new Texture2D(graphicsDevice, num2, num3);
                    array2[k].SetData(array3);
                }
                else
                {
                    array2[k] = null;
                }
            }

            array[j] = new ItemPart(array2, num6);
        }

        return new Item(array, text2, text, num, text3, flag, flag2, flag3, text4);
    }

    public override string ToString() => "SFD.Co";

    private GraphicsDevice GetGraphicsDevice(ContentManager contentManager)
    {
        var graphicsDeviceService = (IGraphicsDeviceService)contentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
        if (graphicsDeviceService == null)
        {
            throw new ContentLoadException("No GraphicsDevice");
        }

        var graphicsDevice = graphicsDeviceService.GraphicsDevice;
        if (graphicsDevice == null)
        {
            throw new ContentLoadException("No GraphicsDevice");
        }

        return graphicsDevice;
    }
}