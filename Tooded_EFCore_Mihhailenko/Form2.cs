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
    private readonly PictureBox picturePilt = new PictureBox();
    private readonly CheckBox chkAktiivne = new CheckBox()
    {
        Text = "Müügil",
        Checked = true,
        AutoSize = true
    };
    private readonly Button btnLisa = new Button();
    private readonly Button btnUuenda = new Button();
    private readonly Button btnKustuta = new Button();
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
        Text = "Toodete haldus";
        ClientSize = new Size(1080, 740);
        StartPosition = FormStartPosition.CenterParent;
        FlowLayoutPanel andmed = new FlowLayoutPanel();
        andmed.Dock = DockStyle.Right;
        andmed.Width = 300;
        andmed.Padding = new Padding(12);
        andmed.FlowDirection = FlowDirection.TopDown;
        andmed.WrapContents = false;
        andmed.AutoScroll = true;
        lblReziim.Height = 30;
        txtNimetus.PlaceholderText = "Toote nimetus";
        txtNimetus.MaxLength = 200;
        txtKogus.PlaceholderText = "Kogus";
        txtHind.PlaceholderText = "Hind, näiteks 2,50";
        txtKirjeldus.PlaceholderText = "Kirjeldus";
        txtKirjeldus.Multiline = true;
        txtKirjeldus.Height = 70;
        txtKirjeldus.MaxLength = 2000;
        comboKategooria.DropDownStyle = ComboBoxStyle.DropDownList;
        picturePilt.Height = 130;
        picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
        Control[] valjad = { lblReziim, txtNimetus, txtKogus, txtHind, comboKategooria, txtKirjeldus, chkAktiivne, picturePilt };
        foreach (Control vali in valjad)
        {
            vali.Width = 260;
            andmed.Controls.Add(vali);
        }
        Button pilt = new Button();
        pilt.Text = "Vali pilt";
        pilt.Click += ValiPilt;
        Button eemalda = new Button();
        eemalda.Text = "Eemalda pilt";
        eemalda.Click += eemalda_Click;
        btnLisa.Text = "Lisa toode";
        btnLisa.Click += (s,e) => Salvesta(false);
        btnUuenda.Text = "Salvesta muudatused";
        btnUuenda.Click += (s,e) => Salvesta(true);
        btnKustuta.Text = "Eemalda toode";
        btnKustuta.Click += Kustuta;
        Button uus = new Button();
        uus.Text = "Uus toode";
        uus.Click += (s,e) => TyhjendaVorm();
        foreach (Button nupp in new[] { pilt, eemalda, btnLisa, btnUuenda, btnKustuta, uus })
        {
            nupp.Width = 260;
            nupp.Height = 34;
            andmed.Controls.Add(nupp);
        }
        Panel tabeliAla = new Panel();
        tabeliAla.Dock = DockStyle.Fill;
        txtOtsing.Dock = DockStyle.Top;
        txtOtsing.PlaceholderText = "Otsi nime järgi";
        txtOtsing.TextChanged += (s,e) => { LaeTooted(); TyhjendaVorm(); };
        dataGridViewTooted.Dock = DockStyle.Fill;
        dataGridViewTooted.ReadOnly = true;
        dataGridViewTooted.AllowUserToAddRows = false;
        dataGridViewTooted.MultiSelect = false;
        dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewTooted.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewTooted.CellClick += ValiToode;
        tabeliAla.Controls.Add(dataGridViewTooted);
        tabeliAla.Controls.Add(txtOtsing);
        Controls.Add(tabeliAla);
        Controls.Add(andmed);
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

    private void ValiToode(object? sender, DataGridViewCellEventArgs e)
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
            picturePilt.Image = Kaustad.LaePilt(valitudToode.Pilt);
            lblReziim.Text = "Muudan toodet nr " + id;
            btnLisa.Visible = false;
            btnUuenda.Visible = true;
            btnLisa.Enabled = false;
            btnKustuta.Enabled = true;
            btnUuenda.Enabled = true;
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
                MessageBox.Show("Pilt võib olla kuni 10 MB.");
                return;
            }

            using Image pilt = Image.FromFile(aken.FileName);
            if (pilt.Width > 8000 || pilt.Height > 8000)
            {
                MessageBox.Show("Pildi mõõtmed võivad olla kuni 8000 × 8000.");
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
            MessageBox.Show("Sisesta toote nimetus.");
            return false;
        }
        if (!int.TryParse(txtKogus.Text, out kogus) || kogus < 0)
        {
            MessageBox.Show("Laokogus peab olema täisarv ja vähemalt 0.");
            return false;
        }
        string hindTekst = txtHind.Text.Trim().Replace(',', '.');
        if (!decimal.TryParse(hindTekst, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out hind))
        {
            MessageBox.Show("Sisesta hind arvuna, näiteks 2,50.");
            return false;
        }
        if (hind < 0)
        {
            MessageBox.Show("Hind ei saa olla negatiivne.");
            return false;
        }
        if (hind > 99999999.99m)
        {
            MessageBox.Show("See hind on liiga suur. Sisesta väiksem hind.");
            return false;
        }
        if (decimal.Round(hind, 2) != hind)
        {
            MessageBox.Show("Sisesta hind kuni kahe komakohaga, näiteks 2,50.");
            return false;
        }
        if (comboKategooria.SelectedValue == null)
        {
            MessageBox.Show("Vali kategooria.");
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

        if (muutmine && MessageBox.Show("Kas salvestan valitud toote muudatused?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
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
                MessageBox.Show("Toode on vahepeal kustutatud. Värskenda tabelit.");
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
            toode.KategooriaId = Convert.ToInt32(comboKategooria.SelectedValue);
            toode.Kirjeldus = txtKirjeldus.Text.Trim();
            toode.Aktiivne = chkAktiivne.Checked;
            if (eemaldaPilt)
            {
                toode.Pilt = null;
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
            MessageBox.Show("Toote andmed on muutunud. Vajuta Värskenda ja vali toode uuesti.");
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

            bool kasutatud = db.TellimuseRead.Any(r => r.ToodeId == toode.Id);
            string teade = kasutatud ? "Seda toodet on juba ostetud. Kas eemaldada see müügist? Varasemad ostud jäävad alles." : "Kas kustutan toote lõplikult?";
            if (MessageBox.Show(teade, "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            db.Entry(toode).Property(t => t.ReaVersioon).OriginalValue = valitudToode.ReaVersioon;
            string? vanaPilt = toode.Pilt;
            if (kasutatud)
            {
                toode.Aktiivne = false;
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
            MessageBox.Show("Toodet on vahepeal muudetud. Värskenda tabelit.");
        }
        catch (Exception ex)
        {
            Vead.Naita("Toodet ei saanud eemaldada. Proovi uuesti.", ex);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
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
