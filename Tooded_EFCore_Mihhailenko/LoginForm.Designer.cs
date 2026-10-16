namespace Tooded_EFCore_Mihhailenko
{
    partial class LoginForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private TextBox txtEmail;
        private TextBox txtParool;
        private Button btnLogin;
        private Label lblVeateade;

        private void InitializeComponent()
        {
            this.txtEmail = new TextBox();
            this.txtParool = new TextBox();
            this.btnLogin = new Button();
            this.lblVeateade = new Label();
            this.SuspendLayout();

            // txtEmail
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Location = new Point(30, 30);
            this.txtEmail.Size = new Size(260, 23);
            this.txtEmail.PlaceholderText = "E-mail";
            this.txtEmail.TabIndex = 0;

            // txtParool
            this.txtParool.Name = "txtParool";
            this.txtParool.Location = new Point(30, 75);
            this.txtParool.Size = new Size(260, 23);
            this.txtParool.PasswordChar = '*';
            this.txtParool.PlaceholderText = "Parool";
            this.txtParool.TabIndex = 1;

            // btnLogin
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Location = new Point(30, 120);
            this.btnLogin.Size = new Size(260, 35);
            this.btnLogin.Text = "Logi sisse";
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Click += btnLogin_Click;

            // lblVeateade
            this.lblVeateade.Name = "lblVeateade";
            this.lblVeateade.Location = new Point(30, 170);
            this.lblVeateade.Size = new Size(260, 40);
            this.lblVeateade.ForeColor = Color.Red;
            this.lblVeateade.Text = "";

            // LoginForm
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(320, 230);
            this.Name = "LoginForm";
            this.Text = "Sisselogimine";
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtParool);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lblVeateade);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
