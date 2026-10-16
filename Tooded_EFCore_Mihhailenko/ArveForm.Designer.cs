namespace Tooded_EFCore_Mihhailenko
{
    partial class ArveForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private Label lblPealkiri;
        private Label lblArveNumber;
        private Label lblKuupaev;
        private Label lblMuuja;
        private Label lblOstja;
        private Label lblKokku;
        private Label lblTanu;
        private DataGridView dataGridViewArve;
        private Button btnGenPDF;
        private Button btnSaadaEmail;
        private Button btnTagasi;

        private void InitializeComponent()
        {
            this.lblPealkiri = new Label();
            this.lblArveNumber = new Label();
            this.lblKuupaev = new Label();
            this.lblMuuja = new Label();
            this.lblOstja = new Label();
            this.lblKokku = new Label();
            this.lblTanu = new Label();
            this.dataGridViewArve = new DataGridView();
            this.btnGenPDF = new Button();
            this.btnSaadaEmail = new Button();
            this.btnTagasi = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewArve)).BeginInit();
            this.SuspendLayout();

            // lblPealkiri
            this.lblPealkiri.Text = "ARVE";
            this.lblPealkiri.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            this.lblPealkiri.Location = new Point(25, 20);
            this.lblPealkiri.Size = new Size(200, 55);

            // lblArveNumber
            this.lblArveNumber.Location = new Point(270, 30);
            this.lblArveNumber.Size = new Size(385, 25);
            this.lblArveNumber.TextAlign = ContentAlignment.MiddleRight;

            // lblKuupaev
            this.lblKuupaev.Location = new Point(270, 60);
            this.lblKuupaev.Size = new Size(385, 25);
            this.lblKuupaev.TextAlign = ContentAlignment.MiddleRight;

            // lblMuuja
            this.lblMuuja.Text = "Müüja: Õppepood";
            this.lblMuuja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblMuuja.Location = new Point(25, 105);
            this.lblMuuja.Size = new Size(630, 25);

            // lblOstja
            this.lblOstja.Location = new Point(25, 145);
            this.lblOstja.Size = new Size(630, 45);

            // dataGridViewArve
            this.dataGridViewArve.Name = "dataGridViewArve";
            this.dataGridViewArve.Location = new Point(25, 210);
            this.dataGridViewArve.Size = new Size(630, 245);
            this.dataGridViewArve.BackgroundColor = Color.White;
            this.dataGridViewArve.BorderStyle = BorderStyle.FixedSingle;
            this.dataGridViewArve.ReadOnly = true;
            this.dataGridViewArve.AllowUserToAddRows = false;
            this.dataGridViewArve.AllowUserToDeleteRows = false;
            this.dataGridViewArve.RowHeadersVisible = false;
            this.dataGridViewArve.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewArve.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            this.dataGridViewArve.EnableHeadersVisualStyles = false;
            this.dataGridViewArve.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            this.dataGridViewArve.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.dataGridViewArve.ColumnHeadersHeight = 32;
            this.dataGridViewArve.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewArve.Columns.Add("Toode", "Toode");
            this.dataGridViewArve.Columns.Add("Kogus", "Kogus");
            this.dataGridViewArve.Columns.Add("Hind", "Hind");
            this.dataGridViewArve.Columns.Add("Summa", "Summa");
            this.dataGridViewArve.Columns[0].FillWeight = 48;
            this.dataGridViewArve.Columns[1].FillWeight = 12;
            this.dataGridViewArve.Columns[2].FillWeight = 20;
            this.dataGridViewArve.Columns[3].FillWeight = 20;
            this.dataGridViewArve.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridViewArve.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridViewArve.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // lblKokku
            this.lblKokku.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblKokku.BackColor = Color.WhiteSmoke;
            this.lblKokku.Location = new Point(350, 475);
            this.lblKokku.Size = new Size(305, 45);
            this.lblKokku.TextAlign = ContentAlignment.MiddleRight;
            this.lblKokku.Text = "KOKKU: 0,00 €";

            // lblTanu
            this.lblTanu.Text = "Aitäh ostu eest!";
            this.lblTanu.ForeColor = Color.Gray;
            this.lblTanu.Location = new Point(25, 540);
            this.lblTanu.Size = new Size(630, 25);
            this.lblTanu.TextAlign = ContentAlignment.MiddleCenter;

            // btnGenPDF
            this.btnGenPDF.Text = "Genereeri PDF";
            this.btnGenPDF.Location = new Point(25, 590);
            this.btnGenPDF.Size = new Size(195, 35);
            this.btnGenPDF.Click += btnGenPDF_Click;

            // btnSaadaEmail
            this.btnSaadaEmail.Text = "Saada emailiga";
            this.btnSaadaEmail.Location = new Point(240, 590);
            this.btnSaadaEmail.Size = new Size(195, 35);
            this.btnSaadaEmail.Click += btnSaadaEmail_Click;

            // btnTagasi
            this.btnTagasi.Text = "Tagasi";
            this.btnTagasi.Location = new Point(455, 590);
            this.btnTagasi.Size = new Size(200, 35);
            this.btnTagasi.Click += btnTagasi_Click;

            // ArveForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(680, 650);
            this.Name = "ArveForm";
            this.Text = "Arve";
            this.Controls.Add(this.lblPealkiri);
            this.Controls.Add(this.lblArveNumber);
            this.Controls.Add(this.lblKuupaev);
            this.Controls.Add(this.lblMuuja);
            this.Controls.Add(this.lblOstja);
            this.Controls.Add(this.dataGridViewArve);
            this.Controls.Add(this.lblKokku);
            this.Controls.Add(this.lblTanu);
            this.Controls.Add(this.btnGenPDF);
            this.Controls.Add(this.btnSaadaEmail);
            this.Controls.Add(this.btnTagasi);
            this.Load += ArveForm_Load;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewArve)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}