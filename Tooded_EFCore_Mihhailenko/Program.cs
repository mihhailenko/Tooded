using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Kaustad.Loo();

        using (var db = new ToodedContext())
        {
            db.Database.Migrate();
            LooAlgkategooriad(db);
            LooAlgkasutajad(db);
        }

        Application.Run(new LoginForm());
    }

    private static void LooAlgkategooriad(ToodedContext db)
    {
        if (db.Kategooriatabel.Any()) return;
        db.Kategooriatabel.AddRange(
            new Kategooria { Kategooria_nimetus = "Joogid" },
            new Kategooria { Kategooria_nimetus = "Puu- ja köögiviljad" },
            new Kategooria { Kategooria_nimetus = "Maiustused" });
        db.SaveChanges();
    }

    private static void LooAlgkasutajad(ToodedContext db)
    {
        if (db.Kasutajad.Any()) return;

        db.Kasutajad.AddRange(
            new Kasutaja { Nimi = "Admin", Email = "admin@admin.ee",
                           ParoolHash = PasswordHelper.Hash("admin123"), Roll = "admin" },
            new Kasutaja { Nimi = "Kasutaja", Email = "kasutaja@pood.ee",
                           ParoolHash = PasswordHelper.Hash("kasutaja123"), Roll = "kasutaja" });
        db.SaveChanges();
    }
}
