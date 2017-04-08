using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    class Program {
        static void Main(string[] args) {
            Crawler c = new Crawler();
            var articles = c.Articles("/r/conservative");
            foreach (string text in articles) {
                continue;
            }
        }
    }
}
