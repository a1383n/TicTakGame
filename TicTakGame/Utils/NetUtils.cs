using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TicTakGame.Utils
{
    class NetUtils
    {
        public static IPAddress getLocalIPAddress()
        {
            IPAddress[] iPs =  Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            for (int i = 0; i < iPs.Length; i++)
            {
                IPAddress iPAddress = iPs[i];
                if (iPAddress.GetAddressBytes().Length == 4)
                    return iPAddress;
            }

            throw new Exception("Unable to find your ip address");
        }
    }
}
