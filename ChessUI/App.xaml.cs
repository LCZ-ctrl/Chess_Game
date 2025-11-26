using System.Windows;

namespace ChessUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 一次性创建好所有窗口
            WindowManager.Init();

            // 显示主菜单（可带淡入）
            WindowManager.Show(WindowManager.MainMenu);
        }
    }
}
