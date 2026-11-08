using System.Drawing.Drawing2D;

namespace Tooded_EFCore;

internal sealed class KorviNupp : BarboraActionButton
{
    public bool Pohiline { get => Primary; set => Primary = value; }
    public string? Ikoon { get => IconPath; set => IconPath = value; }
    public bool Ring { get => Radius > 100; set => Radius = value ? 999 : 23; }
    public KorviNupp()
    {
        MinimumSize = Size.Empty;
        Size = new Size(160, 46);
        Font = BarboraTheme.SemiBold(10);
        ForeColor = BarboraTheme.Text;
    }
}

internal sealed class KorviTooteRida : Control
{
    private readonly string nimi;
    private readonly int algneKogus;
    private readonly decimal hind;
    private readonly decimal? tavahind;
    private readonly string uhik;
    private readonly bool loetav;
    private readonly Toode? toode;
    private readonly Func<int, Task<bool>> seaKogus;
    private readonly KorviNupp miinus;
    private readonly KorviNupp pluss;
    private readonly KorviNupp eemalda;
    private readonly TextBox kogus;
    private readonly CancellationTokenSource elu = new();
    private Image? pilt;
    private bool piltAlustatud;
    private bool muutmine;

    public OstukorviRida? KorviRida { get; }
    public int Kogus => algneKogus;
    public int? SisestatudKogus => int.TryParse(kogus.Text, out int value) && value > 0 ? value : null;

    public KorviTooteRida(string nimi, int kogus, decimal hind, decimal? tavahind, string uhik,
        Toode? toode, OstukorviRida? korviRida, bool loetav, Func<int, Task<bool>> seaKogus, Action eemalda)
    {
        this.nimi = nimi;
        algneKogus = kogus;
        this.hind = hind;
        this.tavahind = tavahind;
        this.uhik = uhik;
        this.toode = toode;
        KorviRida = korviRida;
        this.loetav = loetav;
        this.seaKogus = seaKogus;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
        Height = 154;
        Margin = Padding.Empty;
        Font = BarboraTheme.Font(10);
        AccessibleName = $"{nimi}, {kogus} tk, {hind * kogus:N2} eurot";

        miinus = new KorviNupp { Ring = true, Ikoon = "icons/minus%20light.svg", AccessibleName = "Vähenda kogust: " + nimi, TabIndex = 0 };
        pluss = new KorviNupp { Ring = true, Ikoon = "icons/plus%20light.svg", AccessibleName = "Suurenda kogust: " + nimi, TabIndex = 2 };
        this.eemalda = new KorviNupp { Ring = true, Ikoon = "icons/ic_trash_bin.svg", AccessibleName = "Eemalda: " + nimi, TabIndex = 3 };
        this.kogus = new TextBox
        {
            Text = kogus.ToString(),
            TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.None,
            Font = BarboraTheme.Font(11, FontStyle.Bold),
            MaxLength = 9,
            AccessibleName = "Kogus: " + nimi,
            TabIndex = 1
        };
        Controls.AddRange([miinus, this.kogus, pluss, this.eemalda]);
        foreach (Control control in Controls) control.Visible = !loetav;
        if (toode != null) pluss.Enabled = toode.Aktiivne && kogus < toode.Kogus;
        miinus.Click += async (_, _) => await Commit(algneKogus - 1);
        pluss.Click += async (_, _) => await Commit(algneKogus + 1);
        this.eemalda.Click += (_, _) => eemalda();
        this.kogus.KeyPress += (_, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
        this.kogus.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await CommitText(); }
            if (e.KeyCode == Keys.Escape) { this.kogus.Text = algneKogus.ToString(); e.SuppressKeyPress = true; }
        };
        this.kogus.Leave += async (_, _) => await CommitText();
        HandleCreated += async (_, _) =>
        {
            if (piltAlustatud || toode == null) return;
            piltAlustatud = true;
            try
            {
                var image = await PildiVahemalu.LaeAsync(toode, elu.Token);
                if (IsDisposed || elu.IsCancellationRequested) image?.Dispose();
                else { pilt = image; Invalidate(); }
            }
            catch (OperationCanceledException) { }
        };
    }

    private async Task CommitText()
    {
        if (loetav || muutmine || IsDisposed) return;
        if (SisestatudKogus == null)
        {
            kogus.Text = algneKogus.ToString();
            return;
        }
        if (SisestatudKogus.Value != algneKogus) await Commit(SisestatudKogus.Value);
    }

    private async Task Commit(int value)
    {
        if (loetav || muutmine) return;
        muutmine = true;
        Enabled = false;
        try { await seaKogus(value); }
        finally { if (!IsDisposed) { Enabled = true; kogus.Text = algneKogus.ToString(); muutmine = false; } }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (miinus == null) return;
        float s = DeviceDpi / 96f;
        bool kitsas = Width < 710 * s;
        Height = (int)((kitsas ? 176 : 154) * s);
        int left = (int)((kitsas ? 126 : Width / s - 286) * s);
        int top = (int)((kitsas ? 119 : 61) * s);
        miinus.SetBounds(left, top, (int)(34 * s), (int)(34 * s));
        kogus.SetBounds(left + (int)(39 * s), top + (int)(7 * s), (int)(42 * s), (int)(25 * s));
        pluss.SetBounds(left + (int)(86 * s), top, (int)(34 * s), (int)(34 * s));
        eemalda.SetBounds(Width - (int)(45 * s), (int)(93 * s), (int)(30 * s), (int)(30 * s));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        float s = DeviceDpi / 96f;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool kitsas = Width < 710 * s;
        var imageRect = new RectangleF(16 * s, 21 * s, 94 * s, 94 * s);
        if (pilt != null)
        {
            float scale = Math.Min(imageRect.Width / pilt.Width, imageRect.Height / pilt.Height);
            g.DrawImage(pilt, imageRect.X + (imageRect.Width - pilt.Width * scale) / 2, imageRect.Y + (imageRect.Height - pilt.Height * scale) / 2, pilt.Width * scale, pilt.Height * scale);
        }
        else BarboraTheme.DrawAsset(g, "order_summary/image_not_found.svg", imageRect);
        int textLeft = (int)(126 * s);
        int titleRight = kitsas ? Width - (int)(20 * s) : Width - (int)(312 * s);
        using var titleFont = BarboraTheme.Font(10.5f, FontStyle.Bold);
        using var secondaryFont = BarboraTheme.Font(9);
        using var priceFont = BarboraTheme.Font(14, FontStyle.Bold);
        using var subtotalFont = BarboraTheme.Font(14, FontStyle.Bold);
        TextRenderer.DrawText(g, nimi, titleFont, new Rectangle(textLeft, (int)(20 * s), Math.Max(60, titleRight - textLeft), (int)(45 * s)), BarboraTheme.Text,
            TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        bool puuduv = !loetav && toode != null && (!toode.Aktiivne || toode.Kogus < algneKogus);
        TextRenderer.DrawText(g, puuduv ? "Pole soovitud koguses saadaval" : uhik, secondaryFont,
            new Rectangle(textLeft, (int)(66 * s), Math.Max((int)(160 * s), titleRight - textLeft), (int)(20 * s)),
            puuduv ? BarboraTheme.Red : BarboraTheme.Muted, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        string price = $"{hind:N2} €";
        TextRenderer.DrawText(g, price, priceFont, new Rectangle(textLeft, (int)(90 * s), (int)(110 * s), (int)(29 * s)), tavahind.HasValue ? BarboraTheme.Red : BarboraTheme.Text,
            TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        if (tavahind.HasValue)
        {
            using var oldFont = BarboraTheme.Font(9, FontStyle.Strikeout);
            TextRenderer.DrawText(g, $"{tavahind.Value:N2} €", oldFont, new Rectangle(textLeft + (int)(100 * s), (int)(98 * s), (int)(95 * s), (int)(22 * s)), BarboraTheme.Muted);
        }
        int sumTop = (int)((kitsas ? 124 : 57) * s);
        TextRenderer.DrawText(g, $"{hind * algneKogus:N2} €", subtotalFont,
            new Rectangle(Width - (int)(157 * s), sumTop, (int)(112 * s), (int)(30 * s)), BarboraTheme.Text,
            TextFormatFlags.Right | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        if (loetav)
            TextRenderer.DrawText(g, algneKogus + " tk", titleFont,
                new Rectangle(Width - (int)(278 * s), sumTop, (int)(100 * s), (int)(30 * s)), BarboraTheme.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine);
        using var border = new Pen(BarboraTheme.Border);
        g.DrawLine(border, 16 * s, Height - 1, Width - 16 * s, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { elu.Cancel(); pilt?.Dispose(); }
        base.Dispose(disposing);
    }
}

internal sealed class KorviTyhiVaade : Control
{
    public bool Kinnitatud { get; set; }
    private readonly KorviNupp? alusta;
    public KorviTyhiVaade(Action? ostle = null)
    {
        DoubleBuffered = true; BackColor = Color.White; Height = 412;
        if (ostle != null)
        {
            alusta = new KorviNupp { Text = "Alusta ostlemist", Pohiline = true, AccessibleName = "Alusta ostlemist" };
            alusta.Click += (_, _) => ostle(); Controls.Add(alusta);
        }
    }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (alusta == null) return;
        float s = DeviceDpi / 96f;
        alusta.SetBounds((Width - (int)(210 * s)) / 2, (int)(346 * s), (int)(210 * s), (int)(46 * s));
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        float s = DeviceDpi / 96f;
        string asset = Kinnitatud ? "order_summary/order_success.svg" : "cart/cart_logged_in.png";
        BarboraTheme.DrawAsset(e.Graphics, asset, new RectangleF((Width - 190 * s) / 2, 28 * s, 190 * s, 180 * s));
        using var title = BarboraTheme.Font(17, FontStyle.Bold);
        using var text = BarboraTheme.Font(10);
        TextRenderer.DrawText(e.Graphics, Kinnitatud ? "Tellimus on kinnitatud" : "Sinu ostukorv on tühi", title,
            new Rectangle(12, (int)(230 * s), Width - 24, (int)(40 * s)), BarboraTheme.Text, TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(e.Graphics, Kinnitatud ? "Aitäh tellimuse eest! Arve saad salvestada või saata e-postiga." : "Leia kõik vajalikud tooted otsingu või tootekategooriate abil.", text,
            new Rectangle(30, (int)(282 * s), Width - 60, (int)(52 * s)), BarboraTheme.Muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
    }
}
