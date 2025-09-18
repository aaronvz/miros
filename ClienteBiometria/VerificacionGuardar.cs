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
using System.Xml.Serialization;

namespace ENROLLMENT_V3
{
    public partial class VerificacionGuardar : Form
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

        EquipoData equipoData;
        SedeData sedeDataEquipo;

        public DataSet dsVuelos;
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

            if (keyData == Keys.Down)
            {
                if(dgvVuelos.Rows.Count > 0)
                {
                    dgvVuelos.ClearSelection();

                    if (indice + 1 >= dgvVuelos.Rows.Count)
                        indice = -1;
    
                    if (indice == -1)
                    {
                        indice = 0;
                        dgvVuelos.Rows[indice].Selected = true;
                        dgvVuelos.Rows[indice].Cells[0].Value = "true";

                        DataGridViewCellEventArgs eventArgs = new DataGridViewCellEventArgs(0, indice);

                        dgvVuelos_CellContentClick(null, eventArgs);
                    }
                    else if (indice >= 0 && indice <= dgvVuelos.Rows.Count - 1)
                    {
                        indice++;
                        dgvVuelos.Rows[indice].Selected = true;
                        dgvVuelos.Rows[indice].Cells[0].Value = "true";

                        DataGridViewCellEventArgs eventArgs = new DataGridViewCellEventArgs(0, indice);

                        dgvVuelos_CellContentClick(null, eventArgs);
                    }
                }
                
            }

            if (keyData == Keys.Up)
            {
                if (dgvVuelos.Rows.Count > 0)
                {
                    dgvVuelos.ClearSelection();

                    if (indice == 0)
                        indice = dgvVuelos.Rows.Count;

                    if (indice == -1)
                    {
                        indice = dgvVuelos.Rows.Count - 1;
                        dgvVuelos.Rows[indice].Selected = true;
                        dgvVuelos.Rows[indice].Cells[0].Value = "true";

                        DataGridViewCellEventArgs eventArgs = new DataGridViewCellEventArgs(0, indice);

                        dgvVuelos_CellContentClick(null, eventArgs);
                    }
                    else if (indice >= 0 && indice <= dgvVuelos.Rows.Count)
                    {
                        indice--;
                        dgvVuelos.Rows[indice].Selected = true;
                        dgvVuelos.Rows[indice].Cells[0].Value = "true";

                        DataGridViewCellEventArgs eventArgs = new DataGridViewCellEventArgs(0, indice);

                        dgvVuelos_CellContentClick(null, eventArgs);
                    }
                }

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public VerificacionGuardar(LoginData _loginData, NBiometricClient _biometricClient, DataTable _dtPersonas, EquipoData _equipoData, SedeData _sedeDataEquipo)
        {
            indice = -1;
            this.loginData = _loginData;
            biometricClient = _biometricClient;

            equipoData = _equipoData;
            sedeDataEquipo = _sedeDataEquipo;

            dtPersonas = _dtPersonas;

            dsVuelos = new DataSet();

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
                //MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
                funciones.CajaMensaje(ex.Message);
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
                funciones.CajaMensaje(ex.Message);
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

                dgv.Columns[10].HeaderText = "NUMERO DE VUELO";
                dgv.Columns[11].HeaderText = "TRANSPORTE";
                dgv.Columns[12].HeaderText = "DESTINO";

                if (ds.Tables[0].Rows.Count == 1) {
                    dgvVuelos.Rows[0].Selected = true;
                    dgvVuelos.Rows[0].Cells[0].Value = "true";

                    DataGridViewRow dgvr = dgvVuelos.Rows[0];
                    lblIdVuelo.Text = dgvr.Cells["idvuelo"].Value.ToString();
                    txtTramite.Text = dgvr.Cells["descripcionvuelo"].Value.ToString();
                }

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

                txtTramite.Text = txtCriterio.Text = string.Empty;
                CargarDataGridView(dgvVuelos, "Vuelo");
                txtCriterio.Focus();
                txtCriterio.Select();

                wsBiometricsDGM = new WsBiometricsDGMSoapClient();

                DataSet dsListarPersonas = ListarPersonas();
                if (bool.Parse(dsListarPersonas.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsListarPersonas.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje(ex.Message);
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
                funciones.CajaMensaje(ex.Message);
            }
        }

        private void txtCriterio_TextChanged(object sender, EventArgs e)
        {
            try
            {
                (dgvVuelos.DataSource as DataTable).DefaultView.RowFilter = string.Format("descripcionvuelo LIKE '%{0}%'", txtCriterio.Text.Trim());
                if (dgvVuelos.Rows.Count == 1)
                {
                    dgvVuelos.Rows[0].Selected = true;

                    DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)dgvVuelos.Rows[0].Cells[0];
                    chk.Value = true;

                    DataGridViewCellEventArgs eventArgs = new DataGridViewCellEventArgs(0, 0);

                    dgvVuelos_CellContentClick(null, eventArgs);

                }
                else
                {
                    dgvVuelos.ClearSelection();
                    lblIdVuelo.Text = string.Empty;
                    txtTramite.Text = string.Empty;
                }
                    
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje(ex.Message);
            }
        }

        private void btnVerificar_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                string msgError = string.Empty;

                if (txtTramite.Text.Trim().Equals(string.Empty) || txtTramite.Text.Trim().Equals(""))
                    msgError += "Seleccione un vuelo. ";

                if (msgError.Equals(string.Empty) == false)
                    throw new Exception(msgError);

                VerificacionValida = true;
                vuelo = lblIdVuelo.Text + "/" + txtTramite.Text;

                icaopaisorigen = this.lblIcaoPaisOrigen.Text;
                icaopaisdestino = this.lblIcaoPaisDestino.Text;

                this.Close();
                return;
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje(ex.Message);
            }
        }

        private void dgvVuelos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow dgvr = dgvVuelos.Rows[e.RowIndex];
                lblIdVuelo.Text = dgvr.Cells["idvuelo"].Value.ToString();
                txtTramite.Text = dgvr.Cells["descripcionvuelo"].Value.ToString();// + "/" + dgvr.Cells["Codigo"].Value.ToString();

                this.paisDestino = dgvr.Cells["idpaisdestino"].Value.ToString();
                this.ciudadDestino = dgvr.Cells["idciudaddestino"].Value.ToString();

                //this.lblIcaoPaisOrigen.Text = dgvr.Cells["icaopaisorigen"].Value.ToString();
                //this.lblIcaoPaisDestino.Text = dgvr.Cells["icaopaisdestino"].Value.ToString();

                if (sender != null)
                {
                    dgvVuelos.ClearSelection();
                    dgvVuelos.Rows[e.RowIndex].Selected = true;
                    ((DataGridViewCheckBoxCell)dgvr.Cells[0]).Value = true;
                }


                for (int i = 0; i < dgvVuelos.Rows.Count; i++)
                {
                    if (i == e.RowIndex) continue;
                    ((DataGridViewCheckBoxCell)dgvVuelos.Rows[i].Cells[0]).Value = false;
                }

                if (sender != null)
                    indice = e.RowIndex;
            }
            catch (Exception ex)
            {
                //txtMensaje.Text = "dgvTramites_CellClick(). " + ex.Message;
                funciones.CajaMensaje(ex.Message);
            }
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
                funciones.CajaMensaje(ex.Message);
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
                funciones.CajaMensaje(ex.Message);
            }
        }

        private void pbxVuelos_MouseClick(object sender, MouseEventArgs e)
        {

        }

        public DataSet GetVuelos()
        {
            MovimientoDiagnostico mv = new MovimientoDiagnostico();
            DateTime fechaIni = DateTime.Now;
            DateTime fechaFin = DateTime.Now;

            string jsonString = "";
            string body = "";

            DataSet dsResultado = ArmarDsResultado();
            try
            {
                VueloRequest vueloRequest = new VueloRequest();
                vueloRequest.id = 0;
                vueloRequest.tipo = equipoData.nombre_tipo_flujo.Substring(0, 1);//Settings.Default.TIPO_MOVIMIENTO.Substring(0, 1); //"R";// 
                vueloRequest.codigodelegacion = sedeDataEquipo.clave;

                jsonString = JsonConvert.SerializeObject(vueloRequest);

                byte[] data = UTF8Encoding.UTF8.GetBytes(jsonString);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.API_REST_MIROS + Settings.Default.API_GET_VUELOS);
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        fechaFin = DateTime.Now;
                        if (strReader == null)
                            throw new Exception("Respuesta nula desde el servidor. ");

                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            body = objReader.ReadToEnd();
                            VueloResponse vueloResponse = JsonConvert.DeserializeObject<VueloResponse>(body);

                            if (vueloResponse.codigo != 200)
                                throw new Exception("Error al guardar la entrega. Código: " + vueloResponse.codigo + ", Mensaje: " + vueloResponse.mensaje);

                            if (vueloResponse.data == null)
                                throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                            string ruta = Application.StartupPath + @"\Catalogos\Vuelo.xml";

                            ListaVuelo lista = new ListaVuelo();
                            lista.Nodo = new List<NodoVuelo>();

                            foreach (VueloData vueloData in vueloResponse.data)
                            {
                                NodoVuelo nodo = new NodoVuelo();
                                nodo.idvuelo = vueloData.idvuelo;
                                nodo.idtransporte = vueloData.idtransporte;
                                nodo.idciudaddestino = vueloData.idciudaddestino;
                                nodo.idpaisorigen = vueloData.idpaisorigen;
                                nodo.idciudadorigen = vueloData.idciudadorigen;
                                nodo.nombrepaisorigen = vueloData.nombrepaisorigen;
                                nodo.icaopaisorigen = vueloData.icaopaisorigen;
                                nodo.icaopaisdestino = vueloData.icaopaisdestino;
                                nodo.idpaisdestino = vueloData.idpaisdestino;
                                nodo.nombrepaisdestino = vueloData.nombrepaisdestino;
                                nodo.descripcionvuelo = vueloData.descripcionvuelo;
                                nodo.transporte = vueloData.transporte;
                                lista.Nodo.Add(nodo);
                            }

                            var encoding = Encoding.GetEncoding("ISO-8859-1");
                            var serializer = new XmlSerializer(typeof(ListaVuelo));
                            using (var writer = new StreamWriter(ruta, false, encoding))
                            {
                                serializer.Serialize(writer, lista);
                            }

                            dsVuelos = new DataSet();
                            dsVuelos.ReadXml(ruta);

                            dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                            dsResultado.Tables[0].Rows[0]["DATOS"] = dsVuelos.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetVuelos(). " + ex.Message;
            }

            if (Settings.Default.MODO_DIAGNÓSTICO)
            {
                mv.id_movimiento = null;
                mv.tipo_documento = string.Empty;
                mv.numero_documento = string.Empty;
                mv.nombre = string.Empty;
                mv.fecha_nacimiento = string.Empty;
                mv.comando = "GetVuelos()";
                mv.segundos = 0;
                mv.fecha_ini = fechaIni;
                mv.fecha_fin = fechaFin;
                mv.request = jsonString;
                mv.response = body;

                ReportesDB reporte = new ReportesDB();
                reporte.InsertarRegistro(mv);
            }

            return dsResultado;
        }

        private void btnVuelos_MouseClick(object sender, MouseEventArgs e)
        {
            this.Enabled = false;
            try
            {
                DataSet dsVuelos = this.GetVuelos();
                if (bool.Parse(dsVuelos.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsVuelos.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                lblIdVuelo.Text = string.Empty;
                txtTramite.Text = txtCriterio.Text = string.Empty;
                CargarDataGridView(dgvVuelos, "Vuelo");
                txtCriterio.Focus();
                txtCriterio.Select();

            }
            catch (Exception ex)
            {
                funciones.CajaMensaje(ex.Message);
            }
            this.Enabled = true;
        }

        private void dgvPersonas_Click(object sender, EventArgs e)
        {

        }

        private void btnVerificar_Click(object sender, EventArgs e)
        {

        }
    }
}
