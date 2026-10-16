namespace Tooded_EFCore;

public partial class FormMain : Form
{
    private readonly Button btnTooted = new(), btnPood = new(), btnArve = new(), btnLogiValja = new();
    public bool LogisValja { get; private set; }

    public FormMain()
    {
        InitializeComponent();
        Text = "Peamenüü";
        ClientSize = new Size(260, 250);

        SeaNupp(btnTooted, "Tootehaldus", 30, (s, e) => { if (AktiivneKasutaja.OnAdmin) Ava(new Form2()); });
        SeaNupp(btnPood, "Pood", 80, (s, e) => Ava(new Form3()));
        SeaNupp(btnArve, "Ostukorv ja arve", 130, (s, e) => Ava(new ArveForm()));

        SeaNupp(btnLogiValja, "Logi välja", 180, btnLogiValja_Click);
        Controls.AddRange(new Control[] { btnTooted, btnPood, btnArve, btnLogiValja });
        btnTooted.Visible = AktiivneKasutaja.OnAdmin;
        if (!AktiivneKasutaja.OnAdmin)
        {
            btnPood.Top = 30;
            btnArve.Top = 80;
            btnLogiValja.Top = 130;
            ClientSize = new Size(260, 200);
        }
        Text = $"Peamenüü – {AktiivneKasutaja.Nimi}";
    }

    private void SeaNupp(Button nupp, string tekst, int y, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(30, y, 200, 40);
        nupp.Click += kasitleja;
    }

    private void btnLogiValja_Click(object? sender, EventArgs e)
    {
        Ostukorv.Tyhjenda();
        AktiivneKasutaja.Nimi = "";
        AktiivneKasutaja.Email = "";
        AktiivneKasutaja.Roll = "";
        LogisValja = true;
        Close();
    }

    private static void Ava(Form vorm)
    {
        using (vorm)          // vorm vabastatakse pärast sulgemist
            vorm.ShowDialog();
    }
}
