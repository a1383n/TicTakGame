using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Game
{
    enum GameStatus
    {
       CellSelected,
       TurnX,
       TurnO,
       End
    }

    enum GameResult
    {
        InProgress,
        Draw,
        XWin,
        OWin
    }

    class Game
    {
        private Net.Game.GameService service;

        public const byte XSymbol = 0x1;
        public const byte OSymbol = 0x2;

        public const byte turnTimeOutInSecond = 10;

        public byte[] gameBoard = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public byte playerRule = XSymbol;


        public byte currentPlayer = XSymbol;

        public GameStatus gameStatus = GameStatus.TurnX;

        public delegate void GameEvent(GameStatus status);
        public event GameEvent GameStatusChanged;

        public Game(Net.Game.GameService service)
        {
            this.service = service;

            playerRule = service.handshakeResult[0];
            currentPlayer = service.handshakeResult[1] == 1 ? playerRule : playerRule == XSymbol ? OSymbol : XSymbol;

        }

        /// <summary>
        /// Set the current player action on the game borad, Then change the turn
        /// </summary>
        /// <param name="index">The cell index. 0-7</param>
        public void selectCell(byte index)
        {
            if (gameBoard[index] == 0 && gameStatus != GameStatus.End)
            {
                gameBoard[index] = currentPlayer;
                changeStatus(GameStatus.CellSelected);
                service.sendPacket(new Net.Game.Packet
                {
                    playerRole = playerRule,
                    selectedBlock = index
                });
            }
            else
                throw new InvalidOperationException("Table is out of cell or game is ended");
        }

        public void next()
        {
            currentPlayer = currentPlayer == XSymbol ? OSymbol : XSymbol;
            changeStatus(currentPlayer == XSymbol ? GameStatus.TurnX : GameStatus.TurnO);
        }

        private void changeStatus(GameStatus status)
        {
            if (GameStatusChanged != null)
            {
                gameStatus = status;
                GameStatusChanged(status);
            }
        }

        public bool IsAllCellPicked()
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

        public GameResult getGameResult()
        {
            return checkGameTable() switch
            {
                0 => GameResult.InProgress,
                XSymbol => GameResult.XWin,
                OSymbol => GameResult.OWin,
                -1 => GameResult.Draw,
                _ => throw new Exception($"Unexpected value: {this.checkGameTable()}"),
            };
        }

        public static string byteToPlayer(byte player) => player == XSymbol ? "X" : "O";
    }
}
