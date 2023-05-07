using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Net.Exceptions
{
    public sealed class UnexpectedPacketReceived : SystemException
    {
        public UnexpectedPacketReceived(String packetType) : base($"Unexpected Packet was received. Should received packet with type of {packetType}") { }
    }
}
