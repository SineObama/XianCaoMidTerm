using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MidTermProject
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static readonly bool NDEBUG = false;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            ViewModels.ItemViewModel.instance.initDB();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public static void messageAsync(string s)
        {
            var i = new Windows.UI.Popups.MessageDialog(s ?? "NullReferenceError: the message is null").ShowAsync();
        }

        public static async System.Threading.Tasks.Task<bool> message(string s)
        {
            var i = await new Windows.UI.Popups.MessageDialog(s ?? "NullReferenceError: the message is null").ShowAsync();
            return true;
        }

        static string lastMessage = "";
        public static void debugMessage(string s)
        {
            if (NDEBUG)
                return;
            if (lastMessage != s)
            {
                lastMessage = s;
                var i = new Windows.UI.Popups.MessageDialog(s ?? "NullReferenceError: the message is null").ShowAsync();
            }
        }

        public static void updateTile(string title, string description)
        {
            XmlDocument d = new XmlDocument();
            d.LoadXml(File.ReadAllText("tile.xml", System.Text.Encoding.UTF8));
            XmlNodeList list = d.GetElementsByTagName("text");
            for (int i = 0; i < list.Length; i++)
                if (i % 2 == 0)
                    list[i].InnerText = "第" + title + "学期";
                else
                    list[i].InnerText = description + "学年度";
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(d));
        }

        public static async Task<BitmapImage> setBGI()
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("bgimg");
                try
                {
                    using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        bitmapImage.SetSource(fileStream);
                    }
                }
                catch { throw; }
            }
            catch (Exception e) { App.debugMessage(e.Message); }
            return bitmapImage;
        }

        public static async Task<BitmapImage> setBGI(Windows.Storage.StorageFile file)
        {
            await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, "bgimg", NameCollisionOption.ReplaceExisting);
            return await setBGI();
        }
    }
}
