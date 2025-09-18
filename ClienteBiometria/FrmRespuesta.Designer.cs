namespace ENROLLMENT_V3
{
    partial class FrmRespuesta
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRespuesta));
            this.pnlRespuesta = new System.Windows.Forms.Panel();
            this.lblIdMovimiento = new System.Windows.Forms.Label();
            this.pbxCheck = new System.Windows.Forms.PictureBox();
            this.pbxWarning = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.dgvRespuestas = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.btnVerificar = new System.Windows.Forms.Button();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.pic_txt_dgm = new System.Windows.Forms.PictureBox();
            this.lrespuestaback = new System.Windows.Forms.Label();
            this.pnlRespuesta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRespuestas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlRespuesta
            // 
            this.pnlRespuesta.BackColor = System.Drawing.Color.SteelBlue;
            this.pnlRespuesta.Controls.Add(this.lrespuestaback);
            this.pnlRespuesta.Controls.Add(this.lblIdMovimiento);
            this.pnlRespuesta.Controls.Add(this.pbxCheck);
            this.pnlRespuesta.Controls.Add(this.pbxWarning);
            this.pnlRespuesta.Controls.Add(this.groupBox1);
            this.pnlRespuesta.Controls.Add(this.label3);
            this.pnlRespuesta.Controls.Add(this.btnVerificar);
            this.pnlRespuesta.Location = new System.Drawing.Point(63, 105);
            this.pnlRespuesta.Name = "pnlRespuesta";
            this.pnlRespuesta.Size = new System.Drawing.Size(787, 578);
            this.pnlRespuesta.TabIndex = 31;
            // 
            // lblIdMovimiento
            // 
            this.lblIdMovimiento.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdMovimiento.ForeColor = System.Drawing.Color.White;
            this.lblIdMovimiento.Location = new System.Drawing.Point(3, 502);
            this.lblIdMovimiento.Name = "lblIdMovimiento";
            this.lblIdMovimiento.Size = new System.Drawing.Size(787, 19);
            this.lblIdMovimiento.TabIndex = 133;
            this.lblIdMovimiento.Text = "RESPUESTA(S):";
            this.lblIdMovimiento.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblIdMovimiento.Visible = false;
            // 
            // pbxCheck
            // 
            this.pbxCheck.BackColor = System.Drawing.Color.Transparent;
            this.pbxCheck.Image = ((System.Drawing.Image)(resources.GetObject("pbxCheck.Image")));
            this.pbxCheck.Location = new System.Drawing.Point(751, 148);
            this.pbxCheck.Name = "pbxCheck";
            this.pbxCheck.Size = new System.Drawing.Size(24, 25);
            this.pbxCheck.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxCheck.TabIndex = 36;
            this.pbxCheck.TabStop = false;
            this.pbxCheck.Visible = false;
            // 
            // pbxWarning
            // 
            this.pbxWarning.BackColor = System.Drawing.Color.Transparent;
            this.pbxWarning.Image = ((System.Drawing.Image)(resources.GetObject("pbxWarning.Image")));
            this.pbxWarning.Location = new System.Drawing.Point(663, 3);
            this.pbxWarning.Name = "pbxWarning";
            this.pbxWarning.Size = new System.Drawing.Size(48, 48);
            this.pbxWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbxWarning.TabIndex = 35;
            this.pbxWarning.TabStop = false;
            this.pbxWarning.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.dgvRespuestas);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Black;
            this.groupBox1.Location = new System.Drawing.Point(39, 94);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox1.Size = new System.Drawing.Size(707, 408);
            this.groupBox1.TabIndex = 132;
            this.groupBox1.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(5, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(164, 21);
            this.label7.TabIndex = 6;
            this.label7.Text = "Listado de personas:";
            // 
            // dgvRespuestas
            // 
            this.dgvRespuestas.AllowUserToAddRows = false;
            this.dgvRespuestas.AllowUserToDeleteRows = false;
            this.dgvRespuestas.AllowUserToResizeColumns = false;
            this.dgvRespuestas.AllowUserToResizeRows = false;
            this.dgvRespuestas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRespuestas.Location = new System.Drawing.Point(4, 23);
            this.dgvRespuestas.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dgvRespuestas.MultiSelect = false;
            this.dgvRespuestas.Name = "dgvRespuestas";
            this.dgvRespuestas.RowHeadersVisible = false;
            this.dgvRespuestas.RowHeadersWidth = 51;
            this.dgvRespuestas.RowTemplate.Height = 100;
            this.dgvRespuestas.Size = new System.Drawing.Size(696, 419);
            this.dgvRespuestas.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(787, 34);
            this.label3.TabIndex = 131;
            this.label3.Text = "RESPUESTA(S):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnVerificar
            // 
            this.btnVerificar.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnVerificar.FlatAppearance.BorderSize = 0;
            this.btnVerificar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Blue;
            this.btnVerificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerificar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerificar.ForeColor = System.Drawing.Color.White;
            this.btnVerificar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnVerificar.Location = new System.Drawing.Point(339, 525);
            this.btnVerificar.Name = "btnVerificar";
            this.btnVerificar.Size = new System.Drawing.Size(110, 38);
            this.btnVerificar.TabIndex = 5;
            this.btnVerificar.Text = "Aceptar";
            this.btnVerificar.UseVisualStyleBackColor = false;
            this.btnVerificar.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnVerificar_MouseClick);
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
            // picb_logo
            // 
            this.picb_logo.BackColor = System.Drawing.Color.Transparent;
            this.picb_logo.Image = ((System.Drawing.Image)(resources.GetObject("picb_logo.Image")));
            this.picb_logo.Location = new System.Drawing.Point(665, 10);
            this.picb_logo.Name = "picb_logo";
            this.picb_logo.Size = new System.Drawing.Size(185, 86);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 33;
            this.picb_logo.TabStop = false;
            // 
            // pic_txt_dgm
            // 
            this.pic_txt_dgm.Location = new System.Drawing.Point(170, 14);
            this.pic_txt_dgm.Name = "pic_txt_dgm";
            this.pic_txt_dgm.Size = new System.Drawing.Size(489, 82);
            this.pic_txt_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_txt_dgm.TabIndex = 32;
            this.pic_txt_dgm.TabStop = false;
            // 
            // lrespuestaback
            // 
            this.lrespuestaback.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lrespuestaback.ForeColor = System.Drawing.Color.White;
            this.lrespuestaback.Location = new System.Drawing.Point(39, 54);
            this.lrespuestaback.Name = "lrespuestaback";
            this.lrespuestaback.Size = new System.Drawing.Size(707, 37);
            this.lrespuestaback.TabIndex = 134;
            this.lrespuestaback.Text = "RESPUESTA(S):";
            this.lrespuestaback.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lrespuestaback.Click += new System.EventHandler(this.lrespuestaback_Click);
            // 
            // FrmRespuesta
            // 
            this.AcceptButton = this.btnVerificar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(901, 714);
            this.ControlBox = false;
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.pic_txt_dgm);
            this.Controls.Add(this.pnlRespuesta);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRespuesta";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Load += new System.EventHandler(this.Verificacion_Load);
            this.pnlRespuesta.ResumeLayout(false);
            this.pnlRespuesta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRespuestas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlRespuesta;
        private System.Windows.Forms.PictureBox pic_txt_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Button btnVerificar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView dgvRespuestas;
        private System.Windows.Forms.PictureBox pbxCheck;
        private System.Windows.Forms.PictureBox pbxWarning;
        private System.Windows.Forms.Label lblIdMovimiento;
        private System.Windows.Forms.Label lrespuestaback;
    }
}