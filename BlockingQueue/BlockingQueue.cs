using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BlockingQueue
{
    class BlockingQueue<T>
    {
        private Queue<T> pi;
        private readonly object lockObject = new object();

        public BlockingQueue()
        {
            pi = new Queue<T>();
        }

        public void Enqueue(T data)
        {
            lock (lockObject)
            {
                pi.Enqueue(data);
                Monitor.PulseAll(pi);
            }
        }

        public bool IsEmpty()
        {
            lock (lockObject)
            {
                return pi.Count == 0;
            }
        }

        public T Dequeue()
        {
            lock (lockObject)
            {
                while (pi.Count == 0)
                {
                    Monitor.Wait(lockObject);
                }
                return pi.Dequeue();
            }
        }
    }
}
