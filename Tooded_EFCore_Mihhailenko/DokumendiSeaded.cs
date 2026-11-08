using System.Globalization;
using System.Net.Mail;
using System.Text.Json;

namespace Tooded_EFCore;

// Shop details are local configuration, never copied from a customer's email.
public sealed class DokumendiSeaded
{
    public string Muujanimi { get; set; } = "Õppepood";
    public string Aadress { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string Email { get; set; } = "";
    public string Registrikood { get; set; } = "";
    public string Kmkr { get; set; } = "";
    public string Pangakonto { get; set; } = "";
    public string Pank { get; set; } = "";
    public string Swift { get; set; } = "";
    public string Veebileht { get; set; } = "";
    public string Lahtiolekuajad { get; set; } = "";
    // Null means no tax accounting data, not a fictitious 0% exemption.
    public decimal? Kaibemaks { get; set; }

    public static string Fail => Path.Combine(Kaustad.Juur, "dokumendid.json");
    public static DokumendiSeaded Loe()
    {
        if (!File.Exists(Fail)) return new();
        var value = JsonSerializer.Deserialize<DokumendiSeaded>(File.ReadAllText(Fail))
            ?? throw new InvalidDataException("Dokumendi seaded puuduvad.");
        value.Kontrolli();
        return value;
    }
    public void Kontrolli()
    {
        foreach (var value in new[] { Muujanimi, Aadress, Telefon, Email, Registrikood, Kmkr, Pangakonto, Pank, Swift, Veebileht, Lahtiolekuajad })
            if (value == null || value.Length > 300 || value.Any(c => char.IsControl(c)))
                throw new InvalidDataException("Rekvisiidid võivad olla kuni 300 märki, ilma reavahetusteta.");
        if (string.IsNullOrWhiteSpace(Muujanimi)
            || Kaibemaks is < 0 or > 100 || (Kaibemaks.HasValue && decimal.Round(Kaibemaks.Value, 2) != Kaibemaks.Value))
            throw new InvalidDataException("Kontrolli müüja nime ja käibemaksumäära.");
        if (Email.Length > 0 && (!MailAddress.TryCreate(Email, out var email) || email.Address != Email))
            throw new InvalidDataException("Sisesta korrektne kontaktmeil.");
        if (Veebileht.Length > 0 && !KataloogiFail.OnHttps(Veebileht))
            throw new InvalidDataException("Veebiaadress peab kasutama HTTPS-i.");
    }
    public void Salvesta()
    {
        Kontrolli();
        Directory.CreateDirectory(Kaustad.Juur);
        string tmp = Fail + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(tmp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(tmp, Fail, true);
        }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
    public static string Raha(decimal value, int decimals = 2) => value.ToString("F" + decimals, CultureInfo.GetCultureInfo("et-EE"));
}
