using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidTermProject.Models;

namespace MidTermProject.Network
{
    //class HtmlReader
    //{
    //}

    class Tag
    {
        private string _tagName = rawText;
        public string tagName { get { return _tagName; } }

        private string _innerHtml;
        public string innerHtml { get { return _innerHtml; } }

        private string _proStr { get { return _proStr; } set { _proStr = value; proTransform(); } }
        public string proStr { get { return _proStr; } }

        // 在给_proStr赋值的时候自动处理
        private Dictionary<string, string> _property = new Dictionary<string, string>();
        public Dictionary<string, string> property { get { return _property; } }

        public MyArray<Tag> child = new MyArray<Tag>();

        bool treated;  // 是否已经解析子节点

        public Tag(string inner = "", bool treat = false) { _proStr = ""; _innerHtml = inner; treated = treat; }

        // todo
        private void proTransform()
        {
            //string s = _proStr;
            //int len = s.Length;
            //// 以空格或等号分割键值对
            //MyArray<string> a = new MyArray<string>();
            //hehe he = hehe.key;
            //string t = "";
            //int index = 0;
            //bool exit = false;
            //for (; index < len; index++)
            //{
            //    char c = s[index];
            //    switch (he)
            //    {
            //        case hehe.key:
            //            if (c == '/' || c == '>')
            //            {
            //                exit = true;
            //                break;
            //            }
            //            if (c == ' ' || c == '=')
            //            {
            //                if (t != "")
            //                    a.add(t);
            //                t = "";
            //                if (c == '=')
            //                    a.add("=");

            //            }
            //            else if (c == '\'')
            //                he = hehe.string1;
            //            else if (c == '"')
            //                he = hehe.string2;
            //            else
            //                t += c;
            //            break;
            //        case hehe.string1:
            //            if (c == '\'')
            //            {
            //                a.add(t);
            //                he = hehe.key;
            //            }
            //            else
            //                t += c;
            //            break;
            //        case hehe.string2:
            //            if (c == '"')
            //            {
            //                a.add(t);
            //                he = hehe.key;
            //            }
            //            else
            //                t += c;
            //            break;
            //    }
            //    if (exit)
            //        break;
            //}
        }

        public static MyArray<Tag> build(string s)
        {
            string treated = preTreatment(s);
            return recursiveRead(treated);
        }

        // 预处理
        static string preTreatment(string s)
        {
            s = s.Replace('\n', ' ');
            string r = "";
            bool blank = false;
            int len = s.Length;
            for (int index = 0; index < len; index++)
            {
                if (s[index] == ' ')
                {
                    if (blank)
                        continue;
                    blank = true;
                }
                else
                {
                    blank = false;
                }
                r += s[index];
            }
            return r;
        }

        static MyArray<Tag> recursiveRead(string s)
        {
            MyArray<Tag> array = new MyArray<Tag>();
            while (s != null && s != "")
            {
                {  // 忽略'<'前的部分
                    int tagBegin = s.IndexOf('<');
                    if (tagBegin < 0)  // 只剩下没有标签的纯文本
                    {
                        array.add(new Tag(s, true));
                        break;
                    }
                    if (tagBegin > 0)  // 含有标签外的纯文本
                    {
                        string raw = s.Substring(0, tagBegin);
                        array.add(new Tag(raw, true));
                        s = s.Substring(tagBegin);
                    }
                }

                Tag tag = new Tag();
                s = readPro(s, tag);
                if (s[0] == '/')   // 字关闭标签
                {
                    tag.treated = true;
                    array.add(tag);
                    s = s.Substring(2);
                    continue;
                }

                //Tag tag = new Tag();
                //// 读取名字和属性
                //int contentEnd = tagEnd - (closed ? 1 : 0);
                //string content = s.Substring(0, contentEnd);  // <button click="click"
                //int tagNameEnd = content.IndexOf(' ');
                //if (tagNameEnd < 0)  // 无空格比如<div>
                //    tagNameEnd = contentEnd;
                //tag._tagName = content.Substring(1, tagNameEnd - 1);
                //tag._proStr = content.Substring(tagNameEnd, contentEnd - tagNameEnd).Trim();

                //if (closed)
                //{
                //    array.add(tag);
                //    s = s.Substring(tagEnd + 1);
                //    continue;
                //}

                //// 读取内容
            }
            return array;
        }

        /// <summary>
        /// 读取标签中的名字与属性，返回剩下的文本，包括结束标志/
        /// </summary>
        /// <param name="s">以'&lt;'开始的文本</param>
        /// <returns>以'&gt;'或'/&gt;'开始的剩下的文本</returns>
        static string readPro(string s, Tag tag)
        {
            int len = s.Length;

            // 获取标签名字
            int tagNameEnd = len;
            int index = s.IndexOf(' ');
            if (index >= 0 && index < tagNameEnd)
                tagNameEnd = index;
            index = s.IndexOf('/');
            if (index >= 0 && index < tagNameEnd)
                tagNameEnd = index;
            index = s.IndexOf('>');
            if (index >= 0 && index < tagNameEnd)
                tagNameEnd = index;

            if (tagNameEnd == len)
                throw new NotFound("tag name");

            tag._tagName = s.Substring(1, tagNameEnd - 1);
            s = s.Substring(tagNameEnd);

            // 获取标签属性
            index = 0;
            readProState state = readProState.key;
            bool exit = false;
            while (index < len)
            {
                char c = s[index];
                switch (state)
                {
                    case readProState.key:
                        if (c == '>' || c == '/')
                            exit = true;
                        else if (c == '\'')
                            state = readProState.string1;
                        else if (c == '"')
                            state = readProState.string2;
                        break;
                    case readProState.string1:
                        if (c == '\'')
                            state = readProState.key;
                        break;
                    case readProState.string2:
                        if (c == '"')
                            state = readProState.key;
                        break;
                }
                if (exit)
                    break;
                index++;
            }
            if (index >= len)
                throw new NotFound("ending >");
            tag._proStr = s.Substring(0, index).Trim();

            return s;
        }

        enum readProState { key, string1, string2 }

        public static readonly string rawText = "rawText";
    }

}
