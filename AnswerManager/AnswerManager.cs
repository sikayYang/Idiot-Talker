using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IdiotTalk
{
    public class AnswerManager
    {
        private static AnswerManager _answerManager;
        private Parser _parser;
        private string _knowledgeBasePath;
        //无实际意义的虚词词性标注集合
        private const string _funcitonWord = "b d e g h i o u wp x";
        private static object _sycobj = new object();
        //设置知识库路径
        public string KnowledgeBasePath
        {
            set { _knowledgeBasePath = value; }
        }
        private AnswerManager()
        {
            _parser = new Parser();
            _knowledgeBasePath =  @"F:\Demo\DemoKnowledgeBase\knowledgeBase.xml";
        }

        public static AnswerManager Instance
        {
            get
            {
                if (_answerManager == null)
                {
                    lock(_sycobj)
                    {
                        if (_answerManager == null)
                        {
                            _answerManager = new AnswerManager();
                            return _answerManager;
                        }
                    }
                    
                }
                return _answerManager;
            }
        }

        /// <summary>
        /// 对传入的xml片段获取对应的句子模式
        /// </summary>
        /// <param name="xmlSen">句法分析后的xml片段</param>
        /// <param name="id">是否保留词性标记在句子中出现的位置id</param>
        /// <returns>返回字符串中以空格分割，数字代表其后的词性标记在原始句子中的位置</returns>
        public string GetPattern(string xmlSeg)
         {
            StringBuilder sb = new StringBuilder() ;
            try
            {
                XDocument xdoc = XDocument.Parse(xmlSeg);
                
                foreach (XElement xele in xdoc.Descendants("word"))
                {
                    if (!_funcitonWord.Contains(xele.Attribute("pos").Value))
                            sb.AppendFormat("{0},{1} ", xele.Attribute("id").Value, xele.Attribute("pos").Value);
                }
            }
            catch
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                new Exception(string.Format("Error in {0} Load xml segment",st.ToString()));
            }
            return sb.ToString();
        }
        
       
       

        /// <summary>
        /// 对输入的句子进行词性标注
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public string POS(string sentence)
        {
            _parser.Pattern = HttpParams.Pattern.POS;
            try
            {
                return _parser.Analyze(sentence);
            }
            catch
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                new Exception(string.Format("Error in {0} POS sentence", st.ToString()));
                return string.Empty;
            }
           
        }

        /// <summary>
        ///根据输入的问题，从知识库中搜索答案
        /// </summary>
        /// <param name="question">问题</param>
        /// <returns>1）当知识库中没有这类问题模板时返回空串；2)知识库中有这类问题模板，但是语料库中没有这个问题的语料信息，返回“语料不足”
        ///        3)知识库中有此类问题的模板，并且已经从语料库中学到这个问题的答案，则返回答案 </returns>
        public string GetAnswer(string question)
        {
            string answer = string.Empty;
            try
            {
                //加载知识库
                XDocument knowledgeBase = XDocument.Load(_knowledgeBasePath);
                string questionPOS = POS(question);
                XDocument questionDoc = XDocument.Parse(questionPOS);
                string questionPattern = GetPattern(questionPOS);
                string patternWithoutID = EliminatePatternID(questionPattern);
                //获取所有的knowledge节点
                XElement knowledge = knowledgeBase.Descendants("knowledge").Where(ele => ele.Attribute("questionPattern").Value ==patternWithoutID).FirstOrDefault();
                bool isRightNode = true;
                if (knowledge!=null)
                {
                    int[] wordIDs;
                    string[] wordPOSs;
                    int len;
                    SplitPOSAndID(questionPattern, out wordIDs, out wordPOSs, out len);

                    //获取区分不同问题的特征下标
                    string []quesFeatureIndexes = knowledge.Attribute("questionFearture").Value.Split(' ');
                    foreach(XElement answerDetail in knowledge.Descendants("answerDetail"))
                    {
                        isRightNode = true;
                        foreach(var str in quesFeatureIndexes)
                        {
                            string quesCont = questionDoc.Descendants("word").Where(ele => ele.Attribute("id").Value == wordIDs[Convert.ToInt32(str)].ToString()).FirstOrDefault().Attribute("cont").Value;
                            string knowledgeCont = answerDetail.Attribute(wordPOSs[Convert.ToInt32(str)]).Value;
                            if (quesCont != knowledgeCont)
                            {
                                isRightNode = false;
                                break;
                            }
                        }
                        if (isRightNode)
                        {
                            answer = answerDetail.Descendants("answer").FirstOrDefault().Value;
                            break;
                        }
                    }
                    if (!isRightNode)
                        answer = "语料信息不足，无法回答该问题";
                }
                    
            }catch
            {
                StackTrace st = new StackTrace(new StackFrame(true));
                new Exception(string.Format("Error in {0} get answer", st.ToString()));
            }
            return answer;
        }

        /// <summary>
        /// 向知识库中添加新的知识
        /// </summary>
        /// <param name="file">语料库文件路径</param>
        /// <param name="questionPOS">词性标注后的问题</param>
        /// <param name="answerPOS">词性标注后的答案</param>
        /// <param name="exactAnswerPOS">词性标注后的精确答案</param>
        /// <returns>向知识库中添加的记录条数</returns>
        public int AddNewPattern(FileInfo file, string  questionPOS,string answerPOS,string exactAnswerPOS)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_knowledgeBasePath);

            //获取模板并消除模板中的ID信息
            string questionPattern = EliminatePatternID(GetPattern(questionPOS));
            string answerPattern = EliminatePatternID(GetPattern(answerPOS));
            string exactAnswerPattern = EliminatePatternID(GetPattern(exactAnswerPOS));
            //获取问题和答案关联特征
            List<Tuple<string, string, int, int>> features = LearnFeatures(questionPOS,answerPOS);
            StringBuilder sb = new StringBuilder();
            //准备knowledge节点的属性信息
            int[] answerFeatureIndexes = new int[features.Count];
            for(int i=0;i<features.Count;i++)
            {
                sb.Append(features[i].Item3.ToString()+" ");
                answerFeatureIndexes[i] = features[i].Item4;
            }
            string quesFeatureIndexes = sb.ToString();

            int[] exactAnswerIndexes = ExactAnswerPos(answerPOS, exactAnswerPOS);
            sb.Clear();
            foreach (var i in exactAnswerIndexes)
                sb.Append(i.ToString()+" ");
            string strExactAnswerIndexes = sb.ToString().Trim();
            sb.Clear();
            foreach(var i in answerFeatureIndexes)
                sb.Append(i.ToString()+" ");
            string strAswerFeaIndexes = sb.ToString().Trim();

            //填充knowledge节点属性
            XmlNode xmlNode = xdoc.SelectSingleNode(string.Format("//knowledge[@questionPattern=\"{0}\"]",questionPattern));
            XmlElement xele;
            if (xmlNode == null)
            {
                xele = xdoc.CreateElement("knowledge");
                xele.SetAttribute("questionPattern", questionPattern);
                xele.SetAttribute("questionFearture", quesFeatureIndexes.Trim());
                xele.SetAttribute("answerPattern", answerPattern);
                xele.SetAttribute("answerFeature", strAswerFeaIndexes);
                xele.SetAttribute("exactAnswerPosition", strExactAnswerIndexes);
            }
            else
                xele = xmlNode as XmlElement;
            //获取语料库中所有符合答案模板的语句
            List<string> targetSents= Extractor.ExtractSentence(file,answerPattern);
            XmlElement quesDetail;
            //对符合答案模板的语料信息进行整理并装填为answerDetail结点
            foreach (string sent in targetSents)
            {
               string[] splitedPattern= LocateAnswerPattern(sent,answerPattern);
                List<Tuple<string, string>> exactAnswerTuple = ExtractFeature(sent,splitedPattern,exactAnswerIndexes);
                sb.Clear();
                foreach (var t in exactAnswerTuple)
                    sb.Append(t.Item2);
               quesDetail = FillDetailNode(xdoc,ExtractFeature(sent,splitedPattern,answerFeatureIndexes),sb.ToString());
                xele.AppendChild(quesDetail);
            }
            xdoc.DocumentElement.AppendChild(xele);
            xdoc.Save(_knowledgeBasePath);
            return targetSents.Count;
            
        }
        public int AddNewPattern(DirectoryInfo dir,string questionPOS, string answerPOS, string exactAnswerPOS)
        {
            int count = 0;
            foreach(var childDir in dir.EnumerateDirectories())
                count += AddNewPattern(childDir, questionPOS, answerPOS, exactAnswerPOS); ;
            foreach (var file in dir.EnumerateFiles())
                count += AddNewPattern(file,questionPOS,answerPOS,exactAnswerPOS);
            return count;
        }

        /// <summary>
        /// 生成一个具体的答案节点
        /// </summary>
        /// <param name="attributes">用于区分不同问题的属性值集合</param>
        /// <param name="exactAnswer">该问题的准确答案</param>
        /// <returns></returns>
        public XmlElement FillDetailNode(XmlDocument xdoc, List<Tuple<string,string>> attributes,string exactAnswer)
        {
            XmlElement quesDetail = xdoc.CreateElement("answerDetail");
            foreach (var attr in attributes)
                quesDetail.SetAttribute(attr.Item1,attr.Item2);
            XmlElement answer = xdoc.CreateElement("answer");
            answer.InnerText = exactAnswer;
            quesDetail.AppendChild(answer);
            return quesDetail;
        }

        /// <summary>
        /// 消除模板里的ID
        /// </summary>
        /// <param name="patternWithID">含有ID的模板</param>
        /// <returns></returns>
        public string EliminatePatternID(string patternWithID)
        {
            StringBuilder sb = new StringBuilder();
            string[] splitedString = patternWithID.Split(' ');
            foreach (string str in splitedString)
                sb.AppendFormat("{0} ", str.Substring(str.IndexOf(',') + 1));
            return sb.ToString().Trim()+" ";
        }

        /// <summary>
        /// 获取区分具体问题的属性
        /// </summary>
        /// <param name="posQuestion">词性标注后的问题</param>
        /// <param name="posAnswer">词性标注后的答案</param>
        /// <returns>以元组数组的形式返回属性值，每个元组的第一项为词性标记，第二项为对应的词,第三项为该词性标记在问题模板中的位置，第四项为词性标记在答案模板中的位置</returns>
      public List<Tuple<string,string,int,int>> LearnFeatures(string posQuestionXml,string posAnswerXml)
        {
            XDocument questionDoc = XDocument.Parse(posQuestionXml);
            XDocument answerDoc = XDocument.Parse(posAnswerXml);

            List<Tuple<string, string,int,int>> features = new List<Tuple<string, string,int,int>>();
            int[] questionIDs,answerIDs;
            string[] questionPOSs,answerPOSs;
            int questionLen,answerLen;

            SplitPOSAndID(GetPattern(posQuestionXml),out questionIDs,out questionPOSs,out questionLen);
            SplitPOSAndID(GetPattern(posAnswerXml), out answerIDs, out answerPOSs, out answerLen);

            for(int i=0;i<questionLen;i++)
            {
                for(int j=0;j<answerLen;j++)
                {
                    if(questionPOSs[i]==answerPOSs[j])
                    {
                        string quesAttrCont = questionDoc.Descendants("word").Where(ele => ele.Attribute("id").Value == questionIDs[i].ToString())
                                                                                        .FirstOrDefault().Attribute("cont").Value;
                        string answerAttrCont= answerDoc.Descendants("word").Where(ele => ele.Attribute("id").Value == answerIDs[j].ToString())
                                                                                        .FirstOrDefault().Attribute("cont").Value;
                        if (quesAttrCont==answerAttrCont)
                        {
                            Tuple<string, string,int,int> tuple = new Tuple<string, string,int,int>(questionPOSs[i],quesAttrCont,i,j);
                           features.Add(tuple);
                        }
                    }
                }
            }
            return features ;
        }


        /// <summary>
        /// 分离模板中的ID和词性标注
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="IDs">分离后ID的存储数组</param>
        /// <param name="POSs">分离后POS的存储数组</param>
        /// <param name="len">数组长度</param>
        /// <returns></returns>
        private bool SplitPOSAndID(string pattern,out int [] IDs,out string[] POSs,out int len)
        {
            string[] splitedPattern = pattern.Trim().Split(' ');
            IDs = new int[splitedPattern.Length];
            POSs = new string[splitedPattern.Length];
            len = splitedPattern.Length;
            for(int i=0;i<splitedPattern.Length;i++)
            {
                try
                {
                    IDs[i]= Convert.ToInt32(splitedPattern[i].Split(',')[0]);
                    POSs[i] = splitedPattern[i].Split(',')[1];
                }catch
                {
                    StackTrace st = new StackTrace(new StackFrame(true));
                    new Exception(string.Format("Error in {0} Convert ID", st.ToString()));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 在答案语句中提取精确答案的词性位置
        /// </summary>
        /// <param name="answerPOSXml">词性标记后的答案语句</param>
        /// <param name="exactAnswerPOSXml">词性标记后的精确答案的位置</param>
        /// <returns></returns>
        private int[] ExactAnswerPos(string answerPOSXml,string exactAnswerPOSXml)
        {
            List<int> indexes = new List<int>();
            XDocument answerDoc = XDocument.Parse(answerPOSXml);
            XDocument exactAnswerDoc = XDocument.Parse(exactAnswerPOSXml);
            int[] answerIDs, exactAnswerIDs;
            string[] answerPOS, exactAnswerPOS;
            int answerLen,exactAnswerLen;
            SplitPOSAndID(GetPattern(answerPOSXml), out answerIDs, out answerPOS,out answerLen);
            SplitPOSAndID(GetPattern(exactAnswerPOSXml),out exactAnswerIDs,out exactAnswerPOS,out exactAnswerLen);
            for(int i=0;i<exactAnswerLen;i++)
            {
                for(int j=0;j<answerLen;j++)
                {
                    if(answerPOS[j]==exactAnswerPOS[i])
                    {
                        string answerCont= answerDoc.Descendants("word").Where(ele => ele.Attribute("id").Value == answerIDs[j].ToString()).FirstOrDefault().Attribute("cont").Value;
                        string exactAnswerCont= exactAnswerDoc.Descendants("word").Where(ele => ele.Attribute("id").Value == exactAnswerIDs[i].ToString()).FirstOrDefault().Attribute("cont").Value;
                        if(answerCont==exactAnswerCont)
                            indexes.Add(j);
                    }
                }
            }
            return indexes.ToArray();
        }

        /// <summary>
        /// 提取句子中的答案模板所在的词性标注
        /// </summary>
        /// <param name="posAnswerXml">经词性标注后含有答案模板的句子</param>
        /// <param name="answerPattern">答案模板（不含ID）</param>
        /// <returns>以数组形式返回句子中答案模板中含有的成分，每组为标号，词性标记</returns>
        private string [] LocateAnswerPattern(string posSenXml,string answerPattern)
        {
            
            string pattern = EliminatePatternID(GetPattern(posSenXml));
            int answerLen = answerPattern.Trim().Split(' ').Length;
            string[] answerInSen = new string[answerLen];
            string[] items = pattern.Substring(0, pattern.IndexOf(answerPattern)).Trim().Split(' ');
            int startPos;
            if (items.Length != 0 && items[0] == "")
                startPos = 0;
            else startPos = items.Length;
            string[] word = GetPattern(posSenXml).Split(' ');
            for (int i = 0; i < answerLen; i++)
                answerInSen[i] = word[startPos + i];
            return answerInSen;
        }

        /// <summary>
        /// 从含有词性标注的句子中提取指定位置的特征
        /// </summary>
        /// <param name="posSent">经词性标注后的句子</param>
        /// <param name="splitedPOS">切分后的模板</param>
        /// <param name="indexes">指定</param>
        /// <returns></returns>
        private List<Tuple<string ,string>> ExtractFeature(string posSent,string[]splitedPattern,int[] indexes)
        {
            XDocument xdoc = XDocument.Parse(posSent);
            List<Tuple<string, string>> features=new List<Tuple<string, string>>();
            int[] IDs;
            string[] POSs;
            int len;
            for(int i=0;i<indexes.Length;i++)
            {
                SplitPOSAndID(splitedPattern[indexes[i]],out IDs,out POSs,out len);
                string cont = xdoc.Descendants("word").Where(ele => ele.Attribute("id").Value == IDs[0].ToString()).FirstOrDefault().Attribute("cont").Value;
                Tuple<string, string> tuple = new Tuple<string, string>(POSs[0],cont);
                features.Add(tuple);
            }
            return features;
        }
        
    }

}
