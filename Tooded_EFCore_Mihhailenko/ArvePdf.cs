using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

public class ArvePdf
{
    private readonly Tellimus tellimus;
    private readonly PdfDocument dokument;
    private XGraphics? graafika;
    private readonly XFont font = new XFont("Arial", 10);
    private readonly XFont paks = new XFont("Arial", 10, XFontStyle.Bold);
    private readonly XFont numbrid = new XFont("Arial", 9);
    private double y;

    private ArvePdf(Tellimus tellimus, PdfDocument dokument)
    {
        this.tellimus = tellimus;
        this.dokument = dokument;
    }

    public static string Loo(Tellimus tellimus)
    {
        using PdfDocument dokument = new PdfDocument();
        ArvePdf arve = new ArvePdf(tellimus, dokument);
        return arve.Salvesta();
    }

    private string Salvesta()
    {
        if (tellimus.Id < 1 || tellimus.Read.Count == 0)
        {
            throw new InvalidOperationException("Salvestatud tellimus puudub.");
        }

        Directory.CreateDirectory(Kaustad.Arved);
        // Iga loomine saab oma faili. Tellimuse number dokumendis ei muutu.
        string tee = Path.Combine(Kaustad.Arved, $"Arve_{tellimus.Id}_{Guid.NewGuid():N}.pdf");
        dokument.Info.Title = "Arve " + tellimus.Number;
        dokument.Info.Author = "Õppepood";
        try
        {
            UusLeht();
            foreach (TellimuseRida rida in tellimus.Read.OrderBy(r => r.Id))
            {
                List<string> read = JaotaRead(graafika!, rida.TooteNimi, font, 238);
                // Tavaline rida liigub tervikuna järgmisele lehele; väga pikk rida võib jätkuda.
                double reaKorgus = read.Count * 14 + 12;
                if (reaKorgus <= 500 && y + reaKorgus > 755)
                {
                    UusLeht();
                }

                int indeks = 0;
                while (indeks < read.Count)
                {
                    if (y + 30 > 755)
                    {
                        UusLeht();
                    }

                    int mahub = Math.Max(1, (int)((755 - y - 12) / 14));
                    int mitu = Math.Min(mahub, read.Count - indeks);
                    double korgus = mitu * 14 + 12;
                    graafika!.DrawRectangle(XPens.LightGray, 40, y, 515, korgus);
                    graafika.DrawLine(XPens.LightGray, 300, y, 300, y + korgus);
                    graafika.DrawLine(XPens.LightGray, 360, y, 360, y + korgus);
                    graafika.DrawLine(XPens.LightGray, 445, y, 445, y + korgus);
                    for (int i = 0; i < mitu; i++)
                    {
                        graafika.DrawString(read[indeks + i], font, XBrushes.Black, 48, y + 16 + i * 14);
                    }

                    if (indeks == 0)
                    {
                        graafika.DrawString(rida.Kogus.ToString(), numbrid, XBrushes.Black, new XRect(305, y, 50, korgus), XStringFormats.Center);
                        graafika.DrawString($"{rida.Hind:F2}", numbrid, XBrushes.Black, new XRect(365, y, 74, korgus), XStringFormats.CenterRight);
                        graafika.DrawString($"{rida.Summa:F2}", numbrid, XBrushes.Black, new XRect(450, y, 98, korgus), XStringFormats.CenterRight);
                    }

                    indeks += mitu;
                    y += korgus;
                }
            }

            if (y + 85 > 755)
            {
                UusLeht();
            }

            graafika!.DrawRectangle(XBrushes.WhiteSmoke, 40, y + 15, 515, 38);
            graafika.DrawString($"KOKKU: {tellimus.Summa:F2} €", new XFont("Arial", 12, XFontStyle.Bold), XBrushes.Black, new XRect(48, y + 15, 500, 38), XStringFormats.CenterRight);
            graafika.DrawString("Aitäh ostu eest!", font, XBrushes.Gray, 40, y + 78);
            graafika.Dispose();
            graafika = null;
            for (int i = 0; i < dokument.Pages.Count; i++)
            {
                using XGraphics jalus = XGraphics.FromPdfPage(dokument.Pages[i], XGraphicsPdfPageOptions.Append);
                jalus.DrawString($"{tellimus.Number} · {i + 1} / {dokument.Pages.Count}", font, XBrushes.Gray, new XRect(40, 790, 515, 22), XStringFormats.Center);
            }

            // CreateNew välistab ka teoreetilise failinime kokkulangemise.
            using FileStream fail = new FileStream(tee, FileMode.CreateNew, FileAccess.Write);
            dokument.Save(fail);
            return tee;
        }
        catch
        {
            try
            {
                if (File.Exists(tee))
                {
                    File.Delete(tee);
                }
            }
            catch
            {
            }

            throw;
        }
        finally
        {
            graafika?.Dispose();
        }

    }

    private void UusLeht()
    {
        graafika?.Dispose();
        PdfPage leht = dokument.AddPage();
        leht.Size = PdfSharpCore.PageSize.A4;
        graafika = XGraphics.FromPdfPage(leht);
        graafika.DrawString("ARVE", new XFont("Arial", 24, XFontStyle.Bold), XBrushes.Black, 40, 60);
        graafika.DrawString(tellimus.Number, paks, XBrushes.Black, new XRect(270, 35, 285, 24), XStringFormats.CenterRight);
        graafika.DrawString($"{tellimus.Kuupaev:dd.MM.yyyy HH:mm}", font, XBrushes.Black, new XRect(270, 62, 285, 20), XStringFormats.CenterRight);
        graafika.DrawLine(XPens.Gray, 40, 94, 555, 94);
        y = 116;
        foreach (string tekst in new[]
        {
            "Müüja: Õppepood",
            "Ostja: " + tellimus.OstjaNimi,
            "E-post: " + tellimus.OstjaEmail,
            "Staatus: " + tellimus.Staatus
        })
        {
            foreach (string r in JaotaRead(graafika, tekst, font, 510))
            {
                graafika.DrawString(r, font, XBrushes.Black, 40, y);
                y += 14;
            }

            y += 5;
        }

        y += 8;
        graafika.DrawRectangle(XBrushes.WhiteSmoke, 40, y, 515, 28);
        graafika.DrawString("Toode", paks, XBrushes.Black, 48, y + 18);
        graafika.DrawString("Kogus", paks, XBrushes.Black, 305, y + 18);
        graafika.DrawString("Hind €", paks, XBrushes.Black, 375, y + 18);
        graafika.DrawString("Summa €", paks, XBrushes.Black, 476, y + 18);
        y += 28;
    }

    private static List<string> JaotaRead(XGraphics g, string tekst, XFont font, double laius)
    {
        List<string> read = new List<string>();
        string rida = "";
        foreach (char taht in tekst.Replace('\r', ' ').Replace('\n', ' '))
        {
            if (g.MeasureString(rida + taht, font).Width > laius && rida.Length > 0)
            {
                int tyhik = rida.LastIndexOf(' ');
                if (tyhik > 0)
                {
                    read.Add(rida.Substring(0, tyhik));
                    rida = rida.Substring(tyhik + 1);
                }
                else
                {
                    read.Add(rida);
                    rida = "";
                }
            }

            rida += taht;
        }

        if (rida.Length > 0 || read.Count == 0)
        {
            read.Add(rida);
        }

        return read;
    }
}
