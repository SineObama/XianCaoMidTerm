using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MidTermProject.Models
{
    /// <summary>
    /// 主要用于输出调试
    /// </summary>
    class MessageBox
    {
        public static readonly bool NDEBUG = true;

        /// <summary>
        /// 异步弹框
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="title">标题，可忽略</param>
        /// <returns>没有含义</returns>
        public static async Task<bool> Async(string content, string title = "")
        {
            await new MessageDialog(content ?? "", title ?? "").ShowAsync();
            return true;
        }

        public static async Task<bool> debugAsync(string content, string title = "")
        {
            bool unused = true;
            if (!NDEBUG)
                unused = await Async(content, title);
            return unused;
        }
    }
}
