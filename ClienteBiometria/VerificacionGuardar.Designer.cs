namespace ENROLLMENT_V3
{
    partial class VerificacionGuardar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificacionGuardar));
            this.nFVDedoB = new Neurotec.Biometrics.Gui.NFingerView();
            this.cmbEscaners = new System.Windows.Forms.ComboBox();
            this.nFVDedoA = new Neurotec.Biometrics.Gui.NFingerView();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblIcaoPaisDestino = new System.Windows.Forms.Label();
            this.lblIcaoPaisOrigen = new System.Windows.Forms.Label();
            this.lblIdVuelo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvPersonas = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnVuelos = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTramite = new System.Windows.Forms.TextBox();
            this.txtCriterio = new System.Windows.Forms.TextBox();
            this.dgvVuelos = new System.Windows.Forms.DataGridView();
            this.valor = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnSalir = new System.Windows.Forms.Button();
            this.btnVerificar = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pic_txt_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPersonas)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVuelos)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // nFVDedoB
            // 
            this.nFVDedoB.BackColor = System.Drawing.Color.White;
            this.nFVDedoB.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoB.Location = new System.Drawing.Point(14, 14);
            this.nFVDedoB.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoB.Name = "nFVDedoB";
            this.nFVDedoB.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoB.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoB.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoB.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoB.Size = new System.Drawing.Size(83, 107);
            this.nFVDedoB.TabIndex = 24;
            this.nFVDedoB.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoB.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoB.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoB.TreeWidth = 2D;
            // 
            // cmbEscaners
            // 
            this.cmbEscaners.FormattingEnabled = true;
            this.cmbEscaners.Location = new System.Drawing.Point(89, 81);
            this.cmbEscaners.Name = "cmbEscaners";
            this.cmbEscaners.Size = new System.Drawing.Size(264, 21);
            this.cmbEscaners.TabIndex = 26;
            this.cmbEscaners.Visible = false;
            this.cmbEscaners.SelectedIndexChanged += new System.EventHandler(this.CmbEscaners_SelectedIndexChanged);
            // 
            // nFVDedoA
            // 
            this.nFVDedoA.BackColor = System.Drawing.Color.White;
            this.nFVDedoA.BoundingRectColor = System.Drawing.Color.Red;
            this.nFVDedoA.Location = new System.Drawing.Point(13, 14);
            this.nFVDedoA.MinutiaColor = System.Drawing.Color.Red;
            this.nFVDedoA.Name = "nFVDedoA";
            this.nFVDedoA.NeighborMinutiaColor = System.Drawing.Color.Orange;
            this.nFVDedoA.ResultImageColor = System.Drawing.Color.Green;
            this.nFVDedoA.SelectedMinutiaColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SelectedSingularPointColor = System.Drawing.Color.Magenta;
            this.nFVDedoA.SingularPointColor = System.Drawing.Color.Red;
            this.nFVDedoA.Size = new System.Drawing.Size(83, 107);
            this.nFVDedoA.TabIndex = 23;
            this.nFVDedoA.TreeColor = System.Drawing.Color.Crimson;
            this.nFVDedoA.TreeMinutiaNumberDiplayFormat = Neurotec.Biometrics.Gui.MinutiaNumberDiplayFormat.DontDisplay;
            this.nFVDedoA.TreeMinutiaNumberFont = new System.Drawing.Font("Arial", 10F);
            this.nFVDedoA.TreeWidth = 2D;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(178, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 20);
            this.label2.TabIndex = 20;
            this.label2.Text = "Nombre:";
            this.label2.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 20);
            this.label1.TabIndex = 19;
            this.label1.Text = "Usuario:";
            this.label1.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.lblIcaoPaisDestino);
            this.panel1.Controls.Add(this.lblIcaoPaisOrigen);
            this.panel1.Controls.Add(this.lblIdVuelo);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.btnSalir);
            this.panel1.Controls.Add(this.btnVerificar);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lblNombre);
            this.panel1.Controls.Add(this.lblUsuario);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmbEscaners);
            this.panel1.Location = new System.Drawing.Point(63, 106);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1074, 543);
            this.panel1.TabIndex = 31;
            // 
            // lblIcaoPaisDestino
            // 
            this.lblIcaoPaisDestino.AutoSize = true;
            this.lblIcaoPaisDestino.Location = new System.Drawing.Point(877, 11);
            this.lblIcaoPaisDestino.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblIcaoPaisDestino.Name = "lblIcaoPaisDestino";
            this.lblIcaoPaisDestino.Size = new System.Drawing.Size(94, 13);
            this.lblIcaoPaisDestino.TabIndex = 134;
            this.lblIcaoPaisDestino.Text = "lblIcaoPaisDestino";
            // 
            // lblIcaoPaisOrigen
            // 
            this.lblIcaoPaisOrigen.AutoSize = true;
            this.lblIcaoPaisOrigen.Location = new System.Drawing.Point(755, 11);
            this.lblIcaoPaisOrigen.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblIcaoPaisOrigen.Name = "lblIcaoPaisOrigen";
            this.lblIcaoPaisOrigen.Size = new System.Drawing.Size(89, 13);
            this.lblIcaoPaisOrigen.TabIndex = 133;
            this.lblIcaoPaisOrigen.Text = "lblIcaoPaisOrigen";
            // 
            // lblIdVuelo
            // 
            this.lblIdVuelo.AutoSize = true;
            this.lblIdVuelo.Location = new System.Drawing.Point(28, 11);
            this.lblIdVuelo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblIdVuelo.Name = "lblIdVuelo";
            this.lblIdVuelo.Size = new System.Drawing.Size(35, 13);
            this.lblIdVuelo.TabIndex = 132;
            this.lblIdVuelo.Text = "label8";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.dgvPersonas);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Black;
            this.groupBox1.Location = new System.Drawing.Point(537, 41);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(508, 441);
            this.groupBox1.TabIndex = 131;
            this.groupBox1.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(11, -6);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(177, 20);
            this.label7.TabIndex = 6;
            this.label7.Text = "Listado de personas:";
            // 
            // dgvPersonas
            // 
            this.dgvPersonas.AllowUserToAddRows = false;
            this.dgvPersonas.AllowUserToDeleteRows = false;
            this.dgvPersonas.AllowUserToResizeColumns = false;
            this.dgvPersonas.AllowUserToResizeRows = false;
            this.dgvPersonas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPersonas.Location = new System.Drawing.Point(7, 26);
            this.dgvPersonas.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvPersonas.MultiSelect = false;
            this.dgvPersonas.Name = "dgvPersonas";
            this.dgvPersonas.RowHeadersVisible = false;
            this.dgvPersonas.RowHeadersWidth = 51;
            this.dgvPersonas.RowTemplate.Height = 100;
            this.dgvPersonas.Size = new System.Drawing.Size(494, 410);
            this.dgvPersonas.TabIndex = 4;
            this.dgvPersonas.Click += new System.EventHandler(this.dgvPersonas_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(0, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(574, 34);
            this.label3.TabIndex = 131;
            this.label3.Text = "SELECCIONE EL NÚMERO DE VUELO";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnVuelos);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.txtTramite);
            this.groupBox4.Controls.Add(this.txtCriterio);
            this.groupBox4.Controls.Add(this.dgvVuelos);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.Color.Black;
            this.groupBox4.Location = new System.Drawing.Point(15, 41);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Size = new System.Drawing.Size(508, 441);
            this.groupBox4.TabIndex = 130;
            this.groupBox4.TabStop = false;
            // 
            // btnVuelos
            // 
            this.btnVuelos.Location = new System.Drawing.Point(430, 31);
            this.btnVuelos.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnVuelos.Name = "btnVuelos";
            this.btnVuelos.Size = new System.Drawing.Size(72, 41);
            this.btnVuelos.TabIndex = 6;
            this.btnVuelos.Text = "Vuelos";
            this.btnVuelos.UseVisualStyleBackColor = true;
            this.btnVuelos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnVuelos_MouseClick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(3, -4);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(148, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "Número de vuelo:";
            // 
            // txtTramite
            // 
            this.txtTramite.Location = new System.Drawing.Point(5, 28);
            this.txtTramite.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtTramite.Multiline = true;
            this.txtTramite.Name = "txtTramite";
            this.txtTramite.ReadOnly = true;
            this.txtTramite.Size = new System.Drawing.Size(422, 44);
            this.txtTramite.TabIndex = 4;
            this.txtTramite.TabStop = false;
            // 
            // txtCriterio
            // 
            this.txtCriterio.Location = new System.Drawing.Point(5, 76);
            this.txtCriterio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtCriterio.Name = "txtCriterio";
            this.txtCriterio.Size = new System.Drawing.Size(498, 20);
            this.txtCriterio.TabIndex = 3;
            this.txtCriterio.Click += new System.EventHandler(this.txtCriterio_Click);
            this.txtCriterio.TextChanged += new System.EventHandler(this.txtCriterio_TextChanged);
            this.txtCriterio.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCriterio_KeyPress);
            // 
            // dgvVuelos
            // 
            this.dgvVuelos.AllowUserToAddRows = false;
            this.dgvVuelos.AllowUserToDeleteRows = false;
            this.dgvVuelos.AllowUserToResizeColumns = false;
            this.dgvVuelos.AllowUserToResizeRows = false;
            this.dgvVuelos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVuelos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.valor});
            this.dgvVuelos.Location = new System.Drawing.Point(5, 107);
            this.dgvVuelos.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvVuelos.MultiSelect = false;
            this.dgvVuelos.Name = "dgvVuelos";
            this.dgvVuelos.ReadOnly = true;
            this.dgvVuelos.RowHeadersVisible = false;
            this.dgvVuelos.RowHeadersWidth = 51;
            this.dgvVuelos.RowTemplate.Height = 24;
            this.dgvVuelos.Size = new System.Drawing.Size(497, 329);
            this.dgvVuelos.TabIndex = 4;
            this.dgvVuelos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVuelos_CellContentClick);
            this.dgvVuelos.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.dgvVuelos_PreviewKeyDown);
            // 
            // valor
            // 
            this.valor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.valor.HeaderText = "";
            this.valor.MinimumWidth = 6;
            this.valor.Name = "valor";
            this.valor.ReadOnly = true;
            this.valor.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.valor.Width = 6;
            // 
            // btnSalir
            // 
            this.btnSalir.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnSalir.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnSalir.FlatAppearance.BorderSize = 0;
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSalir.Location = new System.Drawing.Point(301, 488);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(110, 38);
            this.btnSalir.TabIndex = 6;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // btnVerificar
            // 
            this.btnVerificar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnVerificar.FlatAppearance.BorderSize = 0;
            this.btnVerificar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnVerificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerificar.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerificar.ForeColor = System.Drawing.Color.White;
            this.btnVerificar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnVerificar.Location = new System.Drawing.Point(174, 488);
            this.btnVerificar.Name = "btnVerificar";
            this.btnVerificar.Size = new System.Drawing.Size(110, 38);
            this.btnVerificar.TabIndex = 5;
            this.btnVerificar.Text = "Confirmar";
            this.btnVerificar.UseVisualStyleBackColor = false;
            this.btnVerificar.Click += new System.EventHandler(this.btnVerificar_Click);
            this.btnVerificar.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnVerificar_MouseClick);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.nFVDedoA);
            this.panel3.Location = new System.Drawing.Point(358, 44);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(110, 136);
            this.panel3.TabIndex = 36;
            this.panel3.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.nFVDedoB);
            this.panel2.Location = new System.Drawing.Point(464, 58);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(110, 136);
            this.panel2.TabIndex = 35;
            this.panel2.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(3, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 20);
            this.label5.TabIndex = 34;
            this.label5.Text = "Escáner:";
            this.label5.Visible = false;
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombre.ForeColor = System.Drawing.Color.White;
            this.lblNombre.Location = new System.Drawing.Point(261, 50);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(89, 20);
            this.lblNombre.TabIndex = 33;
            this.lblNombre.Text = "lblNombre";
            this.lblNombre.Visible = false;
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Location = new System.Drawing.Point(86, 38);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(89, 20);
            this.lblUsuario.TabIndex = 32;
            this.lblUsuario.Text = "lblUsuario";
            this.lblUsuario.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(26, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(284, 20);
            this.label4.TabIndex = 31;
            this.label4.Text = "Por favor ingrese huella registrada";
            this.label4.Visible = false;
            // 
            // pic_txt_dgm
            // 
            this.pic_txt_dgm.Location = new System.Drawing.Point(182, 14);
            this.pic_txt_dgm.Name = "pic_txt_dgm";
            this.pic_txt_dgm.Size = new System.Drawing.Size(316, 82);
            this.pic_txt_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_txt_dgm.TabIndex = 32;
            this.pic_txt_dgm.TabStop = false;
            // 
            // picb_logo
            // 
            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Image = ((System.Drawing.Image)(resources.GetObject("picb_logo.Image")));
            this.picb_logo.Location = new System.Drawing.Point(508, 17);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(185, 86);
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
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(714, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(316, 82);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 35;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(1036, 14);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(101, 82);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 36;
            this.pictureBox2.TabStop = false;
            // 
            // VerificacionGuardar
            // 
            this.AcceptButton = this.btnVerificar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnSalir;
            this.ClientSize = new System.Drawing.Size(1186, 687);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.pic_txt_dgm);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificacionGuardar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Load += new System.EventHandler(this.Verificacion_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPersonas)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVuelos)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Neurotec.Biometrics.Gui.NFingerView nFVDedoB;
        private System.Windows.Forms.ComboBox cmbEscaners;
        private Neurotec.Biometrics.Gui.NFingerView nFVDedoA;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pic_txt_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label lblNombre;
        public System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Button btnVerificar;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtTramite;
        private System.Windows.Forms.TextBox txtCriterio;
        private System.Windows.Forms.DataGridView dgvVuelos;
        private System.Windows.Forms.DataGridViewCheckBoxColumn valor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvPersonas;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lblIdVuelo;
        private System.Windows.Forms.Label lblIcaoPaisDestino;
        private System.Windows.Forms.Label lblIcaoPaisOrigen;
        private System.Windows.Forms.Button btnVuelos;
    }
}