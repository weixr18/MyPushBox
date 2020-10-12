using System;
using System.Collections.Generic;

/*
 * TARGET:
 * minimize the memory use of states
 * 
 * METHOD:
 * 1. BoardInfo只存动态信息
 *     > player位置
 *     > boxes位置
 * 2. GE只负责静态信息
 *     > targets
 *     > 墙壁地面等
 *    GameState/BoardInfo均不能完全表示状态，需要和一个GE配合
 *    GE 不包含 BI
 * 2.1 读盘直接生成GE和BoardInfo，GE不依赖于BoardInfo
 * 2.2 用户单步操作只改变BI
 * 2.3 UI更新：根据GE和BI
 * 2.4 搜索
 *     > AIPlayer只是一个套壳
 *     > PBProblem和MainPage共享一个GE
 *     > 搜索仍保留GameState树
 * 3. GridType矩阵仍保留在GameEngine中，但从不改变
 *     > 执行操作时仅执行改变BoardInfo，矩阵不变
 *    
 *    
 * 进一步改进：
 *     直接在BoardInfo层面构造树？
 * 
 */

namespace MyPushBox
{
    /// <summary>
    /// Type of Grid. Checked.
    /// </summary>
    public enum GridType
    {
        Ground = 1,
        Target = 2,
        Brick = 3,
        OutSide = 4,

        // RedBox 只能和 Target 互相转化！！！
    }

    /// <summary>
    /// Operation of Player. Checked.
    /// </summary>
    public enum PlayerOperation
    {
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
    }

    public class MyPoint {
        public int X { get; set; }
        public int Y { get; set; }

        public MyPoint(int x, int y) {
            X = x;
            Y = y;
        }

        public bool Equals(MyPoint p)
        {
            return (this.X == p.X) && (this.Y == p.Y);
        }

        public override string ToString()
        {
            return String.Format("X:{0:G}, Y:{1:G}\n", X, Y);
        }
    }

    /// <summary>
    /// Dynamic information of the game. Checked.
    /// </summary>
    public class BoardInfo
    {
        
        public List<MyPoint> Boxes;

        /// <summary>
        /// Player Position. Attention that : 
        /// (X -- column index -- matrix axis 1) and 
        /// (Y -- row index -- matrix axis 0)
        /// </summary>
        public MyPoint Player;

        public BoardInfo(MyPoint player, List<MyPoint> boxes)
        {
            Player = player;
            Boxes = boxes;
        }

        public BoardInfo Clone() {
            var player = new MyPoint(Player.X, Player.Y);
            var boxes = new List<MyPoint>(Boxes);

            BoardInfo info = new BoardInfo(player, boxes);
            return info;
        }

        public bool Equals(BoardInfo obj)
        {
            return Player.Equals(obj.Player) && Boxes.Equals(obj.Boxes);
        }
    }


    /// <summary>
    /// Game Engine. Not done.
    /// </summary>
    public class GameEngine {
        
        /// <summary>
        /// Row number of the field.
        /// </summary>
        public int RowNum { get; }

        /// <summary>
        /// Column number of the field.
        /// </summary>
        public int ColumnNum { get; }

        /// <summary>
        /// Target positions of the boxes. Statistic.
        /// </summary>
        public List<MyPoint> Targets;

        /// <summary>
        /// Static information of the field. A Matrix of grid types. Statistic.
        /// </summary>
        public GridType[,] GridMatrix { get; }


        /// <summary>
        /// Initialization of Game Engine.
        /// </summary>
        /// <param name="g">Matrix of the grid types.</param>
        public GameEngine(GridType[,] g, List<MyPoint> targets) {
            RowNum = g.GetLength(0);
            ColumnNum = g.GetLength(1);
            Targets = targets;
            GridMatrix = g;
        }

        /// <summary>
        /// Operate the move according to Operation o and dynamic info.
        /// </summary>
        /// <param name="o">Movement</param>
        /// <param name="info">Dynamic info</param>
        /// <returns>Whether the operation is available.</returns>
        public bool PlayerOperate(PlayerOperation o, BoardInfo info) {

            // Prepare variables.

            int rowNum = RowNum;
            int columnNum = ColumnNum;
            int playerX = info.Player.X;
            int playerY = info.Player.Y;

            int nxtY = -1;
            int nxtX = -1;
            int altY = -1;
            int altX = -1;
            bool hasAlternateGrid = false;

            switch (o)
            {
                case PlayerOperation.MoveUp:
                {
                    if (playerY == 0)
                       return false;
                    nxtY = playerY - 1;
                    nxtX = playerX;
                    altY = playerY - 2;
                    altX = playerX;
                    hasAlternateGrid = (playerY > 1);
                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    if (playerY == rowNum - 1)
                        return false;
                    nxtY = playerY + 1;
                    nxtX = playerX;
                    altY = playerY + 2;
                    altX = playerX;
                    hasAlternateGrid = (playerY < rowNum - 2);
                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    if (playerX == 0)
                        return false;
                    nxtY = playerY;
                    nxtX = playerX - 1;
                    altY = playerY;
                    altX = playerX - 2;
                    hasAlternateGrid = (playerX > 1);
                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    if (playerX == columnNum - 1)
                        return false;
                    nxtY = playerY;
                    nxtX = playerX + 1;
                    altY = playerY;
                    altX = playerX + 2;
                    hasAlternateGrid = (playerX < columnNum - 2);
                    break;
                }
            }

            
            // Logic Judgement.
             
            // next is brick
            if (GridMatrix[nxtY, nxtX] == GridType.Brick)
            {
                return false;
            }

            // next is not brick
            else if (GridMatrix[nxtY, nxtX] == GridType.Ground || GridMatrix[nxtY, nxtX] == GridType.Target)
            {
                if (!hasAlternateGrid) {
                    throw new Exception("Map data invalid error.");
                }
                if (GridMatrix[altY, altX] == GridType.OutSide) {
                    throw new Exception("Map data invalid error.");
                }

                // Search if there is a box to move.

                foreach(var box in info.Boxes)
                {
                    if (nxtY == box.Y && nxtX == box.X)
                    {
                        // alternate grid is brick
                        if (GridMatrix[altY, altX] == GridType.Brick)
                        {
                            return false;
                        }
                        else 
                        {
                            // if alternate grid has a box, cannot move.
                            foreach (var boxB in info.Boxes)
                            {
                                if (boxB.Y == altY && boxB.X == altX)
                                {
                                    return false;
                                }
                            }

                            // now the box could be moved.
                            box.Y = altY;
                            box.X = altX;
                            break;
                        }
                    }
                };

                // Move the player.
                info.Player.Y = nxtY;
                info.Player.X = nxtX;

                return true;
            }
            
            // wrong GameState
            else
            {
                throw new Exception("Map Data damaged.");
            }
            
        }
    }
}