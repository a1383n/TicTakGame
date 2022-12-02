using System;
using System.Threading;
using System.Threading.Tasks;

namespace TicTakGame.Utils
{
    public class BackgroundQueue
    {
        public Task previousTask = Task.FromResult(true);
        private object key = new object();
        public Task QueueTask(Action action)
        {
            lock (key)
            {
                previousTask = previousTask.ContinueWith(t => action()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                return previousTask;
            }
        }

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => work()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }

        public Task QueueTask<T>(Task<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => work
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }
    }
}