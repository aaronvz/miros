using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using System.Drawing.Imaging;

using System.Management;

//COMPONENTES DEL SDK DE TRABAJO
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;
using Neurotec.Samples;
using Neurotec.IO;

//COMPONENTES
using CapaEN;
using CapaLN;

using DGMReaderNS;
using EncriptarXML_LN;
using System.Runtime.InteropServices;

using System.Net;
using Newtonsoft.Json;

using System.Xml.Linq;

using Microsoft.Reporting.WinForms;

using System.Diagnostics;

using ENROLLMENT_V3.Properties;
using ENROLLMENT_V3.Reportes;

using ENROLLMENT_V3.WsBiometricsDGM;


using System.Reflection;

namespace ENROLLMENT_V3
{
    public partial class Usuarios : Form
    {

        #region Variables Cámara

        WsBiometricsDGMSoapClient wsBiometricsDGM;      

        int ErrCount;
        object ErrLock = new object();
        object LvLock = new object();

        #endregion

        //ConfEnrollment configuracion;

        Sede sedeEstacion;

        
        int x, y;
        int ancho = 100;
        int alto = 137;

        LoginData loginData;

        public Usuarios(LoginData _loginData)
        {
            InitializeComponent();
            loginData = _loginData;
            x = y = 10;
            //configuracion = new ConfEnrollment();            
        }        

        private void btnCapturar_Click(object sender, EventArgs e)
        {
            try
            {
               
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void ReportError(string message, bool lockdown)
        {
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            if (lockdown) EnableUI(false);

            if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (ErrLock) { ErrCount--; }
        }

        private void EnableUI(bool enable)
        {
            if (InvokeRequired) Invoke((Action)delegate { EnableUI(enable); });
            else
            {
                //SettingsGroupBox.Enabled = enable;
                //InitGroupBox.Enabled = enable;
                //LiveViewGroupBox.Enabled = enable;
            }
        }      

        int tamanio;
        string[] array;
        List<string> listaProbatorios = new List<string>();

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void picb_cerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel_superior_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }



        private void lblFecha_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }



        private void pic_logo_dgm_MouseHover(object sender, EventArgs e)
        {

        }

        private void pic_logo_dgm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pic_txt_dgm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        DialogResult salir;
        private void btn_cerrar_info_Click(object sender, EventArgs e)
        {
            try
            {
                salir = MessageBox.Show("¿Está seguro que quiere salir de Enrollment?", "Salir", MessageBoxButtons.YesNo);
                if (salir == DialogResult.Yes)
                {
                    funciones.CancelarOperacionBiometrica(_biometricFaceClient);
                    funciones.CancelarOperacionBiometrica(_biometricFaceClientIcao);
                    funciones.CancelarOperacionBiometrica(_biometricFingerClient);

                    Application.Exit();
                    Environment.Exit(Environment.ExitCode);
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "Ingreso_FormClosing(). " + ex.Message;
                MessageBox.Show("Ingreso_FormClosing(). " + ex.Message);
            }
        }
        private PARAMETRIZACION parametrizacion;
        private FUNCIONES funciones;

        private DPI dpiTitular;
        private DPI dpiPadre;
        private DPI dpiMadre;

        private Image fotoDPI;

        int intentosMOCTitular;
        int intentosMOCPadre;
        int intentosMOCMadre;
        
        #region Declaración de variables para utilización del SDK

        //Administrador de dispositivos
        //HUELLAS
        private NDeviceManager _deviceFingerManager;
        //ROSTROS
        private NDeviceManager _deviceFaceManager;

        //Cliente biométrico
        //HUELLAS
        private NBiometricClient _biometricFingerClient;
        //ROSTRO
        private NBiometricClient _biometricFaceClient;
        private NBiometricClient _biometricFaceClientIcao;

        private ManualResetEvent _isIdle = new ManualResetEvent(true);

        //Sujetos
        private NSubject _subjectFinger;
        //private NSubject _subjectFace;

        //Dedo
        private NFinger _subjectFingerDerecho;
        private NFinger _subjectFingerIzquierdo;

        //Rostro
        //private NFace _nFace;
        private NFace _nFaceSegmented;

        #endregion

        #region Declaración de variables globales

        UsuariosEN usuariosEN;

        #endregion

        #region Métodos privados

        private void ActivarControlesHuellas(bool capturing)
        {
            if (cmbEscanersHuellas.Items.Count > 1)
                cmbEscanersHuellas.Enabled = !capturing;

            btnEscanearDDerecho.Enabled = !capturing;
            btnActualizarLista.Enabled = !capturing;


            var estatusDedoDerecho = !capturing && _subjectFingerDerecho != null && _subjectFingerDerecho.Status == NBiometricStatus.Ok;
            var estatusDedoIzquierdo = !capturing && _subjectFingerIzquierdo != null && _subjectFingerIzquierdo.Status == NBiometricStatus.Ok;
            chkMostrarBinarias.Enabled = (estatusDedoDerecho && estatusDedoIzquierdo);
        }

        private void ActivarControlesRostro(bool capturing)
        {
            btnCapturarRostro.Enabled = capturing;
            btnActualizarCamaras.Enabled = !capturing;

            CameraListBox.Enabled = true;

            if (capturing)
            {
                if (CameraListBox.Items.Count > 1)
                    CameraListBox.Enabled = !capturing;
                else
                    CameraListBox.Enabled = false;
            }


        }

        private void EnrollCompleto(NBiometricTask task, NSubject nSubject, NFinger nFinger, Label lblCalidad)
        {
            ActivarControlesHuellas(false);
            NBiometricStatus status = task.Status;

            if (status == NBiometricStatus.Canceled)
                return;

            if (status == NBiometricStatus.Ok)
                lblCalidad.Text = String.Format("Calidad: {0}", nFinger.Objects[0].Quality);
            else
            {
                MessageBox.Show(string.Format("La plantilla no fue extraída: {0}.", status), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                nSubject = null;
                nFinger = null;
                ActivarControlesHuellas(false);
            }
        }

        private void ListarEscanersHuellas(ComboBox cmbEscanerHuellas, bool validarDisponible)
        {
            try
            {
                cmbEscanerHuellas.Items.Clear();
                cmbEscanerHuellas.Enabled = cmbEscanerHuellas.Visible = true;

                //INICIANDO MANEJADOR DE DISPOSITIVOS, DE CAPTURA DE HUELLAS
                _deviceFingerManager = _biometricFingerClient.DeviceManager;

                if (_deviceFingerManager == null)
                    throw new Exception("Problemas con el manejador de dispositivos de lectura de huellas. ");

                foreach (NDevice item in _deviceFingerManager.Devices)
                    cmbEscanerHuellas.Items.Add(item);

                if (cmbEscanersHuellas.Items.Count == 0 && validarDisponible)
                    throw new Exception("No se encontraron dispositivos de lectura de disponibles. ");

                if (cmbEscanerHuellas.Items.Count == 1)
                {
                    cmbEscanerHuellas.Enabled = /*cmbEscanerHuellas.Visible =*/ false;
                    cmbEscanerHuellas.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("ListarEscaners(). " + ex.Message);
            }
        }

        private void ListarCamaras(ComboBox cmbCamaras, bool validarDisponible)
        {
            try
            {
                cmbCamaras.Items.Clear();
                cmbCamaras.Enabled = /*cmbCamaras.Visible =*/ true;

                //INICIANDO MANEJADOR DE DISPOSITIVOS, DE CAPTURA DE ROSTROS
                _deviceFaceManager = _biometricFaceClient.DeviceManager;


                if (_deviceFaceManager == null)
                    throw new Exception("Problemas con el manejador de dispositivos de captura de rostros. ");

                foreach (NDevice item in _deviceFaceManager.Devices)
                    cmbCamaras.Items.Add(item);

                if (cmbCamaras.Items.Count == 0 && validarDisponible)
                    throw new Exception("No se encontraron dispositivos de lectura de disponibles. ");

                if (cmbCamaras.Items.Count == 1)
                {
                    cmbCamaras.Enabled = /*cmbEscanerHuellas.Visible =*/ false;
                    cmbCamaras.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ListarCamaras(). " + ex.Message);
            }
        }



        #endregion

        #region Eventos de los controles del formulario

        private async void btnEscanearClickAsync(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCH.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Operación MOCH en proceso!");

                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";

                lblHitSibio.Text = "NO_HIT";
                lblHitSibio.ForeColor = Color.Red;
                pbxHitSibio.Image = pbxWarning.Image;

                if (_biometricFingerClient.FingerScanner == null)
                    throw new Exception(@"Por favor, seleccione un escáner de la lista.");

                btnForzarDDerecho.Enabled = (chkHuellasAutomatico.Checked) ? false : true;

                ActivarControlesHuellas(true);
                lblCalidadDDerecho.Text = string.Empty;

                // Create a objeto tipo NFinger
                _subjectFingerDerecho = new NFinger();


                // Set Manual capturing mode if not automatic selected
                if (!chkHuellasAutomatico.Checked)
                {
                    _subjectFingerDerecho.CaptureOptions = NBiometricCaptureOptions.Manual;

                }

                // Agregar un dedo al sujeto y a la vista
                _subjectFinger = new NSubject();
                _subjectFinger.Fingers.Add(_subjectFingerDerecho);
                _subjectFingerDerecho.PropertyChanged += OnAttributesPropertyChangedDerecho;

                nFVDDerecho.Finger = _subjectFingerDerecho;
                nFVDDerecho.ShownImage = ShownImage.Original;

                cmbEscanersHuellas.Enabled = false;

                btnActualizarLista.Enabled = false;
                cmbDedoDerecho.Enabled = false;
                txtComentarioDDerecho.Enabled = false;
                btnEscanearDDerecho.Enabled = false;
                chkHuellasAutomatico.Enabled = false;

                try
                {
                    // Empezando la captura
                    _biometricFingerClient.FingersReturnBinarizedImage = true;
                    NBiometricTask tarea = _biometricFingerClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, _subjectFinger);

                    var tareaEjecutada = await _biometricFingerClient.PerformTaskAsync(tarea);
                    EnrollCompleto(tareaEjecutada, _subjectFinger, _subjectFingerDerecho, lblCalidadDDerecho);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("¡Error al realizar la captura dedo derecho!. " + ex.Message);
                    txtMensaje.Text = "¡Error al realizar la captura dedo derecho!. " + ex.Message;
                    _biometricFingerClient.Cancel();
                }

                if (cmbEscanersHuellas.Items.Count > 1)
                    cmbEscanersHuellas.Enabled = true;

                btnActualizarLista.Enabled = true;
                cmbDedoDerecho.Enabled = true;
                txtComentarioDDerecho.Enabled = true;
                btnEscanearDDerecho.Enabled = true;
                chkHuellasAutomatico.Enabled = true;

                btnEscanearDDerecho.Text = "Repetir";

                btnEscanearDIzquierdo.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnEscanearClickAsync(). " + ex.Message;
                MessageBox.Show("btnEscanearClickAsync(). " + ex.Message);
            }
        }

        private async void btnEscanearIzquierdoClickAsync(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCH.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Operación MOCH en proceso!");

                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";

                lblHitSibio.Text = "NO_HIT";
                lblHitSibio.ForeColor = Color.Red;
                pbxHitSibio.Image = pbxWarning.Image;

                if (_biometricFingerClient.FingerScanner == null)
                    throw new Exception(@"Por favor, seleccione un escáner de la lista.");

                btnForzarDIzquierdo.Enabled = (chkHuellasAutomatico.Checked) ? false : true;

                ActivarControlesHuellas(true);
                lblCalidadDIzquierdo.Text = string.Empty;

                // Create a objeto tipo NFinger
                _subjectFingerIzquierdo = new NFinger();

                // Set Manual capturing mode if not automatic selected
                if (!chkHuellasAutomatico.Checked)
                {
                    _subjectFingerIzquierdo.CaptureOptions = NBiometricCaptureOptions.Manual;

                }

                // Agregar un dedo al sujeto y a la vista
                _subjectFinger = new NSubject();
                _subjectFinger.Fingers.Add(_subjectFingerIzquierdo);
                _subjectFingerIzquierdo.PropertyChanged += OnAttributesPropertyChangedIzquierdo;

                nFVDIzquierdo.Finger = _subjectFingerIzquierdo;
                nFVDIzquierdo.ShownImage = ShownImage.Original;

                cmbEscanersHuellas.Enabled = false;

                btnActualizarLista.Enabled = false;
                cmbDedoIzquierdo.Enabled = false;
                txtComentarioDIzquierdo.Enabled = false;
                btnEscanearDIzquierdo.Enabled = false;
                chkHuellasAutomatico.Enabled = false;

                try
                {
                    // Empezando la captura
                    _biometricFingerClient.FingersReturnBinarizedImage = true;
                    NBiometricTask tarea = _biometricFingerClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.CreateTemplate, _subjectFinger);

                    var tareaEjecutada = await _biometricFingerClient.PerformTaskAsync(tarea);
                    EnrollCompleto(tareaEjecutada, _subjectFinger, _subjectFingerIzquierdo, lblCalidadDIzquierdo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("¡Error al realizar la captura dedo izquierdo!. " + ex.Message);
                    txtMensaje.Text = "¡Error al realizar la captura dedo izquierdo!. " + ex.Message;
                    _biometricFingerClient.Cancel();
                }

                if (cmbEscanersHuellas.Items.Count > 1)
                    cmbEscanersHuellas.Enabled = false;

                btnActualizarLista.Enabled = true;
                cmbDedoIzquierdo.Enabled = true;
                txtComentarioDIzquierdo.Enabled = true;
                btnEscanearDIzquierdo.Enabled = true;
                chkHuellasAutomatico.Enabled = true;

                btnEscanearDIzquierdo.Text = "Repetir";
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnEscanearIzquierdoClickAsync(). " + ex.Message;
                MessageBox.Show("btnEscanearIzquierdoClickAsync(). " + ex.Message);
            }
        }

        private void OnAttributesPropertyChangedDerecho(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                BeginInvoke(new Action<NBiometricStatus>(status => lblCalidadDDerecho.Text = status.ToString()), _subjectFingerDerecho.Status);
            }
        }

        private void OnAttributesPropertyChangedIzquierdo(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                BeginInvoke(new Action<NBiometricStatus>(status => lblCalidadDIzquierdo.Text = status.ToString()), _subjectFingerIzquierdo.Status);
            }
        }

        private void btnActualizarListaButtonClick(object sender, EventArgs e)
        {
            ListarEscanersHuellas(cmbEscanersHuellas, true);
        }


        private void cmbEscanersHuellas_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                _biometricFingerClient.FingerScanner = cmbEscanersHuellas.SelectedItem as NFScanner;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbEscaners_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbEscaners_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void chkMostrarBinariasCheckedChanged(object sender, EventArgs e)
        {

        }

        private void FingerViewMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && chkMostrarBinarias.Enabled)
            {
                chkMostrarBinarias.Checked = !chkMostrarBinarias.Checked;
            }
        }

        /*private void ForceCaptureButtonClick(object sender, EventArgs e)
        {
            _biometricClient.Force();
        }*/

        #endregion

        private void enrollment_Load(object sender, EventArgs e)
        {
            try
            {
                funciones = new FUNCIONES();
                funciones.CargarLogo(pic_txt_dgm);

                wsBiometricsDGM = new WsBiometricsDGMSoapClient();

                this.Enabled = false;

                string sedeEquipo = Properties.Settings.Default.SEDE;

                DataSet dsSede = CargarSede(sedeEquipo);

                if (bool.Parse(dsSede.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsSede.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                sedeEstacion = (Sede)(dsSede.Tables[0].Rows[0]["DATOS"]);

                //DatosLicencia();
                //DatosEquipo();

                btnNuevo_Click(sender, e);
                lblFecha.Text = DateTime.Now.ToLongDateString() + " - ";

                CargarCmbTextBoxes(cmbTextBoxes, false);
                MaxLengthTextBoxes();

                this.Enabled = true;

                NativeMethods.CHANGEFILTERSTRUCT changeFilter = new NativeMethods.CHANGEFILTERSTRUCT();
                changeFilter.size = (uint)Marshal.SizeOf(changeFilter);
                changeFilter.info = 0;
                if (!NativeMethods.ChangeWindowMessageFilterEx(this.Handle, NativeMethods.WM_COPYDATA, NativeMethods.ChangeWindowMessageFilterExAction.Allow, ref changeFilter))
                {
                    int error = Marshal.GetLastWin32Error();
                    MessageBox.Show(String.Format("The error {0} occured.", error));
                }

                if (loginData != null)
                {
                    lblIdUsuario.Text = loginData.ID_USUARIO.ToString();

                    lbl_dpi_info.Text = loginData.CUI;
                    lbl_nombres_info.Text = loginData.NOMBRES;
                    lbl_apellidos_info.Text = loginData.APELLIDOS;

                    txtUsuario.Text = loginData.USUARIO;
                    txtUsuario.Enabled = false;
                    txtContrasenia.Enabled = false;
                    txtContrasenia2.Enabled = false;
                    txtPrimerNombre.Text = loginData.NOMBRES;
                    txtPrimerApellido.Text = string.Empty;

                    //int i = chkListPrivilegios.FindString(loginData[0].descripcion);
                    //chkListPrivilegios.SetItemChecked(i, true);                                                      
                }
                else
                {
                    txtUsuario.Enabled = true;
                    txtContrasenia.Enabled = true;
                    txtContrasenia2.Enabled = true;
                }
                
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "enrollment_Load(). " + ex.Message;
                MessageBox.Show("enrollment_Load(). " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task<Tuple<byte[], NSubject>> AbrirPlantillaImagenAsync(byte[] bytes, NFingerView nfvHuella)
        {
            NSubject nsSujeto = null;
            nfvHuella.Finger = null;
            //msgLabel.Text = string.Empty;
            //ResetMatedMinutiaeOnViews();

            // Check if given file is a template
            try
            {
                nsSujeto = NSubject.FromMemory(bytes);
            }
            catch (Exception ex) { }

            // If file is a template - return, otherwise assume that the file is an image and try to extract it
            if (nsSujeto != null && nsSujeto.Fingers.Count > 0)
            {
                nfvHuella.Finger = nsSujeto.Fingers[0];
                return new Tuple<byte[], NSubject>(bytes, nsSujeto);
            }

            // Create a finger object
            var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
            nsSujeto = new NSubject();
            nsSujeto.Fingers.Add(nfHuella);
            nfvHuella.Finger = nfHuella;

            // Extract a template from the subject
            _biometricFingerClient.FingersReturnBinarizedImage = true;
            try
            {
                var status = await _biometricFingerClient.CreateTemplateAsync(nsSujeto);
                if (status != NBiometricStatus.Ok)
                {
                    MessageBox.Show(string.Format("The template was not extracted: {0}.", status), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {

                Neurotec.Samples.Utils.ShowException(ex);
            }
            /*finally
            {
                EnableCheckBoxes();
            }*/
            return new Tuple<byte[], NSubject>(bytes, nsSujeto);
        }

        private void ControlsBackColor(string mode)
        {
            switch (mode)
            {
                case "NEW":
                    txtPrimerNombre.BackColor = Color.White;
                    txtPrimerApellido.BackColor = Color.White;
                    cmbGenero.BackColor = Color.White;
                    cmbEstadoCivil.BackColor = Color.White;
                    cmbOcupaciones.BackColor = Color.White;
                    dtpFechaNacimiento.BackColor = Color.White;
                    cmbTiposDocumento.BackColor = Color.White;
                    txtNumeroSerie.BackColor = Color.White;
                    txtCui.BackColor = Color.White;
                    cmbPaisNacimiento.BackColor = Color.White;
                    txtDepartamentoNacimiento.BackColor = Color.White;
                    cmbDeptoNacimiento.BackColor = Color.White;
                    cmbMunicNacimiento.BackColor = Color.White;
                    cmbPaisResidencia.BackColor = Color.White;
                    txtResidencia1.BackColor = Color.White;
                    txtResidencia2.BackColor = Color.White;
                    cmbDeptoResidencia.BackColor = Color.White;
                    cmbMunicResidencia.BackColor = Color.White;
                    //txtTelCelular.BackColor = Color.White;
                    txtTelCasa.BackColor = Color.White;
                    txtTelTrabajo.BackColor = Color.White;
                    txtEmail.BackColor = Color.White;

                    break;

                case "ADULT":
                    txtPrimerNombre.BackColor = Color.Yellow;
                    txtPrimerApellido.BackColor = Color.Yellow;
                    cmbGenero.BackColor = Color.Yellow;
                    cmbEstadoCivil.BackColor = Color.Yellow;
                    cmbOcupaciones.BackColor = Color.Yellow;
                    dtpFechaNacimiento.BackColor = Color.Yellow;
                    cmbTiposDocumento.BackColor = Color.Yellow;
                    txtNumeroSerie.BackColor = Color.White;
                    txtCui.BackColor = Color.Yellow;
                    cmbPaisNacimiento.BackColor = Color.Yellow;
                    cmbDeptoNacimiento.BackColor = Color.Yellow;
                    txtDepartamentoNacimiento.BackColor = Color.Yellow;
                    cmbMunicNacimiento.BackColor = Color.Yellow;
                    cmbPaisResidencia.BackColor = Color.White;
                    txtResidencia1.BackColor = Color.Yellow;
                    txtResidencia2.BackColor = Color.White;
                    cmbDeptoResidencia.BackColor = Color.Yellow;
                    cmbMunicResidencia.BackColor = Color.Yellow;

                    //txtTelCelular.BackColor = Color.Yellow;
                    txtTelCasa.BackColor = Color.White;
                    txtTelTrabajo.BackColor = Color.White;
                    txtEmail.BackColor = Color.White;

                    break;

                case "YOUNG":
                    txtPrimerNombre.BackColor = Color.Yellow;
                    txtPrimerApellido.BackColor = Color.Yellow;
                    cmbGenero.BackColor = Color.Yellow;
                    cmbEstadoCivil.BackColor = Color.Yellow;
                    cmbOcupaciones.BackColor = Color.Yellow;
                    dtpFechaNacimiento.BackColor = Color.Yellow;
                    cmbTiposDocumento.BackColor = Color.Yellow;
                    txtNumeroSerie.BackColor = Color.White;
                    txtCui.BackColor = Color.Yellow;
                    cmbPaisNacimiento.BackColor = Color.Yellow;
                    cmbDeptoNacimiento.BackColor = Color.Yellow;
                    txtDepartamentoNacimiento.BackColor = Color.Yellow;
                    cmbMunicNacimiento.BackColor = Color.Yellow;
                    cmbPaisResidencia.BackColor = Color.White;
                    txtResidencia1.BackColor = Color.Yellow;
                    txtResidencia2.BackColor = Color.White;
                    cmbDeptoResidencia.BackColor = Color.Yellow;
                    cmbMunicResidencia.BackColor = Color.Yellow;
                    
                    //txtTelCelular.BackColor = Color.Yellow;
                    txtTelCasa.BackColor = Color.White;
                    txtTelTrabajo.BackColor = Color.White;
                    txtEmail.BackColor = Color.White;

                    break;
            }
        }

        private async Task<int> Iniciar_Cliente_Huellas(object sender, EventArgs e)
        {
            try
            {
                //INICIANDO EL CLIENTE DE BIOMETRÍA PARA DISPOSITIVOS DE CAPTURA DE HUELLAS
                _biometricFingerClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                await _biometricFingerClient.InitializeAsync();
                return 0;
            }
            catch (Exception ex)
            {
                //UtilsLectorHuellas.GetException(ex);
                throw new Exception("Iniciar_Lector_Huellas(). " + ex.Message);
            }
        }

        private async Task<int> Iniciar_Cliente_Fotos(object sender, EventArgs e)
        {
            try
            {
                //INICIANDO EL CLIENTE DE BIOMETRÍA PARA DISPOSITIVOS DE CAPTURA DE HUELLAS
                if (_biometricFaceClient != null && _biometricFaceClient.CurrentBiometric != null) _biometricFaceClient.Cancel();
                _biometricFaceClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Face };
                await _biometricFaceClient.InitializeAsync();

                if (_biometricFaceClientIcao != null && _biometricFaceClientIcao.CurrentBiometric != null) _biometricFaceClientIcao.Cancel();
                _biometricFaceClientIcao = new NBiometricClient { BiometricTypes = NBiometricType.Face };
                await _biometricFaceClientIcao.InitializeAsync();

                return 0;
            }
            catch (Exception ex)
            {
                //UtilsLectorHuellas.GetException(ex);
                throw new Exception("Iniciar_Cliente_Fotos(). " + ex.Message);
            }
        }

        private void CargarCmbTiposTramite(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\TiposTramite.xml", "TiposTramite", "TipoTramite", "Codigo", "Tipo");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbTiposTramite(). " + ex.Message);
            }
        }

        private void CargarCmbTiposPasaporte(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\TiposPasaporte.xml", "TiposPasaporte", "TipoPasaporte", "Codigo", "Tipo");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbTiposPasaporte(). " + ex.Message);
            }
        }

        private void CargarCmbGenero(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Genero.xml", "Lista", "Nodo", "Codigo", "Nombre");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbGenero(). " + ex.Message);
            }
        }

        private void CargarCmbEstadoCivil(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\EstadoCivil.xml", "EstadosCiviles", "EstadoCivil", "Codigo", "Nombre");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbEstadoCivil(). " + ex.Message);
            }
        }

        private void CargarCmbOcupaciones(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Ocupaciones.xml", "Ocupaciones", "Ocupacion", "Codigo", "Nombre");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbOcupaciones(). " + ex.Message);
            }
        }

        private void CargarCmbOjos(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Ojos.xml", "Ojos", "Ojo", "Codigo", "Color");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbOjos(). " + ex.Message);
            }
        }

        private void CargarCmbTez(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Tez.xml", "Teces", "Tez", "Codigo", "Color");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbTez(). " + ex.Message);
            }
        }

        private void CargarCmbCabellos(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Cabello.xml", "Cabellos", "Cabello", "Codigo", "Color");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbCabellos(). " + ex.Message);
            }
        }

        private void CargarCmbTiposDocumento(ComboBox cmb, bool SeleccionarIndice, string tipoControl)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string filtro = "";
                if (tipoControl.Equals("TITULAR"))
                    filtro = " codigo IN (5, 8) ";
                else if (tipoControl.Equals("MENOR"))
                    filtro = " codigo IN (8) ";
                else if (tipoControl.Equals("PADRES"))
                    filtro = " codigo IN (2, 5, 6, 7, 8) ";

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Documentos.xml", "Documentos", "Documento", "Codigo", "Nombre");
                cmb.DataSource = ds.Tables[0].Select(filtro, " codigo ASC ").CopyToDataTable();
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0)
                {

                }

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbTiposDocumento(). " + ex.Message);
            }
        }

        private void CargarCmbPaises(ComboBox cmb, bool SeleccionarIndice, string CodigoPais)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (CodigoPais.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Pais.xml", "Lista", "Nodo", "Codigo", "Nombre");
                    cmb.DataSource = ds.Tables[0].Select(" 1 > 0 ", " VALOR ASC ").CopyToDataTable();
                    cmb.DisplayMember = "VALOR";
                    cmb.ValueMember = "CODIGO";

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedValue = CodigoPais;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbPaises(). " + ex.Message);
            }
        }

        private void CargarCmbPaisesSedeEntrega(ComboBox cmb, bool SeleccionarIndice, string Pais)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Sedes.xml", "Sedes", "Sede", "Pais", "Pais");

                string[] sColumnas = { "CODIGO", "VALOR" };
                DataTable dtDistinct = ds.Tables[0].DefaultView.ToTable(true, sColumnas).Select(" 1 > 0 ", " valor ASC ").CopyToDataTable();

                //RESTO DEL MUNDO
                if (sedeEstacion.PAIS.Equals("GUATEMALA") == false)
                {
                    DataRow[] drEliminar = dtDistinct.Select(" valor = 'GUATEMALA' ");

                    if (drEliminar.Length > 0)
                        dtDistinct.Rows.Remove(drEliminar[0]);
                }

                cmb.DataSource = dtDistinct;
                cmb.DisplayMember = "VALOR";
                cmb.ValueMember = "CODIGO";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedValue = Pais;
                else
                    cmb.SelectedIndex = -1;

            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbPaisesEntrega(). " + ex.Message);
            }
        }

        private void CargarCmbDepartamentos(ComboBox cmb, bool SeleccionarIndice, string Indice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Indice.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Deptos.xml", "Deptos", "Depto", "Codigo", "Nombre");

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = "VALOR";
                    cmb.ValueMember = "CODIGO";

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = int.Parse(Indice);
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbDepartamentos(). " + ex.Message);
            }
        }

        private void CargarCmbMunicipios(ComboBox cmb, bool SeleccionarIndice, string CodigoDepartamento)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (CodigoDepartamento.Equals("-1") == false)
                {
                    DataSet ds = new DataSet();
                    ds.Tables.Add(new DataTable());
                    ds.Tables[0].Columns.Add("CODIGO");
                    ds.Tables[0].Columns.Add("VALOR");
                    ds.Tables[0].Columns.Add("CODIGODEPTO");

                    XmlDocument documentoXML = new XmlDocument();

                    documentoXML.Load(Application.StartupPath + "\\Catalogos\\Munis.xml");

                    XmlNodeList listaNodo = documentoXML.GetElementsByTagName("Munis");
                    XmlNodeList lista = ((XmlElement)listaNodo[0]).GetElementsByTagName("Muni");

                    foreach (XmlElement nodo in lista)
                    {
                        int i = 0;

                        XmlNodeList nCodigo = nodo.GetElementsByTagName("CodigoMuni");
                        XmlNodeList nValor = nodo.GetElementsByTagName("NombreMuni");
                        XmlNodeList nCodigoDepto = nodo.GetElementsByTagName("CodigoDepto");

                        DataRow dr = ds.Tables[0].NewRow();
                        dr["CODIGO"] = nCodigo[i].InnerText;
                        dr["VALOR"] = nValor[i].InnerText;

                        if (nCodigoDepto[i].InnerText.Equals(CodigoDepartamento))
                            ds.Tables[0].Rows.Add(dr);
                    }

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = 0;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbDepartamentos(). " + ex.Message);
            }
        }

        private void CargarCmbSedesCuidad(ComboBox cmb, bool SeleccionarIndice, string Pais)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Pais.Equals("-1") == false)
                {

                    DataSet ds = LeerXmlLinqCatalogosFiltro(Application.StartupPath + "\\Catalogos\\Sedes.xml", "Sedes", "Sede", "Pais", Pais, "Nombre", "Ciudad");

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (Pais.Equals(sedeEstacion.PAIS))
                        cmb.SelectedValue = sedeEstacion.NOMBRE;
                    else
                        cmb.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbSedesCuidad(). " + ex.Message);
            }
        }

        private void CargarCmbEstados(ComboBox cmb, bool SeleccionarIndice, string Indice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Indice.Equals("-1") == false)
                {
                    DataSet ds = new DataSet();
                    ds.Tables.Add(new DataTable());
                    ds.Tables[0].Columns.Add("CODIGO");
                    ds.Tables[0].Columns.Add("VALOR");

                    XmlDocument documentoXML = new XmlDocument();

                    documentoXML.Load(Application.StartupPath + "\\Catalogos\\States.xml");

                    XmlNodeList listaNodo = documentoXML.GetElementsByTagName("States");
                    XmlNodeList lista = ((XmlElement)listaNodo[0]).GetElementsByTagName("State");

                    foreach (XmlElement nodo in lista)
                    {
                        int i = 0;

                        XmlNodeList nCodigo = nodo.GetElementsByTagName("Code");
                        XmlNodeList nValor = nodo.GetElementsByTagName("Name");

                        DataRow dr = ds.Tables[0].NewRow();
                        dr["CODIGO"] = nCodigo[i].InnerText;
                        dr["VALOR"] = nValor[i].InnerText;
                        ds.Tables[0].Rows.Add(dr);
                    }

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = int.Parse(Indice);
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbEstados(). " + ex.Message);
            }
        }

        private void CargarCmbZipCodes(ComboBox cmb, bool SeleccionarIndice, string Estado)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Estado.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlLinqCatalogosFiltro(Application.StartupPath + "\\Catalogos\\ZipCodes.xml", "ZipEntries", "ZipEntry", "State", Estado, "ZipCode", "ZipCode");

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = 0;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbZipCodes(). " + ex.Message);
            }
        }

        private void CargarCmbCiudadesZipCode(ComboBox cmb, bool SeleccionarIndice, string ZipCode)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (ZipCode.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlLinqCatalogosFiltro(Application.StartupPath + "\\Catalogos\\ZipCodes.xml", "ZipEntries", "ZipEntry", "ZipCode", ZipCode, "ZipCode", "City");
                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(new DataTable());
                    //ds.Tables[0].Columns.Add("CODIGO");
                    //ds.Tables[0].Columns.Add("VALOR");

                    //DataSet ds2 = new DataSet();
                    //ds2.ReadXml(Application.StartupPath + "\\Catalogos\\ZipCodes.xml");

                    //XmlDocument documentoXML = new XmlDocument();

                    //documentoXML.Load(Application.StartupPath + "\\Catalogos\\ZipCodes.xml");

                    //XmlNodeList listaNodo = documentoXML.GetElementsByTagName("ZipEntries");
                    //XmlNodeList lista = ((XmlElement)listaNodo[0]).GetElementsByTagName("ZipEntry");

                    //foreach (XmlElement nodo in lista)
                    //{
                    //    int i = 0;

                    //    XmlNodeList nCodigo = nodo.GetElementsByTagName("ZipCode");
                    //    XmlNodeList nValor = nodo.GetElementsByTagName("City");
                    //    XmlNodeList nCodigoDepto = nodo.GetElementsByTagName("State");

                    //    DataRow dr = ds.Tables[0].NewRow();
                    //    dr["CODIGO"] = nCodigo[i].InnerText;
                    //    dr["VALOR"] = nCodigo[i].InnerText + " - " + nValor[i].InnerText;

                    //    if (nCodigoDepto[i].InnerText.Equals(ZipCode))
                    //        ds.Tables[0].Rows.Add(dr);
                    //}

                    cmb.DataSource = ds.Tables[0];
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = 0;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbCiudadesEstado(). " + ex.Message);
            }
        }

        private void CargarCmbCiudadesEstado(ComboBox cmb, bool SeleccionarIndice, string Estado)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Estado.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlLinqCatalogosFiltro(Application.StartupPath + "\\Catalogos\\ZipCodes.xml", "ZipEntries", "ZipEntry", "State", Estado, "City", "City");


                    string[] sColumnas = { "CODIGO", "VALOR" };
                    DataTable dtDistinct = ds.Tables[0].DefaultView.ToTable(true, sColumnas).Select(" 1 > 0 ", " valor ASC ").CopyToDataTable();

                    cmb.DataSource = dtDistinct;
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = 0;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbCiudadesEstado(). " + ex.Message);
            }
        }

        private void CargarCmbJefes(ComboBox cmb, bool SeleccionarIndice, string Estado)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                if (Estado.Equals("-1") == false)
                {
                    DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Jefes.xml", "Jefes", "Jefe", "Codigo", "Valor");


                    string[] sColumnas = { "CODIGO", "VALOR" };
                    DataTable dtDistinct = ds.Tables[0].DefaultView.ToTable(true, sColumnas).Select(" 1 > 0 ", " valor ASC ").CopyToDataTable();

                    cmb.DataSource = dtDistinct;
                    cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                    cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                    if (cmb.Items.Count > 0 && SeleccionarIndice)
                        cmb.SelectedIndex = 0;
                    else
                        cmb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbJefes(). " + ex.Message);
            }
        }

        private DataSet CargarSede(string NombreSede)
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                XDocument listaNodos = XDocument.Load(Application.StartupPath + "\\Catalogos\\Sedes.xml", LoadOptions.None);
                XElement lista = listaNodos.Element("Sedes");

                XElement xeSede;

                try
                {
                    xeSede = lista.Elements().Single(p => p.Element("Nombre").Value == NombreSede);
                } catch (Exception ex)
                {
                    throw new Exception("Error al leer la sede con nombre: " + NombreSede + ". Excepción: " + ex.Message);
                }

                listaNodos = XDocument.Load(Application.StartupPath + "\\Catalogos\\Pais.xml", LoadOptions.None);
                lista = listaNodos.Element("Lista");

                string Pais = xeSede.Element("Pais").Value;
                XElement xeCodigoPais;

                try
                {
                    xeCodigoPais = lista.Elements().Single(p => p.Element("Nombre").Value == Pais);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al leer el código de país con nombre: " + Pais + ". Excepción: " + ex.Message);
                }

                Sede sede = new Sede();
                sede.CODIGO_PAIS = xeCodigoPais.Element("Codigo").Value;
                sede.PAIS = xeSede.Element("Pais").Value;
                sede.MISION = xeSede.Element("Mision").Value;
                sede.CIUDAD = xeSede.Element("Ciudad").Value;
                sede.NOMBRE = xeSede.Element("Nombre").Value;

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = sede;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "CargarSede(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private DataSet DatosLicencia()
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                XDocument listaNodos = XDocument.Load(Application.StartupPath + "\\ENROL\\Conf\\DGM.xml", LoadOptions.None);
                XElement lista = listaNodos.Element("DGM_DEVICES");

                Equipo equipo = new Equipo();
                equipo.DEVICE_TYPE = lista.Elements().First().Element("DEVICE_TYPE").Value.ToString();
                equipo.BIOS_SERIAL = lista.Elements().First().Element("BIOS_SERIAL").Value.ToString();
                equipo.UUID = lista.Elements().First().Element("UUID").Value.ToString(); ;
                equipo.PROCESSOR_ID = lista.Elements().First().Element("PROCESSOR_ID").Value.ToString();
                equipo.DRIVE_VOLUME = lista.Elements().First().Element("DRIVE_VOLUME").Value.ToString();
                equipo.MAC = lista.Elements().First().Element("MAC").Value.ToString();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["DATOS"] = equipo;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "DatosLicencia(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private DataSet DatosEquipo()
        {
            DataSet ds = ArmarDsResultado();
            try
            {

                //// create management class object
                //ManagementClass mc = new ManagementClass("Win32_ComputerSystem");

                ////collection to store all management objects
                //ManagementObjectCollection moc = mc.GetInstances();

                //Equipo equipo = new Equipo();
                //equipo.DEVICE_TYPE = mc.GetInstances().GetEnumerator().Current()[""].ToString();
                //equipo.BIOS_SERIAL = lista.Elements().First().Element("BIOS_SERIAL").Value.ToString();
                //equipo.UUID = lista.Elements().First().Element("UUID").Value.ToString(); ;
                //equipo.PROCESSOR_ID = lista.Elements().First().Element("PROCESSOR_ID").Value.ToString();
                //equipo.DRIVE_VOLUME = lista.Elements().First().Element("DRIVE_VOLUME").Value.ToString();
                //equipo.MAC = lista.Elements().First().Element("MAC").Value.ToString();

                //if (moc.Count != 0)
                //{

                //    foreach (ManagementObject mo in mc.GetInstances())

                //    {

                //        // display general system information

                //        Console.WriteLine("\nMachine Make: {0}\nMachine Model: {1}\nSystem Type: {2}\nHost Name: {3}\nLogon User Name: {4}\n",

                //                          mo["Manufacturer"].ToString(),

                //                          mo["Model"].ToString(),

                //                          mo["SystemType"].ToString(),

                //                          mo["DNSHostName"].ToString(),

                //                          mo["UserName"].ToString());

                //    }

                //}


                //XDocument listaNodos = XDocument.Load(Application.StartupPath + "\\Conf\\DGM.xml", LoadOptions.None);
                //XElement lista = listaNodos.Element("DGM_DEVICES");



                //ds.Tables[0].Rows[0]["RESULTADO"] = true;
                //ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //ds.Tables[0].Rows[0]["DATOS"] = equipo;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "DatosLicencia(). " + ex.Message;
                ds.Tables[0].Rows[0]["DATOS"] = new object();
            }
            return ds;
        }

        private void CargarCmbDedos(ComboBox cmb, bool SeleccionarIndice, string mano)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                string[] dedosMano = new string[] { "Indice", "Pulgar", "Medio", "Anular", "Meñique", "Ninguno" };
                DataSet ds = new DataSet();
                ds.Tables.Add(new DataTable());
                ds.Tables[0].Columns.Add("CODIGO");
                ds.Tables[0].Columns.Add("VALOR");


                for (int i = 1; i <= 6; i++)
                {
                    DataRow dr = ds.Tables[0].NewRow();

                    if (mano.ToUpper().Equals("DERECHA"))
                        dr["CODIGO"] = i;
                    else
                        dr["CODIGO"] = i + 6;

                    dr["VALOR"] = i + " - " + dedosMano[i - 1];

                    ds.Tables[0].Rows.Add(dr);
                }

                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = ds.Tables[0].Columns[1].ColumnName;
                cmb.ValueMember = ds.Tables[0].Columns[0].ColumnName;

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.Text = "1 - Indice";//cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbDedos(). " + ex.Message);
            }
        }

        private void CargarCmbTextBoxes(ComboBox cmb, bool SeleccionarIndice)
        {
            try
            {
                cmb.SelectedIndex = -1;
                cmb.DataSource = null;
                cmb.DisplayMember = null;
                cmb.ValueMember = null;
                cmb.Items.Clear();

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\ENROL\\Conf\\TextBox.xml", "TextBoxes", "TextBox", "Name", "MaxLength");
                cmb.DataSource = ds.Tables[0];
                cmb.DisplayMember = "CODIGO";
                cmb.ValueMember = "VALOR";

                if (cmb.Items.Count > 0 && SeleccionarIndice)
                    cmb.SelectedIndex = 0;
                else
                    cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw new Exception("CargarCmbTextBoxes(). " + ex.Message);
            }
        }

        private DataSet LeerXmlCatalogos(string archivoXML, string nombreListaNodos, string nombreLista, string snCodigo, string snValor)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("CODIGO");
            ds.Tables[0].Columns.Add("VALOR");

            try
            {
                XmlDocument documentoXML = new XmlDocument();

                documentoXML.Load(archivoXML);

                XmlNodeList listaNodo = documentoXML.GetElementsByTagName(nombreListaNodos);
                XmlNodeList lista = ((XmlElement)listaNodo[0]).GetElementsByTagName(nombreLista);

                foreach (XmlElement nodo in lista)
                {
                    int i = 0;

                    XmlNodeList nCodigo = nodo.GetElementsByTagName(snCodigo);
                    XmlNodeList nValor = nodo.GetElementsByTagName(snValor);

                    DataRow dr = ds.Tables[0].NewRow();
                    dr["CODIGO"] = nCodigo[i].InnerText;
                    dr["VALOR"] = nValor[i].InnerText;

                    ds.Tables[0].Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LeerXmlCatalogos(). " + ex.Message);
            }

            return ds;
        }

        private DataSet LeerXmlLinqCatalogosFiltro(string archivoXML, string nombreListaNodos, string nombreLista, string snCampoFiltro, string snValorFiltro, string snCampoCodigo, string snCampoValor)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("CODIGO");
            ds.Tables[0].Columns.Add("VALOR");

            try
            {
                XDocument listaNodos = XDocument.Load(archivoXML, LoadOptions.None);
                XElement lista = listaNodos.Element(nombreListaNodos);

                foreach (XElement ZipEntry in lista.Elements().Where(p => p.Element(snCampoFiltro).Value == snValorFiltro))
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    dr["CODIGO"] = ZipEntry.Element(snCampoCodigo).Value;
                    dr["VALOR"] = ZipEntry.Element(snCampoValor).Value;

                    ds.Tables[0].Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LeerXmlCatalogos(). " + ex.Message);
            }

            return ds;
        }

        private DataSet ArmarDsUsuario()
        {
            DataSet ds = new DataSet("DATOS_USUARIO");
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("NOMBRE", typeof(string));
            ds.Tables[0].Columns.Add("USUARIO", typeof(string));
            ds.Tables[0].Columns.Add("CONTRASENIA", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_BMP", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_PNG", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_BINARIZADA", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_PLANTILLA", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_WSQ", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_NFR", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_PLANTILLA_ISO_SOURCE_AFIS", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_PLANTILLA_CC_NEURO_FROM_PNG", typeof(string));

            ds.Tables[0].Columns.Add("HUELLA_FINAL_PNG", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_FINAL_PNG_STRING_BASE64", typeof(string));
            ds.Tables[0].Columns.Add("HUELLA_FINAL_WSQ_STRING_BASE64", typeof(string));

            DataRow dr = ds.Tables[0].NewRow();

            ds.Tables[0].Rows.Add(dr);

            return ds;
        }
        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {

                //tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";
                //tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                //tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";

                this.Enabled = false;
                bool camposValidos = false;
                camposValidos = await ValidarCampos();

                if (camposValidos)
                {
                    //string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "usuarios", "DATOS_USUARIO_" + txtPrimerNombre.Text.Trim().ToUpper() + "_" + txtPrimerApellido.Text.Trim().ToUpper() + ".xml");

                    //if (File.Exists(rutaXML))
                    //    throw new Exception("El archivo ya existe");

                    //string rutaXML2 = Path.Combine(Application.StartupPath, "ENROL", "usuarios", "Done", "DATOS_USUARIO_" + txtPrimerNombre.Text.Trim().ToUpper() + "_" + txtPrimerApellido.Text.Trim().ToUpper() + ".xml");

                    //if (File.Exists(rutaXML2))
                    //    throw new Exception("El archivo ya existe (2)");

                    //VerificacionGuardar vGuardar = new VerificacionGuardar();
                    //vGuardar.lblUsuario.Text = lbl_usuario.Text;
                    //vGuardar.lblNombreUsuario.Text = lblNombreUsuario.Text;
                    //vGuardar.ShowDialog();

                    //if (!vGuardar.VerificacionValida)
                    //    throw new Exception("¡No se pudo verificar la identidad del usuario!");

                    DialogResult result = MessageBox.Show("¿Está seguro que desea GUARDAR?", "Salir", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        BiometriaUsuarioRequest biometriaUsuarioRequest = new BiometriaUsuarioRequest();

                        biometriaUsuarioRequest.id_usuario = lblIdUsuario.Text;
                        string fotoJpegBase64 = "";
                        using (var ms = new MemoryStream())
                        {
                            pbxRostroIcao.Image.Save(ms, ImageFormat.Jpeg);
                            fotoJpegBase64 = Convert.ToBase64String(ms.ToArray());
                            ms.Dispose();
                        }

                        biometriaUsuarioRequest.foto = fotoJpegBase64;

                        biometriaUsuarioRequest.huellas = new Huellas[2];
                        ///////////////////GENERANDO LAS HUELLAS FINALES//////////////////
                        ///MANO DERECHA
                        biometriaUsuarioRequest.huellas[0] = new Huellas();
                        biometriaUsuarioRequest.huellas[0].id_posicion = 1;// int.Parse(cmbDedoDerecho.GetItemText(cmbDedoDerecho.SelectedItem).Split('-')[0].Trim());
                        
                        byte[] abHuellaD = _subjectFingerDerecho.Image.Save(NImageFormat.Png).ToArray();
                        Image iHuellaDigitalD;

                        using (var stream = new MemoryStream(abHuellaD, 0, abHuellaD.Length))
                        {
                            iHuellaDigitalD = Image.FromStream(stream);

                            DataSet dsHuellaFinal = GenerarHuellaPNG_STRING_BASE64(iHuellaDigitalD);

                            if (dsHuellaFinal == null || dsHuellaFinal.Tables.Count < 1 || dsHuellaFinal.Tables[0].Rows.Count < 1)
                                throw new Exception("¡Error al generar la huella derecha!");

                            if (bool.Parse(dsHuellaFinal.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception("!Error al generar la huella derecha!. " + dsHuellaFinal.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            biometriaUsuarioRequest.huellas[0].huella_png = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                            biometriaUsuarioRequest.huellas[0].huella_wsq = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                        }
                        ///MANO DERECHA
                        
                        /// MANO IZQUIERDA
                        biometriaUsuarioRequest.huellas[1] = new Huellas();
                        biometriaUsuarioRequest.huellas[1].id_posicion = 2;//int.Parse(cmbDedoIzquierdo.GetItemText(cmbDedoIzquierdo.SelectedItem).Split('-')[0].Trim());
                        
                        byte[] abHuellaI = _subjectFingerIzquierdo.Image.Save(NImageFormat.Png).ToArray();
                        Image iHuellaDigitalI;

                        using (var stream = new MemoryStream(abHuellaI, 0, abHuellaI.Length))
                        {
                            iHuellaDigitalI = Image.FromStream(stream);

                            DataSet dsHuellaFinal = GenerarHuellaPNG_STRING_BASE64(iHuellaDigitalI);

                            if (dsHuellaFinal == null || dsHuellaFinal.Tables.Count < 1 || dsHuellaFinal.Tables[0].Rows.Count < 1)
                                throw new Exception("¡Error al generar la huella izquierda!");

                            if (bool.Parse(dsHuellaFinal.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                throw new Exception("!Error al generar la huella izquierda!. " + dsHuellaFinal.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                            biometriaUsuarioRequest.huellas[1].huella_png = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                            biometriaUsuarioRequest.huellas[1].huella_wsq = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                            
                        }
                        /// MANO IZQUIERDA
                        ///////////////////GENERANDO LAS HUELLAS FINALES//////////////////

                        DataSet dsUsuario = InsertarInformacionUsuario(biometriaUsuarioRequest);

                        if (bool.Parse(dsUsuario.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsUsuario.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        BiometriaUsuarioResponse biometriaUsuarioResponse = (BiometriaUsuarioResponse)(dsUsuario.Tables[0].Rows[0]["DATOS"]);
                       
                        MessageBox.Show("Almacenado con éxito! ");
                        btnGuardar.Enabled = false;
                        txtMensaje.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnGuardar_Click(). Mensaje: " + ex.Message + ". StackTrace: " + ex.StackTrace;
                MessageBox.Show("btnGuardar_Click(). Mensaje: " + ex.Message + ". StackTrace: " + ex.StackTrace);
            }

            this.Enabled = true;
        }

        public DataSet InsertarInformacionUsuario(BiometriaUsuarioRequest biometriaUsuarioRequest)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                string postString = JsonConvert.SerializeObject(biometriaUsuarioRequest);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest request;
                string url = @Settings.Default.API_REST_MIROS + Settings.Default.API_INSERTAR_BIOMETRIA_USUARIO;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", $"Bearer {loginData.JWT_TOKEN}");

                request.ContentLength = data.Length;

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                BiometriaUsuarioResponse biometriaUsuarioResponse;
                biometriaUsuarioResponse = JsonConvert.DeserializeObject<BiometriaUsuarioResponse>(body);

                if (biometriaUsuarioResponse.codigo != 200)
                    throw new Exception("¡Sucedió un error, por favor consulte al administrador!" + biometriaUsuarioResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = biometriaUsuarioResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "InsertarInformacionUsuario(). " + ex.Message;
            }

            return dsResultado;
        }
        public DataSet GenerarHuellaPNG_STRING_BASE64(Image huellaDigital)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));
            ds.Tables[0].Columns.Add("IMAGEN_PNG", typeof(Image));
            ds.Tables[0].Columns.Add("IMAGEN_PNG_STRING_BASE64", typeof(string));
            ds.Tables[0].Columns.Add("IMAGEN_WSQ_STRING_BASE64", typeof(string));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            try
            {
                //Bitmap ImagenFinal;

                //ImagenFinal = new Bitmap(416, 416);
                //using (Graphics graph = Graphics.FromImage(ImagenFinal))
                //{
                //    System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, 416, 416);
                //    graph.FillRectangle(Brushes.White, ImageSize);

                //    //ImageSize = new Rectangle(79, 40, 258, 336);
                //    //graph.FillRectangle(Brushes.Blue, ImageSize);

                //    graph.DrawImage(huellaDigital, new System.Drawing.Rectangle(79, 40, 258, 336));
                //}
                Bitmap ImagenFinal = (Bitmap) (huellaDigital);

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = ImagenFinal;

                MemoryStream ms = new MemoryStream();
                ImagenFinal.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = Convert.ToBase64String(ms.ToArray());

                using (NImage imagenNeuro = NImage.FromBitmap((Bitmap)ImagenFinal))
                {
                    // Create WSQInfo to store bit rate
                    using (var info = (WsqInfo)NImageFormat.Wsq.CreateInfo(imagenNeuro))
                    {
                        var bitrate = WsqInfo.DefaultBitRate;
                        info.BitRate = bitrate;

                        string sWsqBase64 = Convert.ToBase64String(imagenNeuro.Save(NImageFormat.Wsq).ToArray());
                        ds.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"] = sWsqBase64;
                    }
                }
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = ex.Message;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = null;
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"] = string.Empty;
            }
            return ds;
        }

        public void EncriptarArchivo(string contraseña, string rutaArchivoPlano)
        {
            try
            {
                string rutaLlavePublica = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Keys") + "\\pasaportes_PublicKey.asc";
                string rutaLlavePrivada = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Keys") + "\\pasaportes_PrivateKey.asc";
                string archivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoPlano) + ".txt";

                EncriptarXML encriptarXml = new EncriptarXML();
                encriptarXml.Encryption(rutaLlavePublica, rutaLlavePrivada, contraseña, archivoEncriptado, rutaArchivoPlano);

                //MessageBox.Show("¡Encriptado con éxito!");
                //txtMensaje.Text = "¡Encriptado con éxito!" + Environment.NewLine + archivoEncriptado;

            }
            catch (Exception ex)
            {
                MessageBox.Show("EncriptarArchivo(). " + ex.Message);
                txtMensaje.Text = "EncriptarArchivo(). " + ex.Message;
            }
        }

        public async Task<bool> ValidarCampos()
        {
            DataSet dsIdentidad = ValidarIdentidad();
            DataSet dsRostro = ValidarFotografia();
            DataSet dsHuellas = await ValidarHuellas();
            DataSet dsFirma = await ValidarFirma();
            DataSet dsProbatorios = await ValidarProbatorios();

            txtMensaje.Text = string.Empty;

            //if (bool.Parse(dsIdentidad.Tables[0].Rows[0]["RESULTADO"].ToString()))
            //    tab_principal.TabPages["tabIdentidad"].ImageKey = "check.bmp";
            //else
            //{
            //    tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
            //    txtMensaje.Text += dsIdentidad.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            //    //MessageBox.Show(dsIdentidad.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            //}


            if (bool.Parse(dsRostro.Tables[0].Rows[0]["RESULTADO"].ToString()))
                tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";
            else
            {
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
                txtMensaje.Text += dsRostro.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            }


            if (bool.Parse(dsHuellas.Tables[0].Rows[0]["RESULTADO"].ToString()))
                tab_principal.TabPages["tabHuellas"].ImageKey = "check.bmp";
            else
            {
                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";
                txtMensaje.Text += dsHuellas.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            }


            //if (bool.Parse(dsFirma.Tables[0].Rows[0]["RESULTADO"].ToString()))
            //    tab_principal.TabPages["tabFirma"].ImageKey = "check.bmp";
            //else
            //{
            //    tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
            //    txtMensaje.Text += dsFirma.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            //}


            //if (bool.Parse(dsProbatorios.Tables[0].Rows[0]["RESULTADO"].ToString()))
            //    tab_principal.TabPages["tabProbatorios"].ImageKey = "check.bmp";
            //else
            //{
            //    tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";
            //    txtMensaje.Text += dsProbatorios.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            //}


            //bool b1 = tab_principal.TabPages["tabIdentidad"].ImageKey == "check.bmp";
            bool b2 = tab_principal.TabPages["tabFotografia"].ImageKey == "check.bmp";
            bool b3 = tab_principal.TabPages["tabHuellas"].ImageKey == "check.bmp";
            //bool b4 = tab_principal.TabPages["tabFirma"].ImageKey == "check.bmp";
            //bool b5 = tab_principal.TabPages["tabProbatorios"].ImageKey == "check.bmp";

            //if (b1 && b2 && b3 && b4 && b5)
            if (b2 && b3)
                return true;
            else
                return false;
        }

        private void cmbDeptoNacimiento_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                CargarCmbMunicipios(cmbMunicNacimiento, false, "-1");

                if (cmbDeptoNacimiento.SelectedValue != null)
                {
                    cmbMunicNacimiento.Enabled = true;
                    string sCodigo = cmbDeptoNacimiento.SelectedValue.ToString();

                    //GUATEMALA
                    if (cmbPaisNacimiento.SelectedValue.ToString().Equals("320"))
                    {
                        int iCodigo = -1;
                        try
                        {
                            iCodigo = int.Parse(sCodigo);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Código de departamento de nacimiento incorrecto. ");
                        }

                        if (iCodigo > 0)
                            CargarCmbMunicipios(cmbMunicNacimiento, false, iCodigo.ToString());

                    }//ESTADOS UNIDOS
                    else if (cmbPaisNacimiento.SelectedValue.ToString().Equals("840"))
                        CargarCmbCiudadesEstado(cmbMunicNacimiento, false, sCodigo);
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbDeptoNacimiento_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbDeptoNacimiento_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbDeptoResidencia_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {

                if (cmbDeptoResidencia.SelectedValue != null)
                {
                    int iCodigoDepartamento = -1;
                    string sCodigoDepartamento = cmbDeptoResidencia.SelectedValue.ToString();

                    try
                    {
                        iCodigoDepartamento = int.Parse(sCodigoDepartamento);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Código de departamento de residencia incorrecto. ");
                    }

                    if (iCodigoDepartamento > 0)
                    {
                        CargarCmbMunicipios(cmbMunicResidencia, false, iCodigoDepartamento.ToString());
                        cmbMunicResidencia.Enabled = true;
                    }
                }
                else
                    CargarCmbMunicipios(cmbMunicResidencia, false, "0");
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbDeptoResidencia_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbDeptoResidencia_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void btnActualizarCamaras_Click(object sender, EventArgs e)
        {
            try
            {
                ListarCamaras(CameraListBox, true);
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnActualizarCamaras_Click(). " + ex.Message;
                MessageBox.Show("btnActualizarCamaras_Click(). " + ex.Message);
            }

        }

        private void btnActivarCapturaRostroAsync_Click(object sender, EventArgs e)
        {
            try
            {
                if (CameraListBox.SelectedIndex < 0)
                    throw new Exception(@"Por favor, seleccione una cámara de la lista.");

                lblMatchFace.Text = string.Empty;
                
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnActivarCapturaRostroAsync_Click(). " + ex.Message;
                MessageBox.Show("btnActivarCapturaRostroAsync_Click(). " + ex.Message);

                btnDetenerCapturaRostro_Click(sender, e);
            }
        }

        private async void btnCapturarRostro_Click(object sender, EventArgs e)
        {
            btnCapturarRostro.Enabled = false;

            try
            {
                if (_biometricFaceClient.FaceCaptureDevice == null) throw new Exception("¡Seleccione una cámara!");

                byte[] byteArray = facesView.Face.Image.Save().ToArray();
                pbxRostroIcao.Image = funciones.ImageFromByteArray(byteArray);

                NFace face = new NFace { Image = NImage.FromMemory(byteArray) };

                NSubject _subject = new NSubject();
                _subject.Faces.Add(face);

                var task = _biometricFaceClientIcao.CreateTask(NBiometricOperations.Capture | NBiometricOperations.Segment | NBiometricOperations.CreateTemplate, _subject);
                var performedTask = await _biometricFaceClientIcao.PerformTaskAsync(task);

                pbxRostroIcao.Image = null;
                if (performedTask.Status != NBiometricStatus.Ok)
                    throw new Exception("Error al tomar la fotografía. Status:" + performedTask.Status + ((performedTask.Error != null && !performedTask.Error.Equals("")) ? ", Error:" + performedTask.Error : ""));

                if (performedTask.Status == NBiometricStatus.Ok)
                {
                    pbxRostroIcao.Image = _subject.Faces.First().Image.ToBitmap();

                    if(_subject.Faces.Count > 1)
                        pbxRostroIcao.Image = _subject.Faces[1].Image.ToBitmap();
                }

                ValidarFotografia();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnCapturarRostro_Click(). " + ex.Message;
                MessageBox.Show("btnCapturarRostro_Click(). " + ex.Message);
            }
            btnCapturarRostro.Enabled = true;
        }

        private void btnDetenerCapturaRostro_Click(object sender, EventArgs e)
        {
            try
            {
                _biometricFaceClient.Cancel();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnDetenerCapturaRostro_Click(). " + ex.Message;
                MessageBox.Show("btnDetenerCapturaRostro_Click. " + ex.Message);
            }
        }

        private async void btnForzarCapturaRostro_Click(object sender, EventArgs e)
        {

            //this.Enabled = false;




            //try
            //{
            //    //MainCamera.TakePhotoShutter();

            //    await TakePhoto();
            //    //_biometricFaceClient.Force();

            //    bool generalize = false;
            //    bool fromFile = true;
            //    bool fromCamera = false;
            //    bool checkIcao = true;
            //    int count = 3;
            //    NBiometricCaptureOptions options = NBiometricCaptureOptions.None;

            //    lblEstadoCapturaRostro.Visible = false;

            //    NSubject _newSubject = new NSubject();
            //    _newSubject.Clear();
            //    faceView2.Face = null;

            //    Bitmap bmp = (Bitmap)(pbxVistaPrevia.Image);

            //    NFace face = new NFace
            //    {
            //        Image = NImage.FromBitmap(bmp),
            //        CaptureOptions = NBiometricCaptureOptions.Stream
            //    };

            //    _newSubject.Faces.Add(face);
            //    faceView2.Face = _newSubject.Faces.First();
            //    icaoWarningView2.Face = _newSubject.Faces.First();

            //    _biometricFaceClient.FacesCheckIcaoCompliance = checkIcao;
            //    NBiometricOperations operations = fromFile ? NBiometricOperations.CreateTemplate : NBiometricOperations.Capture | NBiometricOperations.CreateTemplate;
            //    if (checkIcao) operations |= NBiometricOperations.Segment;

            //    NBiometricTask biometricTask = _biometricFaceClient.CreateTask(operations, _newSubject);
            //    SetStatusText(Color.Orange, fromFile ? "Extracting template ..." : "Starting capturing ...");


            //    SetIsBusy(true);

            //    //EnableControls();
            //    faceView2.ShowAge = !checkIcao;
            //    faceView2.ShowEmotions = !checkIcao;
            //    faceView2.ShowExpression = !checkIcao;
            //    faceView2.ShowGender = !checkIcao;
            //    faceView2.ShowProperties = !checkIcao;
            //    faceView2.ShowIcaoArrows = true;

            //    try
            //    {
            //        var performedTask = await _biometricFaceClient.PerformTaskAsync(biometricTask);
            //        OnCreateTemplateCompleted(performedTask);

            //        if (performedTask.Status == NBiometricStatus.Ok)
            //        {
            //            _nFaceSegmented = _newSubject.Faces[1];
            //            faceView2.Face = _nFaceSegmented;
            //            icaoWarningView2.Face = _nFaceSegmented;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        //Utilities.ShowError(ex);
            //        SetIsBusy(false);
            //    }



            //await ValidarFotografia();


            //}
            //catch (Exception ex)
            //{
            //    txtMensaje.Text = "btnDetenerCapturaRostro_Click(). " + ex.Message;
            //    MessageBox.Show("btnDetenerCapturaRostro_Click(). " + ex.Message);
            //}

            //this.Enabled = true;
        }


        private void SetIsBusy(bool value)
        {
            if (value)
                _isIdle.Reset();
            else
                _isIdle.Set();
        }

        private bool IsBusy()
        {
            return !_isIdle.WaitOne(0);
        }

        private void CancelAndWait()
        {
            if (IsBusy())
            {
                _biometricFaceClient.Cancel();
                _isIdle.WaitOne();
            }
        }

        private void cmdActivarFirma_Click(object sender, EventArgs e)
        {
            try
            {
                //sigPlusNET1.ClearTablet();

                //sigPlusNET1.SetTabletState(1);
                //sigPlusNET1.SetJustifyMode(0);

                cmdActivarFirma.Enabled = false;
                btnCapturarFirma.Enabled = true;
                cmdLimpiarFirma.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmdActivarFirma_Click(). " + ex.Message;
                MessageBox.Show("cmdActivarFirma_Click(). " + ex.Message);
            }
        }

        private void btnCapturarFirma_Click(object sender, EventArgs e)
        {
            try
            {

                //sigPlusNET1.SetTabletState(0);

                //sigPlusNET1.SetImageXSize(500);
                //sigPlusNET1.SetImageYSize(150);
                //sigPlusNET1.SetJustifyMode(5);

                cmdActivarFirma.Enabled = false;
                btnCapturarFirma.Enabled = false;
                cmdLimpiarFirma.Enabled = true;

                //sigPlusNET1.SetJustifyMode(0);


            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnCapturarFirma_Click(). " + ex.Message;
                MessageBox.Show("btnCapturarFirma_Click(). " + ex.Message);
            }
        }

        private void cmdLimpiarFirma_Click(object sender, EventArgs e)
        {
            try
            {
                //sigPlusNET1.ClearTablet();
                //sigPlusNET1.SetTabletState(0);

                cmdActivarFirma.Enabled = true;
                btnCapturarFirma.Enabled = false;
                cmdLimpiarFirma.Enabled = false;

                tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmdLimpiarFirma_Click(). " + ex.Message;
                MessageBox.Show("cmdLimpiarFirma_Click(). " + ex.Message);
            }
        }

        private void enrollment_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_biometricFaceClient != null)
                    _biometricFaceClient.Cancel();

                if (_biometricFingerClient != null)
                    _biometricFingerClient.Cancel();
                
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "Ingreso_FormClosing(). " + ex.Message;
                MessageBox.Show("Ingreso_FormClosing(). " + ex.Message);
            }
        }
        String nom_escaner;
        int limiteEscaneos = 32;

        private void btn_escanear_Click(object sender, EventArgs e)
        {
            try
            {                

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btn_escanear_Click(). " + ex.Message;
                MessageBox.Show("btn_escanear_Click(). " + ex.Message);
            }
        }

        private void VerProbatorios(String src, int contar)
        {
            try
            {
                int i = contar;
                /********/
                Label lblProbatorio = new Label();
                lblProbatorio.Name = "lblProbatorio_" + i.ToString();
                //ComprimirImagen(src, src, 65);

                //PROCESAMIENTO DE ROSTROS
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length <= 0)
                    InvocarProcesamientoRostro();
                //COMPRIMIR - GENERAR MINIATURA
                ActivarProcesamientoRostro(src);


                string pathThumbnail = src.Split('.')[0] + "_thumbnail." + src.Split('.')[1];
                Image.FromFile(src).GetThumbnailImage(ancho, alto, null, IntPtr.Zero).Save(pathThumbnail, ImageFormat.Png);

                Image img = LoadBitmapUnlocked(pathThumbnail);//CreateNonIndexedImage(pathThumbnail);//(Image)(System.Drawing.Bitmap.FromFile(pathThumbnail).Clone());
                 
                try { File.Delete(pathThumbnail);} catch { }


                lblProbatorio.Image = (Image)(img.Clone());
                img.Dispose();

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                lblProbatorio.Image.Tag = src;
                lblProbatorio.Size = new System.Drawing.Size(ancho, alto);
                lblProbatorio.Location = new System.Drawing.Point(x, splitContainer1.Panel2.AutoScrollPosition.Y + y);
                lblProbatorio.BorderStyle = BorderStyle.FixedSingle;
                lblProbatorio.ContextMenuStrip = cmsProbatorios;
                splitContainer1.Panel2.Controls.Add(lblProbatorio);

                i++;

                x += ancho + 10;

                if (x > (splitContainer1.Panel2.Width - ancho))
                {
                    y += alto + 10;
                    x = 10;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("VerProbatorios(). " + ex.Message);
            }
        }

        //public static Image CreateNonIndexedImage(string path)
        //{
        //    using (var sourceImage = Image.FromFile(path))
        //    {
        //        var targetImage = new Bitmap(sourceImage.Width, sourceImage.Height,
        //          PixelFormat.Format32bppArgb);
        //        using (var canvas = Graphics.FromImage(targetImage))
        //        {
        //            canvas.DrawImageUnscaled(sourceImage, 0, 0);
        //        }
        //        return targetImage;
        //    }
        //}

        //private void ComprimirImagen(string inputFile, string ouputfile, long compression)
        //{
        //    try
        //    {
        //        using (var image = LoadBitmapUnlocked(inputFile))
        //        {

        //            EncoderParameters eps = new EncoderParameters(1);
        //            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);

        //            string mimetype = "image/jpeg";//GetMimeType(new System.IO.FileInfo(inputFile).Extension);
        //            ImageCodecInfo ici = GetEncoderInfo(mimetype);

        //            image.Save(ouputfile, ici, eps);
        //            //image.Save(inputFile, ici, eps);
        //            image.Dispose();
        //            GC.Collect();
        //            GC.WaitForPendingFinalizers();
        //        }

        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("ComprimirImagen(). " + ex.Message);
        //    }
        //}

        //private Bitmap LoadBitmapUnlocked(string file_name)
        //{
        //    using (Bitmap bm = new Bitmap(file_name))
        //    {
        //        return new Bitmap(bm);
        //    }
        //}

        //static string GetMimeType(string ext)
        //{
        //    try
        //    {
        //        //    CodecName FilenameExtension FormatDescription MimeType 
        //        //    .BMP;*.DIB;*.RLE BMP ==> image/bmp 
        //        //    .JPG;*.JPEG;*.JPE;*.JFIF JPEG ==> image/jpeg 
        //        //    *.GIF GIF ==> image/gif 
        //        //    *.TIF;*.TIFF TIFF ==> image/tiff 
        //        //    *.PNG PNG ==> image/png 
        //        switch (ext.ToLower())
        //        {
        //            case ".bmp":
        //            case ".dib":
        //            case ".rle":
        //                return "image/bmp";

        //            case ".jpg":
        //            case ".jpeg":
        //            case ".jpe":
        //            case ".fif":
        //                return "image/jpeg";

        //            case "gif":
        //                return "image/gif";
        //            case ".tif":
        //            case ".tiff":
        //                return "image/tiff";
        //            case "png":
        //                return "image/png";
        //            default:
        //                return "image/jpeg";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetMimeType(). " + ex.Message);
        //    }
        //}

        //static ImageCodecInfo GetEncoderInfo(string mimeType)
        //{
        //    try
        //    {
        //        ImageCodecInfo[] encoders;
        //        encoders = ImageCodecInfo.GetImageEncoders();

        //        ImageCodecInfo encoder = (from enc in encoders
        //                                  where enc.MimeType == mimeType
        //                                  select enc).First();
        //        return encoder;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("GetEncoderInfo(). " + ex.Message);
        //    }
        //}

        private void probatorios_image_base64(string src)
        {
            try
            {
                array = new string[tamanio];
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    Image.FromFile(src).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);

                    array[tamanio - 1] = base64String;
                    listaProbatorios.Add(array[tamanio - 1]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("probatorios_image_base64(). " + ex.Message);
            }

        }

        private void CmbIngresoManual(ComboBox cmb, bool activar, string nuevoItem)
        {
            try
            {
                if (activar)
                {
                    cmb.SelectedIndex = -1;
                    cmb.DataSource = null;
                    cmb.DisplayMember = null;
                    cmb.ValueMember = null;
                    cmb.Items.Clear();
                    cmb.DropDownStyle = ComboBoxStyle.DropDown;
                    cmb.Enabled = false;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("VALOR", typeof(string));
                    dt.Columns.Add("CODIGO", typeof(string));
                    DataRow dr = dt.NewRow();
                    dr["VALOR"] = nuevoItem;
                    dr["CODIGO"] = "-1";

                    dt.Rows.Add(dr);
                    cmb.DataSource = dt;
                    cmb.DisplayMember = "VALOR";
                    cmb.ValueMember = "CODIGO";
                    cmb.SelectedIndex = 0;
                }
                else
                {
                    cmb.DropDownStyle = ComboBoxStyle.DropDown;
                    cmb.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CmbIngresoManual(). " + ex.Message);
            }
        }
        

        public DataSet Depto_Munic_EmisionDPI(string departamento, string municipio)
        {
            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                CargarCmbDepartamentos(cmbDeptoDPI, false, "0");

                try
                {
                    cmbDeptoDPI.SelectedValue = int.Parse(departamento).ToString();
                } catch
                {
                    throw new Exception("El departamento con el código: " + departamento + ", no existe, contacte al administrador del sistema.");
                }

                CargarCmbMunicipios(cmbMunicipioDPI, false, int.Parse(departamento).ToString());

                try
                {
                    cmbMunicipioDPI.SelectedValue = int.Parse(municipio).ToString();
                }
                catch
                {
                    throw new Exception("El municipio con el código: " + departamento + ", no existe, contacte al administrador del sistema.");
                }

                lblEmisionDPI.Text = cmbMunicipioDPI.Text + ", " + cmbDeptoDPI.Text;
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;

            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "Depto_Munic_EmisionDPI(). " + ex.Message;
            }
            return ds;
        }

        public Task<DataSet> MOC_DPI(string cui)
        {
            return Task.Run(() =>
            {
                DataSet ds = ArmarDsResultado();
                ds.Tables[0].Rows[0]["RESULTADO"] = false;

                try
                {
                    DGMReader APIReader = new DGMReader();

                    bool MOC_DPI = false;
                    int Remaining = -1;
                    APIReader.apConnectReader();

                    string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    string _apConnectReader = APIReader.apConnectReader();

                    if (_apIsReaderConnected.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apIsReaderConnected = " + _apIsReaderConnected);

                    if (_apConnectReader.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apConnectReader = " + _apConnectReader);

                    string Err;
                    string Valor = "";

                    Err = APIReader.apGetCUI(ref Valor);
                    if (!cui.Trim().Equals(Valor.Trim()))
                        throw new Exception("El CUI del DPI leído (" + txtCui.Text.Trim() + "), no coincide con el de la operación de MOCH (" + Valor.Trim() + ").");

                    string respuesta = APIReader.apMatchOnCard(10, ref MOC_DPI, ref Remaining);

                    if (MOC_DPI)
                        ds.Tables[0].Rows[0]["RESULTADO"] = true;
                    else
                        ds.Tables[0].Rows[0]["RESULTADO"] = false;

                    ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                }
                catch (Exception ex)
                {
                    ds.Tables[0].Rows[0]["RESULTADO"] = false;
                    ds.Tables[0].Rows[0]["MSG_ERROR"] = "MOC_DPI(). " + ex.Message;
                }
                return ds;
            }
            );
        }

        public void BloquearControles(string opcion)
        {
            try
            {
                if (opcion.Equals("MOC"))
                {
                    txtCui.Enabled = false;
                    txtPrimerNombre.Enabled = false;

                    txtPrimerApellido.Enabled = false;

                    dtpFechaNacimiento.Enabled = false;
                    cmbGenero.Enabled = false;
                    //cmbEstadoCivil.Enabled = false;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = false;

                    cmbTiposDocumento.Enabled = false;
                    txtNumeroId.Enabled = false;
                    txtNumeroSerie.Enabled = false;

                    cmbPaisNacimiento.Enabled = false;
                    cmbDeptoNacimiento.Enabled = false;
                    cmbMunicNacimiento.Enabled = false;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    cmbMunicResidencia.Enabled = true;                    
                }

                if (opcion.Equals("WsRENAP"))
                {
                    txtCui.Enabled = false;
                    txtPrimerNombre.Enabled = false;

                    txtPrimerApellido.Enabled = false;

                    dtpFechaNacimiento.Enabled = false;
                    cmbGenero.Enabled = false;
                    //cmbEstadoCivil.Enabled = false;
                    cmbEstadoCivil.Enabled = true;

                    cmbTiposDocumento.Enabled = false;
                    //GESTIONADO EN: cmbTiposDocumento_SelectionChangeCommitted
                    //txtNumeroId.Enabled = false;
                    //txtNumeroSerie.Enabled = false;

                    cmbPaisNacimiento.Enabled = false;
                    cmbDeptoNacimiento.Enabled = false;
                    cmbMunicNacimiento.Enabled = false;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    cmbMunicResidencia.Enabled = true;

                    //NO SE SABE SI RENAP TIENE LOS DATOS COMPLETOS
                    //txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = txtNombrePadre.Text.Equals(string.Empty) ? true : false;
                    //txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = txtNombreMadre.Text.Equals(string.Empty) ? true : false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("BloquearControles(). " + ex.Message);
            }
        }

        public void DesbloquearControles(string opcion)
        {
            try
            {
                if (opcion.Equals("NUEVO INGRESO"))
                {
                    txtCui.Enabled = false;
                    txtPrimerNombre.Enabled = true;

                    txtPrimerApellido.Enabled = true;
                    dtpFechaNacimiento.Enabled = true;
                    cmbGenero.Enabled = true;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = true;

                    cmbTiposDocumento.Enabled = true;
                    lblNumeroId.Enabled = txtNumeroId.Enabled = true;
                    txtNumeroSerie.Enabled = true;

                    cmbPaisNacimiento.Enabled = true;
                    cmbDeptoNacimiento.Enabled = true;
                    //cmbMunicNacimiento.Enabled = true;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    //cmbMunicResidencia.Enabled = true;
                }

                if (opcion.Equals("MOC"))
                {
                    txtCui.Enabled = true;
                    txtPrimerNombre.Enabled = true;
                    
                    txtPrimerApellido.Enabled = true;
                    dtpFechaNacimiento.Enabled = true;
                    cmbGenero.Enabled = true;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = true;

                    cmbTiposDocumento.Enabled = true;
                    txtNumeroId.Enabled = true;
                    txtNumeroSerie.Enabled = true;

                    cmbPaisNacimiento.Enabled = true;
                    cmbDeptoNacimiento.Enabled = true;
                    cmbMunicNacimiento.Enabled = true;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    cmbMunicResidencia.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("BloquearControles(). " + ex.Message);
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                NuevoIngreso();
                //DatosPrueba();
                this.Enabled = true;

                btnGuardar.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnNuevo_Click(). " + ex.Message;
                MessageBox.Show("btnNuevo_Click(). " + ex.Message);
            }
        }

        public async void NuevoIngreso()
        {
            try
            {
                lblIdUsuario.Text = "0";
                lblComprimir.Text = string.Empty;
                dpiTitular = new DPI();
                dpiPadre = new DPI();
                dpiMadre = new DPI();

                lbl_sede.Text = "SEDE (" + sedeEstacion.PAIS.ToUpper() + ")";

                pbxCheck.Image.Tag = "Check";
                pbxWarning.Image.Tag = "Warning";
                pbxLoad.Image.Tag = "Loading";
                pbxUsuario.Image.Tag = "FotoDefault";

                pbxCheckColor.Image.Tag = "Check";
                pbxWarningColor.Image.Tag = "Waning";

                intentosMOCTitular = intentosMOCPadre = intentosMOCMadre = 1;

                parametrizacion = new PARAMETRIZACION();
                //funciones = new FUNCIONES();                

                //IDENTIDAD
                lbl_dpi_info.Text = "CUI"; lbl_nombres_info.Text = "Nombres"; lbl_apellidos_info.Text = "Apellidos";
                lblEmisionDPI.Text = string.Empty;
                //lbl_etiqueta_usuario.Text = "Usuario";
                //lbl_usuario.Text = "Nombre usuario";

                //txtPrimerNombre.Text = txtPrimerApellido.Text = string.Empty;

                //CargarCmbGenero(cmbGenero, false);
                //CargarCmbEstadoCivil(cmbEstadoCivil, false);
                //cmbEstadoCivil_SelectionChangeCommitted(new Object(), new EventArgs());
                //CargarCmbOcupaciones(cmbOcupaciones, false);
                //dtpFechaNacimiento.Value = DateTime.Today;

                ////DOCUMENTO DE IDENTIFICACIÓN
                //CargarCmbTiposDocumento(cmbTiposDocumento, false, "TITULAR");
                //txtNumeroId.Text = string.Empty;
                //txtNumeroSerie.Text = string.Empty;
                //txtCui.Text = string.Empty;

                pbxFotoDPITitular.Image = picb_usuario.Image = pbxUsuario.Image;
                pbxDPI.Image = pbxWarning.Image;
                pbxMOCH.Image = pbxWarning.Image;
                pbxMOCF.Image = pbxWarning.Image;
                pbxAlertas.Image = pbxWarning.Image;

                pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                contadorFotos = 0;

                ////LUGAR DE NACIMIENTO
                //CargarCmbPaises(cmbPaisNacimiento, true, "320");
                //cmbPaisNacimiento_SelectionChangeCommitted(new Object(), new EventArgs());

                ////DIRECCIÓN DE RESIDENCIA
                //CargarCmbPaises(cmbPaisResidencia, true, "0");
                //cmbPaisResidencia.SelectedValue = int.Parse(sedeEstacion.CODIGO_PAIS);
                //cmbPaisResidencia_SelectionChangeCommitted(new Object(), new EventArgs());

                //txtResidencia1.Text = string.Empty;
                //txtResidencia2.Text = string.Empty;
                
                ////CONTACTO
                //txtTelCasa.Text = txtTelCelular.Text = txtTelTrabajo.Text = txtEmail.Text = string.Empty;

                ////USUARIO
                //CargarCmbJefes(cmbJefes, false, "0");
                //chkListPrivilegios.ClearSelected();

                //FOTOGRAFÍA       
                try { File.Delete(Application.StartupPath + "\\ENROL\\Fotos\\Rostro.JPG"); } catch { }
                try { File.Delete(Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG"); } catch { }
                try { File.Delete(Application.StartupPath + "\\ENROL\\ROSTRO\\SegmentedFace.jpeg"); } catch { }

                //faceView2.Face = null;
                pbxRostroIcao.Image = null;

                pbxFondoIcao.BackColor = Color.White;
                lblMatchFace.Text = string.Empty;

                ActivarControlesRostro(true);                

                btnActivarCapturaRostro.Text = "Activar";

                chkIcao.Checked = true;
                chkIcao.Visible = false;
                txtObservacionesIcao.Text = string.Empty;
                txtObservacionesIcao.Visible = false;

                //HUELLAS
                //CargarCmbDedos(cmbDedoDerecho, true, "DERECHA");
                cmbDedoDerecho.Enabled = false;

                //CargarCmbDedos(cmbDedoIzquierdo, true, "IZQUIERDA");
                cmbDedoIzquierdo.Enabled = false;

                lblComentarioDDerecho.Visible = lblComentarioDIzquierdo.Visible = false;
                txtComentarioDDerecho.Text = txtComentarioDIzquierdo.Text = string.Empty;
                txtComentarioDDerecho.Visible = txtComentarioDIzquierdo.Visible = false;

                nFVDDerecho.Finger = null;
                nFVDIzquierdo.Finger = null;

                lblCalidadDDerecho.Text = string.Empty;
                lblCalidadDIzquierdo.Text = string.Empty;

                btnEscanearDDerecho.Enabled = false;
                btnEscanearDIzquierdo.Enabled = false;

                btnEscanearDDerecho.Text = "Capturar";
                btnEscanearDIzquierdo.Text = "Capturar";

                chkMostrarBinarias.Visible = false;

                chkHuellasAutomatico.Checked = true;

                btnForzarDDerecho.Enabled = btnForzarDIzquierdo.Enabled = false;
                chkHuellasAutomatico.Enabled = false;

                btnActivarHuellas.Enabled = true;

                pbxHitSibio.Image = pbxWarning.Image;
                lblHitSibio.Text = "NO_HIT";

                //FIRMA
                chkNoPuedeFirmar.Enabled = true;
                chkNoPuedeFirmar.Checked = false;

                //sigPlusNET1.ClearTablet();
                cmdActivarFirma.Enabled = true;
                btnCapturarFirma.Enabled = false;
                cmdLimpiarFirma.Enabled = false;

                DesbloquearControles("NUEVO INGRESO");

                pbxDPI.Enabled = pbxMOCH.Enabled = false;

                //CAMARAS
                await Iniciar_Cliente_Fotos(new Object(), new EventArgs());
                ListarCamaras(CameraListBox, true);
                
                //HUELLAS
                await Iniciar_Cliente_Huellas(new Object(), new EventArgs());
                ListarEscanersHuellas(cmbEscanersHuellas, true);

                //PROBATORIOS
                x = y = 10;

                tamanio = 0;

                LimpiarBandejaProbatorios();

                tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";

                tab_principal.SelectedIndex = 0;

                try { tab_principal.TabPages.Remove(tab_principal.TabPages[0]); } catch { };
                try { tab_principal.TabPages.Remove(tab_principal.TabPages[2]); } catch { };
                try { tab_principal.TabPages.Remove(tab_principal.TabPages[2]); } catch { };

                ControlsBackColor("NEW");

                foto = no_caso = tipo_pasaporte = nombres = apellidos = apellido_casada = direccion = tel_casa = tel_trabajo = tel_celular = correo = pais = sexo = estado_civil = nacionalidad = fecha_nacimiento = string.Empty;
                depto_nacimiento = muni_nacimiento = pais_nacimiento = identificacion = depto_emision = municipio_emision = color_ojos = color_tez = color_cabello = estatura = padre = madre = sede_entrega = string.Empty;
                partida_nacimiento = libro = folio = acta = pasaporte_autorizado = identificacion_padre = identificacion_madre = autorizado_dgm = usuario = estacion = lugar_fecha = cui_menor = tipo_entrega = string.Empty;
                direccion_entrega1 = direccion_entrega2 = direccion_entrega3 = string.Empty;
        
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "NuevoIngreso(). " + ex.Message;
                MessageBox.Show("NuevoIngreso(). " + ex.Message);
            }
        }

        public void LimpiarBandejaProbatorios()
        {
            try
            {
                foreach (var control in splitContainer1.Panel2.Controls.Cast<Control>().ToArray())
                {
                    Label lbl = (Label)control;

                    try { File.Delete(lbl.Image.Tag.ToString());} catch { }

                    string rutaThumbnail = lbl.Image.Tag.ToString().Split('.')[0] + "_thumbnail." + lbl.Image.Tag.ToString().Split('.')[1];

                    try { File.Delete(rutaThumbnail);} catch { }

                    lbl.Image.Dispose();
                    lbl.Dispose();

                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }

                splitContainer1.Panel2.Controls.Clear();
                //listaProbatorios = new List<string>();
            }
            catch(Exception ex)
            {
                throw new Exception("LimpiarBandejaProbatorios(). " + ex.Message);
            }
        }

        public void MaxLengthTextBoxes()
        {
            try
            {
                DataTable dtTextBoxes = (DataTable)(cmbTextBoxes.DataSource);

                foreach (DataRow drTextBox in dtTextBoxes.Rows)
                {
                    Control[] controlCollection = tab_principal.TabPages["tabIdentidad"].Controls.Find(drTextBox["CODIGO"].ToString(), true);

                    foreach (Control control in controlCollection)
                    {
                        if (control is TextBox)
                        {
                            cmbTextBoxes.Text = drTextBox["CODIGO"].ToString();
                            TextBox txtTemporal = (TextBox)(control);
                            txtTemporal.MaxLength = int.Parse(cmbTextBoxes.SelectedValue.ToString());
                            txtTemporal.CharacterCasing = CharacterCasing.Upper;
                        }
                    }
                }
                txtComentarioDIzquierdo.MaxLength = txtComentarioDDerecho.MaxLength = 250;
                txtComentarioDIzquierdo.CharacterCasing = txtComentarioDDerecho.CharacterCasing = CharacterCasing.Upper;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "MaxLengthTextBoxes(). " + ex.Message;
                MessageBox.Show("MaxLengthTextBoxes(). " + ex.Message);
            }
        }

        public void DatosPrueba()
        {
            txtResidencia1.Text = txtResidencia2.Text = "4ta. Avenida 3-08 zona 1, Guatemala";
            cmbDeptoResidencia.Text = "GUATEMALA";
            CargarCmbMunicipios(cmbMunicResidencia, true, cmbDeptoResidencia.SelectedValue.ToString());

            txtTelCelular.Text = txtTelCasa.Text = txtTelTrabajo.Text = "56265627";
            txtEmail.Text = "johny_073@hotmail.com";
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnActivarHuellas_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";

                //DEDO DERECHO
                cmbDedoDerecho.Enabled = true;
                cmbDedoDerecho.SelectedIndex = -1;
                cmbDedoDerecho.SelectedIndex = 0;

                txtComentarioDDerecho.Text = string.Empty;
                txtComentarioDDerecho.Visible = false;

                btnEscanearDDerecho.Enabled = true;

                //DEDO IZQUIERDO
                cmbDedoIzquierdo.Enabled = false;
                cmbDedoIzquierdo.SelectedIndex = -1;
                cmbDedoIzquierdo.SelectedIndex = 0;

                txtComentarioDIzquierdo.Text = string.Empty;
                txtComentarioDIzquierdo.Visible = false;

                btnEscanearDIzquierdo.Enabled = false;

                btnActivarHuellas.Enabled = false;

                chkHuellasAutomatico.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnActivarHuellas_Click(). " + ex.Message;
                MessageBox.Show("btnActivarHuellas_Click(). " + ex.Message);
            }
        }

        private void cmbPaisResidencia_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                if (cmbPaisResidencia.SelectedValue != null)
                {

                    cmbMunicResidencia.Enabled = false;
                    string sCodigoPais = cmbPaisResidencia.SelectedValue.ToString();

                    if (sCodigoPais.Equals("320"))//GUATEMALA
                    {
                        lblDeptoResidencia.Visible = lblMunicResidencia.Visible = true;
                        cmbDeptoResidencia.Visible = cmbMunicResidencia.Visible = true;

                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "0");
                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");
                        
                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "####-####";                        
                    }
                    else if (sCodigoPais.Equals("840"))//ESTADOS UNIDOS
                    {
                        lblDeptoResidencia.Visible = lblMunicResidencia.Visible = false;
                        cmbDeptoResidencia.Visible = cmbMunicResidencia.Visible = false;

                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1");

                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");

                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "(###)###-####";                        
                    }
                    else
                    {
                        lblDeptoResidencia.Visible = cmbDeptoResidencia.Visible = false;
                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1"); ;

                        lblMunicResidencia.Visible = cmbMunicResidencia.Visible = false;
                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");

                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "############";
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbPaisResidencia_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbPaisResidencia_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbDedoDerecho_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtComentarioDDerecho.Text = string.Empty;
                lblComentarioDDerecho.Visible = txtComentarioDDerecho.Visible = false;

                if (cmbDedoDerecho.Text.ToUpper().Contains("INDICE") == false)
                    lblComentarioDDerecho.Visible = txtComentarioDDerecho.Visible = true;

                if (cmbDedoDerecho.Text.ToUpper().Contains("NINGUNO") == true)
                {
                    nFVDDerecho.Finger = null;
                    btnEscanearDDerecho.Enabled = false;
                    lblCalidadDDerecho.Text = string.Empty;
                }
                else
                    btnEscanearDDerecho.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbDedoDerecho_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbDedoDerecho_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbDedoIzquierdo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtComentarioDIzquierdo.Text = string.Empty;
                lblComentarioDIzquierdo.Visible = txtComentarioDIzquierdo.Visible = false;

                if (cmbDedoIzquierdo.Text.ToUpper().Contains("INDICE") == false)
                    lblComentarioDIzquierdo.Visible = txtComentarioDIzquierdo.Visible = true;

                if (cmbDedoIzquierdo.Text.ToUpper().Contains("NINGUNO") == true)
                {
                    nFVDIzquierdo.Finger = null;
                    btnEscanearDIzquierdo.Enabled = false;
                    lblCalidadDIzquierdo.Text = string.Empty;
                }
                else
                    btnEscanearDIzquierdo.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbDedoIzquierdo_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbDedoIzquierdo_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void chkNoPuedeFirmar_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                cmdLimpiarFirma_Click(sender, e);
                if (chkNoPuedeFirmar.Checked)
                {
                    cmdActivarFirma.Enabled = btnCapturarFirma.Enabled = cmdLimpiarFirma.Enabled = false;
                    tab_principal.TabPages["tabFirma"].ImageKey = "check.bmp";
                }
                else
                {
                    cmdActivarFirma.Enabled = true;
                    btnCapturarFirma.Enabled = cmdLimpiarFirma.Enabled = false;
                    tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                }

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "chkNoPuedeFirmar_CheckedChanged(). " + ex.Message;
                MessageBox.Show("chkNoPuedeFirmar_CheckedChanged(). " + ex.Message);
            }

        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.SelectedIndex = (tab_principal.SelectedIndex + 1 < tab_principal.TabCount) ? tab_principal.SelectedIndex + 1 : tab_principal.SelectedIndex;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnSiguiente_Click(). " + ex.Message;
                MessageBox.Show("btnSiguiente_Click(). " + ex.Message);
            }
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.SelectedIndex = (tab_principal.SelectedIndex - 1 >= 0) ? tab_principal.SelectedIndex - 1 : tab_principal.SelectedIndex;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnAnterior_Click(). " + ex.Message;
                MessageBox.Show("btnAnterior_Click(). " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("¿Está seguro que desea SALIR?", "Salir", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                    this.Close();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnSalir_Click(). " + ex.Message;
                MessageBox.Show("btnSalir_Click(). " + ex.Message);
            }
        }

        private void chkDesconocido_CheckedChanged(object sender, EventArgs e)
        {           
        }

        private void cmbPaisNacimiento_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtDepartamentoNacimiento.Text = txtMunicipioNacimiento.Text = string.Empty;

                //cmbDeptoNacimiento.DropDownStyle = ComboBoxStyle.DropDownList;
                //cmbMunicNacimiento.DropDownStyle = ComboBoxStyle.DropDownList;

                cmbMunicNacimiento.Enabled = false;

                //GUATEMALA
                if (cmbPaisNacimiento.SelectedValue.ToString().Equals("320") == true)
                {
                    lblDepartamentoNacimiento.Text = "Departamento:";
                    lblMunicipioNacimiento.Text = "Municipio:";

                    cmbDeptoNacimiento.Visible = cmbMunicNacimiento.Visible = true;
                    CargarCmbDepartamentos(cmbDeptoNacimiento, false, "0");
                    CargarCmbMunicipios(cmbMunicNacimiento, false, "0");

                    txtDepartamentoNacimiento.Visible = txtMunicipioNacimiento.Visible = false;

                }//RESTO DEL MUNDO
                else //(cmbPaisNacimiento.SelectedValue.ToString().Equals("840") == true)
                {
                    lblDepartamentoNacimiento.Text = "Estado:";
                    txtDepartamentoNacimiento.Visible = true;

                    lblMunicipioNacimiento.Visible = txtMunicipioNacimiento.Visible = false;

                    cmbDeptoNacimiento.Visible = cmbMunicNacimiento.Visible = false;
                    CargarCmbEstados(cmbDeptoNacimiento, false, "-1");
                    CargarCmbMunicipios(cmbMunicNacimiento, false, "-1");

                    //cmbDeptoNacimiento.DropDownStyle = ComboBoxStyle.DropDown;
                    //cmbMunicNacimiento.DropDownStyle = ComboBoxStyle.DropDown;

                }
                //RESTO DEL MUNDO
                //else
                //{
                //    lblDepartamentoNacimiento.Text = "Estado:";
                //    lblMunicipioNacimiento.Text = "Provincia:";

                //    cmbDeptoNacimiento.Visible = cmbMunicNacimiento.Visible = false;
                //    CargarCmbDepartamentos(cmbDeptoNacimiento, false, "-1");
                //    CargarCmbMunicipios(cmbMunicNacimiento, false, "-1");

                //    //txtDepartamentoNacimiento.Visible = txtMunicipioNacimiento.Visible = true;
                //    txtDepartamentoNacimiento.Visible = true;
                //}
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbPaisNacimiento_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbPaisNacimiento_SelectionChangeCommitted(). " + ex.Message);
            }
        }        

        internal static string foto, no_caso, tipo_pasaporte, nombres, apellidos, apellido_casada, direccion, tel_casa, tel_trabajo, tel_celular, correo, pais, sexo, estado_civil, nacionalidad, fecha_nacimiento,
            depto_nacimiento, muni_nacimiento, pais_nacimiento, identificacion, depto_emision, municipio_emision, color_ojos, color_tez, color_cabello, estatura, padre, madre, sede_entrega,
            partida_nacimiento, libro, folio, acta, pasaporte_autorizado, identificacion_padre, identificacion_madre, autorizado_dgm, usuario, estacion, lugar_fecha, cui_menor, tipo_entrega,
            direccion_entrega1, direccion_entrega2, direccion_entrega3;

        private void cmbZipCodeEntrega_SelectionChangeCommitted(object sender, EventArgs e)
        {
        }        

        private void cmbEstadoCivil_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                //txtApellidoCasada.Enabled = true;
                //txtApellidoCasada.Leave -= TextBoxLeave_Action;
                //txtApellidoCasada.BackColor = Color.White;

                //if (cmbEstadoCivil.Text.Contains("SOLTERO") || cmbEstadoCivil.Text.Contains("SOLTERA"))
                //{
                //    txtApellidoCasada.Text = string.Empty;
                //    txtApellidoCasada.Enabled = false;
                //}
                //else if (cmbEstadoCivil.Text.Contains("CASADO") || cmbEstadoCivil.Text.Contains("CASADA"))
                //{
                    
                //}
                ////txtApellidoCasada.Enabled = ((cmbEstadoCivil.Text.Contains("CASADO") || cmbEstadoCivil.Text.Contains("CASADA") || cmbEstadoCivil.Text.Contains("CASADO(A)") || cmbEstadoCivil.Text.Contains("UNION DE HECHO") || cmbEstadoCivil.Text.Contains("UNIDA")) && cmbGenero.Text.Equals("FEMENINO")) ? true : false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("cmbEstadoCivil_SelectionChangeCommitted(). " + ex.Message);
                txtMensaje.Text = "cmbEstadoCivil_SelectionChangeCommitted(). " + ex.Message;
            }
        }

        private void cmbTextBoxes_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                string valor = cmbTextBoxes.SelectedValue.ToString();
                MessageBox.Show("Valor: " + valor);
            }
            catch (Exception ex)
            {
            }
        }

        private void txtNoCaso_KeyPress(object sender, KeyPressEventArgs e)
        {            
        }

        private void pbxDPI_Click(object sender, EventArgs e)
        {            
        }

        private async void pbxMOCH_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxDPI.Image.Tag.Equals("Warning"))
                    throw new Exception("¡Primero lea la información del DPI!");

                if (pbxDPI.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxMOCH.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCTitular >= (parametrizacion.INTENTOS_MOC_TITULAR + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_TITULAR + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCH.Image = pbxWarning.Image;

                        _biometricFingerClient.Force();
                        tabHuellas.Enabled = false;

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCH.Image = pbxLoad.Image;
                        
                        DataSet dsMOC = await MOC_DPI(txtCui.Text.Trim());

                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();


                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCH.Image = pbxCheck.Image;
                            intentosMOCTitular--;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCTitular) + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCH.Image = pbxWarning.Image;
                        }
                        intentosMOCTitular++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCH_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCH_Click(). " + ex.Message);
                pbxMOCH.Image = pbxWarning.Image;
            }
            //panel_inferior.Enabled = picb_cerrar.Enabled = true;
            tabHuellas.Enabled = true;
        }

        private void pic_txt_dgm_Click(object sender, EventArgs e)
        {
            try
            {
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length > 0)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();


            }
            catch (Exception ex)
            {
                MessageBox.Show("pic_txt_dgm_Click(). " + ex.Message);
            }
        }

        private void cmbDeptoResidencia_TextChanged(object sender, EventArgs e)
        {
            if(cmbDeptoResidencia.SelectedValue != null && int.TryParse(cmbDeptoResidencia.SelectedValue.ToString(), out int a))
                cmbDeptoResidencia_SelectionChangeCommitted(sender, e);
        }
        
        private void cmbPaisSedeEntrega_TextChanged(object sender, EventArgs e)
        {            
        }

        private void cmbTipoPasaporte_TextChanged(object sender, EventArgs e)
        {
            //if (cmbTipoPasaporte.SelectedValue != null && int.TryParse(cmbTipoPasaporte.SelectedValue.ToString(), out int a))
            //    cmbTipoPasaporte_SelectionChangeCommitted(sender, e);
        }

        private void cmbDeptoNacimiento_TextChanged(object sender, EventArgs e)
        {
            if (cmbDeptoNacimiento.SelectedValue != null && int.TryParse(cmbDeptoNacimiento.SelectedValue.ToString(), out int a))
                cmbDeptoNacimiento_SelectionChangeCommitted(sender, e);
        }

        private void pbxValidacionWsRenap_Click(object sender, EventArgs e)
        {
            try
            {
                //if (cmbTiposDocumento.Text.ToUpper().Contains("DPI") || cmbTiposDocumento.Text.ToUpper().Contains("CUI"))
                //{
                //    //if (txtPrimerNombre.Enabled)
                //    {
                //        pbxValidacionWsRenap.Image = pbxWarningColor.Image;

                //        bool vCamposValidos = ((txtCui.Text.Trim().Equals(string.Empty) == false || txtCui.Text.Trim().Equals("") == false) && (txtPrimerNombre.Text.Trim().Equals(string.Empty) == false || txtPrimerNombre.Text.Trim().Equals("") == false) && (txtPrimerApellido.Text.Trim().Equals(string.Empty) == false || txtPrimerApellido.Text.Trim().Equals("") == false) && (dtpFechaNacimiento.Text.Trim().Equals(string.Empty) == false || dtpFechaNacimiento.Text.Trim().Equals("") == false));

                //        if (vCamposValidos)
                //        {
                //            pbxValidacionWsRenap.Image = pbxLoadColor.Image;

                //            vValidacionWsRenap = new ValidacionWsRenap();
                //            await vValidacionWsRenap.DatosDpi(txtCui.Text, pbxFotoDPITitular.Image, txtPrimerNombre.Text, txtSegundoNombre.Text, txtTercerNombre.Text, txtPrimerApellido.Text, txtSegundoApellido.Text, txtApellidoCasada.Text, dtpFechaNacimiento.Text, cmbGenero.Text, ""/*txtNombrePadre.Text*/ + ", " + "" /*txtApellidoPadre.Text*/, ""/*txtNombreMadre.Text*/ + ", " + ""/*txtApellidoMadre.Text*/, cmbEstadoCivil.Text);

                //            if (bool.Parse(vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["RESULTADO"].ToString()))
                //                pbxValidacionWsRenap.Image = pbxCheckColor.Image;
                //            else
                //            {
                //                pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                //                MessageBox.Show("Error al consultar el Servicio Web de RENAP: " + vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                //            }
                //        }
                //    }

                //    if (vValidacionWsRenap != null)
                //        vValidacionWsRenap.ShowDialog();
                //}
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxValidacionWsRenap_Click(). " + ex.Message;
                MessageBox.Show("pbxValidacionWsRenap_Click(). " + ex.Message);
            }
        }

        private void txtObservacionesIcao_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtObservacionesIcao.BackColor = (txtObservacionesIcao.Text.Trim().Equals("") || txtObservacionesIcao.Text.Trim().Equals(string.Empty)) ? Color.Yellow : Color.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtObservacionesIcao_TextChanged(). " + ex.Message);
                txtMensaje.Text = "txtObservacionesIcao_TextChanged(). " + ex.Message;
            }
        }

        private void FaceView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        public DataSet ConsultaInformacionxUsuario(string usuario, string contrasenia)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {

                InfoWsUsuarioDGMConsulta vParametrosUsuario = new InfoWsUsuarioDGMConsulta();
                vParametrosUsuario.usuario = usuario;

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
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaInformacionxUsuario(). " + ex.Message;
            }

            return dsResultado;
        }

        private void CmbEstadoCivil_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabIdentidad_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void lblFecha_Click(object sender, EventArgs e)
        {

        }

        private void cmbGenero_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                //if (cmbGenero.Text.Equals("MASCULINO"))
                //{
                //    txtApellidoCasada.Text = string.Empty;
                //    txtApellidoCasada.Enabled = false;
                //}
                //else
                //    txtApellidoCasada.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("cmbGenero_SelectionChangeCommitted(). " + ex.Message);
                txtMensaje.Text = "cmbGenero_SelectionChangeCommitted(). " + ex.Message;
            }
        }

        private void LblComprimir_TextChanged(object sender, EventArgs e)
        {
            if (lblComprimir.Text.Equals(string.Empty) == false)
            {
                
            }
        }

        private async void CameraListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _biometricFaceClient.FaceCaptureDevice = CameraListBox.SelectedItem as NCamera;

            DataSet dsActivarCamara = await funciones.ActivarCamara(facesView, _biometricFaceClient);
            if (bool.Parse(dsActivarCamara.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                throw new Exception(dsActivarCamara.Tables[0].Rows[0]["MSG_ERROR"].ToString());
        }

        private void facesView_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pbxAlertas_Click(object sender, EventArgs e)
        {            
        }

        private void enrollment_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void picb_logo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(txtMensaje.Text);
        }        


        private void cmbTiposDocumento_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtNumeroId.Text = txtNumeroSerie.Text = txtCui.Text = string.Empty;

                if (cmbTiposDocumento.Text.Equals(string.Empty) == false && cmbTiposDocumento.Text.Equals("") == false)
                {
                    txtCui.Enabled = true;
                    if (cmbTiposDocumento.Text.Contains("DPI") == false)
                    {
                        lblNumeroId.Enabled = txtNumeroId.Enabled = true;
                        lblNumeroSerie.Enabled = txtNumeroSerie.Enabled = false;
                        pbxDPI.Enabled = pbxMOCH.Enabled = true;
                    }
                    else
                    {
                        lblNumeroId.Enabled = txtNumeroId.Enabled = false;
                        lblNumeroSerie.Enabled = txtNumeroSerie.Enabled = true;
                        pbxDPI.Enabled = pbxMOCH.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbTiposDocumento_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbTiposDocumento_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        public DataSet ConsultaArraigosxNombres(string primerNombre, string segundoNombre, string primerApellido, string segundoApellido)
        {
            DataSet dsResultado = ArmarDsResultado();
            //try
            //{
            //    Arraigos arraigo;

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/arraigo_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

            //    var user = "migracion-pasaportes-enrollment-3.0";
            //    var password = "abc123";

            //    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
            //    request.Headers.Add("Authorization", "Basic " + credentials);

            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //    using (Stream stream = response.GetResponseStream())
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        var json = reader.ReadToEnd();
            //        arraigo = JsonConvert.DeserializeObject<Arraigos>(json);
            //    }

            //    string s = JsonConvert.SerializeObject(arraigo.data);
            //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

            //    arraigo.informacionArraigos = new DataSet();
            //    arraigo.informacionArraigos.Tables.Add(dt);

            //    dsResultado.Tables[0].Rows[0]["DATOS"] = arraigo;
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            //}
            //catch (Exception ex)
            //{
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
            //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaArraigosxNombres(). " + ex.Message;
            //}
            return dsResultado;
        }

        public DataSet ConsultaAlertasxNombres(string primerNombre, string segundoNombre, string primerApellido, string segundoApellido)
        {
            DataSet dsResultado = ArmarDsResultado();
            //try
            //{
            //    Alertas alertas;

            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/alertas_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

            //    var user = "migracion-pasaportes-enrollment-3.0";
            //    var password = "abc123";

            //    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
            //    request.Headers.Add("Authorization", "Basic " + credentials);

            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //    using (Stream stream = response.GetResponseStream())
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        var json = reader.ReadToEnd();
            //        alertas = JsonConvert.DeserializeObject<Alertas>(json);
            //    }
            //    string s = JsonConvert.SerializeObject(alertas.data);
            //    DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

            //    alertas.informacionAlerta = new DataSet();
            //    alertas.informacionAlerta.Tables.Add(dt);

            //    dsResultado.Tables[0].Rows[0]["DATOS"] = alertas;
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            //}
            //catch (Exception ex)
            //{
            //    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
            //    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaAlertaxNombres(). " + ex.Message;
            //}
            return dsResultado;
        }
    

        private Task<DataSet> ConsultarDocumentoPago(string tipoDocumento, string valor)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                try
                {

                    var user = "migracion-pasaportes-enrollment-3.0";
                    var password = "abc123";
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/boleta_banrural_pasaportes?boleta=" + valor);

                    switch (tipoDocumento)
                    {
                        case "CUI":

                            break;

                        case "BOLETA":
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/boleta_banrural_pasaportes?boleta=" + valor);

                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                BOLETA vBoleta = JsonConvert.DeserializeObject<BOLETA>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vBoleta;
                            }

                            break;

                        case "TRANSACCION":
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/transaccion_banrural_pasaportes?transaccion=" + valor);

                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                TRANSACCION vBoleta = JsonConvert.DeserializeObject<TRANSACCION>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vBoleta;
                            }
                            break;

                        case "RECIBO":

                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/recibo_banrural_pasaportes?recibo=" + valor);

                            request.Headers.Add("Authorization", "Basic " + credentials);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var json = reader.ReadToEnd();
                                RECIBO vRecibo = JsonConvert.DeserializeObject<RECIBO>(json);
                                dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"] = vRecibo;
                            }
                            break;
                    }

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarDocumentoPago(). " + ex.Message;
                }
                return dsResultado;
            });

        }

        private async void txtNoRecibo_KeyPress(object sender, KeyPressEventArgs e)
        {            
        }        

        private void chkVistaEnVivo_CheckedChanged(object sender, EventArgs e)
        {          
        }

        private void chkIcao_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //pbxFondoIcao.BackColor = Color.White;
                //lblMatchFace.Text = string.Empty;
                //pbxMOCF.Image = pbxWarning.Image;

                txtObservacionesIcao.Visible = !chkIcao.Checked;
                txtObservacionesIcao.BackColor = txtObservacionesIcao.Visible ? Color.Yellow : Color.White;

                //if (txtObservacionesIcao.Visible)
                //    txtObservacionesIcao.TextChanged += TextBoxLeave_Action;
                //else
                //    txtObservacionesIcao.TextChanged -= TextBoxLeave_Action;
            }
            catch (Exception ex)
            {
                MessageBox.Show("chkIcao_CheckedChanged(). " + ex.Message);
                txtMensaje.Text = "chkIcao_CheckedChanged(). " + ex.Message;
            }
        }
       
        private void tmrHora_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToLongTimeString();
        }

        private void txtEstatura_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                e.Handled = !(char.IsDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back);
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtEstatura_KeyPress(). " + ex.Message);
                txtMensaje.Text = "txtEstatura_KeyPress(). " + ex.Message;
            }
        }

        private void tsmiGirarIzquierdaProbatorio_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                labelTemp.Image.RotateFlip(RotateFlipType.Rotate90FlipXY);
                                labelTemp.Refresh();

                                Image imgReal = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());//Image.FromFile(labelTemp.Image.Tag.ToString());
                                imgReal.RotateFlip(RotateFlipType.Rotate90FlipXY);

                                try { File.Delete(labelTemp.Image.Tag.ToString());} catch { }

                                imgReal.Save(labelTemp.Image.Tag.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);
                                imgReal.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("tsmiGirarIzquierdaProbatorio_Click(). " + ex.Message);
                txtMensaje.Text = "tsmiGirarIzquierdaProbatorio_Click(). " + ex.Message;
            }
        }

        private void tsmiGirarDerechaProbatorio_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                labelTemp.Image.RotateFlip(RotateFlipType.Rotate270FlipXY);
                                labelTemp.Refresh();

                                Image imgReal = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());//Image.FromFile(labelTemp.Image.Tag.ToString());
                                imgReal.RotateFlip(RotateFlipType.Rotate270FlipXY);

                                try { File.Delete(labelTemp.Image.Tag.ToString());} catch { }

                                imgReal.Save(labelTemp.Image.Tag.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);
                                imgReal.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("tsmiGirarDerechaProbatorio_Click(). " + ex.Message);
                txtMensaje.Text = "tsmiGirarDerechaProbatorio_Click(). " + ex.Message;
            }
        }       

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {                
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            if (sourceControl is Label)
                            {
                                Label labelTemp = (Label)(sourceControl);
                                Image imgTemp = LoadBitmapUnlocked(labelTemp.Image.Tag.ToString());//CreateNonIndexedImage(labelTemp.Image.Tag.ToString());

                                int anchoProbatorio, altoProbatorio = 0;
                                anchoProbatorio = imgTemp.Width;
                                altoProbatorio = imgTemp.Height;

                                //-1 = Horizontal, 0 = Cuadrado, 1 = Vertical
                                int orientacionProbatorio = 0;

                                if (anchoProbatorio > altoProbatorio)
                                    orientacionProbatorio = -1;
                                else if (altoProbatorio > anchoProbatorio)
                                    orientacionProbatorio = 1;
                                else
                                    orientacionProbatorio = 0;


                                System.Drawing.Rectangle pantalla = Screen.FromControl(this).Bounds;
                                int anchoPantalla, altoPantalla = 0;

                                anchoPantalla = pantalla.Width;
                                altoPantalla = pantalla.Height;

                                VisorProbatorio visor = new VisorProbatorio();
                                visor.Size = new System.Drawing.Size(anchoPantalla, altoPantalla);

                                float relacionAspecto = float.Parse(anchoProbatorio.ToString()) / float.Parse(altoProbatorio.ToString());
                                
                                if (orientacionProbatorio == 1)
                                {
                                    int anchoVisor = Convert.ToInt16(altoPantalla * relacionAspecto);
                                    int altoVisor = altoPantalla - 40;
                                    visor.Size = new System.Drawing.Size(anchoVisor, altoVisor);
                                }   
                                else
                                {
                                    int anchoVisor = Convert.ToInt16(altoPantalla / relacionAspecto);
                                    int altoVisor = altoPantalla - 40;
                                    visor.Size = new System.Drawing.Size(altoVisor, anchoVisor);
                                }
                                    
                                visor.pbxProbatorio.Image = imgTemp;
                                //imgTemp.Dispose();

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                                visor.ShowDialog();
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("abrirToolStripMenuItem_Click(). " + ex.Message);
                txtMensaje.Text = "abrirToolStripMenuItem_Click(). " + ex.Message;
            }
        }

        private void toolSMIEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to cast the sender to a ToolStripItem
                ToolStripItem menuItem = sender as ToolStripItem;
                if (menuItem != null)
                {
                    // Retrieve the ContextMenuStrip that owns this ToolStripItem
                    ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                    if (owner != null)
                    {
                        // Get the control that is displaying this context menu
                        Control sourceControl = owner.SourceControl;

                        if (sourceControl != null)
                        {
                            Label lbl = (Label)sourceControl;

                            try{File.Delete(lbl.Image.Tag.ToString());}catch { }

                            string rutaThumbnail = lbl.Image.Tag.ToString().Split('.')[0] + "_thumbnail." + lbl.Image.Tag.ToString().Split('.')[1];

                            try { File.Delete(rutaThumbnail); } catch { }

                            lbl.Image.Dispose();
                            lbl.Dispose();                            

                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();

                            ValidarProbatorios();

                            Control.ControlCollection controlCollection = splitContainer1.Panel2.Controls;

                            if (controlCollection.Count == 0)
                                x = y = 10;
                            else
                            {
                                int xx;
                                int yy;
                                int imagenes;

                                xx = yy = 10;
                                imagenes = 0;

                                foreach (Control control in controlCollection)
                                {
                                    if (control is Label)
                                    {
                                        imagenes++;

                                        Label labelTemp = (Label)(control);
                                        labelTemp.Location = new System.Drawing.Point(xx, splitContainer1.Panel2.AutoScrollPosition.Y + yy);

                                        xx += ancho + 10;

                                        if (xx > (splitContainer1.Panel2.Width - ancho))
                                        {
                                            yy += alto + 10;
                                            xx = 10;
                                        }
                                    }                                    
                                }
                                x = xx;
                                y = yy;
                                tamanio = imagenes;
                            }
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("toolSMIEliminar_Click(). " + ex.Message);
                txtMensaje.Text = "toolSMIEliminar_Click(). " + ex.Message;
            }
        }

        private void btnSeleccionarEscaner_Click(object sender, EventArgs e)
        {            
        }

        private void btnForzarDIzquierdo_Click(object sender, EventArgs e)
        {
            try
            {
                _biometricFingerClient.Force();
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnForzarDIzquierdo_Click(). " + ex.Message);
                txtMensaje.Text = "btnForzarDIzquierdo_Click(). " + ex.Message;
            }
        }

        private void chkHuellasAutomatico_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //btnForzarDDerecho.Enabled = btnForzarDIzquierdo.Enabled = (chkHuellasAutomatico.Checked) ? false : true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnForzarDDerecho_Click(). " + ex.Message);
                txtMensaje.Text = "btnForzarDDerecho_Click(). " + ex.Message;
            }
        }

        private void btnForzarDDerecho_Click(object sender, EventArgs e)
        {
            try
            {
                _biometricFingerClient.Force();
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnForzarDDerecho_Click(). " + ex.Message);
                txtMensaje.Text = "btnForzarDDerecho_Click(). " + ex.Message;
            }
        }

        public int contadorFotos = 0;
        public bool FotoCumpleIcao = false;

        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);                
            }
        }

        private void InvocarProcesamientoRostro()
        {
            try
            {
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");

                if (proc.Length <= 0)
                {
                    // Start the form with the file name as a parameter
                    string appPath = Application.StartupPath + "\\ENROL\\Win64x64";
                    string childPath = Path.Combine(appPath, "ProcesarRostrosNeuro.exe");
                    Process.Start(childPath, this.Handle.ToString());
                }
                else
                {
                    string windowTitle = "Procesamiento de Rostros";
                    // Find the window with the name of the main form
                    IntPtr ptrWnd = NativeMethods.FindWindow(null, windowTitle);
                    if (ptrWnd == IntPtr.Zero)
                    {
                        string appPath = Application.StartupPath + "\\ENROL\\Win64x64";
                        string childPath = Path.Combine(appPath, "ProcesarRostrosNeuro.exe");
                        Process.Start(childPath, this.Handle.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("BtnIniciarChild_Click(). " + ex.Message);
            }
        }
        private void ActivarProcesamientoRostro(string strComando)
        {
            string windowTitle = "Procesamiento de Rostros";
            // Find the window with the name of the main form
            IntPtr ptrWnd = NativeMethods.FindWindow(null, windowTitle);
            if (ptrWnd == IntPtr.Zero)
            {
                MessageBox.Show(String.Format("No window found with the title '{0}'.", windowTitle), "SendMessage", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                IntPtr ptrCopyData = IntPtr.Zero;
                try
                {
                    // Create the data structure and fill with data
                    NativeMethods.COPYDATASTRUCT copyData = new NativeMethods.COPYDATASTRUCT();
                    copyData.dwData = new IntPtr(2);    // Just a number to identify the data type
                    copyData.cbData = strComando.Length + 1;// "Enter".Length + 1;// textBox1.Text.Length + 1;  // One extra byte for the \0 character
                    copyData.lpData = Marshal.StringToHGlobalAnsi(strComando);//"Enter");

                    // Allocate memory for the data and copy
                    ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
                    Marshal.StructureToPtr(copyData, ptrCopyData, false);

                    // Send the message
                    NativeMethods.SendMessage(ptrWnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, ptrCopyData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "SendMessage", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Free the allocated memory after the contol has been returned
                    if (ptrCopyData != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(ptrCopyData);
                }
            }
        }
        private Color GetColorForConfidence(NIcaoWarnings warnings, NIcaoWarnings flag, byte confidence)
        {
            if ((warnings & flag) == flag)
            {
                //return confidence <= 100 ? WarningColor : IndeterminateColor;
                return confidence <= 100 ? Color.Red : Color.Orange;
            }
            //return NoWarningColor;
            return Color.Green;
        }

        private Color GetColorForFlags(NIcaoWarnings warnings, params NIcaoWarnings[] flags)
        {
            //return flags.Any(f => (f & warnings) == f) ? WarningColor : NoWarningColor;
            return flags.Any(f => (f & warnings) == f) ? Color.Red : Color.Green;
        }

        
      private async void tab_principal_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab_principal.SelectedTab.Name.Equals("tabProbatorios"))
                    btn_escanear.Focus();

                await ValidarCampos();
            }
            catch (Exception ex)
            {
            }
        }        

        private DataSet ValidarIdentidad()
        {
            DataSet dsResultado = ArmarDsResultado();
            string msgError = string.Empty;

            try
            {                                
                //if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                {
                    
                    int edad = DateTime.Now.Year - dtpFechaNacimiento.Value.Year;
                    if (DateTime.Now.Month < dtpFechaNacimiento.Value.Month || (DateTime.Now.Month == dtpFechaNacimiento.Value.Month && DateTime.Now.Day < dtpFechaNacimiento.Value.Day))
                        edad--;

                    //txtCui.Text = txtCui.Text.Trim();
                    //if (txtCui.Text.Equals(string.Empty) == true || txtCui.Text.Equals("") == true)
                    //    msgError += "Ingrese un CUI. ";

                    //if ((txtCui.Text.Length == txtCui.MaxLength) == false)
                    //    msgError += "El CUI debe contener " + txtCui.MaxLength.ToString() + " números. ";

                    //if (txtCui.Text.All(char.IsDigit) == false)
                    //    msgError += "El CUI debe contener únicamente NÚMEROS. ";

                    txtPrimerNombre.Text = txtPrimerNombre.Text.Trim();
                    if(txtPrimerNombre.Text.Equals("") || txtPrimerNombre.Text.Equals(string.Empty))
                        msgError += "Ingrese primer nombre. ";

                    //txtSegundoNombre.Text = txtSegundoNombre.Text.Trim();
                    //txtTercerNombre.Text = txtTercerNombre.Text.Trim();

                    txtPrimerApellido.Text = txtPrimerApellido.Text.Trim();
                    if (txtPrimerApellido.Text.Equals("") || txtPrimerApellido.Text.Equals(string.Empty))
                        msgError += "Ingrese primer apellido. ";

                    //txtSegundoApellido.Text = txtSegundoApellido.Text.Trim();

                    //txtApellidoCasada.Text = txtApellidoCasada.Text.Trim();

                    //if (cmbGenero.Text.Equals("MASCULINO") && !(txtApellidoCasada.Text.Equals("") || txtApellidoCasada.Text.Equals(string.Empty)))
                    //{
                    //    txtApellidoCasada.Enabled = true;
                    //    msgError += "Borre apellido de casada. ";
                    //}

                    //if (cmbGenero.Enabled)
                    //    if(cmbGenero.Text.Equals("") || cmbGenero.Text.Equals(string.Empty))
                    //        msgError += "Seleccione un género. ";

                    //if (cmbEstadoCivil.Enabled)
                    //    if (cmbEstadoCivil.Text.Equals("") || cmbEstadoCivil.Text.Equals(string.Empty))
                    //        msgError += "Seleccione un estado civil. ";

                    //if (cmbOcupaciones.Enabled)
                    //    if (cmbOcupaciones.Text.Equals("") || cmbOcupaciones.Text.Equals(string.Empty))
                    //        msgError += "Seleccione una ocupación. ";

                    //if(cmbTiposDocumento.SelectedIndex < 0)
                    //    msgError += "Seleccione un tipo de documento de identificación. ";

                    //txtNumeroSerie.Text = txtNumeroSerie.Text.Trim();

                    //txtCui.Text = txtCui.Text.Trim();
                    //if (txtCui.Text.Equals(string.Empty) || txtCui.Text.Equals("") || txtCui.Text.Length != txtCui.MaxLength || (txtCui.Text.All(char.IsDigit) == false))
                    //    msgError += "Ingrese un CUI válido. ";

                    //if (cmbPaisNacimiento.SelectedIndex < 0)
                    //    msgError += "Seleccione un país de nacimiento. ";
                    //else
                    //{
                    //    if (cmbPaisNacimiento.SelectedValue.Equals("320"))
                    //    {
                    //        msgError += (cmbDeptoNacimiento.SelectedIndex < 0) ? "Seleccione departamento de nacimiento. " : string.Empty;
                    //        msgError += (cmbMunicNacimiento.SelectedIndex < 0) ? "Seleccione municipio de nacimiento. " : string.Empty;
                    //    }
                    //    else
                    //    {
                    //        txtDepartamentoNacimiento.Text = txtDepartamentoNacimiento.Text.Trim();
                    //        msgError += (txtDepartamentoNacimiento.Equals("") || txtDepartamentoNacimiento.Equals(string.Empty)) ? "Ingrese un departamento de residencia. " : string.Empty;
                    //    }
                    //}

                    //txtResidencia1.Text = txtResidencia1.Text.Trim();
                    //if (txtResidencia1.Text.Equals(string.Empty) == true || txtResidencia1.Text.Equals("") == true)
                    //    msgError += "Ingrese dirección de residencia. ";

                    //if (cmbPaisResidencia.SelectedIndex < 0)
                    //    msgError += "Seleccione un país de residencia. ";
                    //else
                    //{
                    //    if (cmbPaisResidencia.SelectedValue.Equals("320"))
                    //    {
                    //        msgError += (cmbDeptoResidencia.SelectedIndex < 0) ? "Seleccione departamento de residencia. " : string.Empty;
                    //        msgError += (cmbMunicResidencia.SelectedIndex < 0) ? "Seleccione municipio de residencia. " : string.Empty;
                    //    }
                    //}

                    ////(###)###-#### = 13
                    ////####-#### 9
                    ////txtTelCelular.Text = txtTelCelular.Text.Trim();
                    ////if (txtTelCelular.Text.Equals(string.Empty) == true || txtTelCelular.Text.Equals("") == true || txtTelCelular.MaskFull == false)
                    ////    msgError += "Ingrese un número de celular valido (" + txtTelCelular.Mask + "). ";

                    //if(txtTelCasa.Text.Any(char.IsDigit))
                    //    if(txtTelCasa.MaskFull == false)
                    //        msgError += "Ingrese un número de teléfono de casa valido (" + txtTelCasa.Mask + "). ";

                    //if (txtTelTrabajo.Text.Any(char.IsDigit))
                    //    if (txtTelTrabajo.MaskFull == false)
                    //        msgError += "Ingrese un número de teléfono de trabajo valido (" + txtTelTrabajo.Mask + "). ";

                    //if (txtCui.Text.Length > 0)
                    //{
                    //    if (txtCui.Enabled)
                    //    {
                    //        DataSet dsDeptoMunicEmisionDPI = Depto_Munic_EmisionDPI(txtCui.Text.Substring(txtCui.Text.Length - 4, 2), txtCui.Text.Substring(txtCui.Text.Length - 2, 2));

                    //        if (bool.Parse(dsDeptoMunicEmisionDPI.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    //            msgError += dsDeptoMunicEmisionDPI.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                    //    }
                    //}

                    txtUsuario.Text = txtUsuario.Text.Trim();

                    if (chkListPrivilegios.CheckedItems.Count != 1)
                        msgError += "Seleccione sólo un privilegio. ";

                    if (txtContrasenia.Enabled)
                    {
                        string strContrasenia = string.Concat(txtContrasenia.Text.Where(char.IsLetterOrDigit));
                        if (txtContrasenia.Text.Equals(strContrasenia) == false)
                            msgError += "Ingrese letras y números únicamente. ";

                        if (txtContrasenia.Text.Equals(txtContrasenia2.Text) == false)
                            msgError += "Las contraseñas no coinciden. ";
                    }

                    //if (cmbJefes.SelectedIndex < 0)
                    //    msgError += "Seleccione un Jefe o Encargado. ";

                    if (msgError.Equals(string.Empty) == false)
                        throw new Exception(msgError);

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    //tab_principal.TabPages["tabIdentidad"].ImageKey = "check.bmp";
                }

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarIdentidad(). " + ex.Message;
                //tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
            }

            return dsResultado;
        }

        private DataSet ValidarFotografia()
        {
            //return Task.Run(() =>
            //{
            DataSet dsResultado = ArmarDsResultado();
            string msgError = string.Empty;

            try
            {
                //if (faceView2.Face == null)
                if (pbxRostroIcao.Image == null)
                    msgError += "No se encontró fotografía de entrega. ";

                if (msgError.Equals(string.Empty) == false || msgError.Equals("") == false)
                    throw new Exception(msgError);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarFotografia(). " + ex.Message;
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
            }

            return dsResultado;

            //});
        }

        private async Task<DataSet> ValidarHuellas()
        {
            DataSet ds = ArmarDsResultado();

            string mensajeError = string.Empty;

            try
            {
                if ((tab_principal.TabPages["tabHuellas"].ImageKey.Equals("check.bmp") == false && !pbxHitSibio.Image.Tag.Equals("Loading")) || pbxHitSibio.Image.Tag.Equals("Warning"))
                {
                    if (cmbDedoDerecho.Text.ToUpper().Contains("NINGUNO") == false || cmbDedoIzquierdo.Text.ToUpper().Contains("NINGUNO") == false)
                    {
                        if (_subjectFingerDerecho == null)
                            mensajeError += "NFingerDerecho es nulo. ";

                        if (_subjectFingerIzquierdo == null)
                            mensajeError += "NFingerIzquierdo es nulo. ";

                        if (mensajeError.Equals(string.Empty) == false)
                            throw new Exception(mensajeError);

                        if (_subjectFingerDerecho.Status != NBiometricStatus.Ok)
                            mensajeError += "NFingerDerecho estatus diferente de OK. ";

                        if (_subjectFingerIzquierdo.Status != NBiometricStatus.Ok)
                            mensajeError += "NFingerIzquierdo estatus diferente de OK. ";

                        if (nFVDDerecho.Finger == null)
                            mensajeError += "NFingerViewDerecho estatus diferente de OK. ";

                        if (nFVDIzquierdo.Finger == null)
                            mensajeError += "NFingerViewIzquierdo estatus diferente de OK. ";

                        if (mensajeError.Equals(string.Empty) == false)
                            throw new Exception(mensajeError);

                        string dedo = cmbDedoDerecho.Text.ToUpper();

                        //string dedo = cmbDedoDerecho.SelectedItem

                        txtComentarioDDerecho.Text = txtComentarioDDerecho.Text.Trim();
                        txtComentarioDIzquierdo.Text = txtComentarioDIzquierdo.Text.Trim();

                        dedo = cmbDedoIzquierdo.Text.ToUpper();

                        DataSet dsHuellasIguales = await HuellasIguales(_subjectFingerDerecho, _subjectFingerIzquierdo);
                        //await HuellasIguales(dsHuellasIguales, _subjectFingerDerecho, _subjectFingerIzquierdo);

                        if (dsHuellasIguales == null)
                            mensajeError += "Error al comparar 2 huellas. ";

                        if (dsHuellasIguales.Tables.Count < 1)
                            mensajeError += "Error al comparar 2 huellas (2). ";

                        if (dsHuellasIguales.Tables[0].Rows.Count < 1)
                            mensajeError += "Error al comparar 2 huellas (3). ";

                        if (bool.Parse(dsHuellasIguales.Tables[0].Rows[0]["RESULTADO"].ToString()) == true)
                            mensajeError += "Las huellas son iguales. ";//MessageBox.Show("Las huellas son iguales. Detalle: " + dsHuellasIguales.Tables[0].Rows[0]["MSG_ERROR"].ToString());//mensajeError += "Las huellas son iguales. ";

                        if (!mensajeError.Equals(string.Empty))
                            throw new Exception(mensajeError);

                        //if (cmbTiposDocumento.Text.Equals(string.Empty) || cmbTiposDocumento.Text.Equals(""))
                        //    mensajeError += "Ingrese un documento de identificación. ";

                        //if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                        {
                            if (cmbTiposDocumento.Text.ToUpper().Contains("DPI") || cmbTiposDocumento.Text.ToUpper().Contains("CUI"))
                            {
                                //pbxHitSibio.Image = pbxWarningColor.Image;
                                pbxHitSibio.Image = pbxLoad.Image;

                                //VALIDACIÓN HIT
                                string WsqH1 = "";
                                string WsqH2 = "";

                                byte[] abHuellaD = _subjectFingerDerecho.Image.Save(NImageFormat.Png).ToArray();
                                Image iHuellaDigitalD;

                                using (var stream = new MemoryStream(abHuellaD, 0, abHuellaD.Length))
                                {
                                    iHuellaDigitalD = Image.FromStream(stream);

                                    DataSet dsHuellaFinal = GenerarHuellaPNG_STRING_BASE64(iHuellaDigitalD);

                                    if (dsHuellaFinal == null || dsHuellaFinal.Tables.Count < 1 || dsHuellaFinal.Tables[0].Rows.Count < 1)
                                        throw new Exception("¡Error al generar la huella derecha!");

                                    if (bool.Parse(dsHuellaFinal.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception("!Error al generar la huella derecha!. " + dsHuellaFinal.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    //pasaporte_xml.HuellaPNG1 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                                    WsqH1 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                                }
                                ///MANO DERECHA

                                /// MANO IZQUIERDA

                                byte[] abHuellaI = _subjectFingerIzquierdo.Image.Save(NImageFormat.Png).ToArray();
                                Image iHuellaDigitalI;

                                using (var stream = new MemoryStream(abHuellaI, 0, abHuellaI.Length))
                                {
                                    iHuellaDigitalI = Image.FromStream(stream);

                                    DataSet dsHuellaFinal = GenerarHuellaPNG_STRING_BASE64(iHuellaDigitalI);

                                    if (dsHuellaFinal == null || dsHuellaFinal.Tables.Count < 1 || dsHuellaFinal.Tables[0].Rows.Count < 1)
                                        throw new Exception("¡Error al generar la huella izquierda!");

                                    if (bool.Parse(dsHuellaFinal.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception("!Error al generar la huella izquierda!. " + dsHuellaFinal.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    //pasaporte_xml.HuellaPNG2 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                                    WsqH2 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                                }
                                if (Properties.Settings.Default.VALIDAR_HIT && txtCui.Text.Equals(string.Empty) == false && txtCui.Text.Equals("") == false)
                                {
                                    DataSet dsResultado = await ConsultarHuellasSIBIO(txtCui.Text, WsqH1, "Left" + DedosRenap(cmbDedoIzquierdo.Text), WsqH2, "Right" + DedosRenap(cmbDedoDerecho.Text));

                                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    {
                                        lblHitSibio.Text = "NO_HIT";
                                        lblHitSibio.ForeColor = Color.Red;
                                        pbxHitSibio.Image = pbxWarning.Image;

                                        lblHitSibio.Text = "Msg: " + mensajeError;

                                        //mensajeError = dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                                        //MessageBox.Show(mensajeError, "ValidarHuellas");
                                    }
                                    else
                                    {
                                        Sibio vSibio = (Sibio)dsResultado.Tables[0].Rows[0]["DATOS_SIBIO"];

                                        if (vSibio.data.Equals("HIT"))
                                        {
                                            lblHitSibio.Text = "HIT";
                                            lblHitSibio.ForeColor = Color.Green;
                                            pbxHitSibio.Image = pbxCheck.Image;
                                        }
                                        else if (vSibio.data.Equals("NO_HIT"))
                                        {
                                            lblHitSibio.Text = "NO_HIT";
                                            lblHitSibio.ForeColor = Color.Red;
                                            pbxHitSibio.Image = pbxWarning.Image;
                                        }
                                        else
                                        {
                                            lblHitSibio.Text = "Msg: " + vSibio.mensaje;
                                            lblHitSibio.ForeColor = Color.Black;
                                            pbxHitSibio.Image = pbxWarning.Image;
                                        }
                                    }
                                }
                            }

                            //if (mensajeError.Equals(string.Empty) == false)
                            //MessageBox.Show(mensajeError, "ValidarHuellas");//throw new Exception(mensajeError);
                        }

                        ds.Tables[0].Rows[0]["RESULTADO"] = true;
                        ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarHuellas(). " + ex.Message;
                pbxHitSibio.Image = pbxWarning.Image;
                //tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";
            }

            return ds;
        }

        private string DedosRenap(string valor)
        {
            //1 - Indice
            //2 - Pulgar
            //3 - Medio
            //4 - Anular
            //5 - Meñique
            //6 - Ninguno

            switch (valor)
            {
                case "1":
                case "Indice":
                case "1 - Indice":
                    return "Index";
                    break;

                case "2":
                case "Pulgar":
                case "2 - Pulgar":
                    return "Thumb";
                    break;

                case "3":
                case "Medio":
                case "3 - Medio":
                    return "Middle";
                    break;

                case "4":
                case "Anular":
                case "4 - Anular":
                    return "Ring";
                    break;

                case "5":
                case "Meñique":
                case "5 - Meñique":
                    return "Little";
                    break;

                default:
                    return "";

            }
        }

        public Task<DataSet> ConsultarHuellasSIBIO(string vCui, string vFingerLeft, string vLeftCod, string vFingerRight, string vRightCod)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                try
                {
                    SibioParametrosConsulta vParametrosSibio = new SibioParametrosConsulta();
                    vParametrosSibio.cui = vCui;
                    vParametrosSibio.fingerleft = vFingerLeft;
                    vParametrosSibio.leftcod = vLeftCod;
                    vParametrosSibio.fingerright = vFingerRight;
                    vParametrosSibio.rightcod = vRightCod;

                    string postString = JsonConvert.SerializeObject(vParametrosSibio);

                    byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                    HttpWebRequest request;
                    request = WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/renap_sibio") as HttpWebRequest;
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

                    Sibio vSibio = JsonConvert.DeserializeObject<Sibio>(body);
                    dsResultado.Tables[0].Rows[0]["DATOS_SIBIO"] = vSibio;

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultarHuellasSIBIO(). " + ex.Message;
                }
                return dsResultado;
            }
            );
        }

        private DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("DATOS", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_PAGO_PASAPORTE", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_SIBIO", typeof(object));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }

        private async Task<DataSet> HuellasIguales(NFinger fingerA, NFinger fingerB)
        {

            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                NSubject _subjectA;
                NSubject _subjectB;

                _subjectA = new NSubject();
                _subjectA.Fingers.Add(new NFinger { Image = fingerA.Image });

                _subjectB = new NSubject();
                _subjectB.Fingers.Add(new NFinger { Image = fingerB.Image });

                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                {
                    string huellaA = Convert.ToBase64String(fingerA.Image.Save(NImageFormat.Wsq).ToArray());
                    string huellaB = Convert.ToBase64String(fingerB.Image.Save(NImageFormat.Wsq).ToArray());
                    DataSet dsBiometria = wsBiometricsDGM.CompararDosHuellasStrBase64IMG_WSQ(huellaA, huellaB);

                    DataSet dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                    dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                        throw new Exception("¡Las huellas son distintas!");

                }
                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                {
                    NBiometricClient nBiometricClient;
                    nBiometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };
                    await nBiometricClient.InitializeAsync();

                    var status = await nBiometricClient.CreateTemplateAsync(_subjectA);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir huella DERECHA para comparación");

                    status = await nBiometricClient.CreateTemplateAsync(_subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("Error al convertir huella IZQUIERDA para comparación");

                    status = await _biometricFingerClient.VerifyAsync(_subjectA, _subjectB);
                    //status = _biometricFingerClient.Verify(_subjectA, _subjectB);

                    if (status != NBiometricStatus.Ok)
                        throw new Exception("¡Las huellas son distintas!");
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = MethodBase.GetCurrentMethod().Name + "(). " + ex.Message;
            }

            return ds;
        }

        private async Task<DataSet> RostrosIguales(Image imgRostroA, Image imgRostroB)
        {

            DataSet ds = ArmarDsResultado();
            ds.Tables[0].Rows[0]["RESULTADO"] = false;

            try
            {
                var status = NBiometricStatus.None;

                var msRostroA = new MemoryStream();
                imgRostroA.Save(msRostroA, ImageFormat.Png);

                var msRostroB = new MemoryStream();
                imgRostroB.Save(msRostroB, ImageFormat.Png);

                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                {

                    string strRostroA = Convert.ToBase64String(msRostroA.ToArray());
                    string strRostroB = Convert.ToBase64String(msRostroA.ToArray());
                    DataSet dsBiometria = wsBiometricsDGM.CompararDosRostrosStrBase64IMG(strRostroA, strRostroB);

                    DataSet dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                    dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == false)
                        throw new Exception("¡Los rostros son distintos!");
                }
                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                {
                    NSubject vNSubjectDPI = new NSubject();
                    NSubject vNSubjectPasaporte = new NSubject();

                    vNSubjectDPI.Faces.Add(new NFace() { SampleBuffer = NBuffer.FromArray(msRostroA.ToArray()) });
                    vNSubjectPasaporte.Faces.Add(new NFace() { SampleBuffer = NBuffer.FromArray(msRostroA.ToArray()) });

                    string msgError = "";
                    status = await _biometricFaceClient.CreateTemplateAsync(vNSubjectDPI);
                    if (status != NBiometricStatus.Ok)
                        msgError += "Error al generar la plantilla en base a la foto del DPI. ";

                    status = await _biometricFaceClient.CreateTemplateAsync(vNSubjectPasaporte);
                    if (status != NBiometricStatus.Ok)
                        msgError += "Error al generar la plantilla en base a la foto del PASAPORTE. ";

                    if (vNSubjectDPI == null)
                        msgError += "La plantilla de la foto del DPI es nula. ";

                    if (vNSubjectPasaporte == null)
                        msgError += "La plantilla de la foto del PASAPORTE es nula. ";

                    if (msgError.Equals(string.Empty) == false || msgError.Equals("") == false)
                        throw new Exception(msgError);

                    if (msgError.Equals(string.Empty))
                    {
                        status = await _biometricFaceClient.VerifyAsync(vNSubjectDPI, vNSubjectPasaporte);

                        if (status != NBiometricStatus.Ok)
                            throw new Exception("¡Las huellas son distintas!");
                    }
                }

                msRostroA.Dispose();
                msRostroB.Dispose();

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = MethodBase.GetCurrentMethod().Name + "(). " + ex.Message;
            }

            return ds;
        }

        private Task<DataSet> ValidarFirma()
        {
            return Task.Run(() =>
            {
                CheckForIllegalCrossThreadCalls = false;                

                DataSet ds = ArmarDsResultado();
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                return ds;

                string mensajeError = string.Empty;

                try
                {
                    if (tab_principal.TabPages["tabFirma"].ImageKey == "warning.bmp")
                    {
                        if (chkNoPuedeFirmar.Checked == false)
                        {
                            int ancho = 500;
                            int alto = 150;

                            //FALTA VALIDACIÓN DE FIRMA
                            //Image firma = sigPlusNET1.GetSigImage();
                            Bitmap bit = new Bitmap(500, 150);

                            bool esBlanco = EsImagenEnBlanco(ancho, alto, bit);

                            if (esBlanco)
                                mensajeError += "La firma esta en blanco. ";

                            if (mensajeError.Equals(string.Empty) == false)
                                throw new Exception(mensajeError);

                            tab_principal.TabPages["tabFirma"].ImageKey = "check.bmp";

                            ds.Tables[0].Rows[0]["RESULTADO"] = true;
                            ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                        }
                        else
                        {
                            ds.Tables[0].Rows[0]["RESULTADO"] = true;
                            ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ds.Tables[0].Rows[0]["RESULTADO"] = false;
                    ds.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarFirma(). " + ex.Message;
                    tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                }
                
                return ds;
            });
        }

        private bool EsImagenEnBlanco(int ancho, int alto, Bitmap bit)
        {
            bool imagenEnBlanco = true;
            try
            {
                string blancoAlpha = "ffffffff";

                using (Graphics graph = Graphics.FromImage(bit))
                {
                    int i = 1;
                    while (imagenEnBlanco && i < alto)
                    {


                        String colorA = bit.GetPixel(125, i).Name;
                        Color colorB = bit.GetPixel(250, i);
                        Color colorC = bit.GetPixel(375, i);

                        bool b1 = bit.GetPixel(125, i).Name != Color.White.Name;
                        bool b2 = bit.GetPixel(250, i).Name != Color.White.Name;
                        bool b3 = bit.GetPixel(375, i).Name != Color.White.Name;

                        if (bit.GetPixel(125, i).Name != blancoAlpha || bit.GetPixel(250, i).Name != blancoAlpha || bit.GetPixel(375, i).Name != blancoAlpha)
                            imagenEnBlanco = false;

                        i++;
                    }

                    int j = 1;
                    while (imagenEnBlanco && j < ancho)
                    {
                        if (bit.GetPixel(j, 37).Name != blancoAlpha || bit.GetPixel(j, 75).Name != blancoAlpha || bit.GetPixel(j, 112).Name != blancoAlpha)
                            imagenEnBlanco = false;

                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                imagenEnBlanco = true;
                throw new Exception("EsImagenEnBlanco(). " + ex.Message);
            }

            return imagenEnBlanco;
        }
        private Task<DataSet> ValidarProbatorios()
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();
                DataSet ds = ArmarDsResultado();
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                return ds;

                string msgError = string.Empty;

                try
                {
                    int probatoriosValidos = 0;
                    Control.ControlCollection controlCollection = splitContainer1.Panel2.Controls;

                    int cantidadControles = controlCollection.Count;

                    for (int i = 0; i < cantidadControles; i++)
                    {
                        Control control = controlCollection[i];

                        if (control is Label)
                            probatoriosValidos++;

                        if (probatoriosValidos >= 3)
                            i = cantidadControles + 2;
                    }

                    int edad = DateTime.Now.Year - dtpFechaNacimiento.Value.Year;
                    if (DateTime.Now.Month < dtpFechaNacimiento.Value.Month || (DateTime.Now.Month == dtpFechaNacimiento.Value.Month && DateTime.Now.Day < dtpFechaNacimiento.Value.Day))
                        edad--;

                    int minimoProbatorios = 1;
                    if (probatoriosValidos < minimoProbatorios)
                        throw new Exception("El número de probatorios mínimo son " + minimoProbatorios + ". ");

                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                    tab_principal.TabPages["tabProbatorios"].ImageKey = "check.bmp";
                }
                catch (Exception ex)
                {
                    dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                    dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarProbatorios(). " + ex.Message;
                    tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";
                }

                return dsResultado;
            });

        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "CasoPasaporte_.xml");
                string rutaXML2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "Done", "CasoPasaporte_.xml");

                if (File.Exists(rutaXML) == false && File.Exists(rutaXML2) == false)
                    throw new Exception("¡Guarde el archivo!");

                foto = Convert.ToBase64String(_nFaceSegmented.Image.Save(NImageFormat.Png).ToArray());
                no_caso = "";
                tipo_pasaporte = "";
                nombres = txtPrimerNombre.Text.Trim();
                apellidos = txtPrimerApellido.Text.Trim();
                apellido_casada = string.Empty;
                direccion = txtResidencia1.Text + Environment.NewLine + " " + txtResidencia2.Text;
                if (cmbPaisResidencia.Text.Equals("GUATEMALA"))
                    direccion += Environment.NewLine + cmbDeptoResidencia.Text + ", " + cmbMunicResidencia.Text;
                else if (cmbPaisResidencia.Text.Equals("ESTADOS UNIDOS DE AMÉRICA"))
                    direccion += Environment.NewLine + "";

                if (txtTelCasa.Text.Any(char.IsDigit))
                    if (txtTelCasa.MaskFull == true)
                        tel_casa = txtTelCasa.Text;

                if (txtTelTrabajo.Text.Any(char.IsDigit))
                    if (txtTelTrabajo.MaskFull == true)
                        tel_trabajo = txtTelTrabajo.Text;

                if (txtTelCelular.Text.Any(char.IsDigit))
                    if (txtTelCelular.MaskFull == true)
                        tel_celular = txtTelCelular.Text;

                correo = txtEmail.Text;
                pais = cmbPaisResidencia.Text;
                sexo = cmbGenero.Text;
                estado_civil = cmbEstadoCivil.Text;
                nacionalidad = "GUATEMALA";
                fecha_nacimiento = dtpFechaNacimiento.Text;
                depto_nacimiento = cmbDeptoNacimiento.Text;
                muni_nacimiento = cmbMunicNacimiento.Text;
                pais_nacimiento = cmbPaisNacimiento.Text;

                identificacion = string.IsNullOrEmpty(txtCui.Text) ? txtNumeroId.Text : txtCui.Text;
                depto_emision = cmbDeptoDPI.Text;
                municipio_emision = cmbMunicipioDPI.Text;


                color_ojos = string.Empty;
                color_cabello = string.Empty;
                color_tez = string.Empty;
                estatura = string.Empty;
                //padre = txtNombrePadre.Text + " " + txtApellidoPadre.Text;
                //madre = txtNombreMadre.Text + " " + txtApellidoMadre.Text;

                partida_nacimiento = "";
                libro = "";
                folio = "";
                acta = "";
                pasaporte_autorizado = "Pasaporte Autorizado";
                autorizado_dgm = "DELEGADO DE MIGRACION";
                usuario = lbl_usuario.Text;//lblNombreUsuario.Text;
                estacion = Environment.MachineName;//"GUAPASSNUEVO";
                lugar_fecha = sedeEstacion.CIUDAD + ", " + DateTime.Now.ToLongDateString() + " a las  " + DateTime.Now.ToLongTimeString();

                ds_reportes.dt_identidadDataTable dt = new ds_reportes.dt_identidadDataTable();
                byte[] imageBytes = Convert.FromBase64String(FrmEnrolamiento.foto);

                DataRow row = dt.NewRow();
                row["foto"] = imageBytes;
                row["no_caso"] = FrmEnrolamiento.no_caso;
                row["tipo_pasaporte"] = FrmEnrolamiento.tipo_pasaporte;
                row["nombres"] = FrmEnrolamiento.nombres;
                row["apellidos"] = FrmEnrolamiento.apellidos;
                row["apellido_casada"] = FrmEnrolamiento.apellido_casada;
                row["direccion"] = FrmEnrolamiento.direccion;
                row["tel_casa"] = FrmEnrolamiento.tel_casa;
                row["tel_trabajo"] = FrmEnrolamiento.tel_trabajo;
                row["tel_celular"] = FrmEnrolamiento.tel_celular;
                row["correo"] = FrmEnrolamiento.correo;
                row["pais"] = FrmEnrolamiento.pais;
                row["sexo"] = FrmEnrolamiento.sexo;
                row["estado_civil"] = FrmEnrolamiento.estado_civil;
                row["nacionalidad"] = FrmEnrolamiento.nacionalidad;
                row["fecha_nacimiento"] = FrmEnrolamiento.fecha_nacimiento;
                row["depto_nacimiento"] = FrmEnrolamiento.depto_nacimiento;
                row["municipio_nacimiento"] = FrmEnrolamiento.muni_nacimiento;
                row["pais_nacimiento"] = FrmEnrolamiento.pais_nacimiento;
                row["identificacion"] = FrmEnrolamiento.identificacion;
                row["depto_emision"] = FrmEnrolamiento.depto_emision;
                row["municipio_emision"] = FrmEnrolamiento.municipio_emision;
                row["color_ojos"] = FrmEnrolamiento.color_ojos;
                row["color_tez"] = FrmEnrolamiento.color_tez;
                row["color_cabello"] = FrmEnrolamiento.color_cabello;
                row["estatura"] = FrmEnrolamiento.estatura;
                row["padre"] = FrmEnrolamiento.padre;
                row["madre"] = FrmEnrolamiento.madre;
                row["sede_entrega"] = FrmEnrolamiento.sede_entrega;
                row["partida_nacimiento"] = FrmEnrolamiento.partida_nacimiento;
                row["libro"] = FrmEnrolamiento.libro;
                row["folio"] = FrmEnrolamiento.folio;
                row["acta"] = FrmEnrolamiento.acta;
                row["pasaporte_autorizado"] = FrmEnrolamiento.pasaporte_autorizado;
                row["identificacion_padre"] = FrmEnrolamiento.identificacion_padre;
                row["identificacion_madre"] = FrmEnrolamiento.identificacion_madre;
                row["autorizado_dgm"] = FrmEnrolamiento.autorizado_dgm;
                row["usuario"] = FrmEnrolamiento.usuario;
                row["estacion"] = FrmEnrolamiento.estacion;
                row["lugar_fecha"] = FrmEnrolamiento.lugar_fecha;
                row["cui_menor"] = FrmEnrolamiento.cui_menor;
                row["tipo_entrega"] = FrmEnrolamiento.tipo_entrega;
                row["direccion_entrega1"] = FrmEnrolamiento.direccion_entrega1;
                row["direccion_entrega2"] = FrmEnrolamiento.direccion_entrega2;
                row["direccion_entrega3"] = FrmEnrolamiento.direccion_entrega3;
                dt.Rows.Add(row);

                VisorReportes visorReportes = new VisorReportes();

                BindingSource dt_identidadBindingSource = new BindingSource();
                ds_reportes ds_reportes = new ds_reportes();

                dt_identidadBindingSource.DataMember = "dt_identidad";
                dt_identidadBindingSource.DataSource = ds_reportes;

                ReportDataSource reportDataSource1 = new ReportDataSource();
                reportDataSource1.Name = "ds_reportes";
                reportDataSource1.Value = dt_identidadBindingSource;
                visorReportes.reportViewer1.LocalReport.DataSources.Add(reportDataSource1);
                visorReportes.reportViewer1.LocalReport.ReportEmbeddedResource = "ENROLLMENT_V3.Reportes.rpt_identidad.rdlc";

                dt_identidadBindingSource.DataSource = dt;
                visorReportes.reportViewer1.RefreshReport();               

                string rutaPDF = Path.Combine(Application.StartupPath, "ENROL", "PDFs", "CasoPasaporte_" + FrmEnrolamiento.no_caso.Trim() + ".pdf");                

                visorReportes.Activate();
                visorReportes.Show();

                byte[] Bytes = visorReportes.reportViewer1.LocalReport.Render(format: "PDF", deviceInfo: "");

                using (FileStream stream = new FileStream(rutaPDF, FileMode.Create))
                {
                    stream.Write(Bytes, 0, Bytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnImprimir_Click(). " + ex.Message);
                txtMensaje.Text = "btnImprimir_Click(). " + ex.Message;
            }
        }

        private void TextBoxLeave_Action(object sender, EventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = textBox.Text.Trim();
                textBox.BackColor = (textBox.Text.Equals("") || textBox.Text.Equals(string.Empty)) ? Color.Yellow : Color.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show("TextBoxLeave(). " + ex.Message);
                txtMensaje.Text = "TextBoxLeave(). " + ex.Message;
            }
        }
    }
}
