using System;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MidTermProject.Models
{
    struct TileData
    {
        public string dayName;
        public int numOfLesson;
        public MyArray<Item> lesson;
    }

    /// <summary>
    /// 根据调用函数的时间更新显示当天的课程数。
    /// </summary>
    class Tile
    {
        static DateTime time;

        public static void forceUpdate()
        {
            refresh();
        }

        public static void update()
        {
            if (time != null && time == DateTime.Today)
                return;
            refresh();
        }

        static void refresh()
        {
            time = DateTime.Today;
            string title, description;
            TileData td = ViewModels.ItemViewModel.instance.getDateForTile(time.DayOfWeek);
            title = td.dayName + " " + td.numOfLesson + "门课程";
            description = "";
            if (td.numOfLesson != 0)
                description += td.lesson.elementAt(0).className;
            for (int i = 1; i < td.numOfLesson; i++)
            {
                if (i >= 3)
                {
                    description += " ...";
                    break;
                }
                description += "\n" + td.lesson.elementAt(i).className;
            }
            XmlDocument d = new XmlDocument();
            d.LoadXml(System.IO.File.ReadAllText("tile.xml", Encoding.UTF8));
            XmlNodeList list = d.GetElementsByTagName("text");
            list[0].InnerText = td.dayName;
            list[2].InnerText = td.dayName;
            if (td.numOfLesson != 0)
            {
                list[1].InnerText = td.numOfLesson + "门课";
                list[3].InnerText = "今天有" + td.numOfLesson + "门课";
            }
            else
            {
                list[1].InnerText = "没有课";
                list[3].InnerText = "今天有居然没有课";
            }
            list[4].InnerText = title;
            list[5].InnerText = description;
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(d));
        }
    }
}
