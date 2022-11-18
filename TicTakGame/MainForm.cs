using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TicTakGame
{
    public partial class MainForm : Form
    {
        private Net.Game.GameService gameService;

        public MainForm()
        {
            InitializeComponent();

            //  ipLabel.Text = Utils.NetUtils.getIPAddress(Utils.NetUtils.getPrimeryNetworkInterface()).ToString();
            ipLabel.Text = "0.0.0.0";
        }

        private void GameService_ServiceStatusChanged(Net.Game.ServiceStatus status)
        {
            Action action = delegate
            {
                switch (status)
                {
                    case Net.Game.ServiceStatus.IDLE:
                        portLabel.Text = "N/A";
                        serverStatusLabel.Text = "IDLE";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Black;
                        progressBar1.Visible = false;
                        groupBox1.Enabled = true;
                        groupBox2.Enabled = true;
                        startServerButton.Text = "Start";
                        break;
                    case Net.Game.ServiceStatus.Disconnected:
                        serverStatusLabel.Text = "Disconnected";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Red;
                        break;
                    case Net.Game.ServiceStatus.WaitingForConnection:
                        portLabel.Text = gameService.serverEndpoint.Port.ToString();
                        serverStatusLabel.Text = "Started. Waiting ...";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Green;
                        startServerButton.Text = "Stop";
                        progressBar1.Visible = false;
                        groupBox1.Enabled = true;
                        break;
                    case Net.Game.ServiceStatus.Connecting:
                        serverStatusLabel.Text = "Connecting ...";
                        serverStatusLabel.ForeColor = System.Drawing.Color.Green;
                        break;
                    case Net.Game.ServiceStatus.Connected:
                        this.Hide();
                        (new GameForm(gameService)).Show();
                        this.Show();
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
            if (gameService == null || gameService.status != Net.Game.ServiceStatus.WaitingForConnection)
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

        private async void serverWorker_DoWork(object sender, DoWorkEventArgs e)
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

            if (gameService == null || gameService.status == Net.Game.ServiceStatus.IDLE || gameService.status == Net.Game.ServiceStatus.Disconnected)
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
