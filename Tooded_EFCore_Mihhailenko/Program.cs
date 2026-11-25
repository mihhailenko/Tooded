using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Globalization;

namespace Tooded_EFCore;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        string keel = "et-EE";
        try
        {
            if (Properties.Settings.Default.UserLanguage == "en-US")
                keel = "en-US";
        }
        catch (ConfigurationErrorsException)
        {
            // Kui seadefaili ei saa lugeda, jääb vaikimisi eesti keel.
        }
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(keel);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
        ApplicationConfiguration.Initialize();
        bool valmis = false;
        while (!valmis)
        {
            try
            {
                Kaustad.Loo();
                using ToodedContext db = new ToodedContext();
                UuendaAndmebaas(db);
                LooAlgkasutajad(db);

                valmis = true;
            }
            catch (Exception ex)
            {
                Vead.Naita("Poodi ei saanud avada. Proovi uuesti. Kui viga kordub, võta ühendust poe haldajaga.", ex);
                DialogResult vastus = Vaade.Teade("Kas soovid uuesti proovida?", "Andmebaas", MessageBoxButtons.RetryCancel);
                if (vastus != DialogResult.Retry)
                {
                    return;
                }
            }
        }

        Application.Run(new LoginForm());
    }

    private static void LooAlgkasutajad(ToodedContext db)
    {
        if (db.Kasutajad.Any())
        {
            return;
        }

        db.Kasutajad.Add(new Kasutaja
        {
            Nimi = "Admin",
            Email = "admin@admin.ee",
            ParoolHash = PasswordHelper.Hash("admin123"),
            Roll = "admin"
        });
        db.Kasutajad.Add(new Kasutaja
        {
            Nimi = "Kasutaja",
            Email = "kasutaja@pood.ee",
            ParoolHash = PasswordHelper.Hash("kasutaja123"),
            Roll = "kasutaja"
        });
        db.SaveChanges();
    }

    private static void UuendaAndmebaas(ToodedContext db)
    {
        SqlConnectionStringBuilder seaded = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        string nimi = seaded.InitialCatalog;
        seaded.InitialCatalog = "master";
        using SqlConnection master = new SqlConnection(seaded.ConnectionString);
        master.Open();
        using SqlCommand kontroll = new SqlCommand("SELECT DB_ID(@nimi)", master);
        kontroll.Parameters.AddWithValue("@nimi", nimi);
        bool olemas = kontroll.ExecuteScalar() != DBNull.Value;
        try
        {
            // Olemasoleva baasi avamise viga ei tohi muutuda uue baasi loomise katseks.
            if (olemas)
            {
                db.Database.OpenConnection();
            }

            db.Database.Migrate();
        }
        catch (SqlException ex) when (!olemas && ex.Number == 1801)
        {
            // Teine aken võis sama baasi vahepeal luua.
            SqlConnection.ClearPool((SqlConnection)db.Database.GetDbConnection());
            db.Database.OpenConnection();
            db.Database.Migrate();
        }
        finally
        {
            db.Database.CloseConnection();
        }
    }
}
