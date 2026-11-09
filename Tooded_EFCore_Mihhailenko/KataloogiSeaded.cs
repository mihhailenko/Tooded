using System.ComponentModel.DataAnnotations;

namespace Tooded_EFCore;

// Üks väliskataloog; URL võib muutuda, kataloogi püsiv identiteet ei muutu.
public class KataloogiSeaded
{
    public int Id { get; set; } = 1;
    public string Url { get; set; } = "";
    public string? KataloogiId { get; set; }
    public long Versioon { get; set; }
    public string? ETag { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? SisuHash { get; set; }
    public DateTimeOffset? ViimaneEdu { get; set; }
    public DateTimeOffset? ViimaneKontroll { get; set; }
    public string? SisuJson { get; set; }
    [Timestamp] public byte[] ReaVersioon { get; set; } = [];
}
