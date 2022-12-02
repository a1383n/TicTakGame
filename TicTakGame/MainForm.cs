using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

using TicTakGame.Net.Game;
using TicTakGame.Net.Enum;
using TicTakGame.Utils;

namespace TicTakGame
{
    public partial class MainForm : Form
    {
        private GameService gameService;

        public MainForm()
        {
            InitializeComponent();

            ipLabel.Text = NetUtils.getLocalIPAddress().ToString();
        }

        private void GameService_ServiceStatusChanged(ServiceStatus status)
        {
            Action action = delegate
            {
                switch (status)
                {
                    case ServiceStatus.IDLE:
                        portLabel.Text = "N/A";
                        serverStatusLabel.Text = "IDLE";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Black;
                        progressBar1.Visible = false;
                        groupBox1.Enabled = true;
                        groupBox2.Enabled = true;
                        startServerButton.Text = "Start";
                        break;
                    case ServiceStatus.WaitingForConnection:
                        portLabel.Text = gameService.serverEndpoint.Port.ToString();
                        serverStatusLabel.Text = "Started. Waiting ...";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Green;
                        startServerButton.Text = "Stop";
                        progressBar1.Visible = false;
                        groupBox1.Enabled = true;
                        break;
                    case ServiceStatus.Connecting:
                        serverStatusLabel.Text = "Connecting ...";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Green;
                        break;
                    case ServiceStatus.ClientConnected:
                        serverStatusLabel.Text = "Connected";
                        progressBar1.Visible = false;
                        GameForm gameForm = new GameForm();
                        gameForm.initializeService(gameService);
                        this.Hide();
                        gameForm.ShowDialog();
                        this.Show();
                        gameService.cancellationToken.Cancel();
                        gameService.Dispose();

                        groupBox1.Enabled = true;
                        groupBox2.Enabled = true;
                        progressBar1.Visible = false;
                        break;
                }
            };

            if (InvokeRequired)
                Invoke(action);
            else
                action.Invoke();
        }

        private void startServerButton_Click(object sender, EventArgs e)
        {
            if (gameService == null || gameService.status == ServiceStatus.IDLE)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;

                progressBar1.Visible = true;

                gameService = Net.Game.GameService.Builder.createServer(58110);
                gameService.ServiceStatusChanged += GameService_ServiceStatusChanged;

                serverWorker.RunWorkerAsync();
            }
            else
            {
                gameService.cancellationToken.Cancel();
                gameService.Dispose();
            }
        }

        private void serverWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                gameService.connectOrAccept().Wait(gameService.cancellationToken.Token);
            }
            catch (OperationCanceledException) { serverWorker.Dispose(); }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                serverWorker.Dispose();
                gameService.Dispose();
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();

            IPAddress iPAddress;
            if (!IPAddress.TryParse(ipAddressTextBox.Text, out iPAddress))
            {
                errorProvider1.SetError(ipAddressTextBox, "Invalid IP");
                return;
            }

            if (gameService == null || gameService.status == ServiceStatus.IDLE)
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                progressBar1.Visible = true;
                gameService = Net.Game.GameService.Builder.createClient(new IPEndPoint(iPAddress, (int)portTextBox.Value));
                gameService.ServiceStatusChanged += GameService_ServiceStatusChanged;
                serverWorker.RunWorkerAsync();
            }
        }
    }
}
