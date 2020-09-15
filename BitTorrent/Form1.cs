using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitTorrent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Static functions will be removed and relocated to their respective classes

        // Source: https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently
        public static void SplitFile(string inputFile, int chunkSize, string path)
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
            }
        }

        

        private void uploadButton_Click(object sender, EventArgs e)
        {
            // Create tracker
            string ip = "127.0.0.1";
            IPAddress trackerIP = IPAddress.Parse(ip);
            int segmentSize = 32 * 1024;
            Tracker tracker = new Tracker(trackerIP, segmentSize);

            // Store torrent in chosen directory
            string trackerPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Trackers\\" + ip;
            Directory.CreateDirectory(trackerPath);

            // Determine file for upload directory and make directory for segmented file
            string filePath = filePathTextBox.Text;
            string splitFilePath = trackerPath + "\\File\\";
            Directory.CreateDirectory(splitFilePath);

            // Split file for upload into segments
            if (filePath.Length != 0)
            {
                SplitFile(filePath, segmentSize, splitFilePath);
                MessageBox.Show("Splitting complete", "Success");
            }
            else
            {
                MessageBox.Show("Please specify the path of the file you want to upload", "Error");
            }

            tracker.CreateServer();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            // Get tracker ip from text box
            string ip = trackerIPTextBox.Text;
            IPAddress trackerIP = IPAddress.Parse(ip);

            // Connect as a peer to the tracker and request the file
            Peer peer = new Peer(IPAddress.Parse("127.0.0.1"));
            string msg = peer.NextMissing().ToString();
            peer.ConnectToTracker(trackerIP, msg);
        }
    }
}
