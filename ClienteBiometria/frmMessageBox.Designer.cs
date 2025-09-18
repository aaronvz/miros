namespace ENROLLMENT_V3
{
    partial class frmMessageBox
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

        public void setText(string mensaje)
        {
            lmensaje.Text = mensaje;
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.pbxWarning = new System.Windows.Forms.PictureBox();
            this.lmensaje = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(220, 89);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(159, 34);
            this.button1.TabIndex = 1;
            this.button1.Text = "Aceptar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pbxWarning
            // 
            this.pbxWarning.BackColor = System.Drawing.Color.Transparent;
            this.pbxWarning.Image = global::ENROLLMENT_V3.Properties.Resources.happy_removebg_preview;
            this.pbxWarning.Location = new System.Drawing.Point(13, 13);
            this.pbxWarning.Margin = new System.Windows.Forms.Padding(4);
            this.pbxWarning.Name = "pbxWarning";
            this.pbxWarning.Size = new System.Drawing.Size(54, 41);
            this.pbxWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxWarning.TabIndex = 32;
            this.pbxWarning.TabStop = false;
            // 
            // lmensaje
            // 
            this.lmensaje.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lmensaje.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lmensaje.ForeColor = System.Drawing.Color.Red;
            this.lmensaje.Location = new System.Drawing.Point(74, 13);
            this.lmensaje.Multiline = true;
            this.lmensaje.Name = "lmensaje";
            this.lmensaje.ReadOnly = true;
            this.lmensaje.Size = new System.Drawing.Size(481, 70);
            this.lmensaje.TabIndex = 33;
            this.lmensaje.Text = "Mensaje";
            this.lmensaje.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // frmMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 134);
            this.Controls.Add(this.lmensaje);
            this.Controls.Add(this.pbxWarning);
            this.Controls.Add(this.button1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMessageBox";
            this.Text = "Información";
            ((System.ComponentModel.ISupportInitialize)(this.pbxWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pbxWarning;
        private System.Windows.Forms.TextBox lmensaje;
    }
}