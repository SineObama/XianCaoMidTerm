using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MidTermProject.Models
{
    class Item
    {
        public int day = 0, index = 0, last = 0;
        public string className = "", classroom = "", section = "", week = "";
        public string note = "";

        private Item() { }

        public static Item createEmpty()
        {
            return new Item();
        }

        public string getString()
        {
            string result = "";
            if (className != null && className != "")
                result += className;
            if (classroom != null && classroom != "")
                result += "\n" + classroom;
            if (section != null && section != "")
                result += "\n" + section;
            if (week != null && week != "")
                result += "\n" + week;
            if (note != null && note != "")
                result += "\n" + note;
            return result;
        }
    }

    class DayModel
    {
        public static readonly int maxNum = 15;
        public ObservableCollection<Item> allItems { get; set; } = new ObservableCollection<Item>();

        private DayModel() { }

        /// <summary>
        /// 没有具体的课
        /// </summary>
        /// <returns></returns>
        public static DayModel createNull()
        {
            return new DayModel();
        }

        /// <summary>
        /// 有默认的15节空白的课
        /// </summary>
        /// <returns></returns>
        public static DayModel createEmpty()
        {
            DayModel dm = new DayModel();
            for (int i = 0; i < maxNum; i++)
                dm.allItems.Add(Item.createEmpty());
            return dm;
        }
    }

    class WeekModel
    {
        public static readonly int maxNum = 7;
        // 0 for Monday ...
        public ObservableCollection<DayModel> allDayModel { get; set; } = new ObservableCollection<DayModel>();

        private WeekModel() { }

        /// <summary>
        /// 默认7天，每天15节空白课
        /// </summary>
        /// <returns></returns>
        public static WeekModel createEmpty()
        {
            WeekModel wm = new WeekModel();
            for (int i = 0; i < maxNum; i++)
                wm.allDayModel.Add(DayModel.createEmpty());
            return wm;
        }

        /// <summary>
        /// 把从教务网获取的课程表Excel（我们获得的是Html格式）转成数据。
        /// </summary>
        /// <param name="html">以string存储的Html</param>
        /// <returns>存有对应课程表数据的模块</returns>
        public static WeekModel createWithHtml(string html)
        {
            WeekModel wm = new WeekModel();
            for (int i = 0; i < maxNum; i++)
                wm.allDayModel.Add(DayModel.createNull());

            // 获取课程表主体：table
            TagReader tableReader = new TagReader(html, "table");
            string table = tableReader.read();
            if (table == null)
                throw new NotFound("table");

            // 从每一行tr创建td的Reader
            TagReader trReader = new TagReader(table, "tr");
            trReader.read();
            TagReader[] tdReader = new TagReader[DayModel.maxNum];
            for (int section = 0; section < DayModel.maxNum; section++)
            {
                string tr = trReader.read();
                if (tr == null)
                    throw new NotFound("tr");
                tdReader[section] = new TagReader(tr, "td");
                tdReader[section].read();
            }

            // 解析主体
            int span = 0;
            for (int day = 0; day < maxNum; day++)
            {
                span = 0;
                ObservableCollection<Item> today = wm.allDayModel[day].allItems;
                for (int section = 0; section < DayModel.maxNum; section++, span--)
                {
                    if (span > 0)
                        continue;
                    Item item = Item.createEmpty();
                    item.index = today.ToArray().Length;
                    item.day = day;

                    // td为包括标签本身的文本<td >...</td>
                    string td = tdReader[section].read(true);
                    if (td.IndexOf("&nbsp;") >= 0)  // 表示课程信息为空
                    {
                        item.last = 1;
                        today.Add(item);
                        continue;
                    }

                    // 获取课程长度
                    string search = "rowspan=";
                    span = 1;
                    int index = td.IndexOf(search);
                    if (index >= 0)
                    {
                        string tem = td.Substring(index + search.Length, 1);
                        if (!Int32.TryParse(tem, out span))
                            span = 1;
                    }

                    // 更新td为标签的内容
                    index = td.IndexOf('>');
                    if (index < 0)
                        throw new NotFound("'>'");
                    td = td.Substring(index + 1);
                    index = td.IndexOf("</td>");
                    if (index < 0)
                        throw new TableTransformException("inner error: can not find </td>");
                    td = td.Substring(0, index);

                    // 以换行<br>分割课程表中4个信息
                    string[] spliter = { "<br>" };
                    string[] division = td.Split(spliter, StringSplitOptions.None);
                    if (division.Length != 4)
                        throw new TableTransformException("invaild format in <td>");
                    item.className = division[0];
                    item.classroom = division[1];
                    item.section = division[2];
                    item.week = division[3];
                    item.last = span;

                    today.Add(item);
                }
            }
            return wm;
        }

        /// <summary>
        /// 以简单逻辑读取html中的标签
        /// </summary>
        class TagReader
        {
            string src, beginS, endS;
            int endSLen;
            public TagReader(string i, string tag)
            {
                src = i;
                beginS = "<" + tag;
                endS = "</" + tag + ">";
                endSLen = endS.Length;
            }
            public string read(bool withTag = false)
            {
                if (src == null)
                    return null;
                int end = src.IndexOf(endS);
                if (end < 0)
                {
                    src = null;
                    return null;
                }
                string td = src.Substring(0, end + (withTag ? endSLen : 0));
                src = src.Substring(end + endSLen);
                int begin = td.IndexOf(beginS);
                if (begin < 0)
                    throw new UnexpectedFormat("unfound '" + beginS + "'");
                td = td.Substring(begin);
                if (withTag)
                    return td;
                int i = td.IndexOf('>');
                if (i < 0)
                    throw new UnexpectedFormat("unfound '>'");
                return td.Substring(i + 1);
            }

            class UnexpectedFormat : TableTransformException
            {
                public UnexpectedFormat(string s = "") : base("unexpected format: " + s) { }
            }
        }
    }

    class NotFound : TableTransformException
    {
        public NotFound(string s = "") : base("not found: " + s) { }
    }

    class TableTransformException : Exception
    {
        public TableTransformException(string s = "") : base(s) { }
    }
}
