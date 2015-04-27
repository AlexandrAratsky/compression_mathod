using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;

namespace CompressionMethod
{

    public partial class MainForm : Form
    {
        private Dictionary<byte,int> freq = new Dictionary<byte, int>();
        private HuffmanNode root;
        private Dictionary<byte, string> table = new Dictionary<byte, string>();
        private byte[] buffer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ShowCodeTable()
        {
            if (codeTableToolStripMenuItem.Checked)
            {
                var cd = new CodeTable(table);
                    cd.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
    
        }

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = @"Bin files (*.bin)| *.bin";
            openFileDialog.Title = @"Открыть сжатый бинарный файл";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                table.Clear();
                BinaryFormatter binFormat = new BinaryFormatter();

                using (Stream fStream = File.OpenRead(openFileDialog.FileName))
                {
                    BinFile file = (BinFile)binFormat.Deserialize(fStream);
                    NextNode(file.Root, "");
                    ShowCodeTable();
                    backgroundWorker_decoder.RunWorkerAsync(file);
                }
            }

        }

        private void оПрограммеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            //string text = (string) e.Argument;
            byte[] buffer = (byte[]) e.Argument;
            for (int index = 0; index < buffer.Length; index++)
            {
                var c = buffer[index];
                if (!freq.ContainsKey(c)) freq.Add(c, 1);
                else freq[c]++;
                backgroundWorker.ReportProgress((int)((float)index / buffer.Length * 100), "Рассчёт частоты символов");
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
        private void NextNode(NodeLetter node, string code)
        {
            if (!node.IsLetter)
            {
                NextNode(node.Left, code + "0");
                NextNode(node.Right, code + "1");
            }
            else table.Add(node.Letter, code);
        }

        private NodeLetter newNodeLetter(HuffmanNode node)
        {
            if (node != null)
            {
                if (!node.IsLetter)
                    return new NodeLetter() {Left = newNodeLetter(node.Left), Right = newNodeLetter(node.Right)};
                else return new NodeLetter(node.Letter);
            }
            return null;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            statusLabel.Text = @"Дерево Хаввмана построено";
            root = (HuffmanNode) e.Result;
            table.Clear();
            NextNode(root, "");
            var cd = new CodeTable(table);
            cd.Show();
        }

        private void codeingMenu_Click(object sender, EventArgs e)
        {
            int count_bit = table.Keys.Sum(c => freq[c]*table[c].Length);
            const int N = sizeof (ushort)*8;
            int n = count_bit/N + ((count_bit%N > 0) ? 1 : 0);
            BinFile bin = new BinFile(buffer.Length, n, newNodeLetter(root));
            Tuple<byte[], BinFile> info = new Tuple<byte[], BinFile>(buffer, bin);
            backgroundWorker_coder.RunWorkerAsync(info);
        }

        private void backgroundWorker_coder_DoWork(object sender, DoWorkEventArgs e)
        {
            const int N = sizeof(ushort) * 8;
            BinFile bin = ((Tuple<byte[], BinFile>)e.Argument).Item2;
            byte[] text = ((Tuple<byte[], BinFile>)e.Argument).Item1;
            int len = text.Length;            
            string tmp = "";
            int i = 0; int j = 0;
            while (i < len)
            {
                while (tmp.Length < N) tmp += (i < len ? table[text[i++]] : "#");
                ushort t = 0;
                for (int k = 0; k < N; k++)
                    if (tmp[k] == '1') t += (ushort)(1 << (N - k - 1));
                bin[j++] = t;
                backgroundWorker_coder.ReportProgress((int)(((float)i/len)*100),j+") "+Convert.ToString(t,2));
                tmp = tmp.Remove(0, N);
            }
            e.Result = bin;
        }

        private void backgroundWorker_coder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = (string)e.UserState;
        }

        private void backgroundWorker_coder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BinFile bin = (BinFile) e.Result;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryFormatter binFormat = new BinaryFormatter();
                using (Stream fStream = new FileStream(saveFileDialog.FileName+".bin", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormat.Serialize(fStream, bin);
                }
            }
            progressBar.Value = 0;
            statusLabel.Text = Path.GetFileName(saveFileDialog.FileName + ".bin") + @" сохранён!";
        }

        private void backgroundWorker_decoder_DoWork(object sender, DoWorkEventArgs e)
        {
            BinFile bin = (BinFile)e.Argument;
            NodeLetter node = bin.Root;
            byte[] buf = new byte[bin.Lenght];
            int i = 0;
            for (int k = 0; k < bin.Count; k++)
                for (int l = sizeof(ushort) * 8; l > 0; l--)
                {
                    if (node.IsLetter)
                    {
                        buf[i++] = node.Letter;
                        if (i >= bin.Lenght) break;
                        node = bin.Root;
                    }
                    node = (bin[k] & (1 << l - 1)) != 0 ? node.Right : node.Left;
                }
            e.Result = buf;
        }

        private void backgroundWorker_decoder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            byte[] buf = (byte[]) e.Result;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream fStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fStream.Write(buf,0,buf.Length);
                }
                progressBar.Value = 0;
                statusLabel.Text = "Декодирование завершено";
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                BinaryFormatter binFormat = new BinaryFormatter();
                NodeLetter s = newNodeLetter(root);
                table.Clear();
                NextNode(s, "");
                var st = new CodeTable(table);
                st.Show();
                using (Stream fStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    binFormat.Serialize(fStream, s);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                table.Clear();
                BinaryFormatter binFormat = new BinaryFormatter();

                using (Stream fStream = File.OpenRead(openFileDialog.FileName))
                {
                    NodeLetter node = (NodeLetter)binFormat.Deserialize(fStream);
                    NextNode(node, "");
                    var cd = new CodeTable(table);
                    cd.Show();
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                table.Clear();
                using (Stream str = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    buffer = new byte[str.Length];
                    str.Read(buffer, 0, (int)str.Length);
                    backgroundWorker.RunWorkerAsync(buffer);
                }
                
                
                //richTB.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.PlainText);
                //backgroundWorker.RunWorkerAsync(buffer);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void очиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void backgroundWorker_decoder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = (string)e.UserState;
        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


    }


    [Serializable]
    public class BinFile
    {
        private int lenght;
        private int N;
        private ushort[] bits;
        private NodeLetter root;

        public BinFile(int len, int ushort_count, NodeLetter root_node)
        {
            lenght = len;
            bits = new ushort[ushort_count];
            N = ushort_count;
            root = root_node;
        }

        public NodeLetter Root
        {
            get { return root; }
        }
        public int Lenght
        {
            get { return lenght; }
        }
        public ushort this[int index]
        {
            get { return bits[index]; }
            set { bits[index] = value; }
        }
        public int Count 
        {
            get { return N; }
        }
    }

    [Serializable]
    public class NodeLetter
    {
        private bool is_letter;
        private byte letter;
        private NodeLetter left = null;
        private NodeLetter right = null;

        public NodeLetter(byte c)
        {
            is_letter = true;
            letter = c;
        }
        public NodeLetter()
        {
            is_letter = false;
            letter = 0;
        }

        public bool IsLetter
        {
            get { return is_letter; }
        }
        public byte Letter
        {
            get { return letter; }
        }
        public NodeLetter Left
        {
            get { return left; }
            set { left = value; }
        }
        public NodeLetter Right
        {
            get { return right; }
            set { right = value; }
        }
    }

    internal class HuffmanNode
    {
        private bool isLetter;
        private byte letter;
        private int freq;

        private HuffmanNode left;
        private HuffmanNode right;

        public HuffmanNode(byte c, int fr)
        {
            letter = c;
            isLetter = true;
            freq = fr;
        }
        public HuffmanNode(int fr)
        {
            letter = 0;
            isLetter = false;
            freq = fr;
        }
        public int Freq
        {
            get { return freq; }
        }
        public byte Letter
        {
            get { return letter; }
        }
        public bool IsLetter
        {
            get { return isLetter; }
        }
        public HuffmanNode Left
        {
            get { return left; }
            set { left = value; }
        }
        public HuffmanNode Right
        {
            get { return right; }
            set { right = value; }
        }
    }


}
