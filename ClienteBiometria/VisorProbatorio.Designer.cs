namespace ENROLLMENT_V3
{
    partial class VisorProbatorio
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
            this.pbxProbatorio = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxProbatorio)).BeginInit();
            this.SuspendLayout();
            // 
            // pbxProbatorio
            // 
            this.pbxProbatorio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxProbatorio.Location = new System.Drawing.Point(0, 0);
            this.pbxProbatorio.Name = "pbxProbatorio";
            this.pbxProbatorio.Size = new System.Drawing.Size(800, 450);
            this.pbxProbatorio.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxProbatorio.TabIndex = 0;
            this.pbxProbatorio.TabStop = false;
            // 
            // VisorProbatorio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pbxProbatorio);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VisorProbatorio";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ver Probatorio";
            this.Load += new System.EventHandler(this.VisorProbatorio_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxProbatorio)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox pbxProbatorio;
    }
}