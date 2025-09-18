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
using ENROLLMENT_V3.WsBiometricsDGMSoap;

using System.Reflection;

using ENROLLMENT_V3.Properties;

namespace ENROLLMENT_V3
{
    public partial class Verificacion : Form
    {
        FUNCIONES funciones = new FUNCIONES();
        List<DataWsUsuariosDGM> listDataWsUsuariosDGM;
        WsBiometricsDGMSoapClient wsBiometricsDGM;
        public Verificacion()
        {
            InitializeComponent();
            txtUsuario.Text = string.Empty;
            txtContrasenia.Text = string.Empty;
            
        }

        #region Declaración de variables para utilización del SDK

        //Administrador de dispositivos
        private NDeviceManager _deviceManager;
        //Cliente biométrico
        private NBiometricClient _biometricClient;
        
        //Sujeto
        private NSubject _subjectA;
        private NSubject _subjectB;
        
        //Dedo
        private NFinger nFingerA;

        #endregion       

        private void ListarEscaners()
        {
            try
            {
                cmbEscaners.Items.Clear();

                if (_deviceManager != null)
                    foreach (NDevice item in _deviceManager.Devices)
                        cmbEscaners.Items.Add(item);

                if (cmbEscaners.Items.Count == 1)
                {
                    cmbEscaners.SelectedIndex = 0;
                    cmbEscaners.Enabled = false;
                }
                else if(cmbEscaners.Items.Count >= 2)
                {
                    cmbEscaners.Enabled = true;
                }

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
                btnVerificar.Enabled = txtUsuario.Enabled = txtContrasenia.Enabled = true;

                _deviceManager = _biometricClient.DeviceManager;
                ListarEscaners();

                if (_biometricClient.FingerScanner == null)
                    throw new Exception(@"Por favor, seleccione un escáner de la lista.");

                // Create a objeto tipo NFinger
                nFingerA = new NFinger();

                // Agregar un dedo al sujeto y a la vista
                _subjectA = new NSubject();
                _subjectA.Fingers.Add(nFingerA);

                nFVDedoA.Finger = nFingerA;
                nFVDedoA.ShownImage = ShownImage.Original;

                nFVDedoB.Finger = new NFinger();
                nFVDedoB.ShownImage = ShownImage.Original;

                // Begin capturing
                NBiometricTask task = _biometricClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, _subjectA);
                var performedTask = await _biometricClient.PerformTaskAsync(task);
                ComprobarStatusExtraccion(performedTask.Status);
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
                _biometricClient.FingersReturnBinarizedImage = true;

                var status = await _biometricClient.CreateTemplateAsync(nsSujeto);
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
                _biometricClient.FingersReturnBinarizedImage = true;

                var status = await _biometricClient.CreateTemplateAsync(nsSujeto);
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

        private async void btnVerificar_Click(object sender, EventArgs e)
        {
            try
            {
                string msgError = string.Empty;

                if (txtUsuario.Text.Trim().Equals(string.Empty) || txtUsuario.Text.Trim().Equals(""))
                    msgError += "Ingrese un usuario. ";

                if (txtContrasenia.Text.Trim().Equals(string.Empty) || txtContrasenia.Text.Trim().Equals(""))
                    msgError += "Ingrese constraseña. ";

                if(_subjectA.Fingers[0].Status != NBiometricStatus.Ok)
                    msgError += "Huella no encontrada. ";

                if (msgError.Equals(string.Empty) == false)
                    throw new Exception(msgError);

                string strContrasenia = funciones.CalculateMD5Hash(txtContrasenia.Text);
                DataSet dsUsuario = ConsultaInformacionxUsuario(txtUsuario.Text, strContrasenia);

                if (bool.Parse(dsUsuario.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsUsuario.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                InfoWsUsuarioDGM infoWsUsuarioDGM = (InfoWsUsuarioDGM)(dsUsuario.Tables[0].Rows[0]["DATOS"]);
                listDataWsUsuariosDGM = infoWsUsuarioDGM.data;

                if (listDataWsUsuariosDGM.Count == 0)
                    throw new Exception("¡Usuario o contraseña no válidos!");

                if (listDataWsUsuariosDGM[0].bloqueado.ToString().Equals("1"))
                    throw new Exception("¡El usuario se encuentra inactivado!");

                if (listDataWsUsuariosDGM[0].cambioclave == null)
                    listDataWsUsuariosDGM[0].cambioclave = "0";

                Properties.Settings.Default.DRIVE_LETTER = null;
                if (Properties.Settings.Default.USB_DEVICE)
                {
                    if (listDataWsUsuariosDGM[0].tbl_ms_serial_number == null)
                        throw new Exception("¡Sin dispositivo de cifrado asignado!. ");

                    funciones.AsignarDriveLetter(listDataWsUsuariosDGM[0].tbl_ms_serial_number);
                    Properties.Settings.Default.SUFIJO_ENCRIPTACION = "U";
                }
                else
                {
                    Properties.Settings.Default.DRIVE_LETTER = Path.Combine(Application.StartupPath, "ENROL");
                    Properties.Settings.Default.SUFIJO_ENCRIPTACION = "M";
                }

                DataSet dsDirectorioLlaves = funciones.ExisteDirectorioLlaves(listDataWsUsuariosDGM[0].usuario, Environment.MachineName);

                if (bool.Parse(dsDirectorioLlaves.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsDirectorioLlaves.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                if (Properties.Settings.Default.TEST)
                    NuevaInstanciaEnrollment();

                //MessageBox.Show("Eliminar TEST, Eliminar Envía FTP, Verificar que todos los combos puedan seleccionarse con las flechas del cursor, Verificar que cuando el país de nacimiento sea distinto a Guatemala, valide el estado de nacimiento verficandolo en la impresión, Eliminar el país de nacimiento 'Guatemala', cuando sea pasaporte Diplomatico Artículo 98, Cuando es CASADO pide apellido de casada, Al seleccinar CASADA focus en txtApellidoCasada");
                //bool reiniciarContraseña = false;
                //listDataWsUsuariosDGM[0].activo = "0";
                //SI NO HAY BIOMETRÍA O ESTA MARCADA LA OPCION PARA CAMBIO DE CONTRASEÑA
                if (listDataWsUsuariosDGM[0].activo == null || listDataWsUsuariosDGM[0].activo.Equals("0") || listDataWsUsuariosDGM[0].cambioclave.Equals("1"))
                {
                    Usuarios frmUsuario = new Usuarios(listDataWsUsuariosDGM);
                    this.Hide();

                    frmUsuario.ShowDialog();
                    this.Close();
                }//USUARIO NO ESTÁ BLOQUEADO Y LA BIOMETRÍA ESTÁ ACTIVA
                else if (listDataWsUsuariosDGM[0].bloqueado.ToString().Equals("0") && listDataWsUsuariosDGM[0].activo.Equals("1") && (listDataWsUsuariosDGM[0].cambioclave.Equals("0")))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        string huellaLocal = Convert.ToBase64String(_subjectA.Fingers[0].Image.Save(NImageFormat.Wsq).ToArray()); 
                        
                        if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                        {
                            byte[] bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella1wsq);
                            var resultTuple = await GenerarPlantillaImagenAsync(bytes);

                            MostrarHuellaDesdeBytes(Convert.FromBase64String(listDataWsUsuariosDGM[0].huella1wsq), nFVDedoB);
                            DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaLocal, listDataWsUsuariosDGM[0].huella1wsq);

                            DataSet dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                            if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                            dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                            if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                            {
                                bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella2wsq);
                                resultTuple = await GenerarPlantillaImagenAsync(bytes);

                                MostrarHuellaDesdeBytes(Convert.FromBase64String(listDataWsUsuariosDGM[0].huella2wsq), nFVDedoB);
                                dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaLocal, listDataWsUsuariosDGM[0].huella2wsq);

                                dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                                if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                                dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                                if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                                {
                                    NuevoScan();
                                    throw new Exception("¡Las huellas no coinciden!. Puntuacion: " + DsCoincidenciaAB.Tables[0].Rows[0]["PUNTUACION"].ToString());
                                }
                                    
                                NuevaInstanciaEnrollment();
                            }
                            else
                                NuevaInstanciaEnrollment();
                        }
                        else if(Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                        {
                            byte[] bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella1wsq);
                            var resultTuple = await GenerarPlantillaImagenAsync(bytes);
                            MostrarHuellaDesdeBytes(bytes, nFVDedoB);

                            _subjectB = resultTuple.Item2;

                            if (_subjectA != null && _subjectB != null)
                            {
                                _biometricClient.MatchingWithDetails = true;
                                nFVDedoA.MatedMinutiae = null;
                                nFVDedoB.MatedMinutiae = null;
                                try
                                {
                                    var status = await _biometricClient.VerifyAsync(_subjectA, _subjectB);
                                    if (status == NBiometricStatus.MatchNotFound)
                                    {
                                        bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella2wsq);
                                        var resultTuple2 = await GenerarPlantillaImagenAsync(bytes);
                                        MostrarHuellaDesdeBytes(bytes, nFVDedoB);

                                        _subjectB = resultTuple2.Item2;

                                        if (_subjectA != null && _subjectB != null)
                                        {
                                            _biometricClient.MatchingWithDetails = true;
                                            nFVDedoA.MatedMinutiae = null;
                                            nFVDedoB.MatedMinutiae = null;

                                            var status2 = await _biometricClient.VerifyAsync(_subjectA, _subjectB);
                                            ComprobarStatusVerificacion(status2);
                                            NuevaInstanciaEnrollment();
                                        }
                                    }
                                    else
                                    {
                                        ComprobarStatusVerificacion(status);
                                        NuevaInstanciaEnrollment();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error al validar la identidad del usuario: " + ex.Message);
                        //MessageBox.Show("btnEscanearClickAsync(). " + ex.Message);
                    }
                }

                //if (txtUsuario.Text == "crear" && txtContrasenia.Text == "123456")
                //{
                //    Ingreso frmIngreso = new Ingreso();
                //    this.Hide();

                //    frmIngreso.ShowDialog();
                //    this.Close();

                //}
                //else if (txtUsuario.Text == "crear2" && txtContrasenia.Text == "123456")
                //{
                //    Usuarios frmUsuario = new Usuarios(listDataWsUsuariosDGM);
                //    this.Hide();

                //    frmUsuario.ShowDialog();
                //    this.Close();
                //}
                //else
                //{                    
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        public void NuevaInstanciaEnrollment()
        {
            try
            {
                enrollment er = new enrollment(listDataWsUsuariosDGM);
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
        private void ComprobarStatusVerificacion(NBiometricStatus status)
        {
            try
            {
                if (status != NBiometricStatus.Ok)
                {
                    NuevoScan();
                    int score = 0;
                    try { int.TryParse(_subjectA.MatchingResults[0].Score.ToString(), out score); } catch { };
                    throw new Exception("¡No fue posible verificar dactilar!, Estatus: " + status.ToString() + ", coincidencias: " + score);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void ComprobarStatusExtraccion(NBiometricStatus status)
        {
            try
            {
                if (status != NBiometricStatus.Ok)
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
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_biometricClient != null)
                _biometricClient.Cancel();
        }

        private void txtUsuario_Enter(object sender, EventArgs e)
        {
            txtUsuario.SelectAll();
        }

        private void txtContrasenia_Enter(object sender, EventArgs e)
        {
            txtContrasenia.SelectAll();
        }

        private async void Verificacion_Load(object sender, EventArgs e)
        {
            try
            {
                FUNCIONES funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);

                txtUsuario.Enabled = txtContrasenia.Enabled = btnVerificar.Enabled = false;

                wsBiometricsDGM = new WsBiometricsDGMSoapClient();

                _biometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                await _biometricClient.InitializeAsync();

                _deviceManager = _biometricClient.DeviceManager;

                NuevoScan();
                txtUsuario.Enabled = txtContrasenia.Enabled = btnVerificar.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            cmbEscaners.BeginUpdate();
            try
            {
                cmbEscaners.Items.Clear();
                if (_deviceManager != null)
                {
                    foreach (NDevice item in _deviceManager.Devices)
                    {
                        cmbEscaners.Items.Add(item);
                    }
                }
            }
            finally
            {
                cmbEscaners.EndUpdate();
            }
        }
    }
}
