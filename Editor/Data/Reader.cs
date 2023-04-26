using System;
using System.Collections.Generic;
using Editor.Colors;

namespace Editor.Data;

public static class Reader
{
    public static void ReadDataFromFile(string file)
    {
        try
        {
            var list = Parser.ReadData(file);
            ProcessData(list);
        }
        catch (Exception ex)
        {
            throw new Exception("Error: reading file '" + file + "' failed\r\n" + ex);
        }
    }

    // Token: 0x060055AC RID: 21932 RVA: 0x0017D994 File Offset: 0x0017BB94
    private static void ProcessData(List<DataNode> dataNodes)
    {
        try
        {
            foreach (var dataNode in dataNodes)
            {
                if (dataNode.Value == string.Empty)
                {
                    throw new Exception("Error: reader received empty value for property '" + dataNode.Property + "'");
                }

                switch (dataNode.Property.ToUpperInvariant())
                {
                    case "COLOR":
                        ColorDatabase.ConstructColor(dataNode.ChildNodes, dataNode.Value);
                        continue;

                    case "COLORPALETTE":
                        ColorPaletteDatabase.ConstructColorPalette(dataNode.ChildNodes, dataNode.Value);
                        continue;
                }

                throw new Exception("Error: reader received unexpected property '" + dataNode.Property + "'");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error: reader failed\r\n" + ex);
        }
    }
}