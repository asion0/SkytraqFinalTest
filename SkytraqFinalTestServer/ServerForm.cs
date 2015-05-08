﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections;
using System.Net;
using System.Reflection;

namespace SkytraqFinalTestServer
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
        }

        private TcpServer server = null;
        private BackgroundWorker bw = new BackgroundWorker();
        public static WorkerParam wp = new WorkerParam();

        public const int MaxSiteCount = 16;

        private BackgroundWorker[] bwSite = new BackgroundWorker[MaxSiteCount];
        public static WorkerParam[] wpSite = new WorkerParam[MaxSiteCount];
        public string workingNumber;
        private string logPath;
        private string logName;
        //Version
        private void ServerForm_Load(object sender, EventArgs e)
        {
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            wp.bw = bw;

            port.Text = "9000";
            //this.Text = "Skytraq Final Test Server 1.0.0.5";
            this.Text = "Skytraq Final Test " + Assembly.GetExecutingAssembly().GetName().Version;

            //V815 doesn't show
            if (Program.version.IsV815())
            {
                crcLabel.Visible = false;
                modeLabel.Visible = false;
                crcValue.Visible = false;
                workingMode.Visible = false;
            }

            LoginForm form = new LoginForm();
            if(DialogResult.OK != form.ShowDialog())
            {
                this.Close();
                return;
            }
            workingNumber = form.workNo.Text;
            logPath = System.Environment.CurrentDirectory + "\\Log\\" + workingNumber;
            try
            {
                Directory.CreateDirectory(logPath);
            }
            catch
            {
                MessageBox.Show("無法建立Log目錄！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        //背景執行
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            server = new TcpServer(workingNumber);
            server.ListenToConnection(Convert.ToInt32(port.Text));
            e.Cancel = true;

            //p.duration = w.ElapsedMilliseconds;
            w.Stop();
        }

        //處理進度
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerReportParam r = e.UserState as WorkerReportParam;

            if (r.reportType == WorkerReportParam.ReportType.ShowIP)
            {
                ip.Text = r.output;
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowProgress)
            {
                AddConsoleMessage(r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.CrcError)
            {
                crcValue.Text = "####";
                crcValue.ForeColor = Color.Red;
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowCrc)
            {
                crcValue.Text = r.output;
                crcValue.ForeColor = Color.Blue;
            }
            else if (r.reportType == WorkerReportParam.ReportType.DisplayMode)
            {
                workingMode.Text = r.output;
                workingMode.ForeColor = Color.Blue;
            }
            else if (r.reportType == WorkerReportParam.ReportType.DisplayError)
            {
                workingMode.Text = r.output;
                workingMode.ForeColor = Color.Red;
            }
        }

        //執行完成
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);
        }

        private bool serverStart = false;
        private void start_Click(object sender, EventArgs e)
        {
            if (serverStart)
            {   //Go stop
                start.Text = "Start";
                bw.CancelAsync();
                serverStart = false;
            }
            else
            {   //Go start
                AddConsoleMessage("Host Name :" + Dns.GetHostName());
                //consoleMsg.Items.Add("Host Name :" + Dns.GetHostName());
                String ip = TcpServer.GetIpAddress();
                if (ip.Length > 0)
                {
                    AddConsoleMessage("Server IP : " + ip);
                    //consoleMsg.Items.Add("Server IP : " + ip);
                }
                else
                {
                    AddConsoleMessage("No available network interface");
                    //consoleMsg.Items.Add("No available network interface");
                }
                start.Text = "Stop";
                bw.RunWorkerAsync();
                serverStart = true;
            }
        }
        public enum TestType
        {
            AllPass,
            TestNG,
            TestException
        }
        public static TestType testType = TestType.AllPass;

        private void allPass_CheckedChanged(object sender, EventArgs e)
        {
            testType = TestType.AllPass;
        }

        private void testNg_CheckedChanged(object sender, EventArgs e)
        {
            testType = TestType.TestNG;
        }

        private void testException_CheckedChanged(object sender, EventArgs e)
        {
            testType = TestType.TestException;
        }

        private void notify00_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Notify Got!");
        }

        private void ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GpsTester.QuitAllProcess();
        }

        static public bool noAckValue = false;
        static public bool noTestValue = false;
        private void noAck_CheckedChanged(object sender, EventArgs e)
        {
            noAckValue = (sender as CheckBox).Checked;
        }

        private void noTest_CheckedChanged(object sender, EventArgs e)
        {
            noTestValue = (sender as CheckBox).Checked;
        }

        public void AddConsoleMessage(string s)
        {
            bool scroll = (consoleMsg.TopIndex == consoleMsg.Items.Count - (int)(consoleMsg.Height / consoleMsg.ItemHeight));
            consoleMsg.Items.Add(s);
            if (scroll)
            {
                consoleMsg.TopIndex = consoleMsg.Items.Count - (int)(consoleMsg.Height / consoleMsg.ItemHeight);
            }

            // 20150507 Spec form Angus.
            //4. Log file檔名之格式為 " A5xx-xxxxxxxxxxx_V8XX_20150506_140520.log”,檔名上顯示日期及時間.
            if (logName == null)
            {
                Module mod = typeof(ServerForm).Assembly.GetModules()[0];
                logName = workingNumber + "_" + mod.Name.Substring(0, 4) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Server.log";
            }
            StreamWriter w = File.AppendText(logPath + "\\" + logName);
            w.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), s);
            w.Close();
        }
    }
 }
