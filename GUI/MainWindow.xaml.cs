using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IdiotTalk;
using System.IO;
using System.Threading.Tasks;

namespace GUI
{
    internal enum SessionState
    {
        /// <summary>
        /// 等待用户输入答案
        /// </summary>
        WaitQuestion,

        /// <summary>
        /// 等待用户输入答案
        /// </summary>
        WaitAnswer,

        /// <summary>
        /// 等待用户输入精确答案
        /// </summary>
        WaitExactAnswer

    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string corpursDir = @"F:\Demo\DemoData";
        SessionState sessionState = SessionState.WaitQuestion;
        private string question, answer, exactAnswer;
       public MainWindow()
        {
            InitializeComponent();
            textBox.Focus();
        }
        //关闭按钮
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //最小化按钮
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        //取消按钮
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            this.textBox.Text = "";
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text != string.Empty)
            {
                switch (sessionState)
                {
                    case SessionState.WaitQuestion:
                        question = textBox.Text;
                        stackPanel.Children.Add(CreateLabel(textBox.Text, true));
                        Task.Factory.StartNew(AsynRetrivalAnswer);
                        button2.IsEnabled = false;
                        button2.Background = new SolidColorBrush(Color.FromRgb(109, 109, 109));
                        break;
                    case SessionState.WaitAnswer:
                        answer = textBox.Text;
                        stackPanel.Children.Add(CreateLabel(answer, true));
                        stackPanel.Children.Add(CreateLabel("请帮我指出这句话中的精确答案"));
                        sessionState = SessionState.WaitExactAnswer;
                        break;
                    case SessionState.WaitExactAnswer:
                        exactAnswer = textBox.Text;
                        stackPanel.Children.Add(CreateLabel(exactAnswer, true));
                        stackPanel.Children.Add(CreateLabel("学习中，请稍等。。。"));
                        button2.IsEnabled = false;
                        button2.Background = new SolidColorBrush(Color.FromRgb(109, 109, 109));
                        Task.Factory.StartNew(AsynLearningPattern);
                        sessionState = SessionState.WaitQuestion;
                        break;
                }
            }
            textBox.Text = "";
            textBox.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter&&button2.IsEnabled)
                this.button2_Click(sender,e);
        }

        //异步生成模板，知识库生成完毕后更新UI界面
        private void AsynLearningPattern( )
        {
            AnswerManager.Instance.AddNewPattern(new DirectoryInfo(corpursDir),
                                                                         AnswerManager.Instance.POS(question),
                                                                          AnswerManager.Instance.POS(answer),
                                                                          AnswerManager.Instance.POS(exactAnswer));
            this.Dispatcher.BeginInvoke(
                new Action(
                              () =>
                              {
                                  stackPanel.Children.Add(CreateLabel("我懂了，谢谢！"));
                                  button2.IsEnabled = true;
                                  button2.Background = new SolidColorBrush(Color.FromRgb(219, 229, 209));
                              }
                              )
                );
            
        }
        //异步检索知识库
        private void AsynRetrivalAnswer()
        {
            string answerFromBase = AnswerManager.Instance.GetAnswer(question);
            if (answerFromBase == string.Empty)
            {
                this.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        stackPanel.Children.Add(CreateLabel("这个问题我不懂，你能教我吗？"));
                        button2.IsEnabled = true;
                        button2.Background = new SolidColorBrush(Color.FromRgb(219, 229, 209));
                        }
                    ));
                
                sessionState = SessionState.WaitAnswer;
            }
            //已经学习过的问题类型
            else
            {
                this.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        stackPanel.Children.Add(CreateLabel(answerFromBase));
                        button2.IsEnabled = true;
                        button2.Background = new SolidColorBrush(Color.FromRgb(219, 229, 209));
                    }
                    ));
                sessionState = SessionState.WaitQuestion;
            }
        }

        //生成UI句子
        private Label CreateLabel(string text,bool rightHand=false)
        {
            Label label = new Label();
            label.Content = text;
            label.FontSize = 15;
            if (rightHand)
                label.HorizontalContentAlignment = HorizontalAlignment.Right;
            return label;
        }

    }
   
}
