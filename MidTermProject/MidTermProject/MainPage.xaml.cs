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
using MidTermProject.ViewModels;
using MidTermProject.Models;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MidTermProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ItemViewModel vm;

        public MainPage()
        {
            this.InitializeComponent();
            Network.SYSUEncryptSupporter.init();
            vm = ItemViewModel.instance;
            table.ItemsSource = vm.week.column;
        }

        private void get_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GetPage), "");
        }
    }

    class MyGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            // todo 错误处理
            TableRow _item = item as TableRow;
            if (_item == null)
                throw new NullReferenceException("internal error");
            if (_item.span != 0)
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, _item.span);
            //element.SetValue(VariableSizedWrapGrid.BackgroundProperty, );
            //Windows.UI.Xaml.Controls.Grid.b;
            //Brush a = Background;
            //a.SetValue(ColorProperty, Windows.UI.Colors.Blue);
            base.PrepareContainerForItemOverride(element, item);
        }

    }
}
