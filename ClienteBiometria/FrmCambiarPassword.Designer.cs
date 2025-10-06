namespace ENROLLMENT_V3
{
    partial class FrmCambiarPassword
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
            this.lblValidacion = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnCambiar = new System.Windows.Forms.Button();
            this.txtConfirmarPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmarPassword = new System.Windows.Forms.Label();
            this.txtNuevaPassword = new System.Windows.Forms.TextBox();
            this.lblNuevaPassword = new System.Windows.Forms.Label();
            this.lblUsuario = new System.Windows.Forms.Label();
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
            this.panelMain.Controls.Add(this.lblValidacion);
            this.panelMain.Controls.Add(this.btnCancelar);
            this.panelMain.Controls.Add(this.btnCambiar);
            this.panelMain.Controls.Add(this.txtConfirmarPassword);
            this.panelMain.Controls.Add(this.lblConfirmarPassword);
            this.panelMain.Controls.Add(this.txtNuevaPassword);
            this.panelMain.Controls.Add(this.lblNuevaPassword);
            this.panelMain.Controls.Add(this.lblUsuario);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Location = new System.Drawing.Point(25, 130);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(450, 300);
            this.panelMain.TabIndex = 32;

            // lblValidacion

            this.lblValidacion.AutoSize = true;
            this.lblValidacion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValidacion.ForeColor = System.Drawing.Color.Orange;
            this.lblValidacion.Location = new System.Drawing.Point(30, 210);
            this.lblValidacion.Name = "lblValidacion";
            this.lblValidacion.Size = new System.Drawing.Size(0, 19);
            this.lblValidacion.TabIndex = 24;

            // Botón Cancelar

            this.btnCancelar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancelar.Location = new System.Drawing.Point(240, 245);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(132, 40);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // Botón Cambiar

            this.btnCambiar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCambiar.FlatAppearance.BorderSize = 0;
            this.btnCambiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnCambiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCambiar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCambiar.ForeColor = System.Drawing.Color.White;
            this.btnCambiar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCambiar.Location = new System.Drawing.Point(80, 245);
            this.btnCambiar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCambiar.Name = "btnCambiar";
            this.btnCambiar.Size = new System.Drawing.Size(132, 40);
            this.btnCambiar.TabIndex = 3;
            this.btnCambiar.Text = "Cambiar";
            this.btnCambiar.UseVisualStyleBackColor = false;
            this.btnCambiar.Click += new System.EventHandler(this.btnCambiar_Click);

            // Confirmar Contraseña

            this.txtConfirmarPassword.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConfirmarPassword.Location = new System.Drawing.Point(30, 175);
            this.txtConfirmarPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtConfirmarPassword.Name = "txtConfirmarPassword";
            this.txtConfirmarPassword.PasswordChar = '*';
            this.txtConfirmarPassword.Size = new System.Drawing.Size(390, 25);
            this.txtConfirmarPassword.TabIndex = 2;
            this.txtConfirmarPassword.TextChanged += new System.EventHandler(this.txtConfirmarPassword_TextChanged);

            // lblConfirmarPassword

            this.lblConfirmarPassword.AutoSize = true;
            this.lblConfirmarPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfirmarPassword.ForeColor = System.Drawing.Color.White;
            this.lblConfirmarPassword.Location = new System.Drawing.Point(27, 150);
            this.lblConfirmarPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConfirmarPassword.Name = "lblConfirmarPassword";
            this.lblConfirmarPassword.Size = new System.Drawing.Size(175, 21);
            this.lblConfirmarPassword.TabIndex = 18;
            this.lblConfirmarPassword.Text = "Confirmar contraseña:";

            // Nueva Contraseña

            this.txtNuevaPassword.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNuevaPassword.Location = new System.Drawing.Point(30, 115);
            this.txtNuevaPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtNuevaPassword.Name = "txtNuevaPassword";
            this.txtNuevaPassword.PasswordChar = '*';
            this.txtNuevaPassword.Size = new System.Drawing.Size(390, 25);
            this.txtNuevaPassword.TabIndex = 1;
            this.txtNuevaPassword.TextChanged += new System.EventHandler(this.txtNuevaPassword_TextChanged);

            // lblNuevaPassword

            this.lblNuevaPassword.AutoSize = true;
            this.lblNuevaPassword.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNuevaPassword.ForeColor = System.Drawing.Color.White;
            this.lblNuevaPassword.Location = new System.Drawing.Point(27, 90);
            this.lblNuevaPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNuevaPassword.Name = "lblNuevaPassword";
            this.lblNuevaPassword.Size = new System.Drawing.Size(144, 21);
            this.lblNuevaPassword.TabIndex = 16;
            this.lblNuevaPassword.Text = "Nueva contraseña:";

            // lblUsuario

            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Location = new System.Drawing.Point(27, 55);
            this.lblUsuario.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(73, 21);
            this.lblUsuario.TabIndex = 12;
            this.lblUsuario.Text = "Usuario:";

            // Titulo

            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(27, 10);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(396, 35);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "CAMBIAR CONTRASEÑA";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // pbxLogo

            this.pbxLogo.Location = new System.Drawing.Point(125, 21);
            this.pbxLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogo.Name = "pbxLogo";
            this.pbxLogo.Size = new System.Drawing.Size(250, 60);
            this.pbxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxLogo.TabIndex = 64;
            this.pbxLogo.TabStop = false;

            // pic_logo_dgm

            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            this.pic_logo_dgm.Location = new System.Drawing.Point(25, 21);
            this.pic_logo_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(90, 60);
            this.pic_logo_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_logo_dgm.TabIndex = 63;
            this.pic_logo_dgm.TabStop = false;

            // picb_logo

            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Location = new System.Drawing.Point(385, 21);
            this.picb_logo.Margin = new System.Windows.Forms.Padding(4);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(90, 60);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 62;
            this.picb_logo.TabStop = false;

            // FrmCambiarPassword

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(500, 450);
            this.ControlBox = false;
            this.Controls.Add(this.pbxLogo);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCambiarPassword";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cambiar Contraseña";
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
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.TextBox txtNuevaPassword;
        private System.Windows.Forms.Label lblNuevaPassword;
        private System.Windows.Forms.TextBox txtConfirmarPassword;
        private System.Windows.Forms.Label lblConfirmarPassword;
        private System.Windows.Forms.Button btnCambiar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblValidacion;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
    }
}