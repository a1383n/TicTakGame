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

        public string clienName;
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

        private void log(string s)
        {
            System.IO.File.AppendAllText(@"C:\Users\amirs\Desktop\tiktok.log",s + "\n");
        }

        private async System.Threading.Tasks.Task<Device> DumpDiscoveryPacket()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    Packet packet = Packet.FromBytes(result.Buffer);
                    Device device = packet.GetDevice(result.RemoteEndPoint);

                    if (device.clienName == Environment.MachineName)
                        continue;
                    else if (devices.IndexOf(device) == -1)
                    {
                        log($"Packet from {result.RemoteEndPoint} recived. {packet.type}");
                        devices.Add(device);
                    }

                    // Note: IPEndPoint was created with remote address and our local port
                    ReplayDiscoveryPacket(packet, new IPEndPoint(result.RemoteEndPoint.Address, Port));

                   // return device;
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
            {
                log($"Packet to {endPoint.ToString()} was sent. {packet.type.ToString()}");
                callBackClient.Send(replay, replay.Length, endPoint);
            }
        }


        public System.Threading.Tasks.Task<Device> LisenForDiscoveryPacket()
        {
            _IsDiscoverable = true;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
            udpClient = new UdpClient(iPEndPoint);

            return DumpDiscoveryPacket();
        }

        public void StopLisenForDiscoveyPacket()
        {
            _IsDiscoverable = false;
            replayThred.Abort();
            udpClient.Close();
        }

        private void SendBrodcastDiscoveryPacket()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            byte[] bytes = new Packet {
                status = this.status,
                type = Type.Discovery,
                extra =
                {
                    { "client_name" , System.Environment.MachineName }
                }
            }.ToBytes();

            udpClient.Send(bytes,bytes.Length,iPEndPoint);
        }

        public void DiscoverAround()
        {
            SendBrodcastDiscoveryPacket();
        }

        public EventHandler NewClientFound;


    }
}
