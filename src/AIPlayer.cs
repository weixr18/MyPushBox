//#define _DEBUG_

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


namespace MyPushBox {

    class GameState : IComparable
    {
        
        public BoardInfo bi;
        public double path_cost;
        public double distance_cost;
        public GameState last_GameState;
        public PlayerOperation o;

        public GameState(BoardInfo info)
        {

            this.bi = (BoardInfo)info.Clone();
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

            return s1.bi.d.Equals(s2.bi.d);
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


            return !s1.bi.d.Equals(s2.bi.d);
        }

        public override string ToString()
        {
            String s = "";
            s += String.Format("  player position: ({0:G}, {1:G})\n", bi.p.y, bi.p.x);
            s += String.Format(
                "  (path, distance, sum): ({0:G}, {1:G}, {2:G})\n",
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
            return bi.d.GetHashCode();
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


        public GameStateProblem(GameState start_GameState)
        {
            
            this.open_queue = new PriorityQueue<GameState>(6000);
            this.close_list = new HashSet<GameState>(37000);
            start_GameState.last_GameState = null;
            start_GameState.o = 0;
            start_GameState.path_cost = 0;
            this.start_GameState = start_GameState;
        }

        public abstract List<GameState> GetNextGameStates(GameState current_GameState);

        public abstract double GetEndDistance(GameState s);

        public abstract bool IsEndState(GameState s);

        public List<PlayerOperation> RunAStar()
        {
            this.open_queue.Push(start_GameState);
            int round_num = 0;
            GameState current_GameState = null;
            List<GameState> next_GameStates;

            while (true)
            {
#if _DEBUG_
                if(round_num % 10 == 0)
                    Debug.WriteLine(String.Format("-----Round {0:G} Open:{1:G}-----", round_num, this.open_queue.Count));
#endif
                round_num += 1;
                //Console.WriteLine(String.Format("{0:G} {1:G}", this.open_queue.Count, this.close_list.Count));

                try
                {
                    current_GameState = this.open_queue.Pop();
                }
                catch (System.InvalidOperationException e)
                {
                    //Console.WriteLine("Search over. Cannot find an answer.");
                    return new List<PlayerOperation>();
                }

                //Debug.WriteLine("Current:");
                //Debug.WriteLine(current_GameState);
                this.close_list.Add(current_GameState);

                if (IsEndState(current_GameState))
                {
                    Debug.WriteLine("-----------Shortest path found.-----------");
                    GameState tmp_GameState = current_GameState;
                    int tmp_n = 0;
                    Stack<GameState> path_stack = new Stack<GameState>();
                    var res = new List<PlayerOperation>();

                    while (tmp_GameState.last_GameState != null)
                    {
                        //Console.WriteLine("{0:G}:\n", tmp_n);
                        //Console.WriteLine(tmp_GameState);
                        path_stack.Push(tmp_GameState);
                        tmp_GameState = tmp_GameState.last_GameState;
                        tmp_n += 1;
                    }
                    while (path_stack.Count > 0) {
                        tmp_GameState = path_stack.Pop();
                        res.Add(tmp_GameState.o);
                    }
                    return res;
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
                        /*
                        else if (this.close_list.Contains(s))
                        {
                            // closed
                            if (s.path_cost > current_GameState.path_cost + 1)
                            {
                                this.close_list.Remove(s);
                                s.path_cost = current_GameState.path_cost + 1;
                                this.open_queue.Push(s);
                                Debug.WriteLine("Remove close.");
#if _DEBUG_
                                
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
                        */
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

        GameEngine GE;

        public PushBoxProblem(GameState start_state, GameEngine _GE)
            : base(start_state)
        {
            GE = _GE;
        }

        public override List<GameState> GetNextGameStates(GameState current_state) {
            
            var list = new List<GameState>();

            foreach (PlayerOperation o in 
                Enum.GetValues(typeof(PlayerOperation))) {

                GameState new_state = new GameState(current_state.bi);
                
                var res = GE.PlayerOperate(o, new_state.bi);
                if (res) {
                    new_state.last_GameState = current_state;
                    new_state.o = o;
                    new_state.distance_cost = GetEndDistance(new_state);

                    list.Add(new_state);
                }
            }

            return list;
        }

        public override double GetEndDistance(GameState s) {

            List<Point> boxs = new List<Point>();
            List<Point> targets = new List<Point>();
            GridType[,] d = s.bi.d;

            /// get boxes and targets
            for (int i = 0; i < s.bi.rowNum; i++) 
            {
                for (int j = 0; j < s.bi.columnNum; j++) 
                {
                    if (d[i, j] == GridType.Box)
                    {
                        boxs.Add(new Point(i, j));
                    }
                    else if (d[i, j] == GridType.Target
                         || d[i, j] == GridType.TarPlayer) 
                    {
                        targets.Add(new Point(i, j));
                    }
                }
            }

            if (boxs.Count != targets.Count) {
                throw new Exception("Boxes and targets don't match.");
            }

            /// get each distance
            int n = boxs.Count;
            int[,] disMatrix = new int[n, n];
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < n; j++) {
                    disMatrix[i, j] = Manhatton(boxs[i], targets[j]);
                }
            }

            /// use Hungary Algorithm to find the minimum distance sum
            ZMatrix m = new ZMatrix(disMatrix);
            double res = m.Calculation();

            return res;
        }

        private int Manhatton(Point p1, Point p2) { 
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        public override bool IsEndState(GameState s) {

            foreach (var grid in s.bi.d) {
                if (grid == GridType.Box) {
                    return false;
                }
                if (grid == GridType.Target) {
                    return false;
                }
                if (grid == GridType.TarPlayer) {
                    return false;
                }
            }
            return true;
        }
    }

    public class AIPlayer
    {
        PushBoxProblem pbp;

        public AIPlayer() { 
        }

        public void SetStartBoard(BoardInfo info, GameEngine GE) {
            GameState start_state = new GameState(info);
            pbp = new PushBoxProblem(start_state, GE);
        }

        public List<PlayerOperation> SearchPath()
        {
            return pbp.RunAStar();
        }
    }

}