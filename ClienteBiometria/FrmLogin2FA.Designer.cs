namespace ENROLLMENT_V3
{
    partial class FrmLogin2FA
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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

        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblStepTitle = new System.Windows.Forms.Label();
            this.lblCodeInfo = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pbxLogo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            this.SuspendLayout();

            // Panel Principal

            this.panelMain.BackColor = System.Drawing.Color.SteelBlue;
            this.panelMain.Controls.Add(this.btnCancel);
            this.panelMain.Controls.Add(this.btnNext);
            this.panelMain.Controls.Add(this.lblStepTitle);
            this.panelMain.Controls.Add(this.lblCodeInfo);
            this.panelMain.Controls.Add(this.txtCode);
            this.panelMain.Controls.Add(this.lblCode);
            this.panelMain.Controls.Add(this.txtPassword);
            this.panelMain.Controls.Add(this.lblPassword);
            this.panelMain.Controls.Add(this.txtUsername);
            this.panelMain.Controls.Add(this.lblUsername);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Location = new System.Drawing.Point(75, 130);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(500, 350);
            this.panelMain.TabIndex = 32;

            // Botón Cancelar

            this.btnCancel.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(280, 295);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(132, 40);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Botón Siguiente/Verificar

            this.btnNext.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNext.Location = new System.Drawing.Point(100, 295);
            this.btnNext.Margin = new System.Windows.Forms.Padding(4);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(132, 40);
            this.btnNext.TabIndex = 6;
            this.btnNext.Text = "Siguiente";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);

            // lblStepTitle

            this.lblStepTitle.AutoSize = true;
            this.lblStepTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStepTitle.ForeColor = System.Drawing.Color.White;
            this.lblStepTitle.Location = new System.Drawing.Point(28, 50);
            this.lblStepTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStepTitle.Name = "lblStepTitle";
            this.lblStepTitle.Size = new System.Drawing.Size(257, 25);
            this.lblStepTitle.TabIndex = 22;
            this.lblStepTitle.Text = "Paso 1: Credenciales de usuario";

            // lblCodeInfo

            this.lblCodeInfo.AutoSize = true;
            this.lblCodeInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCodeInfo.ForeColor = System.Drawing.Color.LightYellow;
            this.lblCodeInfo.Location = new System.Drawing.Point(30, 250);
            this.lblCodeInfo.Name = "lblCodeInfo";
            this.lblCodeInfo.Size = new System.Drawing.Size(0, 19);
            this.lblCodeInfo.TabIndex = 21;
            this.lblCodeInfo.Visible = false;

            // Código de verificación

            this.txtCode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCode.Location = new System.Drawing.Point(30, 215);
            this.txtCode.Margin = new System.Windows.Forms.Padding(4);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(430, 29);
            this.txtCode.TabIndex = 5;
            this.txtCode.Visible = false;
            this.txtCode.TextChanged += new System.EventHandler(this.txtCode_TextChanged);
            this.txtCode.Enter += new System.EventHandler(this.txtCode_Enter);

            // lblCode

            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCode.ForeColor = System.Drawing.Color.White;
            this.lblCode.Location = new System.Drawing.Point(27, 190);
            this.lblCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(165, 21);
            this.lblCode.TabIndex = 20;
            this.lblCode.Text = "Código de verificación:";
            this.lblCode.Visible = false;

            // Contraseña

            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(30, 155);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(430, 29);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            this.txtPassword.Enter += new System.EventHandler(this.txtPassword_Enter);

            // lblPassword

            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.ForeColor = System.Drawing.Color.White;
            this.lblPassword.Location = new System.Drawing.Point(27, 130);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(96, 21);
            this.lblPassword.TabIndex = 18;
            this.lblPassword.Text = "Contraseña:";

            // Usuario

            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsername.Location = new System.Drawing.Point(30, 95);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(430, 29);
            this.txtUsername.TabIndex = 1;
            this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
            this.txtUsername.Enter += new System.EventHandler(this.txtUsername_Enter);

            // lblUsername

            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.ForeColor = System.Drawing.Color.White;
            this.lblUsername.Location = new System.Drawing.Point(27, 70);
            this.lblUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(73, 21);
            this.lblUsername.TabIndex = 12;
            this.lblUsername.Text = "Usuario:";

            // Titulo

            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(27, 10);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(446, 35);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "AUTENTICACIÓN DE DOS FACTORES";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // pbxLogo

            this.pbxLogo.Location = new System.Drawing.Point(200, 21);
            this.pbxLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogo.Name = "pbxLogo";
            this.pbxLogo.Size = new System.Drawing.Size(250, 60);
            this.pbxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxLogo.TabIndex = 64;
            this.pbxLogo.TabStop = false;

            // pic_logo_dgm

            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            this.pic_logo_dgm.Location = new System.Drawing.Point(75, 21);
            this.pic_logo_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(90, 60);
            this.pic_logo_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_logo_dgm.TabIndex = 63;
            this.pic_logo_dgm.TabStop = false;

            // picb_logo

            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Location = new System.Drawing.Point(485, 21);
            this.picb_logo.Margin = new System.Windows.Forms.Padding(4);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(90, 60);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 62;
            this.picb_logo.TabStop = false;

            // FrmLogin2FA

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(650, 500);
            this.ControlBox = false;
            this.Controls.Add(this.pbxLogo);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin2FA";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autenticación 2FA";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblCodeInfo;
        private System.Windows.Forms.Label lblStepTitle;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
    }
}