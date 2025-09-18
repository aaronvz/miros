using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;

//COMPONENTES DEL SDK DE TRABAJO
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;

using System.IO;
using Newtonsoft.Json;
using CapaEN;
using ENROLLMENT_V3.WsBiometricsDGM;

using System.Reflection;

using ENROLLMENT_V3.Properties;

namespace ENROLLMENT_V3
{
    public partial class VerificacionGuardarTerrestre : Form
    {
        #region Declaración de variables para utilización del SDK

        //Administrador de dispositivos
        private NDeviceManager _deviceManager;
        //Cliente biométrico
        private NBiometricClient biometricClient;

        //Sujeto
        private NSubject _subjectA;
        private NSubject _subjectB;

        //Dedo
        private NFinger nFingerA;

        #endregion       

        FUNCIONES funciones = new FUNCIONES();
        WsBiometricsDGMSoapClient wsBiometricsDGM;

        public bool VerificacionValida;
        public string vuelo;
        public string paisDestino;
        public string ciudadDestino;

        public string icaopaisorigen;
        public string icaopaisdestino;

        private int indice;

        LoginData loginData;

        private DataTable dtPersonas;

        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Enter)
                if (btnVerificar.Visible)
                {
                    this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                    this.Dispose();
                    this.Close();

                }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public VerificacionGuardarTerrestre(LoginData _loginData, NBiometricClient _biometricClient, DataTable _dtPersonas)
        {
            indice = -1;
            this.loginData = _loginData;
            biometricClient = _biometricClient;

            dtPersonas = _dtPersonas;

            if (biometricClient != null)
            {
                try
                {
                    biometricClient.Cancel();
                }
                catch (Exception)
                {

                    throw;
                }
            }

            InitializeComponent();
            VerificacionValida = false;
        }
        
        public DataSet ListarPersonas()
        {
            DataSet ds = funciones.GetDsResultado();

            try
            {
                dgvPersonas.DataSource = null;
                dgvPersonas.DataSource = dtPersonas;

                dgvPersonas.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvPersonas.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvPersonas.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvPersonas.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvPersonas.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                dgvPersonas.Columns[2].Visible = false;

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

        private void ListarEscaners()
        {
            try
            {
                cmbEscaners.Items.Clear();

                btnSalir.Enabled = false;

                if (_deviceManager != null)
                    foreach (NDevice item in _deviceManager.Devices)
                        cmbEscaners.Items.Add(item);

                if (cmbEscaners.Items.Count == 1)
                    cmbEscaners.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private async void NuevoScan()
        {
            try
            {
                //txtUsuario.Text = txtContrasenia.Text = string.Empty;

                //_deviceManager = biometricClient.DeviceManager;
                //ListarEscaners();

                btnSalir.Enabled = true;

                if (biometricClient.FingerScanner == null)
                    throw new Exception(@"Por favor, seleccione un escáner de la lista.");

                // Create a objeto tipo NFinger
                nFingerA = new NFinger();
                nFingerA.Position = NFPosition.RightIndex;

                // Agregar un dedo al sujeto y a la vista
                _subjectA = new NSubject();
                _subjectA.Fingers.Add(nFingerA);

                nFVDedoA.Finger = nFingerA;
                nFVDedoA.ShownImage = ShownImage.Original;

                nFVDedoB.Finger = new NFinger();
                nFVDedoB.ShownImage = ShownImage.Original;

                cmbEscaners.Enabled = false;
                // Begin capturing
                NBiometricTask task = biometricClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, _subjectA);
                var performedTask = await biometricClient.PerformTaskAsync(task);
                ComprobarStatusExtraccion(performedTask.Status);
                nFVDedoA.Finger = _subjectA.Fingers[0];
                cmbEscaners.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }            
        }

        private async System.Threading.Tasks.Task<Tuple<byte[], NSubject>> GenerarPlantillaImagenAsync(byte[] bytes)
        {
            NSubject nsSujeto = null;

            try
            {
                // Create a finger object
                var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);

                // Extract a template from the subject
                biometricClient.FingersReturnBinarizedImage = true;

                var status = await biometricClient.CreateTemplateAsync(nsSujeto);
                ComprobarStatusExtraccion(status);
            }
            catch (Exception ex)
            {

                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). Excepcion: " + ex.Message + ". InnerException: " + ex.InnerException);
            }
            /*finally
            {
                EnableCheckBoxes();
            }*/
            return new Tuple<byte[], NSubject>(bytes, nsSujeto);
        }

        private async System.Threading.Tasks.Task<Tuple<byte[], NSubject>> GenerarPlantillaBioAsync(byte[] bytes)
        {
            NSubject nsSujeto = null;
            try
            {
                // Create a finger object
                var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);

                // Extract a template from the subject
                biometricClient.FingersReturnBinarizedImage = true;

                var status = await biometricClient.CreateTemplateAsync(nsSujeto);
                ComprobarStatusExtraccion(status);
            }
            catch (Exception ex)
            {

                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). Excepcion: " + ex.Message + ". InnerException: " + ex.InnerException);
            }
            /*finally
            {
                EnableCheckBoxes();
            }*/
            return new Tuple<byte[], NSubject>(bytes, nsSujeto);
            //return new Tuple<DataSet> (new DataSet());
        }

        private void MostrarHuellaDesdeBytes(byte[] bytes, NFingerView nfvHuella)
        {
            try
            {
                nfvHuella.Finger = null;

                // Create a finger object
                var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                nfvHuella.Finger = nfHuella;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). Excepcion: " + ex.Message + ". InnerException: " + ex.InnerException);
            }
            
        }

        public void NuevaInstanciaEnrollment()
        {
            try
            {
                FrmEnrolamiento er = new FrmEnrolamiento(loginData, null, null, null, null);
                this.Hide();
                er.ShowDialog();
                this.Close();
            }
            catch(Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("DATOS", typeof(object));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }

        public DataSet ConsultaInformacionxUsuario(string usuario, string contrasenia)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {

                InfoWsUsuarioDGMConsulta vParametrosUsuario = new InfoWsUsuarioDGMConsulta();
                vParametrosUsuario.usuario = usuario;
                vParametrosUsuario.clave = contrasenia;

                string postString = JsonConvert.SerializeObject(vParametrosUsuario);

                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest request;
                request = WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/enrollment_auth_user") as HttpWebRequest;
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);


                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                InfoWsUsuarioDGM clsUsuario;
                clsUsuario = JsonConvert.DeserializeObject<InfoWsUsuarioDGM>(body);

                dsResultado.Tables[0].Rows[0]["DATOS"] = clsUsuario;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = MethodBase.GetCurrentMethod().Name + "(). " + ex.Message;
            }

            return dsResultado;
        }

        private void ComprobarStatusExtraccion(NBiometricStatus status)
        {
            try
            {
                if (status != NBiometricStatus.Ok && status != NBiometricStatus.Canceled)
                {
                    NuevoScan();
                    throw new Exception("¡No fue posible realizar la extracción!, Estatus: " + status.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {
                if (biometricClient != null) if(biometricClient.CurrentBiometric != null) biometricClient.Cancel();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (biometricClient != null) if(biometricClient.CurrentBiometric != null) biometricClient.Cancel();
        }

        private void CargarDataGridView(DataGridView dgv, string nombre)
        {
            try
            {
                dgv.DataSource = null;
                string ruta = Application.StartupPath + @"\Catalogos\" + nombre + ".xml";
                DataSet ds = new DataSet();
                ds.ReadXml(ruta);
                
                dgv.DataSource = ds.Tables[0];
                //dgv.Columns[0].Visible = false;
                dgv.Columns[1].Visible = false;
                dgv.Columns[2].Visible = false;
                dgv.Columns[3].Visible = false;
                dgv.Columns[4].Visible = false;
                dgv.Columns[5].Visible = false;
                dgv.Columns[6].Visible = false;
                dgv.Columns[7].Visible = false;
                dgv.Columns[8].Visible = false;
                dgv.Columns[9].Visible = false;

                dgv.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgv.Columns[10].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            }
            catch (Exception ex)
            {
                throw new Exception("CargarDataGridView(" + nombre + "). " + ex.Message);
            }
        }

        private void Verificacion_Load(object sender, EventArgs e)
        {
            try
            {
                FUNCIONES funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);
                funciones.CargarLogo(pictureBox1);

                DataSet dsListarPersonas = ListarPersonas();
                if (bool.Parse(dsListarPersonas.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsListarPersonas.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void CmbEscaners_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                biometricClient.FingerScanner = cmbEscaners.SelectedItem as NFScanner;
                NuevoScan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("CmbEscaners_SelectedIndexChanged(). " + ex.Message);
            }
        }

        private void txtCriterio_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnVerificar_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string msgError = string.Empty;

                icaopaisorigen = this.lblIcaoPaisOrigen.Text;
                icaopaisdestino = this.lblIcaoPaisDestino.Text;

                this.Close();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void dgvVuelos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void txtCriterio_Click(object sender, EventArgs e)
        {
            indice = -1;
        }

        private void dgvVuelos_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (btnVerificar.Visible)
                        this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("dgvVuelos_PreviewKeyDown. " + ex.Message);
            }
        }

        private void txtCriterio_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    if (btnVerificar.Visible)
                        this.btnVerificar_MouseClick(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtCriterio_KeyPress. " + ex.Message);
            }
        }
    }
}
