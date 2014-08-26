using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SocketDemo
{
    /// <summary>
    /// CommunicationBase給客戶端和主機端共用，可傳送接收訊息
    /// </summary>
    public class CommunicationBase
    {
        /// <summary>
        /// 傳送訊息
        /// </summary>
        /// <param name="msg">要傳送的訊息</param>
        /// <param name="tmpTcpClient">TcpClient</param>
        public static void SendMsg(string msg, TcpClient tmpTcpClient)
        {
            NetworkStream ns = tmpTcpClient.GetStream();
            if (ns.CanWrite)
            {
                byte[] msgByte = Encoding.Default.GetBytes(msg);
                ns.Write(msgByte, 0, msgByte.Length);
            }
        }

        /// <summary>
        /// 接收訊息
        /// </summary>
        /// <param name="tmpTcpClient">TcpClient</param>
        /// <returns>接收到的訊息</returns>
        public static string ReceiveMsg(TcpClient tmpTcpClient)
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[tmpTcpClient.ReceiveBufferSize];
            int numberOfBytesRead = 0;
            NetworkStream ns = tmpTcpClient.GetStream();

            if (ns.CanRead)
            {
                do
                {
                    numberOfBytesRead = ns.Read(receiveBytes, 0, tmpTcpClient.ReceiveBufferSize);
                    receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                }
                while (ns.DataAvailable);
            }
            return receiveMsg;
        }

        public static int ReceiveByte(TcpClient tmpTcpClient, ref byte[] receiveBytes)
        {
            //string receiveMsg = string.Empty;
            receiveBytes = new byte[tmpTcpClient.ReceiveBufferSize];
            int numberOfBytesRead = 0;
            NetworkStream ns = tmpTcpClient.GetStream();

            if (ns.CanRead)
            {
                do
                {
                    numberOfBytesRead = ns.Read(receiveBytes, 0, tmpTcpClient.ReceiveBufferSize);
                    //receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                }
                while (ns.DataAvailable);
            }
            return numberOfBytesRead;
        }
    }
}
