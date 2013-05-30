using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PlagiarismChecker
{
    public static class BuaaData
    {
        public static void initData(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                FileInfo tmp = new FileInfo(file);
                string allFileText = tmp.Name + tmp.Extension;
                string[] data = allFileText.Split('[', ']');
                string text = FileOpr.readFile(file);
                string newFileName = data[3] + "_" + data[7];
                string[] route = file.Split('\\');
                File.Delete(file);
                if (file.IndexOf("Accepted") >= 0)
                {
                    if (file.IndexOf("[c]") >= 0)
                    {
                        newFileName += ".c";
                    }
                    else if (file.IndexOf("[c++]") >= 0)
                    {
                        newFileName += ".cpp";
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                route[route.Length - 1] = newFileName;
                string newFullFile = route[0];
                for (int i = 1; i < route.Length; ++i)
                    newFullFile += "\\" + route[i];
                FileOpr.saveToFile(text, newFullFile);
            }
        }
    }
}
