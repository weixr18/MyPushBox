using System;
using System.Collections.Generic;

namespace MyPushBox {

    class PriorityQueue<T>
    {
        IComparer<T> comparer;
        public T[] heap;

        public int Count { get; private set; }

        public PriorityQueue() : this(null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }

        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
            this.heap = new T[capacity];
        }

        public void Push(T v)
        {
            if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
            heap[Count] = v;
            SiftUp(Count++);
        }

        public T Pop()
        {
            var v = Top();
            heap[0] = heap[--Count];
            if (Count > 0) SiftDown(0);
            return v;
        }

        public T Top()
        {
            if (Count > 0) return heap[0];
            throw new InvalidOperationException("Priority queue is Empty!");
        }

        void SiftUp(int n)
        {
            var v = heap[n];
            for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
            heap[n] = v;
        }

        void SiftDown(int n)
        {
            var v = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
                if (comparer.Compare(v, heap[n2]) >= 0) break;
                heap[n] = heap[n2];
            }
            heap[n] = v;
        }

        public bool Has(T v)
        {
            for (int i = 0; i < heap.GetLength(0); i++)
            {
                if (heap[i].Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public void Heapify()
        {
            int n = heap.GetLength(0);

            for (int i = n / 2; i >= 0; i--)
            {
                SiftUp(i);
            }

            /*
            Transform bottom-up.  The largest index there's any point to looking at
            is the largest with a child index in-range, so must have 2*i + 1 < n,
            or i < (n-1)/2.  If n is even = 2*j, this is (2*j-1)/2 = j-1/2 so
            j-1 is the largest, which is n//2 - 1.  If n is odd = 2*j+1, this is
            (2*j+1-1)/2 = j so j-1 is the largest, and that's again n//2-1.
            */

            //for i in reversed(range(n//2)):
            //    _siftup(x, i)

        }
    }


}