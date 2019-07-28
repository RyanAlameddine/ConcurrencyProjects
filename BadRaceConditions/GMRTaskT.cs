using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BadRaceConditions
{
    public class GMRTask<T> : GMRTask
    {
        public struct GMRAwaiterT : INotifyCompletion
        {
            private GMRTask<T> task;

            public GMRAwaiterT(GMRTask<T> task)
            {
                this.task = task;
            }

            public bool IsCompleted => task.IsCompletedSuccessfully || task.IsFaulted;

            public T GetResult()
            {
                if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ar)
                    {
                        if (ar.InnerExceptions.Count == 1)
                        {
                            throw ar.InnerException;
                        }
                        else
                        {
                            throw ar;
                        }
                    }
                    else if (task.Exception == null)
                    {
                        throw new Exception("Promise rejected without exception");
                    }
                    else
                    {
                        throw task.Exception;
                    }
                }

                return task.Result;
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

        public T Result { get; private set; }

        private Action<GMRTask<T>> continuation;

        private GMRTask()
        {

        }

        public new GMRTask<T> ContinueWith(Action<GMRTask> continuation)
        {
            if(this.IsCompletedSuccessfully || this.IsFaulted)
            {
                continuation(this);
                return this;
            }

            GMRTask<T> continueTask = new GMRTask<T>();
            Action<GMRTask<T>> onContinue = (t) =>
            {
                continuation(t);
                continueTask.continuation?.Invoke(continueTask);
            };
            this.continuation += onContinue;
            return continueTask;
        }

        public GMRTask<TResult> ContinueWith<TResult>(Func<GMRTask<T>, Task<TResult>> continuation)
        {
            if (this.IsCompletedSuccessfully || this.IsFaulted)
            {
                GMRTask<TResult> continueTaskSync = new GMRTask<TResult>();
                continuation(this).ContinueWith(t2 =>
                {
                    continueTaskSync.Result = t2.Result;
                    
                    continueTaskSync.continuation?.Invoke(continueTaskSync);
                });

                return continueTaskSync;
            }

            GMRTask<TResult> continueTask = new GMRTask<TResult>();
            Action<GMRTask<T>> onContinue = (t) =>
            {
                Task<TResult> awaitTask = continuation(t);
                awaitTask.ContinueWith((t2) =>
                {
                    continueTask.Result = t2.Result;
                    
                    continueTask.continuation?.Invoke(continueTask);
                });
            };
            this.continuation += onContinue;
            return continueTask;
        }

        public GMRTask<TResult> ContinueWith<TResult>(Func<GMRTask<T>, TResult> continuation)
        {
            if (this.IsCompletedSuccessfully || this.IsFaulted)
            {
                GMRTask<TResult> continueTaskSync = new GMRTask<TResult>();
                continueTaskSync.Result = continuation(this);
                continueTaskSync.continuation?.Invoke(continueTaskSync);

                return continueTaskSync;
            }

            GMRTask<TResult> continueTask = new GMRTask<TResult>();
            Action<GMRTask<T>> onContinue = (t) =>
            {
                var res = continuation(t);
                continueTask.Result = res;
                
                continueTask.continuation?.Invoke(continueTask);
            };
            this.continuation += onContinue;
            return continueTask;
        }

        public GMRTask(Action<Action<T>, Action<Exception>> toRunFunction)
        {
            toRunFunction((val) =>
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
            }); ;
        }


        public new GMRAwaiterT GetAwaiter()
        {
            return new GMRAwaiterT(this);
        }
    }
}
