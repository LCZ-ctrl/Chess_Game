using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessLogic;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        private GameState gameState;
        private Position selectedPos = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();


            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void InitializeBoard()
        {
            for(int r = 0; r < 8; r++)
            {
                for(int c = 0; c < 8; c++)
                {
                    //Image image = new Image();
                    //pieceImages[r, c] = image;
                    //PieceGrid.Children.Add(image);

                    //Rectangle highlight = new Rectangle();
                    //highlights[r, c] = highlight;
                    //HighlightGrid.Children.Add(highlight);

                    Image image = new Image
                    {
                        Stretch = Stretch.Uniform,  // 保持比例
                        HorizontalAlignment = HorizontalAlignment.Center,  // 水平居中
                        VerticalAlignment = VerticalAlignment.Center,  // 垂直居中
                        //RenderOptions.BitmapScalingMode = BitmapScalingMode.HighQuality  // 高质量缩放（可选，防模糊）
                    };

                    // 添加 0.8 倍缩放（以中心点为原点，避免偏移）
                    image.RenderTransform = new ScaleTransform(0.8, 0.8);
                    image.RenderTransformOrigin = new Point(0.5, 0.5);

                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for(int r = 0; r < 8; r++)
            {
                for(int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if(selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                {
                    OnToPositionSelected(pos);
                }
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMoveForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHightlights();

            if(moveCache.TryGetValue(pos, out Move move))
            {
                if(move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        //private void HandleMove(Move move)
        //{
        //    gameState.MakeMove(move);
        //    DrawBoard(gameState.Board);
        //    SetCursor(gameState.CurrentPlayer);

        //    if (gameState.IsGameOver())
        //    {
        //        ShowGameOver();
        //    }
        //}
        private void HandleMove(Move move)
        {
            // ==================== 音效判断区（关键修改）====================
            switch (move.Type)
            {
                case MoveType.CastleKS:
                case MoveType.CastleQS:
                    MusicManager.PlaySound("castle");
                    break;

                case MoveType.PawnPromotion:
                    MusicManager.PlaySound("promote");  // 兵升变专用音效
                    break;

                case MoveType.EnPassant:
                case MoveType.Normal when gameState.Board[move.ToPos] != null:  // 吃子（包括吃过路兵）
                    MusicManager.PlaySound("capture");
                    break;

                default:  // 包括 Normal（非吃子）、DoublePawn
                    MusicManager.PlaySound("move");
                    break;
            }

            // 执行移动
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            // 移动后检查是否将军（对方国王被将军）
            if (gameState.Board.IsInCheck(gameState.CurrentPlayer.Opponent()))
            {
                MusicManager.PlaySound("check");
            }

            if (gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }



        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach(Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);

            foreach(Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHightlights()
        {
            foreach(Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if(player == Player.White)
            {
                Cursor = ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor = ChessCursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;

                switch (option)
                {
                    case Option.Restart:
                        RestartGame();
                        break;
                    case Option.Menu:
                        OpenMainMenu();
                        break;
                    case Option.Exit:
                        Application.Current.Shutdown();
                        break;
                }
            };
        }

        private void OpenMainMenu()
        {
            // 使用预加载的 MainMenu，并隐藏当前游戏窗口（不用 Close）
            // 如果希望能在黑屏完全遮住时执行额外操作，可传 onOpaque 回调
            WindowManager.Show(WindowManager.MainMenu, this);


            // 回主菜单必定从头播放
            MusicManager.PlayMenuMusic();

        }


        public void RestartGame()
        {
            selectedPos = null;
            HideHightlights();
            moveCache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(!IsMenuOnScreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;

                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

    }
}