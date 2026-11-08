using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace Tooded_EFCore;

public enum TellimuseKirjaLiik { Arve, Kinnitus, Saateleht }

public static class TellimuseKiri
{
    public static string VaradeKaust => Path.Combine(AppContext.BaseDirectory, "Assets", "Barbora", "mail");
    private static string E(string? value) => WebUtility.HtmlEncode(value ?? "");

    public static MailMessage Loo(Tellimus tellimus, string saatja, string saaja, string? pdf,
        DokumendiSeaded? seaded = null, TellimuseKirjaLiik liik = TellimuseKirjaLiik.Arve)
    {
        Kontrolli(tellimus);
        seaded ??= DokumendiSeaded.Loe(); seaded.Kontrolli();
        var message = new MailMessage
        {
            From = new MailAddress(saatja, seaded.Muujanimi, Encoding.UTF8),
            Subject = liik switch
            {
                TellimuseKirjaLiik.Kinnitus => "Tellimus on kinnitatud",
                TellimuseKirjaLiik.Saateleht => "Tellimuse nr " + tellimus.Number + " saateleht",
                _ => "Saadame ostuarve nr " + tellimus.Number
            },
            SubjectEncoding = Encoding.UTF8, BodyEncoding = Encoding.UTF8, HeadersEncoding = Encoding.UTF8,
            Body = "Tere!\r\n\r\n" + Sissejuhatus(tellimus, liik)
                + $"\r\n{tellimus.Number}\r\n{DokumendiSeaded.Raha(tellimus.Summa)} €\r\n\r\n{seaded.Muujanimi}",
            IsBodyHtml = false
        };
        try
        {
            message.To.Add(new MailAddress(saaja));
            string contact = string.IsNullOrEmpty(seaded.Email) ? message.From.Address : seaded.Email;
            message.ReplyToList.Add(new MailAddress(contact));
            string content = LooHtml(tellimus, contact, seaded, liik);
            var html = AlternateView.CreateAlternateViewFromString(content, Encoding.UTF8, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(html);
            // Original graphics are MIME inline resources; no image requests or tracking pixels.
            foreach (string path in Directory.GetFiles(VaradeKaust, "*.png"))
            {
                string contentId = "barbora-" + Path.GetFileNameWithoutExtension(path);
                if (!content.Contains("cid:" + contentId, StringComparison.Ordinal)) continue;
                var resource = new LinkedResource(path, "image/png")
                { ContentId = contentId, TransferEncoding = TransferEncoding.Base64 };
                html.LinkedResources.Add(resource);
            }
            if (liik != TellimuseKirjaLiik.Kinnitus)
            {
                if (pdf == null || !File.Exists(pdf)) throw new FileNotFoundException("PDF puudub.", pdf);
                string name = Regex.Replace(tellimus.Number, "[^a-zA-Z0-9._-]", "_");
                message.Attachments.Add(new Attachment(pdf, MediaTypeNames.Application.Pdf)
                { Name = name[..Math.Min(80, name.Length)] + ".pdf", NameEncoding = Encoding.UTF8 });
            }
            return message;
        }
        catch { message.Dispose(); throw; }
    }

    public static string LooHtml(Tellimus tellimus, string kontaktEmail, DokumendiSeaded? seaded = null,
        TellimuseKirjaLiik liik = TellimuseKirjaLiik.Arve)
    {
        Kontrolli(tellimus); seaded ??= DokumendiSeaded.Loe(); seaded.Kontrolli();
        kontaktEmail = new MailAddress(kontaktEmail).Address;
        string emailUrl = "mailto:" + kontaktEmail;
        var values = new Dictionary<string, string>
        {
            ["ORDER_NUMBER"] = E(tellimus.Number), ["GREETING"] = "Tere!",
            ["INTRODUCTION"] = E(Sissejuhatus(tellimus, liik)),
            ["HEADER_SRC"] = "cid:barbora-eb1e8ea9-e76b-41f3-bca2-afff7ad3138e",
            ["FOOTER_SRC"] = "cid:barbora-befb9417-edfd-489c-9da8-420677dc4a2f",
            ["HEADER_ALT"] = "Barbora. Täname sind tellimuse eest!",
            ["FOOTER_ALT"] = "Kohtumiseni! Barbora",
            ["PHONE_ALT"] = "Telefon", ["EMAIL_ALT"] = "E-post",
            ["RATING_QUESTION"] = "Sinu arvamus on meile väga oluline. Kuidas hindad oma ostukogemust?",
            ["RATING_HINT"] = "1 – väga halb, 5 – väga hea",
            ["SUPPORT_TITLE"] = "Kas sul on küsimusi või tagasisidet?",
            ["PHONE"] = E(string.IsNullOrEmpty(seaded.Telefon) ? "—" : seaded.Telefon),
            ["PHONE_URL"] = E(string.IsNullOrEmpty(seaded.Telefon) ? emailUrl : "tel:" + seaded.Telefon),
            ["EMAIL"] = E(kontaktEmail), ["EMAIL_URL"] = E(emailUrl), ["HOURS"] = E(seaded.Lahtiolekuajad),
            ["SHOP_URL"] = E(seaded.Veebileht.Length > 0 ? seaded.Veebileht : emailUrl), ["SHOP_NAME"] = E(seaded.Muujanimi),
            ["TERMS_TEXT"] = "Ostuga seotud küsimuste korral võta meiega ühendust.",
            ["TERMS_LINK"] = "Kontaktandmed.",
            ["PRIVACY_LINK"] = "Teave isikuandmete töötlemise kohta",
            ["CONFIDENTIALITY"] = "See kiri ja selle manused on mõeldud ainult adressaadile. Kui said kirja ekslikult, teavita palun saatjat ning kustuta kiri koos manustega. Lisateabe saamiseks isikuandmete töötlemise kohta võta meiega ühendust.",
            ["ORDER_INFORMATION"] = liik == TellimuseKirjaLiik.Arve ? "" : Koond(tellimus)
        };
        for (int i = 1; i <= 5; i++)
        {
            values["RATING_" + i] = E(emailUrl + "?subject=" + Uri.EscapeDataString($"Tagasiside tellimuse {tellimus.Number} kohta: {i}/5")
                + "&body=" + Uri.EscapeDataString($"Minu tagasiside tellimuse {tellimus.Number} kohta:"));
            values["RATING_" + i + "_ALT"] = i == 1 ? "1 tärn" : i + " tärni";
        }
        string template = File.ReadAllText(Path.Combine(VaradeKaust, "invoice.html"));
        // One pass: user text containing {{...}} can never introduce new template substitutions.
        return Regex.Replace(template, @"\{\{([A-Z_0-9]+)\}\}", match => values.TryGetValue(match.Groups[1].Value, out var value)
            ? value : throw new InvalidDataException("Tundmatu kirjamalli väli: " + match.Value));
    }

    public static string Eelvaade(Tellimus tellimus, string kontaktEmail, DokumendiSeaded? seaded = null,
        TellimuseKirjaLiik liik = TellimuseKirjaLiik.Arve)
    {
        string html = LooHtml(tellimus, kontaktEmail, seaded, liik);
        // Self-contained local preview uses exactly the same bytes as CID resources in the email.
        foreach (string path in Directory.GetFiles(VaradeKaust, "*.png"))
            html = html.Replace("cid:barbora-" + Path.GetFileNameWithoutExtension(path), "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(path)));
        return html;
    }

    private static string Sissejuhatus(Tellimus t, TellimuseKirjaLiik liik) => liik switch
    {
        TellimuseKirjaLiik.Kinnitus => $"Sinu tellimus on kinnitatud. Tellimuse number: {t.Number}.",
        TellimuseKirjaLiik.Saateleht => $"Saadame sulle tellimuse nr {t.Number} toodete nimekirja.",
        _ => $"Saadame sulle tellimuse nr {t.Number} ostuarve."
    };

    private static string Koond(Tellimus t)
    {
        var html = new StringBuilder("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin-bottom:24px;font-family:Arial;font-size:16px\">");
        foreach (var (label, value) in new[] {
            ("Tellimuse number", t.Number),
            ("Tellimuse kuupäev", t.Kuupaev.ToString("dd.MM.yyyy HH:mm")),
            ("Tellimuse summa", DokumendiSeaded.Raha(t.Summa) + " €"),
            ("Ostja", t.OstjaNimi) })
            html.Append($"<tr><td style=\"padding:12px 0;border-bottom:1px solid #e5e5e5\">{E(label)}</td><td style=\"padding:12px 0;text-align:right;border-bottom:1px solid #e5e5e5\">{E(value)}</td></tr>");
        html.Append("</table>");
        foreach (var row in t.Read.OrderBy(r => r.Id))
            html.Append($"<p style=\"margin:0 0 8px;font-size:14px\">{E(row.TooteNimi)} · {row.Kogus} × {DokumendiSeaded.Raha(row.Hind)} €</p>");
        return html.Append("<div style=\"height:16px\"></div>").ToString();
    }

    internal static void Kontrolli(Tellimus t)
    {
        if (t.Id < 1 || string.IsNullOrWhiteSpace(t.Number) || t.Number.Length > 80 || t.Number.Any(char.IsControl)
            || t.Read.Count == 0 || t.Read.Any(r => r.Kogus < 1 || r.Hind < 0 || r.Summa != r.Kogus * r.Hind)
            || t.Summa != t.Read.Sum(r => r.Summa))
            throw new InvalidOperationException("Salvestatud tellimuse andmed ei sobi dokumendi loomiseks.");
    }
}
