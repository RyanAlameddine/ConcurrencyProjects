using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BadRaceConditions
{
    public class GMRTask
    {
        public struct GMRAwaiter : INotifyCompletion
        {
            private GMRTask task;

            public GMRAwaiter(GMRTask task)
            {
                this.task = task;
            }

            public bool IsCompleted => task.IsCompletedSuccessfully || task.IsFaulted;

            public void GetResult()
            {
                if (task.IsFaulted)
                {
                    if(task.Exception is AggregateException ar)
                    {
                        if(ar.InnerExceptions.Count == 1)
                        {
                            throw ar.InnerException;
                        }
                        else
                        {
                            throw ar;
                        }
                    }
                    else if(task.Exception == null)
                    {
                        throw new Exception("Promise rejected without exception");
                    }
                    else
                    {
                        throw task.Exception;
                    }
                }
            }

            public void OnCompleted(Action continuation)
            {
                lock (task.lockObject)
                {
                    if (IsCompleted)
                    {
                        continuation();
                        return;
                    }
                }
                task.continuation += (t) =>
                {
                    continuation();
                };
            }
        }

        protected readonly object lockObject = new object();
        public Exception Exception { get; protected set; }
        public bool IsCompletedSuccessfully { get; protected set; }
        public bool IsFaulted { get; protected set; }

        private Action<GMRTask> continuation;

        protected GMRTask()
        {

        }

        public GMRTask ContinueWith(Action<GMRTask> continuation)
        {
            GMRTask continueTask = new GMRTask();
            Action<GMRTask> onContinue = (t) =>
            {
                continuation(t);
                continueTask.continuation?.Invoke(continueTask);
            };
            this.continuation += onContinue;
            return continueTask;
        }

        public GMRTask ContinueWith(Func<GMRTask, Task> continuation)
        {
            GMRTask continueTask = new GMRTask();
            Action<GMRTask> onContinue = (t) =>
            {
                Task awaitTask = continuation(t);
                awaitTask.ContinueWith((t2) =>
                {
                    continueTask.continuation?.Invoke(continueTask);
                });
            };
            this.continuation += onContinue;
            return continueTask;
        }

        public GMRTask(Action<Action, Action<Exception>> toRunFunction)
        {
            toRunFunction(() =>
            {
                lock (lockObject)
                {
                    //reslove
                    IsCompletedSuccessfully = true;
                    IsFaulted = false;
                    continuation?.Invoke(this);
                }
            }, ex =>
            {
                lock (lockObject)
                {
                    //reject
                    IsCompletedSuccessfully = false;
                    IsFaulted = true;
                    Exception = ex;
                    continuation?.Invoke(this);
                }
            });
        }

        public GMRAwaiter GetAwaiter()
        {
            return new GMRAwaiter(this);
        }
    }
}
