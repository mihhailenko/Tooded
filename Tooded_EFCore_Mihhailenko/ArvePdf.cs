using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

public sealed class ArvePdf
{
    private readonly Tellimus tellimus;
    private readonly DokumendiSeaded seaded;
    private readonly bool saateleht;
    private readonly PdfDocument dokument;
    private readonly XFont font = new("Microsoft Sans Serif", 8);
    private readonly XFont bold = new("Arial", 8, XFontStyle.Bold);
    private readonly XFont heading = new("Arial", 10, XFontStyle.Bold);
    private readonly XPen piir = new(XColors.Black, .5);
    private XGraphics? g;
    private double y;
    private double[] Columns => saateleht ? [20, 44, 121, 356, 407, 458, 513, 575] : [20, 44, 278, 328, 378, 427, 476, 526, 575];

    private ArvePdf(Tellimus t, PdfDocument pdf, DokumendiSeaded settings, bool delivery)
    { tellimus = t; dokument = pdf; seaded = settings; saateleht = delivery; }

    public static string Loo(Tellimus tellimus, DokumendiSeaded? seaded = null) => Salvesta(tellimus, seaded, false);
    public static string LooSaateleht(Tellimus tellimus, DokumendiSeaded? seaded = null) => Salvesta(tellimus, seaded, true);

    private static string Salvesta(Tellimus tellimus, DokumendiSeaded? settings, bool delivery)
    {
        TellimuseKiri.Kontrolli(tellimus);
        settings ??= DokumendiSeaded.Loe(); settings.Kontrolli();
        Directory.CreateDirectory(Kaustad.Arved);
        string path = Path.Combine(Kaustad.Arved, $"{(delivery ? "Saateleht" : "Arve")}_{tellimus.Id}_{Guid.NewGuid():N}.pdf");
        using var pdf = new PdfDocument();
        var writer = new ArvePdf(tellimus, pdf, settings, delivery);
        pdf.Info.Title = (delivery ? "Saateleht " : "Ostuarve ") + tellimus.Number;
        pdf.Info.Author = settings.Muujanimi;
        try
        {
            writer.Joonista();
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            pdf.Save(stream);
            return path;
        }
        catch { if (File.Exists(path)) File.Delete(path); throw; }
        finally { writer.g?.Dispose(); }
    }

    private void Joonista()
    {
        UusLeht(true);
        int nr = 0;
        foreach (var row in tellimus.Read.OrderBy(r => r.Id))
        {
            nr++;
            List<string> lines = Jaota(row.TooteNimi, Columns[saateleht ? 3 : 2] - Columns[saateleht ? 2 : 1] - 4, font);
            int index = 0;
            while (index < lines.Count)
            {
                if (y + 16 > 775) UusLeht(false);
                int fits = Math.Max(1, (int)((775 - y - 3) / 10));
                if (index == 0 && lines.Count <= 40 && lines.Count > fits) { UusLeht(false); fits = (int)((775 - y - 3) / 10); }
                int take = Math.Min(fits, lines.Count - index);
                double height = Math.Max(12, take * 10 + 2);
                string number = index == 0 ? nr.ToString() : "";
                string qty = row.Kogus + " tk";
                string[] cells;
                if (saateleht)
                    cells = [number, "—", string.Join("\n", lines.Skip(index).Take(take)), index == 0 ? qty : "", index == 0 ? qty : "",
                        index == 0 ? "€" + DokumendiSeaded.Raha(row.Hind, 4) : "", index == 0 ? "€" + DokumendiSeaded.Raha(row.Summa) : ""];
                else
                {
                    decimal? netPrice = seaded.Kaibemaks.HasValue ? row.Hind / (1 + seaded.Kaibemaks.Value / 100) : null;
                    cells = [number, string.Join("\n", lines.Skip(index).Take(take)), index == 0 ? qty : "",
                        index == 0 ? "€" + DokumendiSeaded.Raha(row.Hind, 4) : "",
                        index == 0 ? (netPrice.HasValue ? "€" + DokumendiSeaded.Raha(netPrice.Value, 4) : "—") : "",
                        index == 0 ? (seaded.Kaibemaks.HasValue ? DokumendiSeaded.Raha(seaded.Kaibemaks.Value) : "—") : "",
                        index == 0 ? (netPrice.HasValue ? "€" + DokumendiSeaded.Raha(row.Summa / (1 + seaded.Kaibemaks!.Value / 100), 4) : "—") : "",
                        index == 0 ? "€" + DokumendiSeaded.Raha(row.Summa) : ""];
                }
                Tabelirida(cells, height);
                index += take;
            }
        }
        if (y + 170 > 775) UusLeht(false);
        y += 1;
        double left = saateleht ? 121 : 131;
        if (seaded.Kaibemaks.HasValue)
        {
            decimal net = decimal.Round(tellimus.Summa / (1 + seaded.Kaibemaks.Value / 100), 2, MidpointRounding.AwayFromZero);
            Koondrida("Summa KM-ta " + DokumendiSeaded.Raha(seaded.Kaibemaks.Value) + "%", "€" + DokumendiSeaded.Raha(net), left);
            Koondrida("Käibemaks " + DokumendiSeaded.Raha(seaded.Kaibemaks.Value) + "%", "€" + DokumendiSeaded.Raha(tellimus.Summa - net), left);
        }
        else Koondrida("Käibemaksuarvestus", "Andmed puuduvad", left);
        Koondrida("Arve summa" + (seaded.Kaibemaks.HasValue ? " KM-ga:" : ":"), "€" + DokumendiSeaded.Raha(tellimus.Summa), left, true);
        Koondrida("Tellimuse staatus", tellimus.Staatus, left);
        y += 24;
        Tekst("Aitäh ostu eest!", bold, 20, y); y += 18;
        foreach (string line in Jaota("Tellimuse " + tellimus.Number + " ostja: " + tellimus.OstjaNimi + ". E-post: " + tellimus.OstjaEmail, 550, font))
        { if (y > 770) { UusLeht(false); y += 12; } Tekst(line, font, 20, y); y += 11; }
        y += 16;
        if (y + 45 > 785) { UusLeht(false); y += 12; }
        Tekst("Tellimuse staatus: " + tellimus.Staatus, bold, 20, y); y += 20;
        foreach (string line in Jaota("Õppeprojekti dokument. Kontakt: " + (seaded.Email.Length > 0 ? seaded.Email : seaded.Muujanimi), 550, font))
        { if (y > 780) UusLeht(false); Tekst(line, font, 20, y); y += 11; }
        g!.Dispose(); g = null;
        for (int i = 0; i < dokument.Pages.Count; i++)
        {
            using var footer = XGraphics.FromPdfPage(dokument.Pages[i], XGraphicsPdfPageOptions.Append);
            footer.DrawString($"{tellimus.Number} · {i + 1} / {dokument.Pages.Count}", new XFont("Microsoft Sans Serif", 7), XBrushes.Gray,
                new XRect(20, 810, 555, 15), XStringFormats.Center);
        }
    }

    private void UusLeht(bool esimene)
    {
        g?.Dispose();
        var page = dokument.AddPage(); page.Size = PdfSharpCore.PageSize.A4;
        g = XGraphics.FromPdfPage(page);
        if (esimene)
        {
            string logo = Path.Combine(AppContext.BaseDirectory, "Assets", "Barbora", "converted", "logo", "barbora.png");
            using (var image = XImage.FromFile(logo)) g.DrawImage(image, 23, saateleht ? 58 : 32, 175, 175d * image.PixelHeight / image.PixelWidth);
            if (saateleht) g.DrawString(tellimus.Number, heading, XBrushes.Black, new XRect(302, 6, 270, 16), XStringFormats.TopRight);
            double right = saateleht ? 73 : 43;
            foreach (string line in Jaota((saateleht ? "Saatelehe number: " : "Ostuarve nr: ") + tellimus.Number, 270, heading))
            { Tekst(line, heading, 302, right); right += 12; }
            Tekst("Kuupäev " + tellimus.Kuupaev.ToString("dd.MM.yyyy"), font, 302, right + 1); right += 24;
            Tekst("Ostja:", bold, 302, right); right += 12;
            foreach (string line in Jaota(tellimus.OstjaNimi, 270, font)) { Tekst(line, font, 302, right); right += 11; }
            foreach (string line in Jaota(tellimus.OstjaEmail, 270, font)) { Tekst(line, font, 302, right); right += 11; }
            right += 8;
            foreach (string line in Jaota("Tellimuse nr. " + tellimus.Number, 270, font)) { Tekst(line, font, 302, right); right += 11; }
            foreach (string line in Jaota("Staatus: " + tellimus.Staatus, 270, font)) { Tekst(line, font, 302, right); right += 11; }
            double left = saateleht ? 120 : 94;
            Tekst("Müüja:", heading, 25, left); left += 15;
            foreach (string line in Jaota(seaded.Muujanimi, 260, bold)) { Tekst(line, bold, 25, left); left += 11; }
            string[] details = ["Juriidiline aadress: " + Puuduv(seaded.Aadress), "Tel: " + Puuduv(seaded.Telefon),
                "E-mail: " + Puuduv(seaded.Email), "Reg. Nr " + Puuduv(seaded.Registrikood)];
            if (!saateleht) details = [.. details, "KMKR nr " + Puuduv(seaded.Kmkr),
                "Pangakonto nr: " + Puuduv(seaded.Pangakonto), Puuduv(seaded.Pank), "SWIFT: " + Puuduv(seaded.Swift)];
            foreach (string value in details)
            {
                foreach (string line in Jaota(value, 260, font)) { Tekst(line, font, 25, left); left += 11; }
                left += 3;
            }
            y = Math.Max(saateleht ? 235 : 269, Math.Max(left, right) + 20);
            Tekst(saateleht ? "Tellimuse tooted:" : "Tellitud tooted:", heading, 20, y);
            y += 18;
        }
        else
        {
            y = 24;
            foreach (string line in Jaota((saateleht ? "Saateleht " : "Ostuarve ") + tellimus.Number + " (järg)", 550, heading))
            { Tekst(line, heading, 20, y); y += 12; }
            y += 12;
        }
        string[] header = saateleht
            ? ["Nr.", "EAN kood", "Kauba nimetus", "Ostetud\nkogus", "Lõplik kogus", "Ühiku hind\n(tk/kg)", "Summa"]
            : ["Nr.", "Kauba nimetus", "Kogus / tk", "Hind\nkäibemaksu\nga", "Hind KM-ta", "Käibemaks,\n%", "Lõppsumma\nilma KM-ta", "Summa KM-ga"];
        Tabelirida(header, 30, true);
    }

    private static string Puuduv(string value) => string.IsNullOrWhiteSpace(value) ? "—" : value;
    private void Tekst(string value, XFont f, double x, double baseline) => g!.DrawString(value, f, XBrushes.Black, x, baseline);

    private void Tabelirida(string[] values, double height, bool header = false)
    {
        var x = Columns;
        for (int i = 0; i < values.Length; i++)
        {
            double width = x[i + 1] - x[i];
            g!.DrawRectangle(piir, x[i], y, width, height);
            int nameColumn = saateleht ? 2 : 1;
            var f = header ? bold : font;
            var lines = values[i].Split('\n');
            if (header && lines.Length == 1 && g.MeasureString(values[i], f).Width > width - 4)
                lines = Jaota(values[i], width - 4, f).ToArray();
            double start = y + Math.Max(8.5, (height - lines.Length * 10) / 2 + 8);
            foreach (var line in lines)
            {
                var alignment = header || i == 0 || (saateleht && i is 3 or 4) || (!saateleht && i == 2)
                    ? XStringFormats.TopCenter : i == nameColumn || (saateleht && i == 1) ? XStringFormats.TopLeft : XStringFormats.TopRight;
                g.DrawString(line, f, XBrushes.Black, new XRect(x[i] + 2, start - 8, width - 4, 10), alignment);
                start += 10;
            }
        }
        y += height;
    }

    private void Koondrida(string label, string value, double left, bool strong = false)
    {
        var f = strong ? bold : font;
        var lines = Jaota(label, 310, f);
        var valueLines = Jaota(value, 120, f);
        double height = Math.Max(13, Math.Max(lines.Count, valueLines.Count) * 10 + 3);
        if (y + height > 775) UusLeht(false);
        g!.DrawRectangle(piir, left, y, 575 - left, height);
        for (int i = 0; i < lines.Count; i++) Tekst(lines[i], f, left + 2, y + 9 + i * 10);
        for (int i = 0; i < valueLines.Count; i++)
            g.DrawString(valueLines[i], f, XBrushes.Black, new XRect(575 - 122, y + 1 + i * 10, 120, 10), XStringFormats.TopRight);
        y += height;
    }

    private List<string> Jaota(string? value, double width, XFont f)
    {
        var result = new List<string>();
        string line = "";
        foreach (char ch in (value ?? "").Replace('\r', ' ').Replace('\n', ' '))
        {
            if (line.Length > 0 && g!.MeasureString(line + ch, f).Width > width)
            {
                int space = line.LastIndexOf(' ');
                if (space > 0) { result.Add(line[..space]); line = line[(space + 1)..]; }
                else { result.Add(line); line = ""; }
            }
            line += ch;
        }
        result.Add(line);
        return result;
    }
}
