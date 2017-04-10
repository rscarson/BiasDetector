using Iveonik.Stemmers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace _4106Classifier {
    class Embedding {
        public const int EmbeddingSize = 50;
        public const int MinCount = 10;
        public const int MaxExp = 6;
        public const double StartingAlpha = 0.025;

        private static VectorBuilder<double> Vectors = Vector<double>.Build;
        private static MatrixBuilder<double> Matrices = Matrix<double>.Build;
        private static IStemmer Stemmer = new EnglishStemmer();
        
        public Dictionary<string, Vector<double>> Vocabulary { get; set; }
        public Dictionary<string, Vector<double>> Output { get; set; }
        public Vector<double> Error;
        public double Alpha;

        public Embedding() {
            Vocabulary = new Dictionary<string, Vector<double>>();
            Output = new Dictionary<string, Vector<double>>();
            Error = Vectors.Dense(EmbeddingSize);
            Alpha = StartingAlpha;
        }

        /// <summary>
        /// Look up a word in the embedding vocabulary
        /// </summary>
        /// <param name="word">Input word</param>
        /// <returns>Resulting word vector</returns>
        public Vector<double> Lookup(string word) {
            word = Stemmer.Stem(word);
            if (!Vocabulary.ContainsKey(word))
                Add(word);

            return Vocabulary[word];
        }

        /// <summary>
        /// Add a new word to the embedding
        /// </summary>
        /// <param name="word">Word to add</param>
        public void Add(string word) {
            Vocabulary[word] = Vectors.Random(EmbeddingSize, UniformRandom.Distribution);
            Output[word] = Vectors.Random(EmbeddingSize, UniformRandom.Distribution);
        }

        /// <summary>
        /// Train the model on a given corpus
        /// </summary>
        /// <param name="path">Path to the corpus database</param>
        public void Train(string path) {
            EmbeddingCorpus corpus = new EmbeddingCorpus(path);

            // Pre-add all words
            foreach (List<string> sentence in corpus.Sentences) {
                foreach (string word in sentence) {
                    Add(word);
                }
            }
            
            // Learning
            foreach (List<string> sentence in corpus.Sentences) {
                var _sentence = new List<String>(sentence.Where(w => !corpus.SubSampling(w)));
                var _grams = BiGram.FromSentence(_sentence);

                foreach (var gram in _grams) {
                    Output[gram.Right] = Vectors.Random(EmbeddingSize, UniformRandom.Distribution);

                    // Positive sample
                    UpdateSample(gram.Left, gram.Right, true);

                    // Update negative samples
                    foreach (string word in Vocabulary.Keys) {
                        if (word == gram.Left || !corpus.NegativeSampling(word)) continue;
                        UpdateSample(gram.Left, word, false);
                    }

                    // Update hidden layer
                    for (int i = 0; i < EmbeddingSize; i++) {
                        Vocabulary[gram.Left][i] += Error[i];
                    }
                }
            }
        }

        /// <summary>
        /// Update weights for a sample
        /// </summary>
        /// <param name="left">Context word</param>
        /// <param name="right">New word</param>
        /// <param name="positive">True if positive example</param>
        private void UpdateSample(string left, string right, bool positive) {
            double f = Vocabulary[left].DotProduct(Output[right]);
            double g = 0;

            if (f > MaxExp)
                g = positive ? 0 : -Alpha;
            else if (f < -MaxExp)
                g = positive ? Alpha : 0;
            else g = ActivationFunction(f) * Alpha;

            // Update error and output layers
            for (int i = 0; i < EmbeddingSize; i++) {
                Error[i] += g * Output[right][i];
                Output[right][i] += g * Vocabulary[left][i];
            }
        }

        /// <summary>
        /// Activation function for training
        /// </summary>
        /// <param name="x">Input</param>
        /// <returns>sigmoid function</returns>
        private double ActivationFunction(double x) {
            return 1 / (1 + Math.Exp(-x));
        }

        /// <summary>
        /// Save current database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Save(string path) {
            System.IO.File.Delete(path);
            using (var db = new LiteDatabase(path)) {
                var collection = db.GetCollection<EmbeddedWord>("vocabulary");
                foreach (var word in Vocabulary)
                    collection.Insert(new EmbeddedWord(word.Key, word.Value));
            }
        }

        /// <summary>
        /// Load a stored embedding map
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Load(string path) {
            using (var db = new LiteDatabase(path)) {
                var collection = db.GetCollection<EmbeddedWord>("vocabulary");
                foreach (var word in collection.FindAll())
                    if (!Vocabulary.ContainsKey(word.Word))
                        Vocabulary.Add(word.Word, Vectors.DenseOfArray(word.Vector));
            }
        }

        /// <summary>
        /// Create the embedding database
        /// </summary>
        /// <param name="path">Path to the database</param>
        public void Generate(string path) {
            Crawler crawler = new Crawler();
            Corpus corpus = new Corpus();

            // Left bias sources
            foreach (string source in Article.LeftSources) {
                foreach (Article article in crawler.Articles(source, Article.BiasType.Left)) {
                    corpus.Articles.Add(article);
                }
            }

            // Right bias sources
            foreach (string source in Article.RightSources) {
                foreach (Article article in crawler.Articles(source, Article.BiasType.Right)) {
                    corpus.Articles.Add(article);
                }
            }

            corpus.Save("corpus.db");
            Train("corpus.db");
            Save(path);
        }

        /// <summary>
        /// Class holding the bi-grams for training
        /// </summary>
        public class BiGram {
            public const int WindowSize = 5;

            public string Left { get; set; }
            public string Right { get; set; }

            public BiGram(string left, string right) {
                Left = left; Right = right;
            }

            public new string ToString() {
                return string.Format("[{0}, {1}]", Left, Right);
            }

            /// <summary>
            /// Get a list of 1-skip-bigrams from a sentence
            /// </summary>
            /// <param name="sentence">Raw input sentence</param>
            /// <returns>Set of grams</returns>
            public static List<BiGram> FromSentence(List<string> sentence) {
                List<BiGram> grams = new List<BiGram>();
                for (int i = 0; i < sentence.Count - 1; i++) {
                    string left = sentence[i];
                    for (int j = i + 1; j < sentence.Count && j - i <= WindowSize; j++) {
                        string right = sentence[j];
                        grams.Add(new BiGram(left, right));
                    }
                }

                return grams;
            }
        }

        /// <summary>
        /// For saving and reloading database status
        /// </summary>
        public class EmbeddedWord {
            public string Word { get; set; }
            public double[] Vector { get; set; }

            public EmbeddedWord() { }

            public EmbeddedWord(string word, Vector<double> vector) {
                Word = word; Vector = vector.ToArray();
            }
        }
    }
}
