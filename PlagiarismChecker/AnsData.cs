using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlagiarismChecker
{
    #region AnsData
    /// <summary>
    /// 用户数据类，保存通过分析文件名得出的各个信息，以及不同数据之间的相似度计算
    /// 专门针对acm.buaa.edu.cn的提交日志进行分析，文件名格式如下[举例说明]
    /// [][70472][2012-12-15 12.07.36][buaa_gg][Wrong Answer-0.00][3MS-976KB][c++]
    /// 如文件名较为简单可自行分析
    /// </summary>
    public class AnsData : IComparable
    {
        /// <summary>
        /// 数据源，一般为空白或ip
        /// </summary>
        public string source;
        /// <summary>
        /// 提交日期
        /// </summary>
        public string date;
        /// <summary>
        /// 提交用户名
        /// </summary>
        public string user;
        /// <summary>
        /// 测试结果，一般为Accepted,Wrong Answer等
        /// </summary>
        public string result;
        /// <summary>
        /// 程序的运行时间
        /// </summary>
        public int time;
        /// <summary>
        /// 程序运行时占用的内存大小
        /// </summary>
        public int memory;
        /// <summary>
        /// 程序使用的语言，一般为C++,C,Java或Python
        /// </summary>
        public string language;
        /// <summary>
        /// 完整的文件名
        /// </summary>
        public string fullfilename;
        /// <summary>
        /// 代码文件的原始大小
        /// </summary>
        public long size;
        /// <summary>
        /// 代码总长度
        /// </summary>
        public string text;
        /// <summary>
        /// 自定义类型实例化
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="filesize">文件大小</param>
        /// <param name="fullname">带路径的文件名，用以提取各种信息</param>
        public AnsData(string filename, long filesize, string fullname)
        {
            string[] para = StringFunctions.mysplit(filename);
            source = para[0];
            date = para[2];
            user = para[3];
            result = StringFunctions.splitagain(para[4])[0];
            time = StringFunctions.stringtoint(StringFunctions.splitagain(para[5])[0]);
            memory = StringFunctions.stringtoint(StringFunctions.splitagain(para[5])[1]);
            language = para[6];
            fullfilename = fullname;
            size = filesize;
            StreamReader sr = new StreamReader(fullfilename, System.Text.Encoding.Default);
            text = StringFunctions.deleteEmpty(sr.ReadToEnd());
            sr.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        public AnsData()
        {
        }
        /// <summary>
        /// 输出数据信息
        /// </summary>
        public void print()
        {
            Console.WriteLine(fullfilename + "\t" + memory + "\t" + size);
        }
        /// <summary>
        /// 自定义比较函数，用以排序等工作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            int res = 0;
            try
            {
                AnsData ansobj = (AnsData)obj;
                if (this.memory > ansobj.memory) res = 1;
                else if (this.memory < ansobj.memory) res = -1;
                else
                {
                    if (this.size > ansobj.size) res = 1;
                    else if (this.size < ansobj.size) res = -1;
                }
            }
            catch (Exception e)
            {
                throw new Exception("compare error", e.InnerException);
            }
            return res;
        }
        /// <summary>
        /// 相似度计算方法，代码核心
        /// 综合两种文本相似度计算方法，召回率更高
        /// </summary>
        /// <param name="other">另一个自定义类型</param>
        /// <returns>计算得到的相似度，在[0..1]范围内</returns>
        public double calsim(AnsData other)
        {
            if (language != other.language) return 0;
            double memsim = (double)Math.Min(other.memory, memory) / (double)Math.Max(other.memory, memory);
            double timesim = (double)Math.Min(other.time, time) / (double)Math.Max(other.time, time);
            if (memsim < Consts.MEMTHRESHOLD || timesim < Consts.TIMETHRESHOLD) return memsim * timesim * 0.5;
            double textsim = Math.Max(StringFunctions.calstringsim2(other.text, text), StringFunctions.calstringsim(other.text, text));
            return textsim * memsim * timesim;
        }
    }
    #endregion
}
