using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

internal sealed class MiniKorviRida : Control
{
    private readonly OstukorviRida rida;
    private readonly CancellationTokenSource elu = new();
    private Image? foto;
    public MiniKorviRida(OstukorviRida rida, Func<int, int, Task> muuda)
    {
        this.rida = rida;
        DoubleBuffered = true; Height = 126; Width = 230; BackColor = Color.White;
        Margin = new Padding(0, 0, 0, 6);
        var minus = new BarboraActionButton { IconPath = "icons/minus%20light.svg", Width = 32, Height = 32, MinimumSize = Size.Empty };
        var plus = new BarboraActionButton { IconPath = "icons/plus%20light.svg", Width = 32, Height = 32, MinimumSize = Size.Empty };
        minus.AccessibleName = "Vähenda kogust: " + rida.Nimetus;
        plus.AccessibleName = "Suurenda kogust: " + rida.Nimetus;
        minus.Enabled = plus.Enabled = !Ostukorv.OstuKontrollVajalik;
        minus.Click += async (_, _) => await muuda(rida.ToodeId, -1);
        plus.Click += async (_, _) => await muuda(rida.ToodeId, 1);
        Controls.AddRange([minus, plus]);
        void Mahuta()
        {
            float s = DeviceDpi / 96f;
            minus.SetBounds((int)(72 * s), (int)(82 * s), (int)(32 * s), (int)(32 * s));
            plus.SetBounds((int)(154 * s), (int)(82 * s), (int)(32 * s), (int)(32 * s));
        }
        Resize += (_, _) => Mahuta();
        Mahuta();
        HandleCreated += async (_, _) =>
        {
            try
            {
                using var db = new ToodedContext();
                var product = await db.Toodetabel.AsNoTracking().FirstOrDefaultAsync(t => t.Id == rida.ToodeId, elu.Token);
                if (product == null) return;
                var image = await PildiVahemalu.LaeAsync(product, elu.Token);
                if (IsDisposed || elu.IsCancellationRequested) image?.Dispose();
                else { foto = image; Invalidate(); }
            }
            catch (OperationCanceledException) { }
            catch { /* Original placeholder remains available. */ }
        };
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); float s = DeviceDpi / 96f; var g = e.Graphics;
        var imageRect = new RectangleF(0, 12 * s, 62 * s, 72 * s);
        if (foto != null)
        {
            float ratio = Math.Min(imageRect.Width / foto.Width, imageRect.Height / foto.Height);
            g.DrawImage(foto, imageRect.X + (imageRect.Width - foto.Width * ratio) / 2,
                imageRect.Y + (imageRect.Height - foto.Height * ratio) / 2, foto.Width * ratio, foto.Height * ratio);
        }
        else BarboraTheme.DrawAsset(g, "order_summary/image_not_found.svg", imageRect);
        using var nameFont = BarboraTheme.Font(9);
        using var priceFont = BarboraTheme.Font(11, FontStyle.Bold);
        TextRenderer.DrawText(g, rida.Nimetus, nameFont, new Rectangle((int)(72 * s), (int)(9 * s), Width - (int)(76 * s), (int)(44 * s)), BarboraTheme.Text,
            TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, $"{rida.Summa:N2} €", priceFont, new Rectangle((int)(72 * s), (int)(52 * s), Width - (int)(76 * s), (int)(26 * s)), BarboraTheme.Text);
        TextRenderer.DrawText(g, rida.Kogus.ToString(), nameFont, new Rectangle((int)(104 * s), (int)(82 * s), (int)(50 * s), (int)(32 * s)), BarboraTheme.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        using var pen = new Pen(BarboraTheme.Border); g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { elu.Cancel(); foto?.Dispose(); }
        base.Dispose(disposing);
    }
}
