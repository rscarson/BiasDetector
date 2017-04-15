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
            Classifier classifier = new Classifier();
            
            while (true) {
                Console.Write("Enter subreddit name or article url: ");
                string url = Console.ReadLine();

                Classifier.Classification c;
                if (url.StartsWith("/r/")) {
                    Console.WriteLine("Fetching " + url);
                    c = classifier.ClassifySub(url);
                } else {
                    c = classifier.Classify(url);
                }

                Console.WriteLine("Bias: {0} ({1:0.00%} left / {2:0.00%} right)", c.Bias, c.Left, c.Right);
            }
            */

            Interface i = new Interface();
            i.Show();
            Application.Run();
        }
    }
}
