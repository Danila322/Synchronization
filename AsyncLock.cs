using Synchronization.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization
{
    public sealed class AsyncLock
    {
        private int _waiters;

        private readonly SafeQueue<TaskCompletionSource> _queue = new();

        public Task WaitAsync()
        {
            if (Interlocked.Increment(ref _waiters) == 1) return Task.CompletedTask;

            var source = new TaskCompletionSource();
            _queue.Push(source);
            
            return source.Task;
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref _waiters) == 0) return;

            var source = _queue.WaitAndTake();
            source.SetResult();
        }
    }
}
