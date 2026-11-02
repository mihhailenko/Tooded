namespace Tooded_EFCore;

public static class Kaustad
{
    public static readonly string Juur = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToodedApp");
    public static readonly string Pildid = Path.Combine(Juur, "Images");
    public static readonly string Arved = Path.Combine(Juur, "Arved");
    // Loob kaustad, kui neid veel pole
    public static void Loo()
    {
        Directory.CreateDirectory(Pildid);
        Directory.CreateDirectory(Arved);
        // Varasema projekti pildid viiakse uude kausta, olemasolevaid ei asendata.
        foreach (string vanaKaust in new[]
        {
            "Images",
            Path.Combine(AppContext.BaseDirectory, "Images")
        })
        {
            if (!Directory.Exists(vanaKaust))
            {
                continue;
            }

            foreach (string fail in Directory.GetFiles(vanaKaust))
            {
                string siht = Path.Combine(Pildid, Path.GetFileName(fail));
                if (!File.Exists(siht))
                {
                    File.Copy(fail, siht);
                }
            }
        }
    }

    // Laeb pildi koopiana, et fail ei jääks lukku
    public static Image? LaePilt(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi) || Path.GetFileName(failinimi) != failinimi)
        {
            return null;
        }

        string tee = Path.Combine(Pildid, failinimi);
        if (!File.Exists(tee))
        {
            return null;
        }

        try
        {
            using Image ajutine = Image.FromFile(tee);
            return new Bitmap(ajutine);
        }
        catch
        {
            return null;
        }
    }

    public static void KustutaKasutamataPilt(string? nimi)
    {
        if (string.IsNullOrWhiteSpace(nimi) || Path.GetFileName(nimi) != nimi)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            if (!db.Toodetabel.Any(t => t.Pilt == nimi))
            {
                File.Delete(Path.Combine(Pildid, nimi));
            }
        }
        catch
        { /* Fail jäetakse alles, kui kasutust ei saa kontrollida. */
        }
    }
}
