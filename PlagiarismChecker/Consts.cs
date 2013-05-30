using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlagiarismChecker
{
    #region consts
    /// <summary>
    /// 常量列表
    /// </summary>
    public class Consts
    {
        /// <summary>
        /// 默认路径
        /// </summary>
        public static string STARTPATH = @"F:\";
        /// <summary>
        /// 默认打开窗口的提示信息
        /// </summary>
        public static string DIALOGDESCRIPTION = "请选择文件路径";
        /// <summary>
        /// 文件名的关键词列表
        /// </summary>
        public static List<string> KEYWORDS = new List<string>()
        {
            "BCPC", "Accepted"
        };
        /// <summary>
        /// 内存相似度阈值
        /// </summary>
        public static double MEMTHRESHOLD = 0.99;
        /// <summary>
        /// 运行时间相似度阈值
        /// </summary>
        public static double TIMETHRESHOLD = 0.99;
        /// <summary>
        /// 文本相似度阈值
        /// </summary>
        public static double TEXTTHRESHOLD = 0.8;
        public static long PRIME = (long)1111111111111111111;
        public static int HASHSTEP = 5;
        public static int MAXFILES = 1024;
        public static string GCCROUTE = @"E:\Program Files (x86)\CodeBlocks\MinGW\bin\";
        public static string GCCADDROUTE = @"\MinGW\bin\";
        public static int MAXNODE = 1 << 20;
        public static double SIMTHRESHOLD_MID = 0.75;
        public static double SIMTHRESHOLD_HIGH = 0.95;
        public static double SIMTHRESHOLD_LOW = 0.5;
        public static int MAXSIMGROUPSIZE = 10;
    }
    #endregion
}
