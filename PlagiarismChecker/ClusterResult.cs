using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlagiarismChecker
{
    public partial class ClusterResult : Form
    {
        private List<List<string>> result;
        public ClusterResult()
        {
            InitializeComponent();
        }
        public ClusterResult(List<List<string>> input)
        {
            InitializeComponent();
            result = input;
            listBox1.Items.Clear();
            for (int i = 0; i < result.Count; ++i)
            {
                string text = string.Format("第{0}组：", i + 1);
                for (int j = 0; j < result[i].Count; ++j)
                    text = text + "\t" + result[i][j];
                listBox1.Items.Add(text);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "";
            int index = listBox1.SelectedIndex;
            for (int i = 0; i < result[index].Count; ++i)
                textBox1.Text += result[index][i] + "\r\n";
        }
    }
}
