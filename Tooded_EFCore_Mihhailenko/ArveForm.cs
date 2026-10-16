using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing.Layout;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    public partial class ArveForm : Form
    {
        private DateTime arveKuupaev = DateTime.Now;
        private string arveNumber = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        private string? pdfPath;

        public ArveForm()
        {
            InitializeComponent();
        }

        private void ArveForm_Load(object sender, EventArgs e)
        {
            lblArveNumber.Text = $"Arve nr: {arveNumber}";
            lblKuupaev.Text = $"Kuupäev: {arveKuupaev:dd.MM.yyyy HH:mm}";
            lblOstja.Text = $"Ostja: {AktiivneKasutaja.Nimi}\n{AktiivneKasutaja.Email}";
            dataGridViewArve.Rows.Clear();

            foreach (var rida in Ostukorv.Read)
            {
                dataGridViewArve.Rows.Add(rida.Toode.Toodenimetus,
                    rida.Kogus, $"{rida.Toode.Hind:F2} €", $"{rida.Summa:F2} €");
            }

            lblKokku.Text = $"KOKKU: {Ostukorv.Read.Sum(r => r.Summa):F2} €";
        }

        private void btnTagasi_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGenPDF_Click(object sender, EventArgs e)
        {
            if (Ostukorv.Read.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi. Lisa tooteid poes.");
                return;
            }

            Directory.CreateDirectory("Arved");
            string failinimi = $"Arved/Arve_{arveNumber}.pdf";

            using var document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 10);
            JoonistaArvePais(gfx);

            int y = 228;
            foreach (var rida in Ostukorv.Read)
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
            gfx.DrawString($"{Ostukorv.Read.Sum(r => r.Summa):F2} €", bold, XBrushes.Black,
                new XRect(440, y + 20, 105, 44), XStringFormats.CenterRight);
            gfx.DrawString("Aitäh ostu eest!", font, XBrushes.Gray,
                new XRect(40, y + 85, 515, 20), XStringFormats.Center);

            gfx.Dispose();
            document.Save(failinimi);
            pdfPath = failinimi;
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

        private void btnSaadaEmail_Click(object sender, EventArgs e)
        {
            if (pdfPath == null || !File.Exists(pdfPath))
            {
                MessageBox.Show("Genereeri enne selle arve PDF.");
                return;
            }

            string email = Microsoft.VisualBasic.Interaction.InputBox(
                "Sisesta e-mail", "Saaja", "nimi@example.com");

            if (string.IsNullOrEmpty(email)) return;

            try
            {
                using var mail = new MailMessage();
                mail.From = new MailAddress("xxxxx@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Arve";
                mail.Body = "Arve on manuses.";
                mail.Attachments.Add(new Attachment(pdfPath));

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("xxxxx@gmail.com", "xxxxxx"),
                    EnableSsl = true
                };

                smtp.Send(mail);

                MessageBox.Show("Arve saadetud!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga: " + ex.Message);
            }
        }
    }
}
