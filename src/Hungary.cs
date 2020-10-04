using System;
using System.Collections.Generic;
using System.Drawing;

namespace MyPushBox
{

    class ZZeroNode
    {
        public int X;
        public int ZeroNum;

        public ZZeroNode(int x, int zeroNum)
        {
            X = x;
            ZeroNum = zeroNum;
        }

        public static int Cmp(ZZeroNode a, ZZeroNode b)
        {
            return a.ZeroNum.CompareTo(b.ZeroNum);
        }
    }

    public class ZMatrix
    {
        private int[,] _data;
        private int[,] _old_data;
        private List<Point> _result = new List<Point>();
        private int _x;
        private int _y;

        public ZMatrix(int[,] data)
        {
            _data = (int[,])data.Clone();
            _old_data = data;
            _x = data.GetLength(0);
            _y = data.GetLength(1);
        }
        public double Calculation()
        {
            step1();
            while (!step2())
            {
                step3();
            }

            double res = 0;
            foreach (var point in _result) {
                res += _old_data[point.X, point.Y];
            }

            return res;
        }

        /// <summary>
        /// 在各列中找最小值，將該列中各元素檢去此值，對各行重複一次。
        /// </summary>
        private void step1()
        {
            //列
            for (int x = 0; x < _x; x++)
            {
                int minY = Int32.MaxValue;
                //找到每列最小的值
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] < minY)
                    {
                        minY = _data[x, y];
                    }
                }
                //让该列减去最小的值
                for (int y = 0; y < _y; y++)
                {
                    _data[x, y] -= minY;
                }
            }
            //行
            for (int y = 0; y < _y; y++)
            {
                int minX = Int32.MaxValue;
                //找到每列最小的值
                for (int x = 0; x < _x; x++)
                {
                    if (_data[x, y] < minX)
                    {
                        minX = _data[x, y];
                    }
                }
                //让该列减去最小的值
                for (int x = 0; x < _x; x++)
                {
                    _data[x, y] -= minX;
                }
            }
        }

        /// <summary>
        /// 检验各列，对碰上之第一個零，做记号，同列或同栏的其他零則画X (由零較少的列先做，可不依順序)
        /// 
        /// 检验可否完成仅含零的完全指派，若不能，則false
        /// </summary>
        private bool step2()
        {
            _result.Clear();
            bool[,] isDelete = new bool[_x, _y];

            //零的数量由少到多
            List<ZZeroNode> zeroNodes = new List<ZZeroNode>();
            for (int x = 0; x < _x; x++)
            {
                int zeroNum = 0;
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] == 0)
                    {
                        zeroNum++;
                    }
                }
                if (zeroNum > 0)
                {
                    zeroNodes.Add(new ZZeroNode(x, zeroNum));
                }
            }
            zeroNodes.Sort(ZZeroNode.Cmp);

            //从零较少的行开始
            while (zeroNodes.Count > 0)
            {
                ZZeroNode node = zeroNodes[0];

                if (node.ZeroNum <= 0)
                {
                    zeroNodes.RemoveAt(0);
                }
                else
                {
                    for (int y = 0; y < _y; y++)
                    {
                        if (_data[node.X, y] == 0 && !isDelete[node.X, y])
                        {
                            _result.Add(new Point(node.X, y));
                            zeroNodes.RemoveAt(0);

                            //删除与该零在同一列的其他零
                            for (int xxx = 0; xxx < _x; xxx++)
                            {
                                if (_data[xxx, y] == 0)
                                {
                                    isDelete[xxx, y] = true;
                                    for (int i = 0; i < zeroNodes.Count; i++)
                                    {
                                        if (zeroNodes[i].X == xxx)
                                        {
                                            zeroNodes[i].ZeroNum--;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                zeroNodes.Sort(ZZeroNode.Cmp);
            }

            return _result.Count == _x;
        }

        /// <summary>
        /// 画出最少数目的垂直与水平的删除线來包含所有的零至少一次。
        /// </summary>
        private void step3()
        {
            bool[,] isDelete = new bool[_x, _y];
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] == 0 && !isDelete[x, y])
                    {
                        int xc = 0;
                        int yc = 0;

                        //lie
                        for (int nx = 0; nx < _x; nx++)
                        {
                            if (nx != x && _data[nx, y] == 0)
                            {
                                xc++;
                            }
                        }

                        //hang
                        for (int ny = 0; ny < _y; ny++)
                        {
                            if (ny != y && _data[x, ny] == 0)
                            {
                                yc++;
                            }
                        }

                        if (xc > yc)
                        {
                            for (int xx = 0; xx < _x; xx++)
                            {
                                isDelete[xx, y] = true;
                            }
                        }
                        else
                        {
                            for (int yy = 0; yy < _y; yy++)
                            {
                                isDelete[x, yy] = true;
                            }
                        }
                    }
                }
            }
            //找出未被畫線的元素中之最小值 K
            int k = 99999;
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (!isDelete[x, y])
                    {
                        if (_data[x, y] < k)
                        {
                            k = _data[x, y];
                        }
                    }
                }
            }

            //將含有此些未被畫線的元素的各列所有元素減去K 
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (!isDelete[x, y])
                    {
                        for (int y1 = 0; y1 < _y; y1++)
                        {
                            _data[x, y1] -= k;
                        }
                        break;
                    }
                }
            }

            //若造成負值，則將該欄加上K (Step 4.2)。形成新矩陣後回到Step2
            for (int x = 0; x < _x; x++)
            {
                for (int y = 0; y < _y; y++)
                {
                    if (_data[x, y] < 0)
                    {
                        for (int x1 = 0; x1 < _x; x1++)
                        {
                            _data[x1, y] += k;
                        }
                        break;
                    }
                }
            }
        }

    }

    /*
    class Program1
    {
        static void Main(string[] args)
        {
            int[,] data = new int[4, 4] { 
                { 65, 23, 8, 4 }, 
                { 5, 3, 7, 8 }, 
                { 9, 0, 41, 12 }, 
                { 13, 9, 3, 16 } 
            };
            ZMatrix m = new ZMatrix(data);
            double res = m.Calculation();
        }
    }
    */

}
