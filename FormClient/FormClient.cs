using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace FormClient
{
    public partial class FormClient : Form
    {
        public FormClient()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serverIp.Text = "192.168.0.68";
            comboBox1.Text = "";
            comboBox1.Items.Add("@V838 Testmode_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V838 Download_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V838 LoadTest_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V838 Initial 15 00000001+");
            comboBox1.Items.Add("@V838 Test_Start 01 00000001+");
            comboBox1.Items.Add("@V838 Load_Start 01 00000001+");

            comboBox1.Items.Add("@V828 Testmode_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V828 Download_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V828 LoadTest_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V828 Initial 15 00000001+");
            comboBox1.Items.Add("@V828 Test_Start 01 00000001+");
            comboBox1.Items.Add("@V828 Load_Start 01 00000001+");

            comboBox1.Items.Add("@V822 Testmode_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V822 Download_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V822 LoadTest_CRC_0000 15 00000001+");
            comboBox1.Items.Add("@V822 Initial 15 00000001+");
            comboBox1.Items.Add("@V822 Test_Start 01 00000001+");
            comboBox1.Items.Add("@V822 Load_Start 01 00000001+");

            comboBox1.Items.Add("@V816 Initial 15 00000001+");
            comboBox1.Items.Add("@V816 Set1_Start 01 00000001+");
            comboBox1.Items.Add("@V816 Set2_Start 01 00000001+");

            comboBox1.Items.Add("@V815 Initial 15 00000001+");
            comboBox1.Items.Add("@V815 Test_Start 01 00000001+");

            comboBox1.Items.Add("@V821 Initial 15 00000001+");
            comboBox1.Items.Add("@V821 Test_Start 01 00000001+");


            close.Enabled = false;
            connect.Enabled = false;
            send.Enabled = true;

            close.Visible = false;
            connect.Visible = false;

        }

        private Client client = null;
        private void connect_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.CloseConnect();
                client = null;
            }

            client = new Client();
            client.ConnectToServer(serverIp.Text, 9000);

            close.Enabled = true;
            connect.Enabled = false;
            send.Enabled = true;
        }

        private void close_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.CloseConnect();
                client = null;
            }
            close.Enabled = false;
            connect.Enabled = true;
            send.Enabled = false;
        }

        private void send_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.CloseConnect();
                client = null;
            }

            client = new Client();
            client.ConnectToServer(serverIp.Text, 9000);

            send.Enabled = false;

            if (client == null)
            {
                return;
            }
            client.Send(comboBox1.Text);

            if (client != null)
            {
                client.CloseConnect();
                client = null;
            }
            send.Enabled = true;
        }

        private BackgroundWorker bw1;
        private BackgroundWorker bw2;
        private BackgroundWorker bw3;
        private void send2_Click(object sender, EventArgs e)
        {
            bw1 = new BackgroundWorker();
            bw1.WorkerSupportsCancellation = true;
            bw1.DoWork += new DoWorkEventHandler(BwWork);
            bw1.RunWorkerAsync(0);

            bw2 = new BackgroundWorker();
            bw2.WorkerSupportsCancellation = true;
            bw2.DoWork += new DoWorkEventHandler(BwWork);
            bw2.RunWorkerAsync(1);

            bw3 = new BackgroundWorker();
            bw3.WorkerSupportsCancellation = true;
            bw3.DoWork += new DoWorkEventHandler(BwWork);
            bw3.RunWorkerAsync(2);

        }

        private Client[] c = new Client[3];
        private void BwWork(object sender, DoWorkEventArgs e)
        {
            int idx = (int)e.Argument;
            if (c[idx] != null)
            {
                c[idx].CloseConnect();
                c[idx] = null;
            }

            c[idx] = new Client();
            c[idx].ConnectToServer(serverIp.Text, 9000);

            //send.Enabled = false;

            if (c[idx] == null)
            {
                return;
            }
            if (idx == 0)
            {
                c[idx].Send("@V821 Initial 07 00001111+");
            }
            else if (idx == 1)
            {
                c[idx].Send("@V821 Initial 08 00001111+");
            }
            else
            {
                c[idx].Send("@V821 Initial 09 00001111+");
            }

            if (c[idx] != null)
            {
                c[idx].CloseConnect();
                c[idx] = null;
            }
            //send.Enabled = true;
            if (c[idx] != null)
            {
                c[idx].CloseConnect();
                c[idx] = null;
            }

            c[idx] = new Client();
            c[idx].ConnectToServer(serverIp.Text, 9000);

            //send.Enabled = false;

            if (c[idx] == null)
            {
                return;
            }
            if (idx == 0)
            {
                c[idx].Send("@V821 Test_Start 07 00001111+");
            }
            else if (idx == 1)
            {
                //Thread.Sleep(1000);
                //c[idx].Send("@V821 Test_Start 08 00001111+");
            }
            else
            {
                //c[idx].Send("@V821 Test_Start 09 00001111+");
            }

            if (c[idx] != null)
            {
                c[idx].CloseConnect();
                c[idx] = null;
            }

        }
    }
}
