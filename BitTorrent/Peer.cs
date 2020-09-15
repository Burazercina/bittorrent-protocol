using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrent
{
    class Peer
    {
        private IPAddress IP;
        private HashSet<int> AcquiredSegments;
        private List<int> MissingSegments;

        public Peer(IPAddress IP)
        {
            this.IP = IP;
            AcquiredSegments = new HashSet<int>();
            MissingSegments = new List<int>();
        }

        public HashSet<int> GetAcquiredSegments()
        {
            return AcquiredSegments;
        }

        public IPAddress GetIP()
        {
            return IP;
        }

        // Source: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener
        public void ConnectToTracker(IPAddress trackerIP, string message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                int port = 42000;
                TcpClient client = new TcpClient();
                client.Connect(trackerIP, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                byte[] data = Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new byte[256];

                // String to store the response ASCII representation.
                string responseData = string.Empty;

                // Read the first batch of the TcpServer response bytes.
                int bytes = stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public int NextMissing()
        {
            if (MissingSegments.Count == 0)
                return -1;
            else
                return MissingSegments[0];
        }
    }
}
