namespace Tooded_EFCore_Mihhailenko
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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

        private System.Windows.Forms.DataGridView dataGridViewTooted;
        private System.Windows.Forms.Label lblNimetus;
        private System.Windows.Forms.Label lblKogus;
        private System.Windows.Forms.Label lblHind;
        private System.Windows.Forms.Label lblKategooria;
        private System.Windows.Forms.TextBox txtNimetus;
        private System.Windows.Forms.TextBox txtKogus;
        private System.Windows.Forms.TextBox txtHind;
        private System.Windows.Forms.ComboBox comboKategooria;
        private System.Windows.Forms.PictureBox picturePilt;
        private System.Windows.Forms.Button btnLisa;
        private System.Windows.Forms.Button btnUuenda;
        private System.Windows.Forms.Button btnKustuta;
        private System.Windows.Forms.Button btnValiPilt;

        private System.Windows.Forms.Button btnArve;

        private void InitializeComponent()
        {
            this.dataGridViewTooted = new System.Windows.Forms.DataGridView();
            this.lblNimetus = new System.Windows.Forms.Label();
            this.lblKogus = new System.Windows.Forms.Label();
            this.lblHind = new System.Windows.Forms.Label();
            this.lblKategooria = new System.Windows.Forms.Label();
            this.txtNimetus = new System.Windows.Forms.TextBox();
            this.txtKogus = new System.Windows.Forms.TextBox();
            this.txtHind = new System.Windows.Forms.TextBox();
            this.comboKategooria = new System.Windows.Forms.ComboBox();
            this.picturePilt = new System.Windows.Forms.PictureBox();
            this.btnLisa = new System.Windows.Forms.Button();
            this.btnUuenda = new System.Windows.Forms.Button();
            this.btnKustuta = new System.Windows.Forms.Button();
            this.btnValiPilt = new System.Windows.Forms.Button();
            this.btnArve = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTooted)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePilt)).BeginInit();
            this.SuspendLayout();

            // dataGridViewTooted
            this.dataGridViewTooted.Location = new System.Drawing.Point(40, 280);
            this.dataGridViewTooted.Size = new System.Drawing.Size(694, 187);
            this.dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTooted.MultiSelect = false;
            this.dataGridViewTooted.ReadOnly = true;
            this.dataGridViewTooted.CellClick += dataGridViewTooted_CellClick;

            // lblNimetus
            this.lblNimetus.Location = new System.Drawing.Point(17, 26);
            this.lblNimetus.Size = new System.Drawing.Size(142, 30);
            this.lblNimetus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblNimetus.Text = "Toode nimetus";
            this.lblNimetus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // lblKogus
            this.lblKogus.Location = new System.Drawing.Point(17, 74);
            this.lblKogus.Size = new System.Drawing.Size(142, 30);
            this.lblKogus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblKogus.Text = "Kogus";
            this.lblKogus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // lblHind
            this.lblHind.Location = new System.Drawing.Point(17, 124);
            this.lblHind.Size = new System.Drawing.Size(142, 30);
            this.lblHind.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblHind.Text = "Hind";
            this.lblHind.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // lblKategooria
            this.lblKategooria.Location = new System.Drawing.Point(17, 167);
            this.lblKategooria.Size = new System.Drawing.Size(142, 32);
            this.lblKategooria.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.lblKategooria.Text = "Kategooria";
            this.lblKategooria.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // txtNimetus
            this.txtNimetus.Location = new System.Drawing.Point(185, 27);
            this.txtNimetus.Width = 158;
            this.txtNimetus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);

            // txtKogus
            this.txtKogus.Location = new System.Drawing.Point(185, 75);
            this.txtKogus.Width = 158;
            this.txtKogus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);

            // txtHind
            this.txtHind.Location = new System.Drawing.Point(185, 125);
            this.txtHind.Width = 158;
            this.txtHind.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);

            // comboKategooria
            this.comboKategooria.Location = new System.Drawing.Point(185, 168);
            this.comboKategooria.Width = 158;
            this.comboKategooria.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);

            // picturePilt
            this.picturePilt.Location = new System.Drawing.Point(360, 27);
            this.picturePilt.Size = new System.Drawing.Size(245, 172);
            this.picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
            this.picturePilt.BorderStyle = BorderStyle.Fixed3D;

            // btnValiPilt
            this.btnValiPilt.Location = new System.Drawing.Point(18, 210);
            this.btnValiPilt.Size = new System.Drawing.Size(168, 47);
            this.btnValiPilt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnValiPilt.Text = "Otsi fail";
            this.btnValiPilt.Click += btnValiPilt_Click;

            // btnLisa
            this.btnLisa.Location = new System.Drawing.Point(201, 210);
            this.btnLisa.Size = new System.Drawing.Size(168, 47);
            this.btnLisa.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnLisa.Text = "Lisa";
            this.btnLisa.Click += btnLisa_Click;

            // btnUuenda
            this.btnUuenda.Location = new System.Drawing.Point(384, 210);
            this.btnUuenda.Size = new System.Drawing.Size(168, 47);
            this.btnUuenda.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnUuenda.Text = "Uuenda";
            this.btnUuenda.Click += btnUuenda_Click;

            // btnKustuta
            this.btnKustuta.Location = new System.Drawing.Point(567, 210);
            this.btnKustuta.Size = new System.Drawing.Size(168, 47);
            this.btnKustuta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnKustuta.Text = "Kustuta";
            this.btnKustuta.Click += btnKustuta_Click;

            // btnArve
            this.btnArve.Location = new System.Drawing.Point(625, 27);
            this.btnArve.Name = "btnArve";
            this.btnArve.Size = new System.Drawing.Size(150, 40);
            this.btnArve.Text = "Lisa arvele";
            this.btnArve.Click += btnArve_Click;

            // Form2
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 486);
            this.Controls.Add(this.lblNimetus);
            this.Controls.Add(this.lblKogus);
            this.Controls.Add(this.lblHind);
            this.Controls.Add(this.lblKategooria);
            this.Controls.Add(this.dataGridViewTooted);
            this.Controls.Add(this.txtNimetus);
            this.Controls.Add(this.txtKogus);
            this.Controls.Add(this.txtHind);
            this.Controls.Add(this.comboKategooria);
            this.Controls.Add(this.picturePilt);
            this.Controls.Add(this.btnValiPilt);
            this.Controls.Add(this.btnLisa);
            this.Controls.Add(this.btnUuenda);
            this.Controls.Add(this.btnKustuta);
            this.Controls.Add(this.btnArve);
            this.Text = "Tooded";

            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTooted)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePilt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
