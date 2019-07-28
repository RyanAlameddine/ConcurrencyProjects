using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    class Program
    {
        static async Task HandleClient(NamedPipeServerStream stream)
        {
            using (stream)
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                var line = await reader.ReadLineAsync();
                if(line == null)
                {
                    return;
                }
                await writer.WriteLineAsync(line);
            }
        }

        static NamedPipeServerStream GetNewStream()
        {
            return new NamedPipeServerStream("test-pipe", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

        }

        static async Task Main(string[] args)
        {
            NamedPipeServerStream stream = GetNewStream();

            LinkedList<Task> taskList = new LinkedList<Task>();

            Task connectTask = stream.WaitForConnectionAsync();

            taskList.AddFirst(connectTask);

            while (true)
            {
                var completed = await Task.WhenAny(taskList);

                if(completed == connectTask)
                {
                    var localStream = stream;
                    stream = GetNewStream();
                    connectTask = stream.WaitForConnectionAsync();
                    taskList.First.Value = connectTask;

                    taskList.AddLast(HandleClient(localStream));
                }
                else
                {
                    taskList.Remove(completed);
                }
            }
        }

    }
}
