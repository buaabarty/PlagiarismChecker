using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlagiarismChecker
{
    public class SimilarityData
    {
        private double codeSimilarity { get; set; }
        private double initSimilarity { get; set; }
        private double lexSimilarity { get; set; }
        private double asmSimilarity { get; set; }
        public SimilarityData(double s1, double s2, double s3, double s4)
        {
            codeSimilarity = s1;
            initSimilarity = s2;
            lexSimilarity = s3;
            asmSimilarity = s4;
        }
        public SimilarityData() { }
        public bool isSimilar(double threshold)
        {
            return (codeSimilarity > threshold || initSimilarity > threshold || lexSimilarity > threshold || asmSimilarity > threshold);
        }
        public bool isSimilar()
        {
            return isSimilar(Consts.SIMTHRESHOLD_MID);
        }
    }

}
