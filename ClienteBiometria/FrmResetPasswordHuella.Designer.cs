namespace ENROLLMENT_V3
{
    partial class FrmResetPasswordHuella
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblStepTitle = new System.Windows.Forms.Label();
            this.panelFingerView = new System.Windows.Forms.Panel();
            this.nFVFinger = new Neurotec.Biometrics.Gui.NFingerView();
            this.cmbEscaners = new System.Windows.Forms.ComboBox();
            this.lblScanner = new System.Windows.Forms.Label();
            this.lblFingerScan = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmPassword = new System.Windows.Forms.Label();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.lblToken = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pbxLogo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.panelMain.SuspendLayout();
            this.panelFingerView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            this.SuspendLayout();
            
            // Panel Principal
            
            this.panelMain.BackColor = System.Drawing.Color.SteelBlue;
            this.panelMain.Controls.Add(this.progressBar);
            this.panelMain.Controls.Add(this.lblProgress);
            this.panelMain.Controls.Add(this.btnCancel);
            this.panelMain.Controls.Add(this.btnBack);
            this.panelMain.Controls.Add(this.btnNext);
            this.panelMain.Controls.Add(this.lblStepTitle);
            this.panelMain.Controls.Add(this.panelFingerView);
            this.panelMain.Controls.Add(this.cmbEscaners);
            this.panelMain.Controls.Add(this.lblScanner);
            this.panelMain.Controls.Add(this.lblFingerScan);
            this.panelMain.Controls.Add(this.txtConfirmPassword);
            this.panelMain.Controls.Add(this.lblConfirmPassword);
            this.panelMain.Controls.Add(this.txtNewPassword);
            this.panelMain.Controls.Add(this.lblNewPassword);
            this.panelMain.Controls.Add(this.txtToken);
            this.panelMain.Controls.Add(this.lblToken);
            this.panelMain.Controls.Add(this.txtUsuario);
            this.panelMain.Controls.Add(this.lblUsername);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Location = new System.Drawing.Point(76, 130);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(827, 450);
            this.panelMain.TabIndex = 32;
            
            // Barra de progreso
            
            this.progressBar.Location = new System.Drawing.Point(33, 380);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(464, 15);
            this.progressBar.TabIndex = 24;
            this.progressBar.Value = 25;
            
            // lblProgress
            
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgress.ForeColor = System.Drawing.Color.White;
            this.lblProgress.Location = new System.Drawing.Point(30, 360);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(66, 15);
            this.lblProgress.TabIndex = 23;
            this.lblProgress.Text = "Paso 1 de 4";

            // Botón Cancelar

            this.btnCancel.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(364, 405);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(132, 40);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // Botón para Regresar

            this.btnBack.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBack.Location = new System.Drawing.Point(200, 405);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(132, 40);
            this.btnBack.TabIndex = 7;
            this.btnBack.Text = "Atrás";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // Botón para Continuar

            this.btnNext.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNext.Location = new System.Drawing.Point(33, 405);
            this.btnNext.Margin = new System.Windows.Forms.Padding(4);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(132, 40);
            this.btnNext.TabIndex = 6;
            this.btnNext.Text = "Continuar";
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
            this.lblStepTitle.Text = "Paso 1: Ingrese su información";
            
            // panel de huellas
            
            this.panelFingerView.BackColor = System.Drawing.Color.White;
            this.panelFingerView.Controls.Add(this.nFVFinger);
            this.panelFingerView.Location = new System.Drawing.Point(515, 120);
            this.panelFingerView.Margin = new System.Windows.Forms.Padding(4);
            this.panelFingerView.Name = "panelFingerView";
            this.panelFingerView.Size = new System.Drawing.Size(280, 200);
            this.panelFingerView.TabIndex = 21;
            this.panelFingerView.Visible = true;
            
            // nFVFinger
            
            this.nFVFinger.BackColor = System.Drawing.Color.White;
            this.nFVFinger.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nFVFinger.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVFinger.Location = new System.Drawing.Point(17, 17);
            this.nFVFinger.Margin = new System.Windows.Forms.Padding(4);
            this.nFVFinger.MinutiaColor = System.Drawing.Color.Red;
            this.nFVFinger.Name = "nFVFinger";
            this.nFVFinger.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVFinger.ResultImageColor = System.Drawing.Color.Green;
            this.nFVFinger.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVFinger.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVFinger.SingularPointColor = System.Drawing.Color.Red;
            this.nFVFinger.Size = new System.Drawing.Size(245, 165);
            this.nFVFinger.TabIndex = 23;
            this.nFVFinger.TreeColor = System.Drawing.Color.Crimson;
            this.nFVFinger.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVFinger.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVFinger.TreeWidth = 2D;
            
            // cmbEscaners
            
            this.cmbEscaners.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEscaners.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEscaners.FormattingEnabled = true;
            this.cmbEscaners.Location = new System.Drawing.Point(33, 330);
            this.cmbEscaners.Margin = new System.Windows.Forms.Padding(4);
            this.cmbEscaners.Name = "cmbEscaners";
            this.cmbEscaners.Size = new System.Drawing.Size(463, 21);
            this.cmbEscaners.TabIndex = 5;
            this.cmbEscaners.Visible = true;
            this.cmbEscaners.SelectedIndexChanged += new System.EventHandler(this.cmbEscaners_SelectedIndexChanged);
            
            // lblScanner
            
            this.lblScanner.AutoSize = true;
            this.lblScanner.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScanner.ForeColor = System.Drawing.Color.White;
            this.lblScanner.Location = new System.Drawing.Point(29, 305);
            this.lblScanner.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScanner.Name = "lblScanner";
            this.lblScanner.Size = new System.Drawing.Size(73, 21);
            this.lblScanner.TabIndex = 20;
            this.lblScanner.Text = "Escáner:";
            this.lblScanner.Visible = true;
            
            // lblFingerScan
            
            this.lblFingerScan.AutoSize = true;
            this.lblFingerScan.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFingerScan.ForeColor = System.Drawing.Color.White;
            this.lblFingerScan.Location = new System.Drawing.Point(29, 95);
            this.lblFingerScan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFingerScan.Name = "lblFingerScan";
            this.lblFingerScan.Size = new System.Drawing.Size(418, 21);
            this.lblFingerScan.TabIndex = 19;
            this.lblFingerScan.Text = "Usuario:";
            this.lblFingerScan.Visible = true;

            // Confirmar Contraseña

            this.txtConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConfirmPassword.Location = new System.Drawing.Point(33, 210);
            this.txtConfirmPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '*';
            this.txtConfirmPassword.Size = new System.Drawing.Size(464, 25);
            this.txtConfirmPassword.TabIndex = 4;
            this.txtConfirmPassword.Visible = false;
            this.txtConfirmPassword.Enter += new System.EventHandler(this.txtConfirmPassword_Enter);
                     
            
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfirmPassword.ForeColor = System.Drawing.Color.White;
            this.lblConfirmPassword.Location = new System.Drawing.Point(29, 185);
            this.lblConfirmPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new System.Drawing.Size(175, 21);
            this.lblConfirmPassword.TabIndex = 18;
            this.lblConfirmPassword.Text = "Confirmar contraseña:";
            this.lblConfirmPassword.Visible = false;

            // Establecer Nueva Contraseña

            this.txtNewPassword.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewPassword.Location = new System.Drawing.Point(33, 150);
            this.txtNewPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '*';
            this.txtNewPassword.Size = new System.Drawing.Size(464, 25);
            this.txtNewPassword.TabIndex = 3;
            this.txtNewPassword.Visible = false;
            this.txtNewPassword.Enter += new System.EventHandler(this.txtNewPassword_Enter);
            
            
            this.lblNewPassword.AutoSize = true;
            this.lblNewPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewPassword.ForeColor = System.Drawing.Color.White;
            this.lblNewPassword.Location = new System.Drawing.Point(29, 125);
            this.lblNewPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.Size = new System.Drawing.Size(144, 21);
            this.lblNewPassword.TabIndex = 16;
            this.lblNewPassword.Text = "Nueva contraseña:";
            this.lblNewPassword.Visible = false;
            
            // Token
            
            this.txtToken.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtToken.Location = new System.Drawing.Point(33, 120);
            this.txtToken.Margin = new System.Windows.Forms.Padding(4);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(464, 25);
            this.txtToken.TabIndex = 2;
            this.txtToken.Visible = false;
            this.txtToken.Enter += new System.EventHandler(this.txtToken_Enter);
           
            this.lblToken.AutoSize = true;
            this.lblToken.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToken.ForeColor = System.Drawing.Color.White;
            this.lblToken.Location = new System.Drawing.Point(29, 95);
            this.lblToken.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(165, 21);
            this.lblToken.TabIndex = 14;
            this.lblToken.Text = "Token de verificación:";
            this.lblToken.Visible = false;

            // Ingresar Usuario

            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(33, 120);
            this.txtUsuario.Margin = new System.Windows.Forms.Padding(4);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(464, 25);
            this.txtUsuario.TabIndex = 2;
            this.txtUsuario.TextChanged += new System.EventHandler(this.txtUsuario_TextChanged);
            this.txtUsuario.Enter += new System.EventHandler(this.txtUsuario_Enter);
           
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.ForeColor = System.Drawing.Color.White;
            this.lblUsername.Location = new System.Drawing.Point(29, 95);
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
            this.lblTitle.Size = new System.Drawing.Size(773, 35);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "RESTABLECIMIENTO DE CONTRASEÑA";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // pbxLogo
            
            this.pbxLogo.Location = new System.Drawing.Point(227, 21);
            this.pbxLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogo.Name = "pbxLogo";
            this.pbxLogo.Size = new System.Drawing.Size(421, 101);
            this.pbxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxLogo.TabIndex = 64;
            this.pbxLogo.TabStop = false;
            
            // pic_logo_dgm
            
            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            // this.pic_logo_dgm.Image = null; // Removido por complilación
            this.pic_logo_dgm.Location = new System.Drawing.Point(84, 21);
            this.pic_logo_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(135, 97);
            this.pic_logo_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_logo_dgm.TabIndex = 63;
            this.pic_logo_dgm.TabStop = false;
            
            // picb_logo
            
            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            // this.picb_logo.Image = null; //Removido por complilación
            this.picb_logo.Location = new System.Drawing.Point(656, 21);
            this.picb_logo.Margin = new System.Windows.Forms.Padding(4);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(247, 101);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 62;
            this.picb_logo.TabStop = false;
            
            // FrmResetPasswordHuella
            
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(977, 599);
            this.ControlBox = false;
            this.Controls.Add(this.pbxLogo);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            // this.Icon = null; // Resource removed for compilation
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmResetPasswordHuella";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Restablecer Contraseña";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmResetPasswordHuella_FormClosing);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.panelFingerView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.Label lblFingerScan;
        private System.Windows.Forms.Label lblScanner;
        private System.Windows.Forms.ComboBox cmbEscaners;
        private System.Windows.Forms.Panel panelFingerView;
        private Neurotec.Biometrics.Gui.NFingerView nFVFinger;
        private System.Windows.Forms.Label lblStepTitle;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
    }
}