using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaEN;

namespace ENROLLMENT_V3
{
    public partial class VisorAlertas : Form
    {
        public VisorAlertas(DPI dpi, DataTable dtArraigos, DataTable dtAlertas, string error)
        {
            InitializeComponent();

            lblCuiDPI.Text = dpi.CUI;

            lblNombre.Text = dpi.PRIMER_NOMBRE + ((dpi.SEGUNDO_NOMBRE != null && dpi.SEGUNDO_NOMBRE != "" && dpi.SEGUNDO_NOMBRE != string.Empty) ? " " + dpi.SEGUNDO_NOMBRE : "") + ((dpi.TERCER_NOMBRE != null && dpi.TERCER_NOMBRE != "" && dpi.TERCER_NOMBRE != string.Empty) ? " " + dpi.TERCER_NOMBRE : "") + " ";
            lblApellido.Text = dpi.PRIMER_APELLIDO + ((dpi.SEGUNDO_APELLIDO != null && dpi.SEGUNDO_APELLIDO != "" && dpi.SEGUNDO_APELLIDO != string.Empty) ? " " + dpi.SEGUNDO_APELLIDO : "");

            lblFechaNacimiento.Text = dpi.FECHA_NACIMIENTO;
            lblGenero.Text = dpi.SEXO;            
            pbxFotoDPI.Image = dpi.IMAGE;

            dgvArraigos.DataSource = dtArraigos;
            dgvAlertas.DataSource = dtAlertas;

            lblError.Text = error;
        }

        private void VisorAlertas_Load(object sender, EventArgs e)
        {
            try
            {
                FUNCIONES funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);

            }
            catch (Exception ex)
            {
                MessageBox.Show("VisorAlertas_Load(). " + ex.Message);
            }            
        }
    }
}
