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
        private IPAddress ServerIP;
        private List<Peer> Peers;

        private int SegmentSize; // In bytes
        private int NumberOfSegments;

        private string FilePath;

        public Tracker(IPAddress ServerIP, int SegmentSize, int NumberOfSegments, string FilePath)
        {
            this.ServerIP = ServerIP;
            Peers = new List<Peer>();

            this.SegmentSize = SegmentSize;
            this.NumberOfSegments = NumberOfSegments;

            this.FilePath = FilePath;
            
        }

        // Source: https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently
        // Splits file into segments and returns number of segments
        public static int SplitFile(string inputFile, int chunkSize, string path)
        {
            byte[] buffer = new byte[chunkSize];

            using (Stream input = File.OpenRead(inputFile))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    using (Stream output = File.Create(path + "\\" + index))
                    {
                        int chunkBytesRead = 0;
                        while (chunkBytesRead < chunkSize)
                        {
                            int bytesRead = input.Read(buffer,
                                                       chunkBytesRead,
                                                       chunkSize - chunkBytesRead);

                            if (bytesRead == 0)
                            {
                                break;
                            }
                            chunkBytesRead += bytesRead;
                        }
                        output.Write(buffer, 0, chunkBytesRead);
                    }
                    index++;
                }
                return index;
            }
        }

        public void AddPeer(Peer p)
        {
            Peers.Add(p);
        }

        public async void CreateServer()
        {
            TcpListener server = null;
            try
            {
                int port = 42000;

                server = new TcpListener(ServerIP, port);
                server.Start();

                byte[] bytes = new byte[256];
                string data = null;

                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected!");

                    data = null;

                    NetworkStream stream = client.GetStream();

                    // Send number of segments to this peer
                    string numberOfSegments = this.NumberOfSegments.ToString();
                    byte[] msg = Encoding.ASCII.GetBytes(numberOfSegments);
                    await stream.WriteAsync(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", numberOfSegments);

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                    {
                        // Recieve peer ip and missing index
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Parse recieved message
                        string[] parsedMsg = data.Split(' ');
                        IPAddress newPeerIP = IPAddress.Parse(parsedMsg[0]);
                        Peers.Add(new Peer(newPeerIP));
                        int segmentIndex = Convert.ToInt32(parsedMsg[1]);

                        // Loop through every peer to determine who has the segment
                        string ipToSend = ServerIP.ToString();
                        foreach (Peer peer in Peers)
                        {
                            if (peer.GetAcquiredSegments().Contains(segmentIndex))
                            {
                                ipToSend = peer.GetIP().ToString();
                            }
                        }

                        // Send ip of peer that has the segment
                        msg = Encoding.ASCII.GetBytes(ipToSend);
                        await stream.WriteAsync(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", ipToSend);
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
