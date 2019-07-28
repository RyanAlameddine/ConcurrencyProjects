using System.Threading;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameApp.Sync
{
    public class MonogameSynchronizationContext : SynchronizationContext
    {
        public struct PostPair
        {
            public SendOrPostCallback Callback { get; }
            public object State { get; }

            public PostPair(SendOrPostCallback d, object state)
            {
                Callback = d;
                State = state;
            }
        }

        private readonly object lockObject = new object();
        List<PostPair> postedObjects = new List<PostPair>();
        List<PostPair> postedObjects2 = new List<PostPair>();

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (lockObject)
            {
                postedObjects.Add(new PostPair(d, state));
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            //Send is blocking
            throw new InvalidOperationException("Cannot Send");
        }

        public void ExecuteContinuations()
        {
            lock (lockObject)
            {
                var tmp = postedObjects;
                postedObjects = postedObjects2;
                postedObjects2 = tmp;
                postedObjects.Clear();
            }
            foreach (var obj in postedObjects2)
            {
                obj.Callback(obj.State);
            }
        }
    }
}
