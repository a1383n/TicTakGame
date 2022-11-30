using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TicTakGame.Net.Packet;
using TicTakGame.Utils;
using TicTakGame.Net.Enum;

namespace TicTakGame.Main
{
    class Game
    {
        private Net.Game.GameService service;
        private byte[] gameBoard = new byte[9];
        public Guid myId;
        private List<Player> players;
        public Player currentPlayer;
        public GameStatus gameStatus = GameStatus.WaitingForHandShake;

        public delegate void GameEvent(GameStatus status, object extra);
        public event GameEvent GameStatusChanged;

        public Game(Net.Game.GameService service)
        {
            this.service = service;

            // Fill the game board with zero
            Array.Fill(gameBoard, (byte)0);
        }

        public async Task<HandShakeResult> startHandshake()
        {
            if (!service.client.Connected)
                throw new Exception("Client not connected");

            if (players != null || currentPlayer != null)
                throw new Exception("Handshake only can be executed one time.");


            if (service.isServer)
            {
                // Server should build HandShakeResult and sent it to client
                // Collect players information
                IPAddress localIPAddress = NetUtils.getLocalIPAddress();
                Player my = new Player(localIPAddress);

                myId = my.id;

                HandShakeResult handShakeResult = new HandShakeResult.Builder()
                    .addPlayer(my)
                    .addPlayer(new Player(((IPEndPoint)service.client.Client.RemoteEndPoint).Address))
                    .build();

                // Apply handshake data
                players = handShakeResult.players;
                currentPlayer = players.Find(x => x.id == handShakeResult.firstTurnPlayerId);

                await service.sendPacket(handShakeResult);

                changeStatus(GameStatus.PlayerTurn);

                return handShakeResult;
            }
            else
            {
                // Client should waiting for HandShakeResult to be sent from server
                HandShakeResult handShakeResult = HandShakeResult.fromBytes(await service.getPacket(HandShakeResult.Size));

                myId = handShakeResult.players[1].id;

                // Apply handshake data
                players = handShakeResult.players;
                currentPlayer = players.Find(x => x.id == handShakeResult.firstTurnPlayerId);

                changeStatus(GameStatus.PlayerTurn);

                return handShakeResult;
            }
        }

        public async Task<GameResult> play(int cellIndex)
        {
            changeStatus(GameStatus.CellSelected, cellIndex);

            if (gameBoard[cellIndex] != 0)
                throw new Exception("Cell is not empty");

            gameBoard[cellIndex] = EnumHelper<Role, byte>.GetValueByEnum(currentPlayer.role);

            // Send GamePacket
            await service.sendPacket(new GamePacket(currentPlayer.id, (byte)cellIndex));

            GameResult gameResult = getGameResult();

            if (gameResult == GameResult.InProgress)
            {
                // Change current player
                currentPlayer = players.Find(x => x.role != currentPlayer.role);
                changeStatus(GameStatus.PlayerTurn);
            }
            else
            {
                changeStatus(GameStatus.End, gameResult);
            }

            return gameResult;
        }

        public async Task waitingForOtherPlayerTurn()
        {
            if (currentPlayer.id == myId)
                throw new Exception("It's your turn");

            // Waiting for GamePacket
            byte[] buffer = await service.getPacket(GamePacket.Size);
            GamePacket packet = GamePacket.FromBytes(buffer);
            changeStatus(GameStatus.CellSelected, packet.cellIndex);

            if (packet.playerId != currentPlayer.id)
                throw new Exception("Unexpected packet");

            if (gameBoard[packet.cellIndex] != 0)
                throw new Exception("Cell is not empty");

            gameBoard[packet.cellIndex] = EnumHelper<Role, byte>.GetValueByEnum(currentPlayer.role);
            GameResult gameResult = getGameResult();

            if (gameResult == GameResult.InProgress)
            {
                // Change current player
                currentPlayer = players.Find(x => x.role != currentPlayer.role);
                changeStatus(GameStatus.PlayerTurn);
            }
            else changeStatus(GameStatus.End);
        }

        public bool isMyTurn() => currentPlayer.id == myId;

        public void changeStatus(GameStatus status, object extra = null)
        {
            if (GameStatusChanged != null)
            {
                gameStatus = status;
                GameStatusChanged(status, extra);
            }
        }

        private bool IsAllCellPicked()
        {
            foreach (byte b in gameBoard)
            {
                if (b == 0)
                    return false;
            }

            return true;
        }

        private short checkGameTable()
        {
            /*
            * [0 1 2]
            * [3 4 5]
            * [6 7 8]
            */

            // Vertical check
            if (gameBoard[0] != 0 && gameBoard[0] == gameBoard[3] && gameBoard[3] == gameBoard[6])
                return gameBoard[0];

            if (gameBoard[1] != 0 && gameBoard[1] == gameBoard[4] && gameBoard[4] == gameBoard[7])
                return gameBoard[1];

            if (gameBoard[2] != 0 && gameBoard[2] == gameBoard[5] && gameBoard[5] == gameBoard[8])
                return gameBoard[2];

            // Horizontal check
            if (gameBoard[0] != 0 && gameBoard[0] == gameBoard[1] && gameBoard[1] == gameBoard[2])
                return gameBoard[0];

            if (gameBoard[3] != 0 && gameBoard[3] == gameBoard[4] && gameBoard[4] == gameBoard[5])
                return gameBoard[3];

            if (gameBoard[6] != 0 && gameBoard[6] == gameBoard[7] && gameBoard[7] == gameBoard[8])
                return gameBoard[6];

            // Diagonal check
            if (gameBoard[0] != 0 && gameBoard[0] == gameBoard[4] && gameBoard[4] == gameBoard[8])
                return gameBoard[0];

            if (gameBoard[2] != 0 && gameBoard[2] == gameBoard[4] && gameBoard[4] == gameBoard[6])
                return gameBoard[2];

            if (IsAllCellPicked())
                return -1;

            // Game is not end
            return 0;
        }

        private GameResult getGameResult()
        {
            return checkGameTable() switch
            {
                0 => GameResult.InProgress,
                1 => GameResult.PlayerWin,
                2 => GameResult.PlayerWin,
                -1 => GameResult.Draw,
                _ => throw new Exception($"Unexpected value: {this.checkGameTable()}"),
            };
        }
    }
}
