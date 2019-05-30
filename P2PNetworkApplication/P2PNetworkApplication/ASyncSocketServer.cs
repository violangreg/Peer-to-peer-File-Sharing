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
	public class ASyncSocketServer
	{
        #region Members
        private const int PORT = 12000;                                         // The PORT number for the remote device.  
        private const int FILE_NAME_SIZE = 100;
        public static ManualResetEvent allDone = new ManualResetEvent(false);   // Thread signal.  
        #endregion Members

        #region Methods

        public ASyncSocketServer(){ }

		public void StartListening()
		{
			// Data buffer for incoming data.  
			byte[] bytes = new Byte[1024];

			// Establish the local endpoint for the socket.  
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[1];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 12000);
			// Create a TCP/IP socket.  
			Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and listen for incoming connections.  
			try
			{
				listener.Bind(localEndPoint);
				listener.Listen(100);

                // DEBUGGING
				while (true)
				{
				    // Set the event to nonsignaled state.  
				    allDone.Reset();
                    
                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    
                    // receive the actual file
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

				    // Wait until a connection is made before continuing.  
				    allDone.WaitOne();
                }
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
            Console.ReadLine();
            Environment.Exit(0);
		}

		public void AcceptCallback(IAsyncResult ar)
		{


            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept(ar);

			// Create the state object.  
			StateObject state = new StateObject();
			state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

            // Signal the main thread to continue.  
            allDone.Set();

            //listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }
        
        public void ReadCallback(IAsyncResult ar)
		{
            lock (Lock)
            {
                // Retrieve the state object and the handler socket from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                // Parsing the data, first 100 bytes is the file name, after 100th byte is the file data
                int fileSize = 0;
                for (int i = 0; i < FILE_NAME_SIZE; i++)
                {
                    if (state.buffer[i] == 0) break; // Buffer is 0 when its empty (no written byte)
                    fileSize++;
                }
                byte[] byteFileName = new byte[fileSize];

                // Getting the file name only
                for (int i = 0; i < fileSize; i++)
                {
                    byteFileName[i] = state.buffer[i];
                }

                String s = Encoding.ASCII.GetString(byteFileName);
                int index = s.LastIndexOf("\\");
                String fileNameOnly = s.Substring(index + 1);
                // Console.WriteLine("file name only: " + fileNameOnly); // DEBUGGING
                // Saving file at Desktop
                String path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                String fileSavePath = path + "\\" + fileNameOnly;
                // Console.WriteLine("file save path: " + fileSavePath); // DEBUGGING

                BinaryWriter writer;
                writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Create));
                writer.Write(state.buffer, FILE_NAME_SIZE, bytesRead);
                writer.Flush();
                writer.Close();
            }
        }

        private bool doneFlag = false; // DEBUGGING
        private Object Lock = new object();
        #endregion Methods
    }
}
