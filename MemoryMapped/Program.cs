using System;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace MemoryMapped
{
    public struct DataStruct
    {
        public int x;
        public long y;
        public double z;
    }

    class Program
    {

        static void Main(string[] args)
        {
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("TestingMemoryMappedFile", 1024, MemoryMappedFileAccess.ReadWrite);

            var accessor = mmf.CreateViewAccessor();

            Mutex mutex = new Mutex(false, "TestingMMFMutex");


            //DataStruct ds = new DataStruct();
            //ds.x = 42;
            //ds.y = 856;
            //ds.z = 45234.52345;

            //accessor.Write(0, ref ds);

            while (true)
            {
                var line = Console.ReadLine();
                var bytes = Encoding.UTF8.GetBytes(line);
                mutex.WaitOne();
                accessor.Write(0, bytes.Length);
                accessor.WriteArray(4, bytes, 0, bytes.Length);
                mutex.ReleaseMutex();
            }


            Console.WriteLine("Hello World!");
        }
    }
}
