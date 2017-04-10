using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    static class Extensions {
        /// <summary>
        /// Dimentionality reduction for easy embedding viewing
        /// </summary>
        /// <param name="vector">Vector to reduce</param>
        /// <param name="dimensions">Desired dimentions</param>
        /// <returns>Reduced vector</returns>
        public static Vector<double> Reduce(this Vector<double> vector, int dimensions) {
            Matrix<double> sigma = vector.ToColumnMatrix() * vector.ToColumnMatrix().Transpose();
            var evd = sigma.Evd();

            var e = evd.EigenVectors;
            e = e.SubMatrix(0, e.RowCount, 0, dimensions).Transpose();
            return (e * vector);
        }

        /// <summary>
        /// Translate a string to color
        /// </summary>
        /// <param name="s">String</param>
        /// <returns>color hash</returns>
        public static Color ToColor(this string s) {
            return Color.FromArgb(0xFF | s.GetHashCode());
        }
    }
}
