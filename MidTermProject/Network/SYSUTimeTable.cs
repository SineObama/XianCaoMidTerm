using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MidTermProject.Network
{
    /// <summary>
    /// 提供从中大教务网获取课程表的接口
    /// </summary>
    class SYSUTimeTable
    {
        
        static readonly Uri _cookieUri = new Uri("http://uems.sysu.edu.cn/jwxt/");

        static string _JSESSIONID = null;
        static string _rno = null;
        static bool _signed = false;

        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <returns></returns>
        public static async Task<Stream> getImg()
        {
            if (_JSESSIONID == null || _rno == null)
            {  // get教务网站首页，获取cookie和rno
                HttpWebRequest ckRequest = createRequestWithCookie("http://uems.sysu.edu.cn/jwxt/", "GET");
                HttpWebResponse ckResponse = (HttpWebResponse)await ckRequest.GetResponseAsync();
                refreshJSESSIONID(ckResponse);
                refreshRno(ckResponse);
            }

            // 通过验证码的Uri直接获取图片（而不是从整个网页中获取
            HttpWebRequest imgRequest = (HttpWebRequest)HttpWebRequest.Create("http://uems.sysu.edu.cn/jwxt/jcaptcha");
            HttpWebResponse imgResponse = (HttpWebResponse)await imgRequest.GetResponseAsync();
            return imgResponse.GetResponseStream();
        }

        public static async Task<BitmapImage> StreamToBitmapImage(Stream imgStream)
        {
            var memStream = new MemoryStream();
            await imgStream.CopyToAsync(memStream);
            memStream.Position = 0;
            BitmapImage sourcebm = new BitmapImage();
            await sourcebm.SetSourceAsync(memStream.AsRandomAccessStream());
            return sourcebm;
        }

        /// <summary>
        /// 获取课程表（返回Html格式）。学年度和学期非法将返回空的课程表
        /// </summary>
        /// <param name="sid">学号</param>
        /// <param name="pwd">密码</param>
        /// <param name="captcha">验证码</param>
        /// <param name="xnd">学年度。格式："2014-2015","2015-2016"等</param>
        /// <param name="xq">学期。格式："1","2","3"</param>
        /// <returns>Excel（Html格式）</returns>
        public static async Task<string> getTable(string sid, string pwd, string captcha, string xnd, string xq)
        {
            string error = await signin(sid, pwd, captcha);
            if (!_signed)
                throw new SYSUTimeTableException(error);
            string table = await getExcel(xnd, xq);
            signout();
            return table;
        }

        static async Task<string> signin(string sid, string pwd, string captcha)
        {
            HttpWebRequest request = createRequestWithCookie("http://uems.sysu.edu.cn/jwxt/j_unieap_security_check.do", "POST");
            // 写入post主体
            request.ContentType = "application/x-www-form-urlencoded";
            string body = String.Concat("jcaptcha_response=", captcha, "&rno=", _rno, "&j_username=", sid, "&j_password=", await SYSUEncryptSupporter.encrypt(pwd));
            await writeRequestBody(request, body);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            refreshJSESSIONID(response);

            // 获取登陆的反馈信息（通过失败时返回页面唯一的<span>获取信息，成功时没有<span>） todo 加强登陆成功的判断
            string s = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string[] spliter = { "span" };
            string[] span = s.Split(spliter, StringSplitOptions.None);
            if (span.Length == 1)
            {  // 找不到span
                _signed = true;
                return null;
            }
            refreshRno(response);
            string[] tem = span[1].Split('>');
            if (tem.Length > 1)
                return tem[1].Split('<')[0];
            throw new SYSUTimeTableException("内部错误：不能识别教务网页中的反馈信息");
        }

        static async Task<string> getExcel(string xnd, string xq)
        {
            HttpWebRequest request = createRequestWithCookie("http://uems.sysu.edu.cn/jwxt/ExportToExcel.do?method=exportExcel&xq=" + xq + "&xnd=" + xnd, "GET");
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            // todo 加强成功判断与错误处理
            // "standerd" ContentType:application/vnd.ms-excel; charset=UTF-8
            string[] spliter = { "excel" };
            string[] result = response.ContentType.Split(spliter, StringSplitOptions.None);
            if (result.Length < 2)
                throw new SYSUTimeTableException("内部错误：获取Excel时类型不符");
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        static void signout()
        {
            // todo 实现真正的退出，保证安全
            //HttpWebRequest request = createRequestWithCookie("http://uems.sysu.edu.cn/jwxt/logout.jsp", "GET");
            //HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();  // 这个会出错System.ArgumentNullException不知道为什么
            _JSESSIONID = null;
            _rno = null;
            _signed = false;
        }

        // helpers

        static HttpWebRequest createRequestWithCookie(string uri, string method)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            if (request.CookieContainer == null)  // 好坑，没有这个就不会返回cookie
                request.CookieContainer = new CookieContainer();
            if (_JSESSIONID != null)
                request.CookieContainer.SetCookies(_cookieUri, "JSESSIONID=" + _JSESSIONID);
            return request;
        }

        static async Task<bool> writeRequestBody(HttpWebRequest req, string body)
        {
            Encoding encoding = Encoding.GetEncoding("ascii");
            byte[] data = encoding.GetBytes(body);
            Stream requestStream = await req.GetRequestStreamAsync();
            requestStream.Write(data, 0, data.Length);
            return true;
        }

        static void refreshJSESSIONID(HttpWebResponse res)
        {
            Cookie cookie = res.Cookies["JSESSIONID"];
            if (cookie != null)
                _JSESSIONID = cookie.Value;
            else if (_JSESSIONID == null)  // 调试发现获得JSESSIONID后再get首页不会返回cookie，不管他
                App.debugMessage("cannot get responsed cookie");
        }

        static void refreshRno(HttpWebResponse res)
        {
            string s = new StreamReader(res.GetResponseStream()).ReadToEnd();
            string[] spliter = { "rno" };
            string[] result = s.Split(spliter, StringSplitOptions.None);
            if (result.Length > 2)
            {
                string[] tem = result[2].Split('>')[0].Split('=');
                if (tem.Length > 1)
                    _rno = tem[1];
            }
            else
                App.debugMessage("cannot get rno");
        }

        class SYSUTimeTableException : Exception
        {
            public SYSUTimeTableException(string msg) : base(msg) { }
        }

    }  // End class SYSUTimeTable

    /// <summary>
    /// 通过把（含有教务系统加密脚本md5.js的）HTML读入WebView来调用其中的js加密函数（加密后的密码才会被教务系统接受
    /// </summary>
    class SYSUEncryptSupporter
    {

        static readonly Uri _html = new Uri("ms-appx-web:///Network//SYSUEncrypt.html");

        static WebView _w;
        static bool _loadCompleted = false;

        // 页面初始化时必须调用一次
        public static void init()
        {
            if (_w != null)
                return;
            _w = new WebView();
            _w.Source = _html;
            _w.LoadCompleted += (unused0, unused1) => { _loadCompleted = true; };
        }

        public static async Task<string> encrypt(string s)
        {
            if (!_loadCompleted)
                throw new LoadUncompleted("WebView does not complete loading.");
            string[] tem = { s };
            string result = await _w.InvokeScriptAsync("sysu_encrypt", tem);
            return result;
        }

        public class LoadUncompleted : Exception
        {
            public LoadUncompleted(string message) : base(message) { }
        }

    }  // End class SYSUEncryptSupporter

}
