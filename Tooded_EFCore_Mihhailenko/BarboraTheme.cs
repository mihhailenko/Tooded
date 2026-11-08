using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Tooded_EFCore;

// Original resources are loaded privately for this process; no system font installation.
internal static class BarboraTheme
{
    public static readonly Color BrandRed = Color.FromArgb(204, 0, 0);
    public static readonly Color Red = Color.FromArgb(226, 46, 57);
    public static readonly Color Text = Color.FromArgb(59, 59, 59);
    public static readonly Color Muted = Color.FromArgb(117, 117, 117);
    public static readonly Color Background = Color.FromArgb(247, 247, 247);
    public static readonly Color Border = Color.FromArgb(230, 230, 230);
    public static readonly Color Pink = Color.FromArgb(255, 239, 241);
    public static readonly Color Green = Color.FromArgb(20, 163, 44);
    public static readonly string AssetRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Barbora");
    private static readonly PrivateFontCollection Fonts = new();
    private static readonly Lazy<FontFamily> Family = new(LoadFontFamily);
    private static readonly Lazy<FontFamily> SemiboldFamily = new(() =>
    {
        _ = Family.Value;
        return Fonts.Families.FirstOrDefault(f => f.Name.Equals("Open Sans SemiBold", StringComparison.OrdinalIgnoreCase)) ?? Family.Value;
    });
    private static readonly ConcurrentDictionary<string, Image> Images = new(StringComparer.OrdinalIgnoreCase);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, EntryPoint = "AddFontResourceExW")]
    private static extern int AddFontResourceEx(string name, uint flags, IntPtr reserved);

    private static FontFamily LoadFontFamily()
    {
        foreach (string name in new[] { "OpenSans-Regular.ttf", "OpenSans-SemiBold.ttf", "OpenSans-Bold.ttf" })
        {
            string path = Path.Combine(AssetRoot, "fonts", name);
            if (!File.Exists(path)) continue;
            Fonts.AddFontFile(path);
            AddFontResourceEx(path, 0x10, IntPtr.Zero); // FR_PRIVATE: usable by GDI TextRenderer too.
        }
        return Fonts.Families.FirstOrDefault(f => f.Name.Equals("Open Sans", StringComparison.OrdinalIgnoreCase))
            ?? Fonts.Families.FirstOrDefault()
            ?? new FontFamily("Segoe UI");
    }

    // Caller owns the returned font. Draw routines should use a using statement.
    public static Font Font(float size, FontStyle style = FontStyle.Regular) => new(Family.Value, size, style, GraphicsUnit.Point);
    // For painters whose Graphics is already scaled from the 96-DPI logical canvas.
    // Pixel units prevent GDI+ from applying monitor DPI a second time.
    public static Font LogicalFont(float points, FontStyle style = FontStyle.Regular)
        => new(Family.Value, points * 96f / 72f, style, GraphicsUnit.Pixel);
    public static Font SemiBold(float size) => new(SemiboldFamily.Value, size, FontStyle.Regular, GraphicsUnit.Point);

    // Borrowed immutable image. Do not dispose an Asset result; use new Bitmap(asset) for ownership.
    public static Image? Asset(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) return null;
        string key = relativePath.Replace('\\', '/');
        if (key.Split('/').Any(part => part is ".." or ".")) return null;
        if (Images.TryGetValue(key, out var cached)) return cached;
        string local = key.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine("converted", key[..^4] + ".png") : key;
        string file = Path.Combine(AssetRoot, local.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(file)) return null;
        try
        {
            using var original = Image.FromFile(file);
            var image = new Bitmap(original);
            if (Images.TryAdd(key, image)) return image;
            image.Dispose();
            return Images[key];
        }
        catch (ArgumentException) { return null; }
        catch (IOException) { return null; }
    }

    public static void DrawAsset(Graphics graphics, string relativePath, RectangleF bounds)
    {
        var image = Asset(relativePath);
        if (image == null || bounds.Width <= 0 || bounds.Height <= 0) return;
        float scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        var destination = new RectangleF(bounds.X + (bounds.Width - image.Width * scale) / 2,
            bounds.Y + (bounds.Height - image.Height * scale) / 2, image.Width * scale, image.Height * scale);
        var oldInterpolation = graphics.InterpolationMode;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, destination);
        graphics.InterpolationMode = oldInterpolation;
    }
}
