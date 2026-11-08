using System.Text.Json;

namespace Tooded_EFCore;

// Seadmes salvestatud lemmikud on eraldi iga poe kasutaja jaoks.
internal static class Lemmikud
{
    private static int kasutajaId = -1;
    private static HashSet<int> ids = [];
    public static event EventHandler? Muutus;
    private static string Fail => Path.Combine(Kaustad.Juur, $"lemmikud-{AktiivneKasutaja.Id}.json");
    private static void Loe()
    {
        if (kasutajaId == AktiivneKasutaja.Id) return;
        kasutajaId = AktiivneKasutaja.Id;
        ids = [];
        if (kasutajaId <= 0) return;
        try
        {
            if (File.Exists(Fail) && new FileInfo(Fail).Length <= 256 * 1024)
                ids = (JsonSerializer.Deserialize<HashSet<int>>(File.ReadAllText(Fail)) ?? [])
                    .Where(id => id > 0).Take(10000).ToHashSet();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
    }
    public static bool Sisaldab(int id) { Loe(); return ids.Contains(id); }
    public static int[] Koik() { Loe(); return ids.ToArray(); }
    public static void Muuda(int id)
    {
        Loe();
        if (kasutajaId <= 0 || id <= 0) return;
        var uus = new HashSet<int>(ids);
        if (!uus.Remove(id)) uus.Add(id);
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            string ajutine = Fail + ".tmp";
            File.WriteAllText(ajutine, JsonSerializer.Serialize(uus));
            File.Move(ajutine, Fail, true);
            ids = uus;
            Muutus?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { Vaade.Teade("Lemmikuid ei saanud salvestada. Proovi uuesti."); }
    }
}
