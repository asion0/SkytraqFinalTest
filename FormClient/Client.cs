using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketDemo;


namespace FormClient
{
    public interface IAddMessage
    {
        int AddMessage(string s);
    }

    public class Client
    {
        TcpClient tcpClient = null;
        IAddMessage console = null;
        /// <summary>
        /// 連線至主機
        /// </summary>
        /// 
        private void AddMessage(string s)
        {
            if (console == null)
                return;

            console.AddMessage(s);
        }

        public void ConnectToServer(string hostIP, int port)
        {
            //先建立IPAddress物件,IP為欲連線主機之IP
            IPAddress ipa = IPAddress.Parse(hostIP);

            //建立IPEndPoint
            IPEndPoint ipe = new IPEndPoint(ipa, port);

            //先建立一個TcpClient;
            tcpClient = new TcpClient();

            //開始連線
            try
            {
                Console.WriteLine("主機IP=" + ipa.ToString());
                Console.WriteLine("連線至主機中...\n");
                tcpClient.Connect(ipe);

                if (tcpClient.Connected)
                {

                }
                else
                {
                    Console.WriteLine("連線失敗!");
                }
                //Console.Read();
            }
            catch (Exception ex)
            {
                tcpClient.Close();
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }
        public void Send(string s)
        {
            if (tcpClient == null || !tcpClient.Connected)
            {
                return;
            }
            AddMessage("Send : " + s);

            try
            {

                //Console.WriteLine("連線成功!");
                CommunicationBase.SendMsg(s, tcpClient);
                byte[] rev = null;
                int len = CommunicationBase.ReceiveByte(tcpClient, ref rev);
                if (len != 0)
                {
                    Encoding gn936 = Encoding.GetEncoding(936);
                    string strReceiveCmd = gn936.GetString(rev, 0, len);
                    Console.Write(strReceiveCmd);
                    AddMessage("Receive : " + strReceiveCmd);
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public void CloseConnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
            //Console.WriteLine(ex.Message);
        }

    }
 }
