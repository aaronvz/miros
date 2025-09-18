namespace ENROLLMENT_V3
{
    partial class Verificacion
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Verificacion));
            this.nFVDedoB = new Neurotec.Biometrics.Gui.NFingerView();
            this.btnSalir = new System.Windows.Forms.Button();
            this.cmbEscaners = new System.Windows.Forms.ComboBox();
            this.btnVerificar = new System.Windows.Forms.Button();
            this.nFVDedoA = new Neurotec.Biometrics.Gui.NFingerView();
            this.txtContrasenia = new System.Windows.Forms.TextBox();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.pic_txt_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.label31 = new System.Windows.Forms.Label();
            this.lblEscaner = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            this.SuspendLayout();
            // 
            // nFVDedoB
            // 
            this.nFVDedoB.BackColor = System.Drawing.SystemColors.Control;
            this.nFVDedoB.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoB.Location = new System.Drawing.Point(514, 93);
            this.nFVDedoB.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoB.Name = "nFVDedoB";
            this.nFVDedoB.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoB.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoB.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoB.Size = new System.Drawing.Size(20, 25);
            this.nFVDedoB.TabIndex = 24;
            this.nFVDedoB.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoB.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoB.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoB.TreeWidth = 2D;
            // 
            // btnSalir
            // 
            this.btnSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.Location = new System.Drawing.Point(347, 189);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(133, 41);
            this.btnSalir.TabIndex = 4;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // cmbEscaners
            // 
            this.cmbEscaners.FormattingEnabled = true;
            this.cmbEscaners.Location = new System.Drawing.Point(94, 146);
            this.cmbEscaners.Name = "cmbEscaners";
            this.cmbEscaners.Size = new System.Drawing.Size(386, 21);
            this.cmbEscaners.TabIndex = 26;
            // 
            // btnVerificar
            // 
            this.btnVerificar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerificar.Location = new System.Drawing.Point(94, 192);
            this.btnVerificar.Name = "btnVerificar";
            this.btnVerificar.Size = new System.Drawing.Size(133, 41);
            this.btnVerificar.TabIndex = 3;
            this.btnVerificar.Text = "Iniciar";
            this.btnVerificar.UseVisualStyleBackColor = true;
            this.btnVerificar.Click += new System.EventHandler(this.btnVerificar_Click);
            // 
            // nFVDedoA
            // 
            this.nFVDedoA.BackColor = System.Drawing.SystemColors.Control;
            this.nFVDedoA.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoA.Location = new System.Drawing.Point(490, 93);
            this.nFVDedoA.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoA.Name = "nFVDedoA";
            this.nFVDedoA.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoA.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoA.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoA.Size = new System.Drawing.Size(20, 25);
            this.nFVDedoA.TabIndex = 23;
            this.nFVDedoA.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoA.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoA.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoA.TreeWidth = 2D;
            // 
            // txtContrasenia
            // 
            this.txtContrasenia.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContrasenia.Location = new System.Drawing.Point(93, 93);
            this.txtContrasenia.Name = "txtContrasenia";
            this.txtContrasenia.PasswordChar = '*';
            this.txtContrasenia.Size = new System.Drawing.Size(387, 22);
            this.txtContrasenia.TabIndex = 2;
            this.txtContrasenia.Enter += new System.EventHandler(this.txtContrasenia_Enter);
            // 
            // txtUsuario
            // 
            this.txtUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(93, 36);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(387, 22);
            this.txtUsuario.TabIndex = 1;
            this.txtUsuario.Enter += new System.EventHandler(this.txtUsuario_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(90, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 20);
            this.label2.TabIndex = 20;
            this.label2.Text = "Contraseña:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(90, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 20);
            this.label1.TabIndex = 19;
            this.label1.Text = "Usuario:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel1.Controls.Add(this.lblEscaner);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.txtUsuario);
            this.panel1.Controls.Add(this.nFVDedoB);
            this.panel1.Controls.Add(this.btnSalir);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.nFVDedoA);
            this.panel1.Controls.Add(this.txtContrasenia);
            this.panel1.Controls.Add(this.btnVerificar);
            this.panel1.Controls.Add(this.cmbEscaners);
            this.panel1.Location = new System.Drawing.Point(63, 106);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(574, 254);
            this.panel1.TabIndex = 31;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(486, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 41);
            this.button1.TabIndex = 27;
            this.button1.Text = "Escaners";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // pic_txt_dgm
            // 
            this.pic_txt_dgm.Location = new System.Drawing.Point(170, 14);
            this.pic_txt_dgm.Name = "pic_txt_dgm";
            this.pic_txt_dgm.Size = new System.Drawing.Size(361, 82);
            this.pic_txt_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_txt_dgm.TabIndex = 32;
            this.pic_txt_dgm.TabStop = false;
            // 
            // picb_logo
            // 
            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Image = ((System.Drawing.Image)(resources.GetObject("picb_logo.Image")));
            this.picb_logo.Location = new System.Drawing.Point(537, 14);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(100, 86);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 33;
            this.picb_logo.TabStop = false;
            // 
            // pic_logo_dgm
            // 
            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            this.pic_logo_dgm.Image = ((System.Drawing.Image)(resources.GetObject("pic_logo_dgm.Image")));
            this.pic_logo_dgm.Location = new System.Drawing.Point(63, 14);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(101, 82);
            this.pic_logo_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_logo_dgm.TabIndex = 34;
            this.pic_logo_dgm.TabStop = false;
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.ForeColor = System.Drawing.Color.Red;
            this.label31.Location = new System.Drawing.Point(612, 17);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(22, 16);
            this.label31.TabIndex = 60;
            this.label31.Text = "30";
            // 
            // lblEscaner
            // 
            this.lblEscaner.AutoSize = true;
            this.lblEscaner.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEscaner.ForeColor = System.Drawing.Color.White;
            this.lblEscaner.Location = new System.Drawing.Point(90, 123);
            this.lblEscaner.Name = "lblEscaner";
            this.lblEscaner.Size = new System.Drawing.Size(80, 20);
            this.lblEscaner.TabIndex = 28;
            this.lblEscaner.Text = "Escáner:";
            // 
            // Verificacion
            // 
            this.AcceptButton = this.btnVerificar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(695, 412);
            this.ControlBox = false;
            this.Controls.Add(this.label31);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.pic_txt_dgm);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Verificacion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Load += new System.EventHandler(this.Verificacion_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Neurotec.Biometrics.Gui.NFingerView nFVDedoB;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.ComboBox cmbEscaners;
        private System.Windows.Forms.Button btnVerificar;
        private Neurotec.Biometrics.Gui.NFingerView nFVDedoA;
        private System.Windows.Forms.TextBox txtContrasenia;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pic_txt_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblEscaner;
    }
}