using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly FlowLayoutPanel readPaneel = new();
    private readonly Label lblKokku = new();
    private readonly Label lblRead = new();
    private readonly Label lblOlek = new();
    private readonly Label juhis = new();
    private readonly Label pealkiri = new();
    private readonly Label reaSumma = new();
    private readonly Label soodustus = new();
    private readonly Label soodustuseSilt = new();
    private readonly TextBox txtEmail = new();
    private readonly KorviNupp btnKinnita = new() { Pohiline = true };
    private readonly KorviNupp btnTyhjenda = new() { Ikoon = "icons/trash_bin_small.svg" };
    private readonly KorviNupp btnPDF = new() { Ikoon = "icons/ic_download_invoice.svg" };
    private readonly KorviNupp btnAva = new();
    private readonly KorviNupp btnSaada = new() { Pohiline = true };
    private readonly KorviNupp btnKirjaEelvaade = new();
    private readonly KorviNupp btnSaateleht = new();
    private readonly ComboBox kirjaLiik = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private TellimuseKirjaLiik ValitudKirjaLiik => (TellimuseKirjaLiik)Math.Max(0, kirjaLiik.SelectedIndex);
    private readonly TableLayoutPanel arvePaneel = new();
    private readonly CancellationTokenSource elu = new();
    private readonly Dictionary<int, Toode> tooteAndmed = new();
    private readonly int kasutajaId;
    private Tellimus? tellimus;
    private string? viimanePdf;
    private bool kinnitamine;
    private bool saatmine;
    private bool koguseMuutmine;

    public ArveForm(int? tellimuseId = null)
    {
        if (AktiivneKasutaja.Id == 0) throw new UnauthorizedAccessException();
        kasutajaId = AktiivneKasutaja.Id;
        InitializeComponent();
        LooLiides();
        if (tellimuseId.HasValue) LaeTellimus(tellimuseId.Value);
        else NaitaKorvi();
        Shown += async (_, _) => await LaeTooteAndmedAsync();
    }

    private void LaeTellimus(int id)
    {
        try
        {
            using var db = new ToodedContext();
            tellimus = db.Tellimused.AsNoTracking().Include(t => t.Read)
                .FirstOrDefault(t => t.Id == id && (AktiivneKasutaja.OnAdmin || t.KasutajaId == AktiivneKasutaja.Id));
            if (tellimus == null)
            {
                Vaade.Teade("Tellimust ei leitud või sul puudub ligipääs.");
                Shown += (_, _) => Close();
                return;
            }
            txtEmail.Text = tellimus.OstjaEmail;
            NaitaKorvi();
        }
        catch (Exception ex)
        {
            Vead.Naita("Tellimuse laadimine ei õnnestunud.", ex);
            Shown += (_, _) => Close();
        }
    }

    private async Task LaeTooteAndmedAsync()
    {
        try
        {
            int[] ids = tellimus == null ? Ostukorv.Ridad.Select(r => r.ToodeId).ToArray() : tellimus.Read.Select(r => r.ToodeId).ToArray();
            using var db = new ToodedContext();
            var products = await db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id)).ToListAsync(elu.Token);
            if (IsDisposed) return;
            foreach (var product in products) tooteAndmed[product.Id] = product;
            if (!readPaneel.Controls.OfType<KorviTooteRida>().Any(r => r.SisestatudKogus != r.Kogus)) NaitaKorvi();
        }
        catch (OperationCanceledException) { }
        catch { if (!IsDisposed) lblOlek.Text = "Mõne toote pilti või lisateavet ei saanud laadida. Tellimuse saad siiski üle vaadata."; }
    }

    private void NaitaKorvi()
    {
        if (IsDisposed) return;
        var scroll = new Point(-readPaneel.AutoScrollPosition.X, -readPaneel.AutoScrollPosition.Y);
        readPaneel.SuspendLayout();
        while (readPaneel.Controls.Count > 0)
        {
            var control = readPaneel.Controls[0]; readPaneel.Controls.Remove(control); control.Dispose();
        }
        bool loetav = tellimus != null || Ostukorv.OstuKontrollVajalik;
        decimal kokku = tellimus?.Summa ?? Ostukorv.Kokku;
        decimal kokkuSoodustus = 0;
        if (tellimus == null)
        {
            foreach (var rida in Ostukorv.Ridad)
            {
                tooteAndmed.TryGetValue(rida.ToodeId, out var toode);
                decimal? tavahind = toode?.Hind == rida.Hind && toode.Tavahind > rida.Hind ? toode.Tavahind : null;
                if (tavahind.HasValue) kokkuSoodustus += (tavahind.Value - rida.Hind) * rida.Kogus;
                var row = new KorviTooteRida(rida.Nimetus, rida.Kogus, rida.Hind, tavahind, toode?.Uhik ?? "tk",
                    toode, rida, loetav, value => SeaKogusAsync(rida, value), () => EemaldaRida(rida));
                readPaneel.Controls.Add(row);
            }
            if (Ostukorv.Ridad.Count == 0)
            {
                readPaneel.Controls.Add(new KorviTyhiVaade(() => Close()));
            }
            lblRead.Text = $"{Vaade.Tooteid(Ostukorv.Ridad.Count)} · {Ostukorv.KogusKokku} tk";
            pealkiri.Text = "Ostukorv";
        }
        else
        {
            foreach (var rida in tellimus.Read.OrderBy(r => r.Id))
            {
                tooteAndmed.TryGetValue(rida.ToodeId, out var toode);
                readPaneel.Controls.Add(new KorviTooteRida(rida.TooteNimi, rida.Kogus, rida.Hind, null, "tk",
                    toode, null, true, _ => Task.FromResult(false), () => { }));
            }
            lblRead.Text = $"{tellimus.Number} · {tellimus.Kuupaev:dd.MM.yyyy HH:mm} · {tellimus.Staatus}";
            pealkiri.Text = "Tellimus on kinnitatud";
            Text = "BARBORA · Tellimus " + tellimus.Number;
        }
        lblKokku.Text = $"{kokku:N2} €";
        reaSumma.Text = $"{kokku + kokkuSoodustus:N2} €";
        soodustus.Text = $"−{kokkuSoodustus:N2} €";
        soodustus.Visible = soodustuseSilt.Visible = kokkuSoodustus > 0;
        juhis.Text = tellimus != null ? "Aitäh tellimuse eest! Arve saad avada, salvestada või saata e-postiga."
            : Ostukorv.OstuKontrollVajalik ? "Ostu kinnitus jäi saamata. Kontrolli enne jätkamist ostu tulemust."
            : Ostukorv.Ridad.Count == 0 ? "Lisa ostukorvi tooteid, et tellimust vormistada."
            : "Toodete hinnad ja saadavus kontrollitakse enne tellimuse kinnitamist.";
        btnTyhjenda.Visible = tellimus == null && Ostukorv.Ridad.Count > 0;
        btnTyhjenda.Enabled = !loetav && !koguseMuutmine && !kinnitamine;
        btnKinnita.Visible = tellimus == null;
        btnKinnita.Enabled = tellimus == null && Ostukorv.Ridad.Count > 0 && !kinnitamine && !koguseMuutmine;
        btnKinnita.Text = Ostukorv.OstuKontrollVajalik ? "Kontrolli ostu tulemust" : "Vormista tellimus";
        readPaneel.Enabled = !kinnitamine && !koguseMuutmine;
        arvePaneel.Visible = tellimus != null;
        txtEmail.Enabled = tellimus != null && !saatmine;
        btnSaada.Enabled = btnAva.Enabled = btnPDF.Enabled = tellimus != null && !saatmine;
        btnKirjaEelvaade.Enabled = btnSaateleht.Enabled = kirjaLiik.Enabled = tellimus != null && !saatmine;
        MahutaRead();
        readPaneel.ResumeLayout(true);
        readPaneel.AutoScrollPosition = scroll;
    }

    private bool ridadePaigutus;
    private void MahutaRead()
    {
        if (ridadePaigutus) return;
        ridadePaigutus = true;
        try
        {
            readPaneel.SuspendLayout();
            int width = Math.Max(100, readPaneel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - readPaneel.Padding.Horizontal);
            foreach (Control row in readPaneel.Controls)
                if (row is KorviTooteRida or KorviTyhiVaade) row.Width = width;
            readPaneel.ResumeLayout(true);
            readPaneel.PerformLayout();
        }
        finally { ridadePaigutus = false; }
    }

    private void EemaldaRida(OstukorviRida rida)
    {
        if (tellimus != null || Ostukorv.OstuKontrollVajalik || koguseMuutmine || kinnitamine) return;
        Ostukorv.Ridad.Remove(rida); lblOlek.Text = "Toode eemaldatud."; NaitaKorvi();
    }

    private Task<bool> MuudaKogustAsync(OstukorviRida rida, int vahe)
    {
        long uus = (long)rida.Kogus + vahe;
        if (uus is < 0 or > int.MaxValue)
        {
            lblOlek.Text = "Sisesta väiksem kogus.";
            return Task.FromResult(false);
        }
        return SeaKogusAsync(rida, (int)uus);
    }

    private async Task<bool> SeaKogusAsync(OstukorviRida rida, int kogus)
    {
        if (tellimus != null || Ostukorv.OstuKontrollVajalik || koguseMuutmine || kinnitamine || !Ostukorv.Ridad.Contains(rida)) return false;
        if (kogus == 0) { EemaldaRida(rida); return true; }
        if (kogus < 1) { lblOlek.Text = "Kogus peab olema vähemalt 1."; return false; }
        koguseMuutmine = true; btnKinnita.Enabled = btnTyhjenda.Enabled = readPaneel.Enabled = false;
        try
        {
            using var db = new ToodedContext();
            var toode = await db.Toodetabel.AsNoTracking().FirstOrDefaultAsync(t => t.Id == rida.ToodeId, elu.Token);
            elu.Token.ThrowIfCancellationRequested();
            if (IsDisposed || AktiivneKasutaja.Id != kasutajaId) return false;
            if (toode == null || !toode.Aktiivne || kogus > toode.Kogus)
            {
                lblOlek.Text = "Soovitud kogust pole enam saadaval. Vähenda kogust või eemalda toode."; return false;
            }
            rida.Kogus = kogus; rida.Uuenda(toode); tooteAndmed[toode.Id] = toode;
            lblOlek.Text = "Ostukorv on uuendatud."; return true;
        }
        catch (OperationCanceledException) { return false; }
        catch { if (!IsDisposed) lblOlek.Text = "Kogust ei saanud muuta. Proovi uuesti."; return false; }
        finally { koguseMuutmine = false; if (!IsDisposed) NaitaKorvi(); }
    }

    private async Task<bool> ViiKogusedLopuniAsync()
    {
        var muudatused = readPaneel.Controls.OfType<KorviTooteRida>()
            .Where(r => r.KorviRida != null && r.SisestatudKogus != r.Kogus)
            .Select(r => (Rida: r.KorviRida!, Kogus: r.SisestatudKogus)).ToList();
        if (muudatused.Count == 0) return true;
        if (muudatused.Any(m => !m.Kogus.HasValue)) { lblOlek.Text = "Sisesta korrektne kogus vähemalt 1."; return false; }
        foreach (var m in muudatused) if (!await SeaKogusAsync(m.Rida, m.Kogus!.Value)) return false;
        lblOlek.Text = "Kogused on uuendatud. Vaata uus summa üle ja kinnita tellimus.";
        return false;
    }

    private async void KinnitaOst(object? sender, EventArgs e)
    {
        if (kinnitamine || koguseMuutmine || tellimus != null || Ostukorv.Ridad.Count == 0)
        {
            return;
        }

        if (!await ViiKogusedLopuniAsync()) return;
        kinnitamine = true;
        readPaneel.Enabled = false;
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
                Vaade.Teade("Selle ostu tellimust ei leitud. Kontrolli korvi ja kinnita ost uuesti.");
            }
            else
            {
                bool hinnadMuutusid;
                Tellimus uus = OstuTeenus.Koosta(db, AktiivneKasutaja.Id, out hinnadMuutusid);
                if (hinnadMuutusid)
                {
                    Vaade.Teade("Mõne toote hind muutus. Vaata uus summa üle ja kinnita ost uuesti.");
                    return;
                }
                DialogResult vastus = Vaade.Teade("Kas kinnitad ostu summas " + uus.Summa.ToString("F2") + " €?",
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
            Vaade.Teade("Toote hind või kogus on muutunud. Ost jäi kinnitamata. Vaata ostukorv üle ja proovi uuesti.");
        }
        catch (ArgumentException ex)
        {
            Vaade.Teade(ex.Message);
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
            Vaade.Teade("Sisesta korrektne e-post.");
            return;
        }

        var liik = ValitudKirjaLiik;
        if (liik == TellimuseKirjaLiik.Arve && (viimanePdf == null || !File.Exists(viimanePdf)))
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
        btnKirjaEelvaade.Enabled = btnSaateleht.Enabled = kirjaLiik.Enabled = false;
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
                Vaade.Teade(teade);
                lblOlek.Text = "E-post pole seadistatud.";
                return;
            }

            var rekvisiidid = DokumendiSeaded.Loe();
            string? attachment = liik == TellimuseKirjaLiik.Saateleht ? ArvePdf.LooSaateleht(tellimus, rekvisiidid)
                : liik == TellimuseKirjaLiik.Arve ? viimanePdf : null;
            using MailMessage kiri = TellimuseKiri.Loo(tellimus, seaded.Saatja, saaja.Address, attachment, rekvisiidid, liik);
            using SmtpClient smtp = new SmtpClient(seaded.Server, seaded.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(seaded.Kasutajanimi, saladus)
            };
            using CancellationTokenSource ajalopp = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await smtp.SendMailAsync(kiri, ajalopp.Token);
            lblOlek.Text = "Kiri saadetud.";
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

    private void AvaKirjaEelvaade()
    {
        if (tellimus == null || saatmine) return;
        try
        {
            var settings = DokumendiSeaded.Loe();
            string contact = settings.Email;
            if (string.IsNullOrEmpty(contact)) contact = PostiSeaded.Loe().Saatja;
            if (string.IsNullOrEmpty(contact)) contact = AktiivneKasutaja.Email;
            string folder = Path.Combine(Kaustad.Juur, "KirjadeEelvaated");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, $"kiri-{tellimus.Id}-{Guid.NewGuid():N}.html");
            File.WriteAllText(path, TellimuseKiri.Eelvaade(tellimus, contact, settings, ValitudKirjaLiik), System.Text.Encoding.UTF8);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) { Vead.Naita("Kirja eelvaadet ei saanud avada.", ex); }
    }

    private void AvaSaateleht()
    {
        if (tellimus == null || saatmine) return;
        try { Process.Start(new ProcessStartInfo(ArvePdf.LooSaateleht(tellimus)) { UseShellExecute = true }); }
        catch (Exception ex) { Vead.Naita("Saatelehte ei saanud avada.", ex); }
    }

    private void btnTyhjenda_Click(object? s, EventArgs e)
    {
        if (tellimus != null || Ostukorv.OstuKontrollVajalik || koguseMuutmine || kinnitamine)
        {
            return;
        }

        if (Vaade.Teade("Kas tühjendan ostukorvi?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
        {
            return;
        }

        Ostukorv.Tyhjenda();
        NaitaKorvi();
    }


    private static Label Tekst(string text, float size = 10, bool bold = false)
        => new()
        {
            Text = text,
            AutoSize = true,
            Dock = DockStyle.Top,
            UseMnemonic = false,
            Font = BarboraTheme.Font(size, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = BarboraTheme.Text,
            Margin = new Padding(0, 0, 0, 12)
        };

    private static void Lisa(TableLayoutPanel panel, Control child)
    {
        int row = panel.RowCount++; panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); panel.Controls.Add(child, 0, row);
    }

    private static TableLayoutPanel Veerg(int padding = 0)
    {
        var result = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 0,
            BackColor = Color.White,
            Padding = new Padding(padding),
            Margin = Padding.Empty
        };
        result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); return result;
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "BARBORA · Ostukorv", 1320, 880);
        Font = BarboraTheme.Font(10); BackColor = BarboraTheme.Background;
        MinimumSize = new Size(1000, 680);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.White,
            Padding = new Padding(30, 20, 30, 18),
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 192));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        var logo = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BarboraTheme.Asset("logo/barbora_next_app_bar_logo.svg"),
            Margin = new Padding(0, 7, 20, 7),
            AccessibleName = "BARBORA"
        };
        pealkiri.Font = BarboraTheme.Font(23, FontStyle.Bold); pealkiri.Dock = DockStyle.Fill;
        pealkiri.TextAlign = ContentAlignment.MiddleLeft; pealkiri.Margin = new Padding(20, 0, 0, 0);
        var close = new KorviNupp { Text = "Jätka ostlemist", Ikoon = "icons/ic_arrow_back.svg", Dock = DockStyle.Fill };
        close.Click += (_, _) => Close();
        header.Controls.Add(logo, 0, 0); header.Controls.Add(pealkiri, 1, 0); header.Controls.Add(close, 2, 0);
        root.Controls.Add(header, 0, 0);
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(28, 24, 28, 12),
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 366));
        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 22, 0),
            Padding = new Padding(8, 0, 8, 0)
        };
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 72)); left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var listHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(18, 15, 12, 12) };
        listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 206));
        lblRead.Font = BarboraTheme.Font(11, FontStyle.Bold); lblRead.Dock = DockStyle.Fill; lblRead.TextAlign = ContentAlignment.MiddleLeft;
        lblRead.UseMnemonic = false; lblRead.AutoEllipsis = true;
        btnTyhjenda.Text = "Tühjenda ostukorv"; btnTyhjenda.Dock = DockStyle.Fill; btnTyhjenda.Click += btnTyhjenda_Click;
        listHeader.Controls.Add(lblRead, 0, 0); listHeader.Controls.Add(btnTyhjenda, 1, 0);
        readPaneel.Dock = DockStyle.Fill; readPaneel.AutoScroll = true; readPaneel.WrapContents = false;
        readPaneel.FlowDirection = FlowDirection.TopDown; readPaneel.Margin = Padding.Empty; readPaneel.Padding = Padding.Empty;
        readPaneel.Resize += (_, _) => MahutaRead();
        left.Controls.Add(listHeader, 0, 0); left.Controls.Add(readPaneel, 0, 1); content.Controls.Add(left, 0, 0);
        var rightScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        var right = Veerg(); right.BackColor = BarboraTheme.Background;
        var summary = Veerg(24); summary.Margin = new Padding(0, 0, 0, 16);
        Lisa(summary, Tekst("Tellimuse kokkuvõte", 15, true));
        var sums = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 3, Margin = new Padding(0, 7, 0, 18) };
        sums.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); sums.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        for (int i = 0; i < 3; i++) sums.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var priceCaption = Tekst("Toodete summa");
        reaSumma.Font = BarboraTheme.Font(10); reaSumma.AutoSize = true; reaSumma.Dock = DockStyle.Top; reaSumma.TextAlign = ContentAlignment.TopRight;
        soodustuseSilt.Text = "Allahindlus"; soodustuseSilt.Font = BarboraTheme.Font(10); soodustuseSilt.ForeColor = BarboraTheme.Red;
        soodustuseSilt.AutoSize = true; soodustuseSilt.Dock = DockStyle.Top; soodustuseSilt.Margin = new Padding(0, 0, 0, 14);
        soodustus.Font = BarboraTheme.Font(10); soodustus.ForeColor = BarboraTheme.Red; soodustus.AutoSize = true; soodustus.Dock = DockStyle.Top; soodustus.TextAlign = ContentAlignment.TopRight;
        sums.Controls.Add(priceCaption, 0, 0); sums.Controls.Add(reaSumma, 1, 0); sums.Controls.Add(soodustuseSilt, 0, 1); sums.Controls.Add(soodustus, 1, 1);
        var totalTitle = Tekst("Kokku", 13, true); totalTitle.Margin = new Padding(0, 12, 0, 0);
        lblKokku.Font = BarboraTheme.Font(23, FontStyle.Bold); lblKokku.AutoSize = true; lblKokku.Dock = DockStyle.Top; lblKokku.TextAlign = ContentAlignment.TopRight;
        lblKokku.Margin = new Padding(0, 6, 0, 0);
        sums.Controls.Add(totalTitle, 0, 2); sums.Controls.Add(lblKokku, 1, 2); Lisa(summary, sums);
        btnKinnita.Text = "Vormista tellimus"; btnKinnita.Height = 50; btnKinnita.Dock = DockStyle.Top; btnKinnita.Margin = new Padding(0, 0, 0, 16); btnKinnita.Click += KinnitaOst;
        Lisa(summary, btnKinnita);
        juhis.Font = BarboraTheme.Font(9); juhis.ForeColor = BarboraTheme.Muted; juhis.AutoSize = true; juhis.Dock = DockStyle.Top;
        juhis.MaximumSize = new Size(310, 0); juhis.Margin = Padding.Empty; Lisa(summary, juhis);
        Lisa(right, summary);
        arvePaneel.Dock = DockStyle.Top; arvePaneel.AutoSize = true; arvePaneel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        arvePaneel.BackColor = Color.White; arvePaneel.Padding = new Padding(24); arvePaneel.ColumnCount = 1; arvePaneel.RowCount = 0;
        arvePaneel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); arvePaneel.Margin = new Padding(0, 0, 0, 16);
        Lisa(arvePaneel, Tekst("Sinu tellimuse arve", 14, true));
        var success = new PictureBox { Image = BarboraTheme.Asset("order_summary/order_success.svg"), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Top, Height = 64, Margin = new Padding(0, 0, 0, 16) };
        Lisa(arvePaneel, success);
        btnPDF.Text = "Salvesta PDF-arve"; btnPDF.Dock = DockStyle.Top; btnPDF.Margin = new Padding(0, 0, 0, 10); btnPDF.Click += SalvestaPdf;
        btnAva.Text = "Ava arve"; btnAva.Dock = DockStyle.Top; btnAva.Margin = new Padding(0, 0, 0, 18); btnAva.Click += (_, _) => AvaPdf();
        btnSaateleht.Text = "Ava PDF-saateleht"; btnSaateleht.Dock = DockStyle.Top; btnSaateleht.Margin = new Padding(0, 0, 0, 18);
        btnSaateleht.Click += (_, _) => AvaSaateleht();
        Lisa(arvePaneel, btnPDF); Lisa(arvePaneel, btnAva); Lisa(arvePaneel, btnSaateleht);
        Lisa(arvePaneel, Tekst("Kirja liik", 9));
        kirjaLiik.Items.AddRange(["Arve PDF-iga", "Tellimuse kinnitus", "Saateleht PDF-iga"]);
        kirjaLiik.Font = BarboraTheme.Font(10); kirjaLiik.Dock = DockStyle.Top; kirjaLiik.Margin = new Padding(0, 0, 0, 14);
        kirjaLiik.AccessibleName = "Kirja liik"; kirjaLiik.SelectedIndex = 0;
        Lisa(arvePaneel, kirjaLiik); Lisa(arvePaneel, Tekst("Kirja saaja e-post", 9));
        txtEmail.Font = BarboraTheme.Font(10); txtEmail.BorderStyle = BorderStyle.FixedSingle; txtEmail.Text = AktiivneKasutaja.Email;
        txtEmail.PlaceholderText = "nimi@näide.ee"; txtEmail.MaxLength = 254; txtEmail.Dock = DockStyle.Top; txtEmail.Margin = new Padding(0, 0, 0, 12); txtEmail.AccessibleName = "Arve saaja e-post";
        Lisa(arvePaneel, txtEmail);
        btnKirjaEelvaade.Text = "Vaata kirja eelvaadet"; btnKirjaEelvaade.Dock = DockStyle.Top; btnKirjaEelvaade.Margin = new Padding(0, 0, 0, 10);
        btnKirjaEelvaade.Click += (_, _) => AvaKirjaEelvaade(); Lisa(arvePaneel, btnKirjaEelvaade);
        btnSaada.Text = "Saada arve e-postiga"; btnSaada.Dock = DockStyle.Top; btnSaada.Click += SaadaEmail; Lisa(arvePaneel, btnSaada);
        kirjaLiik.SelectedIndexChanged += (_, _) => btnSaada.Text = ValitudKirjaLiik == TellimuseKirjaLiik.Arve ? "Saada arve e-postiga" : "Saada kiri e-postiga";
        Lisa(right, arvePaneel);
        lblOlek.Font = BarboraTheme.Font(9); lblOlek.ForeColor = BarboraTheme.Muted; lblOlek.AutoSize = true; lblOlek.Dock = DockStyle.Top;
        lblOlek.MaximumSize = new Size(340, 0); lblOlek.Margin = new Padding(8, 0, 8, 12); Lisa(right, lblOlek);
        rightScroll.Controls.Add(right); content.Controls.Add(rightScroll, 1, 0); root.Controls.Add(content, 0, 1);
        var footer = Tekst("BARBORA · Kohalik õppeprototüüp", 8); footer.ForeColor = BarboraTheme.Muted;
        footer.Dock = DockStyle.Fill; footer.TextAlign = ContentAlignment.MiddleCenter; root.Controls.Add(footer, 0, 2);
        Controls.Add(root);
        FormClosing += (_, e) => { if (saatmine || kinnitamine) { e.Cancel = true; Vaade.Teade("Oota toimingu lõppu."); } };
        ResumeLayout(true);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        elu.Cancel();
        base.OnFormClosed(e);
    }
}
