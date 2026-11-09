using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tooded_EFCore;

public sealed class KataloogiFail
{
    public const int MaxBytes = 128 * 1024 * 1024;
    public required int SchemaVersion { get; set; }
    public required string CatalogId { get; set; }
    public required long CatalogVersion { get; set; }
    public bool AllowEmptyCatalog { get; set; }
    public required List<KataloogiKategooria> Categories { get; set; }
    public required List<KataloogiToode> Products { get; set; }
    public JsonElement? CatalogData { get; set; }

    public static KataloogiFail Loe(byte[] sisu)
    {
        if (sisu.Length > MaxBytes) throw new InvalidDataException("Kataloog võib olla kuni 128 MB.");
        var fail = JsonSerializer.Deserialize<KataloogiFail>(sisu, new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, MaxDepth = 32 })
            ?? throw new InvalidDataException("Kataloogi sisu puudub.");
        fail.Kontrolli();
        return fail;
    }

    public static bool OnHttps(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment);
    private static bool OnId(string? id) => id != null && Regex.IsMatch(id, "^[a-z0-9][a-z0-9._-]{0,99}$", RegexOptions.CultureInvariant);
    private static bool Tekst(string? text, int max) => !string.IsNullOrWhiteSpace(text) && text.Length <= max;
    private static bool Hind(decimal hind) => hind >= 0 && hind <= 99999999.99m && decimal.Round(hind, 2) == hind;
    public void Kontrolli()
    {
        if (SchemaVersion is not (1 or 2) || !OnId(CatalogId) || CatalogVersion < 1)
            throw new InvalidDataException("Tundmatu kataloogivorming või vigane kataloogi versioon / tunnus.");
        if (Categories == null || Products == null || Categories.Count > 2000 || Products.Count > 100000
            || (Products.Count == 0 && !AllowEmptyCatalog))
            throw new InvalidDataException("Kataloogi loendid puuduvad, on liiga suured või on tühi kataloog kinnitamata.");
        var kats = new Dictionary<string, KataloogiKategooria>(StringComparer.Ordinal);
        foreach (var k in Categories)
            if (k == null || !OnId(k.Id) || !Tekst(k.Name, 150) || (k.Description?.Length ?? 0) > 2000 || !kats.TryAdd(k.Id, k))
                throw new InvalidDataException("Kategooria andmed või korduv tunnus ei sobi.");
        KontrolliJson(CatalogData, 32 * 1024 * 1024);
        foreach (var k in Categories) KontrolliJson(k.SourceData, 128 * 1024);
        foreach (var k in Categories)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal) { k.Id };
            var parent = k.ParentId;
            while (parent != null)
            {
                if (!kats.TryGetValue(parent, out var p) || !seen.Add(parent) || seen.Count > 8)
                    throw new InvalidDataException("Kategooriapuu sisaldab puuduvat ülemkategooriat, tsüklit või üle 8 taseme.");
                parent = p.ParentId;
            }
        }
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in Products)
            if (t == null || !OnId(t.Id) || !ids.Add(t.Id) || !Tekst(t.Name, 200) || !kats.ContainsKey(t.CategoryId ?? "")
                || !Hind(t.Price) || t.Stock < 0 || !Tekst(t.Unit, 40) || (t.Description?.Length ?? 0) > (SchemaVersion == 2 ? 100000 : 4000)
                || (t.Badge?.Length ?? 0) > 40 || (t.ImageUrl != null && (t.ImageUrl.Length > 2048 || !OnHttps(t.ImageUrl)))
                || (t.RegularPrice.HasValue && (!Hind(t.RegularPrice.Value) || t.RegularPrice <= t.Price)))
                throw new InvalidDataException("Toote andmed, hind, pildi HTTPS-aadress või korduv tunnus ei sobi.");
        foreach (var t in Products) KontrolliJson(t.SourceData, 512 * 1024);
    }

    private static void KontrolliJson(JsonElement? value, int max)
    {
        if (value.HasValue && value.Value.ValueKind != JsonValueKind.Null
            && (value.Value.ValueKind != JsonValueKind.Object || System.Text.Encoding.UTF8.GetByteCount(value.Value.GetRawText()) > max))
            throw new InvalidDataException("Kataloogi lisaandmed peavad olema piiratud suurusega JSON-objekt.");
    }
}

public sealed class KataloogiKategooria
{
    public JsonElement? SourceData { get; set; }
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? ParentId { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public sealed class KataloogiToode
{
    public JsonElement? SourceData { get; set; }
    public required string Id { get; set; }
    public required string CategoryId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public decimal? RegularPrice { get; set; }
    public required int Stock { get; set; }
    public required string Unit { get; set; }
    public string? ImageUrl { get; set; }
    public string? Badge { get; set; }
    public required bool Active { get; set; }
}
