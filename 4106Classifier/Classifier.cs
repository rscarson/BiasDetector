using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    public class Classifier {
        public double MinimumDistance = 0.15;

        public AverageEmbedding Left { get; set; }
        public AverageEmbedding Right { get; set; }
        public Crawler Crawler { get; set; }

        public Classifier() {
            Left = AverageEmbedding.Load("classifier.left.db");
            Right = AverageEmbedding.Load("classifier.right.db");
            Crawler = new Crawler();
        }

        /// <summary>
        /// Classify an article
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Classification Classify(string url) {
            if (url.StartsWith("/r/")) {
                return ClassifySub(url);
            } else {
                Article a = Crawler.GetArticle(url, Article.BiasType.Unknown);
                return Classify(a);
            }
        }

        /// <summary>
        /// Classify an article
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Classification Classify(Article a) {
            double l = Left.Probability(a.Document()), r = Right.Probability(a.Document());

            double difference = Math.Abs(l - r);
            Classification c = new Classification(l, r);
            c.Bias = ClassifyResults(l, r);

            c.Title = a.URL;
            return c;
        }

        public Classification ClassifySub(string sub) {
            Classification classification = new Classification(0, 0);
            var articles = Crawler.FrontPage(sub);
            foreach (Article a in articles) {
                Classification c = Classify(a);
                if (c.Bias == Article.BiasType.Left)
                    classification.Left++;
                if (c.Bias == Article.BiasType.Right)
                    classification.Right++;

                Console.WriteLine(a.URL);
                Console.WriteLine("Bias: {0} ({1:0.00%} left / {2:0.00%} right)\n", c.Bias, c.Left, c.Right);
            }

            double l = classification.Left / (double)articles.Count;
            double r = classification.Right / (double)articles.Count;
            classification.Bias = ClassifyResults(l, r);
            classification.Left = l; classification.Right = r;
            classification.Title = sub;

            return classification;
        }

        private Article.BiasType ClassifyResults(double l, double r) {
            double difference = Math.Abs(l - r);
            if (Single.IsNaN((float)difference)) {
                // Could not fetch article
                return Article.BiasType.Unknown;
            } else if (difference < MinimumDistance) {
                // Similar results; no bias
                return Article.BiasType.None;
            } else if (l > r) {
                return Article.BiasType.Left;
            } else {
                return Article.BiasType.Right;
            }
        }

        public class Classification {
            public string Title { get; set; }
            public double Left { get; set; }
            public double Right { get; set; }
            public Article.BiasType Bias { get; set; }

            public Classification(double left, double right) {
                Left = left; Right = right;
            }
        }
    }
}
