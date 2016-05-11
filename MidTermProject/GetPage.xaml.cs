using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MidTermProject.Network;
using MidTermProject.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MidTermProject
{
    public sealed partial class GetPage : Page
    {
        public GetPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            bgimg.ImageSource = await Models.BackgroundImage.getAsync();
            getImg();
        }

        private async void get_Click(object sender, RoutedEventArgs e)
        {
            string tableHtml = "";
            try
            {
                int year = 0;
                if (!Int32.TryParse(xn.Text, out year))
                    throw new Exception("学年数非法");
                tableHtml = await SYSUTimeTable.getTable(sid.Text, pwd.Password, captcha.Text, year + "-" + (year + 1), xq.Text);
                var unused = Models.MessageBox.Async("获取成功！");
            }
            catch (Exception ex)
            {
                var unused = Models.MessageBox.Async(ex.Message);
                getImg();
                return;
            }
            ItemViewModel.instance.updateWithHtml(tableHtml);
            Models.Tile.update(xq.Text, xn.Text);
            Frame.Navigate(typeof(MainPage), "");
        }

        async void getImg()
        {
            try { img.Source = await SYSUTimeTable.StreamToBitmapImage(await SYSUTimeTable.getImg()); }
            catch (Exception ex) { var unused = Models.MessageBox.Async(ex.Message); }
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
