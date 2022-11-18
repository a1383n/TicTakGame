using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Net
{
    interface PacketTemplate
    {
        abstract public byte[] toBytes();
    }
}
