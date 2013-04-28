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
        CLexer cl = new CLexer();
        double[] codeSimilarity = new double[Consts.MAXFILES];
        double[] initSimilarity = new double[Consts.MAXFILES];
        double[] lexSimilarity = new double[Consts.MAXFILES];
        double[] asmSimilarity = new double[Consts.MAXFILES];
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            InitStylesPriority();
        }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "选择左侧代码";
            fileDialog.Filter = "C语言代码|*.c";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(fileDialog.FileName, System.Text.Encoding.Default);
                leftText.Text = sr.ReadToEnd();
                textBox2.Text = fileDialog.FileName;
            }
            initLeftCode(leftText.Text);
           /* fileDialog = new OpenFileDialog();
            fileDialog.Title = "选择右侧代码";
            fileDialog.Filter = "C语言代码|*.c";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(fileDialog.FileName, System.Text.Encoding.Default);
                rightText.Text = sr.ReadToEnd();
            }*/
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            /*rightText.VisibleRange.ClearStyle(SameWordsStyle);
            string text = leftText.SelectedText;
            if (text.Length == 0) return;
            var ranges = rightText.VisibleRange.GetRanges(System.Text.RegularExpressions.Regex.Unescape(text), RegexOptions.Multiline).ToArray();
            foreach (var r in ranges)
            {
                r.SetStyle(SameWordsStyle);
            }*/
            rightText.Text = readFile("a.l");
        }
        /// <summary>
        /// 反汇编按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            /*Process objdump = new Process();
            objdump.StartInfo.FileName = @"E:\Program Files (x86)\CodeBlocks\MinGW\bin\objdump.exe";
            objdump.StartInfo.Arguments = "-d a.o";
            objdump.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            objdump.StartInfo.RedirectStandardOutput = true;
            objdump.StartInfo.UseShellExecute = false;
            objdump.StartInfo.CreateNoWindow = true;
            objdump.Start();
            string output = objdump.StandardOutput.ReadToEnd();
            objdump.WaitForExit();
            int n = objdump.ExitCode;
            objdump.Close();
            rightText.Text = output;*/
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
                listBox1.Items.Clear();
                foreach (string str in result)
                {
                    listBox1.Items.Add(str);
                }
                textBox1.Text = foldPath;
                build();
            }
        }
        private string readFile(string filename)
        {
            StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);
            string resuilt = sr.ReadToEnd();
            sr.Close();
            return resuilt;
        }
        private void initFile(string filename)
        {
            string temp = StringFunctions.foldToFileName(filename);
            string code = readFile(filename);
            saveToFile(code, temp);
            string asm = getASM(temp);
            saveToFile(asm, StringFunctions.fileCToASM(temp));
            CLexer nowC = new CLexer(code);
            string initcode = nowC.getInitCode();
            string lexcode = nowC.GetLexResult();
            saveToFile(initcode, StringFunctions.fileCToI(temp));
            saveToFile(lexcode, StringFunctions.fileCToLex(temp));
        }
        private void initLeftCode(string code)
        {
            saveToFile(code, "a.c");
            string asm = getASM("a.c");
            saveToFile(asm, "a.a");
            CLexer nowC = new CLexer(code);
            string initcode = nowC.getInitCode();
            string lexcode = nowC.GetLexResult();
            saveToFile(initcode, "a.i");
            saveToFile(lexcode, "a.l");
        }
        /// <summary>
        /// 对列表中的文件预处理
        /// </summary>
        private void build()
        {
            string[] tx = new string[listBox1.Items.Count];
            int cnt = 0;
            if (leftText.Text == "") 
            {
                MessageBox.Show("请先选择待测代码！");
                return ;
            }
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("请先选择文件夹！");
                return;
            }
            string lcode = readFile("a.c");
            string linit = readFile("a.i");
            string lasm = readFile("a.a");
            string llex = readFile("a.l");
            for (int i = 0; i < listBox1.Items.Count; ++i)
            {
                string str = listBox1.Items[i].ToString();
                string temp = StringFunctions.foldToFileName(str);
                string code = readFile(str);
                saveToFile(code, temp);
                string asm = getASM(temp);
                saveToFile(asm, StringFunctions.fileCToASM(temp));
                CLexer nowC = new CLexer(code);
                string initcode = nowC.getInitCode();
                string lexcode = nowC.GetLexResult();
                saveToFile(initcode, StringFunctions.fileCToI(temp));
                saveToFile(lexcode, StringFunctions.fileCToLex(temp));
                ++cnt;
                codeSimilarity[i] = StringFunctions.calSim(lcode, code);
                initSimilarity[i] = StringFunctions.calSim(linit, initcode);
                lexSimilarity[i] = StringFunctions.calSim(llex, lexcode);
                asmSimilarity[i] = StringFunctions.calSim(lasm, asm);
                if (codeSimilarity[i] > 0.75 || initSimilarity[i] > 0.75 || lexSimilarity[i] > 0.8 || asmSimilarity[i] > 0.93)
                {
                    tx[cnt-1] = listBox1.Items[i].ToString();
                    codeSimilarity[cnt - 1] = codeSimilarity[i];
                    initSimilarity[cnt - 1] = initSimilarity[i];
                    lexSimilarity[cnt - 1] = lexSimilarity[i];
                    asmSimilarity[cnt - 1] = asmSimilarity[i];
                }
                else --cnt;
            }
            listBox1.Items.Clear();
            for (int i = 0; i < cnt; ++i)
                listBox1.Items.Add(tx[i]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            rightText.Text = readFile("a.i");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
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
            /*saveToFile(leftText.Text);
            Process compiler = new Process();
            compiler.StartInfo.FileName = @"E:\Program Files (x86)\CodeBlocks\MinGW\bin\gcc.exe";
            compiler.StartInfo.Arguments = "-S a.i -o a.s";
            compiler.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.CreateNoWindow = true;
            compiler.Start();
            compiler.WaitForExit();
            StreamReader sr = new StreamReader("a.s", System.Text.Encoding.Default);
            rightText.Text = sr.ReadToEnd();*/
            rightText.Text = readFile("a.a");
            //rightText.Text = StringFunctions.formatASM(rightText.Text);
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*StreamReader sr = new StreamReader(listBox1.SelectedItem.ToString(), System.Text.Encoding.Default);
            saveToFile(sr.ReadToEnd(), StringFunctions.foldToFileName(listBox1.SelectedItem.ToString()));
            string asm = getASM(listBox1.SelectedItem.ToString());
            rightText.Text = asm;*/
            //rightText.Text = sr.ReadToEnd();
            int index = listBox1.SelectedIndex;
            if (index < 0) return;
            label4.Text = StringFunctions.doubleToString(codeSimilarity[index]);
            label5.Text = StringFunctions.doubleToString(initSimilarity[index]);
            label6.Text = StringFunctions.doubleToString(lexSimilarity[index]);
            label7.Text = StringFunctions.doubleToString(asmSimilarity[index]);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            leftText.Text = readFile("a.a");
            rightText.Text = readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
        }
        /// <summary>
        /// 比对原代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            leftText.Text = readFile("a.c");
            rightText.Text = readFile(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString()));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            leftText.Text = readFile("a.i");
            rightText.Text = readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            leftText.Text = readFile("a.l");
            rightText.Text = readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(listBox1.SelectedItem.ToString())));
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
            build();
        }

    }
}
