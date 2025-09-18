namespace ENROLLMENT_V3
{
    partial class FrmLoader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLoader));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pic_logo_dgm = new System.Windows.Forms.PictureBox();
            this.picb_logo = new System.Windows.Forms.PictureBox();
            this.pic_txt_dgm = new System.Windows.Forms.PictureBox();
            this.pbxLogin = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogin)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.pbxLogin);
            this.panel1.Controls.Add(this.lblTitulo);
            this.panel1.Location = new System.Drawing.Point(84, 130);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(819, 328);
            this.panel1.TabIndex = 31;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft YaHei", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 3);
            this.lblTitulo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(819, 42);
            this.lblTitulo.TabIndex = 131;
            this.lblTitulo.Text = "ESPERE POR FAVOR...";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pic_logo_dgm
            // 
            this.pic_logo_dgm.BackColor = System.Drawing.Color.Transparent;
            this.pic_logo_dgm.Image = ((System.Drawing.Image)(resources.GetObject("pic_logo_dgm.Image")));
            this.pic_logo_dgm.Location = new System.Drawing.Point(84, 17);
            this.pic_logo_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_logo_dgm.Name = "pic_logo_dgm";
            this.pic_logo_dgm.Size = new System.Drawing.Size(135, 101);
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
            this.picb_logo.Size = new System.Drawing.Size(247, 106);
            this.picb_logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picb_logo.TabIndex = 33;
            this.picb_logo.TabStop = false;
            // 
            // pic_txt_dgm
            // 
            this.pic_txt_dgm.Location = new System.Drawing.Point(227, 17);
            this.pic_txt_dgm.Margin = new System.Windows.Forms.Padding(4);
            this.pic_txt_dgm.Name = "pic_txt_dgm";
            this.pic_txt_dgm.Size = new System.Drawing.Size(421, 101);
            this.pic_txt_dgm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_txt_dgm.TabIndex = 32;
            this.pic_txt_dgm.TabStop = false;
            // 
            // pbxLogin
            // 
            this.pbxLogin.BackColor = System.Drawing.Color.White;
            this.pbxLogin.Image = ((System.Drawing.Image)(resources.GetObject("pbxLogin.Image")));
            this.pbxLogin.Location = new System.Drawing.Point(293, 80);
            this.pbxLogin.Margin = new System.Windows.Forms.Padding(4);
            this.pbxLogin.Name = "pbxLogin";
            this.pbxLogin.Size = new System.Drawing.Size(180, 180);
            this.pbxLogin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxLogin.TabIndex = 35;
            this.pbxLogin.TabStop = false;
            // 
            // FrmLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(968, 511);
            this.ControlBox = false;
            this.Controls.Add(this.pic_logo_dgm);
            this.Controls.Add(this.picb_logo);
            this.Controls.Add(this.pic_txt_dgm);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLoader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ingreso_FormClosing);
            this.Load += new System.EventHandler(this.Verificacion_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pic_logo_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picb_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_txt_dgm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxLogin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pic_txt_dgm;
        private System.Windows.Forms.PictureBox picb_logo;
        private System.Windows.Forms.PictureBox pic_logo_dgm;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.PictureBox pbxLogin;
    }
}