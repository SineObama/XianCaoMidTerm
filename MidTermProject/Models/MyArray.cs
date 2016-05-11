using System;

namespace MidTermProject.Models
{
    class MyArray<Entry>
    {
        private int _length;
        public int length { get { return _length; } }

        private int _capacity;
        public int capacity { get { return _capacity; } }

        private Entry[] data;

        public MyArray()
        {
            _length = 0;
            _capacity = 2;
            data = new Entry[_capacity];
        }

        public Entry elementAt(int index)
        {
            if (index < 0||index >= _length)
                throw new IndexOutOfRangeException();
            return data[index];
        }

        public void add(Entry i)
        {
            if (_capacity == _length)
                reserve(_capacity * 2);
            data[_length++] = i;
        }

        public void reserve(int i)
        {
            if (i > _capacity)
            {
                Entry[] tem = new Entry[i];
                data.CopyTo(tem, 0);
                data = tem;
                _capacity = i;
            }
        }

        class MyArrayException : Exception
        {
            public MyArrayException(string s = "") : base(s) { }
        }

        class Underflow : MyArrayException
        {
            public Underflow(string s = "") : base("underflow: " + s) { }
        }

        class Overflow : MyArrayException
        {
            public Overflow(string s = "") : base("overflow: " + s) { }
        }
    }
}
