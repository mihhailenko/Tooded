using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class KategooriateHaldusForm : Form
{
    private readonly DataGridView tabel = new DataGridView();
    private readonly TextBox nimi = new TextBox();
    private readonly TextBox kirjeldus = new TextBox();
    private int? valitudId;
    private readonly Label olek = new Label();
    private readonly Button lisa = new Button();
    private readonly Button muuda = new Button();
    private readonly Button kustuta = new Button();
    public KategooriateHaldusForm()
    {
        if (!AktiivneKasutaja.OnAdmin)
        {
            throw new UnauthorizedAccessException();
        }

        LooLiides();
    }

    private void Lae()
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            tabel.DataSource = db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).Select(k => new { k.Id, k.Kategooria_nimetus, k.Kirjeldus, Tooteid = k.Tooded.Count }).ToList();
            tabel.Columns["Id"].Visible = false;
            Vaade.VeeruNimi(tabel, "Kategooria_nimetus", "Kategooria", 150);
            Vaade.VeeruNimi(tabel, "Kirjeldus", "Kirjeldus", 180);
            Vaade.VeeruNimi(tabel, "Tooteid", "Tooteid", 65);
            tabel.ClearSelection();
            tabel.CurrentCell = null;
            valitudId = null;
            nimi.Clear();
            kirjeldus.Clear();
            olek.Text = "Uus kategooria";
            lisa.Visible = true;
            muuda.Visible = false;
            kustuta.Enabled = false;
        }
        catch (Exception ex)
        {
            Vead.Naita("Kategooriate laadimine ei õnnestunud.", ex);
        }
    }

    private void Salvesta(bool muutmine)
    {
        if (!AktiivneKasutaja.OnAdmin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nimi.Text) || (muutmine && valitudId == null))
        {
            Vaade.Teade("Sisesta nimetus ning muutmiseks vali kategooria tabelist.");
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            if (db.Kategooriatabel.Any(k => k.Kategooria_nimetus == nimi.Text.Trim() && (!muutmine || k.Id != valitudId)))
            {
                Vaade.Teade("Selle nimega kategooria on juba olemas.");
                return;
            }

            Kategooria? k = new Kategooria();
            if (muutmine)
            {
                k = db.Kategooriatabel.Find(valitudId);
            }
            if (k == null)
            {
                Vaade.Teade("Kategooria on kustutatud. Värskenda tabelit.");
                return;
            }

            k.Kategooria_nimetus = nimi.Text.Trim();
            k.Kirjeldus = kirjeldus.Text.Trim();
            if (!muutmine)
            {
                db.Kategooriatabel.Add(k);
            }

            db.SaveChanges();
            Lae();
        }
        catch (Exception ex)
        {
            Vead.Naita("Kategooria salvestamine ei õnnestunud.", ex);
        }
    }

    private void Kustuta(object? sender, EventArgs e)
    {
        if (!AktiivneKasutaja.OnAdmin || valitudId == null)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            if (db.Toodetabel.Any(t => t.KategooriaId == valitudId))
            {
                Vaade.Teade("Kategoorias on tooteid (ka peidetud). Vii need enne teise kategooriasse.");
                return;
            }

            if (Vaade.Teade("Kas kustutan kategooria?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            Kategooria? k = db.Kategooriatabel.Find(valitudId);
            if (k != null)
            {
                db.Kategooriatabel.Remove(k);
                db.SaveChanges();
            }

            Lae();
        }
        catch (Exception ex)
        {
            Vead.Naita("Kategooriat ei saa kustutada. Kontrolli, kas sinna on lisatud tooteid.", ex);
        }
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "Kategooriate haldus", 1060, 650);
        MinimumSize = new Size(860, 520);
        TableLayoutPanel raam = Vaade.Raam(this, "Kategooriate haldus", "Lisa kategooria või vali tabelist olemasolev kategooria muutmiseks.");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        raam.Controls.Add(sisu, 0, 1);
        TableLayoutPanel editor = Vaade.Veerg();
        olek.AutoSize = true;
        olek.Dock = DockStyle.Top;
        olek.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        olek.ForeColor = Vaade.Sinine;
        Vaade.Lisa(editor, olek);
        nimi.PlaceholderText = "Näiteks joogid";
        nimi.MaxLength = 150;
        kirjeldus.PlaceholderText = "Lühike kategooria kirjeldus";
        kirjeldus.MaxLength = 1000;
        kirjeldus.Multiline = true;
        kirjeldus.Height = 100;
        kirjeldus.ScrollBars = ScrollBars.Vertical;
        Vaade.Vali(editor, "Kategooria nimetus", nimi);
        Vaade.Vali(editor, "Kirjeldus", kirjeldus);
        lisa.Text = "Lisa uus kategooria";
        lisa.Click += (s, e) => Salvesta(false);
        muuda.Text = "Salvesta muudatused";
        muuda.Click += (s, e) => Salvesta(true);
        kustuta.Text = "Kustuta kategooria";
        kustuta.Click += Kustuta;
        Button uus = new Button
        {
            Text = "Tühjenda / värskenda"
        };
        uus.Click += (s, e) => Lae();
        foreach (Button nupp in new[]
        {
            lisa,
            muuda,
            uus,
            kustuta
        })
        {
            Vaade.Nupp(nupp, nupp == lisa || nupp == muuda, nupp == kustuta);
            nupp.Dock = DockStyle.Top;
            Vaade.Lisa(editor, nupp);
        }

        sisu.Controls.Add(Vaade.Kerimine(Vaade.Kaart(editor)), 1, 0);
        tabel.Margin = new Padding(0, 0, 16, 0);
        sisu.Controls.Add(tabel, 0, 0);
        tabel.Dock = DockStyle.Fill;
        tabel.ReadOnly = true;
        tabel.MultiSelect = false;
        tabel.AllowUserToAddRows = false;
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabel.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow r = tabel.Rows[e.RowIndex];
            valitudId = (int)r.Cells["Id"].Value;
            nimi.Text = r.Cells["Kategooria_nimetus"].Value?.ToString();
            kirjeldus.Text = r.Cells["Kirjeldus"].Value?.ToString();
            olek.Text = "Kategooria nr " + valitudId;
            lisa.Visible = false;
            muuda.Visible = true;
            kustuta.Enabled = true;
        };
        Vaade.Tabel(tabel);
        Lae();
        ResumeLayout(true);
    }
}
