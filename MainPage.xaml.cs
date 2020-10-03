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

    enum GridType
    {
        Ground = 1,
        Target = 2,
        Box = 3,
        RedBox = 4,
        Player = 5,
        TarPlayer = 6,
        Brick = 7,
        OutSide = 8,
    }

    /*
     * RedBox 只能和 Target 互相转化！！！
     */

    enum PlayerOperation {
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
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

    struct Player {
        public int x;
        public int y;

        public Player(int _x, int _y) {
            x = _x;
            y = _y;
        }
    }

    

    public sealed partial class MainPage : Page
    {
        private static Dictionary<char, GridType> BlockCode;
        private static Dictionary<GridType, BitmapImage> BlockType;
        private const int GRID_SIZE = 50;
        private BoardInfo Board;
        private Image[,] Images;
        private Player player;


        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeProperties();
            Board = GetBoardInfo().Result;
            this.InitializeLayout();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;
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

                        player = new Player(player_x, player_y);
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


        private void InitializeLayout()
        {
            int rowCount = Board.rowNum;
            int colCount = Board.columnNum;
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

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    Image img = new Image();
                    Images[r, c] = img;
                    img.Source = BlockType[Board.d[r, c]];
                    grid_table.Children.Add(img);
                    Grid.SetColumn(img, c);
                    Grid.SetRow(img, r);
                }
            }
        }


        private void PlayerOperate(PlayerOperation o) {

            /**
             * Get variables.
             */
            Image playerImage = Images[player.y, player.x];
            Image backgroundImage = null;
            Image nextImage = null;

            int bg_y = -1;
            int bg_x = -1;
            int nxt_y = -1;
            int nxt_x = -1;
            
            ref GridType playerGrid = ref Board.d[player.y, player.x];

            switch (o) {
                case PlayerOperation.MoveUp:
                {
                    if (player.y == 0)
                    {
                        return;
                    }
                    backgroundImage = Images[player.y - 1, player.x];
                    nextImage = (player.y == 1) ? null : Images[player.y - 2, player.x];
                    bg_y = player.y - 1;
                    bg_x = player.x;
                    nxt_y = player.y - 2;
                    nxt_x = player.x;

                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    if (player.y == Board.rowNum - 1)
                    {
                        return;
                    }
                    backgroundImage = Images[player.y + 1, player.x];
                    nextImage = (player.y == Board.rowNum - 2) ? null : Images[player.y + 2, player.x];

                    bg_y = player.y + 1;
                    bg_x = player.x;
                    nxt_y = player.y + 2;
                    nxt_x = player.x;

                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    if (player.x == 0)
                    {
                        return;
                    }
                    backgroundImage = Images[player.y, player.x - 1];
                    nextImage = (player.x == 1) ? null : Images[player.y, player.x - 2];

                    bg_y = player.y;
                    bg_x = player.x - 1;
                    nxt_y = player.y;
                    nxt_x = player.x - 2;

                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    if (player.x == Board.columnNum - 1)
                    {
                        return;
                    }
                    backgroundImage = Images[player.y, player.x + 1];
                    nextImage = (player.x == Board.columnNum - 2) ? null : Images[player.y, player.x + 2];

                    bg_y = player.y;
                    bg_x = player.x + 1;
                    nxt_y = player.y;
                    nxt_x = player.x + 2;

                    break;
                }
            }

            /**
             * Logic Judgement.
             */

            // reached brick
            if (backgroundImage.Source == BlockType[GridType.Brick])
            {
                return;
            }
            // move to ground
            else if (backgroundImage.Source == BlockType[GridType.Ground])
            {
                backgroundImage.Source = BlockType[GridType.Player];
                Board.d[bg_y, bg_x] = GridType.Player;

                if (Board.d[player.y, player.x] == GridType.TarPlayer)
                {
                    playerImage.Source = BlockType[GridType.Target];
                    Board.d[player.y, player.x] = GridType.Target;
                }
                else 
                {
                    playerImage.Source = BlockType[GridType.Ground];
                    Board.d[player.y, player.x] = GridType.Ground;
                }

            }
            // move to target
            else if (backgroundImage.Source == BlockType[GridType.Target])
            {
                backgroundImage.Source = BlockType[GridType.TarPlayer];
                Board.d[bg_y, bg_x] = GridType.TarPlayer;

                if (Board.d[player.y, player.x] == GridType.TarPlayer)
                {
                    playerImage.Source = BlockType[GridType.Target];
                    Board.d[player.y, player.x] = GridType.Target;
                }
                else
                {
                    playerImage.Source = BlockType[GridType.Ground];
                    Board.d[player.y, player.x] = GridType.Ground;
                }


            }
            // push box
            else if (backgroundImage.Source == BlockType[GridType.Box])
            {
                if (nextImage == null)
                {
                    return;
                }

                if (nextImage.Source == BlockType[GridType.Ground]
                    || nextImage.Source == BlockType[GridType.Target])
                {
                    // self
                    if (Board.d[player.y, player.x] == GridType.TarPlayer)
                    {
                        playerImage.Source = BlockType[GridType.Target];
                        Board.d[player.y, player.x] = GridType.Target;
                    }
                    else
                    {
                        playerImage.Source = BlockType[GridType.Ground];
                        Board.d[player.y, player.x] = GridType.Ground;
                    }

                    // bg
                    backgroundImage.Source = BlockType[GridType.Player];
                    Board.d[bg_y, bg_x] = GridType.Player;

                    // next
                    if (nextImage.Source == BlockType[GridType.Ground])
                    {
                        nextImage.Source = BlockType[GridType.Box];
                        Board.d[nxt_x, nxt_y] = GridType.Box;
                    }
                    else if (nextImage.Source == BlockType[GridType.Target])
                    {
                        nextImage.Source = BlockType[GridType.RedBox];
                        Board.d[nxt_x, nxt_y] = GridType.RedBox;
                    }


                }
                else if (nextImage.Source == BlockType[GridType.Brick]
                    || nextImage.Source == BlockType[GridType.Box]
                    || nextImage.Source == BlockType[GridType.RedBox]) 
                {
                    return;
                }
                else
                {
                    throw new Exception("Map Data damaged.");
                }
            }
            // push red box
            else if (backgroundImage.Source == BlockType[GridType.RedBox])
            {
                if (nextImage == null)
                {
                    return;
                }

                if (nextImage.Source == BlockType[GridType.Ground]
                    || nextImage.Source == BlockType[GridType.Target])
                {

                    // self
                    if (Board.d[player.y, player.x] == GridType.TarPlayer)
                    {
                        playerImage.Source = BlockType[GridType.Target];
                        Board.d[player.y, player.x] = GridType.Target;
                    }
                    else
                    {
                        playerImage.Source = BlockType[GridType.Ground];
                        Board.d[player.y, player.x] = GridType.Ground;
                    }

                    // bg
                    backgroundImage.Source = BlockType[GridType.TarPlayer];
                    Board.d[bg_y, bg_x] = GridType.TarPlayer;

                    // next
                    if (nextImage.Source == BlockType[GridType.Ground])
                    {
                        nextImage.Source = BlockType[GridType.Box];
                        Board.d[nxt_x, nxt_y] = GridType.Box;
                    }
                    else if (nextImage.Source == BlockType[GridType.Target])
                    {
                        nextImage.Source = BlockType[GridType.RedBox];
                        Board.d[nxt_x, nxt_y] = GridType.RedBox;
                    }

                }
                else if (nextImage.Source == BlockType[GridType.Brick]
                    || nextImage.Source == BlockType[GridType.Box]
                    || nextImage.Source == BlockType[GridType.RedBox])
                {
                    return;
                }
                else
                {
                    throw new Exception("Map Data damaged.");
                }
            }
            // wrong state
            else
            {
                throw new Exception("Map Data damaged.");
            }

            /**
             * Move Player.
             */
            switch (o)
            {
                case PlayerOperation.MoveUp:
                {
                    player.y -= 1;
                    break;
                }
                case PlayerOperation.MoveDown:
                {
                    player.y += 1;
                    break;
                }
                case PlayerOperation.MoveLeft:
                {
                    player.x -= 1;
                    break;
                }
                case PlayerOperation.MoveRight:
                {
                    player.x += 1;
                    break;
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
                        PlayerOperate(PlayerOperation.MoveUp);
                        break;
                    }
                case VirtualKey.S:
                case VirtualKey.Down:
                    {
                        PlayerOperate(PlayerOperation.MoveDown);
                        break;
                    }
                case VirtualKey.D:
                case VirtualKey.Right:
                    {
                        PlayerOperate(PlayerOperation.MoveRight);
                        break;
                    }
                case VirtualKey.A:
                case VirtualKey.Left:
                    {
                        PlayerOperate(PlayerOperation.MoveLeft);
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
