using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;
[Index(nameof(Email), IsUnique = true)] // sama e-postiga kahte kasutajat ei saa olla
public class Kasutaja
{
    public int Id { get; set; }
    public bool PeabParooliMuutma { get; set; }
    public List<Tellimus> Tellimused { get; set; } = new List<Tellimus>();
    public string Nimi { get; set; } = "";
    public string Email { get; set; } = ""; // hoitakse väiketähtedega
    public string ParoolHash { get; set; } = "";
    public string Roll { get; set; } = "kasutaja"; // "admin" või "kasutaja"
}
