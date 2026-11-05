using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace Tooded_EFCore;

public class PostiSeaded
{
    // Parooli saab avada ainult sama Windowsi kasutaja.
    [Key, MaxLength(184)]
    public string WindowsiKasutaja { get; set; } = "";

    public string Server { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Saatja { get; set; } = "";
    public string Kasutajanimi { get; set; } = "";
    public string KaitstudParool { get; set; } = "";

    public static PostiSeaded Loe()
    {
        using ToodedContext db = new ToodedContext();
        string kasutaja = WindowsIdentity.GetCurrent().User!.Value;
        PostiSeaded? seaded = db.PostiSeaded.Find(kasutaja);
        if (seaded != null)
        {
            return seaded;
        }

        // Vana fail viiakse baasi ainult siis, kui seal veel seadeid pole.
        string vanaFail = Path.Combine(Kaustad.Juur, "post.json");
        if (!File.Exists(vanaFail))
        {
            return new PostiSeaded();
        }

        seaded = JsonSerializer.Deserialize<PostiSeaded>(File.ReadAllText(vanaFail));
        if (seaded == null)
        {
            throw new InvalidDataException("E-posti seadete fail on tühi.");
        }

        seaded.Salvesta(null);
        File.Delete(vanaFail);
        return seaded;
    }

    public void Salvesta(string? uusParool)
    {
        using ToodedContext db = new ToodedContext();
        WindowsiKasutaja = WindowsIdentity.GetCurrent().User!.Value;
        PostiSeaded? seaded = db.PostiSeaded.Find(WindowsiKasutaja);
        if (seaded == null)
        {
            seaded = new PostiSeaded
            {
                WindowsiKasutaja = WindowsiKasutaja,
                KaitstudParool = KaitstudParool
            };
            db.PostiSeaded.Add(seaded);
        }

        seaded.Server = Server;
        seaded.Port = Port;
        seaded.Saatja = Saatja;
        seaded.Kasutajanimi = Kasutajanimi;
        if (uusParool != null)
        {
            if (uusParool.Length == 0)
            {
                seaded.KaitstudParool = "";
            }
            else
            {
                byte[] parool = Encoding.UTF8.GetBytes(uusParool);
                byte[] kaitstud = ProtectedData.Protect(parool, null, DataProtectionScope.CurrentUser);
                seaded.KaitstudParool = Convert.ToBase64String(kaitstud);
            }
        }

        db.SaveChanges();
        KaitstudParool = seaded.KaitstudParool;
    }

    public string LoeParool()
    {
        if (string.IsNullOrEmpty(KaitstudParool))
        {
            return "";
        }

        byte[] kaitstud = Convert.FromBase64String(KaitstudParool);
        byte[] parool = ProtectedData.Unprotect(kaitstud, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(parool);
    }
}
