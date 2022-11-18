using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TicTakGame.Net.Game;

namespace TicTakGame
{
    public partial class GameForm : Form
    {
        private Net.Game.GameService gameService;
        private Game.Game game;

        public GameForm(GameService service)
        {
            game = new Game.Game(service);
            game.GameStatusChanged += Game_GameStatusChanged;

            InitializeComponent();
        }

        private void Game_GameStatusChanged(Game.GameStatus status)
        {
            switch (status)
            {
                case Game.GameStatus.CellSelected:
                    groupBox1.Enabled = false;
                    if (game.getGameResult() != Game.GameResult.InProgress)
                    {
                        switch (game.getGameResult())
                        {
                            case Game.GameResult.Draw:
                                MessageBox.Show("Game is draw.");
                                break;
                            case Game.GameResult.XWin:
                                MessageBox.Show("X Win");
                                break;
                            case Game.GameResult.OWin:
                                MessageBox.Show("O Win");
                                break;
                        }

                        this.Close();
                        return;
                    }else
                    {
                        game.next();
                    }
                    break;
                case Game.GameStatus.TurnX:
                    statusLabel.Text = "X turn";
                    if (game.currentPlayer == game.playerRule)
                    {
                        groupBox1.Enabled = true;
                    }else
                    {
                        gameWorker.RunWorkerAsync();
                    }
                    break;
                case Game.GameStatus.TurnO:
                    statusLabel.Text = "O turn";
                    if (game.currentPlayer == game.playerRule)
                    {
                        groupBox1.Enabled = true;
                    }else
                    {
                        gameWorker.RunWorkerAsync();
                    }
                    break;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int index = button.TabIndex;

            button.Enabled = false;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.BackgroundImage = game.currentPlayer == Game.Game.XSymbol ? Properties.Resources.x : Properties.Resources.o;

            game.selectCell((byte)index);
        }



        private async void gameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Packet packet = await gameService.receivePacket();
            Button button = (Button)groupBox1.Controls[packet.selectedBlock];
            button.Enabled = false;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.BackgroundImage = game.currentPlayer == Game.Game.XSymbol ? Properties.Resources.x : Properties.Resources.o;
            game.selectCell(packet.selectedBlock);
        }
    }
}
