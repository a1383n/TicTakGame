using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TicTakGame.Net.Discovery
{
    internal class Device : IEquatable<Device>
    {
        public IPAddress iP;

        public string clienName, displayName;
        public Status status;

        bool IEquatable<Device>.Equals(Device other)
        {
            return this.iP.Equals(other.iP);
        }

        public override bool Equals(object obj)
        {
            return this.iP.Equals(((Device)obj).iP);
        }

        public override int GetHashCode()
        {
            return iP.GetHashCode();
        }
    }

    class DiscoveryService
    {
        public const int Port = 58110;

        private Thread replayThred;

        private UdpClient udpClient;

        private bool _IsDiscoverable;
        public bool IsDiscoverable { get { return _IsDiscoverable; }  }

        public Status status = Status.Online;

        public string displayName = null;

        public List<Device> devices = new List<Device>();

        private void DumpDiscoveryPacket()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = udpClient.ReceiveAsync().Result;
                    Packet packet = Packet.FromBytes(result.Buffer);

                    if (devices.IndexOf(packet.GetDevice()) == -1)
                        devices.Add(packet.GetDevice());

                    // Note: IPEndPoint was created with remote address and our local port
                    ReplayDiscoveryPacket(packet, new IPEndPoint(result.RemoteEndPoint.Address, Port));
                }
                catch (Expention.UnexpectedPackteRecevied) { }
            }
        }

        private void ReplayDiscoveryPacket(Packet packet,IPEndPoint endPoint)
        {
            UdpClient callBackClient = new UdpClient();
            byte[] replay = null;

            switch (packet.type)
            {
                case Type.Discovery:
                    replay = new Packet
                    {
                        type = Type.DiscoveryReplay,
                        status = this.status,
                        extra =
                        {
                            { "client_name" , System.Environment.MachineName },
                            { "display_name",  this.displayName },
                        }
                    }.ToBytes();
                    break;
            }

            if (replay != null)
                callBackClient.Send(replay, replay.Length, endPoint);
        }


        public void LisenForDiscoveryPacket()
        {
            _IsDiscoverable = true;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
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

        private void SendBrodcastDiscoveryPacket()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);
            UdpClient udpClient = new UdpClient(iPEndPoint);
            udpClient.EnableBroadcast = true;

            byte[] bytes = new Packet {
                status = this.status,
                type = Type.Discovery,
                extra =
                {
                    { "client_name" , System.Environment.MachineName },
                    { "display_name",  this.displayName },
                }
            }.ToBytes();

            udpClient.Send()
        }

        public void DiscoverAround()
        {

        }
    }
}
