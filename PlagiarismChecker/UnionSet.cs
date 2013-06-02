using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlagiarismChecker
{
    public class UnionSet
    {
        private int[] fa = new int[Consts.MAXNODE];
        private int[] cnt = new int[Consts.MAXNODE];
        private int size;

        public UnionSet(int t_size)
        {
            if (t_size > Consts.MAXNODE)
            {
                return;
            }
            size = t_size;
            for (int i = 0; i < size; ++i)
                fa[i] = i;
        }
        public UnionSet()
        {
        }

        public Cluster Cluster
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    
        public int getAncestor(int v)
        {
            if (fa[v] != v) fa[v] = getAncestor(fa[v]);
            return fa[v];
        }
        public bool hasCommonAncestor(int x, int y)
        {
            int fx = getAncestor(x), fy = getAncestor(y);
            return (fx != fy);
        }
        public bool mergeUnionSet(int x, int y)
        {
            int fx = getAncestor(x), fy = getAncestor(y);
            fa[fx] = fy;
            return (fx != fy);
        }
        public void check()
        {
            for (int i = 0; i < size; ++i)
                fa[i] = getAncestor(i);
        }
        public List<int>[] count()
        {
            check();
            List<int>[] ans = new List<int>[size];
            for (int i = 0; i < size; ++i)
            {
                cnt[i] = 0;
                ans[i] = new List<int>();
            }
            for (int i = 0; i < size; ++i)
            {
                ++cnt[fa[i]];
                ans[fa[i]].Add(i);
            }
            return ans;
        }
    }
}
