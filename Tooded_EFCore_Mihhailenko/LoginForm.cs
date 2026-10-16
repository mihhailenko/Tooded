namespace Tooded_EFCore;

public partial class LoginForm : Form
{
    private readonly TextBox txtEmail = new(), txtParool = new();
    private readonly Button btnLogin = new();
    private readonly Label lblVeateade = new();

    public LoginForm()
    {
        InitializeComponent();
        Text = "Sisselogimine";
        ClientSize = new Size(300, 210);

        txtEmail.SetBounds(30, 30, 240, 27);
        txtEmail.PlaceholderText = "E-post";
        txtParool.SetBounds(30, 70, 240, 27);
        txtParool.PlaceholderText = "Parool";
        txtParool.UseSystemPasswordChar = true;     // näitab punkte

        btnLogin.SetBounds(30, 110, 240, 35);
        btnLogin.Text = "Logi sisse";
        btnLogin.Click += btnLogin_Click;

        lblVeateade.SetBounds(30, 155, 240, 40);
        lblVeateade.ForeColor = Color.Red;

        AcceptButton = btnLogin;   // Enter-klahv vajutab nuppu
        Controls.AddRange(new Control[] { txtEmail, txtParool, btnLogin, lblVeateade });
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim().ToLower();

        using var db = new ToodedContext();
        var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Email == email);

        // Sama teade mõlemal juhul: võõras ei saa teada, kas selline e-post on olemas
        if (kasutaja == null || !PasswordHelper.Verify(txtParool.Text, kasutaja.ParoolHash))
        {
            lblVeateade.Text = "Vale e-post või parool.";
            txtParool.Clear();
            return;
        }

        AktiivneKasutaja.Nimi = kasutaja.Nimi;
        AktiivneKasutaja.Email = kasutaja.Email;
        Ostukorv.Tyhjenda();
        AktiivneKasutaja.Roll = kasutaja.Roll;

        var menyy = new FormMain();
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
                Close();
        };
        menyy.Show();
        Hide();
    }
}
