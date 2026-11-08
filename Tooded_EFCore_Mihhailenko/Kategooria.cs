namespace Tooded_EFCore;

public class Kategooria
{
    public int Id { get; set; }
    public string Kategooria_nimetus { get; set; } = "";
    public string? Kirjeldus { get; set; }
    public string? ValineId { get; set; }
    public int? YlemId { get; set; }
    public Kategooria? Ylem { get; set; }
    public int Jarjestus { get; set; }
    public ICollection<Toode> Tooded { get; set; } = new List<Toode>();
}
