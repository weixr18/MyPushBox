using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;


namespace MyPushBox
{

    public sealed partial class MainPage : Page
    {
        private static Dictionary<char, GridType> BlockCode;
        private static Dictionary<GridType, BitmapImage> BlockType;
        private const int GRID_SIZE = 50;
        private Image[,] Images;
        public GameEngine GE;

        public MainPage()
        {
            // layout component initialize
            this.InitializeComponent();

            // UI hashmaps initialize
            this.InitializeProperties();

            // game engine initialize
            BoardInfo board = GetBoardInfo().Result;
            GE = new GameEngine(board);

            // grid initialize
            this.InitializeGrid();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;
        }


        private void InitializeProperties() 
        {

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
            BlockType.Add(GridType.TarPlayer, bit_player);
            BlockType.Add(GridType.Box, bit_box);
            BlockType.Add(GridType.Target, bit_target);
            BlockType.Add(GridType.RedBox, bit_red_box);
            BlockType.Add(GridType.OutSide, bit_outside);
        }

        private async Task<BoardInfo> GetBoardInfo()
        {
            BoardInfo info;
            int player_x = -1;
            int player_y = -1;
            try
            {
                var uri = new Uri("ms-appx:///Assets/Stages/1.txt");
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
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
                        Player player = new Player(0, 0);
                        info = new BoardInfo(rowCount, columnCount, player);

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
                                if (info.d[i, j] == GridType.Player)
                                {
                                    // i is row index -- y
                                    // j is column index -- x
                                    player_x = j;
                                    player_y = i;
                                }
                            }
                        }

                        if (player_x < 0 || player_y < 0)
                        {
                            throw new FileLoadException("No Player Position in Stage File.");
                        }

                        info.p.x = player_x;
                        info.p.y = player_y;
                       
                    }
                }

                return info;
            }
            catch (Exception e)
            {
                Debug.WriteLine("The file could not be read:");
                Debug.WriteLine(e.Message);
                return new BoardInfo(0, 0, new Player(0, 0));
            }
        }

        private void InitializeGrid()
        {
            int rowCount = GE.rowNum;
            int colCount = GE.columnNum;
            Grid g = grid_table;
            Images = new Image[rowCount, colCount];


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

            this.RefreshGrid();
        }

        private void RefreshGrid() {

            int rowCount = GE.rowNum;
            int colCount = GE.columnNum;
            GridType[,] d = GE.d;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    Image img = new Image();
                    Images[r, c] = img;
                    img.Source = BlockType[d[r, c]];
                    grid_table.Children.Add(img);
                    Grid.SetColumn(img, c);
                    Grid.SetRow(img, r);
                }
            }
        }

        private void Page_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            VirtualKey virtualKey = args.VirtualKey;
            switch (virtualKey)
            {
                case VirtualKey.W:
                case VirtualKey.Up:
                {
                    GE.PlayerOperate(PlayerOperation.MoveUp);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.S:
                case VirtualKey.Down:
                {
                    GE.PlayerOperate(PlayerOperation.MoveDown);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.D:
                case VirtualKey.Right:
                {
                    GE.PlayerOperate(PlayerOperation.MoveRight);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.A:
                case VirtualKey.Left:
                {
                    GE.PlayerOperate(PlayerOperation.MoveLeft);
                    RefreshGrid();
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
