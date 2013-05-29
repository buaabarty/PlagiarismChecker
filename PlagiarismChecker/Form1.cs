using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using FastColoredTextBoxNS;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PlagiarismChecker
{
    
    public partial class Form1 : Form
    {
        #region Initialization
        TextStyle BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        TextStyle BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        TextStyle GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        TextStyle GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
        TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        #endregion

        CLexer cl = new CLexer();
        double[] codeSimilarity = new double[Consts.MAXFILES];
        double[] initSimilarity = new double[Consts.MAXFILES];
        double[] lexSimilarity = new double[Consts.MAXFILES];
        double[] asmSimilarity = new double[Consts.MAXFILES];
        string leftFileName;

        /// <summary>
        /// 
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            InitStylesPriority();
            toolStripStatusLabel1.Text = "请选择目录~";
        }
        #region UI_Compoment
        /// <summary>
        /// 
        /// </summary>
        private void InitStylesPriority()
        {
            leftText.ClearStylesBuffer();
            leftText.Range.ClearStyle(StyleIndex.All);
            leftText.AddStyle(SameWordsStyle);
            leftText.Language = Language.Custom;
            leftText.CommentPrefix = "//";
            leftText.AutoIndentNeeded += leftText_AutoIndentNeeded;
            leftText.OnTextChanged();
            leftText.OnSyntaxHighlight(new TextChangedEventArgs(leftText.Range));
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftText_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            CSyntaxHighlight(leftText, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fctb"></param>
        /// <param name="e"></param>
        private void CSyntaxHighlight(FastColoredTextBox fctb, TextChangedEventArgs e)
        {
            fctb.LeftBracket = '(';
            fctb.RightBracket = ')';
            fctb.LeftBracket2 = '\x0';
            fctb.RightBracket2 = '\x0';
            //clear style of changed range
            e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);

            //string highlighting
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            //comment highlighting
            e.ChangedRange.SetStyle(GreenStyle, @"//|#.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            //number highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            //attribute highlighting
            e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            //class name highlighting
            e.ChangedRange.SetStyle(BoldStyle, @"\b(struct|enum)\s+(?<range>\w+?)\b");
            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(auto|break|case|char|const|continue|default|do|double|enum|extern|float|for|goto|if|else|int|long|register|return|short|signed|sizeof|static|struct|switch|typedef|union|unsigned|signed|void|volatile|while|restrict|_bool|_Complex|_Imaginary)\b|#region\b|#endregion\b");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightText_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            CSyntaxHighlight(rightText, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftText_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        private void CBilingualHighlight(FastColoredTextBox tb)
        {
            foreach (FastColoredTextBoxNS.Range r in tb.GetRanges(@"{*}", RegexOptions.Singleline))
            {
                r.ClearStyle(StyleIndex.All);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftText_SelectionChangedDelayed(object sender, EventArgs e)
        {
            leftText.VisibleRange.ClearStyle(SameWordsStyle);
            if (!leftText.Selection.IsEmpty) return;
            var fragment = leftText.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0) return;
            var ranges = leftText.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
            {
                foreach (var r in ranges)
                {
                    r.SetStyle(SameWordsStyle);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void leftText_AutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            //block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
                return;
            //start of block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if (Regex.IsMatch(args.LineText, @"}[^""']*$"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //label
            if (Regex.IsMatch(args.LineText, @"^\s*\w+\s*:\s*($|//)") &&
                !Regex.IsMatch(args.LineText, @"^\s*default\s*:"))
            {
                args.Shift = -args.TabLength;
                return;
            }
            //some statements: case, default
            if (Regex.IsMatch(args.LineText, @"^\s*(case|default)\b.*:\s*($|//)"))
            {
                args.Shift = -args.TabLength / 2;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)"))//operator is unclosed
                {
                    args.Shift = args.TabLength;
                    return;
                }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void rightText_AutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            //block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
                return;
            //start of block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if (Regex.IsMatch(args.LineText, @"}[^""']*$"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //label
            if (Regex.IsMatch(args.LineText, @"^\s*\w+\s*:\s*($|//)") &&
                !Regex.IsMatch(args.LineText, @"^\s*default\s*:"))
            {
                args.Shift = -args.TabLength;
                return;
            }
            //some statements: case, default
            if (Regex.IsMatch(args.LineText, @"^\s*(case|default)\b.*:\s*($|//)"))
            {
                args.Shift = -args.TabLength / 2;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)"))//operator is unclosed
                {
                    args.Shift = args.TabLength;
                    return;
                }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightText_SelectionChangedDelayed(object sender, EventArgs e)
        {
            rightText.VisibleRange.ClearStyle(SameWordsStyle);
            if (!rightText.Selection.IsEmpty) return;
            var fragment = rightText.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0) return;
            var ranges = rightText.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
            {
                foreach (var r in ranges)
                {
                    r.SetStyle(SameWordsStyle);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightText_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            CSyntaxHighlight(rightText, e);
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(leftFileName)));
        }
        
        private string dumpCode(string filename)
        {
            filename = StringFunctions.fileCToO(filename);
            Process objdump = new Process();
            objdump.StartInfo.FileName = Consts.GCCROUTE + "objdump.exe";
            objdump.StartInfo.Arguments = "-d " + filename;
            objdump.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            objdump.StartInfo.RedirectStandardOutput = true;
            objdump.StartInfo.UseShellExecute = false;
            objdump.StartInfo.CreateNoWindow = true;
            objdump.Start();
            string output = objdump.StandardOutput.ReadToEnd();
            objdump.WaitForExit();
            int n = objdump.ExitCode;
            objdump.Close();
            output = StringFunctions.formatASM(output);
            return output;
        }
        /// <summary>
        /// 获取汇编代码主接口
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string getASM(string filename)
        {
            compile(filename);
            return dumpCode(filename);
        }
        /// <summary>
        /// 选择目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "选择查找相似代码范围";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                List<String> result = FileOpr.GetAllFolderFiles(foldPath, ".c");
                listBox2.Items.Clear();
                int nowcount = 0;
                toolStripProgressBar1.Value = 0;
                foreach (string str in result)
                {
                    toolStripStatusLabel1.Text = String.Format("正在处理第{0}个文件，共{1}个文件", ++nowcount, result.Count);
                    initFile(str);
                    listBox2.Items.Add(str);
                    toolStripProgressBar1.Value = (int)(nowcount * 1000 / result.Count);
                    statusStrip1.Refresh();
                    listBox2.Refresh();
                }
                toolStripStatusLabel1.Text = String.Format("处理完毕，共{0}个文件", result.Count);
                statusStrip1.Refresh();
            }
        }
        
        private void initFile(string filename)
        {
            string temp = StringFunctions.foldToFileName(filename);
            if (File.Exists(temp)) return;
            string code = StringFunctions.readFile(filename);
            saveToFile(code, temp);
            string asm = getASM(temp);
            saveToFile(asm, StringFunctions.fileCToASM(temp));
            CLexer nowC = new CLexer(code);
            string initcode = nowC.getInitCode();
            string lexcode = nowC.GetLexResult();
            saveToFile(initcode, StringFunctions.fileCToI(temp));
            saveToFile(lexcode, StringFunctions.fileCToLex(temp));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(leftFileName)));
        }
        /// <summary>
        /// 优化编译
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="dest"></param>
        private void compile(string filename)
        {
            Process compiler = new Process();
            compiler.StartInfo.FileName = Consts.GCCROUTE + "gcc.exe";
            compiler.StartInfo.Arguments = "-c -O3 " + filename + " -o " + StringFunctions.fileCToO(filename);
            compiler.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.CreateNoWindow = true;
            compiler.Start();
            compiler.WaitForExit();
            //MessageBox.Show("编译完成！");
            compiler.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        private void saveToFile(String Text)
        {
            FileStream fs = new FileStream("a.c", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                sw.Write(Text);
                sw.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败，错误为" + ex.Message.ToString());
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="filename"></param>
        private void saveToFile(String Text, String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                sw.Write(Text);
                sw.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败，错误为" + ex.Message.ToString());
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(leftFileName)));
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            label4.Text = StringFunctions.doubleToString(codeSimilarity[index]);
            label5.Text = StringFunctions.doubleToString(initSimilarity[index]);
            label6.Text = StringFunctions.doubleToString(lexSimilarity[index]);
            label7.Text = StringFunctions.doubleToString(asmSimilarity[index]);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            leftText.Text = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(leftFileName)));
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
        }
        /// <summary>
        /// 比对原代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            leftText.Text = StringFunctions.readFile(StringFunctions.foldToFileName(leftFileName));
            rightText.Text = StringFunctions.readFile(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString()));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            leftText.Text = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(leftFileName)));
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            leftText.Text = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(leftFileName)));
            rightText.Text = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
        }

        private void swap(ref double x, ref double y)
        {
            double temp = x;
            x = y;
            y = temp;
        }
        private void swap(ref object x, ref object y)
        {
            object temp = x;
            x = y;
            y = temp;
        }

        private void myswap(int x, int y)
        {
            swap(ref codeSimilarity[x], ref codeSimilarity[y]);
            swap(ref initSimilarity[x], ref initSimilarity[y]);
            swap(ref asmSimilarity[x], ref asmSimilarity[y]);
            swap(ref lexSimilarity[x], ref lexSimilarity[y]);
            string temp = listBox1.Items[x] as string;
            listBox1.Items[x] = listBox1.Items[y];
            listBox1.Items[y] = temp;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBox1.SelectedIndex;
            if (index == 0) //按照原代码排序
            {
                for (int i = 0; i < listBox1.Items.Count; ++i)
                    for (int j = i + 1; j < listBox1.Items.Count; ++j)
                        if (codeSimilarity[i] < codeSimilarity[j]) myswap(i, j);
            }
            else if (index == 1) //按照预处理排序
            {
                for (int i = 0; i < listBox1.Items.Count; ++i)
                    for (int j = i + 1; j < listBox1.Items.Count; ++j)
                        if (initSimilarity[i] < initSimilarity[j]) myswap(i, j);
            }
            else if (index == 2) //按照词法树排序
            {
                for (int i = 0; i < listBox1.Items.Count; ++i)
                    for (int j = i + 1; j < listBox1.Items.Count; ++j)
                        if (lexSimilarity[i] < lexSimilarity[j]) myswap(i, j);
            }
            else
            {
                for (int i = 0; i < listBox1.Items.Count; ++i)
                    for (int j = i + 1; j < listBox1.Items.Count; ++j)
                        if (asmSimilarity[i] < asmSimilarity[j]) myswap(i, j);
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //build();            
            calAllSim();
        }

        private void calAllSim()
        {
            string[] tx = new string[listBox2.Items.Count];
            int cnt = 0;
            if (leftText.Text == "")
            {
                MessageBox.Show("请从左侧选择一份待测代码！");
                return;
            }
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("请先选择一个非空文件夹！");
                return;
            }
            string lcode = StringFunctions.readFile(StringFunctions.foldToFileName(leftFileName));
            string linit = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(leftFileName)));
            string lasm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(leftFileName)));
            string llex = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(leftFileName)));
            toolStripProgressBar1.Value = 0;
            for (int i = 0; i < listBox2.Items.Count; ++i)
            {
                string code = StringFunctions.readFile(StringFunctions.foldToFileName(listBox2.Items[i] as string));
                string initcode = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                string lexcode = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                string asm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                ++cnt;
                toolStripStatusLabel1.Text = String.Format("正在计算{0}与第{1}个文件的相似度，共{2}个文件", leftFileName, i + 1, listBox2.Items.Count);
                statusStrip1.Refresh();
                codeSimilarity[i] = StringFunctions.calSim(lcode, code);
                initSimilarity[i] = StringFunctions.calSim(linit, initcode);
                lexSimilarity[i] = StringFunctions.calSim(llex, lexcode);
                asmSimilarity[i] = StringFunctions.calSim(lasm, asm);
                if (codeSimilarity[i] > 0.75 || initSimilarity[i] > 0.75 || lexSimilarity[i] > 0.75 || asmSimilarity[i] > 0.75)
                //if (true)
                {
                    tx[cnt - 1] = listBox2.Items[i].ToString();
                    codeSimilarity[cnt - 1] = codeSimilarity[i];
                    initSimilarity[cnt - 1] = initSimilarity[i];
                    lexSimilarity[cnt - 1] = lexSimilarity[i];
                    asmSimilarity[cnt - 1] = asmSimilarity[i];
                }
                else --cnt;
                toolStripProgressBar1.Value = (int)((i + 1) * 1000 / listBox2.Items.Count);
                statusStrip1.Refresh();
            }
            toolStripStatusLabel1.Text = String.Format("相似度计算完毕，共{0}个代码，其中相似代码{1}个", listBox2.Items.Count, cnt);
            listBox1.Items.Clear();
            for (int i = 0; i < cnt; ++i)
                listBox1.Items.Add(tx[i]);
        }


        private void calAllSim2()
        {
            string[] tx = new string[listBox2.Items.Count];
            int cnt = 0;
            if (leftText.Text == "")
            {
                MessageBox.Show("请从左侧选择一份待测代码！");
                return;
            }
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("请先选择一个非空文件夹！");
                return;
            }
            string lcode = StringFunctions.readFile(StringFunctions.foldToFileName(leftFileName));
            string linit = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(leftFileName)));
            string lasm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(leftFileName)));
            string llex = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(leftFileName)));
            toolStripProgressBar1.Value = 0;
            for (int i = 0; i < listBox2.Items.Count; ++i)
            {
                string code = StringFunctions.readFile(StringFunctions.foldToFileName(listBox2.Items[i] as string));
                string initcode = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                string lexcode = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                string asm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(listBox2.Items[i] as string)));
                ++cnt;
                toolStripStatusLabel1.Text = String.Format("正在计算{0}与第{1}个文件的相似度，共{2}个文件", leftFileName, i + 1, listBox2.Items.Count);
                statusStrip1.Refresh();
                codeSimilarity[i] = StringFunctions.calSim(lcode, code);
                initSimilarity[i] = StringFunctions.calSim(linit, initcode);
                lexSimilarity[i] = StringFunctions.calSim(llex, lexcode);
                asmSimilarity[i] = StringFunctions.calSim(lasm, asm);
                //if (codeSimilarity[i] > 0.75 || initSimilarity[i] > 0.75 || lexSimilarity[i] > 0.8 || asmSimilarity[i] > 0.70)
                if (true)
                {
                    tx[cnt - 1] = listBox2.Items[i].ToString();
                    codeSimilarity[cnt - 1] = codeSimilarity[i];
                    initSimilarity[cnt - 1] = initSimilarity[i];
                    lexSimilarity[cnt - 1] = lexSimilarity[i];
                    asmSimilarity[cnt - 1] = asmSimilarity[i];
                }
                else --cnt;
                toolStripProgressBar1.Value = (int)((i + 1) * 1000 / listBox2.Items.Count);
                statusStrip1.Refresh();
            }
            toolStripStatusLabel1.Text = String.Format("相似度计算完毕，共{0}个代码，其中相似代码{1}个", listBox2.Items.Count, cnt);
            listBox1.Items.Clear();
            for (int i = 0; i < cnt; ++i)
                listBox1.Items.Add(tx[i]);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            leftFileName = listBox2.SelectedItem as string;
            leftText.Text = StringFunctions.readFile(leftFileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calAllSim2();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<string> files = new List<string>();
            for (int i = 0; i < listBox2.Items.Count; ++i) files.Add(listBox2.Items[i] as string);
            Cluster data = new Cluster(files, this);
            data.runCluster();
        }

        public void setStatusText(string text)
        {
            toolStripStatusLabel1.Text = text;
            statusStrip1.Refresh();
        }
        public void setProgressBar(int value)
        {
            toolStripProgressBar1.Value = value;
            statusStrip1.Refresh();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Format("一共删除了{0}个文件", FileOpr.deleteAllExtentionFiles(System.Environment.CurrentDirectory, new string[] { ".c", ".o", ".i", ".a", ".l" }));
        }

    }
}
