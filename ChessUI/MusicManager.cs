using System;
using System.Collections.Generic;  // 新增：用于 Dictionary
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace ChessUI
{
    public static class MusicManager
    {
        private static readonly MediaPlayer player = new MediaPlayer();
        // 嵌入资源名（按你的默认命名空间 + 路径）
        //private const string ResourceName = "ChessUI.Assets.menu.mp3";
        private const string ResourceName = "ChessUI.Assets.menu1.mp3";
        // 临时文件路径（解包后的 mp3 放这里）
        //private static readonly string TempFilePath =
        // Path.Combine(Path.GetTempPath(), "ChessUI_menu.mp3");
        private static readonly string TempFilePath =
            Path.Combine(Path.GetTempPath(), "ChessUI_menu1.mp3");
        private static bool extracted;
        private static bool isPlaying;
        // 默认音量（0.0 - 静音, 1.0 - 最大）
        private static double currentVolume = 1.0; // <--- 你可以改这里

        // 新增：音效资源字典（key: type, value: 嵌入资源名）
        private static readonly Dictionary<string, string> soundResources = new()
        {
            { "check", "ChessUI.Assets.check.mp3" },
            { "capture", "ChessUI.Assets.capture.mp3" },
            { "move", "ChessUI.Assets.move.mp3" },
            { "castle", "ChessUI.Assets.castle.mp3" },
            { "promote", "ChessUI.Assets.promote.mp3" }
        };

        // 新增：音效临时路径字典
        private static readonly Dictionary<string, string> tempSoundPaths = new()
        {
            { "check", Path.Combine(Path.GetTempPath(), "ChessUI_check.mp3") },
            { "capture", Path.Combine(Path.GetTempPath(), "ChessUI_capture.mp3") },
            { "move", Path.Combine(Path.GetTempPath(), "ChessUI_move.mp3") },
            { "castle", Path.Combine(Path.GetTempPath(), "ChessUI_castle.mp3") },
            { "promote", Path.Combine(Path.GetTempPath(), "ChessUI_promote.mp3") }
        };

        static MusicManager()
        {
            // 预解包一次
            TryExtractIfNeeded();
            // 初始设置音量（即使还未播放也先设好）
            player.Volume = ClampVolume(currentVolume);
        }

        private static double ClampVolume(double v)
        {
            if (double.IsNaN(v)) return 0.0;
            if (v < 0.0) return 0.0;
            if (v > 1.0) return 1.0;
            return v;
        }

        /// <summary>
        /// 从嵌入资源解出 mp3 到临时文件（只要不存在/为空就重解）
        /// </summary>
        private static void TryExtractIfNeeded()
        {
            try
            {
                if (File.Exists(TempFilePath))
                {
                    var info = new FileInfo(TempFilePath);
                    if (info.Length > 0)
                    {
                        extracted = true;
                        return;
                    }
                }
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(ResourceName);
                if (stream == null)
                {
                    Console.WriteLine("[MusicManager] 未找到嵌入资源：" + ResourceName);
                    extracted = false;
                    return;
                }
                using var fs = new FileStream(TempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);
                extracted = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MusicManager] 解包失败：" + ex.Message);
                extracted = false;
            }
        }

        // 新增：泛化提取方法：从嵌入资源解出音效到临时文件
        private static bool TryExtractSound(string resName, string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    var info = new FileInfo(tempPath);
                    if (info.Length > 0)
                    {
                        return true;
                    }
                }
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName);
                if (stream == null)
                {
                    Console.WriteLine($"[MusicManager] 未找到嵌入资源：{resName}");
                    return false;
                }
                using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);
                Console.WriteLine($"[Debug] 音效提取成功到: {tempPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MusicManager] 音效解包失败：{ex.Message}");
                return false;
            }
        }

        private static void OnMediaEnded(object? s, EventArgs e)
        {
            try
            {
                player.Position = TimeSpan.Zero;
                player.Play();
            }
            catch { }
        }

        /// <summary>
        /// 主菜单音乐：每次调用都从头开始播放，循环。
        /// </summary>
        public static void PlayMenuMusic()
        {
            // 每次都强制从头开始
            Stop();
            if (!extracted) TryExtractIfNeeded();
            if (!extracted)
            {
                Console.WriteLine("[MusicManager] 无法播放：资源未解包成功。");
                return;
            }
            try
            {
                // 避免多次叠加事件
                player.MediaEnded -= OnMediaEnded;
                player.Open(new Uri(TempFilePath, UriKind.Absolute));
                player.MediaEnded += OnMediaEnded;
                // 把音量（可能被外部调整过）应用到 player
                player.Volume = ClampVolume(currentVolume);
                player.Position = TimeSpan.Zero;
                player.Play();
                isPlaying = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MusicManager] 播放失败：" + ex.Message);
                isPlaying = false;
            }
        }

        /// <summary>
        /// 停止播放（对外常用名）
        /// </summary>
        public static void Stop()
        {
            try
            {
                player.MediaEnded -= OnMediaEnded;
                player.Stop();
            }
            catch { }
            isPlaying = false;
        }

        /// <summary>
        /// 兼容旧名（别处如果用的是 StopMusic 也能工作）
        /// </summary>
        public static void StopMusic() => Stop();

        /// <summary>
        /// 设置音量（0.0 - 1.0）。立即生效（若正在播放）。
        /// </summary>
        public static void SetVolume(double volume)
        {
            currentVolume = ClampVolume(volume);
            try
            {
                player.Volume = currentVolume;
            }
            catch { }
        }

        /// <summary>
        /// 获取当前音量值
        /// </summary>
        public static double GetVolume()
        {
            return currentVolume;
        }

        // 新增：播放指定类型音效（不循环，使用独立 player）
        public static void PlaySound(string type)
        {
            if (!soundResources.TryGetValue(type, out string resName)) return;
            string tempPath = tempSoundPaths[type];

            // 提取如果需要
            if (!TryExtractSound(resName, tempPath)) return;

            try
            {
                var soundPlayer = new MediaPlayer();  // 新实例，避免干扰背景音乐
                soundPlayer.Open(new Uri(tempPath, UriKind.Absolute));
                soundPlayer.Volume = ClampVolume(currentVolume);  // 使用当前音量
                soundPlayer.Play();
                Console.WriteLine($"[Debug] 播放音效: {type}, 音量: {soundPlayer.Volume}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MusicManager] 播放音效失败 ({type}): {ex.Message}");
            }
        }
    }
}