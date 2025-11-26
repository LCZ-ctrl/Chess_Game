using System.Windows;

namespace ChessUI
{
    public partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            InitializeComponent();
            // 确保不激活自己
            this.ShowActivated = false;
            // XAML 中已经设置了 ShowInTaskbar = True，请不要在别处重复设置同一属性导致冲突。
        }
    }
}
