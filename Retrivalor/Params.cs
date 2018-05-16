namespace IdiotTalk
{
    /// <summary>
    /// 提供用于建立索引文档的参数
    /// </summary>
    public class Params
    {
        /// <summary>
        /// 文档中文件内容字段名
        /// </summary>
        public const string CONTENTS = "CONTENTS";

        /// <summary>
        /// 文档中文件名字字段名
        /// </summary>
        public static string FILE_NAME="FILE_NAME";

        /// <summary>
        /// 文档中文件路径字段名
        /// </summary>
        public static string FILE_PATH = "FILE_PATH";

        /// <summary>
        ///同一个关键词最多搜索文档的个数
        /// </summary>
        public static int MAX_SEARCH_TIME = 10;
    }
   
  
}
