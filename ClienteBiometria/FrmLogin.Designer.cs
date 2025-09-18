namespace ENROLLMENT_V3
{
    partial class FrmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLogin));
            this.nFVDedoB = new Neurotec.Biometrics.Gui.NFingerView();
            this.cmbEscaners = new System.Windows.Forms.ComboBox();
            this.nFVDedoA = new Neurotec.Biometrics.Gui.NFingerView();
            this.txtContrasenia = new System.Windows.Forms.TextBox();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.l_api = new System.Windows.Forms.Label();
            this.lversion = new System.Windows.Forms.Label();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnVerificar = new System.Windows.Forms.Button();
            this.txtConsola = new System.Windows.Forms.TextBox();
            this.pbxLogin = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblEscanerHuellas = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.pbxCheck = new System.Windows.Forms.PictureBox();
            this.pbxLoad = new System.Windows.Forms.PictureBox();
            this.pbxWarning = new System.Windows.Forms.PictureBox();
            this.pbxLogo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogin)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLoad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            this.SuspendLayout();
            // 
            // nFVDedoB
            // 
            this.nFVDedoB.BackColor = System.Drawing.Color.White;
            this.nFVDedoB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nFVDedoB.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoB.Location = new System.Drawing.Point(19, 17);
            this.nFVDedoB.Margin = new System.Windows.Forms.Padding(4);
            this.nFVDedoB.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoB.Name = "nFVDedoB";
            this.nFVDedoB.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoB.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoB.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoB.Size = new System.Drawing.Size(110, 131);
            this.nFVDedoB.TabIndex = 24;
            this.nFVDedoB.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoB.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoB.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoB.TreeWidth = 2D;
            // 
            // cmbEscaners
            // 
            this.cmbEscaners.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEscaners.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEscaners.FormattingEnabled = true;
            this.cmbEscaners.Location = new System.Drawing.Point(32, 240);
            this.cmbEscaners.Margin = new System.Windows.Forms.Padding(4);
            this.cmbEscaners.Name = "cmbEscaners";
            this.cmbEscaners.Size = new System.Drawing.Size(463, 21);
            this.cmbEscaners.TabIndex = 3;
            this.cmbEscaners.SelectedIndexChanged += new System.EventHandler(this.CmbEscaners_SelectedIndexChanged);
            // 
            // nFVDedoA
            // 
            this.nFVDedoA.BackColor = System.Drawing.Color.White;
            this.nFVDedoA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nFVDedoA.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoA.Location = new System.Drawing.Point(17, 17);
            this.nFVDedoA.Margin = new System.Windows.Forms.Padding(4);
            this.nFVDedoA.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoA.Name = "nFVDedoA";
            this.nFVDedoA.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoA.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoA.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoA.Size = new System.Drawing.Size(110, 131);
            this.nFVDedoA.TabIndex = 23;
            this.nFVDedoA.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoA.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoA.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoA.TreeWidth = 2D;
            // 
            // txtContrasenia
            // 
            this.txtContrasenia.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContrasenia.Location = new System.Drawing.Point(31, 155);
            this.txtContrasenia.Margin = new System.Windows.Forms.Padding(4);
            this.txtContrasenia.Name = "txtContrasenia";
            this.txtContrasenia.PasswordChar = '*';
            this.txtContrasenia.Size = new System.Drawing.Size(464, 25);
            this.txtContrasenia.TabIndex = 2;
            this.txtContrasenia.Enter += new System.EventHandler(this.txtContrasenia_Enter);
            // 
            // txtUsuario
            // 
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsuario.Location = new System.Drawing.Point(33, 71);
            this.txtUsuario.Margin = new System.Windows.Forms.Padding(4);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(464, 25);
            this.txtUsuario.TabIndex = 1;
            this.txtUsuario.Enter += new System.EventHandler(this.txtUsuario_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(39, 113);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 21);
            this.label2.TabIndex = 20;
            this.label2.Text = "Contraseña:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(39, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 21);
            this.label1.TabIndex = 19;
            this.label1.Text = "Usuario:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.l_api);
            this.panel1.Controls.Add(this.lversion);
            this.panel1.Controls.Add(this.btnSalir);
            this.panel1.Controls.Add(this.btnLimpiar);
            this.panel1.Controls.Add(this.btnVerificar);
            this.panel1.Controls.Add(this.txtConsola);
            this.panel1.Controls.Add(this.pbxLogin);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.lblEscanerHuellas);
            this.panel1.Controls.Add(this.txtUsuario);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtContrasenia);
            this.panel1.Controls.Add(this.cmbEscaners);
            this.panel1.Location = new System.Drawing.Point(76, 130);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(827, 396);
            this.panel1.TabIndex = 31;
            // 
            // l_api
            // 
            this.l_api.AutoSize = true;
            this.l_api.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l_api.ForeColor = System.Drawing.Color.White;
            this.l_api.Location = new System.Drawing.Point(29, 379);
            this.l_api.Name = "l_api";
            this.l_api.Size = new System.Drawing.Size(120, 13);
            this.l_api.TabIndex = 37;
            this.l_api.Text = "Versión 2025-04-30-01";
            this.l_api.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lversion
            // 
            this.lversion.AutoSize = true;
            this.lversion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lversion.ForeColor = System.Drawing.Color.White;
            this.lversion.Location = new System.Drawing.Point(352, 371);
            this.lversion.Name = "lversion";
            this.lversion.Size = new System.Drawing.Size(143, 17);
            this.lversion.TabIndex = 36;
            this.lversion.Text = "Versión 2025-04-30-01";
            this.lversion.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // btnSalir
            // 
            this.btnSalir.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnSalir.FlatAppearance.BorderSize = 0;
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSalir.Location = new System.Drawing.Point(364, 320);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(4);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(132, 47);
            this.btnSalir.TabIndex = 6;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.ForeColor = System.Drawing.Color.White;
            this.btnLimpiar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLimpiar.Location = new System.Drawing.Point(200, 320);
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(4);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(132, 47);
            this.btnLimpiar.TabIndex = 5;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnLimpiar_MouseClick);
            // 
            // btnVerificar
            // 
            this.btnVerificar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnVerificar.FlatAppearance.BorderSize = 0;
            this.btnVerificar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnVerificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerificar.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerificar.ForeColor = System.Drawing.Color.White;
            this.btnVerificar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnVerificar.Location = new System.Drawing.Point(33, 320);
            this.btnVerificar.Margin = new System.Windows.Forms.Padding(4);
            this.btnVerificar.Name = "btnVerificar";
            this.btnVerificar.Size = new System.Drawing.Size(132, 47);
            this.btnVerificar.TabIndex = 4;
            this.btnVerificar.Text = "Iniciar";
            this.btnVerificar.UseVisualStyleBackColor = false;
            this.btnVerificar.Click += new System.EventHandler(this.btnVerificar_Click);
            // 
            // txtConsola
            // 
            this.txtConsola.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtConsola.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConsola.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsola.ForeColor = System.Drawing.Color.Black;
            this.txtConsola.Location = new System.Drawing.Point(515, 287);
            this.txtConsola.Margin = new System.Windows.Forms.Padding(4);
            this.txtConsola.Multiline = true;
            this.txtConsola.Name = "txtConsola";
            this.txtConsola.ReadOnly = true;
            this.txtConsola.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConsola.Size = new System.Drawing.Size(296, 105);
            this.txtConsola.TabIndex = 35;
            this.txtConsola.Text = "Cargando...";
            this.txtConsola.WordWrap = false;
            this.txtConsola.TextChanged += new System.EventHandler(this.txtConsola_TextChanged);
            // 
            // pbxLogin
            // 
            this.pbxLogin.BackColor = System.Drawing.Color.Transparent;
            this.pbxLogin.Image = ((System.Drawing.Image)(resources.GetObject("pbxLogin.Image")));
            this.pbxLogin.Location = new System.Drawing.Point(649, 250);
            this.pbxLogin.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogin.Name = "pbxLogin";
            this.pbxLogin.Size = new System.Drawing.Size(32, 30);
            this.pbxLogin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxLogin.TabIndex = 34;
            this.pbxLogin.TabStop = false;
            this.pbxLogin.Click += new System.EventHandler(this.pbxLogin_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(27, -2);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(787, 42);
            this.label4.TabIndex = 32;
            this.label4.Text = "SUBDIRECCIÓN DE CONTROL MIGRATORIO";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.nFVDedoB);
            this.panel3.Location = new System.Drawing.Point(667, 71);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(147, 171);
            this.panel3.TabIndex = 30;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.nFVDedoA);
            this.panel2.Location = new System.Drawing.Point(515, 71);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(147, 171);
            this.panel2.TabIndex = 29;
            // 
            // lblEscanerHuellas
            // 
            this.lblEscanerHuellas.AutoSize = true;
            this.lblEscanerHuellas.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEscanerHuellas.ForeColor = System.Drawing.Color.White;
            this.lblEscanerHuellas.Location = new System.Drawing.Point(39, 198);
            this.lblEscanerHuellas.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEscanerHuellas.Name = "lblEscanerHuellas";
            this.lblEscanerHuellas.Size = new System.Drawing.Size(73, 21);
            this.lblEscanerHuellas.TabIndex = 28;
            this.lblEscanerHuellas.Text = "Escáner:";
            this.lblEscanerHuellas.Click += new System.EventHandler(this.lblEscanerHuellas_Click);
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.ForeColor = System.Drawing.Color.Red;
            this.label31.Location = new System.Drawing.Point(843, 27);
            this.label31.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(23, 16);
            this.label31.TabIndex = 60;
            this.label31.Text = "31";
            this.label31.Visible = false;
            // 
            // pbxCheck
            // 
            this.pbxCheck.BackColor = System.Drawing.Color.Transparent;
            this.pbxCheck.Image = ((System.Drawing.Image)(resources.GetObject("pbxCheck.Image")));
            this.pbxCheck.Location = new System.Drawing.Point(864, 546);
            this.pbxCheck.Margin = new System.Windows.Forms.Padding(4);
            this.pbxCheck.Name = "pbxCheck";
            this.pbxCheck.Size = new System.Drawing.Size(32, 30);
            this.pbxCheck.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxCheck.TabIndex = 38;
            this.pbxCheck.TabStop = false;
            this.pbxCheck.Visible = false;
            // 
            // pbxLoad
            // 
            this.pbxLoad.BackColor = System.Drawing.Color.Transparent;
            this.pbxLoad.Image = ((System.Drawing.Image)(resources.GetObject("pbxLoad.Image")));
            this.pbxLoad.Location = new System.Drawing.Point(828, 546);
            this.pbxLoad.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLoad.Name = "pbxLoad";
            this.pbxLoad.Size = new System.Drawing.Size(32, 30);
            this.pbxLoad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxLoad.TabIndex = 37;
            this.pbxLoad.TabStop = false;
            this.pbxLoad.Visible = false;
            // 
            // pbxWarning
            // 
            this.pbxWarning.BackColor = System.Drawing.Color.Transparent;
            this.pbxWarning.Image = ((System.Drawing.Image)(resources.GetObject("pbxWarning.Image")));
            this.pbxWarning.Location = new System.Drawing.Point(788, 546);
            this.pbxWarning.Margin = new System.Windows.Forms.Padding(4);
            this.pbxWarning.Name = "pbxWarning";
            this.pbxWarning.Size = new System.Drawing.Size(32, 30);
            this.pbxWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxWarning.TabIndex = 36;
            this.pbxWarning.TabStop = false;
            this.pbxWarning.Visible = false;
            // 
            // pbxLogo
            // 
            this.pbxLogo.Location = new System.Drawing.Point(227, 21);
            this.pbxLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogo.Name = "pbxLogo";
            this.pbxLogo.Size = new System.Drawing.Size(421, 101);
            this.pbxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxLogo.TabIndex = 61;
            this.pbxLogo.TabStop = false;
            this.pbxLogo.Click += new System.EventHandler(this.pbxLogo_Click);
            // 
            // pic_logo_dgm
            // 
            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            this.pic_logo_dgm.Image = ((System.Drawing.Image)(resources.GetObject("pic_logo_dgm.Image")));
            this.pic_logo_dgm.Location = new System.Drawing.Point(84, 21);
            this.pic_logo_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(135, 97);
            this.pic_logo_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_logo_dgm.TabIndex = 34;
            this.pic_logo_dgm.TabStop = false;
            // 
            // picb_logo
            // 
            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Image = ((System.Drawing.Image)(resources.GetObject("picb_logo.Image")));
            this.picb_logo.Location = new System.Drawing.Point(656, 21);
            this.picb_logo.Margin = new System.Windows.Forms.Padding(4);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(247, 101);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 33;
            this.picb_logo.TabStop = false;
            // 
            // FrmLogin
            // 
            this.AcceptButton = this.btnVerificar;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(977, 599);
            this.ControlBox = false;
            this.Controls.Add(this.pbxCheck);
            this.Controls.Add(this.pbxLoad);
            this.Controls.Add(this.pbxWarning);
            this.Controls.Add(this.pbxLogo);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Shown += new System.EventHandler(this.FrmLogin_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogin)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLoad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Neurotec.Biometrics.Gui.NFingerView nFVDedoB;
        private System.Windows.Forms.ComboBox cmbEscaners;
        private Neurotec.Biometrics.Gui.NFingerView nFVDedoA;
        private System.Windows.Forms.TextBox txtContrasenia;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label lblEscanerHuellas;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pbxLogin;
        private System.Windows.Forms.TextBox txtConsola;
        private System.Windows.Forms.PictureBox pbxWarning;
        private System.Windows.Forms.PictureBox pbxLoad;
        private System.Windows.Forms.PictureBox pbxCheck;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnVerificar;
        private System.Windows.Forms.Label lversion;
        private System.Windows.Forms.Label l_api;
    }
}