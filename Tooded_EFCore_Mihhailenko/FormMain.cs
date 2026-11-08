using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace Tooded_EFCore;

public partial class FormMain : Form
{
    public bool LogisValja { get; private set; }
    private readonly Form3 pood;
    private readonly BarboraActionButton korv = new() { Primary = true };
    public FormMain()
    {
        InitializeComponent();
        Vaade.Vorm(this, "BARBORA · õppeprojekt", 1460, 960);
        MinimumSize = new Size(1000, 720);
        StartPosition = FormStartPosition.CenterScreen;
        pood = new Form3 { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill, Margin = Padding.Empty };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(28, 20, 28, 18),
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var logo = new BarboraAsset
        {
            AssetPath = "logo/barbora_next_app_bar_logo.svg",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 7, 28, 7),
            Cursor = Cursors.Hand,
            AccessibleName = "BARBORA – esileht"
        };
        logo.Click += async (_, _) => await pood.AvaAvalehtAsync();
        header.Controls.Add(logo, 0, 0);
        var search = pood.Otsinguriba; search.Dock = DockStyle.Fill; search.Margin = new Padding(0, 3, 24, 3);
        header.Controls.Add(search, 1, 0);
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        BarboraActionButton Action(string label, string icon, EventHandler click, int width)
        {
            var b = new BarboraActionButton
            {
                Text = label,
                IconPath = icon,
                Width = width,
                Height = 48,
                ForeColor = BarboraTheme.Text,
                ShowBorder = false,
                Margin = new Padding(0, 3, 8, 0)
            };
            b.AccessibleName = label; b.Click += click; nav.Controls.Add(b); return b;
        }
        Action("Tellimused", "icons/ic_orders_history.svg", (_, _) => Ava(new TellimusteForm()), 142);
        var account = Action("Minu konto", "bottom_bar/profile.svg", (_, _) => { }, 142);
        var accountMenu = new ContextMenuStrip(components!) { Font = BarboraTheme.Font(10) };
        accountMenu.Items.Add(new ToolStripMenuItem(AktiivneKasutaja.Nimi) { Enabled = false });
        accountMenu.Items.Add("Muuda parooli", null, (_, _) => Ava(new MuudaParooliForm()));
        accountMenu.Items.Add("Logi välja", null, LogiValja);
        account.Click += (_, _) => accountMenu.Show(account, new Point(0, account.Height));
        if (AktiivneKasutaja.OnAdmin)
        {
            var admin = Action("Haldus", "barbora_ui/icons/action/product-properties.svg", (_, _) => { }, 112);
            var menu = new ContextMenuStrip(components!) { Font = BarboraTheme.Font(10) };
            menu.Items.Add("Toodete haldus", null, (_, _) => Ava(new Form2()));
            menu.Items.Add("Kategooriate haldus", null, (_, _) => Ava(new KategooriateHaldusForm()));
            menu.Items.Add("Kasutajate haldus", null, (_, _) => Ava(new KasutajateHaldusForm()));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Veebikataloogi seaded", null, (_, _) => Ava(new KataloogiSeadedForm()));
            menu.Items.Add("E-posti seaded", null, (_, _) => Ava(new PostiSeadedForm()));
            menu.Items.Add("Kirja ja arve andmed", null, (_, _) => Ava(new DokumendiSeadedForm()));
            menu.Items.Add("Kontrolli ühendust", null, KontrolliUhendust);
            menu.Items.Add("Salvesta varukoopia", null, Varunda);
            admin.Click += (_, _) => menu.Show(admin, new Point(0, admin.Height));
        }
        korv.Width = 142; korv.Height = 48; korv.Font = BarboraTheme.Font(11, FontStyle.Bold);
        korv.Margin = new Padding(0, 3, 0, 0); korv.Click += pood.AvaKorv; nav.Controls.Add(korv);
        pood.KorvMuutus += (_, _) => UuendaKorvi();
        header.Resize += (_, _) => { bool compact = ClientSize.Width < 1180 * DeviceDpi / 96; for (int i = 0; i < nav.Controls.Count - 1; i++) { var b = (BarboraActionButton)nav.Controls[i]; b.Text = compact ? "" : i == 0 ? "Tellimused" : i == 1 ? "Minu konto" : "Haldus"; b.Width = (compact ? 48 : i == 0 ? 142 : i == 1 ? 142 : 112) * DeviceDpi / 96; b.Invalidate(); } };
        header.Controls.Add(nav, 2, 0); root.Controls.Add(header, 0, 0); root.Controls.Add(pood, 0, 1); Controls.Add(root);
        pood.Show(); UuendaKorvi();
        FormClosed += (_, _) => { pood.Close(); pood.Dispose(); };
        ResumeLayout(true);
    }
    private void UuendaKorvi() => korv.Text = $"Korv · {Ostukorv.Kokku:N2} €";
    private async void Ava(Form vorm)
    {
        using (vorm) vorm.ShowDialog(this);
        if (!IsDisposed) { UuendaKorvi(); await pood.VarskendaAsync(); }
    }
    private void LogiValja(object? sender, EventArgs e)
    {
        Ostukorv.Tyhjenda(); AktiivneKasutaja.Id = 0; AktiivneKasutaja.Nimi = ""; AktiivneKasutaja.Email = "";
        AktiivneKasutaja.Roll = ""; LogisValja = true; Close();
    }
    private async void KontrolliUhendust(object? sender, EventArgs e)
    {
        try { using var db = new ToodedContext(); Vaade.Teade(await db.Database.CanConnectAsync() ? "Ühendus toimib." : "Ühendust ei saanud luua. Proovi hiljem uuesti."); }
        catch (Exception ex) { Vead.Naita("Ühendust ei saanud kontrollida. Proovi hiljem uuesti.", ex); }
    }
    private async void Varunda(object? sender, EventArgs e)
    {
        if (!AktiivneKasutaja.OnAdmin) return;
        using var aken = new SaveFileDialog { Filter = "Andmebaasi varukoopia|*.bak", FileName = $"Poe_varukoopia_{DateTime.Now:yyyyMMdd_HHmmss}.bak", OverwritePrompt = true };
        if (aken.ShowDialog(this) != DialogResult.OK) return;
        if (File.Exists(aken.FileName)) { Vaade.Teade("Vali uus failinimi. Olemasolevat varukoopiat ei asendata."); return; }
        try
        {
            UseWaitCursor = true;
            using var db = new ToodedContext(); await db.Database.OpenConnectionAsync();
            using var kask = db.Database.GetDbConnection().CreateCommand(); kask.CommandTimeout = 120;
            string nimi = db.Database.GetDbConnection().Database.Replace("]", "]]");
            kask.CommandText = $"BACKUP DATABASE [{nimi}] TO DISK = @tee WITH COPY_ONLY, CHECKSUM";
            kask.Parameters.Add(new SqlParameter("@tee", aken.FileName)); await kask.ExecuteNonQueryAsync();
            kask.CommandText = "RESTORE VERIFYONLY FROM DISK = @tee WITH CHECKSUM"; await kask.ExecuteNonQueryAsync();
            Vaade.Teade("Varukoopia on salvestatud.\nVarunda eraldi ka tootepildid: " + Kaustad.Pildid);
        }
        catch (Exception ex) { Vead.Naita("Varukoopiat ei saanud salvestada. Vali teine kaust ja proovi uuesti.", ex); }
        finally { if (!IsDisposed) UseWaitCursor = false; }
    }
}
