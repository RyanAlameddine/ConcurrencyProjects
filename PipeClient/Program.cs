using System;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipeClient
{
    class Program
    {

        public struct DataStruct
        {
            public int x;
            public long y;
            public double z;
        }
        static async Task Main(string[] args)
        {
            //using (NamedPipeClientStream client = new NamedPipeClientStream(".", "test-pipe", PipeDirection.InOut))
            //{
            //    await client.ConnectAsync();
            //}

            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("TestingMemoryMappedFile", 1024, MemoryMappedFileAccess.ReadWrite);
            var accessor = mmf.CreateViewAccessor();

            var mutex = Mutex.OpenExisting("TestingMMFMutex");

            //accessor.Read(0, out DataStruct ds);

            while (true)
            {
                mutex.WaitOne();
                accessor.Read(0, out int length);
                if(length > 0)
                {
                    byte[] data = new byte[length];
                    accessor.ReadArray(4, data, 0, length);
                    Console.WriteLine(Encoding.UTF8.GetString(data));
                }
            }

            Console.WriteLine("Hello World");
        }
    }
}
