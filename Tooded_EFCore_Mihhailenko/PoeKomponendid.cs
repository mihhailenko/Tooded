using System.Drawing.Drawing2D;

namespace Tooded_EFCore;

// Väike native-komponentide komplekt; fondid ja mõõdud skaleerib WinForms.
internal class PoePaneel : Panel
{
    public PoePaneel() { DoubleBuffered = true; ResizeRedraw = true; }
    public override Size GetPreferredSize(Size proposedSize)
    {
        if (!AutoSize || Controls.Count == 0) return base.GetPreferredSize(proposedSize);
        int width = proposedSize.Width > 0 && proposedSize.Width < 100000 ? proposedSize.Width : Width;
        int contentWidth = Math.Max(1, width - Padding.Horizontal);
        int height = Padding.Vertical;
        foreach (Control child in Controls)
            height += child.GetPreferredSize(new Size(contentWidth, 0)).Height + child.Margin.Vertical;
        return new Size(width, height);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float r = 12 * DeviceDpi / 96f;
        using var tee = new GraphicsPath();
        var ala = new RectangleF(.5f, .5f, Width - 1.5f, Height - 1.5f);
        if (ala.Width < r * 2 || ala.Height < r * 2) return;
        tee.AddArc(ala.Left, ala.Top, r * 2, r * 2, 180, 90);
        tee.AddArc(ala.Right - r * 2, ala.Top, r * 2, r * 2, 270, 90);
        tee.AddArc(ala.Right - r * 2, ala.Bottom - r * 2, r * 2, r * 2, 0, 90);
        tee.AddArc(ala.Left, ala.Bottom - r * 2, r * 2, r * 2, 90, 90);
        tee.CloseFigure();
        using var pliiats = new Pen(Vaade.Piir);
        e.Graphics.DrawPath(pliiats, tee);
    }
}

internal sealed class TootePilt : PictureBox
{
    public string Nimetus { get; set; } = "";
    public TootePilt() { SizeMode = PictureBoxSizeMode.Zoom; BackColor = Color.White; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Image == null) BarboraTheme.DrawAsset(e.Graphics, "order_summary/image_not_found.svg", new RectangleF(Width*.25f,Height*.25f,Width*.5f,Height*.5f));
    }
}
