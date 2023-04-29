using Lidgren.Network;
using SFD;

namespace SFR.Sync.Generic;

internal static class GenericServerData
{
    public static NET_DELIVERY Delivery = new(NetDeliveryMethod.ReliableOrdered, 31);

    public static GenericData Read(NetIncomingMessage incomingMessage)
    {
        var type = (DataType)incomingMessage.ReadInt32();

        byte flagsLength = incomingMessage.ReadByte();
        var flags = new SyncFlags[flagsLength];
        for (int i = 0; i < flagsLength; i++)
        {
            flags[i] = (SyncFlags)incomingMessage.ReadInt32();
        }

        byte dataLength = incomingMessage.ReadByte();
        object[] data = new object[dataLength];

        for (int i = 0; i < dataLength; i++)
        {
            switch (incomingMessage.ReadRangedInteger(0, 3))
            {
                case 0:
                    data[i] = incomingMessage.ReadBoolean();
                    break;
                case 1:
                    data[i] = incomingMessage.ReadInt32();
                    break;
                case 2:
                    data[i] = incomingMessage.ReadSingle();
                    break;
                case 3:
                    data[i] = incomingMessage.ReadString();
                    break;
            }
        }

        return new GenericData(type, flags, data);
    }

    public static NetOutgoingMessage Write(GenericData genericData, NetOutgoingMessage netOutgoingMessage)
    {
        NetMessage.WriteDataType((MessageType)63, netOutgoingMessage);

        int count = 0;
        foreach (object iter in genericData.Args)
        {
            if (iter is object[] arr)
            {
                count += arr.Length;
            }
            else
            {
                count++;
            }
        }

        netOutgoingMessage.Write((int)genericData.Type);

        netOutgoingMessage.Write((byte)genericData.Flags.Length);
        foreach (var flag in genericData.Flags)
        {
            netOutgoingMessage.Write((int)flag);
        }

        netOutgoingMessage.Write((byte)count);
        foreach (object data in genericData.Args)
        {
            if (data is object[] arr)
            {
                foreach (object innerData in arr)
                {
                    WriteMessage(innerData, netOutgoingMessage);
                }
            }
            else
            {
                WriteMessage(data, netOutgoingMessage);
            }
        }

        return netOutgoingMessage;
    }

    private static bool WriteMessage(object data, NetOutgoingMessage netOutgoingMessage)
    {
        switch (data)
        {
            case bool parsedBool:
                netOutgoingMessage.WriteRangedInteger(0, 3, 0);
                netOutgoingMessage.Write(parsedBool);
                return true;
            case int parsedInt:
                netOutgoingMessage.WriteRangedInteger(0, 3, 1);
                netOutgoingMessage.Write(parsedInt);
                return true;
            case float parsedFloat:
                netOutgoingMessage.WriteRangedInteger(0, 3, 2);
                netOutgoingMessage.Write(parsedFloat);
                return true;
            case string parsedString:
                netOutgoingMessage.WriteRangedInteger(0, 3, 3);
                netOutgoingMessage.Write(parsedString);
                return true;
        }

        return false;
    }
}