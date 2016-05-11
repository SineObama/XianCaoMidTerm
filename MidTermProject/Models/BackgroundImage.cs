using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MidTermProject.Models
{
    /// <summary>
    /// 提供一张统一的背景图的接口
    /// </summary>
    class BackgroundImage
    {
        static readonly string name = "BackgroundImage";
        static ImageManager imgMng = ImageManager.instance;

        public static async Task<BitmapImage> getAsync()
        {
            return await imgMng.getAsync(name);
        }

        /// <summary>
        /// 异步设置背景图。
        /// </summary>
        /// <param name="file"></param>
        /// <returns>没有含义。方便同步执行</returns>
        public static async Task<bool> setAsync(StorageFile file)
        {
            await imgMng.setAsync(file, name);
            return true;
        }
    }
}
