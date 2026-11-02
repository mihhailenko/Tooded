using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tooded_EFCore;

public class PostiSeaded
{
    public string Server { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Saatja { get; set; } = "";
    public string Kasutajanimi { get; set; } = "";
    public string KaitstudParool { get; set; } = "";

    private static string Tee
    {
        get
        {
            return Path.Combine(Kaustad.Juur, "post.json");
        }
    }

    public static PostiSeaded Loe()
    {
        if (!File.Exists(Tee))
        {
            return new PostiSeaded();
        }

        return JsonSerializer.Deserialize<PostiSeaded>(File.ReadAllText(Tee)) ?? new PostiSeaded();
    }

    public void Salvesta(string? uusParool)
    {
        if (uusParool != null)
        {
            byte[] parool = Encoding.UTF8.GetBytes(uusParool);
            byte[] kaitstud = ProtectedData.Protect(parool, null, DataProtectionScope.CurrentUser);
            KaitstudParool = Convert.ToBase64String(kaitstud);
        }

        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Tee, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
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
