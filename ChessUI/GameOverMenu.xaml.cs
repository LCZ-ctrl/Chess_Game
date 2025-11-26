using ChessLogic;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        private GameState _gameState;

        public GameOverMenu(GameState gameState)
        {
            InitializeComponent();

            _gameState = gameState;

            // 初始化显示
            UpdateLanguage();

            // 订阅语言切换事件
            LanguageManager.LanguageChanged += UpdateLanguage;
        }

        private void UpdateLanguage()
        {
            var result = _gameState.Result;

            // WinnerText
            WinnerText.Text = result.Winner switch
            {
                Player.White => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "WHITE WINS!",
                    LanguageType.Chinese => "白方胜利！",
                    LanguageType.Russian => "БЕЛЫЕ ПОБЕДИЛИ!",
                    _ => "WHITE WINS!"
                },
                Player.Black => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "BLACK WINS!",
                    LanguageType.Chinese => "黑方胜利！",
                    LanguageType.Russian => "ЧЁРНЫЕ ПОБЕДИЛИ!",
                    _ => "BLACK WINS!"
                },
                _ => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "IT'S A DRAW",
                    LanguageType.Chinese => "平局",
                    LanguageType.Russian => "НИЧЬЯ",
                    _ => "IT'S A DRAW"
                }
            };

            // ReasonText
            ReasonText.Text = GetReasonText(result.Reason, _gameState.CurrentPlayer);

            // 按钮文字
            RestartText.Text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English => "PLAY AGAIN",
                LanguageType.Chinese => "重新开始",
                LanguageType.Russian => "ПЕРЕЗАПУСК",
                _ => "PLAY AGAIN"
            };
            MenuText.Text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English => "MENU",
                LanguageType.Chinese => "菜单",
                LanguageType.Russian => "МЕНЮ",
                _ => "MENU"
            };
            ExitText.Text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English => "EXIT",
                LanguageType.Chinese => "退出",
                LanguageType.Russian => "ВЫХОД",
                _ => "EXIT"
            };
        }

        private string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "WHITE",
                    LanguageType.Chinese => "白方",
                    LanguageType.Russian => "БЕЛЫЕ",
                    _ => "WHITE"
                },
                Player.Black => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "BLACK",
                    LanguageType.Chinese => "黑方",
                    LanguageType.Russian => "ЧЁРНЫЕ",
                    _ => "BLACK"
                },
                _ => ""
            };
        }

        private string GetReasonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => $"STALEMATE - {PlayerString(currentPlayer)} CAN'T MOVE",
                    LanguageType.Chinese => $"僵局 - {PlayerString(currentPlayer)} 无法移动",
                    LanguageType.Russian => $"ПАТ - {PlayerString(currentPlayer)} НЕ МОЖЕТ ХОДИТЬ",
                    _ => $"STALEMATE - {PlayerString(currentPlayer)} CAN'T MOVE"
                },
                EndReason.Checkmate => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => $"CHECKMATE - {PlayerString(currentPlayer)} CAN'T MOVE",
                    LanguageType.Chinese => $"将死 - {PlayerString(currentPlayer)} 无法移动",
                    LanguageType.Russian => $"ШАХ И МАТ - {PlayerString(currentPlayer)} НЕ МОЖЕТ ХОДИТЬ",
                    _ => $"CHECKMATE - {PlayerString(currentPlayer)} CAN'T MOVE"
                },
                EndReason.FiftyMoveRule => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "FIFTY-MOVE RULE",
                    LanguageType.Chinese => "五十步规则",
                    LanguageType.Russian => "ПРАВИЛО 50-ХОДОВ",
                    _ => "FIFTY-MOVE RULE"
                },
                EndReason.InsufficientMaterial => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "INSUFFICIENT MATERIAL",
                    LanguageType.Chinese => "棋子不足",
                    LanguageType.Russian => "НЕДОСТАТОЧНО МАТЕРИАЛА",
                    _ => "INSUFFICIENT MATERIAL"
                },
                EndReason.ThreefoldRepetition => LanguageManager.CurrentLanguage switch
                {
                    LanguageType.English => "THREEFOLD REPETITION",
                    LanguageType.Chinese => "三次重复局面",
                    LanguageType.Russian => "ТРИКРАТНОЕ ПОВТОРЕНИЕ",
                    _ => "THREEFOLD REPETITION"
                },
                _ => ""
            };
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Menu);
        }
    }
}
