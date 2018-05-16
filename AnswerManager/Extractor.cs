using IdiotTalk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace IdiotTalk
{
    class Extractor
    {
        
        private Extractor()
        {

        }

        /// <summary>
        ///从指定目录的所有文件中获取含有指定模板的句子
        /// </summary>
        /// <param name="dir">目录名</param>
        /// <param name="pattern">模板</param>
        /// <returns></returns>
        static public List<string> ExtractSentence(DirectoryInfo dir,string pattern)
        {
            List<string> targetSents = new List<string>();
            foreach (var dirtory in dir.GetDirectories())
                targetSents.AddRange(ExtractSentence(dirtory,pattern));
            foreach (var file in dir.GetFiles())
                targetSents.AddRange(ExtractSentence(file, pattern));
            return targetSents;
        }

        /// <summary>
        /// 从指定文件中获取所有含有指定模板的句子
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pattern"></param>
        /// <returns>经词性标注后，含有指定模板的句子集合</returns>
        static public List<string> ExtractSentence(FileInfo file,string pattern)
        {
            //调用分词工具的最小间隔
            const int minInterval = 6;
            List<string> targetSen = new List<string>();
            using (StreamReader sr = new StreamReader(file.OpenRead(), Encoding.Default))
            {
                int starTime, endTime;
                foreach (string sent in sr.ReadToEnd().Split('。'))
                {
                    if (sent != string.Empty)
                    {
                        starTime = DateTime.Now.Millisecond;
                        string posSen = AnswerManager.Instance.POS(sent);
                        string sentPattern = AnswerManager.Instance.EliminatePatternID(AnswerManager.Instance.GetPattern(posSen));
                        if (sentPattern.Contains(pattern))
                            targetSen.Add(posSen);
                        endTime = DateTime.Now.Millisecond;
                        if (endTime - starTime < minInterval)
                            Thread.Sleep(minInterval - (endTime - starTime));
                    }
                }
            }
            return targetSen;
        }
       
    }
}
