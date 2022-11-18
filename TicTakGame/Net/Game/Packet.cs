using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TicTakGame.Net.Game
{
    public class Packet : PacketTemplate
    {
        public byte playerRole;
        public byte selectedBlock;
        
        public static Packet FromBytes(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Position = 0;
            BinaryReader binaryReader = new BinaryReader(memoryStream);

            return new Packet {
                playerRole = binaryReader.ReadByte(),
                selectedBlock = binaryReader.ReadByte()
            };
        }

        public byte[] toBytes()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(playerRole);
            binaryWriter.Write(selectedBlock);

            return memoryStream.ToArray();
        }
    }
}
