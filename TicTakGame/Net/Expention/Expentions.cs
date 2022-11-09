using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Net.Expention
{
    public sealed class UnexpectedPackteRecevied : SystemException
    {
        public UnexpectedPackteRecevied(String packetType) : base($"Unexpected Packet was recived. Should recived packet with type of {packetType}") { }
    }
}
