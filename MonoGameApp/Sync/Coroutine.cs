using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGameApp.Sync
{
    public readonly struct SynchronizationContextAwaiter : INotifyCompletion
    {
        private static readonly SendOrPostCallback postCallback = state => ((Action)state)();

        private readonly SynchronizationContext context;
        public SynchronizationContextAwaiter(SynchronizationContext context)
        {
            this.context = context;
        }

        public bool IsCompleted => context == SynchronizationContext.Current;

        public void OnCompleted(Action continuation)
        {
            context.Post(postCallback, continuation);
        }

        public void GetResult() { }
    }

    static class Coroutine
    {
        public static MonogameSynchronizationContext SyncContext { get; } = new MonogameSynchronizationContext();

        public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
        {
            return new SynchronizationContextAwaiter(context);
        }

        public static Task StartCoroutine(Func<Task> coroutine)
        {
            var oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            async Task LocalExecute()
            {
                try
                {
                    await coroutine();
                } 
                catch(Exception e)
                {
                    throw e;
                }
            }

            Task toRet = LocalExecute();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return toRet;
        }

        public async static Task ContinueOnMainThread()
        {
            await SyncContext;
        }

        public static async Task ContinueOnMainThread(this Task task)
        {
            await SyncContext;
            await task;
        }

        public static void ExecuteContinuations()
        {
            SyncContext.ExecuteContinuations();
        }

    }
}
