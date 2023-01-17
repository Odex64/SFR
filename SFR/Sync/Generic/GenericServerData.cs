using Lidgren.Network;
using SFD;

namespace SFR.Sync.Generic;

internal static class GenericServerData
{
    public static NET_DELIVERY Delivery = new(NetDeliveryMethod.ReliableOrdered, 31);

    public static GenericData Read(NetIncomingMessage incomingMessage)
    {
        var type = (DataType)incomingMessage.ReadInt32();
        byte length = incomingMessage.ReadByte();
        object[] data = new object[length];

        for (int i = 0; i < length; i++)
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

        return new GenericData(type, data);
    }

    public static NetOutgoingMessage Write(GenericData genericData, NetOutgoingMessage netOutgoingMessage)
    {
        NetMessage.WriteDataType((MessageType)63, netOutgoingMessage);

        netOutgoingMessage.Write((int)genericData.Type);
        netOutgoingMessage.Write((byte)genericData.Args.Length);
        foreach (object data in genericData.Args)
        {
            switch (data)
            {
                case bool parsedBool:
                    netOutgoingMessage.WriteRangedInteger(0, 3, 0);
                    netOutgoingMessage.Write(parsedBool);
                    break;
                case int parsedInt:
                    netOutgoingMessage.WriteRangedInteger(0, 3, 1);
                    netOutgoingMessage.Write(parsedInt);
                    break;
                case float parsedFloat:
                    netOutgoingMessage.WriteRangedInteger(0, 3, 2);
                    netOutgoingMessage.Write(parsedFloat);
                    break;
                case string parsedString:
                    netOutgoingMessage.WriteRangedInteger(0, 3, 3);
                    netOutgoingMessage.Write(parsedString);
                    break;
            }
        }

        return netOutgoingMessage;
    }
}