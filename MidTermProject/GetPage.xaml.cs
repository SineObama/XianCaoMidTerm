using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MidTermProject.Network;
using MidTermProject.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MidTermProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetPage : Page
    {
        public GetPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            getImg();
        }

        private async void get_Click(object sender, RoutedEventArgs e)
        {
            string tableHtml;
            try
            {
                tableHtml = await SYSUTimeTable.getTable(sid.Text, pwd.Password, captcha.Text, xnd.Text, xq.Text);
                App.messageAsync("获取成功！");
               
                ItemViewModel.instance.updateWithHtml(tableHtml);
                Frame.Navigate(typeof(MainPage), "");
            }
            catch (Exception ex)
            {
                App.messageAsync(ex.Message);
                getImg();
            }
        }

        async void  getImg()
        {
            img.Source = await SYSUTimeTable.StreamToBitmapImage(await SYSUTimeTable.getImg());
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), "");
        }

        private void refreshImg_Click(object sender, RoutedEventArgs e)
        {
            getImg();
        }
    }
}
