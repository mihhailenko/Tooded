using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tooded_EFCore;

// Original API data remains in SQL verbatim. Display only plain text and
// validated HTTPS pictures; neither HTML nor links from the source are executed.
public sealed class TooteLisainfo
{
    private readonly JsonElement root;
    public TooteLisainfo(string? json)
    {
        try { root = JsonSerializer.Deserialize<JsonElement>(json ?? "{}"); }
        catch (JsonException) { root = JsonSerializer.Deserialize<JsonElement>("{}"); }
    }
    public static string Tekst(string? html) => Regex.Replace(WebUtility.HtmlDecode(
        Regex.Replace(html ?? "", "<[^>]*>", " ")), @"\s+", " ").Trim();
    private JsonElement? Otsi(string name) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var item) ? item : null;
    public string Loe(string name)
    {
        var item = Otsi(name);
        return item?.ValueKind == JsonValueKind.String ? Tekst(item.Value.GetString()) :
            item?.ValueKind == JsonValueKind.Number ? item.Value.ToString() : "";
    }
    public IReadOnlyList<string> Pildid(string? primary)
    {
        var result = new List<string>();
        if (KataloogiFail.OnHttps(primary)) result.Add(primary!);
        foreach (string key in new[] { "images", "imagesOriginal" })
        {
            var images = Otsi(key);
            if (images?.ValueKind != JsonValueKind.Array) continue;
            foreach (var value in images.Value.EnumerateArray())
                if (value.ValueKind == JsonValueKind.String && KataloogiFail.OnHttps(value.GetString())) result.Add(value.GetString()!);
        }
        // The source may use _s/_m/original URLs for the same photograph.
        return result.DistinctBy(url => Regex.Replace(url, @"(?:_s|_m|_l|_m2)?\.(?:png|jpg|avif)(?:\?.*)?$", "", RegexOptions.IgnoreCase))
            .Take(40).ToArray();
    }
    public IReadOnlyList<(string Name, string Value)> Koostis()
    {
        var result = new List<(string, string)>();
        foreach (var (key, label) in new[] { ("brand", "Kaubamärk"), ("countryOfOrigin", "Päritolumaa"),
            ("netAmount", "Netokogus (g/ml)"), ("manufacturer", "Tootja"), ("ingredients", "Koostisosad"),
            ("allergens", "Allergeenid"), ("foodAdditives", "Lisaained"), ("storageConditions", "Säilitamine"),
            ("instructionsForUse", "Kasutamine"), ("supplierContacts", "Tarnija kontaktandmed"),
            ("extraInfo", "Lisainfo"), ("alcoholConcentration", "Alkoholisisaldus (%)"), ("ageLimitation", "Vanusepiirang") })
        { string value = Loe(key); if (value.Length > 0) result.Add((label, value)); }
        return result;
    }
    public IReadOnlyList<(string Name, string Value)> Toitevaartus()
    {
        var result = new List<(string, string)>();
        var nutrients = Otsi("nutrients");
        if (nutrients?.ValueKind != JsonValueKind.Object) return result;
        foreach (var (key, label, unit) in new[] { ("kj", "Energiasisaldus", "kJ"), ("calories", "Energiasisaldus", "kcal"),
            ("fat", "Rasvad", "g"), ("saturatedFat", "sh küllastunud rasvhapped", "g"),
            ("carbohydrates", "Süsivesikud", "g"), ("sugar", "sh suhkrud", "g"), ("protein", "Valgud", "g"), ("salt", "Sool", "g") })
        {
            if (nutrients.Value.TryGetProperty(key, out var item) && item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("value", out var number) && number.ValueKind == JsonValueKind.Number && number.TryGetDecimal(out var value))
                result.Add((label, value.ToString("0.####", CultureInfo.GetCultureInfo("et-EE")) + " " + unit));
        }
        return result;
    }
    public string Pakkumine()
    {
        var p = Otsi("promotion");
        if (p?.ValueKind != JsonValueKind.Object) return "";
        var parts = new List<string>();
        if (p.Value.TryGetProperty("loyaltyCardRequired", out var card) && card.ValueKind == JsonValueKind.True) parts.Add("Lähtepakkumine nõuab AITÄH sooduskaarti.");
        if (p.Value.TryGetProperty("mixAndMatch", out var multi) && multi.ValueKind == JsonValueKind.True) parts.Add("Lähtepakkumine kehtib määratud koguse või toodete kombinatsiooni korral.");
        if (parts.Count > 0) parts.Add("Õppepoe ostukorvis kasutatakse ülal näidatud tavahinda.");
        if (p.Value.TryGetProperty("validTo", out var until) && until.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(until.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            parts.Add("Lähtepakkumise lõpp: " + date.ToString("dd.MM.yyyy HH:mm zzz"));
        return string.Join("\n", parts);
    }
    public string Voordlushind()
    {
        var u = Otsi("comparativeUnit");
        if (u?.ValueKind != JsonValueKind.Object || !u.Value.TryGetProperty("price", out var p) || p.ValueKind != JsonValueKind.Number || !p.TryGetDecimal(out var price)
            || !u.Value.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String) return "";
        return DokumendiSeaded.Raha(price) + " € / " + Tekst(type.GetString()) + " (lähtekataloog)";
    }
}
