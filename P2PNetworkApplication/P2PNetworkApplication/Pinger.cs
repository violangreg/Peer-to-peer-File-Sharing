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
	public static class Pinger
	{
		const bool netWorkName = true;
		public static HashSet<IPAddress> allIPAddress = new HashSet<IPAddress>();

		// Constructor.

		// Method to start pinging the 255 netowrk ip addresses.
		public static void PingNetwork()
		{
			
			string ipBase = "192.168.2.";
			for (int i = 1; i < 255; i++)
			{
				string ipAddress = ipBase + i.ToString();

				Ping p = new Ping();
				p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
				p.SendAsync(ipAddress, 100, ipAddress);
			}
		}

		// Method that the event handler will invoke.
		public static void p_PingCompleted(object sender, PingCompletedEventArgs e)
		{
			string ip = (string)e.UserState;
			if (e.Reply != null && e.Reply.Status == IPStatus.Success)
			{
				if (netWorkName)
				{
					string name;
					try
					{
						IPHostEntry hostEntry = Dns.GetHostEntry(ip);
						name = hostEntry.HostName;
						IPAddress[] address = Dns.GetHostAddresses(name);
						allIPAddress.Add(address[0]);
					}
					catch (SocketException ex){}
				}
				//else {}
			}
			else if (e.Reply == null)
			{
				// Ping failed. Do nothing.
			}
		}
	}
}