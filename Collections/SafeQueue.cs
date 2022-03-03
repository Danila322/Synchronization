using System.Threading;

namespace Synchronization.Collections
{
    public class SafeQueue<T>
    {
        private const int FreeState = 0;
        private const int ReadState = 1;
        private const int WriteState = 2;

        private int _state;
        private int _writers;
        private Node<T> _head;
        private Node<T> _tail;

        public SafeQueue()
        {
            _head = _tail = new Node<T>();
        }

        public bool IsEmpty => _head == _tail;

        private bool IsFree => _state == FreeState;
        private bool IsReading => _state == ReadState;
        private bool IsWriting => _state == WriteState;

        public T WaitAndTake()
        {
            WaitForRead();

            _head = _head.Next;
            var result =  _head.Data;

            MakeFree();

            return result;
        }

        public void Push(T item)
        {
            AddWriter();

            var newNode = new Node<T>() { Data = item };
            var oldTail = Interlocked.Exchange(ref _tail, newNode);
            
            oldTail.ConnectNext(newNode);

            SubtractWriter();
        }

        private void WaitForNewItem()
        {
            SpinWait spinWait = new SpinWait();

            while (IsEmpty)
            {
                spinWait.SpinOnce();
            }
        }

        private void WaitForRead()
        {
            do
            {
                WaitForNewItem();

                WaitForReadState();

                if(IsEmpty) MakeFree();
            }
            while (IsEmpty);
        }

        private void WaitForReadState()
        {
            SpinWait spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref _state, ReadState, FreeState) != FreeState)
            {
                spinWait.SpinOnce();
            }
        }
        
        private void WaitForWriteState()
        {
            SpinWait spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref _state, WriteState, FreeState) == ReadState)
            {
                spinWait.SpinOnce();
            }
        }

        private void MakeFree()
        {
            Interlocked.Exchange(ref _state, FreeState);
        }

        private void AddWriter()
        {
            if(Interlocked.Increment(ref _writers) == 1 || !IsWriting)
            {
                WaitForWriteState();
            }
        }

        private void SubtractWriter()
        {
            if(Interlocked.Decrement(ref _writers) == 0)
            {
                MakeFree();
            }
        }
    }
}
