using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class Toode
{
    public int Id { get; set; }
    public bool Aktiivne { get; set; } = true;
    public string? Kirjeldus { get; set; }

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] ReaVersioon { get; set; } = Array.Empty<byte>();
    public string Toodenimetus { get; set; } = "";
    public int Kogus { get; set; }

    [Precision(10, 2)]
    public decimal Hind { get; set; }
    public string? Pilt { get; set; } // ? = pilt ei ole kohustuslik
    public int KategooriaId { get; set; }
    public Kategooria Kategooria { get; set; } = null!;
}
