using System;

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

    public class Player
    {
        public int x;
        public int y;

        public Player(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    public struct BoardInfo
    {
        public int rowNum { get; }
        public int columnNum { get; }
        public GridType[,] d;
        public Player p;

        public BoardInfo(int r, int c, Player _p)
        {
            rowNum = r;
            columnNum = c;
            d = new GridType[r, c];
            p = _p;
        }
    }



    public class GameEngine {

        private BoardInfo Board;

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

        public Player p
        {
            get { return Board.p; }
            set { Board.p = (value); }
        }

        public GameEngine(BoardInfo b) {
            Board = b;
        }

        public bool PlayerOperate(PlayerOperation o)
        {

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
                    if (Board.p.y == 0)
                    {
                        return false;
                    }
                    bg_y = Board.p.y - 1;
                    bg_x = Board.p.x;
                    nxt_y = Board.p.y - 2;
                    nxt_x = Board.p.x;
                    hasNextGrid = (Board.p.y > 1);

                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    if (Board.p.y == Board.rowNum - 1)
                    {
                        return false;
                    }

                    bg_y = Board.p.y + 1;
                    bg_x = Board.p.x;
                    nxt_y = Board.p.y + 2;
                    nxt_x = Board.p.x;
                    hasNextGrid = (Board.p.y < Board.rowNum - 2);

                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    if (Board.p.x == 0)
                    {
                        return false;
                    }

                    bg_y = Board.p.y;
                    bg_x = Board.p.x - 1;
                    nxt_y = Board.p.y;
                    nxt_x = Board.p.x - 2;
                    hasNextGrid = (Board.p.x > 1);

                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    if (Board.p.x == Board.columnNum - 1)
                    {
                        return false;
                    }

                    bg_y = Board.p.y;
                    bg_x = Board.p.x + 1;
                    nxt_y = Board.p.y;
                    nxt_x = Board.p.x + 2;
                    hasNextGrid = (Board.p.x < Board.columnNum - 2);

                    break;
                }
            }

            /**
             * Logic Judgement.
             */

            if (Board.d[bg_y, bg_x] == GridType.Brick)
            {
                return false;
            }
            // move to ground
            else if (Board.d[bg_y, bg_x] == GridType.Ground)
            {
                Board.d[bg_y, bg_x] = GridType.Player;

                if (Board.d[Board.p.y, Board.p.x] == GridType.TarPlayer)
                {
                    Board.d[Board.p.y, Board.p.x] = GridType.Target;
                }
                else
                {
                    Board.d[Board.p.y, Board.p.x] = GridType.Ground;
                }
            }
            // move to target
            else if (Board.d[bg_y, bg_x] == GridType.Target)
            {
                Board.d[bg_y, bg_x] = GridType.TarPlayer;
                if (Board.d[Board.p.y, Board.p.x] == GridType.TarPlayer)
                {
                    Board.d[Board.p.y, Board.p.x] = GridType.Target;
                }
                else
                {
                    Board.d[Board.p.y, Board.p.x] = GridType.Ground;
                }
            }
            // push box
            else if (Board.d[bg_y, bg_x] == GridType.Box)
            {
                if (!hasNextGrid)
                {
                    return false;
                }

                if (Board.d[nxt_y, nxt_x] == GridType.Ground
                    || Board.d[nxt_y, nxt_x] == GridType.Target)
                {
                    // self
                    if (Board.d[Board.p.y, Board.p.x] == GridType.TarPlayer)
                    {
                        Board.d[Board.p.y, Board.p.x] = GridType.Target;
                    }
                    else
                    {
                        Board.d[Board.p.y, Board.p.x] = GridType.Ground;
                    }

                    // bg
                    Board.d[bg_y, bg_x] = GridType.Player;

                    // next
                    if (Board.d[nxt_y, nxt_x] == GridType.Ground)
                    {
                        Board.d[nxt_y, nxt_x] = GridType.Box;
                    }
                    else if (Board.d[nxt_y, nxt_x] == GridType.Target)
                    {
                        Board.d[nxt_y, nxt_x] = GridType.RedBox;
                    }


                }
                else if (Board.d[nxt_y, nxt_x] == GridType.Brick
                    || Board.d[nxt_y, nxt_x] == GridType.Box
                    || Board.d[nxt_y, nxt_x] == GridType.RedBox)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Map Data damaged.");
                }
            }
            // push red box
            else if (Board.d[bg_y, bg_x] == GridType.RedBox)
            {
                if (!hasNextGrid)
                {
                    return false;
                }
                if (Board.d[nxt_y, nxt_x] == GridType.Ground
                    || Board.d[nxt_y, nxt_x] == GridType.Target)
                {

                    // self
                    if (Board.d[Board.p.y, Board.p.x] == GridType.TarPlayer)
                    {
                        Board.d[Board.p.y, Board.p.x] = GridType.Target;
                    }
                    else
                    {
                        Board.d[Board.p.y, Board.p.x] = GridType.Ground;
                    }

                    // bg
                    Board.d[bg_y, bg_x] = GridType.TarPlayer;

                    // next
                    if (Board.d[nxt_y, nxt_x] == GridType.Ground)
                    {
                        Board.d[nxt_y, nxt_x] = GridType.Box;
                    }
                    else if (Board.d[nxt_y, nxt_x] == GridType.Target)
                    {
                        Board.d[nxt_y, nxt_x] = GridType.RedBox;
                    }

                }
                else if (Board.d[nxt_y, nxt_x] == GridType.Brick
                    || Board.d[nxt_y, nxt_x] == GridType.Box
                    || Board.d[nxt_y, nxt_x] == GridType.RedBox)
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
             * Move Board.p.
             */
            switch (o)
            {
                case PlayerOperation.MoveUp:
                {
                    Board.p.y -= 1;
                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    Board.p.y += 1;
                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    Board.p.x -= 1;
                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    Board.p.x += 1;
                    break;
                }
            }

            return true;
        }
    }
}