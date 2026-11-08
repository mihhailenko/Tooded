namespace Tooded_EFCore;

public partial class LoginForm : Form
{
    private readonly TextBox txtEmail = new TextBox();
    private readonly TextBox txtParool = new TextBox();
    private readonly Button btnLogin = new BarboraActionButton();
    private readonly Button btnRegistreeru = new BarboraActionButton();
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
        Vaade.Vorm(this, "BARBORA · Logi sisse", 960, 660);
        StartPosition = FormStartPosition.CenterScreen; MinimumSize = new Size(820, 590); MaximizeBox = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(32) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        var brand = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = new Padding(0, 0, 32, 0) };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); brand.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        brand.Controls.Add(new BarboraAsset { AssetPath = "logo/barbora_next_app_bar_logo.svg", Dock = DockStyle.Fill, Margin = new Padding(42, 14, 42, 14) }, 0, 0);
        brand.Controls.Add(new BarboraAsset { AssetPath = "images/img_groceries_bag.png", Dock = DockStyle.Fill }, 0, 1);
        brand.Controls.Add(new Label { Text = "Toidukaupade mugav tellimine", Font = BarboraTheme.Font(16), ForeColor = BarboraTheme.Text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 0, 2);
        var card = new BarboraSurface { Dock = DockStyle.Fill, Padding = new Padding(30, 40, 30, 22) };
        var form = Vaade.Veerg();
        var title = Vaade.Tekstirida("Logi sisse", true); title.Font = BarboraTheme.Font(23, FontStyle.Bold);
        Vaade.Lisa(form, title); Vaade.Lisa(form, Vaade.Tekstirida("Tere tulemast tagasi!"));
        txtEmail.MaxLength = 254; txtEmail.PlaceholderText = "E-posti aadress";
        txtParool.PlaceholderText = "Parool"; txtParool.UseSystemPasswordChar = true;
        Vaade.Vali(form, "E-post", txtEmail); Vaade.Vali(form, "Parool", txtParool);
        btnLogin.Text = "Logi sisse"; Vaade.Nupp(btnLogin, true); btnLogin.Dock = DockStyle.Top; btnLogin.MinimumSize = new Size(100, 46); btnLogin.Click += btnLogin_Click;
        Vaade.Lisa(form, btnLogin);
        var help = Vaade.Tekstirida("Sul ei ole veel kontot?"); help.Margin = new Padding(0, 22, 0, 10); Vaade.Lisa(form, help);
        btnRegistreeru.Text = "Loo konto"; Vaade.Nupp(btnRegistreeru); btnRegistreeru.Dock = DockStyle.Top; btnRegistreeru.Click += btnRegistreeru_Click; Vaade.Lisa(form, btnRegistreeru);
        lblVeateade.AutoSize = true; lblVeateade.Dock = DockStyle.Top; lblVeateade.ForeColor = BarboraTheme.Red; lblVeateade.Margin = new Padding(0, 14, 0, 0); Vaade.Lisa(form, lblVeateade);
        card.Controls.Add(Vaade.Kerimine(form)); root.Controls.Add(brand, 0, 0); root.Controls.Add(card, 1, 0); Controls.Add(root);
        AcceptButton = btnLogin; ResumeLayout(true);
    }
}
