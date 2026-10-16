using System.Net;
using System.Net.Mail;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing.Layout;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly ListBox listOstukorv = new();
    private readonly Label lblKokku = new();
    private readonly TextBox txtEmail = new();
    private readonly Button btnEemalda = new(), btnGenPDF = new(), btnSaadaEmail = new();

    private readonly DateTime arveKuupaev = DateTime.Now;
    private readonly string arveNumber = DateTime.Now.ToString("yyyyMMddHHmmssfff");
    private string? viimanePdf;   // viimati loodud arve täistee

    public ArveForm()
    {
        InitializeComponent();
        Text = "Ostukorv ja arve";
        ClientSize = new Size(760, 380);

        listOstukorv.SetBounds(20, 20, 500, 300);
        listOstukorv.HorizontalScrollbar = true;
        lblKokku.SetBounds(20, 330, 500, 30);
        lblKokku.Font = new Font(Font, FontStyle.Bold);

        txtEmail.SetBounds(540, 20, 200, 27);
        txtEmail.PlaceholderText = "Saaja e-post";

        SeaNupp(btnEemalda, "Eemalda valitud rida", 70, btnEemalda_Click);
        SeaNupp(btnGenPDF, "Genereeri PDF", 115, btnGenPDF_Click);
        SeaNupp(btnSaadaEmail, "Saada e-postiga", 160, btnSaadaEmail_Click);

        Controls.AddRange(new Control[] { listOstukorv, lblKokku, txtEmail,
            btnEemalda, btnGenPDF, btnSaadaEmail });

        NaitaKorvi();
    }

    private void SeaNupp(Button nupp, string tekst, int y, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(540, y, 200, 35);
        nupp.Click += kasitleja;
    }

    private void NaitaKorvi()
    {
        listOstukorv.Items.Clear();
        foreach (var rida in Ostukorv.Ridad)
            listOstukorv.Items.Add(rida);   // ListBox kuvab OstukorviRida.ToString()

        lblKokku.Text = $"Kokku: {Ostukorv.Kokku:F2} €";
    }

    private void btnEemalda_Click(object? sender, EventArgs e)
    {
        if (listOstukorv.SelectedItem is OstukorviRida rida)
        {
            Ostukorv.Ridad.Remove(rida);
            viimanePdf = null;
            NaitaKorvi();
        }
    }

    // ---------- PDF ----------
    private void btnGenPDF_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0)
        {
            MessageBox.Show("Ostukorv on tühi. Lisa tooteid poes.");
            return;
        }

        string failinimi = Path.Combine(Kaustad.Arved, $"Arve_{arveNumber}.pdf");

        using var document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Arial", 10);
        JoonistaArvePais(gfx);

        int y = 228;
        foreach (var rida in Ostukorv.Ridad)
        {
            // Pikema nimetuse jaoks jätame rohkem ruumi.
            int reaKorgus = Math.Max(36,
                ((int)Math.Ceiling(gfx.MeasureString(rida.Toode.Toodenimetus, font).Width / 245) + 1) * 14 + 12);

            if (y + reaKorgus > page.Height - 150)
            {
                gfx.Dispose();
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                JoonistaArvePais(gfx);
                y = 228;
            }

            gfx.DrawRectangle(XPens.LightGray, 40, y, 260, reaKorgus);
            gfx.DrawRectangle(XPens.LightGray, 300, y, 60, reaKorgus);
            gfx.DrawRectangle(XPens.LightGray, 360, y, 90, reaKorgus);
            gfx.DrawRectangle(XPens.LightGray, 450, y, 105, reaKorgus);

            XTextFormatter tekst = new XTextFormatter(gfx);
            tekst.Alignment = XParagraphAlignment.Left;
            tekst.DrawString(rida.Toode.Toodenimetus, font, XBrushes.Black,
                new XRect(48, y + 8, 245, reaKorgus - 12),
                new TextFormatAlignment { Horizontal = XParagraphAlignment.Left });
            gfx.DrawString(rida.Kogus.ToString(), font, XBrushes.Black,
                new XRect(305, y, 50, reaKorgus), XStringFormats.Center);
            gfx.DrawString($"{rida.Toode.Hind:F2} €", font, XBrushes.Black,
                new XRect(365, y, 77, reaKorgus), XStringFormats.CenterRight);
            gfx.DrawString($"{rida.Summa:F2} €", font, XBrushes.Black,
                new XRect(455, y, 92, reaKorgus), XStringFormats.CenterRight);
            y += reaKorgus;
        }

        gfx.DrawRectangle(XBrushes.WhiteSmoke, 340, y + 20, 215, 44);
        XFont bold = new XFont("Arial", 12, XFontStyle.Bold);
        gfx.DrawString("KOKKU", bold, XBrushes.Black,
            new XRect(350, y + 20, 90, 44), XStringFormats.CenterLeft);
        gfx.DrawString($"{Ostukorv.Ridad.Sum(r => r.Summa):F2} €", bold, XBrushes.Black,
            new XRect(440, y + 20, 105, 44), XStringFormats.CenterRight);
        gfx.DrawString("Aitäh ostu eest!", font, XBrushes.Gray,
            new XRect(40, y + 85, 515, 20), XStringFormats.Center);

        gfx.Dispose();
        document.Save(failinimi);
        viimanePdf = failinimi;
        MessageBox.Show($"PDF on loodud:\n{failinimi}");
    }

    private void JoonistaArvePais(XGraphics gfx)
    {
        XFont font = new XFont("Arial", 10);
        XFont bold = new XFont("Arial", 10, XFontStyle.Bold);

        gfx.DrawString("ARVE", new XFont("Arial", 26, XFontStyle.Bold), XBrushes.Black,
            new XRect(40, 35, 200, 40), XStringFormats.TopLeft);
        gfx.DrawString($"Arve nr: {arveNumber}", bold, XBrushes.Black,
            new XRect(270, 40, 285, 20), XStringFormats.TopRight);
        gfx.DrawString($"Kuupäev: {arveKuupaev:dd.MM.yyyy HH:mm}", font, XBrushes.Black,
            new XRect(270, 65, 285, 20), XStringFormats.TopRight);
        gfx.DrawLine(XPens.Gray, 40, 95, 555, 95);

        gfx.DrawString("Müüja: Õppepood", bold, XBrushes.Black,
            new XRect(40, 115, 515, 20), XStringFormats.TopLeft);
        gfx.DrawString($"Ostja: {AktiivneKasutaja.Nimi}", font, XBrushes.Black,
            new XRect(40, 145, 515, 20), XStringFormats.TopLeft);
        gfx.DrawString(AktiivneKasutaja.Email, font, XBrushes.Black,
            new XRect(40, 165, 515, 20), XStringFormats.TopLeft);

        gfx.DrawRectangle(XBrushes.WhiteSmoke, 40, 200, 515, 28);
        gfx.DrawRectangle(XPens.LightGray, 40, 200, 260, 28);
        gfx.DrawRectangle(XPens.LightGray, 300, 200, 60, 28);
        gfx.DrawRectangle(XPens.LightGray, 360, 200, 90, 28);
        gfx.DrawRectangle(XPens.LightGray, 450, 200, 105, 28);
        gfx.DrawString("Toode", bold, XBrushes.Black,
            new XRect(48, 200, 245, 28), XStringFormats.CenterLeft);
        gfx.DrawString("Kogus", bold, XBrushes.Black,
            new XRect(300, 200, 60, 28), XStringFormats.Center);
        gfx.DrawString("Hind", bold, XBrushes.Black,
            new XRect(360, 200, 82, 28), XStringFormats.CenterRight);
        gfx.DrawString("Summa", bold, XBrushes.Black,
            new XRect(450, 200, 97, 28), XStringFormats.CenterRight);
    }

    // ---------- E-post ----------
    private void btnSaadaEmail_Click(object? sender, EventArgs e)
    {
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            MessageBox.Show("Genereeri enne PDF.");
            return;
        }

        string saaja = txtEmail.Text.Trim();
        if (!MailAddress.TryCreate(saaja, out _))
        {
            MessageBox.Show("Sisesta korrektne e-posti aadress.");
            return;
        }

        const string saatja = "xxxxx@gmail.com";
        const string rakenduseParool = "xxxxxx";   // vt 4.2, MITTE tavaline parool

        try
        {
            // using vabastab manuse, muidu jääb PDF-fail lukku
            using var kiri = new MailMessage(saatja, saaja, "Arve", "Arve on manuses.");
            kiri.Attachments.Add(new Attachment(viimanePdf));

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(saatja, rakenduseParool),
                EnableSsl = true
            };
            smtp.Send(kiri);

            MessageBox.Show("Arve saadetud!");
            Ostukorv.Tyhjenda();
            NaitaKorvi();
            viimanePdf = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }
    }
}
