using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4106Classifier {
    public partial class EmbeddingView : Form {
        private Embedding embedding;

        public EmbeddingView() {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;

            embedding = new Embedding();
            embedding.Train("corpus.db");
        }

        private void EmbeddingView_Load(object sender, EventArgs e) {
        }

        private void EmbeddingView_Resize(object sender, EventArgs e) {
            Reset();
        }

        private void Reset() {
            this.Controls.Clear();
            Dictionary<string, Vector<double>> Reduced = new Dictionary<string, Vector<double>>();
            foreach (var word in embedding.Vocabulary) {
                Reduced[word.Key] = word.Value.Reduce(2);
            }

            double
                minX = Reduced.Values.Select(v => v[0]).Min(),
                minY = Reduced.Values.Select(v => v[1]).Min(),
                maxX = Reduced.Values.Select(v => v[0]).Max(),
                maxY = Reduced.Values.Select(v => v[1]).Max();
            foreach (var word in Reduced) {
                double x = ((word.Value[0] - minX) / (maxX - minX)) * (Width - 5) + 5;
                double y = ((word.Value[1] - minY) / (maxY - minY)) * (Height - 5) + 5;


                var label = new Label();
                label.BackColor = word.Key.ToColor();
                label.BorderStyle = BorderStyle.FixedSingle;
                label.SetBounds((int)x, (int)y, 5, 5);

                ToolTip t = new ToolTip();
                t.InitialDelay = t.ReshowDelay = t.AutoPopDelay = 0;
                t.ShowAlways = true;
                t.SetToolTip(label, word.Key);

                this.Controls.Add(label);
            }
        }
    }
}
