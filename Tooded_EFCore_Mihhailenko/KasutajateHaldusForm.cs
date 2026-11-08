using System.Data;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tooded_EFCore;

public class KasutajateHaldusForm : Form
{
    private readonly bool esmane;
    private readonly DataGridView tabel = new DataGridView();
    private readonly TextBox nimi = new TextBox();
    private readonly TextBox email = new TextBox();
    private readonly TextBox parool = new TextBox();
    private readonly ComboBox roll = new ComboBox();
    private readonly ComboBox valitudRoll = new ComboBox();
    private readonly CheckBox uusPeabParooliMuutma = new CheckBox()
    {
        Text = "Uus kasutaja peab parooli muutma",
        Checked = true,
        AutoSize = true
    };
    private bool laadimine;
    private readonly Label valitudKasutaja = new Label();
    private readonly Label teade = new Label();
    public KasutajateHaldusForm(bool esmaneSeadistus = false)
    {
        esmane = esmaneSeadistus;
        if (!esmane && !AktiivneKasutaja.OnAdmin)
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
            var kasutajad = db.Kasutajad.AsNoTracking().OrderBy(k => k.Nimi).Select(k => new { k.Id, k.Nimi, k.Email, k.Roll, k.PeabParooliMuutma }).ToList();
            laadimine = true;
            tabel.Rows.Clear();
            foreach (var kasutaja in kasutajad)
            {
                int indeks = tabel.Rows.Add(kasutaja.Id, kasutaja.Nimi, kasutaja.Email, kasutaja.Roll, kasutaja.PeabParooliMuutma);
                tabel.Rows[indeks].Cells["PeabParooliMuutma"].Tag = kasutaja.PeabParooliMuutma;
            }

            tabel.CurrentCell = null;
            tabel.ClearSelection();
            valitudKasutaja.Text = "Rolli muutmiseks vali tabelist konto.";
        }
        catch (Exception ex)
        {
            Vead.Naita("Kasutajate laadimine ei õnnestunud.", ex);
        }
        finally
        {
            laadimine = false;
        }
    }

    private void MuudaParoolivahetust(object? sender, DataGridViewCellEventArgs e)
    {
        if (laadimine || esmane || !AktiivneKasutaja.OnAdmin || e.RowIndex < 0 || e.ColumnIndex < 0 || tabel.Columns[e.ColumnIndex].Name != "PeabParooliMuutma")
        {
            return;
        }

        DataGridViewCell lahter = tabel.Rows[e.RowIndex].Cells[e.ColumnIndex];
        bool uus = Convert.ToBoolean(lahter.Value);
        bool vana = Convert.ToBoolean(lahter.Tag);
        if (uus == vana)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            int id = (int)tabel.Rows[e.RowIndex].Cells["Id"].Value;
            Kasutaja? kasutaja = db.Kasutajad.Find(id);
            if (kasutaja == null)
            {
                throw new InvalidOperationException("Valitud kasutajat enam ei ole.");
            }

            kasutaja.PeabParooliMuutma = uus;
            db.SaveChanges();
            lahter.Tag = uus;
        }
        catch (Exception ex)
        {
            // Ebaõnnestumisel näitame viimati salvestatud väärtust, mitte salvestamata linnukest.
            laadimine = true;
            try
            {
                lahter.Value = vana;
            }
            finally
            {
                laadimine = false;
            }

            Vead.Naita("Paroolivahetuse nõude salvestamine ei õnnestunud. Värskenda kasutajate loend uuesti avades.", ex);
        }
    }

    private void Lisa(object? sender, EventArgs e)
    {
        if (!esmane && !AktiivneKasutaja.OnAdmin)
        {
            return;
        }

        string aadress = email.Text.Trim().ToLowerInvariant();
        teade.Text = "";
        if (string.IsNullOrWhiteSpace(nimi.Text))
        {
            teade.Text = "Sisesta kasutaja nimi.";
            nimi.Focus();
            return;
        }
        if (!MailAddress.TryCreate(aadress, out var kontroll) || kontroll.Address != aadress)
        {
            teade.Text = "Sisesta e-posti aadress, näiteks nimi@post.ee.";
            email.Focus();
            return;
        }
        string parooliViga = PasswordHelper.Kontrolli(parool.Text, parool.Text);
        if (parooliViga != "")
        {
            teade.Text = parooliViga;
            parool.Focus();
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            using IDbContextTransaction tehing = db.Database.BeginTransaction(IsolationLevel.Serializable);
            if (esmane && db.Kasutajad.Any())
            {
                Vaade.Teade("Esimene konto on juba loodud. Sulge aken ja logi sisse.");
                return;
            }

            if (db.Kasutajad.Any(k => k.Email == aadress))
            {
                teade.Text = "Selle e-postiga konto on juba olemas.";
                email.Focus();
                return;
            }

            db.Kasutajad.Add(new Kasutaja { Nimi = nimi.Text.Trim(), Email = aadress, ParoolHash = PasswordHelper.Hash(parool.Text), Roll = esmane ? "admin" : roll.Text, PeabParooliMuutma = !esmane && uusPeabParooliMuutma.Checked });
            db.SaveChanges();
            tehing.Commit();
            parool.Clear();
            nimi.Clear();
            email.Clear();
            if (esmane)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Lae();
            }
        }
        catch (Exception ex)
        {
            Vead.Naita("Kasutaja lisamine ei õnnestunud.", ex);
        }
    }

    private void MuudaRolli(object? sender, EventArgs e)
    {
        if (!AktiivneKasutaja.OnAdmin || tabel.SelectedRows.Count == 0)
        {
            return;
        }

        int id = (int)tabel.SelectedRows[0].Cells["Id"].Value;
        if (id == AktiivneKasutaja.Id)
        {
            Vaade.Teade("Enda rolli siin muuta ei saa. Nii jääb administraatori ligipääs alles.");
            return;
        }

        if (Vaade.Teade("Kas muudan valitud kasutaja rolli? Muutus rakendub tema järgmisel sisselogimisel.", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            using IDbContextTransaction tehing = db.Database.BeginTransaction(IsolationLevel.Serializable);
            Kasutaja? kasutaja = db.Kasutajad.Find(id);
            if (kasutaja == null)
            {
                Vaade.Teade("Kasutajat enam ei ole.");
                return;
            }

            if (kasutaja.Roll == "admin" && valitudRoll.Text != "admin" && db.Kasutajad.Count(k => k.Roll == "admin") <= 1)
            {
                Vaade.Teade("Viimase administraatori rolli ei saa muuta.");
                return;
            }

            kasutaja.Roll = valitudRoll.Text;
            db.SaveChanges();
            tehing.Commit();
            Lae();
        }
        catch (Exception ex)
        {
            Vead.Naita("Rolli muutmine ei õnnestunud.", ex);
        }
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, esmane ? "Esimese administraatori loomine" : "Kasutajate haldus", esmane ? 520 : 1220, 780);
        MinimumSize = new Size(esmane ? 460 : 1000, 580);
        TableLayoutPanel raam = Vaade.Raam(this, Text, esmane ? "Loo esimene administraatori konto, et alustada õppepoe kasutamist." : "Vali konto rolli muutmiseks. Paroolivahetuse linnuke tabelis salvestatakse kohe.");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = esmane ? 1 : 2,
            RowCount = 1
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (!esmane)
        {
            sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
        }

        raam.Controls.Add(sisu, 0, 1);
        TableLayoutPanel paneel = Vaade.Veerg();
        Vaade.Lisa(paneel, Vaade.Tekstirida(esmane ? "Administraatori andmed" : "Uue konto andmed", true));
        nimi.PlaceholderText = "Ees- ja perekonnanimi";
        nimi.MaxLength = 150;
        email.PlaceholderText = "nimi@näide.ee";
        email.MaxLength = 254;
        parool.PlaceholderText = "Vähemalt 8 märki";
        parool.UseSystemPasswordChar = true;
        Vaade.Vali(paneel, "Nimi", nimi);
        Vaade.Vali(paneel, "E-post", email);
        Vaade.Vali(paneel, "Algparool", parool);
        roll.DropDownStyle = ComboBoxStyle.DropDownList;
        roll.Items.AddRange(new object[] { "kasutaja", "admin" });
        roll.SelectedItem = esmane ? "admin" : "kasutaja";
        roll.Enabled = !esmane;
        Vaade.Vali(paneel, "Uue kasutaja roll", roll);
        uusPeabParooliMuutma.Visible = !esmane;
        uusPeabParooliMuutma.Text = "Nõua uuelt kasutajalt paroolivahetust";
        uusPeabParooliMuutma.MaximumSize = new Size(290, 0);
        uusPeabParooliMuutma.Margin = new Padding(0, 4, 0, 14);
        Vaade.Lisa(paneel, uusPeabParooliMuutma);
        Button lisa = new BarboraActionButton {
            Text = esmane ? "Loo administraator" : "Lisa kasutaja",
            Dock = DockStyle.Top
        };
        Vaade.Nupp(lisa, true);
        lisa.Click += Lisa;
        Vaade.Lisa(paneel, lisa);
        teade.AutoSize = true;
        teade.MaximumSize = new Size(290, 0);
        teade.Dock = DockStyle.Top;
        teade.ForeColor = Vaade.Punane;
        Vaade.Lisa(paneel, teade);
        valitudKasutaja.AutoSize = true;
        valitudKasutaja.Dock = DockStyle.Top;
        valitudKasutaja.ForeColor = Vaade.Vaikne;
        valitudKasutaja.Margin = new Padding(0, 16, 0, 10);
        valitudKasutaja.Visible = !esmane;
        valitudKasutaja.Text = "Rolli muutmiseks vali tabelist konto.";
        Vaade.Lisa(paneel, valitudKasutaja);
        valitudRoll.DropDownStyle = ComboBoxStyle.DropDownList;
        valitudRoll.Items.AddRange(new object[] { "kasutaja", "admin" });
        valitudRoll.SelectedIndex = 0;
        if (!esmane)
        {
            Vaade.Vali(paneel, "Valitud kasutaja roll", valitudRoll);
        }
        Button muuda = new BarboraActionButton {
            Text = "Salvesta valitud konto roll",
            Visible = !esmane,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(muuda);
        muuda.Click += MuudaRolli;
        Vaade.Lisa(paneel, muuda);
        if (!esmane)
        {
            Button varskenda = new BarboraActionButton {
                Text = "Värskenda kasutajate loendit",
                Dock = DockStyle.Top
            };
            Vaade.Nupp(varskenda);
            varskenda.Click += (s, e) => Lae();
            Vaade.Lisa(paneel, varskenda);
        }

        sisu.Controls.Add(Vaade.Kerimine(Vaade.Kaart(paneel)), esmane ? 0 : 1, 0);
        tabel.Dock = DockStyle.Fill;
        tabel.ReadOnly = false;
        tabel.AllowUserToAddRows = false;
        tabel.AllowUserToDeleteRows = false;
        tabel.AutoGenerateColumns = false;
        tabel.EditMode = DataGridViewEditMode.EditOnEnter;
        foreach (string veerg in new[]
        {
            "Id",
            "Nimi",
            "Email",
            "Roll"
        })
        {
            tabel.Columns.Add(new DataGridViewTextBoxColumn { Name = veerg, HeaderText = veerg, ReadOnly = true });
        }

        tabel.Columns.Add(new DataGridViewCheckBoxColumn { Name = "PeabParooliMuutma", HeaderText = "Peab parooli muutma", ReadOnly = false, ThreeState = false, ToolTipText = "Administraator saab linnukest muuta. Salvestatakse kohe; rakendub järgmisel sisselogimisel." });
        tabel.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (tabel.IsCurrentCellDirty && tabel.CurrentCell is DataGridViewCheckBoxCell)
            {
                tabel.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        tabel.CellValueChanged += MuudaParoolivahetust;
        tabel.DataError += (s, e) =>
        {
            e.ThrowException = false;
        };
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.MultiSelect = false;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabel.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            valitudRoll.SelectedItem = tabel.Rows[e.RowIndex].Cells["Roll"].Value;
            valitudKasutaja.Text = "Valitud konto: " + tabel.Rows[e.RowIndex].Cells["Nimi"].Value + "\nVali roll ja salvesta muudatus.";
        };
        Vaade.Tabel(tabel);
        tabel.Margin = new Padding(0, 0, 16, 0);
        tabel.Columns["Id"].Visible = false;
        Vaade.VeeruNimi(tabel, "Nimi", "Nimi", 120);
        Vaade.VeeruNimi(tabel, "Email", "E-post", 170);
        Vaade.VeeruNimi(tabel, "Roll", "Roll", 75);
        Vaade.VeeruNimi(tabel, "PeabParooliMuutma", "Nõua paroolivahetust", 100);
        if (!esmane)
        {
            sisu.Controls.Add(tabel, 0, 0);
        }

        if (!esmane)
        {
            Lae();
        }

        ResumeLayout(true);
    }
}
