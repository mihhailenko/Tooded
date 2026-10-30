using System.Text.RegularExpressions;

namespace Tooded_EFCore;

public static class Vead
{
    public static void Naita(string tegevus, Exception viga)
    {
        string detailid = tegevus;
        Exception? praegune = viga;
        while (praegune != null)
        {
            detailid += Environment.NewLine + praegune.Message;
            praegune = praegune.InnerException;
        }

        // Ühenduse paroolid ei lähe teatesse ega logisse.
        detailid = Regex.Replace(detailid, "(?i)\\b(Password|Pwd|AccessToken)\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^;\\r\\n]*)", "$1=[peidetud]");
        string tee = Path.Combine(Kaustad.Juur, "vead.log");
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.AppendAllText(tee, DateTime.Now + Environment.NewLine + detailid + Environment.NewLine);
        }
        catch
        {
        // Teade kuvatakse ka siis, kui logi ei saa salvestada.
        }

        MessageBox.Show(tegevus, "Toiming ei õnnestunud", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
