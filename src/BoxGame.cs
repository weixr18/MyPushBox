using System;
using System.Drawing;

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

    public enum GridType
    {
        Ground = 1,
        Target = 2,
        Box = 3,
        RedBox = 4,
        Player = 5,
        TarPlayer = 6,
        Brick = 7,
        OutSide = 8,

        // RedBox 只能和 Target 互相转化！！！
    }

    public enum PlayerOperation
    {
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
    }


    public class BoardInfo
    {
        public int rowNum { get; }
        public int columnNum { get; }
        public GridType[,] d;
        public Point p;

        public BoardInfo(int r, int c, Point _p)
        {
            rowNum = r;
            columnNum = c;
            d = new GridType[r, c];
            p = new Point(_p.X, _p.Y);
        }

        public BoardInfo Clone() {
            BoardInfo info = new BoardInfo(rowNum, columnNum, p);
            for (int i = 0; i < rowNum; i++) {
                for (int j = 0; j < columnNum; j++) {
                    info.d[i, j] = this.d[i, j];
                }
            }
            return info;
        }
    }



    public class GameEngine {

        public BoardInfo Board;

        public int rowNum 
        { 
            get { return Board.rowNum; }
        }

        public int columnNum 
        {
            get { return Board.columnNum; }
        }

        public GridType[,] d 
        {
            get { return Board.d; }
        }

        public Point p
        {
            get { return Board.p; }
            set { Board.p = (value); }
        }

        public GameEngine(BoardInfo b) {
            Board = b;
        }

        public bool PlayerOperate(PlayerOperation o) {
            return PlayerOperate(o, this.Board);
        }


        public bool PlayerOperate(PlayerOperation o, BoardInfo info) {
            /**
            * Get variables.
            */

            int bg_y = -1;
            int bg_x = -1;
            int nxt_y = -1;
            int nxt_x = -1;
            bool hasNextGrid = false;

            switch (o)
            {
                case PlayerOperation.MoveUp:
                {
                    if (info.p.Y == 0)
                    {
                        return false;
                    }
                    bg_y = info.p.Y - 1;
                    bg_x = info.p.X;
                    nxt_y = info.p.Y - 2;
                    nxt_x = info.p.X;
                    hasNextGrid = (info.p.Y > 1);

                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    if (info.p.Y == info.rowNum - 1)
                    {
                        return false;
                    }

                    bg_y = info.p.Y + 1;
                    bg_x = info.p.X;
                    nxt_y = info.p.Y + 2;
                    nxt_x = info.p.X;
                    hasNextGrid = (info.p.Y < info.rowNum - 2);

                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    if (info.p.X == 0)
                    {
                        return false;
                    }

                    bg_y = info.p.Y;
                    bg_x = info.p.X - 1;
                    nxt_y = info.p.Y;
                    nxt_x = info.p.X - 2;
                    hasNextGrid = (info.p.X > 1);

                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    if (info.p.X == info.columnNum - 1)
                    {
                        return false;
                    }

                    bg_y = info.p.Y;
                    bg_x = info.p.X + 1;
                    nxt_y = info.p.Y;
                    nxt_x = info.p.X + 2;
                    hasNextGrid = (info.p.X < info.columnNum - 2);

                    break;
                }
            }

            /**
             * Logic Judgement.
             */

            if (info.d[bg_y, bg_x] == GridType.Brick)
            {
                return false;
            }
            // move to ground
            else if (info.d[bg_y, bg_x] == GridType.Ground)
            {
                info.d[bg_y, bg_x] = GridType.Player;

                if (info.d[info.p.Y, info.p.X] == GridType.TarPlayer)
                {
                    info.d[info.p.Y, info.p.X] = GridType.Target;
                }
                else
                {
                    info.d[info.p.Y, info.p.X] = GridType.Ground;
                }
            }
            // move to target
            else if (info.d[bg_y, bg_x] == GridType.Target)
            {
                info.d[bg_y, bg_x] = GridType.TarPlayer;
                if (info.d[info.p.Y, info.p.X] == GridType.TarPlayer)
                {
                    info.d[info.p.Y, info.p.X] = GridType.Target;
                }
                else
                {
                    info.d[info.p.Y, info.p.X] = GridType.Ground;
                }
            }
            // push box
            else if (info.d[bg_y, bg_x] == GridType.Box)
            {
                if (!hasNextGrid)
                {
                    return false;
                }

                if (info.d[nxt_y, nxt_x] == GridType.Ground
                    || info.d[nxt_y, nxt_x] == GridType.Target)
                {
                    // self
                    if (info.d[info.p.Y, info.p.X] == GridType.TarPlayer)
                    {
                        info.d[info.p.Y, info.p.X] = GridType.Target;
                    }
                    else
                    {
                        info.d[info.p.Y, info.p.X] = GridType.Ground;
                    }

                    // bg
                    info.d[bg_y, bg_x] = GridType.Player;

                    // next
                    if (info.d[nxt_y, nxt_x] == GridType.Ground)
                    {
                        info.d[nxt_y, nxt_x] = GridType.Box;
                    }
                    else if (info.d[nxt_y, nxt_x] == GridType.Target)
                    {
                        info.d[nxt_y, nxt_x] = GridType.RedBox;
                    }


                }
                else if (info.d[nxt_y, nxt_x] == GridType.Brick
                    || info.d[nxt_y, nxt_x] == GridType.Box
                    || info.d[nxt_y, nxt_x] == GridType.RedBox)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Map Data damaged.");
                }
            }
            // push red box
            else if (info.d[bg_y, bg_x] == GridType.RedBox)
            {
                if (!hasNextGrid)
                {
                    return false;
                }
                if (info.d[nxt_y, nxt_x] == GridType.Ground
                    || info.d[nxt_y, nxt_x] == GridType.Target)
                {

                    // self
                    if (info.d[info.p.Y, info.p.X] == GridType.TarPlayer)
                    {
                        info.d[info.p.Y, info.p.X] = GridType.Target;
                    }
                    else
                    {
                        info.d[info.p.Y, info.p.X] = GridType.Ground;
                    }

                    // bg
                    info.d[bg_y, bg_x] = GridType.TarPlayer;

                    // next
                    if (info.d[nxt_y, nxt_x] == GridType.Ground)
                    {
                        info.d[nxt_y, nxt_x] = GridType.Box;
                    }
                    else if (info.d[nxt_y, nxt_x] == GridType.Target)
                    {
                        info.d[nxt_y, nxt_x] = GridType.RedBox;
                    }

                }
                else if (info.d[nxt_y, nxt_x] == GridType.Brick
                    || info.d[nxt_y, nxt_x] == GridType.Box
                    || info.d[nxt_y, nxt_x] == GridType.RedBox)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Map Data damaged.");
                }
            }
            // wrong GameState
            else
            {
                throw new Exception("Map Data damaged.");
            }

            /**
             * Move player
             */
            switch (o)
            {
                case PlayerOperation.MoveUp:
                {
                    info.p.Y -= 1;
                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    info.p.Y += 1;
                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    info.p.X -= 1;
                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    info.p.X += 1;
                    break;
                }
            }

            return true;
        }


    }
}