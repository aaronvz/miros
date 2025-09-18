using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;

using ENROLLMENT_V3.Properties;

namespace ENROLLMENT_V3
{
    public partial class FrmParentesco : Form
    {
        public int parentesco = 0;
        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Enter)
                if (btnVerificar.Visible)
                    this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public FrmParentesco()
        {
            parentesco = 0;
            InitializeComponent();
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {

                this.Close();
                this.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {   
        }

        private void Verificacion_Load(object sender, EventArgs e)
        {
            try
            {
                FUNCIONES funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);

                parentesco = 0;

                txtTramite.Text = txtCriterio.Text = string.Empty;
                CargarDataGridView(dgvParentesco, "Parentesco");
                txtCriterio.Focus();
                txtCriterio.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void CargarDataGridView(DataGridView dgv, string nombre)
        {
            try
            {
                dgv.DataSource = null;
                string ruta = Application.StartupPath + @"\Catalogos\" + nombre + ".xml";
                DataSet ds = new DataSet();
                ds.ReadXml(ruta);
                ds.Tables[0].Columns.Add("Parentesco", typeof(string), "Codigo + ' - ' + Nombre");
                //ds.Tables[0].Columns.Add("Gestion", typeof(string), "Nivel + ' - ' + UnidadAdministrativa + '/' + Codigo");

                dgv.DataSource = ds.Tables[0];
                dgv.Columns[1].Visible = false;
                dgv.Columns[2].Visible = false;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarDataGridView(" + nombre + "). " + ex.Message);
            }
        }

        private void btnVerificar_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string msgError = string.Empty;

                if (msgError.Equals(string.Empty) == false)
                    throw new Exception(msgError);

                this.Close();
                this.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void dgvParentesco_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow dgvr = dgvParentesco.Rows[e.RowIndex];
                txtTramite.Text = dgvr.Cells["Parentesco"].Value.ToString();// + "/" + dgvr.Cells["Codigo"].Value.ToString();

                this.parentesco = int.Parse(dgvr.Cells["codigo"].Value.ToString());

                ((DataGridViewCheckBoxCell)dgvr.Cells[0]).Value = true;

                for (int i = 0; i < dgvParentesco.Rows.Count; i++)
                {
                    if (i == e.RowIndex) continue;
                    ((DataGridViewCheckBoxCell)dgvParentesco.Rows[i].Cells[0]).Value = false;
                }
            }
            catch (Exception ex)
            {
                //txtMensaje.Text = "dgvTramites_CellClick(). " + ex.Message;
                MessageBox.Show("dgvParentesco_CellContentClick(). " + ex.Message);
            }
        }

        private void txtCriterio_TextChanged(object sender, EventArgs e)
        {
            try
            {
                (dgvParentesco.DataSource as DataTable).DefaultView.RowFilter = string.Format("Parentesco LIKE '%{0}%'", txtCriterio.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtCriterio_TextChanged(). " + ex.Message);
            }
        }
    }
}
