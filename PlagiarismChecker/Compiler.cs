using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace PlagiarismChecker
{
    public static class Compiler
    {
        public static string dumpCode(string filename)
        {
            filename = StringFunctions.fileCToO(filename);
            Process objdump = new Process();
            objdump.StartInfo.FileName = System.Environment.CurrentDirectory + Consts.GCCADDROUTE + "objdump.exe";
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
        public static string getASM(string filename)
        {
            compile(filename);
            return dumpCode(filename);
        }
        public static void compile(string filename)
        {
            string ext = StringFunctions.extention(filename);
            Process compiler = new Process();
            if (ext == "c")
            {
                compiler.StartInfo.FileName = System.Environment.CurrentDirectory + Consts.GCCADDROUTE + "gcc.exe";
            }
            else if (StringFunctions.isCpp(ext))
            {
                compiler.StartInfo.FileName = System.Environment.CurrentDirectory + Consts.GCCADDROUTE + "g++.exe";
            }
            compiler.StartInfo.Arguments = "-c -O3 " + filename + " -o " + StringFunctions.fileCToO(filename);
            compiler.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.CreateNoWindow = true;
            compiler.Start();
            compiler.WaitForExit();
            compiler.Close();
        }
        public static void init(string filename)
        {
            string ext = StringFunctions.extention(filename);
            Process init = new Process();
            if (ext == "c")
            {
                init.StartInfo.FileName = System.Environment.CurrentDirectory + Consts.GCCADDROUTE + "gcc.exe";
            }
            else if (StringFunctions.isCpp(ext))
            {
                init.StartInfo.FileName = System.Environment.CurrentDirectory + Consts.GCCADDROUTE + "g++.exe";
            }
            init.StartInfo.Arguments = " -E " + filename + " -o " + StringFunctions.fileCToI(filename);
            init.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            init.StartInfo.UseShellExecute = false;
            init.StartInfo.CreateNoWindow = true;
            init.Start();
            init.WaitForExit();
            init.Close();
            StreamReader sr = null;
            string code = "";
            try
            {
                sr = new StreamReader(StringFunctions.fileCToI(filename), System.Text.Encoding.Default);
                code = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e)
            {
                //MessageBox.Show("由于编译错误，未能成功转换！");
            }
            String[] lines = code.Split('\n');
            int lastsharp = 0, index = 0;
            code = "";
            foreach (string line in lines)
            {
                if (line.Length > 0 && line[0] == '#') lastsharp = index;
                ++index;
            }
            index = 0;
            foreach (string line in lines)
            {
                if (line.Trim().Length > 0 && index > lastsharp)
                {
                    code = code + line + '\n';
                }
                ++index;
            }
            FileOpr.saveToFile(code, StringFunctions.fileCToI(filename));
        }
    }
}
