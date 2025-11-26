using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ChessUI
{
    /// <summary>
    /// 负责统一管理预加载窗口与带黑屏过渡的显示逻辑（支持在黑屏完全可见时执行回调）。
    /// 现在增加了一个“ShellWindow”作为任务栏唯一的持有者，以避免任务栏图标闪烁。
    /// </summary>
    public static class WindowManager
    {
        // 新增：始终存在的“壳”窗口。任务栏图标由它持有。
        public static ShellWindow Shell { get; private set; }

        public static MainMenu MainMenu { get; private set; }
        public static LanguageWindow LanguageWindow { get; private set; }
        public static MainWindow MainWindow { get; private set; }

        // 兼容：若你在其他地方使用 WindowManager.GameWindow（比如 MainMenu），把它当作 MainWindow 的别名
        public static Window GameWindow => MainWindow;

        public static void Init()
        {
            // 1) 创建并显示一个不可见但驻留任务栏的 Shell 窗口
            Shell = new ShellWindow();
            // 将 Shell 设为 Application 的 MainWindow（任务栏及激活相关由它承担）
            Application.Current.MainWindow = Shell;
            // 直接 Show（它是透明且位于屏幕外，所以对用户不可见）
            Shell.Show();

            // 2) 创建其它窗口，但把它们的 Owner 指向 Shell，并且不在任务栏显示
            //    这样，任务栏图标始终来自 Shell，切换其它窗口不会导致任务栏图标变动或闪烁。
            MainMenu = new MainMenu
            {
                Owner = Shell,
                ShowInTaskbar = false
            };

            LanguageWindow = new LanguageWindow
            {
                Owner = Shell,
                ShowInTaskbar = false
            };

            MainWindow = new MainWindow
            {
                Owner = Shell,
                ShowInTaskbar = false
            };
            

            // 不要在这里显示其它窗口 — 你后面会通过 WindowManager.Show 控制显示逻辑
        }

        /// <summary>
        /// 显示目标窗口。若提供 from 且可见，则在 from -> target 之间做黑屏过渡。
        /// onOpaque 可选：当黑屏完全遮住画面后（在黑屏渐入动画完成时）调用该回调，再进行界面切换（比如设置语言）。
        /// durationMs 总时长（默认 400ms），平均分给黑入与黑出。
        /// </summary>
        /// <summary>
        /// 显示目标窗口。若提供 from 且可见，则在 from -> target 之间做黑屏过渡。
        /// 若传入 durationMs <= 0，则**立即**切换（不做黑屏过渡）。
        /// onOpaque 可选：当黑屏完全遮住画面后调用（或立即切换时也会被调用）。
        /// </summary>
        public static void Show(Window target, Window from = null, int durationMs = 0, Action onOpaque = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            // ✅ 特殊情况：从游戏窗口直接回到菜单，不要黑屏，直接切换
            if (from == MainWindow && target == MainMenu)
            {
                onOpaque?.Invoke();

                try
                {
                    if (from.IsVisible) from.Hide();
                }
                catch { }

                if (!target.IsVisible)
                {
                    target.Show();
                }
                else
                {
                    target.Activate();
                }

                return;
            }

            // 若没有来源窗口或来源不可见，则直接（或先执行 onOpaque）显示目标
            if (from == null || !from.IsVisible)
            {
                onOpaque?.Invoke();

                if (!target.IsVisible)
                {
                    target.Show();
                }
                else
                {
                    target.Activate();
                }

                return;
            }

            // 默认：正常黑屏过渡
            RunTransition(from, target, Math.Max(0, durationMs), onOpaque);
        }



        // RunTransition 保持原样 —— 不需要改动（Overlay 已经设 ShowInTaskbar = false）
        private static void RunTransition(Window from, Window target, int totalMs, Action onOpaque)
        {
            if (from == null || target == null) return;

            var halfMs = Math.Max(1, totalMs / 2);

            // 创建覆盖 overlay，Owner = from，保证覆盖来源窗口且不出现在任务栏
            Window overlay = new Window
            {
                Owner = from,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Black,
                Opacity = 0,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Width = from.ActualWidth > 0 ? from.ActualWidth : from.Width,
                Height = from.ActualHeight > 0 ? from.ActualHeight : from.Height,
                Left = from.Left,
                Top = from.Top,
                Topmost = true
            };

            // 兜底：若尺寸无效则尝试 RestoreBounds
            if (double.IsNaN(overlay.Width) || overlay.Width <= 0)
            {
                try
                {
                    overlay.Width = from.RestoreBounds.Width;
                    overlay.Height = from.RestoreBounds.Height;
                    overlay.Left = from.RestoreBounds.Left;
                    overlay.Top = from.RestoreBounds.Top;
                }
                catch { }
            }

            overlay.Show();

            // 黑入动画（0 -> 1）
            var fadeIn = new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromMilliseconds(halfMs)))
            {
                FillBehavior = FillBehavior.HoldEnd
            };

            fadeIn.Completed += (s, e) =>
            {
                // 黑屏已完全遮住 —— 在这里执行用户指定的回调（例如设置语言）
                try
                {
                    onOpaque?.Invoke();
                }
                catch
                {
                    // 忽略回调中的异常，保证过渡能继续完成
                }

                // 隐藏来源窗口（不 Close，以保留预加载实例）
                try
                {
                    if (from.IsVisible) from.Hide();
                }
                catch { }

                // 显示目标窗口
                try
                {
                    if (!target.IsVisible)
                    {
                        target.Show();
                    }
                    else
                    {
                        target.Activate();
                    }
                }
                catch { }

                // 黑出动画（1 -> 0）
                var fadeOut = new DoubleAnimation(1d, 0d, new Duration(TimeSpan.FromMilliseconds(halfMs)))
                {
                    FillBehavior = FillBehavior.Stop
                };

                fadeOut.Completed += (s2, e2) =>
                {
                    try
                    {
                        overlay.Close();
                    }
                    catch { }
                };

                overlay.BeginAnimation(Window.OpacityProperty, fadeOut);
            };

            overlay.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
    }
}
