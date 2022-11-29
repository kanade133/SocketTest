using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest
{
    public static class SocketExtension
    {
        public static int SendVarData(this Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;
            byte[] datasize = new byte[4];

            try
            {
                datasize = BitConverter.GetBytes(size);
                sent = s.Send(datasize);

                while (total < size)
                {
                    sent = s.Send(data, total, dataleft, SocketFlags.None);
                    total += sent;
                    dataleft -= sent;
                }

                return total;
            }
            catch
            {
                return 3;

            }
        }

        public static int SendString(this Socket s, string sendStr)
        {
            return s.SendVarData(Encoding.Unicode.GetBytes(sendStr));
        }

        public static void SendBigFile(this Socket clientSocket, string filePath, int packetSize = 10000)
        {
            //创建一个文件对象
            FileInfo fileInfo = new FileInfo(filePath);

            //打开文件流
            using (FileStream fileStream = fileInfo.OpenRead())
            {
                //包的数量
                int packetCount = (int)(fileStream.Length / packetSize);

                //最后一个包的大小
                int lastDataPacket = (int)(fileStream.Length - packetSize * packetCount);

                //发送[文件名]到客户端
                clientSocket.SendString(fileInfo.Name);

                //发送[包的大小]到客户端
                clientSocket.SendString(packetSize.ToString());

                //发送[包的总数量]到客户端
                clientSocket.SendString(packetCount.ToString());

                //发送[最后一个包的大小]到客户端
                clientSocket.SendString(lastDataPacket.ToString());

                //数据包
                byte[] data = new byte[packetSize];
                //开始循环发送数据包
                for (int i = 0; i < packetCount; i++)
                {
                    //从文件流读取数据并填充数据包
                    fileStream.Read(data, 0, data.Length);
                    //发送数据包
                    clientSocket.SendVarData(data);
                }

                //如果还有多余的数据包，则应该发送完毕！
                if (lastDataPacket != 0)
                {
                    data = new byte[lastDataPacket];
                    fileStream.Read(data, 0, data.Length);
                    clientSocket.SendVarData(data);
                }
                //关闭文件流
                fileStream.Close();
            }
        }

        public static byte[] ReceiveVarData(this Socket s)
        {
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];
            recv = s.Receive(datasize, 0, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];
            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, SocketFlags.None);
                if (recv == 0)
                {
                    data = null;
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        }

        public static string ReceiveString(this Socket s)
        {
            return Encoding.Unicode.GetString(s.ReceiveVarData());
        }

        public static void ReceiveBigFile(this Socket clientSocket, string filePath)
        {
            //获得[文件名]   
            string fileName = clientSocket.ReceiveString();

            //获得[包的大小]   
            string packetSize = clientSocket.ReceiveString();

            //获得[包的总数量]   
            int packetCount = int.Parse(clientSocket.ReceiveString());

            //获得[最后一个包的大小]   
            int lastDataPacket = int.Parse(clientSocket.ReceiveString());

            //创建一个新文件   
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < packetCount; i++)
                {
                    byte[] data = clientSocket.ReceiveVarData();
                    //将接收到的数据包写入到文件流对象   
                    stream.Write(data, 0, data.Length);
                }
                if (lastDataPacket != 0)
                {
                    byte[] data = clientSocket.ReceiveVarData();
                    //将接收到的数据包写入到文件流对象   
                    stream.Write(data, 0, data.Length);
                }
                //关闭文件流   
                stream.Close();
            }
        }
    }
}
