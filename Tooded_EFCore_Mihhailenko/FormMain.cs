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
        Vaade.Vorm(this, "Õppepood – peamenüü", 900, 760);
        MinimumSize = new Size(780, 560);
        StartPosition = FormStartPosition.CenterScreen;
        TableLayoutPanel raam = Vaade.Raam(this, "Õppepood", "Tooted, ostud ja konto ühes kohas.");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 0
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        raam.Controls.Add(Vaade.Kerimine(sisu), 0, 1);
        TableLayoutPanel pood = Vaade.Veerg();
        Vaade.Lisa(pood, Vaade.Tekstirida("Ostlemine", true));
        korv.AutoSize = true;
        korv.Dock = DockStyle.Top;
        korv.Margin = new Padding(0, 0, 0, 16);
        Vaade.Lisa(pood, korv);
        LisaNupp(pood, "Ava pood", (s, e) => Ava(new Form3()), true);
        LisaNupp(pood, "Ostukorv ja ostu kinnitamine", (s, e) => Ava(new ArveForm()));
        LisaNupp(pood, AktiivneKasutaja.OnAdmin ? "Kõik tellimused ja müük" : "Minu tellimused", (s, e) => Ava(new TellimusteForm()));
        sisu.Controls.Add(Vaade.Kaart(pood), 0, 0);
        TableLayoutPanel konto = Vaade.Veerg();
        Vaade.Lisa(konto, Vaade.Tekstirida("Minu konto", true));
        kasutaja.AutoSize = true;
        kasutaja.Dock = DockStyle.Top;
        kasutaja.UseMnemonic = false;
        kasutaja.Margin = new Padding(0, 0, 0, 16);
        kasutaja.Text = $"{AktiivneKasutaja.Nimi}\n{AktiivneKasutaja.Email}\nRoll: {(AktiivneKasutaja.OnAdmin ? "administraator" : "kasutaja")}";
        Vaade.Lisa(konto, kasutaja);
        LisaNupp(konto, "Muuda parooli", (s, e) => Ava(new MuudaParooliForm()));
        LisaNupp(konto, "Logi välja", LogiValja);
        sisu.Controls.Add(Vaade.Kaart(konto), 1, 0);
        if (AktiivneKasutaja.OnAdmin)
        {
            TableLayoutPanel haldus = Vaade.Veerg();
            Vaade.Lisa(haldus, Vaade.Tekstirida("Haldamine", true));
            Vaade.Lisa(haldus, Vaade.Tekstirida("Tootevalik ja kasutajate ligipääs."));
            LisaNupp(haldus, "Toodete haldus", (s, e) => Ava(new Form2()));
            LisaNupp(haldus, "Kategooriate haldus", (s, e) => Ava(new KategooriateHaldusForm()));
            LisaNupp(haldus, "Kasutajate haldus", (s, e) => Ava(new KasutajateHaldusForm()));
            sisu.Controls.Add(Vaade.Kaart(haldus), 0, 1);
        }

        if (AktiivneKasutaja.OnAdmin)
        {
            TableLayoutPanel seaded = Vaade.Veerg();
            Vaade.Lisa(seaded, Vaade.Tekstirida("Poe seaded", true));
            LisaNupp(seaded, "E-posti seaded", (s, e) => Ava(new PostiSeadedForm()));
            LisaNupp(seaded, "Kontrolli ühendust", KontrolliUhendust);
            LisaNupp(seaded, "Salvesta varukoopia", Varunda);
            sisu.Controls.Add(Vaade.Kaart(seaded), 1, 1);
        }
        UuendaKorvi();
        ResumeLayout(true);
    }
}
