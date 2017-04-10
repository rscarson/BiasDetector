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

    /// <summary>
    /// Corpus variant for embeddings
    /// </summary>
    class EmbeddingCorpus : Corpus {
        private static IStemmer Stemmer = new EnglishStemmer();

        public const double SubSamplingRate = 0.00001;
        public const double SubSamplingPower = 3.0 / 4.0;

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
            var _frequencies = new Dictionary<string, double>();
            foreach (string word in Frequencies.Keys) {
                _frequencies[word] = Frequencies[word] / Count;
                NegativeSamplingSum += Math.Pow(Frequencies[word], SubSamplingPower);
            }
            Frequencies = _frequencies;
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

        public Corpus() {
            Articles = new List<Article>();
        }

        public Corpus(string path) : this() {
            Load(path);
        }

        /// <summary>
        /// Load from database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Load(string path) {
            using (var db = new LiteDatabase(path)) {
                foreach (Article article in db.GetCollection<Article>("articles").FindAll()) {
                    if (article.Sentences.Count > 0)
                        Articles.Add(article);
                }
            }
        }

        /// <summary>
        /// Write the corpus database to disk
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Save(string path) {
            System.IO.File.Delete(path);
            using (var db = new LiteDatabase(path)) {
                var collection = db.GetCollection<Article>("articles");
                foreach (Article article in Articles) {
                    collection.Insert(article);
                }
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

        /// <summary>
        /// Squish all the sentences together
        /// </summary>
        /// <returns>squish</returns>
        public List<string> Document() {
            List<string> doc = new List<string>();
            foreach (var sentence in Sentences)
                doc.AddRange(sentence);
            return doc;
        }

        /// <summary>
        /// Sources of left biased training data
        /// </summary>
        public static readonly string[] LeftSources = {
            "/r/politics",
            "/r/Liberal",
            "/r/canada"
        };

        /// <summary>
        /// Sources of right biased training data
        /// </summary>
        public static readonly string[] RightSources = {
            "/r/conservative",
            "/r/metacanada",
            "/r/The_Donald",
            "/r/conservatives",
            "/r/Republican",
            "/r/conservatism",
        };
    }
}
