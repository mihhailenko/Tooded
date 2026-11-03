using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Tooded_EFCore;

public partial class FormMain : Form
{
    public bool LogisValja { get; private set; }

    private readonly Label kasutaja = new Label();
    private readonly Label korv = new Label();
    public FormMain()
    {
        InitializeComponent();
        LooLiides();
    }

    private void LisaNupp(TableLayoutPanel paneel, string tekst, EventHandler tegevus, bool pohiline = false)
    {
        Button nupp = new Button
        {
            Text = tekst,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(nupp, pohiline);
        nupp.Click += tegevus;
        Vaade.Lisa(paneel, nupp);
    }

    private void UuendaKorvi()
    {
        korv.Text = $"Korvis: {Ostukorv.Ridad.Count} toodet, {Ostukorv.KogusKokku} tk · {Ostukorv.Kokku:F2} €";
    }

    private void Ava(Form vorm)
    {
        using (vorm)
            vorm.ShowDialog(this);
        UuendaKorvi();
    }

    private void Varunda(object? sender, EventArgs e)
    {
        if (!AktiivneKasutaja.OnAdmin)
        {
            return;
        }

        using SaveFileDialog aken = new SaveFileDialog
        {
            Filter = "Andmebaasi varukoopia|*.bak",
            FileName = $"Poe_varukoopia_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
            OverwritePrompt = true
        };
        if (aken.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        // Ühte faili ei asendata: nii jääb eelmine kontrollitud varukoopia alles.
        if (File.Exists(aken.FileName))
        {
            MessageBox.Show("Vali uus failinimi. Olemasolevat varukoopiat ei asendata.");
            return;
        }

        try
        {
            UseWaitCursor = true;
            using ToodedContext db = new ToodedContext();
            db.Database.OpenConnection();
            using System.Data.Common.DbCommand kask = db.Database.GetDbConnection().CreateCommand();
            kask.CommandTimeout = 120;
            kask.CommandText = "BACKUP DATABASE [Tooded_DB] TO DISK = @tee WITH COPY_ONLY, CHECKSUM";
            kask.Parameters.Add(new SqlParameter("@tee", aken.FileName));
            kask.ExecuteNonQuery();
            kask.CommandText = "RESTORE VERIFYONLY FROM DISK = @tee WITH CHECKSUM";
            kask.ExecuteNonQuery();
            MessageBox.Show("Varukoopia on salvestatud.\n" + "Tootepildid asuvad eraldi kaustas: " + Kaustad.Pildid + "\nVajadusel varunda ka see kaust.");
        }
        catch (Exception ex)
        {
            Vead.Naita("Varukoopiat ei saanud salvestada. Vali teine kaust ja proovi uuesti.", ex);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void LogiValja(object? sender, EventArgs e)
    {
        Ostukorv.Tyhjenda();
        AktiivneKasutaja.Id = 0;
        AktiivneKasutaja.Nimi = "";
        AktiivneKasutaja.Email = "";
        AktiivneKasutaja.Roll = "";
        LogisValja = true;
        Close();
    }

    private void KontrolliUhendust(object? sender, EventArgs e)
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            if (db.Database.CanConnect())
            {
                MessageBox.Show("Ühendus toimib.");
            }
            else
            {
                MessageBox.Show("Ühendust ei saanud luua. Proovi hiljem uuesti.");
            }
        }
        catch (Exception ex)
        {
            Vead.Naita("Ühendust ei saanud kontrollida. Proovi hiljem uuesti.", ex);
        }
    }

    private void LooLiides()
    {
        Text = "Õppepood";
        ClientSize = new Size(470, 700);
        TableLayoutPanel sisu = Vaade.Veerg();
        sisu.Padding = new Padding(20);
        Controls.Add(Vaade.Kerimine(sisu));
        kasutaja.Text = AktiivneKasutaja.Nimi + "\n" + AktiivneKasutaja.Email;
        kasutaja.AutoSize = true;
        korv.AutoSize = true;
        Vaade.Lisa(sisu, kasutaja);
        Vaade.Lisa(sisu, korv);
        LisaNupp(sisu, "Pood", (s,e) => Ava(new Form3()));
        LisaNupp(sisu, "Ostukorv", (s,e) => Ava(new ArveForm()));
        LisaNupp(sisu, "Tellimused", (s,e) => Ava(new TellimusteForm()));
        if (AktiivneKasutaja.OnAdmin)
        {
            LisaNupp(sisu, "Tooted", (s,e) => Ava(new Form2()));
            LisaNupp(sisu, "Kategooriad", (s,e) => Ava(new KategooriateHaldusForm()));
            LisaNupp(sisu, "Kasutajad", (s,e) => Ava(new KasutajateHaldusForm()));
        }
        LisaNupp(sisu, "Muuda parooli", (s,e) => Ava(new MuudaParooliForm()));
        LisaNupp(sisu, "Logi välja", LogiValja);
        UuendaKorvi();
    }
}
