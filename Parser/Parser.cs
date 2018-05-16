using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IdiotTalk
{
    public class Parser
    {
        #region 字段
        private string _httpUrl = HttpParams.HttpUrl;
        private string _userKey = HttpParams.UserKey;
        private string _pattern = HttpParams.Pattern.ALL;
        private string _format = HttpParams.Format.XML;
        #endregion

        #region  属性
        /// <summary>
        /// API所在地址
        /// </summary>
        public string HttpUrl
        {
            get { return this._httpUrl; }
            set { this._httpUrl = value; }
        }

        /// <summary>
        /// 用户访问密码
        /// </summary>
        public string UserKey
        {
            get { return this._userKey; }
            set { this._userKey = value; }
        }

        /// <summary>
        /// 文本以何种模式被分析
        /// </summary>
        public string Pattern
        {
            get { return this._pattern; }
            set { this._pattern = value; }
        }

        /// <summary>
        /// 分析结果以何种格式返回
        /// </summary>
        public string Format
        {
            get { return this._format; }
            set { this._format = value; }
        }
        #endregion



        /// <summary>
        /// 对text进行语法分析
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public  string Analyze(string text)
        {
            string results = "";
           string httpParameter =string.Format("{0}?api_key={1}&text={2}&pattern={3}&format={4}",HttpUrl,UserKey, text,Pattern,Format);
            HttpWebRequest request = WebRequest.Create(httpParameter) as HttpWebRequest;
            try
            {
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader resultStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                results= resultStream.ReadToEnd();
            }
            catch(Exception e)
            {
                Console.WriteLine("Http Request Error: "+e.ToString());
            }
            return results;
        }
        
    }
}
