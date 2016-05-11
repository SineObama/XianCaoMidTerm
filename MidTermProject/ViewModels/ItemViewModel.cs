using System;
using System.Linq;
using MidTermProject.Models;
using SQLitePCL;

namespace MidTermProject.ViewModels
{
    class ItemViewModel
    {
        // 单例模式
        private static ItemViewModel _instance;
        public static ItemViewModel instance { get { return _instance ?? new ItemViewModel(); } }
        private ItemViewModel()
        {
            _instance = this;
            try
            {
                initDB();
            }
            catch (Exception ex) { throw new InternalError("From ItemViewModel.initDB(): " + ex.Message); }
        }

        SQLiteConnection conn;
        // 数据库连接与数据初始化。
        void initDB()
        {
            if (conn != null)
                return;
            conn = new SQLiteConnection("MidTermProject_test0.db");

            using (var statement = conn.Prepare("BEGIN TRANSACTION")) { statement.Step(); }

            // 创建table
            using (var statement = conn.Prepare(createTable)) { statement.Step(); }
            using (var statement = conn.Prepare(createHtmlTable)) { statement.Step(); }

            // 读取样例文件进行测试
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Network//SampleTable.html"));
            // fixme 下面2种读取方式都报同一个错：+		$exception	{"文件名、目录名或卷标语法不正确。 (Exception from HRESULT: 0x8007007B)":null}	System.IO.FileNotFoundException
            // 方法1
            //IRandomAccessStream t = await file.OpenAsync( FileAccessMode.Read);
            //StreamReader reader = new StreamReader(t.AsStream());
            //string html = reader.ReadToEnd();
            // 方法2
            //await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            // 读取数据库
            Item[,] item = new Item[WeekModel.maxNum, DayModel.maxNum];
            using (var statement = conn.Prepare("SELECT * FROM Html"))
            {
                if (statement.Step() != SQLiteResult.DONE)
                {
                    tableHtml = (string)statement["content"];
                }
            }
            using (var statement = conn.Prepare("SELECT * FROM Item"))
            {
                // todo 异常处理
                while (statement.Step() != SQLiteResult.DONE)
                {
                    long day = (long)statement["day"];
                    long index = (long)statement["sub"];
                    Item i = Item.createEmpty();
                    i.day = (long)statement["day"];
                    i.index = (long)statement["sub"];
                    i.last = (long)statement["last"];
                    i.className = (string)statement["className"];
                    i.classroom = (string)statement["classroom"];
                    i.section = (string)statement["section"];
                    i.week = (string)statement["week"];
                    i.note = (string)statement["note"];
                    item[day, index] = i;
                }
            }

            updateWithArray(item);

            using (var statement = conn.Prepare("COMMIT TRANSACTION")) { statement.Step(); }
        }

        void updateWithArray(Item[,] item)
        {
            _week = new Table();
            // 添加第一列：第一节~第十五节
            TableColumn column = new TableColumn();
            column.row.Add(new TableRow(title));
            for (int i = 0; i < DayModel.maxNum; i++)
                column.row.Add(new TableRow(jie[i]));
            _week.column.Add(column);

            for (int day = 0; day < WeekModel.maxNum; day++)
            {
                column = new TableColumn();
                column.row.Add(new TableRow(dayName[day]));
                for (int section = 0; section < DayModel.maxNum && item[day, section] != null; section++)
                {
                    column.row.Add(new TableRow(item[day, section].getString(), item[day, section].last));
                }
                _week.column.Add(column);
            }

            _day = _week.column[_showDay];
        }

        public void updateWithHtml(string html)
        {
            tableHtml = html;
            weekModel = WeekModel.createWithHtml(tableHtml);

            _week = new Table();
            // 添加第一列：第一节~第十五节
            TableColumn column = new TableColumn();
            column.row.Add(new TableRow(title));
            for (int i = 0; i < DayModel.maxNum; i++)
                column.row.Add(new TableRow(jie[i]));
            _week.column.Add(column);

            for (int day = 0; day < WeekModel.maxNum; day++)
            {
                column = new TableColumn();
                column.row.Add(new TableRow(dayName[day]));
                Item[] lesson = weekModel.allDayModel.ElementAt(day).allItems.ToArray();
                for (int section = 0; section < lesson.Length; section++)
                {
                    column.row.Add(new TableRow(lesson[section].getString(), lesson[section].last));
                }
                _week.column.Add(column);
            }

            _day = _week.column[_showDay];

            save();
        }

        void save()
        {
            using (var statement = conn.Prepare("BEGIN TRANSACTION")) { statement.Step(); }

            using (var statement = conn.Prepare("drop table Html")) { statement.Step(); }
            using (var statement = conn.Prepare(createHtmlTable)) { statement.Step(); }
            using (var custstmt = conn.Prepare("INSERT INTO Html (content) VALUES (?)"))
            {
                custstmt.Bind(1, tableHtml);
                custstmt.Step();
            }

            using (var statement = conn.Prepare("drop table Item")) { statement.Step(); }
            using (var statement = conn.Prepare(createTable)) { statement.Step(); }

            var c = weekModel.allDayModel.ToArray();
            for (int i = 0; i < WeekModel.maxNum; i++)
            {
                var r = c[i].allItems.ToArray();
                for (int j = 0; j < r.Length; j++)
                {
                    Item tr = r[j];
                    using (var custstmt = conn.Prepare("INSERT INTO Item (day, sub, last, className, classroom, section, week, note) VALUES (?, ?, ?, ?, ?, ?, ?, ?)"))
                    {
                        custstmt.Bind(1, tr.day);
                        custstmt.Bind(2, tr.index);
                        custstmt.Bind(3, tr.last);
                        custstmt.Bind(4, tr.className);
                        custstmt.Bind(5, tr.classroom);
                        custstmt.Bind(6, tr.section);
                        custstmt.Bind(7, tr.week);
                        custstmt.Bind(8, tr.note);
                        custstmt.Step();
                    }
                }
            }

            using (var statement = conn.Prepare("COMMIT TRANSACTION")) { statement.Step(); }
        }

        WeekModel weekModel;

        Table _week;
        public Table week { get { return _week; } }
        // 只是上面的引用
        TableColumn _day;
        public TableColumn day { get { return _day; } }

        // 窄屏时显示一天课程。提供往前和往后两种调整
        int _showDay = 1;
        public void showNextDay()
        {
            _showDay %= 7;
            _showDay++;
            _day = _week.column[_showDay];
        }
        public void showPreviousDay()
        {
            _showDay--;
            if (_showDay == 0)
                _showDay = 7;
            _day = _week.column[_showDay];
        }

        public string tableHtml = "";

        string createHtmlTable = @"CREATE TABLE IF NOT EXISTS
                                         Html(content VARCHAR( 100000 ));";
        // todo day,index共同作为主码
        string createTable = @"CREATE TABLE IF NOT EXISTS
                                         Item  (day          INTEGER,
                                                sub          INTEGER,
                                                last         INTEGER,
                                                className    VARCHAR( 30 ),
                                                classroom    VARCHAR( 30 ),
                                                section      VARCHAR( 10 ),
                                                week         VARCHAR( 10 ),
                                                note         VARCHAR( 255 )
                                                );";

        string title = "课程表";
        string[] jie = {
"第一节",
"第二节",
"第三节",
"第四节",
"第五节",
"第六节",
"第七节",
"第八节",
"第九节",
"第十节",
"第十一节",
"第十二节",
"第十三节",
"第十四节",
"第十五节",
};
        string[] dayName = {
"周一",
"周二",
"周三",
"周四",
"周五",
"周六",
"周日",
 };

        string sample = @"<html>
<head>
    <style type='text/css'>
        #subprinttable1 {
            border: 1px solid #d3dffa;
            border-collapse: collapse;
            height: 100%;
            font-size: 12px;
        }

            #subprinttable1 td {
                border: 1px solid #d3dffa;
                text-align: center;
            }

                #subprinttable1 td.tab_1 {
                    background-color: #b9cbfa;
                    font: 宋体;
                }
    </style>
</head>
<body>
    <table id='subprinttable1'>
        <tr class='tab_3'>
            <td>&nbsp;</td>
            <td>星期一</td>
            <td>星期二</td>
            <td>星期三</td>
            <td>星期四</td>
            <td>星期五</td>
            <td>星期六</td>
            <td>星期日</td>
        </tr>
        <tr><td width='180' class='tab_3'>第1节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第2节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr>
            <td width='180' class='tab_3'>第3节</td>
            <td class='tab_1' rowspan=4 width='200'>毛泽东思想和中国特色社会主义理论体系概论<br>东B403<br>3-6节<br>(1-18周)</td>
            <td width='200'>&nbsp;</td>
            <td class='tab_1' rowspan=3 width='200'>数据库系统<br>东D202<br>3-5节<br>(1-18周)</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
        </tr>
        <tr>
            <td width='180' class='tab_3'>第4节</td>
            <td class='tab_1' rowspan=2 width='200'>商务英语（大学英语）<br>东E503<br>4-5节<br>(1-17周)</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
            <td width='200'>&nbsp;</td>
        </tr>
        <tr><td width='180' class='tab_3'>第5节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第6节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第7节</td><td width='200'>&nbsp;</td><td class='tab_1' rowspan=3 width='200'>计算机组成原理与接口技术<br>东C203<br>7-9节<br>(1-18周)</td><td width='200'>&nbsp;</td><td class='tab_1' rowspan=3 width='200'>操作系统<br>东D301<br>7-9节<br>(1-18周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第8节</td><td class='tab_1' rowspan=3 width='200'>计算机组成原理与接口技术实验<br>实验中心C102<br>8-10节<br>(5-17周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第9节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第10节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td class='tab_1' rowspan=2 width='200'>体育<br> <br>10-11节<br>(1-18周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第11节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第12节</td><td class='tab_1' rowspan=4 width='200'>3D游戏编程与设计<br>实验中心B402<br>12-15节<br>(1-14周)</td><td class='tab_1' rowspan=2 width='200'>现代操作系统应用开发<br>实验中心B203<br>12-13节<br>(1-18周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第13节</td><td class='tab_1' rowspan=3 width='200'>大数据技术与MATLAB设计<br>实验中心B201<br>13-15节<br>(2-12周)</td><td class='tab_1' rowspan=3 width='200'>应用超级计算(核心通识)<br>东B103<br>13-15节<br>(2-14周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第14节</td><td class='tab_1' rowspan=2 width='200'>现代操作系统应用开发课程实验<br>实验中心B203<br>14-15节<br>(1-18周)</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
        <tr><td width='180' class='tab_3'>第15节</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td><td width='200'>&nbsp;</td></tr>
    </table>
</body>
</html>";
    }
}
