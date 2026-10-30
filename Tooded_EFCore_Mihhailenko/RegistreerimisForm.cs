using System.Net.Mail;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class RegistreerimisForm : Form
{
    private readonly TextBox nimi = new TextBox();
    private readonly TextBox email = new TextBox();
    private readonly TextBox parool = new TextBox();
    private readonly TextBox kordus = new TextBox();
    private readonly Button registreeru = new Button();
    private readonly Label teade = new Label();
    private bool salvestamine;
    public string RegistreeritudEmail { get; private set; } = "";

    public RegistreerimisForm()
    {
        LooLiides();
    }

    private void Registreeru(object? sender, EventArgs e)
    {
        if (salvestamine)
        {
            return;
        }

        string kasutajaNimi = nimi.Text.Trim();
        string aadress = email.Text.Trim().ToLowerInvariant();
        teade.Text = "";
        if (kasutajaNimi.Length == 0 || kasutajaNimi.Length > 150)
        {
            teade.Text = "Sisesta oma nimi.";
            nimi.Focus();
            return;
        }

        if (aadress.Length > 254 || !MailAddress.TryCreate(aadress, out var kontroll) || kontroll.Address != aadress)
        {
            teade.Text = "Sisesta korrektne e-posti aadress.";
            email.Focus();
            return;
        }

        string parooliViga = PasswordHelper.Kontrolli(parool.Text, kordus.Text);
        if (parooliViga != "")
        {
            teade.Text = parooliViga;
            parool.Focus();
            return;
        }

        salvestamine = true;
        registreeru.Enabled = false;
        try
        {
            using ToodedContext db = new ToodedContext();
            if (db.Kasutajad.Any(k => k.Email == aadress))
            {
                teade.Text = "Selle e-postiga konto on juba olemas. Logi sisse.";
                return;
            }

            Kasutaja kasutaja = new Kasutaja
            {
                Nimi = kasutajaNimi,
                Email = aadress,
                ParoolHash = PasswordHelper.Hash(parool.Text),
                Roll = "kasutaja",
                PeabParooliMuutma = false
            };
            db.Kasutajad.Add(kasutaja);
            db.SaveChanges();
            RegistreeritudEmail = aadress;
            parool.Clear();
            kordus.Clear();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
        {
            // Teine registreerimine võis sama aadressi pärast meie kontrolli salvestada.
            teade.Text = "Selle e-postiga konto on juba olemas. Logi sisse.";
        }
        catch (Exception ex)
        {
            Vead.Naita("Registreerimine ei õnnestunud. Proovi uuesti.", ex);
        }
        finally
        {
            salvestamine = false;
            registreeru.Enabled = true;
        }
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "Loo kasutajakonto", 480, 670);
        MinimumSize = new Size(430, 520);
        MaximizeBox = false;
        MinimizeBox = false;
        TableLayoutPanel raam = Vaade.Raam(this, "Loo kasutajakonto", "Täida andmed ja vali vähemalt 8 märgi pikkune parool.");
        TableLayoutPanel paneel = Vaade.Veerg();
        nimi.PlaceholderText = "Ees- ja perekonnanimi";
        nimi.MaxLength = 150;
        email.PlaceholderText = "nimi@näide.ee";
        email.MaxLength = 254;
        parool.PlaceholderText = "Vähemalt 8 märki";
        kordus.PlaceholderText = "Sisesta sama parool uuesti";
        kordus.UseSystemPasswordChar = true;
        parool.UseSystemPasswordChar = true;
        kordus.MaxLength = 256;
        parool.MaxLength = 256;
        Vaade.Vali(paneel, "Nimi", nimi);
        Vaade.Vali(paneel, "E-post", email);
        Vaade.Vali(paneel, "Parool", parool);
        Vaade.Vali(paneel, "Korda parooli", kordus);
        registreeru.Text = "Registreeru";
        Vaade.Nupp(registreeru, true);
        registreeru.Dock = DockStyle.Top;
        registreeru.Click += Registreeru;
        Button loobu = new Button
        {
            Text = "Tagasi sisselogimisse",
            DialogResult = DialogResult.Cancel,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(loobu);
        teade.AutoSize = true;
        teade.MaximumSize = new Size(380, 0);
        teade.Dock = DockStyle.Top;
        teade.ForeColor = Vaade.Punane;
        teade.Margin = new Padding(0, 10, 0, 0);
        Vaade.Lisa(paneel, registreeru);
        Vaade.Lisa(paneel, loobu);
        Vaade.Lisa(paneel, teade);
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(paneel)), 0, 1);
        AcceptButton = registreeru;
        CancelButton = loobu;
        ResumeLayout(true);
    }
}
