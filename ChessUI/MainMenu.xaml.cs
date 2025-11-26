using System;
using System.Windows;

namespace ChessUI
{
    public partial class MainMenu : Window
    {
        // 控制规则框显示/隐藏
        private bool _instructionVisible = false;

        public MainMenu()
        {
            InitializeComponent();
            SetLanguage(LanguageManager.CurrentLanguage);

            // 任何时候主菜单显示 -> 都从头开始播放
            this.IsVisibleChanged += (s, e) =>
            {
                if (this.IsVisible)
                    MusicManager.PlayMenuMusic();
            };

            // 监听全局语言变更（其它地方也可能触发）
            LanguageManager.LanguageChanged += () => SetLanguage(LanguageManager.CurrentLanguage);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // 开始游戏前，停止菜单音乐
            MusicManager.Stop();


            // durationMs = 0 表示“立即切换（无黑屏）”
            WindowManager.MainWindow.RestartGame();
            WindowManager.Show(WindowManager.GameWindow, this, 0);
        }

        private void InstructionButton_Click(object sender, RoutedEventArgs e)
        {
            _instructionVisible = !_instructionVisible;
            InstructionBox.Visibility = _instructionVisible ? Visibility.Visible : Visibility.Collapsed;

            if (_instructionVisible)
                UpdateInstructionText();
        }

        private void CloseInstructionButton_Click(object sender, RoutedEventArgs e)
        {
            _instructionVisible = false;
            InstructionBox.Visibility = Visibility.Collapsed;
        }


        // 改动点：把原来打开 LanguageWindow 的逻辑改为“循环切换语言”
        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // 计算下一个语言（English -> Chinese -> Russian -> English ...）
            var next = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English => LanguageType.Chinese,
                LanguageType.Chinese => LanguageType.Russian,
                LanguageType.Russian => LanguageType.English,
                _ => LanguageType.English
            };

            // 设置为下一个语言。LanguageManager 应触发 LanguageChanged 事件，其他窗口会更新。
            LanguageManager.SetLanguage(next);

            // 立即更新当前窗口的显示，防止短时间内因事件顺序造成延迟（冗余但稳妥）
            SetLanguage(next);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown(); // 退出整个程序
        }

        public void SetLanguage(LanguageType lang)
        {
            switch (lang)
            {
                case LanguageType.English:
                    TitleText.Text = "CHESS";
                    PlayButton.Content = "PLAY";
                    InstructionButton.Content = "INSTRUCTION";
                    LanguageButton.Content = "LANGUAGE";
                    ExitButton.Content = "EXIT";
                    break;
                case LanguageType.Chinese:
                    TitleText.Text = "国际象棋";
                    PlayButton.Content = "开始游玩";
                    InstructionButton.Content = "规则说明";
                    LanguageButton.Content = "语言";
                    ExitButton.Content = "退出";
                    break;
                case LanguageType.Russian:
                    TitleText.Text = "ШАХМАТЫ";
                    PlayButton.Content = "ИГРАТЬ";
                    InstructionButton.Content = "ПРАВИЛА";
                    LanguageButton.Content = "ЯЗЫК";
                    ExitButton.Content = "ВЫХОД";
                    break;
            }
        }

        private void UpdateInstructionText()
        {
            string text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English =>
@"Rules:

1. White moves first. Players take turns. The mouse cursor color indicates whose turn it is.

2. Checkmate the opponent to win.

3. Stalemate or insufficient material results in a draw.

4. Threefold repetition causes a draw.

5. Fifty-move rule: If 50 consecutive turns pass without pawn move or capture, it's a draw.

6. Language switch: English → Chinese → Russian

7. Enjoy the game!",
                LanguageType.Chinese =>
@"规则说明:

1. 白方先手，每轮玩家轮流移动棋子，鼠标的颜色决定轮到哪一方下棋

2. 将死对方者获胜

3. 僵局或者棋力不足将导致平局

4. 如果有三次重复局面出现，将导致平局

5. 如果连续50回合，没有兵被移动，也没有吃子将触发50步规则导致平局

6. 语言切换：英语 → 中文 → 俄语

7. 玩的开心！",
                LanguageType.Russian =>
@"Правила:

1. Белые ходят первыми. Игроки ходят по очереди. Цвет курсора мыши показывает, чей ход.

2. Побеждает тот, кто ставит мат противнику.

3. Пат или недостаточно фигур приводит к ничьей.

4. При трёхкратном повторении позиции объявляется ничья.

5. Правило 50-ходов: если 50 ходов подряд нет движения пешки и взятия фигур, объявляется ничья.

6. Переключение языка: английский → китайский → русский

7. Удачной игры!",
                _ => ""
            };

            InstructionText.Text = text;
        }
    }

    public enum LanguageType
    {
        English,
        Chinese,
        Russian
    }
}
