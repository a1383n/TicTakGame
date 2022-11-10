using System;
using System.Collections.Generic;

using System.Text.Json;
using TicTakGame.Utils;

namespace TicTakGame.Net.Discovery
{
    enum Type : ushort
    {
        Discovery,
        DiscoveryReplay,
        GameRequest,
        GameRequestReplay
    }

    enum Status : ushort
    {
        Online,
        InGame
    }

    class Packet
    {
        public System.Net.IPEndPoint sourceEndPoint;

        public Type type;
        public Status status;

        public const byte PacketIdentifier = 0x8C;

        public Dictionary<string, string> extra = new Dictionary<string, string>();

        public Device GetDevice(System.Net.IPEndPoint remoteEndPoint)
        {
            return new Device
            {
                iP = remoteEndPoint.Address,
                clienName = extra["client_name"],
            };
        }

        public static Packet FromBytes(byte[] bytes)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Position = 0;

            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(memoryStream);

            if (binaryReader.ReadByte() != PacketIdentifier)
            {
                throw new Net.Expention.UnexpectedPackteRecevied("Discovery");
            }

            return new Packet
            {
                type = EnumHelper<Type, ushort>.GetEnum(binaryReader.ReadUInt16()),
                status = EnumHelper<Status, ushort>.GetEnum(binaryReader.ReadUInt16().ToString()),
                extra = JsonSerializer.Deserialize<Dictionary<string, string>>(binaryReader.ReadString() ?? "[]")
            };
        }


        public byte[] ToBytes()
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(memoryStream);

            binaryWriter.Write(PacketIdentifier);
            binaryWriter.Write(EnumHelper<Type,ushort>.GetValueByEnum(type));
            binaryWriter.Write(EnumHelper<Status, ushort>.GetValueByEnum(status));
            binaryWriter.Write(JsonSerializer.Serialize(extra));

            return memoryStream.ToArray();
        }
    }
}
