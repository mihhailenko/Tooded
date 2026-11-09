using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public sealed record KataloogiTulemus(bool Muutus, string Teade, int Tooteid = 0);

// Välisandmed kontrollitakse enne tehingut. Ebaõnnestumine ei muuda müüdavat kataloogi.
public sealed class KataloogiTeenus
{
    private static readonly SemaphoreSlim Lukk = new(1, 1);
    private static readonly HttpClient VaikimisiKlient = new(new HttpClientHandler { AllowAutoRedirect = false })
    { Timeout = TimeSpan.FromMinutes(5) };
    private readonly Func<ToodedContext> looDb;
    private readonly HttpClient klient;

    public KataloogiTeenus(Func<ToodedContext>? looDb = null, HttpClient? klient = null)
    {
        this.looDb = looDb ?? (() => new ToodedContext());
        this.klient = klient ?? VaikimisiKlient;
    }

    public async Task SalvestaUrlAsync(string url, CancellationToken katkestus = default)
    {
        if (!string.IsNullOrWhiteSpace(url) && (!KataloogiFail.OnHttps(url) || url.Length > 2048))
            throw new InvalidDataException("Sisesta korrektne HTTPS-aadress (kuni 2048 märki).");
        await Lukk.WaitAsync(katkestus);
        try
        {
            await using var db = looDb();
            var seaded = await db.KataloogiSeaded.SingleOrDefaultAsync(katkestus);
            if (seaded == null) db.Add(seaded = new KataloogiSeaded());
            if (seaded.Url == url.Trim()) return;
            seaded.Url = url.Trim();
            // Identiteet ja versioon jäävad alles: uus URL ei tohi salaja teist poodi importida.
            seaded.ETag = null;
            seaded.LastModified = null;
            seaded.ViimaneKontroll = null;
            await db.SaveChangesAsync(katkestus);
        }
        finally { Lukk.Release(); }
    }

    public Task<KataloogiTulemus> UuendaAsync(bool sunni = false, CancellationToken katkestus = default)
        => Task.Run(() => UuendaSisuAsync(sunni, false, katkestus), katkestus);

    public Task<KataloogiTulemus> TaastaKohalikudAsync(CancellationToken katkestus = default)
        => Task.Run(() => UuendaSisuAsync(true, true, katkestus), katkestus);

    private async Task<KataloogiTulemus> UuendaSisuAsync(bool sunni, bool taasta, CancellationToken katkestus)
    {
        await Lukk.WaitAsync(katkestus);
        try
        {
            using var ajalimiit = CancellationTokenSource.CreateLinkedTokenSource(katkestus);
            ajalimiit.CancelAfter(TimeSpan.FromMinutes(10));
            katkestus = ajalimiit.Token;
            await using var db = looDb();
            var enne = await db.KataloogiSeaded.AsNoTracking().SingleOrDefaultAsync(katkestus);
            if (enne == null || string.IsNullOrWhiteSpace(enne.Url))
                return new(false, "Veebikataloogi aadress on seadistamata. Kasutusel on kohalik kataloog.");
            if (!KataloogiFail.OnHttps(enne.Url)) throw new InvalidDataException("Kataloogi aadress peab kasutama HTTPS-i.");
            if (!sunni && enne.ViimaneKontroll > DateTimeOffset.UtcNow.AddHours(-6))
                return new(false, "Kataloogi on hiljuti kontrollitud.");

            using var vastus = await LaadiAsync(enne, taasta, katkestus);
            byte[]? sisu = vastus.StatusCode == HttpStatusCode.NotModified ? null : await LoePiiratudAsync(vastus.Content, katkestus);
            string? hash = sisu == null ? null : Convert.ToHexString(SHA256.HashData(sisu));
            KataloogiFail? fail = sisu == null ? null
                : await Task.Run(() => KataloogiFail.Loe(sisu), katkestus).ConfigureAwait(false);

            await using var tehing = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, katkestus);
            var seaded = await db.KataloogiSeaded.SingleAsync(katkestus);
            if (seaded.Url != enne.Url || seaded.Versioon != enne.Versioon || seaded.SisuHash != enne.SisuHash)
                throw new InvalidOperationException("Kataloogi seaded muutusid laadimise ajal. Proovi uuesti.");
            var aeg = DateTimeOffset.UtcNow;
            if (fail == null)
            {
                if (seaded.SisuHash == null) throw new InvalidDataException("Server tagastas 304 enne esimest importi.");
                seaded.ViimaneKontroll = aeg;
                await db.SaveChangesAsync(katkestus);
                await tehing.CommitAsync(katkestus);
                return new(false, "Kataloog on ajakohane.");
            }
            if (seaded.KataloogiId != null && seaded.KataloogiId != fail.CatalogId)
                throw new InvalidDataException("Kataloogi tunnus erineb varasemast. Kasuta sama catalogId väärtust.");
            if (fail.CatalogVersion < seaded.Versioon)
                throw new InvalidDataException("Serveri kataloog on vanem kui kohalik versioon.");
            if (fail.CatalogVersion == seaded.Versioon && seaded.SisuHash != null && hash != seaded.SisuHash)
                throw new InvalidDataException("Kataloogi sisu on muutunud sama versiooninumbri all. Suurenda catalogVersion väärtust.");

            bool muutus = taasta || hash != seaded.SisuHash;
            int muudetud = 0;
            if (muutus)
            {
                var kategooriad = await db.Kategooriatabel.Where(k => k.ValineId != null).ToDictionaryAsync(k => k.ValineId!, katkestus);
                foreach (var k in fail.Categories)
                {
                    if (!kategooriad.TryGetValue(k.Id, out var kohalik))
                    {
                        kohalik = new Kategooria { ValineId = k.Id };
                        db.Add(kohalik);
                        kategooriad.Add(k.Id, kohalik);
                    }
                    kohalik.Kategooria_nimetus = k.Name.Trim();
                    kohalik.Kirjeldus = k.Description;
                    kohalik.Jarjestus = k.SortOrder;
                    kohalik.LisainfoJson = k.SourceData?.GetRawText();
                }
                // Lisa/muuda vanemad enne lapsi EF-i seoste kaudu; JSON-i järjekord pole tähtis.
                foreach (var k in fail.Categories)
                    {
                    var kohalik = kategooriad[k.Id];
                    if (k.ParentId == null) { kohalik.Ylem = null; kohalik.YlemId = null; }
                    else kohalik.Ylem = kategooriad[k.ParentId];
                }
                // Puuduvad kategooriad säilivad viidete ning kohalike toodete jaoks.
                var tooted = await db.Toodetabel.Where(t => t.ValineId != null).ToDictionaryAsync(t => t.ValineId!, katkestus);
                var saabunud = new HashSet<string>(StringComparer.Ordinal);
                foreach (var t in fail.Products)
                {
                    saabunud.Add(t.Id);
                    if (!tooted.TryGetValue(t.Id, out var kohalik))
                    {
                        kohalik = new Toode { ValineId = t.Id };
                        db.Add(kohalik);
                    }
                    if (taasta) kohalik.KohalikMuudatus = false;
                    if (kohalik.KohalikMuudatus) continue;
                    kohalik.Toodenimetus = t.Name.Trim();
                    kohalik.Kirjeldus = t.Description;
                    kohalik.Hind = t.Price;
                    kohalik.Tavahind = t.RegularPrice;
                    kohalik.Kogus = t.Stock;
                    kohalik.Uhik = t.Unit.Trim();
                    kohalik.Margis = t.Badge;
                    kohalik.PildiUrl = t.ImageUrl;
                    kohalik.LisainfoJson = t.SourceData?.GetRawText();
                    kohalik.Aktiivne = t.Active;
                    kohalik.Kategooria = kategooriad[t.CategoryId];
                    muudetud++;
                }
                foreach (var t in tooted.Values.Where(t => !saabunud.Contains(t.ValineId!)))
                    t.Aktiivne = false; // Ka kohaliku erandiga kadunud toode eemaldatakse müügist, mitte ajaloost.
            }
            seaded.KataloogiId = fail.CatalogId;
            if (muutus) seaded.SisuJson = fail.CatalogData?.GetRawText();
            seaded.Versioon = fail.CatalogVersion;
            seaded.SisuHash = hash;
            seaded.ETag = vastus.Headers.ETag?.ToString();
            seaded.LastModified = vastus.Content.Headers.LastModified;
            seaded.ViimaneKontroll = aeg;
            if (muutus) seaded.ViimaneEdu = aeg;
            await db.SaveChangesAsync(katkestus);
            await tehing.CommitAsync(katkestus);
            return new(muutus, muutus ? $"Kataloog uuendatud · versioon {fail.CatalogVersion} · {muudetud} toodet." : "Kataloog on ajakohane.", muudetud);
        }
        finally { Lukk.Release(); }
    }

    private async Task<HttpResponseMessage> LaadiAsync(KataloogiSeaded seaded, bool taasta, CancellationToken katkestus)
    {
        // Tingimuslik päring säästab nii ribalaiust kui serverit. Ümbersuunamised on keelatud.
        using var paring = new HttpRequestMessage(HttpMethod.Get, seaded.Url);
        paring.Headers.Accept.ParseAdd("application/json");
        if (!taasta && seaded.ETag != null) paring.Headers.TryAddWithoutValidation("If-None-Match", seaded.ETag);
        if (!taasta && seaded.LastModified != null) paring.Headers.IfModifiedSince = seaded.LastModified;
        var vastus = await klient.SendAsync(paring, HttpCompletionOption.ResponseHeadersRead, katkestus);
        if (vastus.StatusCode != HttpStatusCode.NotModified && !vastus.IsSuccessStatusCode)
        {
            vastus.Dispose();
            throw new HttpRequestException("Kataloogi server ei vastanud edukalt. Kohalik kataloog jääb alles.");
        }
        return vastus;
    }

    private static async Task<byte[]> LoePiiratudAsync(HttpContent sisu, CancellationToken katkestus)
    {
        const int piir = KataloogiFail.MaxBytes;
        if (sisu.Headers.ContentLength > piir) throw new InvalidDataException("Kataloog võib olla kuni 128 MB.");
        await using var allikas = await sisu.ReadAsStreamAsync(katkestus);
        using var puhver = new MemoryStream();
        var osa = new byte[81920];
        int loetud;
        while ((loetud = await allikas.ReadAsync(osa, katkestus)) > 0)
        {
            if (puhver.Length + loetud > piir) throw new InvalidDataException("Kataloog võib olla kuni 128 MB.");
            await puhver.WriteAsync(osa.AsMemory(0, loetud), katkestus);
        }
        return puhver.ToArray();
    }
}
