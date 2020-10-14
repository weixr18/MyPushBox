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
using System.Linq;

namespace MyPushBox
{

    /// <summary>
    /// Main Page of the program.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private class InfoPack
        {
            public BoardInfo Info;
            public GameEngine Engine;

            public InfoPack(BoardInfo info, GameEngine engine) {
                Info = info;
                Engine = engine;
            }
        }

        private const int GRID_SIZE = 50;
        private Dictionary<char, GridType> BlockCode = new Dictionary<char, GridType>();
        private Dictionary<GridType, BitmapImage> BlockType = new Dictionary<GridType, BitmapImage>();
        private BitmapImage BitGround = new BitmapImage(new Uri("ms-appx:///Assets/Block/Ground.jpg"));
        private BitmapImage BitBrick = new BitmapImage(new Uri("ms-appx:///Assets/Block/Brick.jpg"));
        private BitmapImage BitPlayer = new BitmapImage(new Uri("ms-appx:///Assets/Block/Player.jpg"));
        private BitmapImage BitBox = new BitmapImage(new Uri("ms-appx:///Assets/Block/Box.jpg"));
        private BitmapImage BitTarget = new BitmapImage(new Uri("ms-appx:///Assets/Block/Target.jpg"));
        private BitmapImage BitRedBox = new BitmapImage(new Uri("ms-appx:///Assets/Block/RedBox.jpg"));
        private BitmapImage BitOutside = new BitmapImage(new Uri("ms-appx:///Assets/Block/Outside.jpg"));
       

        private Image[,] Images;
        private List<PlayerOperation> AnswerPath;
        private BoardInfo StartInfo;

        public BoardInfo Info;
        public GameEngine Engine;
        public AIPlayer AI;
        

        /// <summary>
        /// checked.
        /// </summary>
        public MainPage()
        {
            
            // layout component initialize
            this.InitializeComponent();
            PathText.Text = "Welcome.";

            // other properties initialize
            this.InitializeMaps();
            
            var pack = GetBoardInfo(1).Result;
            Engine = pack.Engine;
            Info = pack.Info;
            StartInfo = Info.Clone();

            AI = new AIPlayer();

            // grid initialize and paint
            this.InitializeGrid();
            this.RefreshGrid();
        }

        /// <summary>
        /// checked
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;
        }

        /// <summary>
        /// checked
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;
        }

        /// <summary>
        /// checked.
        /// </summary>
        private void InitializeMaps() 
        {

            BlockCode.Add('-', GridType.Ground);  
            BlockCode.Add('#', GridType.Brick);  
            BlockCode.Add('@', GridType.Ground);  
            BlockCode.Add('$', GridType.Ground);  
            BlockCode.Add('.', GridType.Target);  
            BlockCode.Add('_', GridType.OutSide);
            BlockCode.Add('*', GridType.Target);

            BlockType.Add(GridType.Ground, BitGround);
            BlockType.Add(GridType.Brick, BitBrick);
            BlockType.Add(GridType.Target, BitTarget);
            BlockType.Add(GridType.OutSide, BitOutside);
        }

        /// <summary>
        /// Read the statistic and dynamic information from the file. checked.
        /// </summary>
        /// <returns></returns>
        private async Task<InfoPack> GetBoardInfo(int stageIndex)
        {
            MyPoint player = new MyPoint(-1, -1);
            List<MyPoint> boxes = new List<MyPoint>();
            List<MyPoint> targets = new List<MyPoint>();
            GridType[,] gridMatrix;
            BoardInfo info;
            GameEngine engine;

            try
            {
                var uri = new Uri(String.Format("ms-appx:///Assets/Stages/{0:G}.txt", stageIndex)); 
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
                var encoding = Encoding.GetEncoding("utf-8");

                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, false))
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            throw new FileLoadException("Wrong Stage File Format.");
                        }

                        string[] numStrings = line.Split(' ');
                        int rowNum = Convert.ToInt32(numStrings[0], 10);
                        int columnCount = Convert.ToInt32(numStrings[1], 10);
                        gridMatrix = new GridType[rowNum, columnCount];

                        for (int i = 0; i < rowNum; i++)
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
                                gridMatrix[i, j] = BlockCode[line[j]];

                                if (line[j] == '@')
                                {
                                    // player
                                    player.Y = i;
                                    player.X = j;
                                    // i is row index -- y
                                    // j is column index -- x
                                }
                                else if (line[j] == '$') 
                                {
                                    // box
                                    var box = new MyPoint(j, i);
                                    boxes.Add(box);
                                }
                                else if (line[j] == '*')
                                {
                                    // red box
                                    var box = new MyPoint(j, i);
                                    boxes.Add(box);
                                    var target = new MyPoint(j, i);
                                    targets.Add(target);
                                }
                                else if (line[j] == '.')
                                {
                                    // target list: only for the AI.
                                    var target = new MyPoint(j, i);
                                    targets.Add(target);
                                }

                            }
                        }

                        if (player.X < 0 || player.Y < 0)
                        {
                            throw new FileLoadException("No Player Position in Stage File.");
                        }

                        info = new BoardInfo(player, boxes);
                        engine = new GameEngine(gridMatrix, targets);
                       
                    }
                }

                return new InfoPack(info, engine);
            }
            catch (Exception e)
            {
                var message = "The file could not be read:";
                message += e.Message;
                PathText.Text = message;
                return null;
            }
        }

        /// <summary>
        /// Initialize the grids. Checked.
        /// </summary>
        private void InitializeGrid()
        {
            int rowNum = Engine.RowNum;
            int columnNum = Engine.ColumnNum;
            Grid g = GridTable;
            g.RowDefinitions.Clear();
            g.ColumnDefinitions.Clear();
            Images = new Image[rowNum, columnNum];

            for (int i = 0; i < rowNum; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(GRID_SIZE);
                g.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < columnNum; i++)
            {
                ColumnDefinition rd = new ColumnDefinition();
                rd.Width = new GridLength(GRID_SIZE);
                g.ColumnDefinitions.Add(rd);
            }

        }


        /// <summary>
        /// Refresh the grids according to GameEngine and BoardInfo. Checked.
        /// </summary>
        private void RefreshGrid() {

            int rowNum = Engine.RowNum;
            int columnNum = Engine.ColumnNum;
            GridType[,] g = Engine.GridMatrix;
            Image img;

            // background
            for (int r = 0; r < rowNum; r++)
            {
                for (int c = 0; c < columnNum; c++)
                {
                    img = new Image();
                    Images[r, c] = img;
                    img.Source = BlockType[g[r, c]];
                    GridTable.Children.Add(img);
                    Grid.SetColumn(img, c);
                    Grid.SetRow(img, r);
                }
            }

            // player
            img = new Image();
            Images[Info.Player.Y, Info.Player.X] = img;
            img.Source = BitPlayer;     // player will cover the target.
            GridTable.Children.Add(img);
            Grid.SetColumn(img, Info.Player.X);
            Grid.SetRow(img, Info.Player.Y);
            

            // boxes
            foreach (var box in Info.Boxes)
            {
                img = new Image();
                Images[box.Y, box.X] = img;

                if (g[box.Y, box.X] == GridType.Target) 
                    img.Source = BitRedBox;
                else 
                    img.Source = BitBox;
                
                GridTable.Children.Add(img);
                Grid.SetColumn(img, box.X);
                Grid.SetRow(img, box.Y);
            }

        }


        /// <summary>
        /// Responding to keyboard press down events. Checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            VirtualKey virtualKey = args.VirtualKey;
            switch (virtualKey)
            {
                case VirtualKey.W:
                case VirtualKey.Up:
                {
                    Engine.PlayerOperate(PlayerOperation.Up, Info);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.S:
                case VirtualKey.Down:
                {
                    Engine.PlayerOperate(PlayerOperation.Down, Info);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.D:
                case VirtualKey.Right:
                {
                    Engine.PlayerOperate(PlayerOperation.Right, Info);
                    RefreshGrid();
                    break;
                }
                case VirtualKey.A:
                case VirtualKey.Left:
                {
                    Engine.PlayerOperate(PlayerOperation.Left, Info);
                    RefreshGrid();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Start the search. Checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Button_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                AI.SetStartBoard(StartInfo, Engine);
                PathText.Text = "Searching...";
                var timeStart = DateTime.Now;
                AnswerPath = AI.SearchPath();
                ShowPathButton.IsEnabled = true;

                var timeCost = DateTime.Now - timeStart;
                String message = "Shortest path found.\n";
                message += String.Format("Path length: {0:G}. Search time: {1:G}\n", AnswerPath.Count, timeCost);
                message += "Path: ";
                foreach (var operation in AnswerPath) {
                    message += operation + "  ";
                }
                message += "\n";
                PathText.Text = message;
            }
            catch (OutOfMemoryException) 
            {
                PathText.Text = "Search stopped because of an out of memory exception.";
            }
            
        }

        private void Show_Button_Clicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void Reset_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (StageChoose.SelectedValue != null) {
                PathText.Text = "Current Stage: " + StageChoose.SelectedValue.ToString();
            }
            
            Info = StartInfo.Clone();
            this.RefreshGrid();
        }

        /// <summary>
        /// User choose a new stage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stage_Changed(object sender, RoutedEventArgs e)
        {
            var selectedStage = int.Parse(StageChoose.SelectedValue.ToString());
            var pack = GetBoardInfo(selectedStage).Result;
            Engine = pack.Engine;
            Info = pack.Info;
            StartInfo = Info.Clone();
            PathText.Text = "Current Stage: " + selectedStage.ToString();
            AnswerPath = null;
            ShowPathButton.IsEnabled = false;
            this.InitializeGrid();
            this.RefreshGrid();
        }
    }
}
