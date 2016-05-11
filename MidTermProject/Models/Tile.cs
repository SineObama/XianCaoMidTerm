using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MidTermProject.Models
{
    class Tile
    {
        public static void update(string title, string description)
        {
            XmlDocument d = new XmlDocument();
            d.LoadXml(System.IO.File.ReadAllText("tile.xml", Encoding.UTF8));
            XmlNodeList list = d.GetElementsByTagName("text");
            for (int i = 0; i < list.Length; i++)
                if (i % 2 == 0)
                    list[i].InnerText = "第" + title + "学期";
                else
                    list[i].InnerText = description + "学年度";
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(d));
        }
    }
}
