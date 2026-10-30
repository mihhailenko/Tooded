using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;
[Index(nameof(Number), IsUnique = true)]
[Index(nameof(OstuTunnus), IsUnique = true)]
public class Tellimus
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public Guid OstuTunnus { get; set; }
    public DateTime Kuupaev { get; set; }
    public int KasutajaId { get; set; }
    public Kasutaja Kasutaja { get; set; } = null!;
    public string OstjaNimi { get; set; } = "";
    public string OstjaEmail { get; set; } = "";
    public string Staatus { get; set; } = "Kinnitatud";

    [Precision(28, 2)]
    public decimal Summa { get; set; }
    public List<TellimuseRida> Read { get; set; } = new List<TellimuseRida>();
}
