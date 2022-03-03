using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization.Collections
{
    internal class Node<T>
    {
        private Node<T> _next;

        public T Data { get; init; }

        public Node<T> Next => _next;

        public void ConnectNext(Node<T> next)
        {
            _next = next;
        }
    }
}
