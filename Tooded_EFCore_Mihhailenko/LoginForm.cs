namespace Tooded_EFCore_Mihhailenko
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            using var db = new ToodedContext();

            string email = txtEmail.Text.Trim();
            string parool = txtParool.Text;

            var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Email == email);

            if (kasutaja == null)
            {
                lblVeateade.Text = "Vale email!";
                return;
            }

            if (!PasswordHelper.Verify(parool, kasutaja.ParoolHash))
            {
                lblVeateade.Text = "Vale parool!";
                return;
            }

            AktiivneKasutaja.Roll = kasutaja.Roll;
            AktiivneKasutaja.Nimi = kasutaja.Nimi;
            AktiivneKasutaja.Email = kasutaja.Email;
            Ostukorv.Read.Clear();

            using FormMain main = new FormMain();
            this.Hide();
            main.ShowDialog();
            if (main.DialogResult == DialogResult.Retry)
            {
                txtParool.Clear();
                lblVeateade.Text = "";
                this.Show();
                txtEmail.Focus();
            }
            else
            {
                this.Close();
            }
        }
    }
}
