using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace AsynchronousServer
{
    // Reading data from a client socket requires a state object that passes
    // values between asynchronous calls.

    // Placing these fields in the state object allows their values to be
    // preserved across multiple calls to read data from the client socket.
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class Program
    {
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.Title = "Server";
            StartListening();
        }

        public static void StartListening()
        {
            // Port number to used to identify application
            int port = 11000;

            int maxNumberOfConnections = 10;

            // Get a list of all of the addresses and aliases of the host and use the first address returned
            // in the AddressList array
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            // Create an end point of the service that is being provided
            IPEndPoint ipe = new IPEndPoint(ipAddress, port);

            Console.WriteLine("Local address and port: {0}", ipe.ToString());

            Socket listener;

            try
            {
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(ipe);
                listener.Listen(maxNumberOfConnections);

                while (true)
                {
                    // Set event to non signaled state
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Set event to wait until connection is made
                    allDone.WaitOne();
                }

            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException: {0}", se.ToString());
            }
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException: {0}", ae.ToString());
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine("ObjectDisposedException: {0}", ode.ToString());
            }
            catch (System.Security.SecurityException se)
            {
                Console.WriteLine("SecurityException: {0}", se.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // This part of the code signals the main application thread to keep on processing
            // and establishes a connection to the client
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;

            // Get the returned instance of the client
            Socket handler = listener.EndAccept(ar);

            // This part of the code starts reading data from the client
            StateObject state = new StateObject();
            state.workSocket = handler;

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String msgFromClient = String.Empty;

            // Retrieve StateObject that was passed in through the asynchronous call
            // and assign the information about the connecting socket to a handler
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.  
            int bytesRead = handler.EndReceive(ar);

            // Data was read from the client socket.  
            if (bytesRead > 0)
            {
                // Store whatever data was received
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                msgFromClient = state.sb.ToString();

                // Keep reading until end-of-file tag has been reached
                if (msgFromClient.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", msgFromClient.Length, msgFromClient);

                    // Echo the data back to the client
                    Send(handler, msgFromClient);
                }
                else
                {
                    // If all data was not received, get the rest of it
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }

            }
        }

        public static void Send(Socket handler, string data)
        {
            // Conver the string into bytes to be sent over the network
            byte[] echoMsgToClient = Encoding.ASCII.GetBytes(data);

            // Initiate sending data to the remote device
            handler.BeginSend(echoMsgToClient, 0, echoMsgToClient.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
