namespace Tooded_EFCore;

public class MuudaParooliForm : Form
{
    private readonly TextBox vana = new TextBox();
    private readonly TextBox uus = new TextBox();
    private readonly TextBox kordus = new TextBox();
    private readonly Label teade = new Label();
    public MuudaParooliForm(bool kohustuslik = false)
    {
        Vaade.Vorm(this, "Muuda parooli", 490, 560);
        MinimumSize = new Size(440, 440);
        MaximizeBox = false;
        TableLayoutPanel raam = Vaade.Raam(this, "Muuda parooli", kohustuslik ? "Enne jätkamist määra oma kontole uus isiklik parool." : "Vali uus parool, mida sa mujal ei kasuta.");
        TableLayoutPanel paneel = Vaade.Veerg();
        vana.PlaceholderText = "Sisesta praegune parool";
        uus.PlaceholderText = "Vähemalt 8 märki";
        kordus.PlaceholderText = "Sisesta uus parool uuesti";
        foreach (TextBox kast in new[]
        {
            vana,
            uus,
            kordus
        })
        {
            kast.UseSystemPasswordChar = true;
        }

        Vaade.Vali(paneel, "Praegune parool", vana);
        Vaade.Vali(paneel, "Uus parool", uus);
        Vaade.Vali(paneel, "Korda uut parooli", kordus);
        Vaade.Lisa(paneel, Vaade.Tekstirida("Vali vähemalt 8 märgi pikkune parool, mis erineb vanast."));
        Button nupp = new Button
        {
            Text = "Salvesta parool",
            Dock = DockStyle.Top
        };
        Vaade.Nupp(nupp, true);
        nupp.Click += Salvesta;
        Vaade.Lisa(paneel, nupp);
        Button loobu = new Button
        {
            Text = "Loobu",
            DialogResult = DialogResult.Cancel,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(loobu);
        Vaade.Lisa(paneel, loobu);
        teade.AutoSize = true;
        teade.MaximumSize = new Size(390, 0);
        teade.Dock = DockStyle.Top;
        teade.ForeColor = Vaade.Punane;
        Vaade.Lisa(paneel, teade);
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(paneel)), 0, 1);
        AcceptButton = nupp;
        CancelButton = loobu;
        ResumeLayout(true);
    }

    private void Salvesta(object? sender, EventArgs e)
    {
        teade.Text = PasswordHelper.Kontrolli(uus.Text, kordus.Text);
        if (teade.Text != "")
        {
            uus.Focus();
            return;
        }
        if (uus.Text == vana.Text)
        {
            teade.Text = "Vali uus parool, mis erineb vanast.";
            uus.Focus();
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            Kasutaja? kasutaja = db.Kasutajad.Find(AktiivneKasutaja.Id);
            if (kasutaja == null || !PasswordHelper.Verify(vana.Text, kasutaja.ParoolHash))
            {
                teade.Text = "Praegune parool ei ole õige.";
                vana.Focus();
                return;
            }

            kasutaja.ParoolHash = PasswordHelper.Hash(uus.Text);
            kasutaja.PeabParooliMuutma = false;
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Vead.Naita("Parooli muutmine ei õnnestunud.", ex);
        }
    }
}
