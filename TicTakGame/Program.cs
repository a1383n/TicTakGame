using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;


namespace TicTakGame
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());

            // var v = new TicTakGame.Net.Packet.HandShakeResult.Builder()
            //     .addPlayer(new TicTakGame.Net.Packet.Player(IPAddress.Loopback))
            //     .addPlayer(new TicTakGame.Net.Packet.Player(IPAddress.Loopback))
            //     .build();

            // Console.WriteLine(v.toBytes().Length);
            
        }

    }
}
