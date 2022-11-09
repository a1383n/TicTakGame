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

        private void discoveryServiceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            discoveryService.LisenForDiscoveryPacket();
        }
    }
}
