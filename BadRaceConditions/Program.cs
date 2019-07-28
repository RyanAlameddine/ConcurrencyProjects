using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BadRaceConditions
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GMRTask<string[]> task = new GMRTask<string[]>((reslove, reject) =>
            {

                Task.Delay(1000).ContinueWith(t2 =>
                {
                    return File.ReadAllLinesAsync(@"Z:\Documents\Visual Studio 2019\Projects\Boolman\Boolman\Wireman.cs").ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            reslove(t.Result);
                        }
                        else
                        {
                            reject(t.Exception);
                        }
                        return 42;
                    });
                });
                
            });

            Thread thr = new Thread(() =>
            {
                task.ContinueWith(async tt =>
                {
                    await Task.Delay(500);
                    return tt.Result;
                }).ContinueWith(ts =>
                {
                    return ts.Result;
                });
            });
            thr.Start();

            Thread.Sleep(5000);
            string[] data;
            data = await task;
            Console.WriteLine("Awaited Successfully");
        }
    }
}
