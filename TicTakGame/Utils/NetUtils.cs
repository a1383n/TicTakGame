using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TicTakGame.Utils
{
    class NetUtils
    {
        public static NetworkInterface getPrimeryNetworkInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces()[NetworkInterface.LoopbackInterfaceIndex];
        }

        public static IPAddress getIPAddress(NetworkInterface network)
        {
            return network.GetIPProperties().UnicastAddresses[1].Address;
        }
    }
}
