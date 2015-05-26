using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
        public MainForm()
        {
            InitializeComponent();

            char []whitespace = new char[]{' ', '\t', '\r', '\n'};
            StreamReader fs = new StreamReader(new FileStream("data/answer.txt", FileMode.Open, FileAccess.Read));
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
                MessageBox.Show("总题数与答案个数不一致！");
            }
            answer = line;

            textQuestion.LoadFile("data/0000.rtf");
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
                    }
                }
            }
            qIndex++;
            if (qIndex <= totalNum)
            {
                textQuestion.LoadFile(String.Format("data/{0:0000}.rtf", qIndex));
            }
            else
            {
                String msg = String.Format("测试结束，正确{0}题，总分{1}分", totalCorrect, totalCorrect * scorePerQ);
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
