using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CompressionMethod
{
    public partial class CodeTable : Form
    {
        private Dictionary<byte, string> table;

        public CodeTable(Dictionary<byte, string> t)
        {
            InitializeComponent();
            table = t;
        }

        private void CodeTable_Load(object sender, EventArgs e)
        {
            foreach (var node in table)
            {
                dataGridView.Rows.Add("#"+(node.Key).ToString(), node.Value);
            }
        }
    }
}
