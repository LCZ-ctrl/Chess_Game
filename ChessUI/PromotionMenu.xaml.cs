using ChessLogic;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI
{
    public partial class PromotionMenu : UserControl
    {
        public event Action<PieceType> PieceSelected;
        private Player currentPlayer;

        public PromotionMenu(Player player)
        {
            InitializeComponent();

            currentPlayer = player;

            // 初始化棋子图片
            QueenImg.Source = Images.GetImage(player, PieceType.Queen);
            RookImg.Source = Images.GetImage(player, PieceType.Rook);
            BishopImg.Source = Images.GetImage(player, PieceType.Bishop);
            KnightImg.Source = Images.GetImage(player, PieceType.Knight);

            // 初始化顶部文字
            UpdateLanguage();

            // 订阅语言切换事件，只修改顶部文字
            LanguageManager.LanguageChanged += UpdateLanguage;
        }

        // 仅修改顶部标题文字
        private void UpdateLanguage()
        {
            switch (LanguageManager.CurrentLanguage)
            {
                case LanguageType.English:
                    TitleText.Text = "SELECT A PIECE";
                    break;
                case LanguageType.Chinese:
                    TitleText.Text = "选择棋子";
                    break;
                case LanguageType.Russian:
                    TitleText.Text = "ВЫБЕРИТЕ ФИГУРУ";
                    break;
            }
        }

        // 四个棋子选择事件，保持原逻辑
        private void QueenImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen);
        }

        private void RookImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook);
        }

        private void BishopImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop);
        }

        private void KnightImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight);
        }
    }
}
