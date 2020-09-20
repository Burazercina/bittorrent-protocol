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

        private void uploadButton_Click(object sender, EventArgs e)
        {

            int segmentSize = 32 * 1024;
            string ip = "127.0.0.1";

            // Make directory for tracker
            string trackerPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Trackers\\" + ip;
            Directory.CreateDirectory(trackerPath);

            // Determine file for upload directory and make directory for segmented file
            string filePath = filePathTextBox.Text;
            string splitFilePath = trackerPath + "\\File\\";
            Directory.CreateDirectory(splitFilePath);

            
            if (filePath.Length != 0)
            {
                MessageBox.Show("Splitting complete", "Success");
            }
            else
            {
                MessageBox.Show("Please specify the path of the file you want to upload", "Error");
                return;
            }

            // Split file for upload into segments
            int numberOfSegments = Tracker.SplitFile(filePath, segmentSize, splitFilePath);

            // Create tracker
            IPAddress trackerIP = IPAddress.Parse(ip);
            Tracker tracker = new Tracker(trackerIP, segmentSize, numberOfSegments, filePath);

            // Run server for tracker
            tracker.CreateServer();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            // Get tracker ip from text box
            string ip = trackerIPTextBox.Text;
            if (ip.Length != 0)
            {
                MessageBox.Show("Connecting to " + ip);
            }
            else
            {
                MessageBox.Show("Please specify the IP address of the tracker", "Error");
                return;
            }

            // Connect as a peer to the tracker and request the file
            IPAddress trackerIP = IPAddress.Parse(ip);
            Peer peer = new Peer(IPAddress.Parse("127.0.0.1"));
            peer.ConnectToTracker(trackerIP);
        }
    }
}
