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
        public async void ConnectToTracker(IPAddress trackerIP)
        {
            try
            {
                // Create a TcpClient.
                int port = 42000;
                TcpClient client = new TcpClient();
                await client.ConnectAsync(trackerIP, port);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Buffer for recieving server response
                byte[] data = new byte[256];

                // Recieve number of segments from server
                int responseLength = await stream.ReadAsync(data, 0, data.Length);
                string responseData = Encoding.ASCII.GetString(data, 0, responseLength);
                Console.WriteLine("Received: {0}", responseData);

                // Update the list of missing segments to contain every segment
                int numberOfSegments = Convert.ToInt32(responseData);
                FillMissingSegments(numberOfSegments);

                // Send peer ip and the next missing segment index to server
                string peerIP = this.IP.ToString();
                string segmentWanted = this.NextMissing().ToString();
                string msg = peerIP + " " + segmentWanted;
                data = Encoding.ASCII.GetBytes(msg);
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", msg);

                // Recieve ip of client that has the missing segment
                responseLength = await stream.ReadAsync(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, responseLength);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
                Console.WriteLine("Closed everything");
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

        private void FillMissingSegments(int numberOfSegments)
        {
            for (int i = 0; i < numberOfSegments; i++)
                MissingSegments.Add(i);
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
