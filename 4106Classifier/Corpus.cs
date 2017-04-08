using Iveonik.Stemmers;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    /*
     * articles [
     *  {
     *      "bias": "l",
     *      "sentences": [
     *          ["words", "in", "here"]
     *      ]
     *  }
     * ]
     */

    class EmbeddingCorpus : Corpus {
        private static IStemmer Stemmer = new EnglishStemmer();

        public const double SubSamplingRate = 0.00001;
        public const double SubSamplingPower = 3 / 4;

        public List<List<string>> Sentences { get; set; }
        public Dictionary<string, double> Frequencies { get; set; }
        public double NegativeSamplingSum;
        public int Count;

        public EmbeddingCorpus(string path) : base(path) {
            Load(path);
        }

        /// <summary>
        /// Load from database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public new void Load(string path) {
            Sentences = new List<List<string>>();
            Frequencies = new Dictionary<string, double>();
            Count = 0;
            NegativeSamplingSum = 0;

            // Load sentences
            foreach (Article article in Articles) {
                foreach (List<string> sentence in article.Sentences) {
                    for (int word = 0; word < sentence.Count; word++) {
                        sentence[word] = Stemmer.Stem(sentence[word]);
                        Frequencies[sentence[word]] = Frequencies.ContainsKey(sentence[word]) ? Frequencies[sentence[word]] + 1 : 1;
                        Count++;
                    }

                    Sentences.Add(sentence);
                }
            }

            // Post process frequencies
            foreach (string word in Frequencies.Keys) {
                Frequencies[word] /= Count;
                NegativeSamplingSum += Math.Pow(Frequencies[word], SubSamplingPower);
            }
        }

        /// <summary>
        /// Subsampling test for training
        /// </summary>
        /// <param name="word">Tested word</param>
        /// <returns>True to remove the word</returns>
        public bool SubSampling(string word) {
            double fq = Frequencies[word];
            return (Math.Sqrt(fq / SubSamplingRate) + 1) * (SubSamplingRate / fq) < UniformRandom.Next();
        }

        /// <summary>
        /// Negative sampling test for training
        /// </summary>
        /// <param name="word">Tested word</param>
        /// <returns>True to sample</returns>
        public bool NegativeSampling(string word) {
            double fq = Math.Pow(Frequencies[word], SubSamplingPower);
            return fq / NegativeSamplingSum > UniformRandom.Next();
        }
    }

    /// <summary>
    /// Training corpus
    /// </summary>
    class Corpus {
        public List<Article> Articles { get; set; }
        public Corpus(string path) {
            Load(path);
        }

        /// <summary>
        /// Load from database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Load(string path) {
            using (var db = new LiteDatabase(path)) {
                foreach (Article article in db.GetCollection<Article>("articles").FindAll()) {
                    Articles.Add(article);
                }
            }
        }

        /// <summary>
        /// An article from the database
        /// </summary>
        public class Article {
            public BiasType Bias { get; set; }
            public List<List<string>> Sentences { get; set; }

            public enum BiasType {
                Left, Right
            }
        }
    }
}
