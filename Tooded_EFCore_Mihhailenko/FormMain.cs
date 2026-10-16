namespace Tooded_EFCore_Mihhailenko
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            btnTooted.Visible = AktiivneKasutaja.Roll == "admin";

            if (AktiivneKasutaja.Roll != "admin")
            {
                btnPood.Location = new Point(30, 30);
                btnArve.Location = new Point(30, 80);
                btnLogiValja.Location = new Point(30, 130);
                this.ClientSize = new Size(300, 220);
            }
        }

        private void btnTooted_Click(object sender, EventArgs e)
        {
            if (AktiivneKasutaja.Roll != "admin") return;

            Form2 f = new Form2();
            f.ShowDialog();
        }

        private void btnPood_Click(object sender, EventArgs e)
        {
            Form3 poodForm = new Form3();
            poodForm.ShowDialog();
        }

        private void btnArve_Click(object sender, EventArgs e)
        {
            ArveForm arve = new ArveForm();
            arve.ShowDialog();
        }

        private void btnLogiValja_Click(object sender, EventArgs e)
        {
            Ostukorv.Read.Clear();
            AktiivneKasutaja.Roll = "";
            AktiivneKasutaja.Nimi = "";
            AktiivneKasutaja.Email = "";
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }
    }
}
