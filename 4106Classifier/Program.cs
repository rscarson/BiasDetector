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
            AverageEmbedding classifier = new AverageEmbedding();
            classifier.Train(Article.BiasType.Right);

            Application.Run();
        }
    }
}
