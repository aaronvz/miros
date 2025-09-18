namespace ENROLLMENT_V3.ControlesDesko
{
    partial class StateLabel
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

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelState
            // 
            this.labelState.BackColor = System.Drawing.Color.LightGray;
            this.labelState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelState.Location = new System.Drawing.Point(0, 0);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(68, 27);
            this.labelState.TabIndex = 3;
            this.labelState.Text = "Off";
            this.labelState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StateLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelState);
            this.Name = "StateLabel";
            this.Size = new System.Drawing.Size(68, 27);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelState;
    }
}
