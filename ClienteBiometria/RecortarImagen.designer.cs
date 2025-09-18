namespace ENROLLMENT_V3
{
    partial class RecortarImagen
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecortarImagen));
            this.pnlSuperior = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlInferior = new System.Windows.Forms.Panel();
            this.txtMensaje = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnListo = new System.Windows.Forms.Button();
            this.btnMenos = new System.Windows.Forms.Button();
            this.btnMas = new System.Windows.Forms.Button();
            this.btnRecortar = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtArchivo = new System.Windows.Forms.TextBox();
            this.pbxImagen = new System.Windows.Forms.PictureBox();
            this.btnExplorarI = new System.Windows.Forms.Button();
            this.pnlSuperior.SuspendLayout();
            this.pnlInferior.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxImagen)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlSuperior
            // 
            this.pnlSuperior.Controls.Add(this.label1);
            this.pnlSuperior.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSuperior.Location = new System.Drawing.Point(0, 0);
            this.pnlSuperior.Name = "pnlSuperior";
            this.pnlSuperior.Size = new System.Drawing.Size(800, 100);
            this.pnlSuperior.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(289, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Recortar Fotografía";
            // 
            // pnlInferior
            // 
            this.pnlInferior.Controls.Add(this.txtMensaje);
            this.pnlInferior.Controls.Add(this.label2);
            this.pnlInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInferior.Location = new System.Drawing.Point(0, 350);
            this.pnlInferior.Name = "pnlInferior";
            this.pnlInferior.Size = new System.Drawing.Size(800, 100);
            this.pnlInferior.TabIndex = 2;
            // 
            // txtMensaje
            // 
            this.txtMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMensaje.Location = new System.Drawing.Point(0, 13);
            this.txtMensaje.Multiline = true;
            this.txtMensaje.Name = "txtMensaje";
            this.txtMensaje.ReadOnly = true;
            this.txtMensaje.Size = new System.Drawing.Size(800, 87);
            this.txtMensaje.TabIndex = 1;
            this.txtMensaje.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Mensaje:";
            this.label2.Visible = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 100);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 250);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnListo);
            this.tabPage1.Controls.Add(this.btnMenos);
            this.tabPage1.Controls.Add(this.btnMas);
            this.tabPage1.Controls.Add(this.btnRecortar);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtArchivo);
            this.tabPage1.Controls.Add(this.pbxImagen);
            this.tabPage1.Controls.Add(this.btnExplorarI);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 224);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Fotografía";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnListo
            // 
            this.btnListo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnListo.Location = new System.Drawing.Point(439, 195);
            this.btnListo.Name = "btnListo";
            this.btnListo.Size = new System.Drawing.Size(57, 23);
            this.btnListo.TabIndex = 15;
            this.btnListo.Text = "¡Listo!";
            this.btnListo.UseVisualStyleBackColor = true;
            this.btnListo.Click += new System.EventHandler(this.btnListo_Click);
            // 
            // btnMenos
            // 
            this.btnMenos.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnMenos.Location = new System.Drawing.Point(719, 94);
            this.btnMenos.Name = "btnMenos";
            this.btnMenos.Size = new System.Drawing.Size(36, 23);
            this.btnMenos.TabIndex = 14;
            this.btnMenos.Text = "-";
            this.btnMenos.UseVisualStyleBackColor = true;
            this.btnMenos.Click += new System.EventHandler(this.btnMenos_Click);
            // 
            // btnMas
            // 
            this.btnMas.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnMas.Location = new System.Drawing.Point(719, 65);
            this.btnMas.Name = "btnMas";
            this.btnMas.Size = new System.Drawing.Size(36, 23);
            this.btnMas.TabIndex = 13;
            this.btnMas.Text = "+";
            this.btnMas.UseVisualStyleBackColor = true;
            this.btnMas.Click += new System.EventHandler(this.btnMas_Click);
            // 
            // btnRecortar
            // 
            this.btnRecortar.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRecortar.Location = new System.Drawing.Point(376, 195);
            this.btnRecortar.Name = "btnRecortar";
            this.btnRecortar.Size = new System.Drawing.Size(57, 23);
            this.btnRecortar.TabIndex = 12;
            this.btnRecortar.Text = "Recortar";
            this.btnRecortar.UseVisualStyleBackColor = true;
            this.btnRecortar.Click += new System.EventHandler(this.btnRecortar_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(115, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Dirección:";
            this.label3.Visible = false;
            // 
            // txtArchivo
            // 
            this.txtArchivo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtArchivo.Location = new System.Drawing.Point(177, 6);
            this.txtArchivo.Name = "txtArchivo";
            this.txtArchivo.Size = new System.Drawing.Size(417, 20);
            this.txtArchivo.TabIndex = 10;
            this.txtArchivo.Visible = false;
            // 
            // pbxImagen
            // 
            this.pbxImagen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbxImagen.Location = new System.Drawing.Point(17, 35);
            this.pbxImagen.Name = "pbxImagen";
            this.pbxImagen.Size = new System.Drawing.Size(309, 110);
            this.pbxImagen.TabIndex = 1;
            this.pbxImagen.TabStop = false;
            this.pbxImagen.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxImagen_Paint);
            this.pbxImagen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxImagen_MouseDown);
            this.pbxImagen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbxImagen_MouseMove);
            this.pbxImagen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxImagen_MouseUp);
            // 
            // btnExplorarI
            // 
            this.btnExplorarI.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnExplorarI.Location = new System.Drawing.Point(600, 3);
            this.btnExplorarI.Name = "btnExplorarI";
            this.btnExplorarI.Size = new System.Drawing.Size(57, 23);
            this.btnExplorarI.TabIndex = 0;
            this.btnExplorarI.Text = "...";
            this.btnExplorarI.UseVisualStyleBackColor = true;
            this.btnExplorarI.Visible = false;
            this.btnExplorarI.Click += new System.EventHandler(this.btnExplorarI_Click);
            // 
            // RecortarImagen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pnlInferior);
            this.Controls.Add(this.pnlSuperior);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecortarImagen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Recorte de Fotografía";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlSuperior.ResumeLayout(false);
            this.pnlSuperior.PerformLayout();
            this.pnlInferior.ResumeLayout(false);
            this.pnlInferior.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxImagen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlSuperior;
        private System.Windows.Forms.Panel pnlInferior;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMensaje;
        private System.Windows.Forms.Button btnExplorarI;
        public System.Windows.Forms.PictureBox pbxImagen;
        private System.Windows.Forms.TextBox txtArchivo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRecortar;
        private System.Windows.Forms.Button btnMas;
        private System.Windows.Forms.Button btnMenos;
        private System.Windows.Forms.Button btnListo;
    }
}

