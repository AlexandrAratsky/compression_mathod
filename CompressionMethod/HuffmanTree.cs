using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompressionMethod
{
    class HuffmanNode
    {
        private bool isLetter;
        private char letter;
        private int freq;

        private HuffmanNode left;
        private HuffmanNode right;

        public HuffmanNode(char c, int fr)
        {
            letter = c;
            isLetter = true;
            freq = fr;
        }
        public HuffmanNode(int fr)
        {
            letter = (char)0;
            isLetter = false;
            freq = fr;
        }


        public int Freq
        {
            get { return freq; }
        }
        public char Letter
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

    class HuffmanTree
    {
        private HuffmanNode root;
        private Dictionary<char, string> table = new Dictionary<char, string>();
 
        public HuffmanTree(Dictionary<char, int> freqSymbols)
        {
            List<HuffmanNode> listNode = new List<HuffmanNode>();

            foreach (var i in freqSymbols)
            {
                listNode.Add(new HuffmanNode(i.Key,i.Value));
            }

            if (listNode.Count == 1) throw  new Exception("sgdasgfa");

            while (listNode.Count>1)
            {
                listNode.Sort((a, b) =>
                    {
                        if (a.Freq < b.Freq) return -1;
                        return 1;
                    });
                HuffmanNode first = listNode[0]; listNode.RemoveAt(0);
                HuffmanNode second = listNode[0]; listNode.RemoveAt(0);
                listNode.Add(new HuffmanNode(first.Freq + second.Freq) {Left = first, Right = second});
                
            }

            root = listNode[0];
        }

        private void NextNode(HuffmanNode node, string code)
        {
            if (!node.IsLetter)
            {
                NextNode(node.Left,code+"0");
                NextNode(node.Right,code+"1");
            }
            else table.Add(node.Letter,code); 
        }

        public Dictionary<char, string> GetCodeTable()
        {
            NextNode(root,"");
            return table;
        }
    }
}
