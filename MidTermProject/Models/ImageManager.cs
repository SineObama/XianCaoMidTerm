using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MidTermProject.Models
{
    class ImageManager
    {
        private static ImageManager _instance;
        public static ImageManager instance { get { return _instance ?? new ImageManager(); } }

        private ImageManager() { _instance = this; }

        StorageFolder _imgFolder;
        async Task<StorageFolder> getFolderAsync()
        {
            if (_imgFolder == null)
                _imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("image", CreationCollisionOption.OpenIfExists);
            return _imgFolder;
        }

        /// <summary>
        /// 读取图片。图片不存在会返回空的位图。
        /// </summary>
        /// <param name="name">图片的文件名</param>
        /// <returns>位图</returns>
        public async Task<BitmapImage> getAsync(string name)
        {
            await getFolderAsync();
            BitmapImage bitmapImage = new BitmapImage();
            try
            {  // try是为了忽略FileNotFound异常
                var file = await _imgFolder.GetFileAsync(name);
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(fileStream);
                }
            }
            catch (Exception ex) { var unused = MessageBox.debugAsync(ex.Message); }
            return bitmapImage;
        }

        /// <summary>
        /// 将文件复制到本地目录，赋予新文件名。往后将作为位图读取出来。注意：自动替代同名文件
        /// </summary>
        /// <param name="file">要复制的文件</param>
        /// <param name="name">要保存的文件名</param>
        /// <returns>保存之后的文件</returns>
        public async Task<StorageFile> setAsync(StorageFile file, string name)
        {
            await getFolderAsync();
            return await file.CopyAsync(_imgFolder, name, NameCollisionOption.ReplaceExisting);
        }
    }
}
