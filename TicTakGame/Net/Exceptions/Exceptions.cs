using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Net.Exceptions
{
    public sealed class UnexpectedPacketReceived : SystemException
    {
        public UnexpectedPacketReceived(String packetType) : base($"Unexpected Packet was recived. Should recived packet with type of {packetType}") { }
    }
}
