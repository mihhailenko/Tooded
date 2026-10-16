namespace Tooded_EFCore_Mihhailenko
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.TabControl tabPood;
        private Label lblOstukorv;
        private Button btnMineOstukorvi;

        private void InitializeComponent()
        {
            this.tabPood = new System.Windows.Forms.TabControl();
            this.lblOstukorv = new Label();
            this.btnMineOstukorvi = new Button();
            this.SuspendLayout();

            // tabPood
            this.tabPood.Location = new System.Drawing.Point(10, 10);
            this.tabPood.Name = "tabPood";
            this.tabPood.Size = new System.Drawing.Size(900, 550);

            // lblOstukorv
            this.lblOstukorv.Name = "lblOstukorv";
            this.lblOstukorv.Location = new Point(10, 580);
            this.lblOstukorv.Size = new Size(650, 30);
            this.lblOstukorv.Text = "Ostukorvis: 0 tk    Kokku: 0,00 €";

            // btnMineOstukorvi
            this.btnMineOstukorvi.Name = "btnMineOstukorvi";
            this.btnMineOstukorvi.Text = "Mine ostukorvi";
            this.btnMineOstukorvi.Location = new Point(710, 575);
            this.btnMineOstukorvi.Size = new Size(200, 35);
            this.btnMineOstukorvi.Click += btnMineOstukorvi_Click;

            // Form3
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(930, 630);
            this.Controls.Add(this.tabPood);
            this.Controls.Add(this.lblOstukorv);
            this.Controls.Add(this.btnMineOstukorvi);
            this.Name = "Form3";
            this.Text = "Pood";
            this.ResumeLayout(false);
        }
    }
}
