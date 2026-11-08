using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Tooded_EFCore;

internal static class PildiVahemalu
{
    private static readonly HttpClient Klient = new(new HttpClientHandler { AllowAutoRedirect = false })
    { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly SemaphoreSlim Piir = new(4);
    private static readonly ConcurrentDictionary<string, DateTimeOffset> Vead = new();
    private static readonly string Kaust = Path.Combine(Kaustad.Juur, "Pildipuhver");

    // Pildi viga ei tohi katkestada kataloogi ega peita ostunuppu.
    public static Task<Image?> LaeAsync(Toode toode, CancellationToken ct)
        => Task.Run(() => LaeSisuAsync(toode, ct), ct);
    private static async Task<Image?> LaeSisuAsync(Toode toode, CancellationToken ct)
    {
        var kohalik = Kaustad.LaePilt(toode.Pilt);
        if (kohalik != null || !KataloogiFail.OnHttps(toode.PildiUrl)) return kohalik;
        string url = toode.PildiUrl!;
        string fail = Path.Combine(Kaust, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url))) + ".img");
        await Piir.WaitAsync(ct);
        try
        {
            if (File.Exists(fail) && File.GetLastWriteTimeUtc(fail) > DateTime.UtcNow.AddDays(-7))
            {
                try { return Ava(await File.ReadAllBytesAsync(fail, ct)); }
                catch (Exception ex) when (ex is ArgumentException or InvalidDataException) { File.Delete(fail); }
            }
            if (Vead.TryGetValue(url, out var aeg) && aeg > DateTimeOffset.UtcNow.AddMinutes(-5)) return null;
            using var vastus = await Klient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            vastus.EnsureSuccessStatusCode();
            if (vastus.Content.Headers.ContentLength > 6 * 1024 * 1024) return null;
            await using var voog = await vastus.Content.ReadAsStreamAsync(ct);
            using var malu = new MemoryStream();
            byte[] puhver = new byte[16384];
            int mitu;
            while ((mitu = await voog.ReadAsync(puhver, ct)) > 0)
            {
                if (malu.Length + mitu > 6 * 1024 * 1024) return null;
                malu.Write(puhver, 0, mitu);
            }
            byte[] sisu = malu.ToArray();
            Image pilt = Ava(sisu);
            try
            {
                Directory.CreateDirectory(Kaust);
                string ajutine = fail + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try { await File.WriteAllBytesAsync(ajutine, sisu, ct); File.Move(ajutine, fail, true); }
                finally { if (File.Exists(ajutine)) File.Delete(ajutine); }
            }
            catch (OperationCanceledException) { pilt.Dispose(); throw; }
            catch (IOException) { /* Pilti saab siiski kuvada. */ }
            catch (UnauthorizedAccessException) { }
            return pilt;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch
        {
            Vead[url] = DateTimeOffset.UtcNow;
            try { if (File.Exists(fail)) return Ava(await File.ReadAllBytesAsync(fail, ct)); } catch { }
            return null;
        }
        finally { Piir.Release(); }
    }

    private static Image Ava(byte[] sisu)
    {
        using var voog = new MemoryStream(sisu);
        using var pilt = Image.FromStream(voog);
        if ((long)pilt.Width * pilt.Height > 16000000) throw new InvalidDataException("Liiga suur pilt.");
        return new Bitmap(pilt);
    }
}
