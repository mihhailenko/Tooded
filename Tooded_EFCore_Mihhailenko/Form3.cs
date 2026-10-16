using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form3 : Form
{
    private readonly ToodedContext _db = new();
    private readonly TabControl tabPood = new();
    private readonly Label lblOstukorv = new();
    private readonly Button btnMineOstukorvi = new();

    public Form3()
    {
        InitializeComponent();
        Text = "Pood";
        ClientSize = new Size(930, 630);

        tabPood.SetBounds(10, 10, 900, 550);
        lblOstukorv.SetBounds(10, 580, 650, 30);
        btnMineOstukorvi.SetBounds(710, 575, 200, 35);
        btnMineOstukorvi.Text = "Mine ostukorvi";
        btnMineOstukorvi.Click += btnMineOstukorvi_Click;
        Controls.AddRange(new Control[] { tabPood, lblOstukorv, btnMineOstukorvi });

        LaePood();
        NaitaKorviSummat();
    }

    private void LaePood()
    {
        tabPood.TabPages.Clear();

        var kategooriad = _db.Kategooriatabel
            .Include(k => k.Tooded)          // laeb kohe ka kategooria tooted
            .OrderBy(k => k.Kategooria_nimetus)
            .ToList();

        foreach (var kat in kategooriad)
        {
            var leht = new TabPage(kat.Kategooria_nimetus) { AutoScroll = true };

            int i = 0;
            foreach (var toode in kat.Tooded.OrderBy(t => t.Toodenimetus))
            {
                var kaart = LooTootekaart(toode);
                // 5 kaarti reas: veerg = i % 5, rida = i / 5
                kaart.Location = new Point(10 + (i % 5) * 175, 10 + (i / 5) * 250);
                leht.Controls.Add(kaart);
                i++;
            }

            tabPood.TabPages.Add(leht);
        }
    }

    private Panel LooTootekaart(Toode t)
    {
        var kaart = new Panel { Size = new Size(160, 235), BorderStyle = BorderStyle.FixedSingle };

        var pilt = new PictureBox
        {
            Bounds = new Rectangle(5, 5, 148, 140),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = Kaustad.LaePilt(t.Pilt)
        };

        var silt = new Label
        {
            Bounds = new Rectangle(5, 150, 148, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = $"{t.Toodenimetus}\n{t.Hind:F2} €"
        };

        var kogus = new ComboBox
        {
            Bounds = new Rectangle(5, 197, 60, 27),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        for (int i = 1; i <= Math.Min(10, Math.Max(1, t.Kogus)); i++)
            kogus.Items.Add(i);
        kogus.SelectedIndex = 0;

        var nupp = new Button
        {
            Bounds = new Rectangle(70, 195, 83, 30),
            Text = "Lisa korvi",
            Enabled = t.Kogus > 0             // laos pole = nupp hall
        };

        nupp.Click += (s, e) =>
        {
            int valitudKogus = (int)kogus.SelectedItem!;
            int korvis = Ostukorv.Ridad.Where(r => r.Toode.Id == t.Id).Sum(r => r.Kogus);
            if (korvis + valitudKogus > t.Kogus)
            {
                MessageBox.Show($"Laos on ainult {t.Kogus} tk, korvis on juba {korvis} tk.");
                return;
            }
            Ostukorv.Lisa(t, valitudKogus);
            NaitaKorviSummat();
        };

        pilt.Click += (s, e) =>
            MessageBox.Show($"{t.Toodenimetus}\nHind: {t.Hind:F2} €\nLaos: {t.Kogus} tk");

        kaart.Controls.AddRange(new Control[] { pilt, silt, kogus, nupp });
        return kaart;
    }

    private void NaitaKorviSummat()
    {
        lblOstukorv.Text = $"Ostukorvis: {Ostukorv.Ridad.Sum(r => r.Kogus)} tk    Kokku: {Ostukorv.Kokku:F2} €";
    }

    private void btnMineOstukorvi_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0)
        {
            MessageBox.Show("Ostukorv on tühi.");
            return;
        }
        using var arve = new ArveForm();
        arve.ShowDialog();
        NaitaKorviSummat();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (TabPage leht in tabPood.TabPages)
            foreach (Panel kaart in leht.Controls)
                foreach (Control element in kaart.Controls)
                    if (element is PictureBox pilt) pilt.Image?.Dispose();
        _db.Dispose();
        base.OnFormClosed(e);
    }
}
