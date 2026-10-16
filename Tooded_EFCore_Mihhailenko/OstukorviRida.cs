using Tooded_EFCore;

namespace Tooded_EFCore_Mihhailenko
{
    public class OstukorviRida
    {
        public Toode Toode { get; set; }
        public int Kogus { get; set; }

        public float Summa => (float)Toode.Hind * Kogus;

        public override string ToString()
        {
            return $"{Toode.Toodenimetus} – {Kogus} tk × {Toode.Hind:F2} € = {Summa:F2} €";
        }
    }
}
