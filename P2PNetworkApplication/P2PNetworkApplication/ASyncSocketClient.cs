using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PNetworkApplication
{
	public class ASyncSocketClient
	{
        #region Members
        private const int PORT = 12000; // The PORT number for the remote device.  
        private const int FILE_NAME_SIZE = 100;
		private IPAddress ipAddress;
        private string directory = @"C:\current";

		// ManualResetEvent instances signal completion.  
		private static ManualResetEvent connectDone = new ManualResetEvent(false);
		private static ManualResetEvent sendDone = new ManualResetEvent(false);
		private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        
		private static String response = String.Empty; // The response from the remote device.  
        #endregion Members
        
        public ASyncSocketClient(IPAddress address)
		{
			ipAddress = address;
		}

        #region Methods

        public void StartClient()
		{
			// Connect to a remote device.  
			try
			{
				// Establish the remote endpoint for the socket.    
				IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

				// Create a TCP/IP socket.  
				Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  


                // Sending the files from client to server from a specified directory
                string[] files = Directory.GetFiles(directory);
                
                for(int i = 0; i < files.Length; i++)
                {
                    client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    connectDone.WaitOne();


                    Console.WriteLine("file[" + i + "]: " + files[i]);
                    byte[] byteFile = File.ReadAllBytes(files[i]); // convert file to bytes to send over the socket
                    byte[] byteFileName = Encoding.ASCII.GetBytes(files[i]); // also the file name
                    byte[] wholeFile = new byte[FILE_NAME_SIZE + byteFile.Length]; // combine them together

                    // send name first
                    for (int j = 0; j < byteFileName.Length; j++)
                    {
                        wholeFile[j] = byteFileName[j];
                    }
                    // now the actual file
                    for (int j = 0; j < byteFile.Length; j++)
                    {
                        wholeFile[FILE_NAME_SIZE + j] = byteFile[j];
                    }
                    client.BeginSend(wholeFile, 0, wholeFile.Length, 0, new AsyncCallback(SendCallback), client);
                    sendDone.WaitOne();


                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(true);
                    
                    Console.WriteLine("File transfer complete for " + files[i]);
                }

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Environment.Exit(0);
            }
			catch (Exception e)
			{
				//Console.WriteLine(e.ToString());
			}
		}

		private static void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				// Retrieve the socket from the state object.  
				Socket client = (Socket)ar.AsyncState;

				// Complete the connection.  
				client.EndConnect(ar);

				Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

				// Signal that the connection has been made.  
				connectDone.Set();
			}
			catch (Exception e)
			{
				//Console.WriteLine(e.ToString());
			}
		}

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                string server = client.RemoteEndPoint.ToString();
                
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to {1}.", bytesSent, server);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion Methods
    }
}
