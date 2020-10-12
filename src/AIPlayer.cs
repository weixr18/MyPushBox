//#define _DEBUG_

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


namespace MyPushBox {

    /// <summary>
    /// The State of game. Checked.
    /// </summary>
    class GameState : IComparable
    {
        
        public BoardInfo Info;
        public double PathCost;
        public double DistanceCost;
        public GameState LastState;
        public PlayerOperation Operation;

        public GameState(BoardInfo info)
        {

            this.Info = (BoardInfo)info.Clone();
            this.DistanceCost = -1;
            this.PathCost = -1;
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

        public static bool operator <(GameState stateA, GameState stateB)
        {
            return (stateA.PathCost + stateA.DistanceCost) < (stateB.PathCost + stateB.DistanceCost);
        }

        public static bool operator >(GameState stateA, GameState stateB)
        {
            return (stateA.PathCost + stateA.DistanceCost) > (stateB.PathCost + stateB.DistanceCost);
        }

        public static bool operator ==(GameState stateA, GameState stateB)
        {
            if ((stateA as object) == null)
            {
                return (stateB as object) == null;
            }

            else if ((stateB as object) == null)
            {
                return false;
            }

            return stateA.Info.Equals(stateB.Info);
        }

        public static bool operator !=(GameState stateA, GameState stateB)
        {

            if ((stateA as object) == null)
            {
                return (stateB as object) != null;
            }

            else if ((stateB as object) == null)
            {
                return true;
            }


            return !stateA.Info.Equals(stateB.Info);
        }

        public override string ToString()
        {
            String s = "";
            s += String.Format("  player position: ({0:G}, {1:G})\n", Info.Player.X, Info.Player.Y);
            foreach (var box in Info.Boxes) {
                s += String.Format("      box : ({0:G}, {1:G})\n", box.X, box.Y);
            }
            s += String.Format(
                "  (path, distance, sum): ({0:G}, {1:G}, {2:G})\n",
                this.PathCost, this.DistanceCost,
                this.PathCost + this.DistanceCost
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
            return Info.GetHashCode();
        }

    }


    /// <summary>
    /// A State Search Problem Framework. Supports A(AStar) algorithm.
    /// Checked.
    /// </summary>
    abstract class GameStateProblem
    {
        protected PriorityQueue<GameState> OpenQueue;
        protected HashSet<GameState> CloseList;
        protected GameState StartState;


        public GameStateProblem(GameState StartState)
        {
            this.OpenQueue = new PriorityQueue<GameState>(6000);
            this.CloseList = new HashSet<GameState>(37000);
            StartState.LastState = null;
            StartState.Operation = 0;
            StartState.PathCost = 0;
            this.StartState = StartState;
        }

        public abstract List<GameState> GetNextGameStates(GameState currentState);
        public abstract double GetEndDistance(GameState s);

        public abstract bool IsEndState(GameState s);

        public List<PlayerOperation> RunAStar()
        {
            this.OpenQueue.Push(StartState);
            int roundNum = 0;
            GameState currentState = null;
            List<GameState> nextStates;

            while (true)
            {
#if _DEBUG_
                if(roundNum % 10 == 0)
                    Debug.WriteLine(String.Format("-----Round {0:G} Open:{1:G}-----", roundNum, this.OpenQueue.Count));
#endif
                roundNum += 1;
                //Console.WriteLine(String.Format("{0:G} {1:G}", this.OpenQueue.Count, this.CloseList.Count));

                try
                {
                    currentState = this.OpenQueue.Pop();
                }
                catch (System.InvalidOperationException e)
                {
                    Debug.WriteLine("Search over. Cannot find an answer.");
                    return new List<PlayerOperation>();
                }

                //Debug.WriteLine("Current:");
                //Debug.WriteLine(currentState);
                this.CloseList.Add(currentState);

                if (IsEndState(currentState))
                {
                    Debug.WriteLine("-----------Shortest path found.-----------");
                    GameState tmp_GameState = currentState;
                    int tmp_n = 0;
                    Stack<GameState> path_stack = new Stack<GameState>();
                    var res = new List<PlayerOperation>();

                    while (tmp_GameState.LastState != null)
                    {
                        //Console.WriteLine("{0:G}:\n", tmp_n);
                        //Console.WriteLine(tmp_GameState);
                        path_stack.Push(tmp_GameState);
                        tmp_GameState = tmp_GameState.LastState;
                        tmp_n += 1;
                    }
                    while (path_stack.Count > 0) {
                        tmp_GameState = path_stack.Pop();
                        res.Add(tmp_GameState.Operation);
                    }
                    return res;
                }
                else
                {
                    nextStates = this.GetNextGameStates(currentState);

                    foreach (GameState s in nextStates)
                    {
                        if (s == currentState.LastState)
                        {
                            // father
                            continue;
                        }
                        
                        else if (this.CloseList.Contains(s))
                        {
                            // closed
                            if (s.PathCost > currentState.PathCost + 1)
                            {
                                this.CloseList.Remove(s);
                                s.PathCost = currentState.PathCost + 1;
                                this.OpenQueue.Push(s);
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
                        
                        else if (s.PathCost < 0)
                        {
                            s.PathCost = currentState.PathCost + 1;
                            this.OpenQueue.Push(s);
#if _DEBUG_
                            Console.WriteLine("Add open.");
                            Console.WriteLine(s);
#endif
                            continue;
                        }
                        else if (this.OpenQueue.Has(s))
                        {
                            if (s.PathCost > currentState.PathCost + 1)
                            {
                                // Pop s from queue, refresh path, then put it back.
                                s.PathCost = currentState.PathCost + 1;
                                this.OpenQueue.Heapify();
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
                    foreach (GameState s in this.OpenQueue.heap)
                    {
                        if (s != null)
                        {
                            Console.WriteLine(s);
                        }
                    }

                    Console.WriteLine("Closed list:");
                    foreach (GameState s in this.CloseList)
                    {

                        Console.WriteLine(s);
                    }
                    */
                }
            }
        }
    }


    /// <summary>
    /// Implementation of the GSP Framework on PushBox game. Checked.
    /// </summary>
    class PushBoxProblem : GameStateProblem {

        GameEngine GE;

        public PushBoxProblem(GameState startState, GameEngine gameEngine)
            : base(startState)
        {
            GE = gameEngine;
        }

        /// <summary>
        /// Generate possible game states from current state.
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public override List<GameState> GetNextGameStates(GameState currentState) 
        {
            var list = new List<GameState>();

            foreach (PlayerOperation operation in 
                Enum.GetValues(typeof(PlayerOperation))) 
            {

                GameState new_state = new GameState(currentState.Info);
                var res = GE.PlayerOperate(operation, new_state.Info);
                if (res) {
                    new_state.LastState = currentState;
                    new_state.Operation = operation;
                    new_state.DistanceCost = GetEndDistance(new_state);
                    list.Add(new_state);
                }
            }

            return list;
        }


        /// <summary>
        /// Get the distance from this to end.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public override double GetEndDistance(GameState state) {

            List<MyPoint> boxs = state.Info.Boxes;
            List<MyPoint> targets = GE.Targets;

            if (boxs.Count != targets.Count) {
                throw new Exception("Boxes and targets number don't match.");
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

        /// <summary>
        /// Manhatton distance of two points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private int Manhatton(MyPoint p1, MyPoint p2) { 
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        /// <summary>
        /// Judge if state is the end.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public override bool IsEndState(GameState state) {

            foreach (var target in GE.Targets) 
            {
                bool found = false;
                foreach (var box in state.Info.Boxes) 
                {
                    if (box.Equals(target)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    return false;
                }
            }
            return true;
        }
    }


    /// <summary>
    /// A Wrapper class, does nothing substantial.
    /// </summary>
    public class AIPlayer
    {
        PushBoxProblem PBP;

        public AIPlayer() { 
        }

        public void SetStartBoard(BoardInfo info, GameEngine GE) {
            GameState startState = new GameState(info);
            PBP = new PushBoxProblem(startState, GE);
        }

        public List<PlayerOperation> SearchPath()
        {
            return PBP.RunAStar();
        }
    }

}