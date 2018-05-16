using System;
using System.Text;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using Version = Lucene.Net.Util.Version;

namespace IdiotTalk
{
    /// <summary>
    /// 用于建立索引的类
    /// </summary>
    public class Indexer
    {
        /// <summary>
        /// 对dataDir下的所有文件建立索引并存储在indexDir下
        /// </summary>
        /// <param name="indexDir"></param>
        /// <param name="dataDir"></param>
        public static void Index(string indexDir, string dataDir)
        {
            if (Directory.Exists(indexDir))
            {
                Console.Out.WriteLine("Cannot save index to '" + indexDir + "' directory, please delete it first");
                Environment.Exit(1);
            }

            try
            {
                using (var writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexDir)), new StandardAnalyzer(Version.LUCENE_30), true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    Console.Out.WriteLine("Indexing to directory '" + indexDir + "'...");
                    IndexDirectory(writer, new DirectoryInfo(dataDir));
                    writer.Commit();
                }
            }
            catch (IOException e)
            {
                Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
            }
        }

        internal static void IndexDirectory(IndexWriter writer, DirectoryInfo directory)
        {
            foreach (var subDirectory in directory.GetDirectories())
                IndexDirectory(writer, subDirectory);

            foreach (var file in directory.GetFiles())
                IndexDocs(writer, file);
        }

        internal static void IndexDocs(IndexWriter writer, FileInfo file)
        {
            try
            {
                Console.Out.WriteLine("adding " + file);
                Field nameField = new Field(Params.FILE_NAME, file.Name, Field.Store.YES, Field.Index.NOT_ANALYZED);
                Field pathField = new Field(Params.FILE_PATH, file.FullName, Field.Store.YES, Field.Index.NOT_ANALYZED);
                Field contentField = new Field(Params.CONTENTS, new StreamReader(file.OpenRead(), Encoding.Default));

                Document doc = new Document();
                doc.Add(nameField);
                doc.Add(pathField);
                doc.Add(contentField);

                writer.AddDocument(doc);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: write index file failed!");
            }
        }
    }


}
