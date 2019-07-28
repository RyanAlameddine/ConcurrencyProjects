using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            UdpClient client = new UdpClient();
            client.Send(new byte[] { 2, 5, 5, 6, 5 }, 5, "192.168.1.172", 9999);  
            */

            TcpClient client = new TcpClient();
            client.Connect("192.168.1.172", 9999);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4] { 2, 5, 5, 6 };
            while (true)
            {
                Thread.Sleep(500);
                stream.Write(buffer, 0, 4);
                stream.Read(buffer, 0, 4);

                for (int i = 0; i < 4; i++)
                {
                    Console.Write(buffer[i] + " ");
                }
                Console.WriteLine();
            }

            //Task t = Task.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, 0, 4, null);

        }
    }
}
