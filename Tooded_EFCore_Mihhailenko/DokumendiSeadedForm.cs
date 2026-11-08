using System.Globalization;

namespace Tooded_EFCore;

public sealed class DokumendiSeadedForm : Form
{
    public DokumendiSeadedForm()
    {
        if (!AktiivneKasutaja.OnAdmin) throw new UnauthorizedAccessException();
        Vaade.Vorm(this, "Kirja ja arve andmed", 670, 820);
        MinimumSize = new Size(510, 550);
        var raam = Vaade.Raam(this, Text, "Müüja rekvisiidid PDF-is ja kontaktandmed tellimuskirjas.");
        var panel = Vaade.Veerg();
        DokumendiSeaded settings;
        try { settings = DokumendiSeaded.Loe(); }
        catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException or InvalidDataException or UnauthorizedAccessException)
        {
            settings = new();
            Vaade.Lisa(panel, Vaade.Tekstirida("Varasemaid dokumendi seadeid ei saanud lugeda. Sisesta andmed uuesti ja salvesta."));
        }
        var fields = new Dictionary<string, TextBox>();
        void Field(string name, string label, string value)
        {
            var input = new TextBox { Text = value, MaxLength = 300 };
            fields[name] = input; Vaade.Vali(panel, label, input);
        }
        Field("name", "Müüja nimi", settings.Muujanimi);
        Field("address", "Juriidiline aadress", settings.Aadress);
        Field("phone", "Telefon", settings.Telefon);
        Field("email", "Kontaktmeil (tühi: saatja SMTP-aadress)", settings.Email);
        Field("hours", "Kontaktide lahtiolekuajad", settings.Lahtiolekuajad);
        Field("registration", "Registrikood", settings.Registrikood);
        Field("vat", "KMKR number", settings.Kmkr);
        Field("iban", "Pangakonto", settings.Pangakonto);
        Field("bank", "Pank", settings.Pank);
        Field("swift", "SWIFT", settings.Swift);
        Field("web", "Poe HTTPS-veebiaadress (valikuline)", settings.Veebileht);
        Field("rate", "Käibemaks % (tühi: maksuarvestus puudub)", settings.Kaibemaks?.ToString(CultureInfo.GetCultureInfo("et-EE")) ?? "");
        var save = new BarboraActionButton { Text = "Salvesta", Primary = true, Dock = DockStyle.Top };
        save.Click += (_, _) =>
        {
            try
            {
                decimal? rate = null;
                if (fields["rate"].Text.Trim().Length > 0)
                {
                    if (!decimal.TryParse(fields["rate"].Text.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
                        throw new InvalidDataException("Sisesta käibemaks protsendina või jäta väli tühjaks.");
                    rate = parsed;
                }
                settings.Muujanimi = fields["name"].Text.Trim(); settings.Aadress = fields["address"].Text.Trim();
                settings.Telefon = fields["phone"].Text.Trim(); settings.Email = fields["email"].Text.Trim();
                settings.Lahtiolekuajad = fields["hours"].Text.Trim(); settings.Registrikood = fields["registration"].Text.Trim();
                settings.Kmkr = fields["vat"].Text.Trim(); settings.Pangakonto = fields["iban"].Text.Trim();
                settings.Pank = fields["bank"].Text.Trim(); settings.Swift = fields["swift"].Text.Trim();
                settings.Veebileht = fields["web"].Text.Trim(); settings.Kaibemaks = rate;
                settings.Salvesta(); DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { Vaade.Teade(ex.Message, "Seaded"); }
        };
        Vaade.Lisa(panel, save);
        var cancel = new BarboraActionButton { Text = "Loobu", Dock = DockStyle.Top, DialogResult = DialogResult.Cancel };
        Vaade.Lisa(panel, cancel);
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(panel)), 0, 1);
        AcceptButton = save; CancelButton = cancel;
        ResumeLayout(true);
    }
}
