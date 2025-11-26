using System;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    public partial class PauseMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public PauseMenu()
        {
            InitializeComponent();

            // 初始化显示
            UpdateLanguage();

            // 订阅语言切换事件
            LanguageManager.LanguageChanged += UpdateLanguage;
        }

        private void UpdateLanguage()
        {
            switch (LanguageManager.CurrentLanguage)
            {
                case LanguageType.English:
                    PauseTitle.Text = "Pause";
                    ContinueText.Text = "CONTINUE";
                    RestartText.Text = "RESTART";
                    MenuText.Text = "MENU";
                    break;
                case LanguageType.Chinese:
                    PauseTitle.Text = "暂停";
                    ContinueText.Text = "继续";
                    RestartText.Text = "重新开始";
                    MenuText.Text = "菜单";
                    break;
                case LanguageType.Russian:
                    PauseTitle.Text = "Пауза";
                    ContinueText.Text = "ПРОДОЛЖИТЬ";
                    RestartText.Text = "ПЕРЕЗАПУСК";
                    MenuText.Text = "МЕНЮ";
                    break;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Continue);
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            // 获取宿主窗口（应该是 MainWindow 游戏窗口）
            var wnd = Window.GetWindow(this) as MainWindow;
            if (wnd != null)
            {
                // 清空 MenuContainer（避免再次回到游戏时仍有 PauseMenu）
                wnd.MenuContainer.Content = null;

                // 立即回到主菜单：durationMs = 0 表示不使用黑屏过渡
                WindowManager.Show(WindowManager.MainMenu, wnd, durationMs: 0);
            }
            else
            {
                // 回退方案：直接显示预创建的主菜单（无来源）
                WindowManager.Show(WindowManager.MainMenu, null, durationMs: 0);
            }
        }

    }
}
