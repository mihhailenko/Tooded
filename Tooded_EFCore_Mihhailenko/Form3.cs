using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form3 : Form
{
    private readonly TabControl tabPood = new TabControl();
    private readonly Label lblOstukorv = new Label();
    private readonly TextBox txtOtsing = new TextBox();
    private readonly ComboBox sortimine = new ComboBox();
    private readonly CheckBox ainultLaos = new CheckBox()
    {
        Text = "Ainult laos olevad",
        AutoSize = true
    };
    public Form3()
    {
        InitializeComponent();
        LooLiides();
    }

    private void VabastaKaardid()
    {
        foreach (TabPage leht in tabPood.TabPages)
        {
            foreach (Control pilt in leht.Controls.Find("Tootepilt", true))
            {
                if (pilt is PictureBox vaade)
                {
                    vaade.Image?.Dispose();
                }
            }
        }

        while (tabPood.TabPages.Count > 0)
        {
            TabPage leht = tabPood.TabPages[0];
            tabPood.TabPages.Remove(leht);
            leht.Dispose();
        }
    }

    private void LaePood()
    {
        try
        {
            using ToodedContext db = new ToodedContext();
            List<Kategooria> kategooriad = db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Kategooria_nimetus).ToList();
            string otsing = txtOtsing.Text.Trim();
            IQueryable<Toode> paring = db.Toodetabel.AsNoTracking().Where(t => t.Aktiivne && t.Toodenimetus.Contains(otsing));
            if (ainultLaos.Checked)
            {
                paring = paring.Where(t => t.Kogus > 0);
            }

            List<Toode> tooted = paring.ToList();
            if (sortimine.SelectedIndex == 1)
            {
                tooted = tooted.OrderByDescending(t => t.Toodenimetus).ToList();
            }
            else if (sortimine.SelectedIndex == 2)
            {
                tooted = tooted.OrderBy(t => t.Hind).ThenBy(t => t.Toodenimetus).ToList();
            }
            else if (sortimine.SelectedIndex == 3)
            {
                tooted = tooted.OrderByDescending(t => t.Hind).ThenBy(t => t.Toodenimetus).ToList();
            }
            else
            {
                tooted = tooted.OrderBy(t => t.Toodenimetus).ToList();
            }

            int valitud = tabPood.SelectedIndex;
            VabastaKaardid();
            foreach (Kategooria kat in kategooriad)
            {
                TabPage leht = new TabPage(kat.Kategooria_nimetus)
                {
                    BackColor = Vaade.Taust
                };
                FlowLayoutPanel paneel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(8),
                    WrapContents = true,
                    BackColor = Vaade.Taust
                };
                foreach (Toode toode in tooted)
                {
                    if (toode.KategooriaId == kat.Id)
                    {
                        paneel.Controls.Add(LooTootekaart(toode));
                    }
                }

                if (paneel.Controls.Count == 0)
                {
                    paneel.Controls.Add(new Label { Text = "Sobivaid tooteid ei leitud. Muuda otsingut või laoseisu filtrit.", AutoSize = true, Padding = new Padding(16), ForeColor = Vaade.Vaikne });
                }

                leht.Controls.Add(paneel);
                tabPood.TabPages.Add(leht);
            }

            if (valitud >= 0 && valitud < tabPood.TabCount)
            {
                tabPood.SelectedIndex = valitud;
            }
        }
        catch (Exception ex)
        {
            Vead.Naita("Poe laadimine ei õnnestunud.", ex);
        }
    }

    private Panel LooTootekaart(Toode t)
    {
        Panel kaart = new Panel
        {
            Size = new Size(252, 470),
            Margin = new Padding(8),
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        TableLayoutPanel sisu = Vaade.Veerg();
        PictureBox pilt = new PictureBox
        {
            Name = "Tootepilt",
            Dock = DockStyle.Top,
            Height = 150,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = Kaustad.LaePilt(t.Pilt),
            BackColor = Vaade.Taust,
            Cursor = Cursors.Hand,
            AccessibleName = t.Toodenimetus + " – ava tootepilt",
            Margin = new Padding(0, 0, 0, 12)
        };
        Label nimi = new Label
        {
            Dock = DockStyle.Top,
            Height = 52,
            Text = t.Toodenimetus,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseMnemonic = false
        };
        Label kirjeldus = new Label
        {
            Dock = DockStyle.Top,
            Height = 46,
            Text = string.IsNullOrWhiteSpace(t.Kirjeldus) ? "Täpsema vaate avamiseks klõpsa pildil." : t.Kirjeldus,
            AutoEllipsis = true,
            ForeColor = Vaade.Vaikne,
            UseMnemonic = false
        };
        Label hind = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = $"{t.Hind:F2} €",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Vaade.Sinine
        };
        Label laoseis = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = t.Kogus > 0 ? $"Laos: {t.Kogus} tk" : "Laost otsas",
            ForeColor = t.Kogus > 0 ? Color.FromArgb(41, 111, 76) : Vaade.Punane
        };
        NumericUpDown kogus = new NumericUpDown
        {
            Dock = DockStyle.Top,
            Minimum = 1,
            Maximum = Math.Max(1, t.Kogus),
            Value = 1,
            Enabled = t.Kogus > 0,
            AccessibleName = "Kogus: " + t.Toodenimetus,
            Margin = new Padding(0, 3, 8, 0)
        };
        Button nupp = new Button
        {
            Text = "Lisa korvi",
            Enabled = t.Kogus > 0,
            Dock = DockStyle.Top
        };
        Vaade.Nupp(nupp, true);
        TableLayoutPanel tegevused = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Margin = Padding.Empty
        };
        tegevused.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        tegevused.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        tegevused.Controls.Add(kogus, 0, 0);
        tegevused.Controls.Add(nupp, 1, 0);
        Vaade.Lisa(sisu, pilt);
        Vaade.Lisa(sisu, nimi);
        Vaade.Lisa(sisu, kirjeldus);
        Vaade.Lisa(sisu, hind);
        Vaade.Lisa(sisu, laoseis);
        Label koguseSilt = Vaade.Tekstirida("Kogus");
        koguseSilt.Margin = new Padding(0, 4, 0, 4);
        Vaade.Lisa(sisu, koguseSilt);
        Vaade.Lisa(sisu, tegevused);
        kaart.Controls.Add(sisu);
        nupp.Tag = t.Id;
        kogus.Name = "Kogus";
        nupp.Click += LisaKorvi;
        pilt.Click += (s, e) => AvaToode(t);
        nimi.Click += (s, e) => AvaToode(t);
        // Esimesel avamisel skaleerib vorm ise; hilisemal värskendamisel skaleerime uue kaardi.
        if (IsHandleCreated && DeviceDpi != 96)
        {
            kaart.Scale(new SizeF(DeviceDpi / 96F, DeviceDpi / 96F));
        }

        return kaart;
    }

    private void LisaKorvi(object? sender, EventArgs e)
    {
        Button nupp = (Button)sender!;
        int id = (int)nupp.Tag!;
        NumericUpDown kogus = (NumericUpDown)nupp.Parent!.Controls["Kogus"]!;
        try
        {
            using ToodedContext db = new ToodedContext();
            Toode? toode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == id);
            if (toode == null || !toode.Aktiivne)
            {
                MessageBox.Show("See toode ei ole enam müügil. Värskenda poodi.");
                return;
            }
            Ostukorv.Lisa(toode, (int)kogus.Value);
            NaitaKorviSummat();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message);
        }
        catch (Exception ex)
        {
            Vead.Naita("Toodet ei saanud ostukorvi lisada. Proovi uuesti.", ex);
        }
    }

    private void AvaToode(Toode t)
    {
        using Form aken = new Form();
        Vaade.Vorm(aken, t.Toodenimetus, 680, 650);
        aken.MinimumSize = new Size(500, 460);
        using Image? suurPilt = Kaustad.LaePilt(t.Pilt);
        TableLayoutPanel raam = Vaade.Raam(aken, "Toote üksikasjad", $"{t.Hind:F2} € · Laos {t.Kogus} tk");
        TableLayoutPanel sisu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sisu.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        sisu.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        sisu.Controls.Add(new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Image = suurPilt, BackColor = Color.White }, 0, 0);
        sisu.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White, Text = t.Toodenimetus + "\r\n\r\n" + t.Kirjeldus }, 0, 1);
        raam.Controls.Add(sisu, 0, 1);
        aken.ResumeLayout(true);
        aken.ShowDialog(this);
    }

    private void NaitaKorviSummat()
    {
        lblOstukorv.Text = $"Ostukorvis: {Ostukorv.Ridad.Count} toodet, {Ostukorv.KogusKokku} tk · {Ostukorv.Kokku:F2} €";
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        VabastaKaardid();
        base.OnFormClosed(e);
    }

    private void LooLiides()
    {
        Text = "Pood";
        ClientSize = new Size(1000, 700);
        tabPood.Dock = DockStyle.Fill;
        FlowLayoutPanel all = new FlowLayoutPanel();
        all.Dock = DockStyle.Bottom;
        all.Height = 60;
        lblOstukorv.Width = 650;
        lblOstukorv.Height = 35;
        Button korv = new Button();
        korv.Text = "Ava ostukorv";
        korv.Width = 200;
        korv.Height = 35;
        korv.Click += (s,e) =>
        {
            using ArveForm vorm = new ArveForm();
            vorm.ShowDialog(this);
            NaitaKorviSummat();
            LaePood();
        };
        all.Controls.Add(lblOstukorv);
        all.Controls.Add(korv);
        Controls.Add(tabPood);
        Controls.Add(all);
        Load += (s,e) => LaePood();
        NaitaKorviSummat();
    }
}
