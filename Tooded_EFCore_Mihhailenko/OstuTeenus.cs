using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tooded_EFCore;

public static class OstuTeenus
{
    public static Tellimus Koosta(ToodedContext db, int kasutajaId, out bool hinnadMuutusid)
    {
        Kasutaja? ostja = db.Kasutajad.Find(kasutajaId);
        if (ostja == null)
        {
            throw new ArgumentException("Logi uuesti sisse.");
        }
        if (Ostukorv.Ridad.Count == 0)
        {
            throw new ArgumentException("Ostukorv on tühi.");
        }

        Tellimus tellimus = new Tellimus();
        tellimus.Number = "U-" + Ostukorv.OstuTunnus.ToString("N");
        tellimus.OstuTunnus = Ostukorv.OstuTunnus;
        tellimus.Kuupaev = DateTime.Now;
        tellimus.KasutajaId = ostja.Id;
        tellimus.OstjaNimi = ostja.Nimi;
        tellimus.OstjaEmail = ostja.Email;
        hinnadMuutusid = false;

        foreach (OstukorviRida korviRida in Ostukorv.Ridad.OrderBy(r => r.ToodeId))
        {
            Toode? toode = db.Toodetabel.Find(korviRida.ToodeId);
            if (toode == null || !toode.Aktiivne || korviRida.Kogus < 1 || korviRida.Kogus > toode.Kogus)
            {
                throw new ArgumentException("Toodet „" + korviRida.Nimetus + "” ei ole soovitud koguses müügil.");
            }
            if (toode.Hind != korviRida.Hind)
            {
                hinnadMuutusid = true;
            }
            korviRida.Uuenda(toode);

            TellimuseRida rida = new TellimuseRida();
            rida.Toode = toode;
            rida.ToodeId = toode.Id;
            rida.TooteNimi = toode.Toodenimetus;
            rida.Kogus = korviRida.Kogus;
            rida.Hind = toode.Hind;
            rida.Summa = rida.Hind * rida.Kogus;
            tellimus.Read.Add(rida);
            tellimus.Summa += rida.Summa;
        }
        return tellimus;
    }

    public static void Salvesta(ToodedContext db, Tellimus tellimus)
    {
        using IDbContextTransaction tehing = db.Database.BeginTransaction();
        foreach (TellimuseRida rida in tellimus.Read)
        {
            rida.Toode.Kogus -= rida.Kogus;
        }
        db.Tellimused.Add(tellimus);
        db.SaveChanges();
        tellimus.Number = "T-" + tellimus.Id.ToString("D8");
        db.SaveChanges();

        // Kui ühendus katkeb Commit ajal, otsime sama tunnusega ostu uuesti üles.
        Ostukorv.OstuKontrollVajalik = true;
        tehing.Commit();
    }
}
