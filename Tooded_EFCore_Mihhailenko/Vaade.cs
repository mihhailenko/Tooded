namespace Tooded_EFCore;
// Ühised värvid ja mõned tavaliste WinFormsi elementide seadistused.
internal static class Vaade
{
    public static readonly Color Taust = Color.FromArgb(244, 246, 250);
    public static readonly Color Tekst = Color.FromArgb(31, 42, 58);
    public static readonly Color Vaikne = Color.FromArgb(95, 108, 127);
    public static readonly Color Sinine = Color.FromArgb(40, 94, 190);
    public static readonly Color Punane = Color.FromArgb(174, 47, 57);
    public static readonly Color Piir = Color.FromArgb(216, 223, 233);

    public static DialogResult Teade(string tekst, string pealkiri = "Teade",
        MessageBoxButtons nupud = MessageBoxButtons.OK, MessageBoxIcon ikoon = MessageBoxIcon.None)
    {
        TaskDialogButton kinnita = new TaskDialogButton("Selge");
        TaskDialogButton loobu = new TaskDialogButton("Loobu");
        DialogResult kinnitatud = DialogResult.OK;
        DialogResult suletud = DialogResult.OK;
        if (nupud == MessageBoxButtons.YesNo)
        {
            kinnita.Text = "Jah";
            loobu.Text = "Ei";
            kinnitatud = DialogResult.Yes;
            suletud = DialogResult.No;
        }
        else if (nupud == MessageBoxButtons.RetryCancel)
        {
            kinnita.Text = "Proovi uuesti";
            kinnitatud = DialogResult.Retry;
            suletud = DialogResult.Cancel;
        }
        else if (nupud != MessageBoxButtons.OK)
        {
            throw new ArgumentOutOfRangeException(nameof(nupud));
        }

        TaskDialogPage aken = new TaskDialogPage
        {
            Caption = pealkiri,
            Text = tekst,
            AllowCancel = true,
            DefaultButton = kinnita,
            Icon = ikoon == MessageBoxIcon.Warning ? TaskDialogIcon.Warning : TaskDialogIcon.None
        };
        aken.Buttons.Add(kinnita);
        if (nupud != MessageBoxButtons.OK)
        {
            aken.Buttons.Add(loobu);
        }
        Form? omanik = Form.ActiveForm;
        TaskDialogButton vastus;
        if (omanik == null)
        {
            vastus = TaskDialog.ShowDialog(aken);
        }
        else
        {
            vastus = TaskDialog.ShowDialog(omanik, aken);
        }
        return vastus == kinnita ? kinnitatud : suletud;
    }

    public static void Vorm(Form vorm, string nimi, int laius, int korgus)
    {
        vorm.SuspendLayout();
        vorm.AutoScaleDimensions = new SizeF(96, 96);
        vorm.AutoScaleMode = AutoScaleMode.Dpi;
        vorm.Font = new Font("Segoe UI", 10F);
        vorm.Text = nimi;
        vorm.BackColor = Taust;
        vorm.ForeColor = Tekst;
        vorm.ClientSize = new Size(laius, korgus);
        vorm.StartPosition = FormStartPosition.CenterParent;
        vorm.Shown += (s, e) => MahutaEkraanile(vorm);
        vorm.DpiChanged += (s, e) => vorm.BeginInvoke(new Action(() => MahutaEkraanile(vorm)));
    }

    private static void MahutaEkraanile(Form vorm)
    {
        Rectangle ala = Screen.FromControl(vorm).WorkingArea;
        vorm.MinimumSize = new Size(Math.Min(vorm.MinimumSize.Width, ala.Width), Math.Min(vorm.MinimumSize.Height, ala.Height));
        if (vorm.Width <= ala.Width && vorm.Height <= ala.Height)
        {
            return;
        }

        vorm.Size = new Size(Math.Min(vorm.Width, ala.Width), Math.Min(vorm.Height, ala.Height));
        vorm.Location = new Point(ala.Left + (ala.Width - vorm.Width) / 2, ala.Top + (ala.Height - vorm.Height) / 2);
    }

    public static Label Tekstirida(string tekst, bool pealkiri = false)
    {
        return new Label
        {
            Text = tekst,
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", pealkiri ? 14F : 10F, pealkiri ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = pealkiri ? Tekst : Vaikne,
            Margin = new Padding(0, 0, 0, 10),
            UseMnemonic = false
        };
    }

    public static TableLayoutPanel Veerg()
    {
        TableLayoutPanel paneel = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 0,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Margin = Padding.Empty
        };
        paneel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return paneel;
    }

    public static void Lisa(TableLayoutPanel paneel, Control element)
    {
        int rida = paneel.RowCount++;
        paneel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        paneel.Controls.Add(element, 0, rida);
    }

    public static void Vali(TableLayoutPanel paneel, string silt, Control kast)
    {
        Label tekst = Tekstirida(silt);
        tekst.ForeColor = Tekst;
        tekst.Margin = new Padding(0, 5, 0, 4);
        Lisa(paneel, tekst);
        kast.Dock = DockStyle.Top;
        kast.Margin = new Padding(0, 0, 0, 10);
        kast.AccessibleName = silt;
        Lisa(paneel, kast);
    }

    public static FlowLayoutPanel Nupuriba()
    {
        return new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            WrapContents = true,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 4)
        };
    }

    public static void Nupp(Button nupp, bool pohiline = false, bool ohtlik = false)
    {
        nupp.AutoSize = true;
        nupp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        nupp.MinimumSize = new Size(100, 36);
        nupp.Padding = new Padding(12, 6, 12, 6);
        nupp.Margin = new Padding(0, 0, 8, 8);
        nupp.FlatStyle = FlatStyle.Flat;
        nupp.UseVisualStyleBackColor = false;
        nupp.BackColor = Color.White;
        nupp.ForeColor = Tekst;
        nupp.FlatAppearance.BorderColor = Piir;
        nupp.FlatAppearance.MouseOverBackColor = Taust;
        nupp.FlatAppearance.MouseDownBackColor = Piir;
        if (pohiline)
        {
            nupp.BackColor = Sinine;
            nupp.ForeColor = Color.White;
            nupp.FlatAppearance.BorderColor = Sinine;
            nupp.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 76, 158);
            nupp.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 62, 130);
        }
        if (ohtlik)
        {
            nupp.ForeColor = Punane;
            nupp.FlatAppearance.BorderColor = Punane;
        }
    }

    public static Panel Kaart(Control sisu)
    {
        Panel kaart = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 14, 14)
        };
        kaart.Controls.Add(sisu);
        return kaart;
    }

    public static Panel Kerimine(Control sisu)
    {
        Panel paneel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Margin = Padding.Empty
        };
        paneel.Controls.Add(sisu);
        return paneel;
    }

    public static TableLayoutPanel Raam(Form vorm, string pealkiri, string selgitus)
    {
        TableLayoutPanel raam = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 2
        };
        raam.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raam.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raam.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        TableLayoutPanel pais = Veerg();
        Lisa(pais, Tekstirida(pealkiri, true));
        Lisa(pais, Tekstirida(selgitus));
        raam.Controls.Add(pais, 0, 0);
        vorm.Controls.Add(raam);
        return raam;
    }

    public static void Tabel(DataGridView tabel)
    {
        tabel.BackgroundColor = Color.White;
        tabel.BorderStyle = BorderStyle.None;
        tabel.GridColor = Piir;
        tabel.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        tabel.RowHeadersVisible = false;
        tabel.EnableHeadersVisualStyles = false;
        tabel.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        tabel.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 237, 245);
        tabel.ColumnHeadersDefaultCellStyle.ForeColor = Tekst;
        tabel.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        tabel.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 237, 245);
        tabel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        tabel.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 10, 6, 10);
        tabel.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
        tabel.DefaultCellStyle.ForeColor = Tekst;
        tabel.DefaultCellStyle.BackColor = Color.White;
        tabel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 232, 253);
        tabel.DefaultCellStyle.SelectionForeColor = Tekst;
        tabel.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
        tabel.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        tabel.RowTemplate.Height = 36;
        tabel.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        tabel.AllowUserToResizeRows = false;
    }

    public static void VeeruNimi(DataGridView tabel, string nimi, string silt, int kaal = 100)
    {
        if (!tabel.Columns.Contains(nimi))
        {
            return;
        }

        DataGridViewColumn veerg = tabel.Columns[nimi];
        veerg.HeaderText = silt;
        veerg.FillWeight = kaal;
    }
}
