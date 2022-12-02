using System;
using System.IO;

namespace TicTakGame.Net.Packet
{
    public abstract class IPacket {
      //  public abstract int size {get;}
      //  public abstract byte identifierByte {get;}
        public abstract byte[] toBytes();
    }
}