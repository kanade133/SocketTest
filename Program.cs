using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter 1(Client) or 2(Server):");
            int i = 0;
            int.TryParse(Console.ReadLine(), out i);
            if (i == 1)
            {
                Console.WriteLine("Enter reciver IP:");
                string ip = Console.ReadLine();
                Console.WriteLine("Enter File Path:");
                string path = Console.ReadLine();
                using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), 10088));
                    clientSocket.SendBigFile(path);
                    clientSocket.Close();
                }
                Console.WriteLine("Send successfully!");
                Console.WriteLine("Press any key to Exit...");
                Console.ReadKey();
            }
            else if (i == 2)
            {
                SocketServer.Init();
                Console.WriteLine("Press any key to Exit...");
                Console.ReadKey();
                SocketServer.Exit();
            }
        }
    }
}
