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

    public class BoardInfo
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
            p = new Player(_p.x, _p.y);
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

        public Player p
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
                    if (info.p.y == 0)
                    {
                        return false;
                    }
                    bg_y = info.p.y - 1;
                    bg_x = info.p.x;
                    nxt_y = info.p.y - 2;
                    nxt_x = info.p.x;
                    hasNextGrid = (info.p.y > 1);

                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    if (info.p.y == info.rowNum - 1)
                    {
                        return false;
                    }

                    bg_y = info.p.y + 1;
                    bg_x = info.p.x;
                    nxt_y = info.p.y + 2;
                    nxt_x = info.p.x;
                    hasNextGrid = (info.p.y < info.rowNum - 2);

                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    if (info.p.x == 0)
                    {
                        return false;
                    }

                    bg_y = info.p.y;
                    bg_x = info.p.x - 1;
                    nxt_y = info.p.y;
                    nxt_x = info.p.x - 2;
                    hasNextGrid = (info.p.x > 1);

                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    if (info.p.x == info.columnNum - 1)
                    {
                        return false;
                    }

                    bg_y = info.p.y;
                    bg_x = info.p.x + 1;
                    nxt_y = info.p.y;
                    nxt_x = info.p.x + 2;
                    hasNextGrid = (info.p.x < info.columnNum - 2);

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

                if (info.d[info.p.y, info.p.x] == GridType.TarPlayer)
                {
                    info.d[info.p.y, info.p.x] = GridType.Target;
                }
                else
                {
                    info.d[info.p.y, info.p.x] = GridType.Ground;
                }
            }
            // move to target
            else if (info.d[bg_y, bg_x] == GridType.Target)
            {
                info.d[bg_y, bg_x] = GridType.TarPlayer;
                if (info.d[info.p.y, info.p.x] == GridType.TarPlayer)
                {
                    info.d[info.p.y, info.p.x] = GridType.Target;
                }
                else
                {
                    info.d[info.p.y, info.p.x] = GridType.Ground;
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
                    if (info.d[info.p.y, info.p.x] == GridType.TarPlayer)
                    {
                        info.d[info.p.y, info.p.x] = GridType.Target;
                    }
                    else
                    {
                        info.d[info.p.y, info.p.x] = GridType.Ground;
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
                    if (info.d[info.p.y, info.p.x] == GridType.TarPlayer)
                    {
                        info.d[info.p.y, info.p.x] = GridType.Target;
                    }
                    else
                    {
                        info.d[info.p.y, info.p.x] = GridType.Ground;
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
                    info.p.y -= 1;
                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    info.p.y += 1;
                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    info.p.x -= 1;
                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    info.p.x += 1;
                    break;
                }
            }

            return true;
        }


    }
}