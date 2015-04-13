using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CompressionMethod
{
    public partial class MainForm : Form
    {
        private Dictionary<char,int> freq = new Dictionary<char, int>();
        private HuffmanNode root;
        private Dictionary<char, string> table = new Dictionary<char, string>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
    
        }

        //private void FreqSymbols()
        //{
        //    lengthFile = 0;
        //    foreach (char c in richTB.Text)
        //    {
        //        if (!freq.ContainsKey(c)) freq.Add(c, 1);
        //        else freq[c]++;
        //        lengthFile++;
        //    }
        //}

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(openFileDialog.FileName) == "bin")
                {
                    // do something with *.bin
                }
                richTB.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.PlainText);
                backgroundWorker.RunWorkerAsync(richTB.Text);
            }

        }

        private void оПрограммеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            string text = (string) e.Argument;
            for (int index = 0; index < text.Length; index++)
            {
                char c = text[index];
                if (!freq.ContainsKey(c)) freq.Add(c, 1);
                else freq[c]++;
                backgroundWorker.ReportProgress((int)((float)index / text.Length * 100), "Рассчёт частоты символов");
            }

            List<HuffmanNode> listNode = freq.Select(i => new HuffmanNode(i.Key, i.Value)).ToList();

            if (listNode.Count == 1) throw new Exception("Всего одна буква в тексте");

            int listCount = listNode.Count;
            while (listNode.Count > 1)
            {
                listNode.Sort((a, b) =>
                {
                    if (a.Freq < b.Freq) return -1;
                    return 1;
                });
                HuffmanNode first = listNode[0]; listNode.RemoveAt(0);
                HuffmanNode second = listNode[0]; listNode.RemoveAt(0);
                listNode.Add(new HuffmanNode(first.Freq + second.Freq) { Left = first, Right = second });
                backgroundWorker.ReportProgress(100 - (int)((float)listNode.Count / listCount * 100), "Кодирование дерева");
            }
            e.Result = listNode[0];
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = (string) e.UserState;
        }


        private void NextNode(HuffmanNode node, string code)
        {
            if (!node.IsLetter)
            {
                NextNode(node.Left, code + "0");
                NextNode(node.Right, code + "1");
            }
            else table.Add(node.Letter, code);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            statusLabel.Text = @"Кодирование завершено";
            root = (HuffmanNode) e.Result;
            NextNode(root, "");
            var cd = new CodeTable(table);
            cd.ShowDialog();
        }
    }
}
