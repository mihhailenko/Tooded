using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Tooded_EFCore;

/// <summary>Desktop rendering of Barbora's white image card, campaign bead and pill cart action.</summary>
internal sealed class Tootekaart : Control
{
    private readonly Toode toode;
    private readonly Func<Toode, int, Task> muuda;
    private readonly Action<Toode> ava;
    private readonly TooteTegevuseNupp lisa = new();
    private readonly TooteTegevuseNupp miinus = new();
    private readonly TooteTegevuseNupp pluss = new();
    private readonly TooteTegevuseNupp lemmik = new();
    private readonly CancellationTokenSource elu = new();
    private readonly ToolTip vihje = new();
    private Image? foto;
    private Task? pildiLaadimine;
    private bool hiir;
    private bool muutmine;
    private bool vabastatud;
    private int korvis;
    private RectangleF pildiAla;
    private RectangleF nimeAla;
    private string? tagasiside;

    public Tootekaart(Toode toode, Func<Toode, int, Task> muuda, Action<Toode> ava)
    {
        this.toode = toode;
        this.muuda = muuda;
        this.ava = ava;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Size = new Size(236, 386);
        MinimumSize = new Size(190, 340);
        Margin = new Padding(0, 0, 12, 16);
        BackColor = BarboraTheme.Background;
        ForeColor = BarboraTheme.Text;
        TabStop = true;
        AccessibleRole = AccessibleRole.Grouping;
        AccessibleDescription = "Enter avab toote üksikasjad. Tühik lisab toote ostukorvi.";

        lisa.Text = "Ostukorvi";
        lisa.Taitmine = true;
        lisa.AccessibleName = "Lisa ostukorvi: " + toode.Toodenimetus;
        lisa.Click += async (_, _) => await Muuda(1);
        miinus.Ikoon = "icons/minus%20light.svg";
        miinus.Ring = true;
        miinus.AccessibleName = "Vähenda kogust: " + toode.Toodenimetus;
        miinus.Click += async (_, _) => await Muuda(-1);
        pluss.Ikoon = "icons/plus%20light.svg";
        pluss.Ring = true;
        pluss.AccessibleName = "Suurenda kogust: " + toode.Toodenimetus;
        pluss.Click += async (_, _) => await Muuda(1);
        lemmik.Ikoon = "icons/heart_outline.svg";
        lemmik.Piirita = true;
        lemmik.AccessibleName = "Salvesta minu toodetesse: " + toode.Toodenimetus;
        lemmik.Click += (_, _) =>
        {
            try { Lemmikud.Muuda(toode.Id); UuendaLemmik(); }
            catch (Exception) { tagasiside = "Salvestamine ei õnnestunud"; Invalidate(); }
        };
        Controls.AddRange([lisa, miinus, pluss, lemmik]);
        vihje.SetToolTip(this, toode.Toodenimetus);
        vihje.SetToolTip(lemmik, "Minu tooted");
        Lemmikud.Muutus += LemmikudMuutusid;
        UuendaKogus();
        UuendaLemmik();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        if (Parent != null) BackColor = Parent.BackColor;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        pildiLaadimine ??= LaePiltAsync();
    }

    private async Task LaePiltAsync()
    {
        try
        {
            var pilt = await PildiVahemalu.LaeAsync(toode, elu.Token);
            if (IsDisposed || elu.IsCancellationRequested) pilt?.Dispose();
            else { foto?.Dispose(); foto = pilt; Invalidate(); }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { if (!IsDisposed) Invalidate(); }
    }

    private void LemmikudMuutusid(object? sender, EventArgs e)
    {
        if (!IsDisposed) UuendaLemmik();
    }

    private void UuendaLemmik()
    {
        bool valitud = Lemmikud.Sisaldab(toode.Id);
        lemmik.Ikoon = valitud ? "icons/heart_filled.svg" : "icons/heart_outline.svg";
        lemmik.AccessibleName = (valitud ? "Eemalda minu toodetest: " : "Salvesta minu toodetesse: ") + toode.Toodenimetus;
        lemmik.Invalidate();
    }

    private async Task Muuda(int vahe)
    {
        if (muutmine || Ostukorv.OstuKontrollVajalik) return;
        muutmine = true;
        UuendaKogus();
        try { await muuda(toode, vahe); }
        finally
        {
            muutmine = false;
            if (!IsDisposed) UuendaKogus();
        }
    }

    public void UuendaKogus()
    {
        korvis = Ostukorv.Ridad.FirstOrDefault(r => r.ToodeId == toode.Id)?.Kogus ?? 0;
        bool saab = toode.Aktiivne && toode.Kogus > 0;
        lisa.Visible = korvis == 0;
        lisa.Text = saab ? "Ostukorvi" : "Hetkel otsas";
        lisa.Enabled = saab && !muutmine && !Ostukorv.OstuKontrollVajalik;
        miinus.Visible = pluss.Visible = korvis > 0;
        miinus.Enabled = korvis > 0 && !muutmine && !Ostukorv.OstuKontrollVajalik;
        pluss.Enabled = saab && korvis < toode.Kogus && !muutmine && !Ostukorv.OstuKontrollVajalik;
        AccessibleName = $"{toode.Toodenimetus}, {toode.Hind:N2} eurot. "
            + (saab ? $"Ostukorvis {korvis} tükki." : "Hetkel laost otsas.");
        Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        float d = DeviceDpi / 96f;
        int inset = (int)Math.Round(12 * d);
        int bottom = Height - (int)Math.Round(54 * d);
        lisa.SetBounds(inset, bottom, Math.Max(1, Width - inset * 2), (int)Math.Round(42 * d));
        miinus.SetBounds(inset, bottom, (int)Math.Round(42 * d), (int)Math.Round(42 * d));
        pluss.SetBounds(Width - inset - (int)Math.Round(42 * d), bottom,
            (int)Math.Round(42 * d), (int)Math.Round(42 * d));
        lemmik.SetBounds(Width - (int)Math.Round(46 * d), (int)Math.Round(6 * d),
            (int)Math.Round(34 * d), (int)Math.Round(34 * d));
    }

    protected override void OnMouseEnter(EventArgs e) { hiir = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hiir = false; Cursor = Cursors.Default; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        float d = DeviceDpi / 96f;
        var punkt = new PointF(e.X / d, e.Y / d);
        Cursor = pildiAla.Contains(punkt) || nimeAla.Contains(punkt) ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
    }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        float d = DeviceDpi / 96f;
        var punkt = new PointF(e.X / d, e.Y / d);
        if (e.Button == MouseButtons.Left && (pildiAla.Contains(punkt) || nimeAla.Contains(punkt))) ava(toode);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Enter) { ava(toode); e.Handled = true; e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Space)
        {
            var tegevus = korvis > 0 ? pluss : lisa;
            if (tegevus.Enabled) tegevus.PerformClick();
            e.Handled = true; e.SuppressKeyPress = true;
        }
    }
    protected override bool IsInputKey(Keys keyData) => (keyData & Keys.KeyCode) is Keys.Enter or Keys.Space || base.IsInputKey(keyData);
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        float d = DeviceDpi / 96f;
        g.ScaleTransform(d, d);
        float w = Width / d, h = Height / d;
        using var shadow = new SolidBrush(Color.FromArgb(235, 235, 235));
        using var white = new SolidBrush(Color.White);
        using var border = new Pen(hiir ? Color.FromArgb(215, 215, 215) : Color.FromArgb(237, 237, 237));
        using var path = KaardiKuju(new RectangleF(.5f, .5f, w - 1.5f, h - 3), 7, 26);
        using var shadowPath = KaardiKuju(new RectangleF(.5f, 2, w - 1.5f, h - 3), 7, 26);
        g.FillPath(shadow, shadowPath);
        g.FillPath(white, path);
        g.DrawPath(border, path);

        pildiAla = new RectangleF(18, 37, w - 36, Math.Max(100, h - 221));
        if (foto != null) JoonistaFoto(g, foto, pildiAla);
        else BarboraTheme.DrawAsset(g, "order_summary/image_not_found.svg",
            new RectangleF(w / 2 - 31, pildiAla.Y + pildiAla.Height / 2 - 31, 62, 62));

        bool soodus = toode.Tavahind is > 0 && toode.Tavahind > toode.Hind;
        if (soodus) JoonistaKampaania(g, new PointF(12, 12), "Kampaania");
        else if (!string.IsNullOrWhiteSpace(toode.Margis))
        {
            using var badgeFont = BarboraTheme.LogicalFont(8.2f, FontStyle.Bold);
            using var badgeText = new SolidBrush(BarboraTheme.Text);
            BarboraTheme.DrawAsset(g, "badges/new_product.svg", new RectangleF(12, 12, 19, 19));
            g.DrawString(toode.Margis, badgeFont, badgeText, new RectangleF(35, 12, w - 80, 24));
        }
        if (!toode.Aktiivne || toode.Kogus == 0)
        {
            using var wash = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
            g.FillRectangle(wash, pildiAla);
        }

        nimeAla = new RectangleF(12, h - 182, w - 24, 59);
        using var nameFont = BarboraTheme.LogicalFont(10.7f);
        using var nameBrush = new SolidBrush(BarboraTheme.Text);
        using var titleFormat = new StringFormat { Trimming = StringTrimming.EllipsisWord, FormatFlags = StringFormatFlags.LineLimit };
        g.DrawString(toode.Toodenimetus, nameFont, nameBrush, nimeAla, titleFormat);
        JoonistaHind(g, new PointF(12, h - 118), soodus);
        string uhikuHind = ArvutaUhikuHind(toode) ?? toode.Uhik;
        using var unitFont = BarboraTheme.LogicalFont(8.3f);
        using var muted = new SolidBrush(BarboraTheme.Muted);
        g.DrawString(uhikuHind, unitFont, muted, new RectangleF(13, h - 80, w - 26, 20));
        if (korvis > 0)
        {
            using var qtyFont = BarboraTheme.LogicalFont(11.2f, FontStyle.Bold);
            using var qtyBrush = new SolidBrush(BarboraTheme.Text);
            using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString($"{korvis} tk", qtyFont, qtyBrush, new RectangleF(56, h - 54, w - 112, 42), center);
        }
        if (muutmine)
        {
            using var loadingFont = BarboraTheme.LogicalFont(7.5f);
            g.DrawString("Uuendan…", loadingFont, muted, new RectangleF(12, h - 138, w - 24, 16));
        }
        if (tagasiside != null)
        {
            using var errorFont = BarboraTheme.LogicalFont(7.4f);
            g.DrawString(tagasiside, errorFont, muted, new RectangleF(12, h - 138, w - 24, 16));
        }
        g.ResetTransform();
        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -3, -3), BarboraTheme.Red, BackColor);
    }

    private void JoonistaHind(Graphics g, PointF punkt, bool soodus)
    {
        using var amountFont = BarboraTheme.LogicalFont(25);
        using var centsFont = BarboraTheme.LogicalFont(10.5f);
        using var currencyFont = BarboraTheme.LogicalFont(10.5f);
        using var color = new SolidBrush(soodus ? BarboraTheme.Red : BarboraTheme.Text);
        using var format = new StringFormat(StringFormat.GenericTypographic);
        string euros = decimal.Truncate(toode.Hind).ToString(CultureInfo.CurrentCulture);
        string cents = ((int)((toode.Hind - decimal.Truncate(toode.Hind)) * 100)).ToString("D2", CultureInfo.InvariantCulture);
        g.DrawString(euros, amountFont, color, punkt, format);
        float euroWidth = g.MeasureString(euros, amountFont, PointF.Empty, format).Width;
        float x = punkt.X + euroWidth;
        g.DrawString(cents, centsFont, color, new PointF(x, punkt.Y + 3), format);
        float smallWidth = g.MeasureString(cents, centsFont, PointF.Empty, format).Width;
        g.DrawString("€", currencyFont, color, new PointF(x + smallWidth + 1, punkt.Y + 3), format);
        if (soodus)
        {
            using var oldFont = BarboraTheme.LogicalFont(8.4f, FontStyle.Strikeout);
            using var oldBrush = new SolidBrush(BarboraTheme.Muted);
            float oldX = x + smallWidth + g.MeasureString("€", currencyFont, PointF.Empty, format).Width + 9;
            g.DrawString($"{toode.Tavahind:N2} €", oldFont, oldBrush, new PointF(oldX, punkt.Y + 7), format);
        }
    }

    private static string? ArvutaUhikuHind(Toode t)
    {
        var matches = Regex.Matches(t.Toodenimetus, @"(?<![\d.,])(?<n>\d+(?:[.,]\d+)?)\s*(?<u>kg|ml|g|l|tk)(?![\p{L}])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (matches.Count == 0)
            return t.Uhik.ToLowerInvariant() is "kg" or "l" or "tk" ? $"{t.Hind:N2} €/{t.Uhik}" : null;
        var m = matches[^1];
        // A package with gross/drained weights needs an explicit model field; do not guess.
        if (matches.Count > 1 && t.Toodenimetus.Contains('/')) return null;
        if (!decimal.TryParse(m.Groups["n"].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal kogus) || kogus <= 0) return null;
        var multiplier = Regex.Match(t.Toodenimetus[..m.Index], @"(?<n>\d+)\s*[x×]\s*$", RegexOptions.IgnoreCase);
        if (multiplier.Success)
        {
            if (!decimal.TryParse(multiplier.Groups["n"].Value, out decimal count) || count <= 0) return null;
            kogus *= count;
        }
        string uhik = m.Groups["u"].Value.ToLowerInvariant();
        if (uhik == "g") { kogus /= 1000; uhik = "kg"; }
        else if (uhik == "ml") { kogus /= 1000; uhik = "l"; }
        decimal hind;
        try { hind = t.Hind / kogus; }
        catch (OverflowException) { return null; }
        return $"{hind:N2} €/{uhik}";
    }

    private static void JoonistaFoto(Graphics g, Image pilt, RectangleF ala)
    {
        float scale = Math.Min(ala.Width / pilt.Width, ala.Height / pilt.Height);
        var siht = new RectangleF(ala.X + (ala.Width - pilt.Width * scale) / 2,
            ala.Y + (ala.Height - pilt.Height * scale) / 2, pilt.Width * scale, pilt.Height * scale);
        var interpolation = g.InterpolationMode;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(pilt, siht);
        g.InterpolationMode = interpolation;
    }

    private static void JoonistaKampaania(Graphics g, PointF punkt, string text)
    {
        using var fill = new SolidBrush(BarboraTheme.Red);
        using var shape = KaardiKuju(new RectangleF(punkt.X, punkt.Y, 76, 23), 5, 5);
        g.FillPath(fill, shape);
        BarboraTheme.DrawAsset(g, "promotion_beads/tail_small.svg", new RectangleF(punkt.X + 72, punkt.Y - .5f, 15, 24));
        using var font = BarboraTheme.LogicalFont(8.1f);
        using var brush = new SolidBrush(Color.White);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(text, font, brush, new RectangleF(punkt.X, punkt.Y, 77, 23), sf);
    }

    private static GraphicsPath KaardiKuju(RectangleF r, float top, float bottom)
    {
        var path = new GraphicsPath();
        top = Math.Min(top, Math.Min(r.Width, r.Height) / 2);
        bottom = Math.Min(bottom, Math.Min(r.Width, r.Height) / 2);
        path.AddArc(r.Left, r.Top, top * 2, top * 2, 180, 90);
        path.AddArc(r.Right - top * 2, r.Top, top * 2, top * 2, 270, 90);
        path.AddArc(r.Right - bottom * 2, r.Bottom - bottom * 2, bottom * 2, bottom * 2, 0, 90);
        path.AddArc(r.Left, r.Bottom - bottom * 2, bottom * 2, bottom * 2, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !vabastatud)
        {
            vabastatud = true;
            Lemmikud.Muutus -= LemmikudMuutusid;
            elu.Cancel();
            foto?.Dispose(); foto = null;
            vihje.Dispose();
            if (pildiLaadimine == null || pildiLaadimine.IsCompleted) elu.Dispose();
            else _ = pildiLaadimine.ContinueWith(_ => elu.Dispose(), TaskScheduler.Default);
        }
        base.Dispose(disposing);
    }

    private sealed class TooteTegevuseNupp : Button
    {
        public bool Taitmine { get; set; }
        public bool Ring { get; set; }
        public bool Piirita { get; set; }
        public string? Ikoon { get; set; }
        private bool hover;
        private bool pressed;
        public TooteTegevuseNupp()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; BackColor = Color.White;
            Cursor = Cursors.Hand; UseVisualStyleBackColor = false; TabStop = true;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            float d = DeviceDpi / 96f; g.ScaleTransform(d, d);
            float w = Width / d, h = Height / d;
            var box = Ring ? new RectangleF((w - 34) / 2, (h - 34) / 2, 34, 34) : new RectangleF(.5f, .5f, w - 1, h - 1);
            Color fillColor = !Enabled ? Color.FromArgb(240, 240, 240) : Taitmine
                ? pressed ? Color.FromArgb(179, 25, 37) : hover ? Color.FromArgb(206, 32, 44) : BarboraTheme.Red
                : hover ? Color.FromArgb(247, 247, 247) : Color.White;
            if (!Piirita)
            {
                using var shape = KaardiKuju(box, box.Height / 2, box.Height / 2);
                using var fill = new SolidBrush(fillColor); g.FillPath(fill, shape);
                if (!Taitmine) { using var edge = new Pen(BarboraTheme.Border); g.DrawPath(edge, shape); }
            }
            if (Ikoon != null)
            {
                var asset = BarboraTheme.Asset(Ikoon);
                if (asset != null)
                {
                    if (Enabled) JoonistaFoto(g, asset, new RectangleF((w - 24) / 2, (h - 24) / 2, 24, 24));
                    else
                    {
                        using var attributes = new System.Drawing.Imaging.ImageAttributes();
                        attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = .35f });
                        g.DrawImage(asset, new Rectangle((int)((w - 24) / 2), (int)((h - 24) / 2), 24, 24),
                            0, 0, asset.Width, asset.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
            else
            {
                using var font = BarboraTheme.LogicalFont(10.8f);
                using var brush = new SolidBrush(!Enabled ? Color.FromArgb(142, 142, 142) : Taitmine ? Color.White : BarboraTheme.Text);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Text, font, brush, box, sf);
            }
            g.ResetTransform();
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -4, -4));
        }
    }
}
