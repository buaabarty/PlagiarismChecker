using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlagiarismChecker
{
    #region FileOpr
    /// <summary>
    /// 文件操作类
    /// </summary>
    public class FileOpr
    {
        /// <summary>
        /// 自定义文件遍历方法，根据筛选器进行筛选，并生成自定义数据类型列表
        /// </summary>
        /// <param name="folder">文件夹位置</param>
        /// <returns>遍历得到的文件列表</returns>
        public static List<AnsData> GetAllFiles(string folder)
        {
            DirectoryInfo myfolder = new DirectoryInfo(folder);
            FileInfo[] files = myfolder.GetFiles();
            List<AnsData> ans = new List<AnsData>();
            foreach (FileInfo file in files)
            {
                AnsData now = new AnsData(file.Name, file.Length, file.FullName);
                bool flag = true;
                foreach (string key in Consts.KEYWORDS)
                {
                    if (!now.fullfilename.Contains(key))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) ans.Add(now);
            }
            return ans;
        }
        public static List<String> GetAllFolderFiles(string folder)
        {
            DirectoryInfo myfolder = new DirectoryInfo(folder);
            FileSystemInfo[] files = myfolder.GetFileSystemInfos();
            List<String> ans = new List<String>();
            foreach (FileSystemInfo file in files)
            {
                FileInfo tfile = file as FileInfo;
                if (tfile != null)
                {
                    ans.Add(tfile.FullName);
                }
                else
                {
                    List<String> temp = GetAllFolderFiles(file.FullName);
                    foreach (String str in temp)
                    {
                        ans.Add(str);
                    }
                }
            }
            return ans;
        }
        public static List<String> GetAllFolderFiles(string folder, string[] extension)
        {
            DirectoryInfo myfolder = new DirectoryInfo(folder);
            FileSystemInfo[] files = myfolder.GetFileSystemInfos();
            List<String> ans = new List<String>();
            foreach (FileSystemInfo file in files)
            {
                FileInfo tfile = file as FileInfo;
                if (tfile != null)
                {
                    for (int i = 0; i < extension.Length; ++i)
                        if (tfile.Extension.ToLower() == extension[i].ToLower())
                        {
                            ans.Add(tfile.FullName);
                            break;
                        }
                }
                else
                {
                    List<String> temp = GetAllFolderFiles(file.FullName, extension);
                    foreach (String str in temp)
                    {
                        ans.Add(str);
                    }
                }
            }
            return ans;
        }
        public static int deleteAllExtentionFiles(string folder, string[] ext)
        {
            int totDelete = 0;
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                FileInfo tmp = new FileInfo(file);
                bool find = false;
                for (int i = 0; i < ext.Length; ++i)
                    if (tmp.Extension == ext[i])
                    {
                        find = true;
                        break;
                    }
                if (find)
                {
                    File.Delete(file);
                    ++totDelete;
                }
            }
            return totDelete;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        public static void saveToFile(String Text)
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
                //MessageBox.Show("保存失败，错误为" + ex.Message.ToString());
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
        public static void saveToFile(String Text, String filename)
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
                //MessageBox.Show("保存失败，错误为" + ex.Message.ToString());
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 读文件，返回一个字符串
        /// </summary>
        /// <param name="filename">带路径的文件名</param>
        /// <returns></returns>
        public static string readFile(string filename)
        {
            StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);
            string resuilt = sr.ReadToEnd();
            sr.Close();
            return resuilt;
        }
    }
    #endregion
}
