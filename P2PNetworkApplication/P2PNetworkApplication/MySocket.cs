using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PNetworkApplication
{
	class MySocket
	{
		public const int PORT = 12000;
		
		// Properties.
		private Thread listenThread;
		private Thread connectionThread;
		private IPHostEntry ipHostInfo;
		private IPAddress myAddress;
		private ASyncSocketServer listener;		

		// Constructor.
		public MySocket()
		{
			ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			myAddress = ipHostInfo.AddressList[1];
            Console.WriteLine(myAddress);
			listener = new ASyncSocketServer();
			connectionThread = new Thread(new ThreadStart(CreateConnection));
			listenThread = new Thread(new ThreadStart(Listen));
			listenThread.Start();
			connectionThread.Start();
		}

		public void CreateConnection()
		{
			Pinger.PingNetwork();
			Console.WriteLine("Pinging Network...");
			// Giving the Pinger time to ping all the connected nodes in the network.
			Thread.Sleep(5000); 
			foreach (IPAddress address in Pinger.allIPAddress)
			{
				//if(!address.Equals(myAddress))
				//{
					Console.WriteLine(address.ToString());
					IPAddress ipAddress = IPAddress.Parse("192.168.2.8");
					Thread t = new Thread(() => Connection(ipAddress));
					t.Start();
				//}
			}
			Console.ReadLine();
		}

		public void Connection(IPAddress address)
		{
			var client = new ASyncSocketClient(address);
			client.StartClient();
		}

		public void Listen()
		{
			listener.StartListening();
		}
	}
}
