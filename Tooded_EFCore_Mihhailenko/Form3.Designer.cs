namespace Tooded_EFCore;

partial class Form3
{
    private System.ComponentModel.IContainer components = null;
    private ComboBox comboLanguage;

    protected override void Dispose(bool disposing)
    {
        if (disposing) VabastaPood();
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        comboLanguage = new ComboBox();
        SuspendLayout();
        //
        // comboLanguage
        //
        comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        comboLanguage.FormattingEnabled = true;
        comboLanguage.Items.AddRange(new object[] { "Eesti", "English" });
        comboLanguage.Location = new Point(12, 10);
        comboLanguage.Name = "comboLanguage";
        comboLanguage.Size = new Size(108, 24);
        comboLanguage.TabIndex = 0;
        comboLanguage.SelectedIndexChanged += comboLanguage_SelectedIndexChanged;
        //
        // Form3
        //
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(comboLanguage);
        Name = "Form3";
        ResumeLayout(false);
    }
}
