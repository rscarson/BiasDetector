using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4106Classifier {
    class Program {
        static void Main(string[] args) {
            /*
            Embedding e = new Embedding();
            e.Load("embedding.db");
            Dictionary<string, Vector<double>> copy = new Dictionary<string, Vector<double>>();
            foreach (string word in e.Vocabulary.Keys) {
                copy[word] = e.Vocabulary[word].Map(p => p * 0.01);
            }
            e.Vocabulary = copy;
            e.Save("embedding.db");
            */

            AverageEmbedding classifier = new AverageEmbedding();//.Load("avgembedding.db");
            classifier.Train(Article.BiasType.Right);


            Corpus corpus = new Corpus("corpus.db");
            var articles = corpus.Articles.OrderBy(p => 0.5 > UniformRandom.Next()).ToList();
            int right = 0;
            foreach (Article a in articles) {
                double p = classifier.Probability(a.Document());
                bool ok = (a.Bias == Article.BiasType.Right && p > 0.5) || (a.Bias != Article.BiasType.Right && p < 0.5);
                if (ok) right++;
                Console.WriteLine(string.Format("{0:0.00%} Chance of match... {1}: {2}", p, ok ? "Ok." : "Wrong!", a.Bias));
            }
            double pc = right / (double)articles.Count;
            Console.WriteLine(string.Format("\n{0:0.00%} accuracy.", pc));

            Application.Run();
        }
    }
}
