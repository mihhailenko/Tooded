using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form3 : Form
{
    private readonly FlowLayoutPanel kategooriad = new(), kaardid = new();
    private readonly BarboraSearchBox otsinguVaade = new();
    private readonly TextBox otsing;
    public Control Otsinguriba => otsinguVaade;
    private readonly Panel feedScroll = new() { AutoScroll = true };
    private readonly FlowLayoutPanel miniRead = new();
    private readonly Label miniHind = new();
    private readonly BarboraSurface mini = new();
    private readonly TableLayoutPanel paigutus = new();
    private readonly List<BarboraActionButton> navNupud = [];
    private bool avaleht = true, ainultLemmikud, ainultUued, vahetan;
    private readonly ComboBox sort = new();
    private readonly CheckBox laos = new() { Text = "Ainult laos", AutoSize = true };
    private readonly CheckBox soodus = new() { Text = "Sooduspakkumised", AutoSize = true };
    private readonly Label pealkiri = new(), olek = new(), allikas = new(), korviSumma = new(), lehekulg = new();
    private readonly BarboraActionButton eelmine = new(), jargmine = new(), varskenda = new();
    private readonly PoePaneel tervitus = new();
    private readonly System.Windows.Forms.Timer viivitus;
    private readonly CancellationTokenSource elu = new();
    private CancellationTokenSource? laadimine;
    private List<Kategooria> kategooriaLoend = [];
    private readonly HashSet<int> avatudKategooriad = [];
    private readonly ToolTip vihjed = new();
    private int? valitud;
    private int leht;
    private const int LeheSuurus = 24;
    private bool valmis;
    public event EventHandler? KorvMuutus;

    public Form3()
    {
        InitializeComponent(); otsing = otsinguVaade.Input; viivitus = new System.Windows.Forms.Timer(components!) { Interval = 280 }; LooLiides();
        Lemmikud.Muutus += LemmikudMuutus;
    }
    private async Task LaeKategooriad()
    {
        using var db = new ToodedContext();
        kategooriaLoend = await db.Kategooriatabel.AsNoTracking().OrderBy(k => k.Jarjestus).ThenBy(k => k.Kategooria_nimetus).ToListAsync(elu.Token);
        if (IsDisposed) return;
        var kasutuses = (await db.Toodetabel.AsNoTracking().Where(t => t.Aktiivne).Select(t => t.KategooriaId).Distinct().ToListAsync(elu.Token)).ToHashSet();
        if (kasutuses.Count > 0)
        {
            bool lisa;
            do { lisa = false; foreach (var k in kategooriaLoend) if (kasutuses.Contains(k.Id) && k.YlemId.HasValue && kasutuses.Add(k.YlemId.Value)) lisa = true; } while (lisa);
            kategooriaLoend = kategooriaLoend.Where(k => kasutuses.Contains(k.Id)).ToList();
        }
        KoostaKategooriad();
    }
    private void KoostaKategooriad()
    {
        kategooriad.SuspendLayout();
        while (kategooriad.Controls.Count > 0) { var c = kategooriad.Controls[0]; kategooriad.Controls.Remove(c); c.Dispose(); }
        LisaKategooria(null, "Kõik tooted", 0);
        var kasutatud = new HashSet<int>();
        void LisaPuu(Kategooria k, int tase)
        {
            if (!kasutatud.Add(k.Id)) return;
            LisaKategooria(k.Id, k.Kategooria_nimetus, tase);
            if (avatudKategooriad.Contains(k.Id))
                foreach (var laps in kategooriaLoend.Where(c => c.YlemId == k.Id)) LisaPuu(laps, tase + 1);
        }
        foreach (var k in kategooriaLoend.Where(k => k.YlemId == null)) LisaPuu(k, 0);

        kategooriad.ResumeLayout(true);
    }
    private void LisaKategooria(int? id, string nimi, int tase)
    {
        var button = new BarboraCategoryButton
        {
            Text = nimi,
            Tag = id,
            Depth = tase,
            Selected = id == valitud,
            AccessibleName = "Kategooria: " + nimi,
            Width = Math.Max(100, kategooriad.Width - SystemInformation.VerticalScrollBarWidth - 8 * DeviceDpi / 96),
            Height = (tase == 0 ? 46 : 40) * DeviceDpi / 96,
            Margin = Padding.Empty
        };
        vihjed.SetToolTip(button, nimi);
        button.Click += async (_, _) =>
        {
            if (id.HasValue && kategooriaLoend.Any(k => k.YlemId == id))
            { if (!avatudKategooriad.Add(id.Value)) avatudKategooriad.Remove(id.Value); }
            await ValiVaadeAsync(false, id);
            KoostaKategooriad();
        };
        kategooriad.Controls.Add(button);
    }
    private void ValiKategooriaVarv()
    {
        foreach (var button in kategooriad.Controls.OfType<BarboraCategoryButton>())
        { button.Selected = Equals(button.Tag, valitud); button.Invalidate(); }
    }
    private HashSet<int>? ValitudKategooriad()
    {
        if (valitud == null) return null;
        var ids = new HashSet<int> { valitud.Value }; bool muutus;
        do { muutus = false; foreach (var k in kategooriaLoend) if (k.YlemId.HasValue && ids.Contains(k.YlemId.Value) && ids.Add(k.Id)) muutus = true; } while (muutus);
        return ids;
    }
    private async Task LaeTooted()
    {
        if (!valmis || IsDisposed) return;
        laadimine?.Cancel(); var allikasCt = CancellationTokenSource.CreateLinkedTokenSource(elu.Token);
        laadimine = allikasCt; var ct = allikasCt.Token; olek.Text = "Laen tooteid…"; eelmine.Enabled = jargmine.Enabled = false;
        try
        {
            using var db = new ToodedContext(); string q = otsing.Text.Trim(); var ids = ValitudKategooriad();
            IQueryable<Toode> paring = db.Toodetabel.AsNoTracking().Where(t => t.Aktiivne);
            if (ainultLemmikud) { var lemmikud = Lemmikud.Koik(); paring = paring.Where(t => lemmikud.Contains(t.Id)); }
            if (ainultUued) paring = paring.Where(t => t.Margis == "Uus");
            if (avaleht && q.Length == 0 && !ainultLemmikud) paring = paring.Where(t => t.Tavahind > t.Hind && t.Kogus > 0);
            if (q.Length > 0) paring = paring.Where(t => t.Toodenimetus.Contains(q) || (t.Kirjeldus != null && t.Kirjeldus.Contains(q)));
            if (ids != null) paring = paring.Where(t => ids.Contains(t.KategooriaId));
            if (laos.Checked) paring = paring.Where(t => t.Kogus > 0);
            if (soodus.Checked) paring = paring.Where(t => t.Tavahind > t.Hind);
            int kokku = await paring.CountAsync(ct); leht = Math.Min(leht, Math.Max(0, (kokku - 1) / LeheSuurus));
            var jarjestatud = sort.SelectedIndex switch
            {
                1 => paring.OrderBy(t => t.Hind).ThenBy(t => t.Id),
                2 => paring.OrderByDescending(t => t.Hind).ThenBy(t => t.Id),
                3 => paring.OrderByDescending(t => t.Tavahind.HasValue).ThenBy(t => t.Toodenimetus).ThenBy(t => t.Id),
                _ => avaleht ? paring.OrderBy(t => t.KategooriaId).ThenBy(t => t.Toodenimetus).ThenBy(t => t.Id) : paring.OrderBy(t => t.Toodenimetus).ThenBy(t => t.Id)
            };
            var tooted = await jarjestatud.Skip(leht * LeheSuurus).Take(LeheSuurus).ToListAsync(ct);
            ct.ThrowIfCancellationRequested(); if (IsDisposed) return;
            kaardid.SuspendLayout(); TyhjendaKaardid();
            foreach (var t in tooted)
            {
                var kaart = new Tootekaart(t, MuudaKorvi, AvaToode);
                if (DeviceDpi != 96) kaart.Scale(new SizeF(DeviceDpi / 96f, DeviceDpi / 96f));
                kaardid.Controls.Add(kaart);
            }
            if (tooted.Count == 0)
            {
                var tyhi = Vaade.Veerg(); tyhi.Width = Math.Max(280, kaardid.ClientSize.Width - 12); tyhi.Padding = new Padding(24);
                Vaade.Lisa(tyhi, new BarboraAsset { AssetPath = "icons/ic_no_product.svg", Dock = DockStyle.Top, Height = 96 });
                Vaade.Lisa(tyhi, Vaade.Tekstirida("Sobivaid tooteid ei leitud", true));
                var selgitus = Vaade.Tekstirida("Proovi teist otsingut või eemalda filtrid. Tühja kataloogi saab haldaja poe seadetest lisada.");
                selgitus.MaximumSize = new Size(440 * DeviceDpi / 96, 0); Vaade.Lisa(tyhi, selgitus);
                var puhasta = new BarboraActionButton { Text = "Näita kõiki tooteid" }; Vaade.Nupp(puhasta);
                puhasta.Click += (_, _) => { valitud = null; ValiKategooriaVarv(); otsing.Clear(); laos.Checked = soodus.Checked = false; PlaaniOtsing(); };
                Vaade.Lisa(tyhi, puhasta); kaardid.Controls.Add(tyhi);
            }
            MahutaKaardid(); kaardid.ResumeLayout(true);
            pealkiri.Text = q.Length > 0 ? $"Otsing: {q}" : soodus.Checked ? "Sooduspakkumised" : kategooriaLoend.FirstOrDefault(k => k.Id == valitud)?.Kategooria_nimetus ?? (ainultLemmikud ? "Minu tooted" : ainultUued ? "Uudistooted" : avaleht ? "Parimad pakkumised" : "Kõik tooted");
            olek.Text = Vaade.Tooteid(kokku);
            lehekulg.Text = kokku == 0 ? "" : $"{leht * LeheSuurus + 1}–{Math.Min(kokku, (leht + 1) * LeheSuurus)} / {kokku}";
            eelmine.Enabled = leht > 0; jargmine.Enabled = (leht + 1) * LeheSuurus < kokku;
            tervitus.Visible = avaleht && q.Length == 0 && valitud == null && !ainultLemmikud && !ainultUued && leht == 0;
            feedScroll.AutoScrollPosition = Point.Empty;
        }
        catch (OperationCanceledException) { }
        catch { if (!IsDisposed && !ct.IsCancellationRequested) olek.Text = "Laadimine ei õnnestunud. Vajuta Värskenda. Eelmine vaade jäi alles."; }
        finally { if (ReferenceEquals(laadimine, allikasCt)) laadimine = null; allikasCt.Dispose(); }
    }
    private void PlaaniOtsing() { if (vahetan) return; if (otsing.TextLength > 0) avaleht = false; leht = 0; viivitus.Stop(); viivitus.Start(); }
    private void TyhjendaKaardid()
    {
        while (kaardid.Controls.Count > 0) { var c = kaardid.Controls[0]; kaardid.Controls.Remove(c); c.Dispose(); }
    }
    private void MahutaKaardid()
    {
        int gap = 12 * DeviceDpi / 96;
        int available = Math.Max(190 * DeviceDpi / 96, kaardid.ClientSize.Width - 8 * DeviceDpi / 96);
        int columns = Math.Max(1, Math.Min(5, available / (212 * DeviceDpi / 96 + gap)));
        foreach (var c in kaardid.Controls.OfType<Tootekaart>()) c.Width = available / columns - c.Margin.Horizontal;
        int rows = (int)Math.Ceiling(kaardid.Controls.Count / (double)columns);
        int height = kaardid.Controls.Count == 0 ? 120 : kaardid.Controls[0].Height + kaardid.Controls[0].Margin.Vertical;
        kaardid.Height = Math.Max(100 * DeviceDpi / 96, rows * height + 4);
    }
    private async Task MuudaKorvi(Toode t, int vahe)
    {
        try
        {
            if (Ostukorv.OstuKontrollVajalik) throw new ArgumentException("Kontrolli ostukorvis eelmise ostu tulemust.");
            if (vahe < 0)
            {
                var rida = Ostukorv.Ridad.FirstOrDefault(r => r.ToodeId == t.Id);
                if (rida != null && --rida.Kogus <= 0) Ostukorv.Ridad.Remove(rida);
            }
            else
            {
                using var db = new ToodedContext(); var toode = await db.Toodetabel.AsNoTracking().FirstOrDefaultAsync(x => x.Id == t.Id, elu.Token);
                elu.Token.ThrowIfCancellationRequested();
                if (IsDisposed) return;
                if (toode == null) throw new ArgumentException("Toode pole enam müügil. Värskenda kataloogi.");
                Ostukorv.Lisa(toode, 1);
            }
            if (!IsDisposed) { UuendaKorv(); olek.Text = vahe > 0 ? "Lisatud ostukorvi" : "Kogus muudetud"; }
        }
        catch (ArgumentException ex) { if (!IsDisposed) olek.Text = ex.Message; }
        catch (OperationCanceledException) { }
        catch { if (!IsDisposed) olek.Text = "Korvi muutmine ei õnnestunud. Proovi uuesti."; }
    }
    public void UuendaKorv()
    {
        korviSumma.Text = Ostukorv.Ridad.Count == 0 ? "Sinu ostukorv on veel tühi" : $"{Ostukorv.KogusKokku} tk sinu korvis    ·    {Ostukorv.Kokku:N2} €";
        foreach (var kaart in kaardid.Controls.OfType<Tootekaart>()) kaart.UuendaKogus(); UuendaMiniKorv(); KorvMuutus?.Invoke(this, EventArgs.Empty);
    }
    public async Task VarskendaAsync()
    {
        try { await LaeKategooriad(); await LaeTooted(); if (!IsDisposed) UuendaKorv(); }
        catch (OperationCanceledException) { }
        catch { if (!IsDisposed) olek.Text = "Kataloogi ei saanud laadida. Proovi uuesti."; }
    }
    private async Task KontrolliKataloogi(bool sunni)
    {
        varskenda.Enabled = false; allikas.Text = "Kontrollin kataloogi uuendusi…";
        try
        {
            var tulemus = await new KataloogiTeenus().UuendaAsync(sunni, elu.Token); if (IsDisposed) return;
            if (tulemus.Muutus) await VarskendaAsync();
            using var db = new ToodedContext(); var seaded = await db.KataloogiSeaded.AsNoTracking().FirstOrDefaultAsync(elu.Token);
            if (IsDisposed) return;
            string demo = seaded?.KataloogiId?.StartsWith("oppepood-demo", StringComparison.Ordinal) == true ? "Demokataloog · väljamõeldud hinnad. " : "";
            allikas.Text = demo + (seaded?.ViimaneEdu is { } aeg ? $"Uuendatud {aeg.ToLocalTime():dd.MM.yyyy HH:mm} · {tulemus.Teade}" : tulemus.Teade);
        }
        catch (OperationCanceledException) { }
        catch { if (!IsDisposed) allikas.Text = "Uuendus pole kättesaadav. Kasutusel on salvestatud kataloog."; }
        finally { if (!IsDisposed) varskenda.Enabled = true; }
    }
    public async void AvaKorv(object? s, EventArgs e)
    {
        using var vorm = new ArveForm(); vorm.ShowDialog(TopLevelControl); await VarskendaAsync();
    }
    private void AvaToode(Toode toode)
    {
        using var detail = new TooteDetailForm(toode, MuudaKorvi);
        detail.ShowDialog(TopLevelControl);
    }
    public Task AvaAvalehtAsync() => ValiVaadeAsync(true, null);
    public Task AvaKataloogAsync() => ValiVaadeAsync(false, null);
    private async Task ValiVaadeAsync(bool home, int? category, bool favorites = false, bool offers = false, bool news = false)
    {
        vahetan = true;
        avaleht = home; valitud = category; ainultLemmikud = favorites; ainultUued = news; leht = 0;
        otsing.Clear(); soodus.Checked = offers; laos.Checked = false;
        vahetan = false;
        ValiKategooriaVarv();
        for (int i = 0; i < navNupud.Count; i++)
        {
            navNupud[i].Selected = i == 0 ? home : i == 1 ? !home && !favorites && !offers && !news : i == 2 ? offers : favorites;
            navNupud[i].Invalidate();
        }
        await LaeTooted();
    }
    private void LemmikudMuutus(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        foreach (var card in kaardid.Controls.OfType<Tootekaart>()) card.UuendaKogus();
        if (ainultLemmikud) PlaaniOtsing();
    }
    private void UuendaMiniKorv()
    {
        while (miniRead.Controls.Count > 0) { var c = miniRead.Controls[0]; miniRead.Controls.Remove(c); c.Dispose(); }
        miniHind.Text = $"Kokku    {Ostukorv.Kokku:N2} €";
        if (Ostukorv.Ridad.Count == 0)
        {
            var state = new TableLayoutPanel
            {
                Width = Math.Max(180, miniRead.ClientSize.Width - 20),
                Height = 360 * DeviceDpi / 96,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty,
                BackColor = Color.White
            };
            state.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            state.RowStyles.Add(new RowStyle(SizeType.Absolute, 190 * DeviceDpi / 96));
            state.RowStyles.Add(new RowStyle(SizeType.Absolute, 62 * DeviceDpi / 96));
            state.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            state.Controls.Add(new BarboraAsset { AssetPath = "cart/cart_logged_in.png", Dock = DockStyle.Fill, Margin = new Padding(36, 20, 36, 4) }, 0, 0);
            state.Controls.Add(new Label { Text = "Sinu ostukorv on tühi", Font = BarboraTheme.Font(13, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 0, 1);
            state.Controls.Add(new Label
            {
                Text = "Lisa oma lemmiktooted korvi ja alusta ostlemist.",
                Font = BarboraTheme.Font(10),
                ForeColor = BarboraTheme.Muted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Padding = new Padding(12, 8, 12, 0)
            }, 0, 2);
            miniRead.Controls.Add(state);
        }
        else
            foreach (var row in Ostukorv.Ridad)
            {
                var view = new MiniKorviRida(row, async (id, delta) => await MuudaKorvi(new Toode { Id = id }, delta));
                if (DeviceDpi != 96) view.Scale(new SizeF(DeviceDpi / 96f, DeviceDpi / 96f));
                view.Width = Math.Max(180, miniRead.ClientSize.Width - 22 * DeviceDpi / 96);
                miniRead.Controls.Add(view);
            }
    }
    private void LooLiides()
    {
        Vaade.Vorm(this, "BARBORA", 1400, 880); MinimumSize = new Size(960, 640);
        BackColor = BarboraTheme.Background;
        paigutus.Dock = DockStyle.Fill; paigutus.ColumnCount = 3; paigutus.RowCount = 2;
        paigutus.Padding = new Padding(24, 0, 24, 20); paigutus.Margin = Padding.Empty;
        paigutus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 224));
        paigutus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        paigutus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 294));
        paigutus.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        paigutus.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty, Padding = new Padding(0, 5, 0, 8) };
        string[] labels = ["Esileht", "Tooted", "Kampaaniad", "Minu tooted"];
        string[] icons = ["bottom_bar/home.svg", "bottom_bar/products.svg", "popular_categories/ic_special_offers.svg", "icons/heart_outline.svg"];
        for (int i = 0; i < labels.Length; i++)
        {
            int index = i;
            var action = new BarboraActionButton
            {
                Text = labels[i],
                IconPath = icons[i],
                Width = i == 2 ? 154 : i == 3 ? 164 : 140,
                Height = 46,
                ShowBorder = false,
                Selected = i == 0,
                Margin = new Padding(0, 0, 10, 0)
            };
            action.Click += async (_, _) => await ValiVaadeAsync(index == 0, null, favorites: index == 3, offers: index == 2);
            navNupud.Add(action); nav.Controls.Add(action);
        }
        varskenda.Text = ""; varskenda.AccessibleName = "Uuenda kataloogi"; varskenda.Size = new Size(42, 42);
        varskenda.FlatStyle = FlatStyle.Flat; varskenda.FlatAppearance.BorderSize = 0;
        varskenda.IconPath = "icons/ic_refresh.svg"; varskenda.ShowBorder = false; varskenda.Cursor = Cursors.Hand;
        varskenda.Click += async (_, _) => { await VarskendaAsync(); await KontrolliKataloogi(true); };
        nav.Controls.Add(varskenda); paigutus.Controls.Add(nav, 0, 0); paigutus.SetColumnSpan(nav, 3);
        var sidebar = new BarboraSurface { Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 12), Margin = new Padding(0, 0, 18, 0) };
        var categoryLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
        categoryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        categoryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); categoryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        categoryLayout.Controls.Add(new Label { Text = "Tootekategooriad", Font = BarboraTheme.Font(11, FontStyle.Bold), Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) }, 0, 0);
        kategooriad.Dock = DockStyle.Fill; kategooriad.FlowDirection = FlowDirection.TopDown; kategooriad.WrapContents = false; kategooriad.AutoScroll = true;
        kategooriad.Resize += (_, _) => { foreach (Control c in kategooriad.Controls) c.Width = Math.Max(100, kategooriad.Width - SystemInformation.VerticalScrollBarWidth - 8 * DeviceDpi / 96); };
        categoryLayout.Controls.Add(kategooriad, 0, 1); sidebar.Controls.Add(categoryLayout); paigutus.Controls.Add(sidebar, 0, 1);
        feedScroll.Dock = DockStyle.Fill; feedScroll.Margin = new Padding(0, 0, 18, 0);
        var feed = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 6, Margin = Padding.Empty };
        feed.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 6; i++) feed.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tervitus.Dock = DockStyle.Top; tervitus.Height = 210; tervitus.Margin = new Padding(0, 0, 0, 20); tervitus.BackColor = BarboraTheme.Background;
        var banners = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        banners.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64)); banners.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        var campaign = new BarboraAsset { AssetPath = "bento_banners/ee/mobile/uncle.jpg", Dock = DockStyle.Fill, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 10, 0) };
        campaign.Click += async (_, _) => await ValiVaadeAsync(false, null, offers: true);
        var promo = new BarboraAsset { AssetPath = "bento_banners/ee/mobile/twin_1.jpg", Dock = DockStyle.Fill, Cursor = Cursors.Hand, Margin = Padding.Empty };
        promo.Click += async (_, _) => await ValiVaadeAsync(false, null, news: true);
        banners.Controls.Add(campaign, 0, 0); banners.Controls.Add(promo, 1, 0); tervitus.Controls.Add(banners); feed.Controls.Add(tervitus, 0, 0);
        pealkiri.Text = "Parimad pakkumised"; pealkiri.Font = BarboraTheme.Font(19); pealkiri.AutoSize = true;
        pealkiri.Dock = DockStyle.Top; pealkiri.Margin = new Padding(0, 0, 0, 14); pealkiri.UseMnemonic = false; feed.Controls.Add(pealkiri, 0, 1);
        var status = new Label { Dock = DockStyle.Top, Height = 26, AutoEllipsis = true, Font = BarboraTheme.Font(9), ForeColor = BarboraTheme.Muted, Margin = new Padding(0,0,0,8) };
        olek.TextChanged += (_, _) => status.Text = olek.Text;
        feed.Controls.Add(status,0,2);
        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, WrapContents = false, Margin = Padding.Empty };
        var offers = new BarboraActionButton { Text = "Sooduspakkumised", IconPath = "icons/ic_filter.svg", Width = 202, Height = 40, Margin = new Padding(0, 0, 10, 12) };
        var inStock = new BarboraActionButton { Text = "Ainult laos", Width = 126, Height = 40, Margin = new Padding(0, 0, 10, 12) };
        var order = new BarboraActionButton { Text = "Järjesta", IconPath = "icons/ic_sort.svg", Width = 142, Height = 40, Margin = new Padding(0, 0, 0, 12) };
        offers.Click += (_, _) => soodus.Checked = !soodus.Checked; inStock.Click += (_, _) => laos.Checked = !laos.Checked;
        filters.Controls.AddRange([offers, inStock, order]); feed.Controls.Add(filters, 0, 3);
        sort.Items.AddRange(["Nimetus A–Z", "Hind: odavamad enne", "Hind: kallimad enne", "Sooduspakkumised enne"]); sort.SelectedIndex = 0;
        var sortMenu = new ContextMenuStrip(components!) { Font = BarboraTheme.Font(10) };
        for (int i = 0; i < sort.Items.Count; i++) { int n = i; sortMenu.Items.Add(sort.Items[i]!.ToString(), null, (_, _) => sort.SelectedIndex = n); }
        order.Click += (_, _) => sortMenu.Show(order, new Point(0, order.Height));
        kaardid.Dock = DockStyle.Top; kaardid.Height = 600; kaardid.WrapContents = true; kaardid.AutoScroll = false; kaardid.Margin = Padding.Empty;
        kaardid.Resize += (_, _) => MahutaKaardid(); feed.Controls.Add(kaardid, 0, 4);
        var footer = new TableLayoutPanel { Dock = DockStyle.Top, Height = 62, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 102)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        olek.Dock = DockStyle.Fill; olek.ForeColor = BarboraTheme.Muted; olek.AutoEllipsis = true; olek.Font = BarboraTheme.Font(9); olek.TextAlign = ContentAlignment.MiddleLeft;
        lehekulg.Dock = DockStyle.Fill; lehekulg.TextAlign = ContentAlignment.MiddleCenter; lehekulg.Font = BarboraTheme.Font(9);
        eelmine.Text = "‹"; jargmine.Text = "›"; eelmine.AccessibleName = "Eelmine leht"; jargmine.AccessibleName = "Järgmine leht";
        foreach (var button in new[] { eelmine, jargmine }) { Vaade.Nupp(button); button.MinimumSize = Size.Empty; button.Dock = DockStyle.Fill; button.Padding = Padding.Empty; }
        eelmine.Click += async (_, _) => { leht--; await LaeTooted(); }; jargmine.Click += async (_, _) => { leht++; await LaeTooted(); };
        footer.Controls.Add(olek, 0, 0); footer.Controls.Add(lehekulg, 1, 0); footer.Controls.Add(eelmine, 2, 0); footer.Controls.Add(jargmine, 3, 0); feed.Controls.Add(footer, 0, 5);
        feedScroll.Controls.Add(feed); paigutus.Controls.Add(feedScroll, 1, 1);
        mini.Dock = DockStyle.Fill; mini.Padding = new Padding(18); mini.Margin = Padding.Empty;
        var basket = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty };
        basket.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        basket.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); basket.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        basket.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); basket.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); basket.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        basket.Controls.Add(new Label { Text = "Ostukorv", Font = BarboraTheme.Font(15, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 0);
        miniRead.Dock = DockStyle.Fill; miniRead.FlowDirection = FlowDirection.TopDown; miniRead.WrapContents = false; miniRead.AutoScroll = true;
        miniRead.Resize += (_, _) => { foreach (Control c in miniRead.Controls) c.Width = Math.Max(180, miniRead.ClientSize.Width - 22 * DeviceDpi / 96); };
        basket.Controls.Add(miniRead, 0, 1); miniHind.Font = BarboraTheme.Font(17, FontStyle.Bold); miniHind.Dock = DockStyle.Fill; miniHind.TextAlign = ContentAlignment.MiddleLeft; basket.Controls.Add(miniHind, 0, 2);
        var checkout = new BarboraActionButton { Text = "Vaata ostukorvi", Primary = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) }; checkout.Click += AvaKorv; basket.Controls.Add(checkout, 0, 3);
        allikas.Dock = DockStyle.Fill; allikas.Font = BarboraTheme.Font(8); allikas.ForeColor = BarboraTheme.Muted; allikas.AutoEllipsis = true; basket.Controls.Add(allikas, 0, 4);
        mini.Controls.Add(basket); paigutus.Controls.Add(mini, 2, 1); Controls.Add(paigutus);
        Resize += (_, _) => { bool show = ClientSize.Width >= 1180 * DeviceDpi / 96; mini.Visible = show; paigutus.ColumnStyles[2].Width = show ? 294 * DeviceDpi / 96 : 0; MahutaKaardid(); };
        otsing.TextChanged += (_, _) => PlaaniOtsing(); sort.SelectedIndexChanged += (_, _) => PlaaniOtsing();
        laos.CheckedChanged += (_, _) => { inStock.Selected = laos.Checked; inStock.Invalidate(); PlaaniOtsing(); };
        soodus.CheckedChanged += (_, _) => { offers.Selected = soodus.Checked; offers.Invalidate(); PlaaniOtsing(); };
        viivitus.Tick += async (_, _) => { viivitus.Stop(); await LaeTooted(); };
        Shown += async (_, _) => { valmis = true; await VarskendaAsync(); await KontrolliKataloogi(false); };
        UuendaKorv(); ResumeLayout(true);
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.F)) { otsing.Focus(); otsing.SelectAll(); return true; }
        if (keyData == Keys.F5) { varskenda.PerformClick(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    private bool suletud;
    private void VabastaPood()
    {
        if (suletud) return;
        suletud = true;
        Lemmikud.Muutus -= LemmikudMuutus;
        vihjed.Dispose();
        elu.Cancel(); laadimine?.Cancel(); viivitus.Stop(); TyhjendaKaardid();
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        VabastaPood(); base.OnFormClosed(e);
    }
}
