using System;
using System.Collections.Generic;
using System.Net;
using TicTakGame.Net.Enum;
using TicTakGame.Utils;

namespace TicTakGame.Net.Packet
{
    class Player : IPacket
    {
        const byte size = 1 + 1 + 16 + 4;
        public const byte IdentifierByte = 0xF2;

        private Role _role = Role.Null;
        public Role role
        {
            set { if (_role == Role.Null) { _role = value; } else { throw new Exception("Player role can set only once."); } }
            get => _role;
        }

        public readonly Guid id;
        public readonly System.Net.IPAddress iPAddress;

        public Player(IPAddress iPAddress)
        {
            this.iPAddress = iPAddress;

            this.id = Guid.NewGuid();
        }

        private Player(Role role, Guid id, IPAddress iPAddress)
        {
            this.role = role;
            this.id = id;
            this.iPAddress = iPAddress;
        }

        public static Player fromBytes(byte[] bytes)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            memoryStream.Write(bytes);
            memoryStream.Position = 0;

            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(memoryStream);

            if (binaryReader.ReadByte() != IdentifierByte)
                throw new Exception("Invalid Packet identifier.");

            return new Player(
                binaryReader.ReadEnum<Role, byte>(),
                new Guid(binaryReader.ReadBytes(16)),
                new IPAddress(binaryReader.ReadBytes(4))
            );
        }

        public override byte[] toBytes()
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(memoryStream);

            binaryWriter.Write(IdentifierByte);
            binaryWriter.Write<Role>(role);
            binaryWriter.Write(id.ToByteArray());
            binaryWriter.Write(iPAddress.GetAddressBytes());

            return memoryStream.ToArray();
        }

        public bool isX() => role == Role.X;


        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                return this.id == ((Player)obj).id;
            }
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }

    class HandShakeResult : IPacket
    {
        public const int Size = 1 + 1 + ((22 + 1) * 2) + 16;
        public const byte IdentifierByte = 0xD1;

        public class Builder
        {
            private List<Player> players = new List<Player>(2);

            public Builder addPlayer(Player player)
            {
                if (players.Count < 2)
                {
                    players.Add(player);
                }
                else throw new Exception("Not enough space on list. Out of range players count " + players.Count);

                return this;
            }

            public HandShakeResult build()
            {
                Random random = new Random();

                // Set roles
                players[0].role = EnumHelper<Role,byte>.GetEnum(random.Next(1,2));
                players[1].role = players[0].role == Role.X ? Role.O : Role.X;

                // Pick random player for first turn
                return new HandShakeResult(this.players, players[random.Next(0, players.Count)].id);
            }
        }

        public readonly List<Player> players;
        public readonly Guid firstTurnPlayerId;

        private HandShakeResult(List<Player> players, Guid firstTurnPlayerId)
        {
            this.players = players;
            this.firstTurnPlayerId = firstTurnPlayerId;
        }

        public override byte[] toBytes()
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(memoryStream);

            binaryWriter.Write(IdentifierByte);
            binaryWriter.Write((byte)players.Count);

            for (int i = 0; i < players.Count; i++)
            {
                byte[] b = players[i].toBytes();
                binaryWriter.Write((byte)b.Length);
                binaryWriter.Write(b);
            }

            binaryWriter.Write(this.firstTurnPlayerId.ToByteArray());

            return memoryStream.ToArray();
        }

        public static HandShakeResult fromBytes(byte[] bytes)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(memoryStream);

            memoryStream.Write(bytes);
            memoryStream.Position = 0;

            if (binaryReader.ReadByte() != IdentifierByte)
                throw new Exception("Invalid Packet identifier.");

            int playerCount = binaryReader.ReadByte();
            List<Player> players = new List<Player>(playerCount);

            for (int i = 0; i < playerCount; i++)
            {
                players.Add(Player.fromBytes(binaryReader.ReadBytes(binaryReader.ReadByte())));
            }

            Guid firstTurnPlayerId = new Guid(binaryReader.ReadBytes(16));

            return new HandShakeResult(players, firstTurnPlayerId);
        }
    }
    public class GamePacket : IPacket
    {
        private const byte PacketIdentifier = 0xD3;
        public const int Size = 17;

        public readonly Guid playerId;
        public readonly byte cellIndex;

        public GamePacket(Guid playerId, byte cellIndex)
        {
            this.playerId = playerId;
            this.cellIndex = cellIndex;
        }

        public static GamePacket FromBytes(byte[] bytes)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(memoryStream);

            memoryStream.Write(bytes);
            memoryStream.Position = 0;

            return new GamePacket(
                new Guid(binaryReader.ReadBytes(16)),
                binaryReader.ReadByte()
            );
        }

        public override byte[] toBytes()
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(memoryStream);

            binaryWriter.Write(playerId.ToByteArray());
            binaryWriter.Write(cellIndex);

            return memoryStream.ToArray();
        }
    }
}
