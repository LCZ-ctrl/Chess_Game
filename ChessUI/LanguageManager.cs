using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI
{
    public static class LanguageManager
    {
        // 当前全局语言
        public static LanguageType CurrentLanguage { get; private set; } = LanguageType.English;

        // 语言切换事件
        public static event Action LanguageChanged;

        // 设置语言
        public static void SetLanguage(LanguageType lang)
        {
            if (CurrentLanguage != lang)
            {
                CurrentLanguage = lang;
                LanguageChanged?.Invoke(); // 触发事件通知所有订阅者
            }
        }
    }
}


