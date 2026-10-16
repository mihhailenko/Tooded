using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Tooded_EFCore_Mihhailenko
{
    public partial class Form3 : Form
    {
        private readonly ToodedContext _db;

        public Form3()
        {
            InitializeComponent();
            _db = new ToodedContext();
            LaeKategooriadJaPildid();
            UuendaOstukorv();
        }

        private void LaeKategooriadJaPildid()
        {
            tabPood.TabPages.Clear();

            var kategooriad = _db.Kategooriatabel.ToList();

            foreach (var kat in kategooriad)
            {
                // Loo uus tab selle kategooria jaoks
                var page = new TabPage(kat.Kategooria_nimetus);
                page.AutoScroll = true;

                // Too kategooria tooted andmebaasist
                var tooted = _db.Toodetabel
                                .Where(t => t.KategooriaId == kat.Id)
                                .ToList();

                int x = 10;
                int y = 10;
                int count = 0;

                foreach (var t in tooted)
                {
                    // Loo paneel iga toote jaoks
                    Panel p = new Panel();
                    p.Width = 150;
                    p.Height = 240;
                    p.BorderStyle = BorderStyle.FixedSingle;

                    // Pildikast
                    PictureBox pb = new PictureBox();
                    pb.Width = 140;
                    pb.Height = 140;
                    pb.SizeMode = PictureBoxSizeMode.Zoom;
                    pb.Location = new Point(5, 5);

                    // lae pilt failist, kui olemas
                    var piltPath = Path.Combine("Images", t.Pilt ?? "puudub.png");
                    if (File.Exists(piltPath))
                        pb.Image = Image.FromFile(piltPath);

                    // Nimetus
                    Label lbl = new Label();
                    lbl.Text = t.Toodenimetus;
                    lbl.AutoSize = false;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Location = new Point(5, 150);
                    lbl.Height = 35;
                    lbl.Width = 140;

                    Label lblHind = new Label();
                    lblHind.Text = $"Hind: {t.Hind:F2} €";
                    lblHind.Location = new Point(5, 185);
                    lblHind.Size = new Size(140, 20);
                    lblHind.TextAlign = ContentAlignment.MiddleCenter;

                    ComboBox comboKogus = new ComboBox();
                    comboKogus.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboKogus.Location = new Point(5, 210);
                    comboKogus.Size = new Size(45, 25);
                    for (int i = 1; i <= 10; i++)
                    {
                        comboKogus.Items.Add(i);
                    }
                    comboKogus.SelectedIndex = 0;
                    p.Tag = comboKogus;

                    Button btnLisaKorvi = new Button();
                    btnLisaKorvi.Text = "Lisa ostukorvi";
                    btnLisaKorvi.Location = new Point(55, 210);
                    btnLisaKorvi.Size = new Size(90, 25);
                    btnLisaKorvi.Tag = t;
                    btnLisaKorvi.Click += btnLisaKorvi_Click;

                    // Lisa elemendid paneelile
                    p.Controls.Add(pb);
                    p.Controls.Add(lbl);
                    p.Controls.Add(lblHind);
                    p.Controls.Add(comboKogus);
                    p.Controls.Add(btnLisaKorvi);

                    // Paiguta paneel TabPage-s
                    p.Location = new Point(x, y);
                    page.Controls.Add(p);

                    // Järgmise positsioon
                    x += 160;
                    count++;

                    // iga 4 pildi järel uus rida
                    if (count % 4 == 0)
                    {
                        x = 10;
                        y += 250;
                    }
                }

                // Lisa TabPage TabControli sisse
                tabPood.TabPages.Add(page);
            }
        }

        private void btnLisaKorvi_Click(object sender, EventArgs e)
        {
            Button nupp = (Button)sender;
            Toode toode = (Toode)nupp.Tag;

            ComboBox comboKogus = (ComboBox)nupp.Parent.Tag;
            int kogus = (int)comboKogus.SelectedItem;

            Ostukorv.LisaToode(toode, kogus);
            UuendaOstukorv();
        }

        private void UuendaOstukorv()
        {
            int kogus = Ostukorv.Read.Sum(r => r.Kogus);
            float summa = Ostukorv.Read.Sum(r => r.Summa);
            lblOstukorv.Text = $"Ostukorvis: {kogus} tk    Kokku: {summa:F2} €";
        }

        private void btnMineOstukorvi_Click(object sender, EventArgs e)
        {
            if (Ostukorv.Read.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi. Lisa enne mõni toode.");
                return;
            }

            using ArveForm arve = new ArveForm();
            arve.ShowDialog();
            UuendaOstukorv();
        }
    }
}
