namespace Tooded_EFCore;

public class OstukorviRida
{
    public int ToodeId { get; set; }
    public string Nimetus { get; set; } = "";
    public decimal Hind { get; set; }
    public int Laos { get; set; }

    public void Uuenda(Toode toode)
    {
        ToodeId = toode.Id;
        Nimetus = toode.Toodenimetus;
        Hind = toode.Hind;
        Laos = toode.Kogus;
    }
    public int Kogus { get; set; }

    public decimal Summa
    {
        get
        {
            return Hind * Kogus;
        }
    }
}

public static class Ostukorv
{
    public static List<OstukorviRida> Ridad { get; } = new List<OstukorviRida>();
    public static bool OstuKontrollVajalik { get; set; }

    public static decimal Kokku
    {
        get
        {
            decimal summa = 0;
            foreach (OstukorviRida rida in Ridad)
            {
                summa += rida.Summa;
            }

            return summa;
        }
    }

    public static long KogusKokku
    {
        get
        {
            long kogus = 0;
            foreach (OstukorviRida rida in Ridad)
            {
                kogus += rida.Kogus;
            }
            return kogus;
        }
    }

    public static Guid OstuTunnus { get; private set; } = Guid.NewGuid();

    public static void Lisa(Toode toode, int kogus)
    {
        if (OstuKontrollVajalik)
        {
            throw new ArgumentException("Kontrolli enne ostukorvis eelmise ostu tulemust.");
        }

        if (!toode.Aktiivne || kogus < 1)
        {
            throw new ArgumentException("Toodet ei saa korvi lisada.");
        }

        OstukorviRida? olemas = null;
        foreach (OstukorviRida rida in Ridad)
        {
            if (rida.ToodeId == toode.Id)
            {
                olemas = rida;
                break;
            }
        }

        long kokku = kogus;
        if (olemas != null)
        {
            kokku += olemas.Kogus;
        }

        if (kokku > toode.Kogus)
        {
            throw new ArgumentException("Soovitud kogust ei ole laos. Vali väiksem kogus.");
        }

        if (olemas != null)
        {
            olemas.Uuenda(toode);
            olemas.Kogus = (int)kokku;
        }
        else
        {
            OstukorviRida rida = new OstukorviRida();
            rida.Uuenda(toode);
            rida.Kogus = kogus;
            Ridad.Add(rida);
        }
    }

    public static void Tyhjenda()
    {
        Ridad.Clear();
        OstuKontrollVajalik = false;
        OstuTunnus = Guid.NewGuid();
    }
}
