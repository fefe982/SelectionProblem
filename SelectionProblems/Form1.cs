using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SelectionProblems
{
    public partial class MainForm : Form
    {
        // index of the current problem, starting form 1
        // 0 indicates before the test starts
        private int qIndex = 0;
        // total number of problems
        private int totalNum = 0;
        // score per problem
        private int scorePerQ = 0;
        // answers to each problem
        private string correctAnswers;
        // testee answers to the problems
        // starting from 1
        // testeeAnswers[0] has no meaning
        private int[] testeeAnswers;
        // The most index the testee has an answer to.
        // A testee may advance to problem, and go back with or without giving an answer to the current problem
        private int largestIndex = 0;
        // the selected answer by the student
        private int selected = -1;
        // whether we should show the detailed result to the students
        private bool showDetailedResult = false;
        // titlebar text;
        private string titleString = "测试";

        // random shuffled order of test indices staring from 1;
        // This should be only used when displaying problems,
        // or saving result.
        // Other subscript will all be the shuffled result.
        private int[] randomShuffle;

        private RadioButton[] radios = new RadioButton[4];

        public MainForm()
        {
            InitializeComponent();

            button_Prev.Enabled = false;

            radios[0] = radioButton1;
            radios[1] = radioButton2;
            radios[2] = radioButton3;
            radios[3] = radioButton4;
            foreach (RadioButton r in radios) { r.Enabled = false; r.Checked = false; }


            char []whitespace = new char[]{' ', '\t', '\r', '\n'};
            try
            {
                using (StreamReader fs = new StreamReader("data/answer.txt", Encoding.GetEncoding(936)))
                {
                    String line = fs.ReadLine();
                    line.Trim();
                    int idx = line.IndexOfAny(whitespace);
                    if (idx > 0)
                    {
                        line = line.Remove(idx);
                    }
                    scorePerQ = Int32.Parse(line);

                    line = fs.ReadLine();
                    line.Trim();
                    idx = line.IndexOfAny(whitespace);
                    if (idx > 0)
                    {
                        line = line.Remove(idx);
                    }
                    totalNum = Int32.Parse(line);

                    line = fs.ReadLine();
                    line.Trim();
                    line = line.ToUpper();
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] < 'A' || line[i] > 'D')
                        {
                            line = line.Remove(i, 1);
                            i--;
                        }
                    }

                    if (line.Length != totalNum)
                    {
                        MessageBox.Show("总题数与答案个数不一致");
                    }
                    correctAnswers = line;
                    testeeAnswers = new int[totalNum + 1];
                    randomShuffle = new int[totalNum + 1];
                    for (int i = 0; i < totalNum + 1; i++)
                    {
                        randomShuffle[i] = i;
                    }

                    Regex setting = new Regex(@"^\s*(\w+)\s*=\s*(\w+)$");
                    while (fs.Peek() >= 0)
                    {
                        line = fs.ReadLine();
                        MatchCollection mc;
                        line = Regex.Replace(line, @"\s*(?:;.*)?$", "");
                        if (line == "")
                        {
                            continue;
                        }
                        mc = setting.Matches(line);
                        if (mc.Count == 1)
                        {
                            String key = mc[0].Groups[1].Value;
                            String val = mc[0].Groups[2].Value;
                            switch (key)
                            {
                            case "show_result":
                                if (val == "true")
                                {
                                    showDetailedResult = true;
                                }
                                break;
                            case "shuffle_problems":
                                if (val == "true")
                                {
                                    Random r = new Random();
                                    for (int n = randomShuffle.Length - 1; n > 1; --n)
                                    {
                                        int k = r.Next(n) + 1;
                                        int temp = randomShuffle[n];
                                        randomShuffle[n] = randomShuffle[k];
                                        randomShuffle[k] = temp;
                                    }
                                }
                                break;
                            case "title":
                                titleString = val;
                                break;
                            default:
                                MessageBox.Show("未知的设置项 - " + key);
                                break;
                            }
                        }
                        else
                        {
                            MessageBox.Show("未能识别的设置 - " + line);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("未找到试卷定义文件 data/answer.txt");
                return;
            }
            try
            {
                textQuestion.LoadFile("data/0000.rtf");
            }
            catch (IOException)
            {
                textQuestion.Text = "请点击下一题";
                MessageBox.Show("缺失题目定义文件 data/0000.rtf");
            }
            setTitle();
        }

        private void button_Next_Click(object sender, EventArgs e)
        {
            if (qIndex > 0 && qIndex <= totalNum)
            {
                selected = -1;
                for (int i = 0; i < 4; i++)
                {
                    if (radios[i].Checked)
                    {
                        selected = i;
                        break;
                    }
                }
                if (selected == -1)
                {
                    MessageBox.Show("请选择一个选项");
                    return;
                }
                else
                {
                    testeeAnswers[qIndex] = selected;
                    if (largestIndex < qIndex)
                    {
                        largestIndex = qIndex;
                    }
                }
            }
            qIndex++;
            if (qIndex > 1 && qIndex <= totalNum)
            {
                button_Prev.Enabled = true;
            }
            else
            {
                button_Prev.Enabled = false;
            }
            if (qIndex <= totalNum)
            {
                showProblem();
            }
            else
            {
                String msg = "";
                int totalCorrect = 0;
                if (showDetailedResult)
                {
                    msg += "测试结果：\n";
                    for (int i = 1; i <= totalNum; i++)
                    {
                        bool answerCorrect = testeeAnswers[i] == correctAnswers[randomShuffle[i] - 1] - 'A';
                        if (i > 1 && i % 10 == 1)
                        {
                            msg += "\n";
                        }
                        msg += i + ". " + (answerCorrect ? "正确" : "错误") + "   ";
                        totalCorrect += answerCorrect ? 1 : 0;
                    }
                    msg += "\n";
                }
                msg += String.Format("测试结束，正确{0}题，总分{1}分", totalCorrect, totalCorrect * scorePerQ);
                textQuestion.Text = msg;
                MessageBox.Show(msg);
                foreach (RadioButton r in radios) { r.Enabled = false; }
            }
            if (qIndex > largestIndex || qIndex < 1)
            {   // no problem, no answer provided previously
                foreach (RadioButton r in radios) { r.Checked = false; r.Enabled = qIndex >= 1 && qIndex <= totalNum; }
            }
            else
            {
                radios[testeeAnswers[qIndex]].Checked = true;
            }
            setTitle();
        }

        private void button_Prev_Click(object sender, EventArgs e)
        {
            if (qIndex <= 1 || qIndex > totalNum)
            {
                button_Prev.Enabled = false;
                return;
            }
            int selected = -1;
            for (int i = 0; i < 4; i++)
            {
                if (radios[i].Checked == true)
                {
                    selected = i;
                    break;
                }
            }
            if (selected >= 0)
            {
                testeeAnswers[qIndex] = selected;
                if (qIndex > largestIndex)
                {
                    largestIndex = qIndex;
                }
            }
            qIndex--;
            showProblem();
            radios[testeeAnswers[qIndex]].Checked = true;
            setTitle();
        }

        private void showProblem()
        {
            String fn = String.Format("data/{0:0000}.rtf", randomShuffle[qIndex]);
            try
            {
                textQuestion.LoadFile(fn);
            }
            catch (IOException)
            {
                MessageBox.Show("打开试题文件 " + fn + " 失败");
                this.Close();
                return;
            }
        }

        private void setTitle()
        {
            if (qIndex < 1 || qIndex > totalNum)
            {
                Text = titleString;
            }
            else
            {
                Text = titleString + "  " + qIndex + " / " + totalNum;
            }
        }
    }
}
