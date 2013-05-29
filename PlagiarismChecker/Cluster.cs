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

    public class Cluster
    {
        private List<string> files = new List<string>();
        private int size;
        private UnionSet graph;
        private Form1 baseForm;
        public Cluster(List<string> fin)
        {
            files = fin;
            size = files.Count;
        }
        public Cluster(List<string> fin, Form1 _base)
        {
            files = fin;
            size = files.Count;
            baseForm = _base;
        }
        public Cluster()
        {
        }
        private List<SimilarityData> calTheFile(string fa)
        {
            List<SimilarityData> ans = new List<SimilarityData>();
            string lcode = StringFunctions.readFile(StringFunctions.foldToFileName(fa));
            string linit = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(fa)));
            string lasm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(fa)));
            string llex = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(fa)));
            for (int i = 0; i < files.Count; ++i)
            {
                string code = StringFunctions.readFile(StringFunctions.foldToFileName(files[i]));
                string initcode = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(files[i])));
                string lexcode = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(files[i])));
                string asm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(files[i])));
                ans.Add(new SimilarityData(StringFunctions.calSim(lcode, code),
                                           StringFunctions.calSim(linit, initcode),
                                           StringFunctions.calSim(llex, lexcode),
                                           StringFunctions.calSim(lasm, asm)));
            }
            return ans;
        }
        private List<SimilarityData> calTheFile(string fa, int from)
        {
            List<SimilarityData> ans = new List<SimilarityData>();
            for (int i = 0; i < from; ++i) ans.Add(new SimilarityData());
            string lcode = StringFunctions.readFile(StringFunctions.foldToFileName(fa));
            string linit = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(fa)));
            string lasm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(fa)));
            string llex = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(fa)));
            for (int i = from; i < files.Count; ++i)
            {
                string code = StringFunctions.readFile(StringFunctions.foldToFileName(files[i]));
                string initcode = StringFunctions.readFile(StringFunctions.fileCToI(StringFunctions.foldToFileName(files[i])));
                string lexcode = StringFunctions.readFile(StringFunctions.fileCToLex(StringFunctions.foldToFileName(files[i])));
                string asm = StringFunctions.readFile(StringFunctions.fileCToASM(StringFunctions.foldToFileName(files[i])));
                ans.Add(new SimilarityData(StringFunctions.calSim(lcode, code),
                                           StringFunctions.calSim(linit, initcode),
                                           StringFunctions.calSim(llex, lexcode),
                                           StringFunctions.calSim(lasm, asm)));
            }
            return ans;
        }
        public void runCluster()
        {
            baseForm.setStatusText("聚类中");
            graph = new UnionSet(size);
            for (int i = 0; i < size; ++i)
            {
                baseForm.setProgressBar((int)(i * 1000 / size));
                List<SimilarityData> edge = calTheFile(files[i], i + 1);
                for (int j = i + 1; j < size; ++j)
                    if (edge[j].isSimilar()) graph.mergeUnionSet(i, j);
            }
            List<int>[] countResult = graph.count();
            List<List<string>> clusterResult = new List<List<string>>();
            int tot = 0;
            for (int i = 0; i < size; ++i)
                if (countResult[i].Count > 1)
                {
                    ++tot;
                    List<string> temp = new List<string>();
                    for (int j = 0; j < countResult[i].Count; ++j)
                        temp.Add(files[countResult[i][j]]);
                    clusterResult.Add(temp);
                }
            baseForm.setProgressBar(1000);
            ClusterResult newForm = new ClusterResult(clusterResult);
            newForm.Show();
            baseForm.setStatusText("聚类完毕");
        }
    }

}
