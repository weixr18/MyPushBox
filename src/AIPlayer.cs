//#define _DEBUG_

using System;
using System.IO;
using System.Collections.Generic;


namespace MyPushBox {

    class GameState : IComparable
    {
        // TODO: change to box game

        public int[] d;
        public double path_cost;
        public double distance_cost;
        public GameState last_GameState;
        public PlayerOperation o;

        public GameState(int[] array)
        {
            if (array.Length != 9)
            {
                throw (new FormatException("GameState should be 9 numbers."));
            }
            this.d = (int[])array.Clone();
            this.distance_cost = -1;
            this.path_cost = -1;
        }

        public int CompareTo(Object obj)
        {
            if (!(obj is GameState))
            {
                throw new ArgumentException("Compared Object is not GameState");
            }
            GameState GameState = obj as GameState;

            if (this < GameState)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public static bool operator <(GameState s1, GameState s2)
        {
            return (s1.path_cost + s1.distance_cost) < (s2.path_cost + s2.distance_cost);
        }

        public static bool operator >(GameState s1, GameState s2)
        {
            return (s1.path_cost + s1.distance_cost) > (s2.path_cost + s2.distance_cost);
        }


        public static bool operator ==(GameState s1, GameState s2)
        {
            if ((s1 as object) == null)
            {
                return (s2 as object) == null;
            }

            else if ((s2 as object) == null)
            {
                return false;
            }

            for (int i = 0; i < s1.d.Length; i++)
            {
                if (s1.d[i] != s2.d[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(GameState s1, GameState s2)
        {

            if ((s1 as object) == null)
            {
                return (s2 as object) != null;
            }

            else if ((s2 as object) == null)
            {
                return true;
            }


            for (int i = 0; i < s1.d.Length; i++)
            {
                if (s1.d[i] != s2.d[i])
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            String s = String.Format(
                "{0:G} {1:G} {2:G}\n{3:G} {4:G} {5:G}\n{6:G} {7:G} {8:G}\n",
                this.d[0],
                this.d[1],
                this.d[2],
                this.d[3],
                this.d[4],
                this.d[5],
                this.d[6],
                this.d[7],
                this.d[8]
                );
            s += String.Format(
                "(path, distance, sum): ({0:G}, {1:G}, {2:G})\n",
                this.path_cost, this.distance_cost,
                this.path_cost + this.distance_cost
                );

            return s;
        }


        public override bool Equals(Object obj)
        {

            if (!(obj is GameState))
            {
                return false;
            }
            GameState GameState = obj as GameState;

            return (this == GameState);
        }

        public override int GetHashCode()
        {
            return this.d[0] * 100000000
            + this.d[1] * 10000000
            + this.d[2] * 1000000
            + this.d[3] * 100000
            + this.d[4] * 10000
            + this.d[5] * 1000
            + this.d[6] * 100
            + this.d[7] * 10
            + this.d[8] * 1;
        }

    }

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
            throw new InvalidOperationException("优先队列为空");
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
            // TODO
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

    abstract class GameStateProblem
    {
        protected PriorityQueue<GameState> open_queue;
        protected HashSet<GameState> close_list;
        protected GameState start_GameState;
        protected GameState end_GameState;

        public GameStateProblem(GameState start_GameState, GameState end_GameState)
        {
            this.open_queue = new PriorityQueue<GameState>(60000);
            this.close_list = new HashSet<GameState>(370000);
            this.start_GameState = start_GameState;
            this.end_GameState = end_GameState;
        }

        public abstract List<GameState> GetNextGameStates(GameState current_GameState);

        public abstract double GetDistance(GameState s1, GameState s2);

        public void RunAStar()
        {
            this.open_queue.Push(start_GameState);
            int round_num = 0;
            GameState current_GameState = null;
            List<GameState> next_GameStates;

            while (true)
            {
                //Console.WriteLine(String.Format("---------------Round {0:G} -------------", round_num));
                round_num += 1;
                //Console.WriteLine(String.Format("{0:G} {1:G}", this.open_queue.Count, this.close_list.Count));

                try
                {
                    current_GameState = this.open_queue.Pop();
                }
                catch (System.InvalidOperationException e)
                {
                    //Console.WriteLine("Search over. Cannot find an answer.");
                    return;
                }

                //Console.WriteLine("Current:");
                //Console.WriteLine(current_GameState);
                this.close_list.Add(current_GameState);

                if (current_GameState == end_GameState)
                {
                    //Console.WriteLine("-----------Shortest path found.-----------");
                    GameState tmp_GameState = current_GameState;
                    int tmp_n = 0;
                    while (tmp_GameState != null)
                    {
                        //Console.WriteLine("{0:G}:\n", tmp_n);
                        //Console.WriteLine(tmp_GameState);
                        tmp_GameState = tmp_GameState.last_GameState;
                        tmp_n += 1;
                    }
                    return;
                }
                else
                {
                    next_GameStates = this.GetNextGameStates(current_GameState);

                    foreach (GameState s in next_GameStates)
                    {
                        if (s == current_GameState.last_GameState)
                        {
                            // father
                            continue;
                        }
                        else if (this.close_list.Contains(s))
                        {
                            // closed
                            if (s.path_cost > current_GameState.path_cost + 1)
                            {
                                this.close_list.Remove(s);
                                s.path_cost = current_GameState.path_cost + 1;
                                this.open_queue.Push(s);
#if _DEBUG_
                                Console.WriteLine("Remove close.");
                                Console.WriteLine(s);
#endif
                            }
                            else
                            {
#if _DEBUG_
                                Console.WriteLine("Add close.");
                                Console.WriteLine(s);
#endif
                            }
                            continue;
                        }
                        else if (s.path_cost < 0)
                        {
                            s.path_cost = current_GameState.path_cost + 1;
                            this.open_queue.Push(s);
#if _DEBUG_
                            Console.WriteLine("Add open.");
                            Console.WriteLine(s);
#endif
                            continue;
                        }
                        else if (this.open_queue.Has(s))
                        {
                            if (s.path_cost > current_GameState.path_cost + 1)
                            {
                                // Pop s from queue, refresh path, then put it back.
                                s.path_cost = current_GameState.path_cost + 1;
                                this.open_queue.Heapify();
#if _DEBUG_
                                Console.WriteLine("Refresh open.");
                                Console.WriteLine(s);
#endif
                            }
                            else
                            {
                                // do nothing.
                            }
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("ERROR 2: wrong GameState.");
                        }

                    }


                    /*
                    Console.WriteLine("Open queue:");
                    foreach (GameState s in this.open_queue.heap)
                    {
                        if (s != null)
                        {
                            Console.WriteLine(s);
                        }
                    }

                    Console.WriteLine("Closed list:");
                    foreach (GameState s in this.close_list)
                    {

                        Console.WriteLine(s);
                    }
                    */
                }
            }
        }
    }

    class PushBoxProblem : GameStateProblem {

        public PushBoxProblem(GameState start_state, GameState end_state)
            : base(start_state, end_state)
        {
            // Nothing to do.
        }

        public override List<GameState> GetNextGameStates(GameState current_state) {
            return new List<GameState>();
        }

        public override double GetDistance(GameState s1, GameState s2) {
            return 0;
        }

    }

    public class AIPlayer
    {
        public AIPlayer() { 
                    
        }

        public List<PlayerOperation> SearchPath()
        {
            return new List<PlayerOperation>();
        }
    }

}