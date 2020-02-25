/*
    Author: Sarker Nadir Afridi Azmi
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Security;

namespace SynchronousClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";

            // This is the port number that we are going to use in order to connect to a specific application program on the host
            int port = 11000;

            // First, we need to get the IP address of the host that we are trying to connect to
            // Resolve() return a list of all the ip addresses and aliases for the requested name
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());

            // We can access the information returned by Resolve() as it essentially returns
            // information in an array called the address list
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            // We need to create a data pipe between the client and remote device
            // Essentially, we need to create a remote end point that the client can talk to
            IPEndPoint ipe = new IPEndPoint(ipAddress, port);

            // This socket establishes a connection between the client and remote device
            Socket s = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s.Connect(ipe);
            }
            catch(ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException {0}", ae.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("ArgumentNullException {0}", e.ToString());
            }

            string message = "This is a test string.";

            // Information over a network can only be sent in bytes
            // Convert the string message into a byte array using the Encoding.ASCII property
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message + "<EOF>");
            int bytesSent;

            try
            {
                bytesSent = s.Send(msg);
            }
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException {0}", ae.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException {0}", se.ToString());
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine("ObjectDisposedException {0}", ode.ToString());
            }

            // Information can only be received in bytes

            byte[] msgRecv = new byte[1024];
            int bytesRecv;

            try
            {
                bytesRecv = s.Receive(msgRecv);
            }
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException {0}", ae.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException {0}", se.ToString());
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine("ObjectDisposedException {0}", ode.ToString());
            }
            catch (SecurityException se)
            {
                Console.WriteLine("SecurityException {0}", se.ToString());
            }

            // Use the Encoding.ASCII property to convert the bytes back into a string
            // Assumption: Data coming from the network is ASCII encoded text
            string msgRecvString = System.Text.Encoding.ASCII.GetString(msgRecv);
            Console.WriteLine("Echoed back from server: {0}", msgRecvString);

            // Shutdown the socket for both sending and receiving and close the connection

            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException {0}", se.ToString());
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine("ObjectDisposedException {0}", ode.ToString());
            }

            s.Close();
        }
    }
}
