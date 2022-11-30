using System;
using System.ComponentModel;
using System.Windows.Forms;

using TicTakGame.Net.Game;
using TicTakGame.Main;
using TicTakGame.Net.Enum;
using TicTakGame.Net.Packet;

namespace TicTakGame
{
    public partial class GameForm : Form
    {
        private GameService gameService;
        private Game game;

        public GameForm()
        {
            InitializeComponent();
        }

        private async void onDoWork(object sender, DoWorkEventArgs args)
        {
            /*
                Handshake: -1
                ReceivePacket: -2
                SendPacket: int
            */
            if (args.Argument is int)
            {
                switch ((int)args.Argument)
                {
                    case -1:
                        await game.startHandshake();
                   //     game.changeStatus(GameStatus.PlayerTurn);
                        break;
                    case -2:
                        await game.waitingForOtherPlayerTurn();
                        // changeStatus(GameStatus.PlayerTurn);
                        break;
                    default:
                        if ((int)args.Argument >= 0)
                            await game.play((int)args.Argument);
                        break;
                }
            }
        }

        public async void initializeService(GameService service)
        {
            gameService = service;
            game = new Game(service);
            game.GameStatusChanged += Game_GameStatusChanged;

            await game.startHandshake();
        }

        private void Game_GameStatusChanged(GameStatus status, object extra)
        {
            Action action = delegate
            {
                switch (status)
                {
                    case GameStatus.WaitingForHandShake:
                        statusLabel.Text = status.ToString();
                        groupBox1.Enabled = false;
                        break;
                    case GameStatus.PlayerTurn:
                        statusLabel.Text = game.isMyTurn() ? "Your turn" : "Turn " + game.currentPlayer.role;
                        groupBox1.Enabled = game.isMyTurn();
                        pictureBox1.Image = game.isMyTurn() ? game.currentPlayer.isX() ? Properties.Resources.x : Properties.Resources.o : !game.currentPlayer.isX() ? Properties.Resources.x : Properties.Resources.o;
                        
                        if (!game.isMyTurn()) {
                            while(gameWorker.IsBusy) {}
                            gameWorker.RunWorkerAsync(-2);
                        }
                        break;
                    case GameStatus.CellSelected:
                        groupBox1.Enabled = false;
                        selectCell((int)extra,game.currentPlayer.role);
                        break;
                    case GameStatus.End:
                        switch ((GameResult)extra)
                        {
                            case GameResult.Draw:
                                MessageBox.Show("Draw!", "Game Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Close();
                                break;
                            case GameResult.PlayerWin:
                                if (game.isMyTurn())
                                    MessageBox.Show("You win", "Game Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                else
                                    MessageBox.Show($"{game.currentPlayer.role} was win", "Game Result", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                this.Close();
                                break;
                        }
                        break;
                }
            };

            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action.Invoke();
        }

        private void selectCell(int index, Role role)
        {
            Button button = (Button)groupBox1.Controls[index];
            button.Enabled = false;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.BackgroundImage = role == Role.X ? Properties.Resources.x : Properties.Resources.o;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int index = button.TabIndex;

            gameWorker.RunWorkerAsync(index);
        }
    }
}
