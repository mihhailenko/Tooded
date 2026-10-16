namespace Tooded_EFCore;

public class OstukorviRida
{
    public Toode Toode { get; set; } = null!;
    public int Kogus { get; set; }

    public decimal Summa => Toode.Hind * Kogus;

    public override string ToString() =>
        $"{Toode.Toodenimetus} – {Kogus} tk × {Toode.Hind:F2} € = {Summa:F2} €";
}

public static class Ostukorv
{
    public static List<OstukorviRida> Ridad { get; } = new();

    public static decimal Kokku => Ridad.Sum(r => r.Summa);

    // Kui toode on juba korvis, suurendatakse kogust
    public static void Lisa(Toode toode, int kogus)
    {
        var olemas = Ridad.FirstOrDefault(r => r.Toode.Id == toode.Id);
        if (olemas != null)
            olemas.Kogus += kogus;
        else
            Ridad.Add(new OstukorviRida { Toode = toode, Kogus = kogus });
    }

    public static void Tyhjenda() => Ridad.Clear();
}
