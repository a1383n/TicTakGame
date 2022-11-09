using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TicTakGame.Net.Discovery
{
    class DiscoveryService
    {
        public const int Port = 58110;

        private Thread replayThred;

        private IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
        private UdpClient udpClient;

        private bool _IsDiscoverable;
        public bool IsDiscoverable { get { return _IsDiscoverable; }  }

        public Status status = Status.Online;

        public string displayName = null;

        private void DumpDiscoveryPacket()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = udpClient.ReceiveAsync().Result;

                    // Note: IPEndPoint was created with remote address and our local port
                    ReplayDiscoveryPacket(Packet.FromBytes(result.Buffer), new IPEndPoint(result.RemoteEndPoint.Address, Port));
                }
                catch (Expention.UnexpectedPackteRecevied) { }
            }
        }

        private void ReplayDiscoveryPacket(Packet packet,IPEndPoint endPoint)
        {
            UdpClient callBackClient = new UdpClient();

            byte[] replay = new Packet
            {
                type = Type.DiscoveryReplay,
                status = this.status,
                extra =
                {
                    { "client_name" , System.Environment.MachineName },
                    { "display_name",  null },
                }
            }.ToBytes();

            callBackClient.Send(replay, replay.Length, endPoint);
        }


        public void LisenForDiscoveryPacket()
        {
            _IsDiscoverable = true;
            udpClient = new UdpClient(iPEndPoint);
            replayThred = new Thread(new ThreadStart(this.DumpDiscoveryPacket));
            replayThred.Start();
        }

        private void StopLisenForDiscoveyPacket()
        {
            _IsDiscoverable = false;
            replayThred.Abort();
            udpClient.Close();
        }

    }
}
