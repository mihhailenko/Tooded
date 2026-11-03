using System.Net.Mail;

namespace Tooded_EFCore;

public class PostiSeadedForm : Form
{
    private readonly TextBox server = new TextBox();
    private readonly TextBox saatja = new TextBox();
    private readonly TextBox kasutaja = new TextBox();
    private readonly TextBox parool = new TextBox();
    private readonly NumericUpDown port = new NumericUpDown()
    {
        Minimum = 1,
        Maximum = 65535,
        Value = 587
    };
    private PostiSeaded seaded = new PostiSeaded();
    public PostiSeadedForm()
    {
        if (!AktiivneKasutaja.OnAdmin)
        {
            throw new UnauthorizedAccessException();
        }
        LooLiides();
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "E-posti seaded", 520, 740);
        MinimumSize = new Size(460, 530);
        MaximizeBox = false;
        TableLayoutPanel raam = Vaade.Raam(this, "E-posti seaded", "Seadista konto, mille kaudu saadad ostjale PDF-arve.");
        TableLayoutPanel paneel = Vaade.Veerg();
        server.PlaceholderText = "Näiteks smtp.gmail.com";
        saatja.PlaceholderText = "nimi@näide.ee";
        kasutaja.PlaceholderText = "E-posti konto kasutajanimi";
        parool.PlaceholderText = "Tühi väli säilitab vana parooli";
        parool.UseSystemPasswordChar = true;
        Vaade.Vali(paneel, "Väljuva posti server", server);
        Vaade.Vali(paneel, "Port", port);
        Vaade.Vali(paneel, "Saatja e-post", saatja);
        Vaade.Vali(paneel, "Kasutajanimi", kasutaja);
        Vaade.Vali(paneel, "E-posti parool", parool);
        Vaade.Lisa(paneel, Vaade.Tekstirida("Serveri ja pordi leiad oma e-posti teenuse seadistustest. Kui teenus seda nõuab, kasuta rakenduseparooli."));
        Button salvesta = new Button
        {
            Text = "Salvesta seaded",
            Dock = DockStyle.Top
        };
        Vaade.Nupp(salvesta, true);
        salvesta.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(server.Text) || !MailAddress.TryCreate(saatja.Text.Trim(), out _) || string.IsNullOrWhiteSpace(kasutaja.Text))
            {
                MessageBox.Show("Täida posti server, saatja e-post ja kasutajanimi.");
                return;
            }

            try
            {
                seaded.Server = server.Text.Trim();
                seaded.Port = (int)port.Value;
                seaded.Saatja = saatja.Text.Trim();
                seaded.Kasutajanimi = kasutaja.Text.Trim();
                seaded.Salvesta(parool.Text.Length == 0 ? null : parool.Text);
                parool.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Vead.Naita("E-posti seadete salvestamine ei õnnestunud.", ex);
            }
        };
        Button eemalda = new Button
        {
            Text = "Eemalda salvestatud parool",
            Dock = DockStyle.Top
        };
        Vaade.Nupp(eemalda, false, true);
        eemalda.Click += (s, e) =>
        {
            if (MessageBox.Show("Kas eemaldada salvestatud e-posti parool?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                seaded.Salvesta("");
                parool.Clear();
                MessageBox.Show("Salvestatud parool eemaldatud.");
            }
            catch (Exception ex)
            {
                Vead.Naita("Parooli eemaldamine ei õnnestunud.", ex);
            }
        };
        Vaade.Lisa(paneel, salvesta);
        Vaade.Lisa(paneel, eemalda);
        Button loobu = new Button
        {
            Text = "Loobu",
            DialogResult = DialogResult.Cancel,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(loobu);
        Vaade.Lisa(paneel, loobu);
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(paneel)), 0, 1);
        AcceptButton = salvesta;
        CancelButton = loobu;
        try
        {
            seaded = PostiSeaded.Loe();
            server.Text = seaded.Server;
            saatja.Text = seaded.Saatja;
            kasutaja.Text = seaded.Kasutajanimi;
            port.Value = Math.Clamp(seaded.Port, 1, 65535);
        }
        catch (Exception ex)
        {
            Vead.Naita("E-posti seadete lugemine ei õnnestunud. Sisesta need uuesti.", ex);
        }

        ResumeLayout(true);
    }
}
