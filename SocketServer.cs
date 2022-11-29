using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    public class SocketServer
    {
        private static Socket serverSocket;
        public static void Init()
        {
            //服务器IP地址
            IPAddress ip = null;
            IPAddress[] ipadrlist = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    IPAddress.Parse(ipa.ToString());
            }
            if (ip == null)
            {
                ip = IPAddress.Parse("127.0.0.1");
            }
            int myProt = 10088;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口
            serverSocket.Listen(1);    //设定最多10个排队连接请求
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();
        }
        public static void Exit()
        {
            serverSocket.Close();
            serverSocket = null;
        }
        private static void ListenClientConnect()
        {
            while (serverSocket != null)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    Console.WriteLine($"接到来自{clientSocket.RemoteEndPoint.ToString()}的请求");
                    Task.Factory.StartNew(ServerTask, clientSocket);
                }
                catch
                {
                    break;
                }
            }
        }
        public static void ServerTask(object socket)
        {
            Socket clientSocket = socket as Socket;

            //获得客户端节点对象
            IPEndPoint clientep = (IPEndPoint)clientSocket.RemoteEndPoint;

            string fullPath = $"D:\\{clientep.Address}.data";
            clientSocket.ReceiveBigFile(fullPath);

            //关闭套接字   
            clientSocket.Close();
            Console.WriteLine($"接收来自{clientep.Address}的文件完成，已写入{fullPath}");
        }
    }
}
