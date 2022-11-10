using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicTakGame
{
    public partial class Form1 : Form
    {
        private Net.Discovery.DiscoveryService discoveryService = new Net.Discovery.DiscoveryService();

        public Form1()
        {
            InitializeComponent();

            discoveryServiceWorker.RunWorkerAsync();
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
        //    discoveryService.StopLisenForDiscoveyPacket();
         //   discoveryServiceWorker.CancelAsync();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            discoveryService.DiscoverAround();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            refreshDeviceList();
        }
    }
}
