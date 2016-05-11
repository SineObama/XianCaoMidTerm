using System.Collections.ObjectModel;

/// <summary>
/// 直接用于数据绑定的模块
/// </summary>

namespace MidTermProject.Models
{
    class Table
    {
        public ObservableCollection<TableColumn> column = new ObservableCollection<TableColumn>();
    }

    class TableColumn
    {
        public ObservableCollection<TableRow> row = new ObservableCollection<TableRow>();
    }

    class TableRow
    {
        public string text;
        public long span;
        public TableRow(string s = "", long span = 1)
        {
            this.text = s;
            this.span = span;
        }
    }
}
