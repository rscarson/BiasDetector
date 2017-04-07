using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    class UniformRandom {
        public static ContinuousUniform Distribution = new ContinuousUniform();
        public static double Next() {
            return Distribution.Sample();
        }
    }
}
