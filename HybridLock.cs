using System;
using System.Threading;

namespace Synchronization
{
    public sealed class HybridLock : IDisposable
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly int _spinCount = 1000;

        private int _waiters;
        private int _ownerId;
        private int _recursionCount;

        private static int ThreadId => Thread.CurrentThread.ManagedThreadId;

        public void Enter()
        {
            if(ThreadId == _ownerId)
            {
                _recursionCount++;
                return;
            }

            if (WaitInLoop()) return;

            if (Interlocked.Increment(ref _waiters) > 1)
            {
                _resetEvent.WaitOne();
            }

            GotLock();
        }

        public void Leave()
        {
            if (ThreadId != _ownerId)
            {
                throw new InvalidOperationException();
            }

            if (--_recursionCount > 0) return;

            _ownerId = 0;

            if (Interlocked.Decrement(ref _waiters) == 0) return;

            _resetEvent.Set();
        }

        public void Dispose()
        {
            _resetEvent.Dispose();
        }

        private void GotLock()
        {
            _ownerId = ThreadId;
            _recursionCount = 1;
        }

        private bool WaitInLoop()
        {
            SpinWait spinWait = new SpinWait();

            for (int i = 0; i < _spinCount; i++)
            {
                if (Interlocked.CompareExchange(ref _waiters, 1, 0) == 0)
                {
                    GotLock();
                    return true;
                }

                spinWait.SpinOnce();
            }

            return false;
        }
    }
}
