using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TicTakGame.Net.Packet;
using TicTakGame.Net.Enum;

namespace TicTakGame.Net.Game
{
    public class GameService
    {
        public class Builder
        {
            private Builder() { }

            public static GameService createServer(int port = 0)
            {
                return new GameService(new TcpListener(new IPEndPoint(IPAddress.Any, port)));
            }

            public static GameService createClient(IPEndPoint severEndPoint)
            {
                return new GameService(severEndPoint);
            }

            public static GameService fake()
            {
                return new GameService(new IPEndPoint(IPAddress.None, 0));
            }
        }

        public TcpClient client;
        private TcpListener server;
        public IPEndPoint serverEndpoint;
        public bool isServer { private set; get; }
        public ServiceStatus status { private set; get; } = ServiceStatus.IDLE;
        public System.Threading.CancellationTokenSource cancellationToken = new System.Threading.CancellationTokenSource();
        public delegate void GameServiceEvent(ServiceStatus status);
        public event GameServiceEvent ServiceStatusChanged;
        private NetworkStream networkStream;

        private GameService(IPEndPoint endPoint)
        {
            this.client = new TcpClient();
            serverEndpoint = endPoint;

            isServer = false;
        }

        private GameService(TcpListener listener)
        {
            server = listener;
            server.Start();

            serverEndpoint = (IPEndPoint)server.LocalEndpoint;
            isServer = true;
        }

        private void changeStatus(ServiceStatus status)
        {
            this.status = status;

            if (ServiceStatusChanged != null)
                ServiceStatusChanged(status);
        }

        public async Task connectOrAccept()
        {
            if (client == null || !client.Connected)
            {
                if (isServer)
                {
                    changeStatus(ServiceStatus.WaitingForConnection);
                    client = await server.AcceptTcpClientAsync();
                    networkStream = client.GetStream();
                    changeStatus(ServiceStatus.ClientConnected);
                }
                else
                {
                    changeStatus(ServiceStatus.Connecting);
                    await client.ConnectAsync(serverEndpoint.Address, serverEndpoint.Port);
                    networkStream = client.GetStream();
                    changeStatus(ServiceStatus.ClientConnected);
                }

                client.SendTimeout = -1;
                client.ReceiveTimeout = -1;
            }
        }


        public void Dispose()
        {
            if (server != null)
                server.Stop();

            if (client != null)
                client.Dispose();

            changeStatus(ServiceStatus.IDLE);
        }

        public async Task<byte[]> getPacket(int size)
        {
            if (client.Connected)
            {
                byte[] buffer = new byte[size];
                if (await networkStream.ReadAsync(buffer, 0, size, cancellationToken.Token) == size)
                {
                    return buffer;
                }
                else
                {
                    throw new Exception("Invalid Packet.");
                }
            }
            else throw new Exception("Not connected");
        }

        public async Task sendPacket(IPacket packet)
        {
            if (client.Connected)
            {
                byte[] buffer = packet.toBytes();
                await networkStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken.Token);
            }
            else throw new Exception("Not connected");
        }
    }
}
