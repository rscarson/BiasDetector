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
    public partial class Interface : Form {
        public Classifier Classifer { get; set; }

        public Interface() {
            InitializeComponent();
            button1.Enabled = false;

            Classifer = new Classifier();
        }

        private void button1_Click(object sender, EventArgs e) {
            string url = textBox1.Text;
            this.url.Text = "Fetching " + url + " please wait........";
            var result = Classifer.Classify(url);
            this.result.Text = result.Bias.ToString();
            percent_left.Text = string.Format("{0:0.00%}", result.Left);
            right_percentage.Text = string.Format("{0:0.00%}", result.Right);
            textBox1.Text = "";
            this.url.Text = result.Title;


            switch (result.Bias) {
                case Article.BiasType.Unknown:
                    this.result.Text = "Unable to fetch article or subreddit.";
                    percent_left.Text = "";
                    right_percentage.Text = "";
                    break;

                case Article.BiasType.None:
                    this.result.Text = "No detectable bias.";
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            button1.Enabled = textBox1.Text.Length > 0;
        }

        private void button2_Click(object sender, EventArgs e) {
            EmbeddingView v = new EmbeddingView();
            v.Show();
        }
    }
}
