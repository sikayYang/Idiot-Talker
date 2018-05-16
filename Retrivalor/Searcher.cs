using System.Collections.Generic;
using Lucene.Net.Analysis.Standard;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;
using System.IO;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace IdiotTalk
{
    /// <summary>
    /// 用于检索的类
    /// </summary>
    public class Searcher
    {
        IndexSearcher indexSearch;
        QueryParser queryParser;

        /// <summary>
        /// 构造函数接收索引存放的目录路径
        /// </summary>
        /// <param name="indexDirectoryPath"></param>
        public Searcher(string indexDirectoryPath)
        {
            indexSearch = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexDirectoryPath)));
            queryParser = new QueryParser(Version.LUCENE_30, Params.CONTENTS, new StandardAnalyzer(Version.LUCENE_30));
        }

        /// <summary>
        /// 检索函数接收要检索的关键字
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        public List<string> Search(IEnumerable<string> searchQuery)
        {
            BooleanQuery boolQuery = new BooleanQuery();
            List<string> files = new List<string>();
            foreach (string searchStr in searchQuery)
                boolQuery.Add(queryParser.Parse(searchStr),Occur.MUST);

            TopDocs topdocs = indexSearch.Search(boolQuery, Params.MAX_SEARCH_TIME);
            foreach (var doc in topdocs.ScoreDocs)
            {
                float score=doc.Score;
                string str = indexSearch.Doc(doc.Doc).GetField(Params.FILE_PATH).ToString();
                files.Add(str.Substring(str.IndexOf(':') + 1).TrimEnd('>'));
            }
            return files;
        }

    }
}
