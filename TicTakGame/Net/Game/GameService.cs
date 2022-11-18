using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TicTakGame.Net.Game
{
    public enum ServiceStatus
    {
        IDLE,
        Disconnected,
        WaitingForConnection,
        ClientConnected,
        Connecting,
        Connected
    }

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
         }

        private TcpClient client;
        private TcpListener server;
        public IPEndPoint serverEndpoint;
        public bool isServer { private set; get; }
        public ServiceStatus status { private set; get; } = ServiceStatus.IDLE;
        public System.Threading.CancellationTokenSource cancellationToken = new System.Threading.CancellationTokenSource();

        public delegate void GameServiceEvent(ServiceStatus status);
        public event GameServiceEvent ServiceStatusChanged;

        private NetworkStream networkStream;

        public byte[] handshakeResult = null;
        public bool isHandshaked { get { return handshakeResult != null; } }

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
                    await startHandshake();
                    changeStatus(ServiceStatus.ClientConnected);
                }
                else
                {
                    changeStatus(ServiceStatus.Connecting);
                    await client.ConnectAsync(serverEndpoint.Address, serverEndpoint.Port);
                    networkStream = client.GetStream();
                    await startHandshake();
                    changeStatus(ServiceStatus.Connected);
                }
            }
        }

        public async Task<Packet> receivePacket()
        {
            if (client.Connected)
            {
                byte[] buffer = new byte[] { 0, 0, 0 };
                await networkStream.ReadAsync(buffer, 0, 3);

                return Packet.FromBytes(buffer);
            }
            else
                throw new Exception("Client not connected");
        }

        public async void sendPacket(Packet packet)
        {
            if (client.Connected)
            {
                byte[] buffer = packet.toBytes();
                await networkStream.WriteAsync(buffer);
            }
            else
                throw new Exception("Client not connected");
        }

        public async Task startHandshake()
        {
            if (!client.Connected)
                throw new Exception("Client not connected");

            if (!isServer)
            {
                // Client pick the random number between 1 and 2 to findout who is X and who is O
                // Number 1 is X and 2 is O
                Random random = new Random();
                int firstNum = random.Next(1, 3);

                // Client pick random number betwwen 0 and 1 to findout who start the game first
                // Number 1 is your turn and number 0 means other side turn
                int secondNum = random.Next(0, 1);

                // Keep this result for using that for config game
                handshakeResult = new byte[] { (byte)firstNum, (byte)secondNum };

                // Send this result to other side
                byte[] buffer = new byte[] { 0x5C, (byte)(handshakeResult[0] == 1 ? 2 : 1), (byte)(handshakeResult[1] == 0 ? 1 : 0) };
                await networkStream.WriteAsync(buffer);
            }else 
            {
                // Wating for client packet

                byte[] buffer = new byte[3] { 0,0,0 };
                await networkStream.ReadAsync(buffer, 0, 3);

                if (buffer[0] != 0x5c)
                    throw new Exception("Invalid packet recevied!");

                handshakeResult = new byte[] { buffer[1], buffer[2] };
            }

           await  networkStream.FlushAsync();
        }

        public void Dispose()
        {
            if (server != null)
                server.Stop();

            if (client != null)
                client.Dispose();

            changeStatus(ServiceStatus.IDLE);
        }
    }
}
