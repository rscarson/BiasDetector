using Iveonik.Stemmers;
using LiteDB;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    class AverageEmbedding {
        public static double StepSize = 1;
        public static double Reg = 0.001;

        private static VectorBuilder<double> Vectors = Vector<double>.Build;
        private static MatrixBuilder<double> Matrices = Matrix<double>.Build;
        private static IStemmer Stemmer = new EnglishStemmer();
        
        public Embedding Embedding { get; set; }
        public int CorpusSize { get; set; }
        public Matrix<double> SoftMax { get; set; }
        public Vector<double> Bias { get; set; }
        public Dictionary<string, int> CorpusAppearances { get; set; }

        public AverageEmbedding() {
            CorpusSize = 0;
            CorpusAppearances = new Dictionary<string, int>();
            Embedding = new Embedding();
            Embedding.Load("embedding.db");
            SoftMax = Matrices.Random(Embedding.EmbeddingSize, 2).Map(p => p * 0.01);
            Bias = Vectors.Dense(2);
        }

        /// <summary>
        /// Weighted average of all embeddings
        /// </summary>
        /// <param name="document">List of words</param>
        /// <returns>value</returns>
        public Vector<double> WeightedAverage(List<string> document) {
            Vector<double> avg = Vectors.Dense(Embedding.EmbeddingSize);
            foreach (string word in document) {
                double weight = Tf_Idf(word, document);
                avg += Embedding.Lookup(word) * weight;
            }
            
            return avg / document.Count;
        }

        /// <summary>
        /// Average embedding learning
        /// </summary>
        /// <param name="bias"></param>
        public void Train(Article.BiasType bias) {
            Corpus corpus = new Corpus("corpus.db");
            var articles = corpus.Articles.OrderBy(p => 0.5 > UniformRandom.Next()).ToList();

            List<Article> testing = new List<Article>(), training = new List<Article>();   
            for (int i = 0; i < articles.Count(); i++) {
                if (i < articles.Count() / 4) testing.Add(articles[i]);
                else training.Add(articles[i]);
            }

            int t_l = training.Where(a => a.Bias == Article.BiasType.Left).Count();
            int t_r = training.Where(a => a.Bias == Article.BiasType.Right).Count();

            // Counts
            foreach (Article a in training) {
                if (a.Bias != bias) continue;
                CorpusSize++;
                var document = a.Document();
                foreach (string word in document) {
                    string stemmed = Stemmer.Stem(word);
                    CorpusAppearances[stemmed] = CorpusAppearances.ContainsKey(stemmed) ? CorpusAppearances[stemmed] + 1 : 1;
                }
            }

            // Get results
            Matrix<double> X = Matrices.Dense(training.Count, Embedding.EmbeddingSize);
            Dictionary<int, int> correct = new Dictionary<int, int>();
            int Xi = 0;
            foreach (Article a in training) {
                var document = a.Document();
                correct[Xi] = (a.Bias == bias) ? 0 : 1;
                X.SetRow(Xi++, WeightedAverage(document));
            }

            for (int iteration = 0; iteration < 10000; iteration++) {
                var scores = X * SoftMax;
                for (int i = 0; i < scores.RowCount; i++) {
                    var row = scores.Row(i);
                    scores.SetRow(i, row + Bias);
                }

                var probs = scores.Clone().PointwiseExp();
                for (int i = 0; i < probs.RowCount; i++) {
                    if (Single.IsInfinity((float)probs[i, 0])) {
                        probs[i, 0] = 1; probs[i, 1] = 0;
                    } else if (Single.IsInfinity((float)probs[i, 1])) {
                        probs[i, 1] = 1; probs[i, 0] = 0;
                    } else {
                        var row = probs.Row(i);
                        double sum = row.Sum();
                        probs.SetRow(i, row / sum);
                    }
                }

                var dscores = probs.Clone();
                for (int i = 0; i < dscores.RowCount; i++) {
                 dscores[i, correct[i]] -= 1;
                }
                dscores /= training.Count;

                double data_loss = 0;
                for (int i = 0; i < probs.RowCount; i++) {
                    double c = probs.Row(i)[correct[i]];
                    c = -Math.Log(c);
                    data_loss += c;
                }
                data_loss /= training.Count;
                var reg_loss = 0.5 * Reg * (SoftMax.PointwiseMultiply(SoftMax)).ColumnSums().Sum();
                var loss = data_loss + reg_loss;
                if (iteration % 10 == 0) {
                    Console.WriteLine(string.Format("Iter. {0}: {1}", iteration, loss));
                }

                var dW = X.Transpose() * dscores;
                var dB = dscores.ColumnSums();
                dW += Reg * SoftMax;

                SoftMax += -StepSize * dW;
                Bias += -StepSize * dB;
            }

            Save("avgembedding.db");

            int right = 0;
            foreach (Article a in testing) {
                double p = Probability(a.Document());
                bool ok = (a.Bias == bias && p > 0.5) || (a.Bias != bias && p < 0.5);
                if (ok) right++;
                Console.WriteLine(string.Format("{0:0.00%} Chance of match... {1}", p, ok ? "Ok." : "Wrong!"));
            }
            double pc = right / (double)testing.Count;
            Console.WriteLine(string.Format("\n{0:0.00%} accuracy.", pc));

        }

        /// <summary>
        /// Classification result
        /// </summary>
        /// <param name="document">input document</param>
        /// <returns>Probability of match</returns>
        public double Probability(List<string> document) {
            var result = WeightedAverage(document);
            var scores = result.ToRowMatrix() * SoftMax + Bias.ToRowMatrix();
            double sum = scores.PointwiseExp().ColumnSums().Sum();

            return Math.Exp(scores[0,0]) / sum;
        }

        /// <summary>
        /// Save current database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Save(string path) {
            System.IO.File.Delete(path);
            using (var db = new LiteDatabase(path)) {
                var collection = db.GetCollection<Saveable>("classifier");
                var s = new Saveable(this);
                collection.Insert(s);
            }
        }

        /// <summary>
        /// Load a stored embedding map
        /// </summary>
        /// <param name="path">Path to the database</param>
        public static AverageEmbedding Load(string path) {
            using (var db = new LiteDatabase(path)) {
                var collection = db.GetCollection<Saveable>("classifier");
                return collection.FindAll().First().ToClassifier();
            }
        }

        /// <summary>
        /// TF-IDF calculation
        /// </summary>
        /// <param name="word">Word to count</param>
        /// <param name="document">Target document</param>
        /// <returns>TF-IDF factor</returns>
        public double Tf_Idf(string word, List<string> document) {
            return TermFrequency(word, document) * InverseDocumentFrequency(word);
        }

        /// <summary>
        /// Logarithmically scaled tf
        /// </summary>
        /// <param name="word">Word to count</param>
        /// <param name="document">Target document</param>
        /// <returns>TF value</returns>
        public double TermFrequency(string word, List<string> document) {
            string stemmed = Stemmer.Stem(word);
            int fq = document.Where(w => Stemmer.Stem(w) == stemmed).Count();

            double tf = (fq == 0) ? 0 : 1 + Math.Log(fq);
            return tf;
        }

        /// <summary>
        /// Idf calculation
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public double InverseDocumentFrequency(string word) {
            string stemmed = Stemmer.Stem(word);
            int appearances = (CorpusAppearances.ContainsKey(stemmed)) ? CorpusAppearances[stemmed] : 0;
            double idf = Math.Log(CorpusSize / (double)Math.Abs(appearances + 1));
            return idf;
        }

        public class Saveable {
            public int CorpusSize { get; set; }
            public double[][] W { get; set; }
            public double[] B { get; set; }
            public Dictionary<string, int> CorpusAppearances { get; set; }

            public Saveable() { }

            public Saveable(AverageEmbedding c) {
                CorpusSize = c.CorpusSize;
                W = c.SoftMax.ToColumnArrays();
                B = c.Bias.ToArray();
                CorpusAppearances = new Dictionary<string, int>(c.CorpusAppearances);
            }

            public AverageEmbedding ToClassifier() {
                var c = new AverageEmbedding();
                c.CorpusSize = CorpusSize;
                c.SoftMax = Matrices.DenseOfColumnArrays(W);
                c.Bias = Vectors.DenseOfArray(B);
                c.CorpusAppearances = CorpusAppearances;

                return c;
            }
        }
    }
}
