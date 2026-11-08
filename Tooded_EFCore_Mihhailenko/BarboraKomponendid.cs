using System.Drawing.Drawing2D;

namespace Tooded_EFCore;

internal static class BarboraJoonistus
{
    public static GraphicsPath Ymar(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
        if (d <= 0) { path.AddRectangle(r); return path; }
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure(); return path;
    }
}

internal class BarboraActionButton : Button
{
    public bool Primary { get; set; }
    public bool Selected { get; set; }
    public string? IconPath { get; set; }
    public int Radius { get; set; } = 22;
    public bool ShowBorder { get; set; } = true;
    private bool hover, pressed;
    public BarboraActionButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        Font = BarboraTheme.Font(10); Cursor = Cursors.Hand;
        Size = new Size(140, 44); MinimumSize = new Size(36, 36);
        UseVisualStyleBackColor = false; BackColor = Color.Transparent;
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.Clear(Parent?.BackColor ?? Color.White); g.SmoothingMode = SmoothingMode.AntiAlias;
        float scale = DeviceDpi / 96f;
        var r = new RectangleF(.75f, .75f, Width - 1.5f, Height - 1.5f);
        using var path = BarboraJoonistus.Ymar(r, Radius * scale);
        Color bg = !Enabled ? Color.FromArgb(237, 237, 237)
            : Primary ? (pressed ? Color.FromArgb(190, 27, 38) : hover ? Color.FromArgb(211, 32, 44) : BarboraTheme.Red)
            : Selected ? BarboraTheme.Pink : hover ? Color.FromArgb(247, 247, 247) : Color.White;
        using var brush = new SolidBrush(bg); g.FillPath(brush, path);
        if (ShowBorder && !Primary)
        { using var pen = new Pen(Selected ? BarboraTheme.Red : BarboraTheme.Border); g.DrawPath(pen, path); }
        int icon = (int)(22 * scale), gap = (int)(8 * scale);
        var textSize = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        int total = textSize.Width + (IconPath == null ? 0 : icon + (string.IsNullOrEmpty(Text) ? 0 : gap));
        int left = Math.Max((int)(8 * scale), (Width - total) / 2);
        if (IconPath != null)
        {
            BarboraTheme.DrawAsset(g, IconPath, new RectangleF(left, (Height - icon) / 2f, icon, icon));
            left += icon + (string.IsNullOrEmpty(Text) ? 0 : gap);
        }
        var textRect = new Rectangle(left, 0, Math.Max(0, Width - left - (int)(8 * scale)), Height);
        TextRenderer.DrawText(g, Text, Font, textRect, !Enabled ? Color.Gray : Primary ? Color.White : Selected ? BarboraTheme.Red : ForeColor,
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        if (Focused && ShowFocusCues)
        { using var focus = new Pen(Primary ? Color.White : BarboraTheme.Red) { DashStyle = DashStyle.Dot }; g.DrawPath(focus, path); }
    }
}

internal class BarboraSurface : Panel
{
    public int Radius { get; set; } = 16;
    public BarboraSurface() { DoubleBuffered = true; ResizeRedraw = true; BackColor = Color.White; Padding = new Padding(20); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.Clear(Parent?.BackColor ?? BackColor); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = BarboraJoonistus.Ymar(new RectangleF(.5f, .5f, Width - 1.2f, Height - 1.2f), Radius * DeviceDpi / 96f);
        using var fill = new SolidBrush(BackColor); e.Graphics.FillPath(fill, path);
        using var pen = new Pen(BarboraTheme.Border); e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class BarboraAsset : Control
{
    public string AssetPath { get; set; } = "";
    public BarboraAsset() { DoubleBuffered = true; TabStop = false; }
    protected override void OnPaint(PaintEventArgs e)
    { base.OnPaint(e); BarboraTheme.DrawAsset(e.Graphics, AssetPath, ClientRectangle); }
}

internal sealed class BarboraInput : Panel
{
    public BarboraInput(TextBox input)
    {
        DoubleBuffered = true; BackColor = Color.White;
        Height = input.Multiline ? input.Height + 22 : 48;
        Padding = new Padding(14, 11, 14, 10);
        input.BorderStyle = BorderStyle.None;
        input.Font = BarboraTheme.Font(10);
        input.Dock = DockStyle.Fill;
        Controls.Add(input);
        input.GotFocus += (_, _) => Invalidate(); input.LostFocus += (_, _) => Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = BarboraJoonistus.Ymar(new RectangleF(.75f, .75f, Width - 1.5f, Height - 1.5f), 10 * DeviceDpi / 96f);
        using var pen = new Pen(ContainsFocus ? BarboraTheme.Red : Color.FromArgb(208, 208, 208));
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class BarboraSearchBox : Panel
{
    public TextBox Input { get; } = new();
    private readonly BarboraActionButton clear = new() { Text = "", IconPath = "icons/ic_close.svg", ShowBorder = false };
    public BarboraSearchBox()
    {
        DoubleBuffered = true; BackColor = Color.White; Height = 48; Padding = new Padding(42, 12, 44, 8);
        Input.BorderStyle = BorderStyle.None; Input.Font = BarboraTheme.Font(11);
        Input.PlaceholderText = "Toodete otsing"; Input.AccessibleName = "Toodete otsing";
        Input.MaxLength = 200; Input.Dock = DockStyle.Fill; Input.BackColor = Color.FromArgb(244, 244, 244);
        Controls.Add(Input); clear.Dock = DockStyle.Right; clear.Width = 36; clear.Visible = false;
        clear.AccessibleName = "Tühjenda otsing"; clear.Click += (_, _) => { Input.Clear(); Input.Focus(); };
        Input.TextChanged += (_, _) => { clear.Visible = Input.TextLength > 0; Invalidate(); };
        Controls.Add(clear);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = BarboraJoonistus.Ymar(new RectangleF(.5f, .5f, Width - 1, Height - 1), Height / 2f);
        using var fill = new SolidBrush(Color.FromArgb(244, 244, 244)); e.Graphics.FillPath(fill, path);
        BarboraTheme.DrawAsset(e.Graphics, "barbora_ui/icons/action/search-large.svg", new RectangleF(14 * DeviceDpi / 96f, (Height - 24 * DeviceDpi / 96f) / 2, 24 * DeviceDpi / 96f, 24 * DeviceDpi / 96f));
    }
}

internal sealed class BarboraCategoryButton : Button
{
    public string? IconPath { get; set; }
    public bool Selected { get; set; }
    public int Depth { get; set; }
    private bool hover;
    public BarboraCategoryButton()
    {
        DoubleBuffered = true; FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        Font = BarboraTheme.Font(9.5f); BackColor = Color.White; Cursor = Cursors.Hand; Height = 46;
        TextAlign = ContentAlignment.MiddleLeft; UseVisualStyleBackColor = false;
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; float s = DeviceDpi / 96f;
        using var brush = new SolidBrush(Selected ? BarboraTheme.Pink : hover ? BarboraTheme.Background : Color.White);
        g.FillRectangle(brush, ClientRectangle);
        if (Selected) { using var line = new SolidBrush(BarboraTheme.Red); g.FillRectangle(line, 0, 8 * s, 3 * s, Height - 16 * s); }
        int x = (int)((12 + Depth * 12) * s);
        if (IconPath != null) { BarboraTheme.DrawAsset(g, IconPath, new RectangleF(x, (Height - 22 * s) / 2, 22 * s, 22 * s)); x += (int)(32 * s); }
        TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, Math.Max(0, Width - x - (int)(22 * s)), Height), Selected ? BarboraTheme.Red : BarboraTheme.Text,
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        if (Depth == 0 && Tag != null) BarboraTheme.DrawAsset(g, "barbora_ui/icons/navigation/chevron-right.svg", new RectangleF(Width - 20 * s, (Height - 14 * s) / 2, 14 * s, 14 * s));
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -3, -3));
    }
}
