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
        int qIndex = 0;
        int totalNum = 0;
        int scorePerQ = 0;
        string answer;
        int totalCorrect = 0;
        int selected = -1;
        bool[] answerCorrect;
        bool showDetailedResult = false;
        public MainForm()
        {
            InitializeComponent();

            char []whitespace = new char[]{' ', '\t', '\r', '\n'};
            StreamReader fs;
            try
            {
                fs = new StreamReader("data/answer.txt");
            }
            catch (Exception)
            {
                MessageBox.Show("未找到试卷定义文件 data/answer.txt");
                return;
            }
            try
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
                answer = line;
                answerCorrect = new bool[totalNum];

                Regex setting = new Regex(@"^\s*(\w+)\s*=\s*(\w+)\s*(?:;.*)?$");
                while (fs.Peek() >= 0)
                {
                    line = fs.ReadLine();
                    MatchCollection mc = setting.Matches(line);
                    if (mc.Count == 1)
                    {
                        String key = mc[0].Groups[1].Value;
                        String val = mc[0].Groups[2].Value;
                        switch(key)
                        {
                        case "show_result":
                            if (val == "true")
                            {
                                showDetailedResult = true;
                            }
                            break;
                        default:
                            MessageBox.Show("未知的设置项 - " + key);
                            break;
                        }
                    }
                }
            }
            finally
            {
                fs.Close();
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
        }

        private void button_Next_Click(object sender, EventArgs e)
        {
            if (qIndex > 0 && qIndex <= totalNum)
            {
                selected = -1;
                if (radioButton1.Checked)
                {
                    selected = 0;
                }
                else if (radioButton2.Checked)
                {
                    selected = 1;
                }
                else if (radioButton3.Checked)
                {
                    selected = 2;
                }
                else if (radioButton4.Checked)
                {
                    selected = 3;
                }
                if (selected == -1)
                {
                    MessageBox.Show("请选择一个选项");
                    return;
                }
                else
                {
                    if (selected == answer[qIndex - 1] - 'A')
                    {
                        totalCorrect++;
                        answerCorrect[qIndex - 1] = true;
                    }
                    else
                    {
                        answerCorrect[qIndex - 1] = false;
                    }
                }
            }
            qIndex++;
            if (qIndex <= totalNum)
            {
                String fn = String.Format("data/{0:0000}.rtf", qIndex);
                try
                {
                    textQuestion.LoadFile(fn);
                }
                catch(IOException)
                {
                    MessageBox.Show("打开试题文件 " + fn + " 失败");
                    this.Close();
                    return;
                }
            }
            else
            {
                String msg = "";
                if (showDetailedResult)
                {
                    msg += "测试结果：\n";
                    for (int i = 0; i < totalNum; i++)
                    {
                        if (i > 0 && i % 10 == 0)
                        {
                            msg += "\n";
                        }
                        msg += (i + 1) + ". " + (answerCorrect[i] ? "正确" : "错误") + "   ";
                    }
                    msg += "\n";
                }
                msg += String.Format("测试结束，正确{0}题，总分{1}分", totalCorrect, totalCorrect * scorePerQ);
                textQuestion.Text = msg;
                MessageBox.Show(msg);
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                radioButton3.Enabled = false;
                radioButton4.Enabled = false;
            }
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
        }
    }
}
