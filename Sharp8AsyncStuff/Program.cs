using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Sharp8AsyncStuff
{
    class Program
    {
        static async IAsyncEnumerable<int> GetItemsAsync(Stream reader)
        {
            byte[] bytes = new byte[4];
            while (reader.CanRead)
            {
                await reader.ReadAsync(bytes.AsMemory());
                int retVal = BitConverter.ToInt32(bytes.AsSpan());
                if(retVal == 0)
                {
                    yield break;
                }
                yield return retVal;
            }
            for(int i = 0; i < 10; i++)
            {
                yield return i;
            }
        }


        static async Task Main(string[] args)
        {
            UdpClient client1 = new UdpClient(9999);
            UdpClient client2 = new UdpClient(1000);


            Socket client1Socket = client1.Client;
            Socket client2Socket = client2.Client;

            List<Socket> readSockets = new List<Socket>();

            while (true)
            {
                readSockets.Clear();
                readSockets.Add(client1Socket);
                readSockets.Add(client2Socket);

                client1Socket.Poll(50, SelectMode.SelectRead);

                Socket.Select(readSockets, null, null, 1000);
            }












            byte[] data = new byte[20];
            {
                BitConverter.TryWriteBytes(data.AsSpan(), 42);
                BitConverter.TryWriteBytes(data.AsSpan().Slice(4), 56);
                BitConverter.TryWriteBytes(data.AsSpan().Slice(8), 94);
                BitConverter.TryWriteBytes(data.AsSpan().Slice(12), 78);
                BitConverter.TryWriteBytes(data.AsSpan().Slice(16), 0);
            }

            MemoryStream memStream = new MemoryStream(data);

            int[] arr = new int[42];
            await arr[1..5].ToAsyncEnumerable().SelectAwait(async x =>
            {
                await Task.Delay(500);
                return x * 2;
            }).ToArrayAsync();


            var items = GetItemsAsync(memStream);

            await foreach(var item in items.WhereAwait(async x =>
            {
                await Task.Delay(x);
                return x > 50;
            }))
            {

            }

            //int[] items = GetItems().Where(x => x > 4).Select(x => x / 2).ToArray();

            Console.WriteLine("Hello World!");
        }
    }
}
