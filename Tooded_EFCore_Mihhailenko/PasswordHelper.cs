namespace Tooded_EFCore;

public static class PasswordHelper
{
    public static string Hash(string parool)
    {
        return "uus:" + BCrypt.Net.BCrypt.EnhancedHashPassword(parool);
    }

    public static bool Verify(string parool, string rasi)
    {
        if (rasi.StartsWith("uus:"))
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(parool, rasi.Substring(4));
        }

        // Enne uuendust loodud kontod saavad vana parooliga sisse logida.
        return BCrypt.Net.BCrypt.Verify(parool, rasi);
    }

    public static string Kontrolli(string parool, string kordus)
    {
        if (string.IsNullOrWhiteSpace(parool) || parool.Length < 8)
        {
            return "Vali vähemalt 8 märgi pikkune parool.";
        }
        if (parool.Length > 128)
        {
            return "Parool on liiga pikk. Kasuta kuni 128 märki.";
        }
        if (parool != kordus)
        {
            return "Paroolid ei ole samad. Sisesta parool uuesti.";
        }
        return "";
    }
}
