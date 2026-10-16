namespace Tooded_EFCore_Mihhailenko
{
    public class Kasutaja
    {
        public int Id { get; set; }
        public string Nimi { get; set; } = "";
        public string Email { get; set; } = "";
        public string ParoolHash { get; set; } = "";
        public string Roll { get; set; } = "kasutaja";
    }
}
