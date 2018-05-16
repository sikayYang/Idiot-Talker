using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdiotTalk
{
    /// <summary>
    /// http请求时的参数
    /// </summary>
    public class HttpParams
    {
        /// <summary>
        /// API的地址
        /// </summary>
        public const string HttpUrl = "https://api.ltp-cloud.com/analysis";

        /// <summary>
        /// 用户密码
        /// </summary>
        public const string UserKey = "h107a4x0m9sxTslLUusZfbfczOjAnCCEdqOcVHAB";

        /// <summary>
        /// 指定分析后结果的返回格式
        /// </summary>
        public class Format
        {
            /// <summary>
            /// xml格式返回结果
            /// </summary>
            public const string XML = "xml";

            /// <summary>
            /// json格式返回结果
            /// </summary>
            public const string JSON = "json";

            /// <summary>
            /// conll格式返回结果
            /// </summary>
            public const string CONLL = "conll";

            /// <summary>
            /// 简洁文本格式返回
            /// </summary>
            public const string PLAIN = "plain";
        }

        /// <summary>
        /// 用以指定分析的格式
        /// </summary>
        public class Pattern
        {
            /// <summary>
            /// 分词
            /// </summary>
            public const string WS = "ws";

            /// <summary>
            /// 
            /// 词性标注
            /// </summary>
            public const string POS = "pos";

            /// <summary>
            /// 命名实体识别
            /// </summary>
            public const string NER = "ner";

            /// <summary>
            /// 
            /// 依存句法分析
            /// </summary>
            public const string DP = "dp";

            /// <summary>
            /// 语义依存分析
            /// </summary>
            public const string SDP = "sdp";


            /// <summary>
            /// 语义角色标注
            /// </summary>
            public const string SRL = "srl";

            /// <summary>
            /// 所有
            /// </summary>
            public const string ALL = "all";
        }
    }
}
