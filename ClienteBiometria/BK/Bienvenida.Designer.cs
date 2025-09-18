namespace ENROLLMENT_V3
{
    partial class Bienvenida
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Bienvenida));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAccion = new System.Windows.Forms.Button();
            this.lblListo = new System.Windows.Forms.Label();
            this.pbxLoad = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pic_txt_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.label31 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLoad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel1.Controls.Add(this.btnAccion);
            this.panel1.Controls.Add(this.lblListo);
            this.panel1.Controls.Add(this.pbxLoad);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(63, 106);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(574, 213);
            this.panel1.TabIndex = 31;
            // 
            // btnAccion
            // 
            this.btnAccion.Location = new System.Drawing.Point(230, 181);
            this.btnAccion.Name = "btnAccion";
            this.btnAccion.Size = new System.Drawing.Size(75, 23);
            this.btnAccion.TabIndex = 35;
            this.btnAccion.Text = "Accion";
            this.btnAccion.UseVisualStyleBackColor = true;
            this.btnAccion.Visible = false;
            this.btnAccion.Click += new System.EventHandler(this.BtnAccion_Click);
            // 
            // lblListo
            // 
            this.lblListo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblListo.ForeColor = System.Drawing.Color.White;
            this.lblListo.Location = new System.Drawing.Point(13, 152);
            this.lblListo.Name = "lblListo";
            this.lblListo.Size = new System.Drawing.Size(544, 52);
            this.lblListo.TabIndex = 34;
            this.lblListo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblListo.TextChanged += new System.EventHandler(this.LblListo_TextChanged);
            // 
            // pbxLoad
            // 
            this.pbxLoad.BackColor = System.Drawing.Color.Transparent;
            this.pbxLoad.Image = ((System.Drawing.Image)(resources.GetObject("pbxLoad.Image")));
            this.pbxLoad.Location = new System.Drawing.Point(449, 125);
            this.pbxLoad.Name = "pbxLoad";
            this.pbxLoad.Size = new System.Drawing.Size(24, 24);
            this.pbxLoad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxLoad.TabIndex = 33;
            this.pbxLoad.TabStop = false;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(11, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(544, 52);
            this.label2.TabIndex = 20;
            this.label2.Text = "Por favor espere mientras carga el sistema...      ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(544, 93);
            this.label1.TabIndex = 19;
            this.label1.Text = "¡Bienvenido al Sistema de Captura de Información Biométrica para Emisión de Pasap" +
    "ortes de Guatemala!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pic_txt_dgm
            // 
            this.pic_txt_dgm.Location = new System.Drawing.Point(170, 14);
            this.pic_txt_dgm.Name = "pic_txt_dgm";
            this.pic_txt_dgm.Size = new System.Drawing.Size(361, 82);
            this.pic_txt_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
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
            // Bienvenida
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(695, 353);
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
            this.Name = "Bienvenida";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Load += new System.EventHandler(this.Bienvenida_Load);
            this.Shown += new System.EventHandler(this.Bienvenida_Shown);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxLoad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pic_txt_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbxLoad;
        private System.Windows.Forms.Label lblListo;
        private System.Windows.Forms.Button btnAccion;
    }
}