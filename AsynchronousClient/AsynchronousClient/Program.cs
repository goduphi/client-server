/*
    Author: Sarker Nadir Afridi Azmi

    Description: An asynchronous client socket does not suspend the application while
                 waiting for network operations to complete.  Instead, it uses the standard
                 .NET Framework asynchronous programming model to process the network
                 connection on one thread while the application continues to run on the
                 original thread.
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AsynchronousClient
{
    // State object for receiving data from remote device. 
    // Reading data from a client socket requires a state object that passes values between asynchronous calls.
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    class Program
    {
        // The port number for the remote device.  
        private const int port = 11000;

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static string response = String.Empty;

        static void Main(string[] args)
        {
            Console.Title = "Client";
            StartClient();
        }

        // Connects specified socket to specified endpoint
        public static void StartClient()
        {
            try
            {
                // Get a list of addresses and aliases
                // Use the first address returned in the AddressList array
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];

                // Create remote endpoint
                IPEndPoint ipe = new IPEndPoint(ipAddress, port);

                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // We pass in the remote endpoint that represents the network device
                // An AsyncCallback called ConnectCallback
                // A state object (the client Socket) which is used to pass state information between async calls
                client.BeginConnect(ipe, new AsyncCallback(ConnectCallback), client);

                connectDone.WaitOne();

                // Send test data to the remote device.  
                Send(client, "This is a test<EOF>");
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}.", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made
                connectDone.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        private static void Send(Socket client, string message)
        {
            // Encode the string data in byte data
            byte[] byteData = Encoding.ASCII.GetBytes(message);

            // Begin sending the data to the remote device
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket client = (Socket)ar.AsyncState;

                // Complete sending data to the remote connection
                int bytesSent = client.EndSend(ar);

                Console.WriteLine("Send {0} bytes to the server.", bytesSent);

                // Signal that the connection has been made
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }

        public static void Receive(Socket client)
        {
            try
            {
                // Create the state object
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving data from the remote device
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception {0}.", e.ToString());
            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device
                int bytesRead = client.EndReceive(ar);

                if(bytesRead > 0)
                {
                    // Store the data received so far
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // There may be more data
                    // Get the rest of the data
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // Put the data into the response variable
                    if(state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }

                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
        }
    }
}
