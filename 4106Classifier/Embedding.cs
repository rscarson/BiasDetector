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
        [JsonIgnore]
        private static VectorBuilder<double> Vectors = Vector<double>.Build;

        [JsonIgnore]
        private static MatrixBuilder<double> Matrices = Matrix<double>.Build;

        [JsonIgnore]
        private static IStemmer Stemmer = new EnglishStemmer();

        [JsonIgnore]
        public Dictionary<string, Vector<double>> Vocabulary { get; set; }

        public int EmbeddingSize { get; set; } // 50
        public int MinCount { get; set; }

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
            word = Stemmer.Stem(word);
            Vocabulary[word] = Vectors.Random(EmbeddingSize, UniformRandom.Distribution);
        }

        /// <summary>
        /// Train the model on a given corpus
        /// </summary>
        /// <param name="path">Path to the corpus database</param>
        public void Train(string path) {
            Corpus corpus = new Corpus(path);
            var Hidden = Matrices.Random(corpus.Count, EmbeddingSize, UniformRandom.Distribution);
            var Output = Vectors.Dense(corpus.Count);

            foreach (List<string> sentence in corpus.Sentences) {
                var _sentence = new List<String>(sentence.Where(w => !corpus.SubSampling(w)));
                var _grams = BiGram.FromSentence(_sentence);
                //TODO: https://github.com/chrisjmccormick/word2vec_commented/blob/master/word2vec.c#L931
            }
        }

        /// <summary>
        /// Class holding the bi-grams for training
        /// </summary>
        public class BiGram {
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
                    string left = Stemmer.Stem(sentence[i]);

                    grams.Add(new BiGram(sentence[i], sentence[i + 1]));
                    if (i + 2 < sentence.Count)
                        grams.Add(new BiGram(sentence[i], sentence[i + 2]));
                }

                return grams;
            }
        }

        /// <summary>
        /// A saved training corpus
        /// </summary>
        public class Corpus {
            public const double SubSamplingRate = 0.00001;
            public const double SubSamplingPower = 3/4;

            public List<List<string>> Sentences { get; set; }
            public Dictionary<string, double> Frequencies { get; set; }
            public double NegativeSamplingSum;
            public int Count;

            public Corpus(string path) {
                Load(path);
            }

            /// <summary>
            /// Load from database
            /// </summary>
            /// <param name="path">Path to the database</param>
            public void Load(string path) {
                Sentences = new List<List<string>>();
                Frequencies = new Dictionary<string, double>();
                Count = 0;
                NegativeSamplingSum = 0;

                // Read in
                using (var db = new LiteDatabase(path)) {
                    var col = db.GetCollection<BsonDocument>("sentences");
                    foreach (BsonDocument doc in col.FindAll()) {
                        List<string> sentence = new List<string>();
                        foreach (string word in doc["words"].AsArray) {
                            string stemmed = Stemmer.Stem(word);
                            sentence.Add(stemmed);
                            Frequencies[stemmed] = Frequencies.ContainsKey(stemmed) ? Frequencies[stemmed] + 1 : 1;
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
    }
}
