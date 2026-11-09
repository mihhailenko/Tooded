namespace Tooded_EFCore;

internal sealed class TooteDetailForm : Form
{
    private readonly CancellationTokenSource elu = new();
    private readonly TootePilt foto = new();
    private CancellationTokenSource? pildiLaadimine;
    public TooteDetailForm(Toode toode, Func<Toode, int, Task> muuda)
    {
        Vaade.Vorm(this, "BARBORA · " + toode.Toodenimetus, 980, 700);
        MinimumSize = new Size(760, 560);
        var raam = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(28) };
        raam.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raam.RowStyles.Add(new RowStyle(SizeType.Absolute, 64)); raam.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var pais = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        pais.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); pais.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pais.Controls.Add(new BarboraAsset { AssetPath = "logo/barbora_next_app_bar_logo.svg", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 22, 22) }, 0, 0);
        var sulge = new BarboraActionButton { Text = "Tagasi toodete juurde", Dock = DockStyle.Right, Width = 228, DialogResult = DialogResult.Cancel };
        pais.Controls.Add(sulge, 1, 0); raam.Controls.Add(pais, 0, 0);
        var sisu = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White, Padding = new Padding(24) };
        sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49)); sisu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51));
        foto.Dock = DockStyle.Fill; foto.Nimetus = toode.Toodenimetus; foto.Margin = new Padding(8, 14, 32, 20);
        var gallery = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        gallery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        gallery.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); gallery.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        gallery.Controls.Add(foto, 0, 0);
        var info = new TooteLisainfo(toode.LisainfoJson);
        var images = info.Pildid(toode.PildiUrl);
        var navigation = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(8) };
        for (int i = 0; i < images.Count; i++)
        {
            string url = images[i];
            var button = new BarboraActionButton { Text = (i + 1).ToString(), Width = 40, Height = 36, AccessibleName = $"Tootepilt {i + 1}" };
            button.Click += async (_, _) => await LaePiltAsync(new Toode { Toodenimetus = toode.Toodenimetus, PildiUrl = url });
            navigation.Controls.Add(button);
        }
        if (images.Count > 1) gallery.Controls.Add(navigation, 0, 1);
        sisu.Controls.Add(gallery, 0, 0);
        var parem = Vaade.Veerg(); parem.Padding = new Padding(0, 16, 0, 0);
        var nimi = Vaade.Tekstirida(toode.Toodenimetus, true); nimi.Font = BarboraTheme.Font(19, FontStyle.Bold);
        nimi.MaximumSize = new Size(390, 0); Vaade.Lisa(parem, nimi);
        Vaade.Lisa(parem, Vaade.Tekstirida(toode.Uhik));
        if (info.Voordlushind() is { Length: > 0 } comparative) Vaade.Lisa(parem, Vaade.Tekstirida(comparative));
        var hind = Vaade.Tekstirida($"{toode.Hind:N2} €", true); hind.Font = BarboraTheme.Font(31);
        hind.ForeColor = toode.Tavahind > toode.Hind ? BarboraTheme.Red : BarboraTheme.Text;
        hind.Margin = new Padding(0, 20, 0, 2); Vaade.Lisa(parem, hind);
        if (toode.Tavahind > toode.Hind)
        {
            var vana = Vaade.Tekstirida($"{toode.Tavahind:N2} €"); vana.Font = BarboraTheme.Font(12, FontStyle.Strikeout); Vaade.Lisa(parem, vana);
        }
        var korvis = Vaade.Tekstirida("");
        var lisa = new BarboraActionButton
        {
            Text = "Ostukorvi",
            Primary = true,
            Height = 48,
            Dock = DockStyle.Top,
            Enabled = toode.Aktiivne && toode.Kogus > 0,
            Margin = new Padding(0, 8, 0, 14)
        };
        lisa.Click += async (_, _) =>
        {
            lisa.Enabled = false; await muuda(toode, 1);
            if (!IsDisposed)
            {
                int mitu = Ostukorv.Ridad.FirstOrDefault(r => r.ToodeId == toode.Id)?.Kogus ?? 0;
                korvis.Text = $"Ostukorvis {mitu} tk";
                lisa.Enabled = mitu < toode.Kogus && !Ostukorv.OstuKontrollVajalik;
            }
        };
        Vaade.Lisa(parem, lisa); Vaade.Lisa(parem, korvis);
        var saadavus = Vaade.Tekstirida(toode.Kogus > 0 ? "Toode on laos" : "Toode on hetkel otsas"); Vaade.Lisa(parem, saadavus);
        string description = toode.LisainfoJson == null || toode.KohalikMuudatus ? toode.Kirjeldus ?? "" : info.Loe("description");
        if (description.Length > 0)
        {
            Vaade.Lisa(parem, Vaade.Tekstirida("Toote kirjeldus", true));
            var kirjeldus = Vaade.Tekstirida(description); kirjeldus.MaximumSize = new Size(390, 0); Vaade.Lisa(parem, kirjeldus);
        }
        void Section(string label, string value)
        {
            Vaade.Lisa(parem, Vaade.Tekstirida(label, true));
            var body = Vaade.Tekstirida(value); body.MaximumSize = new Size(390, 0); Vaade.Lisa(parem, body);
        }
        foreach (var (label, value) in info.Koostis()) Section(label, value);
        var nutrients = info.Toitevaartus();
        if (nutrients.Count > 0)
        {
            Vaade.Lisa(parem, Vaade.Tekstirida("Toitumisalane teave (100 g/ml kohta)", true));
            foreach (var (label, value) in nutrients) Vaade.Lisa(parem, Vaade.Tekstirida(label + ": " + value));
        }
        if (info.Pakkumine() is { Length: > 0 } offer) Section("Lähtepakkumise tingimused", offer);
        sisu.Controls.Add(Vaade.Kerimine(parem), 1, 0); raam.Controls.Add(sisu, 0, 1); Controls.Add(raam);
        Shown += async (_, _) => await LaePiltAsync(toode);
        CancelButton = sulge; ResumeLayout(true);
    }
    private async Task LaePiltAsync(Toode toode)
    {
        pildiLaadimine?.Cancel();
        var request = CancellationTokenSource.CreateLinkedTokenSource(elu.Token);
        pildiLaadimine = request;
        try
        {
            var image = await PildiVahemalu.LaeAsync(toode, request.Token);
            if (IsDisposed || request.IsCancellationRequested) image?.Dispose();
            else { var old = foto.Image; foto.Image = image; old?.Dispose(); }
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(pildiLaadimine, request)) pildiLaadimine = null; request.Dispose(); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { elu.Cancel(); pildiLaadimine?.Cancel(); foto.Image?.Dispose(); foto.Image = null; }
        base.Dispose(disposing);
    }
}
