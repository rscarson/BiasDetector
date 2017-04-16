namespace _4106Classifier {
    partial class Interface {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.url = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.result = new System.Windows.Forms.Label();
            this.right_percentage = new System.Windows.Forms.Label();
            this.percent_left = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(962, 21);
            this.panel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.Location = new System.Drawing.Point(887, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "Analyse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(962, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.url);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.result);
            this.panel2.Controls.Add(this.right_percentage);
            this.panel2.Controls.Add(this.percent_left);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 21);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(962, 564);
            this.panel2.TabIndex = 1;
            // 
            // url
            // 
            this.url.Dock = System.Windows.Forms.DockStyle.Top;
            this.url.Location = new System.Drawing.Point(220, 0);
            this.url.Name = "url";
            this.url.Size = new System.Drawing.Size(522, 23);
            this.url.TabIndex = 4;
            this.url.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button2.Location = new System.Drawing.Point(220, 541);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(522, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Embeddings";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // result
            // 
            this.result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.result.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.result.Location = new System.Drawing.Point(220, 0);
            this.result.Name = "result";
            this.result.Size = new System.Drawing.Size(522, 564);
            this.result.TabIndex = 2;
            this.result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // right_percentage
            // 
            this.right_percentage.Dock = System.Windows.Forms.DockStyle.Right;
            this.right_percentage.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.right_percentage.Location = new System.Drawing.Point(742, 0);
            this.right_percentage.Name = "right_percentage";
            this.right_percentage.Size = new System.Drawing.Size(220, 564);
            this.right_percentage.TabIndex = 1;
            this.right_percentage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // percent_left
            // 
            this.percent_left.Dock = System.Windows.Forms.DockStyle.Left;
            this.percent_left.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.percent_left.Location = new System.Drawing.Point(0, 0);
            this.percent_left.Name = "percent_left";
            this.percent_left.Size = new System.Drawing.Size(220, 564);
            this.percent_left.TabIndex = 0;
            this.percent_left.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Interface
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(962, 585);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Interface";
            this.Text = "Bias Classifier";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label result;
        private System.Windows.Forms.Label right_percentage;
        private System.Windows.Forms.Label percent_left;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label url;
    }
}