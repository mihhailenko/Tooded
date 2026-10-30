namespace Tooded_EFCore;

public partial class LoginForm : Form
{
    private readonly TextBox txtEmail = new TextBox();
    private readonly TextBox txtParool = new TextBox();
    private readonly Button btnLogin = new Button();
    private readonly Button btnRegistreeru = new Button();
    private readonly Label lblVeateade = new Label();
    public LoginForm()
    {
        InitializeComponent();
        LooLiides();
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            lblVeateade.Text = "Sisesta oma e-posti aadress.";
            txtEmail.Focus();
            return;
        }
        if (txtParool.Text.Length == 0)
        {
            lblVeateade.Text = "Sisesta oma parool.";
            txtParool.Focus();
            return;
        }
        btnRegistreeru.Enabled = false;
        btnLogin.Enabled = false;
        lblVeateade.ForeColor = Vaade.Punane;
        lblVeateade.Text = "";
        try
        {
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            using ToodedContext db = new ToodedContext();
            Kasutaja? kasutaja = db.Kasutajad.FirstOrDefault(k => k.Email == email);
            // Sama teade mõlemal juhul: võõras ei saa teada, kas selline e-post on olemas
            if (kasutaja == null || !PasswordHelper.Verify(txtParool.Text, kasutaja.ParoolHash))
            {
                lblVeateade.Text = "Vale e-post või parool.";
                txtParool.Clear();
                return;
            }

            AktiivneKasutaja.Id = kasutaja.Id;
            AktiivneKasutaja.Nimi = kasutaja.Nimi;
            AktiivneKasutaja.Email = kasutaja.Email;
            Ostukorv.Tyhjenda();
            AktiivneKasutaja.Roll = kasutaja.Roll;
            if (kasutaja.PeabParooliMuutma)
            {
                using MuudaParooliForm paroolivorm = new MuudaParooliForm(true);
                if (paroolivorm.ShowDialog(this) != DialogResult.OK)
                {
                    AktiivneKasutaja.Id = 0;
                    AktiivneKasutaja.Roll = "";
                    AktiivneKasutaja.Nimi = "";
                    AktiivneKasutaja.Email = "";
                    return;
                }
            }

            FormMain menyy = new FormMain();
            menyy.FormClosed += (s, a) =>
            {
                if (menyy.LogisValja)
                {
                    txtParool.Clear();
                    lblVeateade.Text = "";
                    Show();
                    txtEmail.Focus();
                }
                else
                {
                    Close();
                }
            };
            menyy.Show();
            Hide();
        }
        catch (Exception ex)
        {
            Vead.Naita("Sisselogimine ei õnnestunud.", ex);
        }
        finally
        {
            btnRegistreeru.Enabled = true;
            btnLogin.Enabled = true;
        }
    }

    private void btnRegistreeru_Click(object? s, EventArgs e)
    {
        using RegistreerimisForm registreerimine = new RegistreerimisForm();
        if (registreerimine.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        txtEmail.Text = registreerimine.RegistreeritudEmail;
        txtParool.Clear();
        txtParool.Focus();
        lblVeateade.ForeColor = Color.DarkGreen;
        lblVeateade.Text = "Konto on loodud. Sisesta parool ja logi sisse.";
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "Õppepood – sisselogimine", 460, 520);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(420, 430);
        MaximizeBox = false;
        TableLayoutPanel raam = Vaade.Raam(this, "Tere tulemast õppepoodi", "Logi sisse, et sirvida tooteid ja vaadata oma oste.");
        TableLayoutPanel paneel = Vaade.Veerg();
        txtEmail.MaxLength = 254;
        txtEmail.PlaceholderText = "nimi@näide.ee";
        txtParool.PlaceholderText = "Sisesta parool";
        txtParool.UseSystemPasswordChar = true;
        Vaade.Vali(paneel, "E-post", txtEmail);
        Vaade.Vali(paneel, "Parool", txtParool);
        btnLogin.Text = "Logi sisse";
        Vaade.Nupp(btnLogin, true);
        btnLogin.Dock = DockStyle.Top;
        btnLogin.Click += btnLogin_Click;
        Vaade.Lisa(paneel, btnLogin);
        Vaade.Lisa(paneel, Vaade.Tekstirida("Esimest korda siin? Loo endale konto."));
        btnRegistreeru.Text = "Loo kasutajakonto";
        Vaade.Nupp(btnRegistreeru);
        btnRegistreeru.Dock = DockStyle.Top;
        Vaade.Lisa(paneel, btnRegistreeru);
        btnRegistreeru.Click += btnRegistreeru_Click;
        lblVeateade.AutoSize = true;
        lblVeateade.Dock = DockStyle.Top;
        lblVeateade.Margin = new Padding(0, 12, 0, 0);
        lblVeateade.ForeColor = Vaade.Punane;
        Vaade.Lisa(paneel, lblVeateade);
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(paneel)), 0, 1);
        AcceptButton = btnLogin;
        ResumeLayout(true);
    }
}
