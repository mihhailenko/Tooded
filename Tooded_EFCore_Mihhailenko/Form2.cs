using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form2 : Form
{
    private readonly DataGridView dataGridViewTooted = new DataGridView();
    private readonly TextBox txtNimetus = new TextBox();
    private readonly TextBox txtKogus = new TextBox();
    private readonly TextBox txtHind = new TextBox();
    private readonly TextBox txtOtsing = new TextBox();
    private readonly TextBox txtKirjeldus = new TextBox();
    private readonly ComboBox comboKategooria = new ComboBox();
    private readonly TootePilt picturePilt = new();
    private CancellationTokenSource? pildiLaadimine;
    private readonly CheckBox chkAktiivne = new CheckBox()
    {
        Text = "Müügil",
        Checked = true,
        AutoSize = true
    };
    private readonly Button btnLisa = new BarboraActionButton();
    private readonly Button btnUuenda = new BarboraActionButton();
    private readonly Button btnKustuta = new BarboraActionButton();
    private readonly Label lblReziim = new Label();
    private Toode? valitudToode;
    private string? valitudPilt;
    private bool eemaldaPilt;
    public Form2()
    {
        if (!AktiivneKasutaja.OnAdmin)
        {
            throw new UnauthorizedAccessException();
        }

        InitializeComponent();
        LooLiides();
        LaeKategooriad();
        LaeTooted();
        TyhjendaVorm();
    }

    private void LooLiides()
    {
        Vaade.Vorm(this, "Toodete haldus", 1220, 820);
        MinimumSize = new Size(1000, 650);
        TableLayoutPanel raam = Vaade.Raam(this, "Toodete haldus", "Vali tabelist toode muutmiseks või alusta uue toote lisamist.");
        TableLayoutPanel paigutus = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        paigutus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        paigutus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
        raam.Controls.Add(paigutus, 0, 1);
        TableLayoutPanel editor = Vaade.Veerg();
        lblReziim.AutoSize = true;
        lblReziim.Dock = DockStyle.Top;
        lblReziim.Font = BarboraTheme.Font(12F, FontStyle.Bold);
        lblReziim.ForeColor = Vaade.Sinine;
        lblReziim.Margin = new Padding(0, 0, 0, 16);
        Vaade.Lisa(editor, lblReziim);
        txtNimetus.MaxLength = 200;
        txtKogus.PlaceholderText = "Näiteks 25";
        txtHind.PlaceholderText = "Näiteks 2,50";
        txtKirjeldus.Multiline = true;
        txtKirjeldus.Height = 80;
        txtKirjeldus.MaxLength = 2000;
        txtKirjeldus.ScrollBars = ScrollBars.Vertical;
        comboKategooria.DropDownStyle = ComboBoxStyle.DropDownList;
        Vaade.Vali(editor, "Toote nimetus", txtNimetus);
        Vaade.Vali(editor, "Laokogus (tk)", txtKogus);
        Vaade.Vali(editor, "Ühiku hind (€)", txtHind);
        Vaade.Vali(editor, "Kategooria", comboKategooria);
        Vaade.Vali(editor, "Kirjeldus", txtKirjeldus);
        Vaade.Lisa(editor, chkAktiivne);
        picturePilt.Height = 150;
        picturePilt.Dock = DockStyle.Top;
        picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
        picturePilt.BackColor = Vaade.Taust;
        Vaade.Vali(editor, "Tootepilt", picturePilt);
        Button vali = new BarboraActionButton {
            Text = "Vali pilt"
        };
        vali.Click += ValiPilt;
        Button eemalda = new BarboraActionButton {
            Text = "Eemalda pilt"
        };
        eemalda.Click += eemalda_Click;
        FlowLayoutPanel pildinupud = Vaade.Nupuriba();
        Vaade.Nupp(vali);
        Vaade.Nupp(eemalda);
        pildinupud.Controls.AddRange(new Control[] { vali, eemalda });
        Vaade.Lisa(editor, pildinupud);
        btnLisa.Text = "Lisa uus toode";
        btnLisa.Click += (s, e) => Salvesta(false);
        btnUuenda.Text = "Salvesta muudatused";
        btnUuenda.Click += (s, e) => Salvesta(true);
        btnKustuta.Text = "Eemalda toode";
        btnKustuta.Click += Kustuta;
        Button tyhjenda = new BarboraActionButton {
            Text = "Alusta uut toodet"
        };
        tyhjenda.Click += (s, e) => TyhjendaVorm();
        Button kategooriad = new BarboraActionButton {
            Text = "Halda kategooriaid"
        };
        kategooriad.Click += (s, e) =>
        {
            using KategooriateHaldusForm vorm = new KategooriateHaldusForm();
            vorm.ShowDialog(this);
            LaeKategooriad();
            LaeTooted();
            TyhjendaVorm();
        };
        foreach (Button nupp in new[]
        {
            btnLisa,
            btnUuenda,
            tyhjenda,
            btnKustuta,
            kategooriad
        })
        {
            Vaade.Nupp(nupp, nupp == btnLisa || nupp == btnUuenda, nupp == btnKustuta);
            nupp.Dock = DockStyle.Top;
            Vaade.Lisa(editor, nupp);
        }

        paigutus.Controls.Add(Vaade.Kerimine(Vaade.Kaart(editor)), 1, 0);
        TableLayoutPanel keskel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 16, 0)
        };
        keskel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        keskel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        keskel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        FlowLayoutPanel otsinguriba = Vaade.Nupuriba();
        txtOtsing.Width = 270;
        txtOtsing.PlaceholderText = "Otsi toodet nime järgi";
        txtOtsing.AccessibleName = "Toote otsing";
        txtOtsing.TextChanged += (s, e) =>
        {
            LaeTooted();
            TyhjendaVorm();
        };
        Button uuenda = new BarboraActionButton {
            Text = "Värskenda"
        };
        Vaade.Nupp(uuenda);
        uuenda.Click += (s, e) =>
        {
            LaeKategooriad();
            LaeTooted();
            TyhjendaVorm();
        };
        otsinguriba.Controls.AddRange(new Control[] { txtOtsing, uuenda });
        dataGridViewTooted.Dock = DockStyle.Fill;
        dataGridViewTooted.ReadOnly = true;
        dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewTooted.MultiSelect = false;
        dataGridViewTooted.AllowUserToAddRows = false;
        dataGridViewTooted.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Vaade.Tabel(dataGridViewTooted);
        dataGridViewTooted.CellClick += ValiToode;
        dataGridViewTooted.KeyUp += (s, e) =>
        {
            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && dataGridViewTooted.CurrentCell != null)
            {
                ValiToode(s, new DataGridViewCellEventArgs(dataGridViewTooted.CurrentCell.ColumnIndex, dataGridViewTooted.CurrentCell.RowIndex));
            }
        };
        keskel.Controls.Add(otsinguriba, 0, 0);
        keskel.Controls.Add(dataGridViewTooted, 0, 1);
        paigutus.Controls.Add(keskel, 0, 0);
        ResumeLayout(true);
    }

    private void LaeKategooriad()
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            comboKategooria.DisplayMember = "Kategooria_nimetus";
            comboKategooria.ValueMember = "Id";
            comboKategooria.DataSource = db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).ToList();
        }
        catch (Exception ex)
        {
            Vead.Naita("Kategooriate laadimine ei õnnestunud.", ex);
        }
    }

    private void LaeTooted()
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            string otsing = txtOtsing.Text.Trim();
            dataGridViewTooted.DataSource = db.Toodetabel.AsNoTracking().Where(t => t.Toodenimetus.Contains(otsing)).OrderBy(t => t.Toodenimetus).Select(t => new { t.Id, Nimetus = t.Toodenimetus, t.Kogus, t.Hind, Kategooria = t.Kategooria.Kategooria_nimetus, t.Aktiivne }).ToList();
            dataGridViewTooted.Columns["Hind"].DefaultCellStyle.Format = "N2";
            dataGridViewTooted.Columns["Id"].Visible = false;
            Vaade.VeeruNimi(dataGridViewTooted, "Nimetus", "Toote nimetus", 200);
            Vaade.VeeruNimi(dataGridViewTooted, "Kogus", "Laos (tk)", 65);
            Vaade.VeeruNimi(dataGridViewTooted, "Hind", "Hind (€)", 75);
            Vaade.VeeruNimi(dataGridViewTooted, "Kategooria", "Kategooria", 120);
            dataGridViewTooted.ClearSelection();
        }
        catch (Exception ex)
        {
            Vead.Naita("Toodete laadimine ei õnnestunud.", ex);
        }
    }

    private void TyhjendaVorm()
    {
        pildiLaadimine?.Cancel();
        picturePilt.Nimetus = "";
        valitudToode = null;
        valitudPilt = null;
        eemaldaPilt = false;
        dataGridViewTooted.CurrentCell = null;
        dataGridViewTooted.ClearSelection();
        txtNimetus.Clear();
        txtKogus.Clear();
        txtHind.Clear();
        txtKirjeldus.Clear();
        chkAktiivne.Checked = true;
        picturePilt.Image?.Dispose();
        picturePilt.Image = null;
        lblReziim.Text = "Uue toote lisamine";
        btnLisa.Visible = true;
        btnUuenda.Visible = false;
        btnLisa.Enabled = true;
        btnKustuta.Enabled = false;
        btnUuenda.Enabled = false;
    }

    private async void ValiToode(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            int id = (int)dataGridViewTooted.Rows[e.RowIndex].Cells["Id"].Value;
            valitudToode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == id);
            if (valitudToode == null)
            {
                TyhjendaVorm();
                LaeTooted();
                return;
            }

            txtNimetus.Text = valitudToode.Toodenimetus;
            txtKogus.Text = valitudToode.Kogus.ToString();
            txtHind.Text = valitudToode.Hind.ToString("F2");
            txtKirjeldus.Text = valitudToode.Kirjeldus;
            comboKategooria.SelectedValue = valitudToode.KategooriaId;
            chkAktiivne.Checked = valitudToode.Aktiivne;
            valitudPilt = null;
            eemaldaPilt = false;
            picturePilt.Image?.Dispose();
            picturePilt.Nimetus = valitudToode.Toodenimetus;
            picturePilt.Image = null; picturePilt.Invalidate();
            lblReziim.Text = "Muudan toodet nr " + id + (valitudToode.ValineId == null ? "" : "\nVeebitoote salvestamine loob kohaliku erandi.");
            btnLisa.Visible = false;
            btnUuenda.Visible = true;
            btnLisa.Enabled = false;
            btnKustuta.Enabled = true;
            btnUuenda.Enabled = true;
            pildiLaadimine?.Cancel();
            var laadimine = new CancellationTokenSource(); pildiLaadimine = laadimine;
            try
            {
                var image = await PildiVahemalu.LaeAsync(valitudToode, laadimine.Token);
                if (IsDisposed || laadimine.IsCancellationRequested || valitudToode?.Id != id) image?.Dispose();
                else { picturePilt.Image?.Dispose(); picturePilt.Image = image; picturePilt.Invalidate(); }
            }
            catch (OperationCanceledException) { }
        }
        catch (Exception ex)
        {
            Vead.Naita("Toote avamine ei õnnestunud.", ex);
        }
    }

    private void ValiPilt(object? sender, EventArgs e)
    {
        using OpenFileDialog aken = new OpenFileDialog
        {
            Filter = "Pildid|*.jpg;*.jpeg;*.png"
        };
        if (aken.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (new FileInfo(aken.FileName).Length > 10 * 1024 * 1024)
            {
                Vaade.Teade("Pilt võib olla kuni 10 MB.");
                return;
            }

            using Image pilt = Image.FromFile(aken.FileName);
            if (pilt.Width > 8000 || pilt.Height > 8000)
            {
                Vaade.Teade("Pildi mõõtmed võivad olla kuni 8000 × 8000.");
                return;
            }

            Bitmap koopia = new Bitmap(pilt);
            picturePilt.Image?.Dispose();
            picturePilt.Image = koopia;
            valitudPilt = aken.FileName;
            eemaldaPilt = false;
        }
        catch (Exception ex)
        {
            Vead.Naita("Valitud pildifaili ei saa avada.", ex);
        }
    }

    private bool KontrolliAndmed(out int kogus, out decimal hind)
    {
        kogus = 0;
        hind = 0;
        if (string.IsNullOrWhiteSpace(txtNimetus.Text))
        {
            Vaade.Teade("Sisesta toote nimetus.");
            return false;
        }
        if (!int.TryParse(txtKogus.Text, out kogus) || kogus < 0)
        {
            Vaade.Teade("Laokogus peab olema täisarv ja vähemalt 0.");
            return false;
        }
        string hindTekst = txtHind.Text.Trim().Replace(',', '.');
        if (!decimal.TryParse(hindTekst, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out hind))
        {
            Vaade.Teade("Sisesta hind arvuna, näiteks 2,50.");
            return false;
        }
        if (hind < 0)
        {
            Vaade.Teade("Hind ei saa olla negatiivne.");
            return false;
        }
        if (hind > 99999999.99m)
        {
            Vaade.Teade("See hind on liiga suur. Sisesta väiksem hind.");
            return false;
        }
        if (decimal.Round(hind, 2) != hind)
        {
            Vaade.Teade("Sisesta hind kuni kahe komakohaga, näiteks 2,50.");
            return false;
        }
        if (comboKategooria.SelectedValue == null)
        {
            Vaade.Teade("Vali kategooria.");
            return false;
        }
        return true;
    }

    private void Salvesta(bool muutmine)
    {
        if (!AktiivneKasutaja.OnAdmin || (muutmine && valitudToode == null))
        {
            return;
        }

        int kogus;
        decimal hind;
        if (!KontrolliAndmed(out kogus, out hind))
        {
            return;
        }

        if (muutmine && Vaade.Teade("Kas salvestan valitud toote muudatused?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
        {
            return;
        }

        string? uusPilt = null;
        bool salvestatud = false;
        try
        {
            using ToodedContext db = new ToodedContext();
            Toode? toode = new Toode();
            if (muutmine && valitudToode != null)
            {
                toode = db.Toodetabel.Find(valitudToode.Id);
            }
            if (toode == null)
            {
                Vaade.Teade("Toode on vahepeal kustutatud. Värskenda tabelit.");
                return;
            }

            string? vanaPilt = toode.Pilt;
            if (muutmine)
            {
                db.Entry(toode).Property(t => t.ReaVersioon).OriginalValue = valitudToode!.ReaVersioon;
            }

            toode.Toodenimetus = txtNimetus.Text.Trim();
            toode.Kogus = kogus;
            toode.Hind = hind;
            if (toode.Tavahind <= hind) toode.Tavahind = null;
            toode.KategooriaId = Convert.ToInt32(comboKategooria.SelectedValue);
            toode.Kirjeldus = txtKirjeldus.Text.Trim();
            toode.Aktiivne = chkAktiivne.Checked;
            // Importtoote käsitsi salvestamine kaitseb selle kõiki välju järgmise impordi eest.
            if (toode.ValineId != null) toode.KohalikMuudatus = true;
            if (eemaldaPilt)
            {
                toode.Pilt = null;
                toode.PildiUrl = null;
            }

            if (valitudPilt != null)
            {
                // Salvestame alati oma PNG koopia: allika nimi ei saa teist pilti asendada.
                uusPilt = Guid.NewGuid().ToString("N") + ".png";
                using Image pilt = Image.FromFile(valitudPilt);
                pilt.Save(Path.Combine(Kaustad.Pildid, uusPilt), System.Drawing.Imaging.ImageFormat.Png);
                toode.Pilt = uusPilt;
            }

            if (!muutmine)
            {
                db.Toodetabel.Add(toode);
            }

            db.SaveChanges();
            salvestatud = true;
            if (vanaPilt != toode.Pilt)
            {
                Kaustad.KustutaKasutamataPilt(vanaPilt);
            }

            TyhjendaVorm();
            LaeTooted();
        }
        catch (DbUpdateConcurrencyException)
        {
            Vaade.Teade("Toote andmed on muutunud. Vajuta Värskenda ja vali toode uuesti.");
        }
        catch (Exception ex)
        {
            Vead.Naita("Toote salvestamine ei õnnestunud.", ex);
        }
        finally
        {
            if (!salvestatud && uusPilt != null)
            {
                // Ühendus võis katkeda pärast salvestamist: enne kustutamist kontrollime viiteid.
                Kaustad.KustutaKasutamataPilt(uusPilt);
            }
        }
    }

    private void Kustuta(object? sender, EventArgs e)
    {
        if (!AktiivneKasutaja.OnAdmin || valitudToode == null)
        {
            return;
        }

        try
        {
            using ToodedContext db = new ToodedContext();
            Toode? toode = db.Toodetabel.Find(valitudToode.Id);
            if (toode == null)
            {
                TyhjendaVorm();
                LaeTooted();
                return;
            }

            bool kasutatud = toode.ValineId != null || db.TellimuseRead.Any(r => r.ToodeId == toode.Id);
            string teade = toode.ValineId != null ? "Kas eemaldan veebitoote müügist? Kohalik erand hoiab selle peidetuna ka järgmisel impordil." : kasutatud ? "Seda toodet on juba ostetud. Kas eemaldada see müügist? Varasemad ostud jäävad alles." : "Kas kustutan toote lõplikult?";
            if (Vaade.Teade(teade, "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            db.Entry(toode).Property(t => t.ReaVersioon).OriginalValue = valitudToode.ReaVersioon;
            string? vanaPilt = toode.Pilt;
            if (kasutatud)
            {
                toode.Aktiivne = false;
                if (toode.ValineId != null) toode.KohalikMuudatus = true;
            }
            else
            {
                db.Toodetabel.Remove(toode);
            }

            db.SaveChanges();
            if (!kasutatud)
            {
                Kaustad.KustutaKasutamataPilt(vanaPilt);
            }

            TyhjendaVorm();
            LaeTooted();
        }
        catch (DbUpdateConcurrencyException)
        {
            Vaade.Teade("Toodet on vahepeal muudetud. Värskenda tabelit.");
        }
        catch (Exception ex)
        {
            Vead.Naita("Toodet ei saanud eemaldada. Proovi uuesti.", ex);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        pildiLaadimine?.Cancel();
        picturePilt.Image?.Dispose();
        base.OnFormClosed(e);
    }

    private void eemalda_Click(object? s, EventArgs e)
    {
        valitudPilt = null;
        eemaldaPilt = true;
        picturePilt.Image?.Dispose();
        picturePilt.Image = null;
    }
}
