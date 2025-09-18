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
using CapaLN;

namespace ENROLLMENT_V3
{
    public partial class FrmDeclaracion : Form
    {
        DeclaracionLN declaracionLN;
        
        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Enter)
                if (btnVerificar.Visible)
                    this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public FrmDeclaracion(DeclaracionData declaracionData, string _url, LoginData _loginData, string _declaracionToken)
        {
            declaracionLN = new DeclaracionLN(_url, _loginData, _declaracionToken);
            InitializeComponent();

            txtCriterio.Enabled = true;
            lblTipoMovimiento.Text = "";
            lblTransporte.Text = "";
            lblProcedencia.Text = "";
            lblNombres.Text = "";
            lblTipoDocumento.Text = "";
            lblNacionalidad.Text = "";
            lblFechaNacimiento.Text = "";

            lblFecha.Text = "";
            lblVuelo.Text = "";
            lblDestino.Text = "";
            lblApellidos.Text = "";
            lblNumeroDocumento.Text = "";
            lblSexo.Text = "";
            lblEmbarcacion.Text = "";

            dgvAcompañantes.DataSource = null;

            if (declaracionData == null)
                declaracionData = new DeclaracionData();

            if (declaracionData.id > 0)
                llenarCampos(declaracionData);
        }

        private void llenarCampos(DeclaracionData declaracionData)
        {
            txtCriterio.Enabled = false;
            txtCriterio.Text = declaracionData.correlativo;
            lblTipoMovimiento.Text = declaracionData.viaje.tipoMovimiento;
            lblTransporte.Text = declaracionData.viaje.empresaTrans;
            lblProcedencia.Text = declaracionData.viaje.paisProcedencia.id + " - " + declaracionData.viaje.paisProcedencia.nombre;
            lblNombres.Text = declaracionData.persona.primerNombre;
            lblTipoDocumento.Text = "PENDIENTE";// declaracionData.persona;
            lblNacionalidad.Text = declaracionData.persona.nacionalidad.id + " - " + declaracionData.persona.nacionalidad.nombre;
            lblFechaNacimiento.Text = declaracionData.persona.fechaNacimiento.ToString("dd/MM/yyyy");

            lblFecha.Text = declaracionData.fechaMovimiento.ToString("dd/MM/yyyy");
            lblVuelo.Text = declaracionData.viaje.noViaje;
            lblDestino.Text = declaracionData.viaje.paisDestino.id + " - " + declaracionData.viaje.paisDestino.nombre;
            lblApellidos.Text = declaracionData.persona.primerApellido;
            lblNumeroDocumento.Text = declaracionData.persona.noDocumento;
            lblSexo.Text = declaracionData.persona.sexo;
            lblEmbarcacion.Text = declaracionData.viaje.noVueloEmb;

            CargarDataGridView(dgvAcompañantes, declaracionData.acompanantes.ToArray());
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

                txtCriterio.Focus();
                txtCriterio.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void CargarDataGridView(DataGridView dgv, DeclaracionAcompaniante[] acompaniantes)
        {
            try
            {
                dgv.DataSource = null;

                if (acompaniantes.Length < 1)
                    return;

                DataTable dtAcompaniantes = new DataTable();
                dtAcompaniantes.Columns.Add("No.", typeof(int));
                dtAcompaniantes.Columns.Add("NumeroDocumento", typeof(string));
                dtAcompaniantes.Columns.Add("Nombre", typeof(string));

                for (int i = 1; i <= acompaniantes.Length; i++)
                {
                    DataRow dr = dtAcompaniantes.NewRow();
                    dr["No."] = i;
                    dr["NumeroDocumento"] = acompaniantes[i - 1].numeroDoc;
                    dr["Nombre"] = acompaniantes[i - 1].nombre;
                    dtAcompaniantes.Rows.Add(dr);
                    
                }

                dgv.DataSource = dtAcompaniantes;
                dgv.Refresh();

            }
            catch (Exception ex)
            {
                throw new Exception("CargarDataGridView(). " + ex.Message);
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

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!txtCriterio.Enabled)
                    throw new Exception("Ya existe una declaración asociada. ");

                if (txtCriterio.Text.Trim().Equals(string.Empty))
                    throw new Exception("Ingrese una declaración. ");

                declaracionLN.SetUrl(Settings.Default.API_DECLARACION_BY_NUMERO);

                bool resultado = declaracionLN.GetByNumero(txtCriterio.Text.Trim());
                if (!resultado)
                    throw new Exception(declaracionLN.GetError());

                llenarCampos(declaracionLN.GetData());
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnBuscar_Click(). " + ex.Message);
            }
        }
    }
}
