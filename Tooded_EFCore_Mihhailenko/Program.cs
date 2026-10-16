using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore_Mihhailenko
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            try
            {
                using var db = new ToodedContext();
                db.Database.Migrate();

                if (!db.Kasutajad.Any(k => k.Email == "admin@admin.ee"))
                {
                    var kasutaja = new Kasutaja
                    {
                        Nimi = "Admin",
                        Email = "admin@admin.ee",
                        ParoolHash = PasswordHelper.Hash("admin123"),
                        Roll = "admin"
                    };
                    db.Kasutajad.Add(kasutaja);
                }

                if (!db.Kasutajad.Any(k => k.Email == "kasutaja@pood.ee"))
                {
                    var kasutaja = new Kasutaja
                    {
                        Nimi = "Kasutaja",
                        Email = "kasutaja@pood.ee",
                        ParoolHash = PasswordHelper.Hash("kasutaja123"),
                        Roll = "kasutaja"
                    };
                    db.Kasutajad.Add(kasutaja);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasi viga: " + ex.Message);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}