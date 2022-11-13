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

        public string clientName;

        public Device GetDevice(System.Net.IPEndPoint remoteEndPoint)
        {
            return new Device
            {
                iP = remoteEndPoint.Address,
                clienName = this.clientName
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
                type = EnumHelper<Type, ushort>.GetEnum(binaryReader.ReadByte()),
                status = EnumHelper<Status, ushort>.GetEnum(binaryReader.ReadByte()),
                clientName = binaryReader.ReadString()
            };
        }


        public byte[] ToBytes()
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(memoryStream);

            binaryWriter.Write(PacketIdentifier);
            binaryWriter.Write((byte) EnumHelper<Type,ushort>.GetValueByEnum(type));
            binaryWriter.Write((byte) EnumHelper<Status, ushort>.GetValueByEnum(status));
            binaryWriter.Write(clientName);

            return memoryStream.ToArray();
        }
    }
}
