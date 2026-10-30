using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class TellimuseRida
{
    public int Id { get; set; }
    public int TellimusId { get; set; }
    public Tellimus Tellimus { get; set; } = null!;
    public int ToodeId { get; set; }
    public Toode Toode { get; set; } = null!;
    // Nimi ja hind jäävad ostu hetke seisuga alles.
    public string TooteNimi { get; set; } = "";
    public int Kogus { get; set; }

    [Precision(10, 2)]
    public decimal Hind { get; set; }

    [Precision(28, 2)]
    public decimal Summa { get; set; }
}
