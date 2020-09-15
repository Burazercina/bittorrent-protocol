using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrent
{
    class Tracker
    {
        IPAddress ServerIP;
        List<Peer> Peers;

        int SegmentSize; // In bytes

        public Tracker(IPAddress ServerIP, int SegmentSize)
        {
            this.ServerIP = ServerIP;
            this.SegmentSize = SegmentSize;
            Peers = new List<Peer>();
        }

        public void AddPeer(Peer p)
        {
            Peers.Add(p);
        }

        public void CreateServer()
        {
            // Create TCP Server
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 42000.
                int port = 42000;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(ServerIP, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // PROTOCOL HERE!

                        ////////////////////////////////////////
                        int segmentIndex = Convert.ToInt32(data);

                        foreach (Peer peer in Peers)
                        {
                            if (peer.GetAcquiredSegments().Contains(segmentIndex))
                            {
                                string peerIP = peer.GetIP().ToString();
                                byte[] msg = Encoding.ASCII.GetBytes(peerIP);
                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine("Sent: {0}", peerIP);
                            }
                        }
                        ////////////////////////////////////////
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}
