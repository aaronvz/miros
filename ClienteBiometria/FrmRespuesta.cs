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

using CapaEN;

namespace ENROLLMENT_V3
{
    public partial class FrmRespuesta: Form
    {
        FUNCIONES funciones = new FUNCIONES();

        private Movimiento respuesta;
        private DataTable dtPersonas;
        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Enter)
                if (btnVerificar.Visible)
                    this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public FrmRespuesta(Movimiento _respuesta, DataTable _dtPersonas)
        {
            respuesta = _respuesta;
            dtPersonas = _dtPersonas;

            InitializeComponent();
        }
        public DataSet ListarRespuestas()
        {
            DataSet ds = funciones.GetDsResultado();

            try
            {
                dgvRespuestas.DataSource = null;

                DataTable dtRespuestas = dtPersonas.Clone();
                //MOSTRANDO PRIMERO REGISTROS CON REVISIÓN SECUNDARIA
                string encabezadoRevision = "Revisión Primaria";
                lrespuestaback.Text = "";
                foreach (CapaEN.Data detalle in respuesta.response.data)
                {
                    DataRow[] drArray = dtPersonas.Select("numerodocumento = '" + detalle.numerodocumento + "'");    
                    DataRow dr = drArray[0];

                    if(detalle.idsegundarevision > 0)
                    {
                        dr["rs"] = pbxWarning.Image;
                        dtRespuestas.LoadDataRow(dr.ItemArray, true);
                        //dgvRespuestas.Columns[2].HeaderText = "Revisión secundaria";
                        encabezadoRevision = "Revisión Secundaria";
                        pnlRespuesta.BackColor = Color.Red;
                        lrespuestaback.Text = "Se hace necesario una revisión secundaria";
                    }
                }
                //MOSTRANDO REGISTROS SIN REVISIÓN SECUNDARIA
                foreach (CapaEN.Data detalle in respuesta.response.data)
                {
                    DataRow[] drArray = dtPersonas.Select("numerodocumento = '" + detalle.numerodocumento + "'");
                    DataRow dr = drArray[0];

                    if (detalle.idsegundarevision == 0)
                    {
                        dr["rs"] = pbxCheck.Image;
                        dtRespuestas.LoadDataRow(dr.ItemArray, true);
                        //gvRespuestas.Columns[2].HeaderText = "Revisión primaria";
                        lrespuestaback.Text = "Bienvenido a Guatemala!!!";
                    }
                }

                dgvRespuestas.DataSource = dtRespuestas;

                dgvRespuestas.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvRespuestas.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvRespuestas.Columns[2].HeaderText = encabezadoRevision;
                dgvRespuestas.Columns[2].Width = 200;
               //dgvRespuestas.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvRespuestas.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvRespuestas.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //ds.Tables[0].Rows[0]["DATOS"] = nSubject.Fingers[0];
            }
            catch (Exception ex)
            {
                //throw new Exception("ListarEscaners(). " + ex.Message);
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ListarPersonas(). " + ex.Message;
            }

            return ds;
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

                lblIdMovimiento.Text = respuesta.Id.ToString() + " (" + respuesta.segundos + ")";

                DataSet dsListarRespuestas = ListarRespuestas();
                if (bool.Parse(dsListarRespuestas.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsListarRespuestas.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
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

        private void rbnPadre_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbnMadre_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbnResponsable_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void lrespuestaback_Click(object sender, EventArgs e)
        {

        }
    }
}
