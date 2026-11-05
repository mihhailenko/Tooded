using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class TellimusteForm : Form
{
    private readonly DataGridView tabel = new DataGridView();
    private readonly DateTimePicker algus = new DateTimePicker();
    private readonly DateTimePicker lopp = new DateTimePicker();
    private readonly Label kokku = new Label();
    public TellimusteForm()
    {
        if (AktiivneKasutaja.Id == 0)
        {
            throw new UnauthorizedAccessException();
        }

        Text = AktiivneKasutaja.OnAdmin ? "Kõik tellimused ja müük" : "Minu tellimused";
        LooLiides();
    }

    private void LaeTellimused()
    {
        if (algus.Value.Date > lopp.Value.Date)
        {
            Vaade.Teade("Alguskuupäev ei või olla lõppkuupäevast hilisem.");
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            DateTime alates = algus.Value.Date;
            DateTime kuni = lopp.Value.Date.AddDays(1);
            IQueryable<Tellimus> paring = db.Tellimused.AsNoTracking().Where(t => t.Kuupaev >= alates && t.Kuupaev < kuni);
            if (!AktiivneKasutaja.OnAdmin)
            {
                paring = paring.Where(t => t.KasutajaId == AktiivneKasutaja.Id);
            }

            var tellimused = paring.OrderByDescending(t => t.Kuupaev).Select(t => new { t.Id, t.Number, t.Kuupaev, t.OstjaNimi, t.OstjaEmail, t.Summa, t.Staatus }).ToList();
            tabel.DataSource = tellimused;
            tabel.Columns["Id"].Visible = false;
            Vaade.VeeruNimi(tabel, "Number", "Tellimuse nr", 110);
            Vaade.VeeruNimi(tabel, "Kuupaev", "Kuupäev", 115);
            Vaade.VeeruNimi(tabel, "OstjaNimi", "Ostja nimi", 120);
            Vaade.VeeruNimi(tabel, "OstjaEmail", "Ostja e-post", 160);
            Vaade.VeeruNimi(tabel, "Summa", "Summa (€)", 75);
            tabel.Columns["Summa"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tabel.Columns["Kuupaev"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            tabel.Columns["Summa"].DefaultCellStyle.Format = "N2";
            kokku.Text = $"Perioodil {tellimused.Count} tellimust · " + (AktiivneKasutaja.OnAdmin ? "Müügitulu: " : "Ostud kokku: ") + $"{tellimused.Sum(t => t.Summa):F2} €";
            tabel.ClearSelection();
        }
        catch (Exception ex)
        {
            Vead.Naita("Tellimuste laadimine ei õnnestunud.", ex);
        }
    }

    private void Ava()
    {
        if (tabel.SelectedRows.Count == 0)
        {
            Vaade.Teade("Vali tabelist tellimus.");
            return;
        }

        int id = (int)tabel.SelectedRows[0].Cells["Id"].Value;
        using ArveForm vorm = new ArveForm(id);
        vorm.ShowDialog(this);
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, Text, 1160, 730);
        MinimumSize = new Size(850, 520);
        TableLayoutPanel raam = Vaade.Raam(this, Text, "Vali periood. Tellimuse avamiseks vali rida ja vajuta Ava tellimus või tee topeltklõps.");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sisu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sisu.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sisu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raam.Controls.Add(sisu, 0, 1);
        FlowLayoutPanel riba = Vaade.Nupuriba();
        lopp.Format = DateTimePickerFormat.Short;
        algus.Format = DateTimePickerFormat.Short;
        algus.Value = new DateTime(2000, 1, 1);
        lopp.Value = DateTime.Today;
        lopp.Width = 145;
        algus.Width = 145;
        algus.AccessibleName = "Perioodi algus";
        lopp.AccessibleName = "Perioodi lõpp";
        riba.Controls.Add(new Label { Text = "Alates", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        riba.Controls.Add(algus);
        riba.Controls.Add(new Label { Text = "Kuni", AutoSize = true, Margin = new Padding(8, 7, 6, 0) });
        riba.Controls.Add(lopp);
        Button lae = new Button
        {
            Text = "Näita perioodi"
        };
        Vaade.Nupp(lae);
        lae.Click += (s, e) => LaeTellimused();
        Button ava = new Button
        {
            Text = "Ava tellimus"
        };
        Vaade.Nupp(ava, true);
        ava.Click += (s, e) => Ava();
        riba.Controls.AddRange(new Control[] { lae, ava });
        kokku.AutoSize = true;
        kokku.Dock = DockStyle.Top;
        kokku.Padding = new Padding(12);
        kokku.BackColor = Color.White;
        kokku.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        sisu.Controls.Add(riba, 0, 0);
        sisu.Controls.Add(tabel, 0, 1);
        sisu.Controls.Add(kokku, 0, 2);
        tabel.Dock = DockStyle.Fill;
        tabel.ReadOnly = true;
        tabel.AllowUserToAddRows = false;
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.MultiSelect = false;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabel.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                Ava();
            }
        };
        Vaade.Tabel(tabel);
        LaeTellimused();
        ResumeLayout(true);
    }
}
