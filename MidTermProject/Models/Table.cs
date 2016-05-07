using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int span;
        public TableRow(string s = "", int span = 1)
        {
            this.text = s;
            this.span = span;
        }
    }
}
