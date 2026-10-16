namespace Tooded_EFCore_Mihhailenko
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private Button btnTooted;
        private Button btnPood;
        private Button btnArve;
        private Button btnLogiValja;

        private void InitializeComponent()
        {
            this.btnTooted = new Button();
            this.btnPood = new Button();
            this.btnArve = new Button();
            this.btnLogiValja = new Button();

            this.SuspendLayout();

            // btnTooted
            this.btnTooted.Name = "btnTooted";
            this.btnTooted.Text = "Tootehaldus";
            this.btnTooted.Location = new Point(30, 30);
            this.btnTooted.Size = new Size(200, 40);
            this.btnTooted.Click += btnTooted_Click;

            // btnPood
            this.btnPood.Name = "btnPood";
            this.btnPood.Text = "Pood";
            this.btnPood.Location = new Point(30, 80);
            this.btnPood.Size = new Size(200, 40);
            this.btnPood.Click += btnPood_Click;

            // btnArve
            this.btnArve.Name = "btnArve";
            this.btnArve.Text = "Arve";
            this.btnArve.Location = new Point(30, 130);
            this.btnArve.Size = new Size(200, 40);
            this.btnArve.Click += btnArve_Click;

            // btnLogiValja
            this.btnLogiValja.Name = "btnLogiValja";
            this.btnLogiValja.Text = "Logi välja";
            this.btnLogiValja.Location = new Point(30, 180);
            this.btnLogiValja.Size = new Size(200, 40);
            this.btnLogiValja.Click += btnLogiValja_Click;

            // FormMain
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Name = "FormMain";
            this.ClientSize = new Size(300, 270);
            this.Controls.Add(this.btnTooted);
            this.Controls.Add(this.btnPood);
            this.Controls.Add(this.btnArve);
            this.Controls.Add(this.btnLogiValja);
            this.Text = "Peamenüü";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
