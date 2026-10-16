namespace Tooded_EFCore_Mihhailenko
{
    public static class Ostukorv
    {
        public static List<OstukorviRida> Read = new List<OstukorviRida>();

        public static void LisaToode(Toode toode, int kogus)
        {
            var rida = Read.FirstOrDefault(r => r.Toode.Id == toode.Id);

            if (rida == null)
            {
                Read.Add(new OstukorviRida
                {
                    Toode = toode,
                    Kogus = kogus
                });
            }
            else
            {
                rida.Kogus += kogus;
            }
        }
    }
}
