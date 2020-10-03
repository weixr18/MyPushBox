using Microsoft.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;



namespace MyPushBox
{

    enum GridType
    {
        Ground = 0,
        Brick = 1,
        Player = 2,
        Box = 3,
        Target = 4,
        RedBox = 5,
        OutSide = 6,
    }

    struct BoardInfo
    {
        public int rowNum { get; }
        public int columnNum { get; }
        public GridType[,] d;

        public BoardInfo(int r, int c)
        {
            rowNum = r;
            columnNum = c;
            d = new GridType[r, c];
        }
    }



    public sealed partial class MainPage : Page
    {

        private static Dictionary<GridType, BitmapImage> BlockType;
        private static Dictionary<char, GridType> BlockCode;
        const int GRID_SIZE = 50;
        private BoardInfo Board;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeProperties();
            this.InitializeLayout();

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private void InitializeProperties() {

            BlockCode = new Dictionary<char, GridType>();
            BlockCode.Add('-', GridType.Ground);  
            BlockCode.Add('#', GridType.Brick);  
            BlockCode.Add('@', GridType.Player);  
            BlockCode.Add('$', GridType.Box);  
            BlockCode.Add('.', GridType.Target);  
            BlockCode.Add('_', GridType.OutSide);
            BlockCode.Add('*', GridType.RedBox);

            BitmapImage bit_ground = new BitmapImage(new Uri("ms-appx:///Assets/Block/Ground.jpg"));
            BitmapImage bit_brick = new BitmapImage(new Uri("ms-appx:///Assets/Block/Brick.jpg"));
            BitmapImage bit_player = new BitmapImage(new Uri("ms-appx:///Assets/Block/Player.jpg"));
            BitmapImage bit_box = new BitmapImage(new Uri("ms-appx:///Assets/Block/Box.jpg"));
            BitmapImage bit_target = new BitmapImage(new Uri("ms-appx:///Assets/Block/Target.jpg"));
            BitmapImage bit_red_box = new BitmapImage(new Uri("ms-appx:///Assets/Block/RedBox.jpg"));
            BitmapImage bit_outside = new BitmapImage(new Uri("ms-appx:///Assets/Block/Outside.jpg"));


            BlockType = new Dictionary<GridType, BitmapImage>();
            BlockType.Add(GridType.Ground, bit_ground);
            BlockType.Add(GridType.Brick, bit_brick);
            BlockType.Add(GridType.Player, bit_player);
            BlockType.Add(GridType.Box, bit_box);
            BlockType.Add(GridType.Target, bit_target);
            BlockType.Add(GridType.RedBox, bit_red_box);
            BlockType.Add(GridType.OutSide, bit_outside);
        }

        private void InitializeLayout()
        {

            Board = GetBoardInfo().Result;
            int rowCount = Board.rowNum;
            int colCount = Board.columnNum;
            Grid g = grid_table;

            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(GRID_SIZE);
                g.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < colCount; i++)
            {
                ColumnDefinition rd = new ColumnDefinition();
                rd.Width = new GridLength(GRID_SIZE);
                g.ColumnDefinitions.Add(rd);
            }

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    Image img = new Image();
                    img.Source = BlockType[Board.d[r, c]];
                    grid_table.Children.Add(img);
                    Grid.SetColumn(img, c);
                    Grid.SetRow(img, r);
                }
            }

            
        }

        private async Task<BoardInfo> GetBoardInfo()
        {
            BoardInfo info;

            try
            {
                var uri = new Uri("ms-appx:///Assets/Stages/1.txt");
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false); ;
                var encoding = Encoding.GetEncoding("utf-8");

                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, false))
                    {
                        string line;

                        line = reader.ReadLine();
                        if (line == null)
                        {
                            throw new FileLoadException("Wrong Stage File Format.");
                        }
                        string[] numStrings = line.Split(' ');
                        int rowCount = Convert.ToInt32(numStrings[0], 10);
                        int columnCount = Convert.ToInt32(numStrings[1], 10);
                        info = new BoardInfo(rowCount, columnCount);

                        for (int i = 0; i < rowCount; i++)
                        {
                            line = reader.ReadLine();
                            if (line == null)
                            {
                                throw new FileLoadException("Wrong Stage File Line Format.");
                            }
                            if (line.Length != columnCount)
                            {
                                throw new FileLoadException("Wrong Stage File Line Format.");
                            }

                            for (int j = 0; j < columnCount; j++)
                            {
                                info.d[i, j] = BlockCode[line[j]];
                            }
                        }
                    }
                }

                return info;
            }
            catch (Exception e)
            {
                Debug.WriteLine("The file could not be read:");
                Debug.WriteLine(e.Message);
                return new BoardInfo(0, 0);
            }
        }


        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {

            if (args.EventType.ToString().Contains("KeyDown"))
            {
                VirtualKey virtualKey = args.VirtualKey;

                switch (virtualKey)
                {
                    case VirtualKey.Escape:
                        {

                            break;
                        }
                    case VirtualKey.N:
                        {

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

    }
}
