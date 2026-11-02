using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly DataGridView tabel = new DataGridView();
    private readonly Label lblKokku = new Label();
    private readonly Label lblOlek = new Label();
    private readonly Label juhis = new Label();
    private readonly TextBox txtEmail = new TextBox();
    private readonly Button btnKinnita = new Button();
    private readonly Button btnEemalda = new Button();
    private readonly Button btnTyhjenda = new Button();
    private readonly Button btnPDF = new Button();
    private readonly Button btnAva = new Button();
    private readonly Button btnSaada = new Button();
    private Tellimus? tellimus;
    private string? viimanePdf;
    private bool kinnitamine;
    private bool saatmine;
    public ArveForm(int? tellimuseId = null)
    {
        if (AktiivneKasutaja.Id == 0)
        {
            throw new UnauthorizedAccessException();
        }

        InitializeComponent();
        LooLiides(tellimuseId);
    }

    private void LaeTellimus(int id)
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            tellimus = db.Tellimused.AsNoTracking().Include(t => t.Read).FirstOrDefault(t => t.Id == id && (AktiivneKasutaja.OnAdmin || t.KasutajaId == AktiivneKasutaja.Id));
            if (tellimus == null)
            {
                MessageBox.Show("Tellimust ei leitud või sul puudub ligipääs.");
                Shown += (s, e) => Close();
                return;
            }

            txtEmail.Text = tellimus.OstjaEmail;
            NaitaKorvi();
        }
        catch (Exception ex)
        {
            Vead.Naita("Tellimuse laadimine ei õnnestunud.", ex);
            Shown += (s, e) => Close();
        }
    }

    private void NaitaKorvi()
    {
        tabel.Rows.Clear();
        if (tellimus == null)
        {
            foreach (OstukorviRida r in Ostukorv.Ridad)
            {
                int i = tabel.Rows.Add(r.Nimetus, r.Kogus, r.Hind.ToString("F2"), r.Summa.ToString("F2"), r.Laos);
                tabel.Rows[i].Tag = r;
            }

            lblKokku.Text = $"{Ostukorv.Ridad.Count} erinevat toodet · {Ostukorv.KogusKokku} tk · Kokku {Ostukorv.Kokku:F2} €";
        }
        else
        {
            foreach (TellimuseRida r in tellimus.Read.OrderBy(r => r.Id))
            {
                tabel.Rows.Add(r.TooteNimi, r.Kogus, r.Hind.ToString("F2"), r.Summa.ToString("F2"), "—");
            }

            lblKokku.Text = $"Tellimus {tellimus.Number} · {tellimus.Kuupaev:dd.MM.yyyy HH:mm} · {tellimus.Staatus} · {tellimus.Summa:F2} €";
            Text = "Tellimus " + tellimus.Number;
        }

        if (tellimus != null)
        {
            juhis.Text = "Ost on kinnitatud. Arve saad avada või e-postiga saata.";
        }
        else if (Ostukorv.OstuKontrollVajalik)
        {
            juhis.Text = "Ostu kinnitus jäi saamata. Kontrolli enne jätkamist ostu tulemust.";
        }
        else if (Ostukorv.Ridad.Count == 0)
        {
            juhis.Text = "Ostukorv on tühi. Lisa poes tooteid.";
        }
        else
        {
            juhis.Text = "Koguse muutmiseks klõpsa koguse lahtril. Seejärel kinnita ost.";
        }
        btnTyhjenda.Visible = tellimus == null;
        btnEemalda.Visible = tellimus == null;
        btnKinnita.Visible = tellimus == null;
        txtEmail.Enabled = tellimus != null && !saatmine;
        tabel.Columns["Kogus"].DefaultCellStyle.BackColor = tellimus == null ? Color.FromArgb(238, 244, 255) : Color.White;
        bool muudetav = tellimus == null && !Ostukorv.OstuKontrollVajalik;
        tabel.ReadOnly = !muudetav;
        btnTyhjenda.Enabled = muudetav && Ostukorv.Ridad.Count > 0;
        btnEemalda.Enabled = muudetav && Ostukorv.Ridad.Count > 0;
        btnKinnita.Enabled = tellimus == null && Ostukorv.Ridad.Count > 0 && !kinnitamine;
        btnKinnita.Text = Ostukorv.OstuKontrollVajalik ? "Kontrolli ostu tulemust" : "Kinnita ost";
        btnSaada.Enabled = tellimus != null;
        btnAva.Enabled = tellimus != null;
        btnPDF.Enabled = tellimus != null;
    }

    private void KontrolliKogust(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (tellimus != null || e.RowIndex < 0 || tabel.Columns[e.ColumnIndex].Name != "Kogus" || tabel.ReadOnly)
        {
            return;
        }

        if (tabel.Rows[e.RowIndex].Tag is not OstukorviRida rida)
        {
            return;
        }

        if (!int.TryParse(e.FormattedValue?.ToString(), out int kogus) || kogus < 1)
        {
            e.Cancel = true;
            MessageBox.Show("Sisesta kogus vähemalt 1. Toote eemaldamiseks vajuta Eemalda valitud.");
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            Toode? toode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == rida.ToodeId);
            if (toode == null || !toode.Aktiivne || kogus > toode.Kogus)
            {
                e.Cancel = true;
                MessageBox.Show("Toode ei ole müügil või soovitud kogust ei ole laos. Vajadusel vajuta Esc ja eemalda rida.");
                return;
            }

            rida.Kogus = kogus;
            rida.Uuenda(toode);
        }
        catch (Exception ex)
        {
            e.Cancel = true;
            Vead.Naita("Laokoguse kontroll ei õnnestunud.", ex);
        }
    }

    private void Eemalda(object? sender, EventArgs e)
    {
        if (tellimus != null || Ostukorv.OstuKontrollVajalik)
        {
            return;
        }

        tabel.CancelEdit();
        if (tabel.CurrentRow?.Tag is OstukorviRida rida)
        {
            Ostukorv.Ridad.Remove(rida);
            NaitaKorvi();
        }
    }

    private void KinnitaOst(object? sender, EventArgs e)
    {
        if (kinnitamine || tellimus != null || !tabel.EndEdit() || Ostukorv.Ridad.Count == 0)
        {
            return;
        }

        kinnitamine = true;
        btnKinnita.Enabled = false;
        bool ostSalvestatud = false;
        try
        {
            using ToodedContext db = new ToodedContext();
            // Sama tunnus jääb alles ka siis, kui ühendus katkeb Commit vastuse ajal.
            Tellimus? eelmine = db.Tellimused.AsNoTracking().Include(t => t.Read).FirstOrDefault(t => t.OstuTunnus == Ostukorv.OstuTunnus && t.KasutajaId == AktiivneKasutaja.Id);
            if (eelmine != null)
            {
                tellimus = eelmine;
                Ostukorv.Tyhjenda();
                ostSalvestatud = true;
            }
            else if (Ostukorv.OstuKontrollVajalik)
            {
                Ostukorv.OstuKontrollVajalik = false;
                MessageBox.Show("Selle ostu tellimust ei leitud. Kontrolli korvi ja kinnita ost uuesti.");
            }
            else
            {
                bool hinnadMuutusid;
                Tellimus uus = OstuTeenus.Koosta(db, AktiivneKasutaja.Id, out hinnadMuutusid);
                if (hinnadMuutusid)
                {
                    MessageBox.Show("Mõne toote hind muutus. Vaata uus summa üle ja kinnita ost uuesti.");
                    return;
                }
                DialogResult vastus = MessageBox.Show("Kas kinnitad ostu summas " + uus.Summa.ToString("F2") + " €?",
                    "Kinnita ost", MessageBoxButtons.YesNo);
                if (vastus != DialogResult.Yes)
                {
                    return;
                }
                OstuTeenus.Salvesta(db, uus);
                tellimus = uus;
                Ostukorv.Tyhjenda();
                ostSalvestatud = true;
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            MessageBox.Show("Toote hind või kogus on muutunud. Ost jäi kinnitamata. Vaata ostukorv üle ja proovi uuesti.");
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message);
        }
        catch (Exception ex)
        {
            Vead.Naita(Ostukorv.OstuKontrollVajalik ? "Ostu kinnituse saamine katkes. Vajuta Kontrolli ostu tulemust." : "Ostu ei õnnestunud salvestada. Korv jäi alles.", ex);
        }
        finally
        {
            kinnitamine = false;
            NaitaKorvi();
        }

        if (ostSalvestatud)
        {
            lblOlek.Text = "Ost on kinnitatud. Nüüd saad arve avada või e-postiga saata.";
            LooPdf();
        }
    }

    private bool LooPdf()
    {
        if (tellimus == null)
        {
            return false;
        }

        try
        {
            viimanePdf = ArvePdf.Loo(tellimus);
            lblOlek.Text = "Arve on valmis. Võid selle avada või salvestada.";
            return true;
        }
        catch (Exception ex)
        {
            Vead.Naita("PDF-i loomine ei õnnestunud. Tellimus on endiselt salvestatud.", ex);
            return false;
        }
    }

    private void SalvestaPdf(object? sender, EventArgs e)
    {
        if (tellimus == null)
        {
            return;
        }
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            if (!LooPdf())
            {
                return;
            }
        }
        using SaveFileDialog aken = new SaveFileDialog();
        aken.Filter = "PDF-arve|*.pdf";
        aken.FileName = "Arve_" + tellimus.Number + ".pdf";
        if (aken.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        try
        {
            if (Path.GetFullPath(aken.FileName) != Path.GetFullPath(viimanePdf!))
            {
                File.Copy(viimanePdf!, aken.FileName, true);
            }
            lblOlek.Text = "Arve on salvestatud.";
        }
        catch (Exception ex)
        {
            Vead.Naita("Arvet ei saanud salvestada. Vali teine kaust ja proovi uuesti.", ex);
        }
    }

    private void AvaPdf()
    {
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            if (!LooPdf())
            {
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo(viimanePdf!) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Vead.Naita("PDF-i avamine ei õnnestunud. Fail asub kaustas " + Kaustad.Arved, ex);
        }
    }

    private async void SaadaEmail(object? sender, EventArgs e)
    {
        if (saatmine || tellimus == null)
        {
            return;
        }

        if (!MailAddress.TryCreate(txtEmail.Text.Trim(), out var saaja))
        {
            MessageBox.Show("Sisesta korrektne e-post.");
            return;
        }

        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            if (!LooPdf())
            {
                return;
            }
        }

        saatmine = true;
        btnAva.Enabled = false;
        btnPDF.Enabled = false;
        btnSaada.Enabled = false;
        txtEmail.Enabled = false;
        lblOlek.Text = "Saadan arvet…";
        try
        {
            PostiSeaded seaded = PostiSeaded.Loe();
            string saladus = seaded.LoeParool();
            if (string.IsNullOrWhiteSpace(seaded.Server) || string.IsNullOrWhiteSpace(seaded.Saatja) || string.IsNullOrEmpty(saladus))
            {
                string teade = "Poe e-post ei ole veel seadistatud. Arve saad salvestada oma arvutisse.";
                if (AktiivneKasutaja.OnAdmin)
                {
                    teade += " E-posti saad seadistada peamenüüs.";
                }
                MessageBox.Show(teade);
                lblOlek.Text = "E-post pole seadistatud.";
                return;
            }

            using MailMessage kiri = new MailMessage(seaded.Saatja, saaja.Address, "Arve " + tellimus.Number, "Tere! Õppepoe arve on manuses.");
            kiri.Attachments.Add(new Attachment(viimanePdf!));
            using SmtpClient smtp = new SmtpClient(seaded.Server, seaded.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(seaded.Kasutajanimi, saladus)
            };
            using CancellationTokenSource ajalopp = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await smtp.SendMailAsync(kiri, ajalopp.Token);
            lblOlek.Text = "Arve saadetud.";
        }
        catch (Exception ex)
        {
            lblOlek.Text = "Saatmine ebaõnnestus. PDF ja tellimus on alles.";
            Vead.Naita("Arvet ei saanud saata. Kontrolli e-posti seadeid ja vaata enne uuesti saatmist, kas kiri on juba kohale jõudnud.", ex);
        }
        finally
        {
            saatmine = false;
            txtEmail.Enabled = true;
            NaitaKorvi();
        }
    }

    private void btnTyhjenda_Click(object? s, EventArgs e)
    {
        if (tellimus != null || Ostukorv.OstuKontrollVajalik)
        {
            return;
        }

        if (MessageBox.Show("Kas tühjendan ostukorvi?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
        {
            return;
        }

        tabel.CancelEdit();
        Ostukorv.Tyhjenda();
        NaitaKorvi();
    }

    private void tabel_CellEndEdit(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || tabel.Rows[e.RowIndex].Tag is not OstukorviRida r)
        {
            return;
        }

        DataGridViewRow rida = tabel.Rows[e.RowIndex];
        rida.Cells["Kogus"].Value = r.Kogus;
        rida.Cells["Hind"].Value = r.Hind.ToString("F2");
        rida.Cells["Summa"].Value = r.Summa.ToString("F2");
        rida.Cells["Laos"].Value = r.Laos;
        lblKokku.Text = $"{Ostukorv.Ridad.Count} erinevat toodet · {Ostukorv.KogusKokku} tk · Kokku {Ostukorv.Kokku:F2} €";
    }

    private void LooLiides(int? tellimuseId = null)
    {
        Vaade.Vorm(this, "Ostukorv ja arve", 1200, 780);
        MinimumSize = new Size(1000, 620);
        TableLayoutPanel raam = Vaade.Raam(this, "Ostukorv ja arve", "Vaata tooted üle, kinnita ost ning salvesta või saada arve.");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
        raam.Controls.Add(sisu, 0, 1);
        TableLayoutPanel vasak = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 16, 0)
        };
        vasak.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        vasak.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        vasak.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        FlowLayoutPanel korvinupud = Vaade.Nupuriba();
        btnKinnita.Text = "Kinnita ost";
        btnKinnita.Click += KinnitaOst;
        btnEemalda.CausesValidation = false;
        btnTyhjenda.CausesValidation = false;
        btnEemalda.Text = "Eemalda valitud";
        btnEemalda.Click += Eemalda;
        btnTyhjenda.Text = "Tühjenda ostukorv";
        btnTyhjenda.Click += btnTyhjenda_Click;
        Vaade.Nupp(btnEemalda, false, true);
        Vaade.Nupp(btnTyhjenda, false, true);
        korvinupud.Controls.AddRange(new Control[] { btnEemalda, btnTyhjenda });
        vasak.Controls.Add(tabel, 0, 0);
        vasak.Controls.Add(korvinupud, 0, 1);
        sisu.Controls.Add(vasak, 0, 0);
        TableLayoutPanel parem = Vaade.Veerg();
        TableLayoutPanel ost = Vaade.Veerg();
        Vaade.Lisa(ost, Vaade.Tekstirida("Ostu kokkuvõte", true));
        lblKokku.AutoSize = true;
        lblKokku.Dock = DockStyle.Top;
        lblKokku.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblKokku.Margin = new Padding(0, 0, 0, 16);
        Vaade.Lisa(ost, lblKokku);
        juhis.AutoSize = true;
        juhis.Dock = DockStyle.Top;
        juhis.ForeColor = Vaade.Vaikne;
        juhis.Margin = new Padding(0, 0, 0, 12);
        Vaade.Lisa(ost, juhis);
        Vaade.Nupp(btnKinnita, true);
        btnKinnita.Dock = DockStyle.Top;
        Vaade.Lisa(ost, btnKinnita);
        Vaade.Lisa(parem, Vaade.Kaart(ost));
        TableLayoutPanel arve = Vaade.Veerg();
        Vaade.Lisa(arve, Vaade.Tekstirida("PDF-arve", true));
        Vaade.Lisa(arve, Vaade.Tekstirida("Arve loomine ja saatmine on saadaval pärast ostu kinnitamist."));
        btnPDF.Text = "Salvesta arve";
        btnPDF.Click += SalvestaPdf;
        btnAva.Text = "Ava PDF";
        btnAva.Click += (s, e) => AvaPdf();
        FlowLayoutPanel pdfnupud = Vaade.Nupuriba();
        Vaade.Nupp(btnPDF);
        Vaade.Nupp(btnAva);
        pdfnupud.Controls.AddRange(new Control[] { btnPDF, btnAva });
        Vaade.Lisa(arve, pdfnupud);
        txtEmail.PlaceholderText = "nimi@näide.ee";
        txtEmail.Text = AktiivneKasutaja.Email;
        Vaade.Vali(arve, "Arve saaja e-post", txtEmail);
        btnSaada.Text = "Saada arve e-postiga";
        btnSaada.Click += SaadaEmail;
        Vaade.Nupp(btnSaada);
        btnSaada.Dock = DockStyle.Top;
        Vaade.Lisa(arve, btnSaada);
        lblOlek.AutoSize = true;
        lblOlek.Dock = DockStyle.Top;
        lblOlek.ForeColor = Vaade.Vaikne;
        lblOlek.Margin = new Padding(0, 8, 0, 4);
        Vaade.Lisa(arve, lblOlek);
        Vaade.Lisa(parem, Vaade.Kaart(arve));
        sisu.Controls.Add(Vaade.Kerimine(parem), 1, 0);
        tabel.Dock = DockStyle.Fill;
        tabel.AllowUserToAddRows = false;
        tabel.AllowUserToDeleteRows = false;
        tabel.MultiSelect = false;
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabel.Columns.Add("Nimetus", "Toode");
        tabel.Columns.Add("Kogus", "Kogus (muudetav)");
        tabel.Columns.Add("Hind", "Ühiku hind €");
        tabel.Columns.Add("Summa", "Summa €");
        tabel.Columns.Add("Laos", "Saadaval");
        foreach (DataGridViewColumn veerg in tabel.Columns)
        {
            veerg.ReadOnly = veerg.Name != "Kogus";
        }

        tabel.Columns["Nimetus"].FillWeight = 220;
        tabel.CellValidating += KontrolliKogust;
        tabel.CellEndEdit += tabel_CellEndEdit;
        tabel.DataError += (s, e) =>
        {
            e.ThrowException = false;
            MessageBox.Show("Sisesta korrektne täisarv.");
        };
        Vaade.Tabel(tabel);
        Vaade.VeeruNimi(tabel, "Nimetus", "Toode", 220);
        Vaade.VeeruNimi(tabel, "Kogus", "Kogus (tk)", 75);
        Vaade.VeeruNimi(tabel, "Hind", "Hind (€)", 75);
        Vaade.VeeruNimi(tabel, "Summa", "Summa (€)", 80);
        Vaade.VeeruNimi(tabel, "Laos", "Saadaval (tk)", 75);
        tabel.Columns["Kogus"].DefaultCellStyle.BackColor = Color.FromArgb(238, 244, 255);
        foreach (string nimi in new[]
        {
            "Kogus",
            "Hind",
            "Summa",
            "Laos"
        })
        {
            tabel.Columns[nimi].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        FormClosing += (s, e) =>
        {
            if (saatmine || kinnitamine)
            {
                e.Cancel = true;
                MessageBox.Show("Oota toimingu lõppu.");
            }
        };
        if (tellimuseId != null)
        {
            LaeTellimus(tellimuseId.Value);
        }
        else
        {
            NaitaKorvi();
        }

        ResumeLayout(true);
    }
}
