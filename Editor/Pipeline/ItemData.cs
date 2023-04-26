using Microsoft.Xna.Framework.Graphics;

namespace Editor.Pipeline;

public class ItemData
{
    public readonly int[] Parts;
    public bool CanEquip = true;
    public bool CanScript = true;
    public string ColorPalette;
    public int EquipmentLayer = -1;
    public string GameName;
    public string Id;
    public Texture2D Image;
    public bool JacketUnderBelt;
    public string Name;
    public int TileHeight = 0;

    public int TileWidth = 0;

    public ItemData()
    {
        Parts = new int[6];
        for (int i = 0; i < Parts.Length; i++)
        {
            Parts[i] = 0;
        }

        // Image = null;
        // ColorPalette = string.Empty;
        // EquipmentLayer = -1;
        // TileHeight = 0;
        // TileWidth = 0;
        // CanEquip = true;
        // CanScript = true;
    }

    public bool IsInvalid() => TileHeight == 0 || TileWidth == 0 || EquipmentLayer == -1;
}