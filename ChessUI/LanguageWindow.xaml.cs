using System.Windows;

namespace ChessUI
{
    public partial class LanguageWindow : Window
    {
        public LanguageWindow()
        {
            InitializeComponent();
            UpdateTitle();

            

            // 可选：语言变更时更新标题
            LanguageManager.LanguageChanged += UpdateTitle;
        }

        private void UpdateTitle()
        {
            switch (LanguageManager.CurrentLanguage)
            {
                case LanguageType.English:
                    LangTitle.Text = "LANGUAGE";
                    break;
                case LanguageType.Chinese:
                    LangTitle.Text = "语言";
                    break;
                case LanguageType.Russian:
                    LangTitle.Text = "ЯЗЫК";
                    break;
            }
        }

        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            // 不立即调用 LanguageManager.SetLanguage —— 延后到黑屏完全可见时调用
            WindowManager.Show(WindowManager.MainMenu, this, 0, () =>
            {
                LanguageManager.SetLanguage(LanguageType.English);
            });
        }

        private void ChineseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Show(WindowManager.MainMenu, this, 0, () =>
            {
                LanguageManager.SetLanguage(LanguageType.Chinese);
            });
        }

        private void RussianButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Show(WindowManager.MainMenu, this, 0, () =>
            {
                LanguageManager.SetLanguage(LanguageType.Russian);
            });
        }

        private void OpenMainMenu()
        {
            // 保留原始无回调的跳转（若其他地方调用）
            WindowManager.Show(WindowManager.MainMenu, this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
