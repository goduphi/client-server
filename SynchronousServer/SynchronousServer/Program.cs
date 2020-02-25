/*
    Author: Sarker Nadir Afridi Azmi
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Security;

namespace SynchronousServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";

            // Max number of incoming connections
            int maxNumberOfConnections = 4;

            // This is the port that will be used by the client to identify this server
            int port = 11000;

            // Listener or server sockets open a port on the network and then wait for a client to connect to that port
            // Resolve() returns information of all the addresses and aliases to an array called the AddressList
            // We will be using the first address returned
            IPHostEntry ipHostinfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostinfo.AddressList[0];

            // Now what we need to do is to create an end point of the service that is being provided
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Now we have to associate the endpoint we created with a socket
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket handler = null;

            try
            {
                listener.Bind(localEndPoint);

                // Listen for a client endpoint
                listener.Listen(maxNumberOfConnections);

                Console.WriteLine("Waiting to connect to a client...");

                // Accept incoming connections from the client
                handler = listener.Accept();

                byte[] msgFromClinet = new byte[1024];
                int bytesRecv = 0;
                string data = null;

                while(true)
                {
                    bytesRecv = handler.Receive(msgFromClinet);
                    data += System.Text.Encoding.ASCII.GetString(msgFromClinet);
                    if(data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                Console.WriteLine("Text received : {0}", data);

                // Encode the string in ASCII encoded bytes because you can only send bytes over the network
                byte[] msgEchoToclient = System.Text.Encoding.ASCII.GetBytes(data);
                handler.Send(msgEchoToclient);

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

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

        }
    }
}
