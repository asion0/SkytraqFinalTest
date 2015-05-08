using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketDemo;

namespace SkytraqFinalTestServer
{
    public class HandleClient
    {
        private TcpClient mTcpClient;
        private string workingNumber;

        public static void AddMessage(string msg)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = msg;
            ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
        }

        public static void SendMessage(string msg)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.reportType = WorkerReportParam.ReportType.GotMessage;
            r.output = msg;
            ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
        }
        
        public HandleClient(TcpClient _tmpTcpClient, string w)
        {
            this.mTcpClient = _tmpTcpClient;
            workingNumber = w;
        }

        public void Communicate()
        {
            try
            {
                string msg = CommunicationBase.ReceiveMsg(this.mTcpClient);
                if (msg.Length > 0)
                {
                    GpsTester g = new GpsTester();
                    //g.DoCommand(msg);
                    string retCmd = g.DoCommand(msg, mTcpClient, workingNumber);
                    if (retCmd.Length != 0)
                    {
                        CommunicationBase.SendMsg(retCmd, this.mTcpClient);
                    }
                }
                else
                {
                    Thread.Sleep(20);
                }
            }
            catch
            {
                //Console.WriteLine("客戶端強制關閉連線!");
                this.mTcpClient.Close();
                //Console.Read();
            }
            //this.mTcpClient.Close();
            //AddMessage("TcpClient closed.");
        } // end HandleClient()


    } // end Class

    class TcpServer
    {
        public TcpServer(string w)
        {
            workingNumber = w;
        }
        private string workingNumber;        
        
        public static void AddMessage(string msg)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = msg;
            ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));
        }

        public static  string GetIpAddress()
        {
            //取得本機名稱
            string hostName = Dns.GetHostName();

            //取得本機IP
            IPAddress[] ipa = Dns.GetHostAddresses(hostName);

            // 取得所有 IP 位址
            int num = 0;
            foreach (IPAddress ipaddress in ipa)
            {
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    break;
                }
                num = num + 1;
            }

            if (num > ipa.Length)
            {
                return "";
            }

            return ipa[num].ToString();
        }

        public void ListenToConnection(int port)
        {
            string ip = GetIpAddress();

            WorkerReportParam r = new WorkerReportParam();
            r.reportType = WorkerReportParam.ReportType.ShowIP;
            r.output = ip.ToString();
            ServerForm.wp.bw.ReportProgress(0, new WorkerReportParam(r));

            //建立本機端的IPEndPoint物件
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);

            //建立TcpListener物件
            TcpListener tcpListener = new TcpListener(ipe);

            //開始監聽port
            tcpListener.Start();
            //Console.WriteLine("等待客戶端連線中... \n");
            AddMessage("Server Ready...");

            TcpClient tmpTcpClient;
            int numberOfClients = 0;
            while (!ServerForm.wp.bw.CancellationPending)
            {
                try
                {
                    //建立與客戶端的連線
                    if (tcpListener.Pending() == false)
                    {
                        Thread.Sleep(5);
                    }
                    else
                    {
                        tmpTcpClient = tcpListener.AcceptTcpClient();

                        if (tmpTcpClient.Connected)
                        {
                            //Console.WriteLine("連線成功!");
                            AddMessage("Client connected...");
                            HandleClient handleClient = new HandleClient(tmpTcpClient, workingNumber);
                            Thread myThread = new Thread(new ThreadStart(handleClient.Communicate));
                            numberOfClients += 1;
                            myThread.IsBackground = true;
                            myThread.Start();
                            myThread.Name = tmpTcpClient.Client.RemoteEndPoint.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Console.Read();
                }
            } // end while
            tcpListener.Stop();
            AddMessage("Server End");
        } // end ListenToConnect()
    }
}
