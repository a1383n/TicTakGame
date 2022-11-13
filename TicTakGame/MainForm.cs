using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TicTakGame
{
    public partial class MainForm : Form
    {
        private Net.Discovery.DiscoveryService discoveryService = new Net.Discovery.DiscoveryService();

        public MainForm()
        {
            InitializeComponent();
        }

        private void refreshDeviceList()
        {
            devicesDataGrid.Rows.Clear();

            for (int i = 0; i < discoveryService.devices.Count; i++)
            {
                Net.Discovery.Device device = discoveryService.devices[i];
                devicesDataGrid.Rows.Add(new string[] { device.iP.ToString(),device.clienName,device.status.ToString() });
            }
        }

        private async void discoveryServiceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            await discoveryService.LisenForDiscoveryPacket();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            discoveryService.StopLisenForDiscoveyPacket();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (discoveryService.IsDiscoverable)
                {
                    discoveryService.StopLisenForDiscoveyPacket();
                }
            }else
            {
                discoveryService.LisenForDiscoveryPacket();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            discoveryService.DiscoverAround();
            refreshDeviceList();
        }
    }
}
