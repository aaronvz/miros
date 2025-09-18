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
using TwainLib;
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

using EOSDigital.API;
using EOSDigital.SDK;

using System.Xml.Linq;

using Microsoft.Reporting.WinForms;

using System.Diagnostics;

using ENROLLMENT_V3.Properties;
using ENROLLMENT_V3.Reportes;

using UtilityCore.Cryptography;

using ENROLLMENT_V3.WsBiometricsDGMSoap;

using System.Reflection;

namespace ENROLLMENT_V3
{
    public partial class enrollment : Form
    {

        #region Variables Cámara

        CanonAPI APIHandler;
        Camera MainCamera;
        CameraValue[] AvList;
        CameraValue[] TvList;
        CameraValue[] ISOList;
        List<Camera> CamList;
        bool IsInit = false;
        Bitmap Evf_Bmp;
        int LVBw, LVBh, w, h;
        float LVBratio, LVration;

        int ErrCount;
        object ErrLock = new object();
        object LvLock = new object();

        #endregion

        //ConfEnrollment configuracion;

        Sede sedeEstacion;

        List<DataWsUsuariosDGM> _listDataWsUsuariosDGM;

        int x, y;
        int ancho = 100;
        int alto = 137;

        WsBiometricsDGMSoapClient wsBiometricsDGM;

        public enrollment(List<DataWsUsuariosDGM> listDataWsUsuariosDGM)
        {
            _listDataWsUsuariosDGM = listDataWsUsuariosDGM;
            InitializeComponent();

            this.lbl_usuario.Text = _listDataWsUsuariosDGM[0].usuario;
            this.lblNombreUsuario.Text = _listDataWsUsuariosDGM[0].nombres + " " + _listDataWsUsuariosDGM[0].apellidos;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].foto);
                Image image;

                using (var stream = new MemoryStream(bytes, 0, bytes.Length))
                {
                    image = Image.FromStream(stream);
                    picb_usuario.Image = image;
                }

                bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella1wsq);
                var nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                NSubject nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);
                nFVDedoA.Finger = nfHuella;


                bytes = Convert.FromBase64String(listDataWsUsuariosDGM[0].huella2wsq);
                nfHuella = new NFinger { Image = NImage.FromMemory(bytes) };
                nsSujeto = new NSubject();
                nsSujeto.Fingers.Add(nfHuella);
                nFVDedoB.Finger = nfHuella;
            } catch
            {
            }


            //this.picb_usuario.Image;

            x = y = 10;
            //configuracion = new ConfEnrollment();

            APIHandler = new CanonAPI();
            APIHandler.CameraAdded += APIHandler_CameraAdded;
            ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
            ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;
            //SavePathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
            //SaveFolderBrowser.Description = "Save Images To...";
            LiveViewPicBox.Paint += LiveViewPicBox_Paint;
            LVBw = LiveViewPicBox.Width;
            LVBh = LiveViewPicBox.Height;
            RefreshCamera();
            IsInit = true;
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

        #region API Events

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try { Invoke((Action)delegate { RefreshCamera(); }); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_StateChanged(Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_ProgressChanged(object sender, int progress)
        {
            //try { Invoke((Action)delegate { MainProgressBar.Value = progress; }); }
            //catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_LiveViewUpdated(Camera sender, System.IO.Stream img)
        {
            try
            {
                lock (LvLock)
                {
                    Evf_Bmp?.Dispose();
                    Evf_Bmp = new Bitmap(img);
                }
                LiveViewPicBox.Invalidate();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            try
            {
                this.Enabled = false;
                string dir = null;
                Invoke((Action)delegate { dir = System.IO.Path.Combine(Application.StartupPath, "DCIM"); });

                Info.FileName = "Rostro.JPG";
                string rutaImagen = Application.StartupPath + "\\ENROL\\ROSTRO\\" + Info.FileName;
                //string rutaMiniatura = rutaImagen.Split('.')[0] + "_min." + Info.FileName.Split('.')[1];

                try { File.Delete(rutaImagen); } catch { }

                sender.DownloadFile(Info, Application.StartupPath + "\\ENROL\\ROSTRO\\");
                try { File.Delete(Application.StartupPath + "\\ENROL\\ROSTRO\\SegmentedFace.jpeg"); } catch { }


                //SE ASIGNARÁ LA IMAGEN SIN SEGMENTAR PRIMERAMENTE
                using (Bitmap bmp = LoadBitmapUnlocked(rutaImagen))
                {
                    NFace face = new NFace
                    {
                        Image = NImage.FromBitmap((Bitmap)(bmp.Clone())),
                        CaptureOptions = NBiometricCaptureOptions.Stream
                    };

                    bmp.Dispose();

                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();

                    NSubject _newSubject = new NSubject();
                    _newSubject.Faces.Add(face);
                    //faceView2.Face = _newSubject.Faces.First();
                    pbxRostroIcao.Image = _newSubject.Faces.First().Image.ToBitmap();
                    icaoWarningView1.Face = _newSubject.Faces.First();
                    
                }

                SetStatusText(Color.Orange, "Extrayendo plantilla...");
                //PROCESAMIENTO DE ROSTROS
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length <= 0)
                    InvocarProcesamientoRostro();
                //COMPRIMIR - GENERAR MINIATURA
                ActivarProcesamientoRostro(rutaImagen);

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "MainCamera_DownloadReady(). " + ex.Message;
                MessageBox.Show("MainCamera_DownloadReady(). " + ex.Message);
                //ReportError(ex.Message, false);
            }
            this.Enabled = true;
        }

        public void AsignarFotoRecortada(Image imagen)
        {
            try
            {
                Bitmap bmp = (Bitmap)(imagen);

                NFace face = new NFace
                {
                    Image = NImage.FromBitmap(bmp),
                    CaptureOptions = NBiometricCaptureOptions.Stream
                };

                NSubject _newSubject = new NSubject();
                _newSubject.Faces.Add(face);
                //faceView2.Face = _newSubject.Faces.First();
                pbxRostroIcao.Image = _newSubject.Faces.First().Image.ToBitmap();
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "AsignarFotoRecortada(). " + ex.Message;
                MessageBox.Show("AsignarFotoRecortada(). " + ex.Message);
            }
        }

        private void ErrorHandler_NonSevereErrorHappened(object sender, ErrorCode ex)
        {
            ReportError($"SDK Error code: {ex} ({((int)ex).ToString("X")})", false);
        }

        private void ErrorHandler_SevereErrorHappened(object sender, Exception ex)
        {
            ReportError(ex.Message, true);
        }

        #endregion

        #region Live view

        private void LiveViewButton_Click(object sender, EventArgs e)
        {

        }

        private void LiveViewPicBox_SizeChanged(object sender, EventArgs e)
        {

        }

        private void LiveViewPicBox_Paint(object sender, PaintEventArgs e)
        {

            if (MainCamera == null || !MainCamera.SessionOpen) return;

            //if (!MainCamera.IsLiveViewOn) e.Graphics.Clear(BackColor);
            if (!MainCamera.IsLiveViewOn) e.Graphics.Clear(Color.Gray);
            else
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        LVBratio = LVBw / (float)LVBh;
                        LVration = Evf_Bmp.Width / (float)Evf_Bmp.Height;
                        if (LVBratio < LVration)
                        {
                            w = LVBw;
                            h = (int)(LVBw / LVration);
                        }
                        else
                        {
                            w = (int)(LVBh * LVration);
                            h = LVBh;
                        }
                        e.Graphics.DrawImage(Evf_Bmp, 0, 0, w, h);
                    }
                }
            }
        }

        private void FocusNear1Button_Click(object sender, EventArgs e)
        {

        }

        private void FocusFar1Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far1); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusFar2Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far2); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusFar3Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far3); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        #endregion

        #region Subroutines

        private void CloseSession()
        {
            try
            {
                if (MainCamera.IsLiveViewOn) { MainCamera.StopLiveView(); }

                MainCamera.CloseSession();
                //AvCoBox.Items.Clear();
                //TvCoBox.Items.Clear();
                //ISOCoBox.Items.Clear();
                //SettingsGroupBox.Enabled = false;
                //LiveViewGroupBox.Enabled = false;
                btnCapturarRostro.Enabled = false;
                btnActivarCapturaRostro.Text = "Activar";
                SessionLabel.Text = "No open session";
                //LiveViewButton.Text = "Start LV";

                MainCamera.LiveViewUpdated -= MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged -= MainCamera_ProgressChanged;
                MainCamera.StateChanged -= MainCamera_StateChanged;
                MainCamera.DownloadReady -= MainCamera_DownloadReady;
            }
            catch (Exception ex)
            {
                //throw new Exception("CloseSession(). " + ex.Message);
                MessageBox.Show("CloseSession(). " + ex.Message);
            }
        }

        private void RefreshCamera()
        {
            try
            {
                CameraListBox.Items.Clear();
                CamList = APIHandler.GetCameraList();
                foreach (Camera cam in CamList) CameraListBox.Items.Add(cam.DeviceName);
                if (MainCamera?.SessionOpen == true) CameraListBox.SelectedIndex = CamList.FindIndex(t => t.ID == MainCamera.ID);
                else if (CamList.Count > 0) CameraListBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw new Exception("RefreshCamera(). " + ex.Message);
            }

        }

        private void btnCapturar_Click(object sender, EventArgs e)
        {
            try
            {
                MainCamera.TakePhotoShutterAsync();

                //LiveViewPicBox.Image = (Image)(pictureBox1.Image);


                //MainCamera.TakePhotoBulbAsync(30);

                //MainCamera.TakePhotoBulb(30);
                //if ((string)TvCoBox.SelectedItem == "Bulb") MainCamera.TakePhotoBulbAsync((int)BulbUpDo.Value);
                //else MainCamera.TakePhotoShutterAsync();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void OpenSession()
        {
            if (CameraListBox.SelectedIndex >= 0)
            {
                MainCamera = CamList[CameraListBox.SelectedIndex];
                MainCamera.OpenSession();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged += MainCamera_ProgressChanged;
                MainCamera.StateChanged += MainCamera_StateChanged;
                MainCamera.DownloadReady += MainCamera_DownloadReady;

                btnActivarCapturaRostro.Text = "Desactivar";
                SessionLabel.Text = MainCamera.DeviceName;
                AvList = MainCamera.GetSettingsList(PropertyID.Av);
                TvList = MainCamera.GetSettingsList(PropertyID.Tv);
                ISOList = MainCamera.GetSettingsList(PropertyID.ISO);
                //foreach (var Av in AvList) AvCoBox.Items.Add(Av.StringValue);
                //foreach (var Tv in TvList) TvCoBox.Items.Add(Tv.StringValue);
                //foreach (var ISO in ISOList) ISOCoBox.Items.Add(ISO.StringValue);
                //AvCoBox.SelectedIndex = AvCoBox.Items.IndexOf(AvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Av)).StringValue);
                //TvCoBox.SelectedIndex = TvCoBox.Items.IndexOf(TvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Tv)).StringValue);
                //ISOCoBox.SelectedIndex = ISOCoBox.Items.IndexOf(ISOValues.GetValue(MainCamera.GetInt32Setting(PropertyID.ISO)).StringValue);
                //SettingsGroupBox.Enabled = true;
                //LiveViewGroupBox.Enabled = true;


                MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                MainCamera.SetCapacity(4096, int.MaxValue);

                //if (!MainCamera.IsLiveViewOn) { MainCamera.StartLiveView(); }
                //else { MainCamera.StopLiveView(); }
            }
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

        #endregion        

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

        private void btn_cerrar_info_Click(object sender, EventArgs e)
        {
            DialogResult salir = MessageBox.Show("¿Está seguro que quiere salir de Enrollment?", "Salir", MessageBoxButtons.YesNo);
            if (salir == DialogResult.Yes)
            {
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length > 0)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();

                Application.Exit();
                Environment.Exit(Environment.ExitCode);
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

        VisorAlertas visorAlertas;
        VisorAlertas visorAlertasPadre;
        VisorAlertas visorAlertasMadre;

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

        private ManualResetEvent _isIdle = new ManualResetEvent(true);

        //Sujetos
        private NSubject _subjectFinger;
        private NSubject _subjectFace;

        //Dedo
        private NFinger _subjectFingerDerecho;
        private NFinger _subjectFingerIzquierdo;

        //Rostro
        private NFace _nFace;
        private NFace _nFaceSegmented;

        #endregion

        #region Declaración de variables globales

        UsuariosEN usuariosEN;
        UsuariosLN usuariosLN;

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
            chkVistaEnVivo.Enabled = capturing;

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

            }
            catch (Exception ex)
            {
                txtMensaje.Text = "enrollment_Load(). " + ex.Message;
                MessageBox.Show("enrollment_Load(). " + ex.Message);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_COPYDATA)
            {
                // Extract the file name
                NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
                int dataType = (int)copyData.dwData;
                if (dataType == 2)
                {
                    string texto = Marshal.PtrToStringAnsi(copyData.lpData);
                    if (texto.Contains("ProcesarRostro_"))
                        lblProcesarRostro.Text = texto;
                    else if (texto.Contains("dsParamEncript"))
                        lblEncriptar.Text = texto;
                    //else
                    //    lblComprimir.Text = texto;
                }
                else
                {
                    MessageBox.Show(String.Format("Unrecognized data type = {0}.", dataType), "SendMessageDemo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void ControlsBackColor(string mode)
        {
            switch (mode)
            {
                case "NEW":
                    txtNoCaso.BackColor = Color.White;
                    cmbTipoTramite.BackColor = Color.White;
                    cmbTipoPasaporte.BackColor = Color.White;
                    txtNoRecibo.BackColor = Color.White;
                    txtPrimerNombre.BackColor = Color.White;
                    txtSegundoNombre.BackColor = Color.White;
                    txtTercerNombre.BackColor = Color.White;
                    txtPrimerApellido.BackColor = Color.White;
                    txtSegundoApellido.BackColor = Color.White;
                    txtApellidoCasada.BackColor = Color.White;
                    cmbGenero.BackColor = Color.White;
                    cmbEstadoCivil.BackColor = Color.White;
                    cmbOcupaciones.BackColor = Color.White;
                    dtpFechaNacimiento.BackColor = Color.White;
                    cmbOjos.BackColor = Color.White;
                    cmbTez.BackColor = Color.White;
                    cmbCabello.BackColor = Color.White;
                    txtEstatura.BackColor = Color.White;
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
                    cmbEstadoResidencia.BackColor = Color.White;
                    cmbZipCodeResidencia.BackColor = Color.White;
                    cmbCiudadResidencia.BackColor = Color.White;
                    txtResidenciaEntrega1.BackColor = Color.White;
                    txtResidenciaEntrega2.BackColor = Color.White;
                    cmbPaisSedeEntrega.BackColor = Color.White;
                    cmbCiudadSedeEntrega.BackColor = Color.White;
                    cmbEstadoEntrega.BackColor = Color.White;
                    cmbZipCodeEntrega.BackColor = Color.White;
                    cmbCiudadEntrega.BackColor = Color.White;
                    txtNombrePadre.BackColor = Color.White;
                    txtApellidoPadre.BackColor = Color.White;
                    txtNombreMadre.BackColor = Color.White;
                    txtApellidoMadre.BackColor = Color.White;
                    cmbTipoIdPadre.BackColor = Color.White;
                    txtNumeroIdPadre.BackColor = Color.White;
                    cmbTipoIdMadre.BackColor = Color.White;
                    txtNumeroIdMadre.BackColor = Color.White;
                    //txtTelCelular.BackColor = Color.White;
                    txtTelCasa.BackColor = Color.White;
                    txtTelTrabajo.BackColor = Color.White;
                    txtEmail.BackColor = Color.White;

                    break;

                case "ADULT":
                    txtNoCaso.BackColor = Color.Yellow;
                    cmbTipoTramite.BackColor = Color.Yellow;
                    cmbTipoPasaporte.BackColor = Color.Yellow;
                    txtNoRecibo.BackColor = Color.Yellow;
                    txtPrimerNombre.BackColor = Color.Yellow;
                    txtSegundoNombre.BackColor = Color.White;
                    txtTercerNombre.BackColor = Color.White;
                    txtPrimerApellido.BackColor = Color.Yellow;
                    txtSegundoApellido.BackColor = Color.White;
                    txtApellidoCasada.BackColor = Color.White;
                    cmbGenero.BackColor = Color.Yellow;
                    cmbEstadoCivil.BackColor = Color.Yellow;
                    cmbOcupaciones.BackColor = Color.Yellow;
                    dtpFechaNacimiento.BackColor = Color.Yellow;
                    cmbOjos.BackColor = Color.Yellow;
                    cmbTez.BackColor = Color.Yellow;
                    cmbCabello.BackColor = Color.Yellow;
                    txtEstatura.BackColor = Color.Yellow;
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
                    cmbEstadoResidencia.BackColor = Color.Yellow;
                    cmbZipCodeResidencia.BackColor = Color.Yellow;
                    cmbCiudadResidencia.BackColor = Color.Yellow;
                    txtResidenciaEntrega1.BackColor = Color.Yellow;
                    txtResidenciaEntrega2.BackColor = Color.White;
                    cmbPaisSedeEntrega.BackColor = Color.Yellow;
                    cmbCiudadSedeEntrega.BackColor = Color.Yellow;
                    cmbEstadoEntrega.BackColor = Color.Yellow;
                    cmbZipCodeEntrega.BackColor = Color.Yellow;
                    cmbCiudadEntrega.BackColor = Color.Yellow;
                    txtNombrePadre.BackColor = Color.Yellow;
                    txtApellidoPadre.BackColor = Color.Yellow;
                    txtNombreMadre.BackColor = Color.Yellow;
                    txtApellidoMadre.BackColor = Color.Yellow;
                    cmbTipoIdPadre.BackColor = Color.White;
                    txtNumeroIdPadre.BackColor = Color.White;
                    cmbTipoIdMadre.BackColor = Color.White;
                    txtNumeroIdMadre.BackColor = Color.White;

                    //txtTelCelular.BackColor = Color.Yellow;
                    txtTelCasa.BackColor = Color.White;
                    txtTelTrabajo.BackColor = Color.White;
                    txtEmail.BackColor = Color.White;

                    break;

                case "YOUNG":
                    txtNoCaso.BackColor = Color.Yellow;
                    cmbTipoTramite.BackColor = Color.Yellow;
                    cmbTipoPasaporte.BackColor = Color.Yellow;
                    txtNoRecibo.BackColor = Color.Yellow;
                    txtPrimerNombre.BackColor = Color.Yellow;
                    txtSegundoNombre.BackColor = Color.White;
                    txtTercerNombre.BackColor = Color.White;
                    txtPrimerApellido.BackColor = Color.Yellow;
                    txtSegundoApellido.BackColor = Color.White;
                    txtApellidoCasada.BackColor = Color.White;
                    cmbGenero.BackColor = Color.Yellow;
                    cmbEstadoCivil.BackColor = Color.Yellow;
                    cmbOcupaciones.BackColor = Color.Yellow;
                    dtpFechaNacimiento.BackColor = Color.Yellow;
                    cmbOjos.BackColor = Color.Yellow;
                    cmbTez.BackColor = Color.Yellow;
                    cmbCabello.BackColor = Color.Yellow;
                    txtEstatura.BackColor = Color.Yellow;
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
                    cmbEstadoResidencia.BackColor = Color.Yellow;
                    cmbZipCodeResidencia.BackColor = Color.Yellow;
                    cmbCiudadResidencia.BackColor = Color.Yellow;
                    txtResidenciaEntrega1.BackColor = Color.Yellow;
                    txtResidenciaEntrega2.BackColor = Color.White;
                    cmbPaisSedeEntrega.BackColor = Color.Yellow;
                    cmbCiudadSedeEntrega.BackColor = Color.Yellow;
                    cmbEstadoEntrega.BackColor = Color.Yellow;
                    cmbZipCodeEntrega.BackColor = Color.Yellow;
                    cmbCiudadEntrega.BackColor = Color.Yellow;
                    txtNombrePadre.BackColor = Color.Yellow;
                    txtApellidoPadre.BackColor = Color.Yellow;
                    txtNombreMadre.BackColor = Color.Yellow;
                    txtApellidoMadre.BackColor = Color.Yellow;
                    cmbTipoIdPadre.BackColor = Color.Yellow;
                    txtNumeroIdPadre.BackColor = Color.Yellow;
                    cmbTipoIdMadre.BackColor = Color.Yellow;
                    txtNumeroIdMadre.BackColor = Color.Yellow;

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
                _biometricFaceClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Face };
                await _biometricFaceClient.InitializeAsync();

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

                string filtro = Properties.Settings.Default.ARTICULO_98 ? "codigo IN(1, 2, 3, 4, 5, 6)" : "codigo IN(1, 2, 3, 4, 5)";

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\TiposPasaporte.xml", "TiposPasaporte", "TipoPasaporte", "Codigo", "Tipo");
                cmb.DataSource = ds.Tables[0].Select(filtro, " codigo ASC ").CopyToDataTable();
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

                DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Generos.xml", "Generos", "Genero", "Codigo", "Nombre");
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

                if(cmbTipoPasaporte.Text.Contains("DIPLOMATICO Art. 98 Cod. Migración") && Properties.Settings.Default.ARTICULO_98)
                    filtro = " codigo IN (2) ";

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
                    string filtro = "";

                    if (cmbTipoPasaporte.Text.Contains("DIPLOMATICO Art. 98 Cod. Migración") && Properties.Settings.Default.ARTICULO_98)
                        filtro = "codigo <> 320";
                    else
                        filtro = " 1 > 0 ";

                    DataSet ds = LeerXmlCatalogos(Application.StartupPath + "\\Catalogos\\Paises.xml", "Paises", "Pais", "Codigo", "Nombre");
                    cmb.DataSource = ds.Tables[0].Select(filtro, " VALOR ASC ").CopyToDataTable();
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

                listaNodos = XDocument.Load(Application.StartupPath + "\\Catalogos\\Paises.xml", LoadOptions.None);
                lista = listaNodos.Element("Paises");

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

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabHuellas"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";

                this.Enabled = false;
                bool camposValidos = false;
                camposValidos = await ValidarCampos();

                if (camposValidos)
                {
                    VerificacionGuardar vGuardar = new VerificacionGuardar(_listDataWsUsuariosDGM);
                    vGuardar.lblUsuario.Text = lbl_usuario.Text;
                    vGuardar.lblNombre.Text = lblNombreUsuario.Text;
                    vGuardar.ShowDialog();

                    if (!vGuardar.VerificacionValida)
                        throw new Exception("¡No se pudo verificar la identidad del usuario!");

                    string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "CasoPasaporte_" + txtNoCaso.Text.Trim() + ".xml");

                    if (File.Exists(rutaXML))
                        throw new Exception("El archivo ya existe");

                    string rutaXML2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "Done", "CasoPasaporte_" + txtNoCaso.Text.Trim() + ".xml");

                    if (File.Exists(rutaXML2))
                        throw new Exception("El archivo ya existe (2)");

                    string direccionEncript = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                    if (File.Exists(direccionEncript))
                        throw new Exception("El archivo ya existe (3)");

                    string direccionEncript2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Done") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                    if (File.Exists(direccionEncript2))
                        throw new Exception("El archivo ya existe (4)");

                    DialogResult result = MessageBox.Show("¿Está seguro que desea GUARDAR?", "Salir", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        XmlDatosEN pasaporte_xml = new XmlDatosEN();

                        string usuario = lblNombreUsuario.Text;
                        //CAMBIAR LA SEDE Y COLOCAR LA QUE ESTÁ CONFIGURADA SEGÚN EL USUARIO
                        pasaporte_xml.SedeEnrolamiento = Properties.Settings.Default.SEDE;

                        pasaporte_xml.FechaCaptura = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        pasaporte_xml.Usuario = usuario;
                        pasaporte_xml.NumeroCaso = txtNoCaso.Text;
                        pasaporte_xml.TipoTramite = cmbTipoTramite.Text;
                        pasaporte_xml.TipoPasaporte = cmbTipoPasaporte.Text;
                        pasaporte_xml.NoRecibo = txtNoRecibo.Text;
                        pasaporte_xml.NoPasaporte = txtNumeroId.Text;
                        pasaporte_xml.CUI = txtCui.Text;
                        pasaporte_xml.Nombre1 = txtPrimerNombre.Text;
                        pasaporte_xml.Nombre2 = txtSegundoNombre.Text + " " + txtTercerNombre.Text;
                        //datos_xml.Nombre3 = txtTercerNombre.Text;
                        pasaporte_xml.Apellido1 = txtPrimerApellido.Text;
                        pasaporte_xml.Apellido2 = txtSegundoApellido.Text;
                        pasaporte_xml.DeCasada = txtApellidoCasada.Text;
                        pasaporte_xml.Sexo = cmbGenero.Text;
                        pasaporte_xml.EstadoCivil = cmbEstadoCivil.Text;
                        pasaporte_xml.Ocupacion = cmbOcupaciones.Text;
                        /*Fecha Nacimiento*/
                        DateTime fechaN = new DateTime();
                        fechaN = DateTime.Parse(dtpFechaNacimiento.Text);
                        pasaporte_xml.FechaNac = fechaN.ToString("yyyy-MM-dd");

                        pasaporte_xml.PaisNac = cmbPaisNacimiento.Text;

                        /////////////////////////////////////////////
                        pasaporte_xml.DepartamentoNac = string.Empty;
                        pasaporte_xml.MunicipioNac = string.Empty;
                        /////////////////////////////////////////////
                        ///

                        //MessageBox.Show("txtDepartamentoNacimiento.Visible: " + txtDepartamentoNacimiento.Visible.ToString() + ", txtDepartamentoNacimiento.Text: " + txtDepartamentoNacimiento.Text);

                        if (cmbPaisNacimiento.SelectedValue.ToString().Equals("320") || cmbPaisNacimiento.SelectedValue.ToString().Equals("840"))
                        {
                            pasaporte_xml.DepartamentoNac = cmbDeptoNacimiento.Text;
                            pasaporte_xml.MunicipioNac = cmbMunicNacimiento.Text;
                        }
                        else
                            pasaporte_xml.DepartamentoNac = txtDepartamentoNacimiento.Text;

                        pasaporte_xml.PaisResidencia = cmbPaisResidencia.Text;
                        pasaporte_xml.DireccionResidencia1 = txtResidencia1.Text;
                        pasaporte_xml.DireccionResidencia2 = txtResidencia2.Text;
                        pasaporte_xml.DireccionResidencia3 = string.Empty;

                        /////////////////////////////////////////////
                        pasaporte_xml.MunicipioResidencia = string.Empty;
                        pasaporte_xml.DeptoResidencia = string.Empty;
                        pasaporte_xml.EstadoResidencia = string.Empty;
                        pasaporte_xml.CiudadResidencia = string.Empty;
                        pasaporte_xml.ZipResidencia = string.Empty;
                        /////////////////////////////////////////////

                        if (cmbEstadoResidencia.Visible == false)
                        {
                            pasaporte_xml.MunicipioResidencia = cmbMunicResidencia.Text;
                            pasaporte_xml.DeptoResidencia = cmbDeptoResidencia.Text;
                        }
                        else
                        {
                            pasaporte_xml.EstadoResidencia = cmbEstadoResidencia.Text;
                            pasaporte_xml.CiudadResidencia = cmbCiudadResidencia.Text;
                            pasaporte_xml.ZipResidencia = Convert.ToString(cmbCiudadResidencia.SelectedValue);
                        }

                        pasaporte_xml.TelefonoCasa = string.Empty;
                        pasaporte_xml.TelefonoTrabajo = string.Empty;
                        pasaporte_xml.TelefonoCelular = string.Empty;

                        if (txtTelCasa.Text.Any(char.IsDigit))
                            if (txtTelCasa.MaskFull == true)
                                pasaporte_xml.TelefonoCasa = txtTelCasa.Text;

                        if (txtTelTrabajo.Text.Any(char.IsDigit))
                            if (txtTelTrabajo.MaskFull == true)
                                pasaporte_xml.TelefonoTrabajo = txtTelTrabajo.Text;

                        if (txtTelCelular.Text.Any(char.IsDigit))
                            if (txtTelCelular.MaskFull == true)
                                pasaporte_xml.TelefonoCelular = txtTelCelular.Text;

                        pasaporte_xml.Email = txtEmail.Text;

                        pasaporte_xml.SedeEntrega = string.Empty;
                        pasaporte_xml.DireccionEnvio1 = string.Empty;
                        pasaporte_xml.DireccionEnvio2 = string.Empty;
                        pasaporte_xml.EstadoEnvio = string.Empty;
                        pasaporte_xml.CiudadEnvio = string.Empty;
                        pasaporte_xml.ZipEnvio = string.Empty;
                        pasaporte_xml.TelefonoEnvio = string.Empty;


                        if (cmbPaisSedeEntrega.SelectedValue.Equals("GUATEMALA"))
                        {
                            pasaporte_xml.SedeEntrega = cmbCiudadSedeEntrega.Text;
                        }//ESTADOS UNIDOS DE AMÉRICA
                        else
                        {
                            pasaporte_xml.DireccionEnvio1 = txtResidencia1.Text;
                            pasaporte_xml.DireccionEnvio2 = txtResidencia1.Text;
                            pasaporte_xml.EstadoEnvio = cmbEstadoResidencia.Text;
                            pasaporte_xml.CiudadEnvio = cmbCiudadResidencia.Text;
                            pasaporte_xml.ZipEnvio = Convert.ToString(cmbCiudadResidencia.SelectedValue);
                            pasaporte_xml.TelefonoEnvio = txtTelCelular.Text;
                        }

                        pasaporte_xml.Nacionalidad = cmbNacionalidad.Text;

                        pasaporte_xml.CUI = string.Empty;
                        pasaporte_xml.TipoId = string.Empty;
                        pasaporte_xml.NumeroId = string.Empty;
                        pasaporte_xml.NumeroSerie = string.Empty;
                        pasaporte_xml.MunicipioEmision = string.Empty;
                        pasaporte_xml.DeptoEmision = string.Empty;
                        pasaporte_xml.TipoIdPadre = string.Empty;
                        pasaporte_xml.NumeroIdPadre = string.Empty;
                        pasaporte_xml.TipoIdMadre = string.Empty;
                        pasaporte_xml.NumeroIdMadre = string.Empty;
                        pasaporte_xml.LibroMenor = string.Empty;
                        pasaporte_xml.FolioMenor = string.Empty;
                        pasaporte_xml.PartidaMenor = string.Empty;
                        pasaporte_xml.CUIMenor = string.Empty;

                        //if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text.Contains("DIPLOMATICO Art. 98 Cod.") || cmbTipoPasaporte.Text == "OFICIAL")
                        if (cmbTipoPasaporte.Text == "ORDINARIO MENOR" || cmbTipoPasaporte.Text == "DIPLOMATICO MENOR")
                        {
                            pasaporte_xml.TipoIdPadre = cmbTipoIdPadre.Text;
                            pasaporte_xml.NumeroIdPadre = txtNumeroIdPadre.Text;

                            pasaporte_xml.TipoIdMadre = cmbTipoIdMadre.Text;
                            pasaporte_xml.NumeroIdMadre = txtNumeroIdMadre.Text;

                            pasaporte_xml.LibroMenor = string.Empty;
                            pasaporte_xml.FolioMenor = string.Empty;
                            pasaporte_xml.PartidaMenor = string.Empty;
                            pasaporte_xml.CUIMenor = txtCui.Text;

                            try
                            {
                                pasaporte_xml.MunicipioEmision = "";
                                pasaporte_xml.DeptoEmision = "";
                                if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI"))
                                {
                                    pasaporte_xml.MunicipioEmision = cmbMunicipioDPI.Text;
                                    pasaporte_xml.DeptoEmision = cmbDeptoDPI.Text;
                                }
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            if (cmbTipoPasaporte.Text.Equals("DIPLOMATICO Art. 98 Cod. Migración"))
                            {
                                pasaporte_xml.CUI = string.Empty;
                                pasaporte_xml.TipoId = cmbTiposDocumento.Text;
                                pasaporte_xml.NumeroId = txtCui.Text;
                                pasaporte_xml.NumeroSerie = string.Empty;
                                pasaporte_xml.MunicipioEmision = string.Empty;
                                pasaporte_xml.DeptoEmision = string.Empty;
                            }
                            else
                            {
                                pasaporte_xml.CUI = txtCui.Text;
                                pasaporte_xml.TipoId = cmbTiposDocumento.Text;
                                pasaporte_xml.NumeroId = txtNumeroId.Text;
                                pasaporte_xml.NumeroSerie = txtNumeroSerie.Text;

                                try
                                {
                                    pasaporte_xml.MunicipioEmision = "";
                                    pasaporte_xml.DeptoEmision = "";
                                    if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI"))
                                    {
                                        pasaporte_xml.MunicipioEmision = cmbMunicipioDPI.Text;
                                        pasaporte_xml.DeptoEmision = cmbDeptoDPI.Text;
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        pasaporte_xml.Ojos = cmbOjos.Text;
                        pasaporte_xml.Tez = cmbTez.Text;
                        pasaporte_xml.Pelo = cmbCabello.Text;
                        pasaporte_xml.Estatura = txtEstatura.Text;

                        pasaporte_xml.NombresPadre = txtNombrePadre.Text;
                        pasaporte_xml.ApellidosPadre = txtApellidoPadre.Text;
                        pasaporte_xml.NombresMadre = txtNombreMadre.Text;
                        pasaporte_xml.ApellidosMadre = txtApellidoMadre.Text;

                        //string fotoJpegBase64 = chkIcao.Checked ? Convert.ToBase64String(faceView2.Face.Image.Save(NImageFormat.Jpeg).ToArray()) : Convert.ToBase64String(faceView2.Face.Image.Save(NImageFormat.Jpeg).ToArray());
                        string fotoJpegBase64 = "";
                        using (var ms = new MemoryStream())
                        {
                            pbxRostroIcao.Image.Save(ms, ImageFormat.Jpeg);
                            fotoJpegBase64 = Convert.ToBase64String(ms.ToArray());
                            ms.Dispose();
                        }

                        pasaporte_xml.Foto = fotoJpegBase64;
                        pasaporte_xml.FotoForzada = (chkIcao.Checked == false) ? "S" : "N";
                        pasaporte_xml.FotoObs = txtObservacionesIcao.Text.Trim();

                        ///////////////////GENERANDO LAS HUELLAS FINALES//////////////////

                        ///MANO DERECHA
                        pasaporte_xml.HuellaPos1 = cmbDedoDerecho.SelectedValue.ToString();
                        pasaporte_xml.HuellaObs1 = (txtComentarioDDerecho.Text.Trim().Equals("") || txtComentarioDDerecho.Text.Trim().Equals(string.Empty)) ? string.Empty : txtComentarioDDerecho.Text;

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

                            pasaporte_xml.HuellaPNG1 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                            pasaporte_xml.HuellaWSQ1 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                        }
                        ///MANO DERECHA

                        /// MANO IZQUIERDA
                        pasaporte_xml.HuellaPos2 = cmbDedoIzquierdo.SelectedValue.ToString();
                        pasaporte_xml.HuellaObs2 = (txtComentarioDIzquierdo.Text.Trim().Equals("") || txtComentarioDIzquierdo.Text.Trim().Equals(string.Empty)) ? string.Empty : txtComentarioDIzquierdo.Text;

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

                            pasaporte_xml.HuellaPNG2 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"].ToString();
                            pasaporte_xml.HuellaWSQ2 = dsHuellaFinal.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"].ToString();
                        }
                        /// MANO IZQUIERDA
                        ///////////////////GENERANDO LAS HUELLAS FINALES//////////////////

                        //FIRMA
                        if (chkNoPuedeFirmar.Checked == false)
                            pasaporte_xml.Firma = funciones.Base64DesdePng(sigPlusNET1.GetSigImage());

                        if (chkNoPuedeFirmar.Checked)
                            pasaporte_xml.FirmaForzada = "S";
                        else
                            pasaporte_xml.FirmaForzada = "N";

                        if (pbxDPI.Image.Tag.Equals("Check"))
                        {
                            pasaporte_xml.DPIIntentado = "S";
                            pasaporte_xml.DPI = "S";
                        }
                        else
                        {
                            pasaporte_xml.DPIIntentado = "N";
                            pasaporte_xml.DPI = "N";
                        }

                        if (pbxMOCH.Image.Tag.Equals("Check"))
                        {
                            pasaporte_xml.DPISinHuellas = "N";
                            pasaporte_xml.MOC = "S";
                        }
                        else
                        {
                            pasaporte_xml.DPISinHuellas = "S";
                            pasaporte_xml.MOC = "N";
                        }

                        if (pbxMOCF.Image.Tag.Equals("Check"))
                        {
                            pasaporte_xml.FaceIntentado = "S";
                            pasaporte_xml.Face = "S";
                        }
                        else
                        {
                            pasaporte_xml.FaceIntentado = "N";
                            pasaporte_xml.Face = "N";
                        }

                        pasaporte_xml.DPIPadreIntentado = "N";
                        pasaporte_xml.DPIPadre = "N";
                        pasaporte_xml.DPIPadreSinHuellas = "N";
                        pasaporte_xml.MOCPadre = "N";
                        pasaporte_xml.FacePadreIntentado = "N";
                        pasaporte_xml.FacePadre = "N";

                        pasaporte_xml.DPIMadreIntentado = "N";
                        pasaporte_xml.DPIMadre = "N";
                        pasaporte_xml.DPIMadreSinHuellas = "N";
                        pasaporte_xml.MOCMadre = "N";
                        pasaporte_xml.FaceMadreIntentado = "N";
                        pasaporte_xml.FaceMadre = "N";

                        Control.ControlCollection controlCollection = splitContainer1.Panel2.Controls;
                        listaProbatorios = new List<string>();

                        foreach (Control control in controlCollection)
                        {
                            if (control is Label)
                            {
                                Label lbl = (Label)(control);
                                //probatorios_image_base64(lbl.Image.Tag.ToString());
                                //array = new string[tamanio];
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                {
                                    Image img = Image.FromFile(lbl.Image.Tag.ToString());
                                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                                    byte[] imageBytes = ms.ToArray();

                                    // Convert byte[] to Base64 String
                                    string base64String = Convert.ToBase64String(imageBytes);

                                    //array[tamanio - 1] = base64String;
                                    listaProbatorios.Add(base64String);

                                    try { File.Delete(lbl.Image.Tag.ToString()); } catch { }
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                }
                            }
                        }
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        string[] sprobatorios = listaProbatorios.ToArray();
                        pasaporte_xml.Probatorios = sprobatorios;
                        pasaporte_xml.Version = "3.0";

                        XmlEnrol xml = new XmlEnrol();

                        DataSet dsDirectorioLlaves = funciones.ExisteDirectorioLlaves(lbl_usuario.Text, Environment.MachineName);

                        if (bool.Parse(dsDirectorioLlaves.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            throw new Exception(dsDirectorioLlaves.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        xml.crearXml(rutaXML, pasaporte_xml);
                        
                        DataSet dsEncriptarArchivo = EncriptarArchivo(rutaXML);

                        //if (bool.Parse(dsEncriptarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        //    throw new Exception("¡Error al encriptar el archivo, contacte al administrador del sistema!. " + dsEncriptarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        //string rutaArchivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                        //DataSet dsEnviarArchivo = new DataSet();
                        //if (Properties.Settings.Default.ENVIAR_FTP)
                        //{
                        //    //xml.crearXml(rutaXML, pasaporte_xml);

                        //    dsEnviarArchivo = funciones.EnvioFTPArchivo(rutaArchivoEncriptado);

                        //    if (bool.Parse(dsEnviarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        //        MessageBox.Show("¡Error al enviar el archivo, intente después con la opción correspondiente!. " + dsEncriptarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        //}

                        //tmrPasaporte.Enabled = false;

                        //int iCorrelativo = 0;
                        //try
                        //{
                        //    string rutaReporteResumen = Application.StartupPath + "\\ENROL\\ReporteCasos\\" + DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                        //    if (!File.Exists(rutaReporteResumen))
                        //    {
                        //        XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));

                        //        //Creamos el nodo raiz y lo añadimos al documento
                        //        XElement nodoRaiz = new XElement("Casos");
                        //        document.Add(nodoRaiz);
                        //        document.Save(rutaReporteResumen);
                        //        iCorrelativo = 1;
                        //    }

                        //    XDocument xmlDoc = XDocument.Load(rutaReporteResumen);

                        //    if (iCorrelativo == 0)
                        //        iCorrelativo = int.Parse(xmlDoc.Elements("Casos").Elements("Caso").Last().Element("Correlativo").Value) + 1;

                        //    xmlDoc.Elements("Casos")
                        //    .Last().Add(new XElement("Caso", new XElement("Correlativo", iCorrelativo.ToString()), new XElement("Hora", horaCaptura), new XElement("NoCaso", txtNoCaso.Text), new XElement("Nombres", pasaporte_xml.Nombre1 + " " + pasaporte_xml.Nombre2 + " " + pasaporte_xml.Nombre3), new XElement("Apellidos", pasaporte_xml.Apellido1 + " " + pasaporte_xml.Apellido2), new XElement("FechaNacimiento", fechaN.ToString("dd/MM/yyyy")), new XElement("Usuario", lbl_usuario.Text), new XElement("NombreUsuario", lblNombreUsuario.Text), new XElement("SedeCaptura", sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS), new XElement("FechaCaptura", fechaCaptura), new XElement("EstacionCaptura", Environment.MachineName)));
                        //    xmlDoc.Save(rutaReporteResumen);
                        //}
                        //catch (Exception ex)
                        //{
                        //    MessageBox.Show("¡Error a bitacorizar! " + ex.Message);
                        //}

                        ////LimpiarBandejaProbatorios();

                        //btnImprimir_Click(sender, e);

                        //ProcesarRostrosNeuroStart();

                        //MessageBox.Show("Almacenado con éxito! ");
                        //txtMensaje.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnGuardar_Click(). Mensaje: " + ex.Message + ". StackTrace: " + ex.StackTrace;
                MessageBox.Show("btnGuardar_Click(). Mensaje: " + ex.Message);// + ". StackTrace: " + ex.StackTrace);
            }

            this.Enabled = true;
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
                Bitmap ImagenFinal;

                ImagenFinal = new Bitmap(416, 416);
                using (Graphics graph = Graphics.FromImage(ImagenFinal))
                {
                    System.Drawing.Rectangle ImageSize = new System.Drawing.Rectangle(0, 0, 416, 416);
                    graph.FillRectangle(Brushes.White, ImageSize);

                    //ImageSize = new Rectangle(79, 40, 258, 336);
                    //graph.FillRectangle(Brushes.Blue, ImageSize);

                    graph.DrawImage(huellaDigital, new System.Drawing.Rectangle(79, 40, 258, 336));
                }

                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = ImagenFinal;

                MemoryStream ms = new MemoryStream();
                ImagenFinal.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = Convert.ToBase64String(ms.ToArray());
                ms.Dispose();

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
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "GenerarHuellaPNG_STRING_BASE64(). " + ex.Message;
                ds.Tables[0].Rows[0]["IMAGEN_PNG"] = null;
                ds.Tables[0].Rows[0]["IMAGEN_PNG_STRING_BASE64"] = string.Empty;
                ds.Tables[0].Rows[0]["IMAGEN_WSQ_STRING_BASE64"] = string.Empty;
            }
            return ds;
        }

        public DataSet EncriptarArchivo(string rutaArchivoPlano)
        {
            string linea = "";
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                #region
                /*
                 * string archivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + lbl_usuario.Text + ".txt";
               string XmlKeysFile = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", "XMLKeys.xml");
               string llavePublica = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", "pasaportes_PublicKey.asc");



               DataSet dsXmlKey = new DataSet();
               dsXmlKey.ReadXml(XmlKeysFile);

               string archivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + lbl_usuario.Text + ".txt";


               UtilityCore.Cryptography.Cryptography Cryp = new UtilityCore.Cryptography.Cryptography();
               Cryp.Encryption(llavePublica, dsXmlKey.Tables[0].Rows[0]["PASSWORD"].ToString(), archivoEncriptado, rutaArchivoPlano);

               File.WriteAllText(archivoEncriptado + ".Done", string.Empty);
               File.Delete(rutaArchivoPlano);*/
                #endregion
                DataSet dsParamEncrip = new DataSet();
                dsParamEncrip.Tables.Add(new DataTable());
                dsParamEncrip.Tables[0].Columns.Add("rutaArchivoPlano", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("archivoEncriptado", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("rutaArchivo", typeof(string));
                dsParamEncrip.Tables[0].Columns.Add("publicKey", typeof(string));

                DataRow drParamEncript = dsParamEncrip.Tables[0].NewRow();

                linea = "1";
                string archivoEncriptado =   Path.GetFileNameWithoutExtension(rutaArchivoPlano) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION;
                linea = "";
                string rutaArchivo = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\";

                linea = "2";
                string publicKey = "";
                if(Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("U"))
                    publicKey = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", lbl_usuario.Text + "_publi.xml");
                else if(Properties.Settings.Default.SUFIJO_ENCRIPTACION.Equals("M"))
                    publicKey = Path.Combine(Properties.Settings.Default.DRIVE_LETTER, "DGMGT", "XMLKeys", Environment.MachineName + "_publi.xml");

                drParamEncript["rutaArchivoPlano"] = rutaArchivoPlano;
                drParamEncript["archivoEncriptado"] = archivoEncriptado;
                drParamEncript["rutaArchivo"] = rutaArchivo;
                drParamEncript["publicKey"] = publicKey;
                dsParamEncrip.Tables[0].Rows.Add(drParamEncript);

                string strDs = dsParamEncrip.GetXml();
                ActivarProcesamientoRostro(strDs);

                //linea = "3";
                //string keypub = new StreamReader(publicKey).ReadToEnd();
                ////linea = "4";
                ////string info = File.ReadAllText(rutaArchivoPlano);

                //linea = "5";
                //byte[] encryp = Yoyo.Encrypt(keypub, File.ReadAllBytes(rutaArchivoPlano));//Encoding.ASCII.GetBytes(info));
                //linea = "6";
                //Yoyo.saveBynaryFile(encryp, archivoEncriptado, ".txt", rutaAchivo);

                //linea = "7";
                //string archivoDone = Path.Combine(rutaAchivo, archivoEncriptado + ".txt.Done");
                //linea = "8";
                //File.WriteAllText(archivoDone, string.Empty);
                //linea = "9";
                //File.Delete(rutaArchivoPlano);

                //linea = "10";
                //System.GC.Collect();
                //System.GC.WaitForPendingFinalizers();

                linea = "11";
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }
            catch (Exception ex)
            {
                try { File.Delete(rutaArchivoPlano); } catch { }; 

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "EncriptarArchivo(). " + ex.Message + " Inner: " + ex.InnerException + ". Línea: " + linea;
                dsResultado.Tables[0].Rows[0]["DATOS"] = null;
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return dsResultado;
        }

        public async Task<bool> ValidarCampos()
        {
            DataSet dsIdentidad = ValidarIdentidad();
            DataSet dsRostro = await ValidarFotografia();
            DataSet dsHuellas = await ValidarHuellas();
            DataSet dsFirma = await ValidarFirma();
            DataSet dsProbatorios = await ValidarProbatorios();

            txtMensaje.Text = string.Empty;

            if (bool.Parse(dsIdentidad.Tables[0].Rows[0]["RESULTADO"].ToString()))
                tab_principal.TabPages["tabIdentidad"].ImageKey = "check.bmp";
            else
            {
                tab_principal.TabPages["tabIdentidad"].ImageKey = "warning.bmp";
                txtMensaje.Text += dsIdentidad.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                //MessageBox.Show(dsIdentidad.Tables[0].Rows[0]["MSG_ERROR"].ToString());
            }


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


            if (bool.Parse(dsFirma.Tables[0].Rows[0]["RESULTADO"].ToString()))
                tab_principal.TabPages["tabFirma"].ImageKey = "check.bmp";
            else
            {
                tab_principal.TabPages["tabFirma"].ImageKey = "warning.bmp";
                txtMensaje.Text += dsFirma.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            }


            if (bool.Parse(dsProbatorios.Tables[0].Rows[0]["RESULTADO"].ToString()))
                tab_principal.TabPages["tabProbatorios"].ImageKey = "check.bmp";
            else
            {
                tab_principal.TabPages["tabProbatorios"].ImageKey = "warning.bmp";
                txtMensaje.Text += dsProbatorios.Tables[0].Rows[0]["MSG_ERROR"].ToString();
            }


            bool b1 = tab_principal.TabPages["tabIdentidad"].ImageKey == "check.bmp";
            bool b2 = tab_principal.TabPages["tabFotografia"].ImageKey == "check.bmp";
            bool b3 = tab_principal.TabPages["tabHuellas"].ImageKey == "check.bmp";
            bool b4 = tab_principal.TabPages["tabFirma"].ImageKey == "check.bmp";
            bool b5 = tab_principal.TabPages["tabProbatorios"].ImageKey == "check.bmp";

            if (b1 && b2 && b3 && b4 && b5)
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
                //ListarCamaras(cmbCamaras, true);

                RefreshCamera();
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

                //SetStatusText(Color.Red, "Extracción: Ninguna");
                //faceView2.Face = null;

                lblMatchFace.Text = string.Empty;
                chkVistaEnVivo.Checked = true;

                if (MainCamera?.SessionOpen == true)
                {
                    tmrSesionCamara.Stop();

                    chkVistaEnVivo.Checked = true;
                    chkVistaEnVivo_CheckedChanged(sender, e);

                    CloseSession();
                    ActivarControlesRostro(false);

                    LiveViewPicBox.Image = null;
                }
                else
                {
                    segSesionCamara = 0;
                    minSesionCamara = 0;
                    lblTimerCamara.Text = "Tiempo: 00 minuto(s) y  00 segundo(s)";

                    tmrSesionCamara.Start();
                    OpenSession();

                    chkVistaEnVivo_CheckedChanged(sender, e);
                    ActivarControlesRostro(true);
                }


                btnCapturarRostro.Text = "Capturar";

                //if (_biometricFaceClient.FaceCaptureDevice == null)
                //    throw new Exception(@"Por favor, seleccione una cámara de la lista (2).");


                //lblCalidadDDerecho.Text = string.Empty;


                // Set face capture from stream
                //_nFace = new NFace { CaptureOptions = NBiometricCaptureOptions.Stream };
                //_subjectFace = new NSubject();
                //_subjectFace.Faces.Add(_nFace);
                //faceView1.Face = _nFace;
                //icaoWarningView2.Face = _nFace;

                //_biometricFaceClient.FacesCheckIcaoCompliance = true;
                //NCamera camara = new NCamera();
                //NBiometricCaptureOptions 


                //var task = _biometricFaceClient.CreateTask(NBiometricOperations.Capture | NBiometricOperations.Segment | NBiometricOperations.CreateTemplate, _subjectFace);
                //lblEstadoCapturaRostro.Text = string.Empty;

                if (cmbCamaras.Items.Count > 1)
                    cmbCamaras.Enabled = false;

                //btnActualizarCamaras.Enabled = false;

                //try
                //{
                //    var performedTask = await _biometricFaceClient.PerformTaskAsync(task);
                //    OnCapturingCompleted(performedTask);
                //    await ValidarFotografia();
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("¡Error al realizar la captura fotografía!. " + ex.Message);
                //    txtMensaje.Text = ex.Message;
                //    btnDetenerCapturaRostro_Click(sender, e);
                //}

                //if (cmbCamaras.Items.Count > 1)
                //    cmbCamaras.Enabled = true;

                //btnActualizarCamaras.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnActivarCapturaRostroAsync_Click(). " + ex.Message;
                MessageBox.Show("btnActivarCapturaRostroAsync_Click(). " + ex.Message);

                btnDetenerCapturaRostro_Click(sender, e);
            }
        }

        private void OnCapturingCompleted(NBiometricTask task, NSubject vNSubject)
        {
            var status = task.Status;
            if (task.Error != null)
                Utils.ShowException(task.Error);
            if (status == NBiometricStatus.Ok)
            {
                _nFaceSegmented = vNSubject.Faces[1];
                //faceView2.Face = _nFaceSegmented;
                pbxRostroIcao.Image = _nFaceSegmented.Image.ToBitmap();
                icaoWarningView1.Face = _nFaceSegmented;
                
                NBiometricStatus vNBStatus = vNSubject.Faces[1].Status;
                //string s = vNSubject.Faces[1].Status;
            }

            //lblEstadoCapturaRostro.Text = status.ToString();
            //lblEstadoCapturaRostro.ForeColor = status == NBiometricStatus.Ok ? Color.Green : Color.Red;
            //ActivarControlesRostro(false);
        }

        private void btnCapturarRostro_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;

                lblMatchFace.Text = string.Empty;
                pbxFondoIcao.BackColor = Color.White;
                pbxMOCF.Image = pbxWarning.Image;

                chkVistaEnVivo.Enabled = false;
                btnCapturarRostro.Enabled = false;

                tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
                contadorFotos++;

                btnCapturarRostro.Text = "Repetir";

                TakePhoto();

                //if (contadorFotos >= int.Parse(Properties.Settings.Default.INTENTOS_ICAO))
                chkIcao.Visible = contadorFotos >= int.Parse(Properties.Settings.Default.INTENTOS_ICAO) ? true : false;

                int i = int.Parse(lblNoDisparos.Text) + 1;
                lblNoDisparos.Text = i.ToString();
                this.Enabled = true;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnCapturarRostro_Click(). " + ex.Message;
                MessageBox.Show("btnCapturarRostro_Click(). " + ex.Message);
            }
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

        public void TakePhoto()
        {
            MainCamera.TakePhotoShutter();
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

        private void OnCreateTemplateCompleted(NBiometricTask task)
        {
            NBiometricStatus status = NBiometricStatus.InternalError;
            status = task.Status;
            //if (task.Error != null) Utilities.ShowError(task.Error);
            if (task.Error != null) throw new Exception("OnCreateTemplateCompleted(). Error al extraer la plantilla del rostro. " + task.Error.Message);
            SetIsBusy(false);
            BeginInvoke(new Action<NBiometricStatus>(UpdateWithTaskResult), status);
        }

        private void UpdateWithTaskResult(NBiometricStatus status)
        {
            if (true)
            {
                //PrepareViews(false, chbCheckIcaoCompliance.Checked, status == NBiometricStatus.Ok);

                //bool withGeneralization = false;
                Color backColor = status == NBiometricStatus.Ok ? Color.Green : Color.Red;
                SetStatusText(backColor, "Extracción: {0}", status == NBiometricStatus.Timeout ? "Fallo detección de vida. " : status.ToString());

                //if (withGeneralization && status == NBiometricStatus.Ok)
                //if (status == NBiometricStatus.Ok)
                //{
                //    NFace generalized = _newSubject.Faces.Last();
                //    generalizationView.Generalized = new[] { generalized };
                //    generalizationView.Selected = generalized;
                //}
                //generalizationView.EnableMouseSelection = true;
                //EnableControls();
            }
        }

        private void SetStatusText(Color backColor, string format, params object[] args)
        {
            lblEstadoCapturaRostro.Text = string.Format(format, args);
            lblEstadoCapturaRostro.BackColor = backColor;
            lblEstadoCapturaRostro.Visible = true;
        }

        private void cmdActivarFirma_Click(object sender, EventArgs e)
        {
            try
            {
                //sigPlusNET1.ClearTablet();

                sigPlusNET1.SetTabletState(1);
                sigPlusNET1.SetJustifyMode(0);

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

                sigPlusNET1.SetTabletState(0);

                sigPlusNET1.SetImageXSize(500);
                sigPlusNET1.SetImageYSize(150);
                sigPlusNET1.SetJustifyMode(5);

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
                sigPlusNET1.ClearTablet();
                sigPlusNET1.SetTabletState(0);

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

                if (MainCamera != null)
                    CloseSession();

                sigPlusNET1.SetTabletState(0);
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
                //nom_escaner = TwainLib.TwainOperations.GetScanSource();
                nom_escaner = Settings.Default.scan;

                if (nom_escaner.Length > 0)
                {
                    var nombreArchivo = TwainOperations.ScanImages(".jpg", true, nom_escaner);
                    int j = (nombreArchivo.Count > limiteEscaneos) ? limiteEscaneos : nombreArchivo.Count;

                    for (int i = 0; i < j; i++)
                    //foreach (var Itm in nombreArchivo)
                    {
                        if (tamanio <= limiteEscaneos)
                        {
                            tamanio += 1;
                            VerProbatorios(nombreArchivo[i], tamanio);
                        }
                        else
                            MessageBox.Show("Límite de escaneos alcanzados (" + limiteEscaneos + "). ");

                    }
                }
                else
                    btnSeleccionarEscaner_Click(sender, e);

                ValidarProbatorios();

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

        ValidacionWsRenap vValidacionWsRenap;

        private async void btnLeerDPI_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    pbxDPI.Image = pbxWarning.Image;
                    //pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                    pbxMOCH.Image = pbxWarning.Image;
                    pbxAlertas.Image = pbxWarning.Image;

                    cmbTipoPasaporte.Enabled = false;
                    txtNumeroId.Text = txtNumeroSerie.Text = txtCui.Text = string.Empty;

                    panel_inferior.Enabled = picb_cerrar.Enabled = false;

                    pbxDPI.Image = pbxLoad.Image;

                    dpiTitular = await LeerDPIAsync();

                    if (dpiTitular.INFORMACION_DPI_LEIDA == false)
                        throw new Exception(dpiTitular.MENSAJE_ERROR);

                    BloquearControles("MOC");
                    txtCui.Text = dpiTitular.CUI;
                    lbl_dpi_info.Text = txtCui.Text;

                    DataSet dsDeptoMunicDPI = Depto_Munic_EmisionDPI(lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 4, 2), lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 2, 2));

                    txtPrimerNombre.Text = dpiTitular.PRIMER_NOMBRE;
                    txtSegundoNombre.Text = dpiTitular.SEGUNDO_NOMBRE;
                    txtTercerNombre.Text = dpiTitular.TERCER_NOMBRE;
                    lbl_nombres_info.Text = txtPrimerNombre.Text + " " + txtSegundoNombre.Text;
                    txtPrimerApellido.Text = dpiTitular.PRIMER_APELLIDO;
                    txtSegundoApellido.Text = dpiTitular.SEGUNDO_APELLIDO;
                    txtApellidoCasada.Text = dpiTitular.CASADA_APELLIDO;
                    lbl_apellidos_info.Text = txtPrimerApellido.Text + " " + txtSegundoApellido.Text;
                    dtpFechaNacimiento.Value = DateTime.ParseExact(dpiTitular.FECHA_NACIMIENTO, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    if (cmbGenero.FindStringExact(dpiTitular.SEXO) < 0)
                        CmbIngresoManual(cmbGenero, true, dpiTitular.SEXO);
                    else
                        cmbGenero.Text = dpiTitular.SEXO;


                    if (cmbEstadoCivil.FindStringExact(dpiTitular.ESTADO_CIVIL) < 0)
                        CmbIngresoManual(cmbEstadoCivil, true, dpiTitular.ESTADO_CIVIL);
                    else
                        cmbEstadoCivil.Text = dpiTitular.ESTADO_CIVIL;

                    if (cmbOcupaciones.FindStringExact(dpiTitular.OCUPACION) < 0)
                        CmbIngresoManual(cmbOcupaciones, true, dpiTitular.OCUPACION);
                    else
                        cmbOcupaciones.Text = dpiTitular.OCUPACION;

                    cmbTiposDocumento.Text = "DPI";
                    txtNumeroSerie.Text = dpiTitular.SERIE_NUMERO;

                    if (dpiTitular.PAIS_NACIMIENTO != "GUATEMALA")
                    {
                        if (cmbPaisNacimiento.FindStringExact(dpiTitular.PAIS_NACIMIENTO) < 0)
                            CmbIngresoManual(cmbPaisNacimiento, true, dpiTitular.PAIS_NACIMIENTO);
                        else
                            cmbPaisNacimiento.Text = dpiTitular.PAIS_NACIMIENTO;

                        cmbPaisNacimiento_SelectionChangeCommitted(sender, e);

                        if (dpiTitular.DEPARTAMENTO_NACIMIENTO != null && dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim() != "" && dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim() != string.Empty)
                        {
                            txtDepartamentoNacimiento.Text = dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim();
                            if (dpiTitular.MUNICIPIO_NACIMIENTO != "" || dpiTitular.MUNICIPIO_NACIMIENTO != string.Empty)
                                txtDepartamentoNacimiento.Text += ", " + dpiTitular.MUNICIPIO_NACIMIENTO;

                            //txtDepartamentoNacimiento.Enabled = false;
                            //SE HACE ESTE CAMBIO PQ ALGUNOS VALORES SON "," DE ESTA MANERA QUEDA A CRITERIO DEL CAPTURADOR
                            txtDepartamentoNacimiento.Enabled = true;
                        }
                        else
                        {
                            txtDepartamentoNacimiento.Text = string.Empty;
                            txtDepartamentoNacimiento.Enabled = true;
                        }
                    }
                    else
                    {
                        cmbPaisNacimiento.Text = dpiTitular.PAIS_NACIMIENTO;

                        if (cmbDeptoNacimiento.FindStringExact(dpiTitular.DEPARTAMENTO_NACIMIENTO) < 0)
                        {
                            CmbIngresoManual(cmbDeptoNacimiento, true, dpiTitular.DEPARTAMENTO_NACIMIENTO);

                            CmbIngresoManual(cmbMunicNacimiento, true, dpiTitular.MUNICIPIO_NACIMIENTO);
                        }
                        else
                        {
                            cmbDeptoNacimiento.Text = dpiTitular.DEPARTAMENTO_NACIMIENTO;
                            CargarCmbMunicipios(cmbMunicNacimiento, false, cmbDeptoNacimiento.SelectedValue.ToString());

                            if (cmbMunicNacimiento.FindStringExact(dpiTitular.MUNICIPIO_NACIMIENTO) < 0)
                                CmbIngresoManual(cmbMunicNacimiento, true, dpiTitular.MUNICIPIO_NACIMIENTO);
                            else
                                cmbMunicNacimiento.Text = dpiTitular.MUNICIPIO_NACIMIENTO;
                        }
                    }

                    cmbPaisResidencia.Text = sedeEstacion.PAIS;
                    //cmbPaisResidencia_SelectionChangeCommitted(sender, e);

                    if (dpiTitular.DEPARTAMENTO_VECINDAD.Trim().Equals(string.Empty) || dpiTitular.DEPARTAMENTO_VECINDAD.Trim().Equals(""))
                    {
                        cmbDeptoResidencia.SelectedIndex = -1;
                        cmbMunicResidencia.SelectedIndex = -1;
                    }
                    else
                    {
                        try
                        {
                            cmbDeptoResidencia.Text = dpiTitular.DEPARTAMENTO_VECINDAD;
                            CargarCmbMunicipios(cmbMunicResidencia, false, cmbDeptoResidencia.SelectedValue.ToString());
                            cmbMunicResidencia.Text = dpiTitular.MUNICIPIO_VECINDAD;
                        }
                        catch
                        {
                            cmbDeptoResidencia.SelectedIndex = -1;
                            cmbMunicResidencia.SelectedIndex = -1;

                            CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1");
                            cmbMunicResidencia.Items.Clear();
                        }
                    }

                    string departamentoVecindad = string.Empty;

                    if (dpiTitular.DEPARTAMENTO_CEDULA.Trim().Equals("") || dpiTitular.DEPARTAMENTO_CEDULA.Trim().Equals(string.Empty))
                        departamentoVecindad = funciones.DepartamentoDesdeCedula(dpiTitular.CEDULA_VECINDAD);
                    else
                        departamentoVecindad = dpiTitular.DEPARTAMENTO_VECINDAD;

                    txtApellidoPadre.Text = txtPrimerApellido.Text;
                    txtApellidoMadre.Text = txtSegundoApellido.Text;
                    pbxFotoDPITitular.Image = dpiTitular.IMAGE;
                    pbxFotoDPITitular.Image.Tag = "FotoTitular";

                    MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                    pbxDPI.Image = pbxCheck.Image;

                    pbxValidacionWsRenap.Image = pbxLoadColor.Image;
                    pbxAlertas.Image = pbxLoad.Image;

                    vValidacionWsRenap = new ValidacionWsRenap();
                    await vValidacionWsRenap.DatosDpi(txtCui.Text, pbxFotoDPITitular.Image, txtPrimerNombre.Text, txtSegundoNombre.Text, txtTercerNombre.Text, txtPrimerApellido.Text, txtSegundoApellido.Text, txtApellidoCasada.Text, dtpFechaNacimiento.Text, cmbGenero.Text, txtNombrePadre.Text + " " + txtApellidoPadre.Text, txtNombreMadre.Text + " " + txtApellidoMadre.Text, cmbEstadoCivil.Text);

                    if (bool.Parse(vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    {
                        MessageBox.Show("Error al consultar el Servicio Web de RENAP: " + vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                        pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                    }

                    else
                    {
                        if (vValidacionWsRenap.txtPadreRENAP.Text.Split(',').Length == 2)
                        {
                            txtNombrePadre.Text = vValidacionWsRenap.txtPadreDPI.Text = vValidacionWsRenap.txtPadreRENAP.Text.Split(',')[0].Trim();
                            txtApellidoPadre.Text = vValidacionWsRenap.txtPadreDPI.Text = vValidacionWsRenap.txtPadreRENAP.Text.Split(',')[1].Trim();
                            txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = false;
                        }
                        else
                            txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = true;

                        if (vValidacionWsRenap.txtMadreRENAP.Text.Split(',').Length == 2)
                        {
                            txtNombreMadre.Text = vValidacionWsRenap.txtMadreDPI.Text = vValidacionWsRenap.txtMadreRENAP.Text.Split(',')[0].Trim();
                            txtApellidoMadre.Text = vValidacionWsRenap.txtMadreDPI.Text = vValidacionWsRenap.txtMadreRENAP.Text.Split(',')[1].Trim();
                            txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = false;
                        }
                        else
                            txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = true;

                        pbxValidacionWsRenap.Image = pbxCheckColor.Image;
                    }

                    //if (bool.Parse(vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["RESULTADO"].ToString()))
                    //    pbxValidacionWsRenap.Image = pbxCheckColor.Image;
                    //else
                    //{
                    //    pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                    //    MessageBox.Show("Error al consultar el Servicio Web de RENAP: " + vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                    //}

                    await Alertas(dpiTitular, pbxAlertas, "TITULAR");

                    _biometricFingerClient.Force();
                    tabHuellas.Enabled = false;
                    for (int intentosMOCTitular = 1; intentosMOCTitular <= parametrizacion.INTENTOS_MOC_TITULAR; intentosMOCTitular++)
                    {
                        pbxMOCH.Image = pbxWarning.Image;

                        if (intentosMOCTitular == 1)
                            MessageBox.Show("¡Inserte la tarjeta en el lector y coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + ")", "Match on Card", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("¡Coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);


                        pbxMOCH.Image = pbxLoad.Image;

                        DataSet dsMOC = await MOC_DPI(txtCui.Text.Trim());
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();


                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCTitular = parametrizacion.INTENTOS_MOC_TITULAR + 1;

                            BloquearControles("MOC");

                            this.Enabled = tab_principal.Enabled = true;

                            pbxMOCH.Image = pbxCheck.Image;
                        }
                        else
                        {
                            if (intentosMOCTitular < parametrizacion.INTENTOS_MOC_TITULAR)
                                MessageBox.Show("¡Las huellas no coinciden, se realizará un nuevo intento, ¡NO retire el DPI!", "Match on Card", MessageBoxButtons.OK);
                            else
                            {
                                MessageBox.Show("¡Límite de intentos MOC alcanzado (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                                this.Enabled = tab_principal.Enabled = true;
                            }

                            pbxMOCH.Image = pbxWarning.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnLeerDPI_Click(). " + ex.Message;
                MessageBox.Show("btnLeerDPI_Click(). " + ex.Message);

                pbxDPI.Image = pbxWarning.Image;
                pbxMOCH.Image = pbxWarning.Image;
            }
            panel_inferior.Enabled = picb_cerrar.Enabled = true;
            tabHuellas.Enabled = true;
        }

        private Task<int> Alertas(DPI dpi, PictureBox pbx, string tipoValidacion)
        {
            return Task.Run(() =>
            {
                DataSet dsResultado = ArmarDsResultado();

                try
                {
                    //pbx.Image = pbxLoad.Image;
                    dsResultado = ConsultaArraigosxNombres(dpi.PRIMER_NOMBRE, dpi.SEGUNDO_NOMBRE, dpi.PRIMER_APELLIDO, dpi.SEGUNDO_APELLIDO);
                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception("Error al consultar ARRAIGOS por NOMBRES: " + dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataTable dtArraigos = new DataTable();
                    Arraigos arraigo = (Arraigos)(dsResultado.Tables[0].Rows[0]["DATOS"]);

                    if (arraigo.informacionArraigos.Tables[0] != null && arraigo.informacionArraigos.Tables[0].Rows.Count > 0)
                    {
                        DataRow[] dr = arraigo.informacionArraigos.Tables[0].Select(" status IN (1, 4) ");
                        if (dr.Length > 0)
                            dtArraigos = arraigo.informacionArraigos.Tables[0].Select(" status IN (1, 4) ").CopyToDataTable();
                    }

                    dsResultado = ConsultaAlertasxNombres(dpi.PRIMER_NOMBRE, dpi.SEGUNDO_NOMBRE, dpi.PRIMER_APELLIDO, dpi.SEGUNDO_APELLIDO);
                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception("Error al consultar ALERTAS por NOMBRES: " + dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    DataTable dtAlertas = new DataTable();
                    Alertas alerta = (Alertas)(dsResultado.Tables[0].Rows[0]["DATOS"]);

                    if (alerta.informacionAlerta.Tables[0] != null && alerta.informacionAlerta.Tables[0].Rows.Count > 0)
                    {
                        DataRow[] dr = arraigo.informacionArraigos.Tables[0].Select(" alerta = 1 ");
                        if (dr.Length > 0)
                            dtAlertas = arraigo.informacionArraigos.Tables[0].Select(" alerta = 1 ").CopyToDataTable();
                    }

                    pbx.Image = ((dtArraigos != null && dtArraigos.Rows.Count > 0) || dtAlertas.Rows.Count > 0) ? pbxWarning.Image : pbxCheck.Image;

                    if (tipoValidacion.ToUpper().Equals("TITULAR"))
                        visorAlertas = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
                    else if (tipoValidacion.ToUpper().Equals("PADRE"))
                        visorAlertasPadre = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
                    else if (tipoValidacion.ToUpper().Equals("MADRE"))
                        visorAlertasMadre = new VisorAlertas(dpiTitular, dtArraigos, dtAlertas, string.Empty);
                }
                catch (Exception ex)
                {
                    pbx.Image = pbxWarning.Image;
                    if (tipoValidacion.ToUpper().Equals("TITULAR"))
                        visorAlertas = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
                    else if (tipoValidacion.ToUpper().Equals("PADRE"))
                        visorAlertasPadre = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
                    else if (tipoValidacion.ToUpper().Equals("MADRE"))
                        visorAlertasMadre = new VisorAlertas(dpiTitular, new DataTable(), new DataTable(), ex.Message);
                }
                return 0;
            });
        }

        public Task<DPI> LeerDPIAsync()
        {
            return Task.Run(() =>
            {
                DPI dpi = new DPI();
                try
                {
                    string[] lectores = new string[10];
                    int TotalLectores = 0;
                    string Err;
                    string NombreLector = "";
                    string Valor = "";

                    DGMReader APIReader = new DGMReader();

                    APIReader.apConnectReader();

                    string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    string _apConnectReader = APIReader.apConnectReader();

                    if (_apIsReaderConnected.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apIsReaderConnected = " + _apIsReaderConnected);

                    if (_apConnectReader.Equals("00") == false)
                        throw new Exception("Error de lectura de DPI, _apConnectReader = " + _apConnectReader);

                    //if (!(_apIsReaderConnected != "00") || !(_apConnectReader != "00"))
                    {
                        //Consultar Lectores disponibles
                        Err = APIReader.apGetReaderNames(ref lectores, ref TotalLectores);

                        NombreLector = lectores[0].ToString();

                        //Determinar si hay una tarjeta insertada en el lector

                        Err = APIReader.apIsCardInserted(NombreLector);

                        //Conectarse con el lector para leer los datos de la tarjeta            
                        Err = APIReader.apConnectReader();

                        //Devuelve en valor el número de CUI impreso y grabado en el chip
                        Err = APIReader.apGetCUI(ref Valor);
                        dpi.CUI = Valor.Trim();

                        //Devuelve en valor el nombre del departamento de vecindad
                        Err = APIReader.apGetDepartmentVecindad(ref Valor);
                        dpi.DEPARTAMENTO_VECINDAD = Valor.Trim();

                        //Devuelve en valor el nombre del municipio de vecindad
                        Err = APIReader.apGetMunicipalityVecindad(ref Valor);
                        dpi.MUNICIPIO_VECINDAD = Valor.Trim();

                        //Devuelve en valor la fecha emisión en el formato como se ve impreso en el DPI
                        Err = APIReader.apGetIssueDate(ref Valor);
                        dpi.FECHA_EMISION = Valor.Trim();

                        //Devuelve en valor la fecha de vencimiento en el formato como se ve impreso en el DPI
                        Err = APIReader.apGetExpireDate(ref Valor);
                        dpi.FECHA_EXPIRA = Valor.Trim();

                        //Devuelve en la variable serie el valor del número de serie del DPI
                        Err = APIReader.apGetCardSerialNumber(ref Valor);
                        dpi.SERIE_NUMERO = Valor.Trim();

                        //Devuelve en valor de la cedula de vecindad en el formato como está impreso en el DPI
                        Err = APIReader.apGetCedula(ref Valor);
                        dpi.CEDULA_VECINDAD = Valor.Trim();

                        //‘Devuelve en valor el nombre del departamento de Extensión de cédula
                        Err = APIReader.apGetDepartmentCedula(ref Valor);
                        dpi.DEPARTAMENTO_CEDULA = Valor.Trim();

                        //Devuelve en valor el nombre del municipio de Extensión de cédula
                        Err = APIReader.apGetMunicipalityCedula(ref Valor);
                        dpi.MUNICIPIO_CEDULA = Valor.Trim();

                        //Devuelve en valor el primer nombre del ciudadano
                        Err = APIReader.apGetFirstName(ref Valor);
                        dpi.PRIMER_NOMBRE = Valor.Trim();

                        //Devuelve en valor el segundo nombre del ciudadano
                        Err = APIReader.apGetMiddleName(ref Valor);
                        dpi.SEGUNDO_NOMBRE = Valor.Trim();

                        //Devuelve en valor el tercer nombre del ciudadano
                        Err = APIReader.apGetThirdName(ref Valor);
                        dpi.TERCER_NOMBRE = Valor.Trim();

                        //Devuelve en valor el apellido nombre del ciudadano
                        Err = APIReader.apGetFirstLastName(ref Valor);
                        dpi.PRIMER_APELLIDO = Valor.Trim();

                        //Devuelve en valor el segundo apellido del ciudadano
                        Err = APIReader.apGetSecondLastName(ref Valor);
                        dpi.SEGUNDO_APELLIDO = Valor.Trim();

                        //Devuelve en valor el apellido de casada del ciudadano
                        Err = APIReader.apGetMarriedName(ref Valor);
                        dpi.CASADA_APELLIDO = Valor.Trim();

                        //Devuelve en valor la fecha de nacimiento del ciudadano en el formato impreso en el DPI
                        Err = APIReader.apGetBirthDate(ref Valor);
                        dpi.FECHA_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el nombre del país de nacimiento del ciudadano 
                        Err = APIReader.apGetCountryBirth(ref Valor);
                        dpi.PAIS_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el nombre del Departamento de nacimiento del ciudadano 
                        Err = APIReader.apGetDepartmentBirth(ref Valor);
                        dpi.DEPARTAMENTO_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el nombre del Municipio de nacimiento del ciudadano 
                        Err = APIReader.apGetMunicipalityBirth(ref Valor);
                        dpi.MUNICIPIO_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el nombre del genero del ciudadano como está impreso en el DPI 
                        Err = APIReader.apGetSex(ref Valor);
                        dpi.SEXO = Valor.Trim();

                        //Devuelve en valor el nombre de la ocupacion del ciudadano como está grabado en el DPI 
                        Err = APIReader.apGetOccupation(ref Valor);
                        dpi.OCUPACION = Valor.Trim();

                        //Devuelve en valor el folio de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetFolio(ref Valor);
                        dpi.FOLIO = Valor.Trim();

                        //Devuelve en valor el libro de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetLibro(ref Valor);
                        dpi.LIBRO = Valor.Trim();

                        //Devuelve en valor la partida de asiento de nacimiento del ciudadano como está impreso en el DPI
                        Err = APIReader.apGetPartida(ref Valor);
                        dpi.PARTIDA = Valor.Trim();

                        //Devuelve en valor el estatus marital del ciudadano 
                        Err = APIReader.apGetMaritalStatus(ref Valor);
                        dpi.ESTADO_CIVIL = Valor.Trim();

                        //Devuelve en valor el MRZ del DPI
                        Err = APIReader.apGetMRZ(ref Valor);
                        dpi.MRZ = Valor.Trim();

                        //Devuelve en valor la nacionalidad del ciudadano
                        Err = APIReader.apGetNationality(ref Valor);
                        dpi.NACIONALIDAD = Valor.Trim();

                        //Devuelve en valor el país de nacimiento del ciudadano
                        Err = APIReader.apGetCountryBirth(ref Valor);
                        dpi.PAIS_NACIMIENTO = Valor.Trim();

                        //Devuelve en valor el país de nacimiento del ciudadano
                        Err = APIReader.apGetEtnia(ref Valor);
                        dpi.ETNIA = Valor.Trim();

                        //Devuelve en la variable imagen los bytes correspondientes a la fotografía del ciudadano ‘almacenada en el DPI       
                        Err = APIReader.apGetFacialImage(ref fotoDPI);
                        dpi.IMAGE = fotoDPI;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            fotoDPI.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] imageBytes = ms.ToArray();

                            // Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            dpi.FOTOGRAFIA_BASE_64 = base64String;
                        }

                        dpi.INFORMACION_DPI_LEIDA = true;
                        dpi.MENSAJE_ERROR = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("LeerDPI(). " + ex.Message);
                    dpi.INFORMACION_DPI_LEIDA = false;
                    dpi.MENSAJE_ERROR = "LeerDPI(). " + ex.Message;
                }
                return dpi;
            }
            );
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

                    //DGMReader APIReader = new DGMReader();
                    //bool Result = false;
                    //int Remaining = -1;
                    //APIReader.apConnectReader();

                    //string _apIsReaderConnected = APIReader.apIsReaderConnected();
                    //string _apConnectReader = APIReader.apConnectReader();

                    //if (!(_apIsReaderConnected != "00") || !(_apConnectReader != "00"))
                    //{
                    //    string respuesta = APIReader.apMatchOnCard(10, ref Result, ref Remaining);

                    //    if (Result)
                    //        ds.Tables[0].Rows[0]["RESULTADO"] = true; //rtbInformacion.Text = "Las huellas coinciden";
                    //    else
                    //        ds.Tables[0].Rows[0]["RESULTADO"] = false; //rtbInformacion.Text = "Las huellas no coinciden";
                    //}
                    //else
                    //    throw new Exception(" Error en la comparacion. _apIsReaderConnected: " + _apIsReaderConnected + ", _apConnectReader: " + _apConnectReader);

                    //ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
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
                    txtSegundoNombre.Enabled = false;
                    txtTercerNombre.Enabled = false;

                    txtPrimerApellido.Enabled = false;
                    txtSegundoApellido.Enabled = false;

                    txtApellidoCasada.Enabled = cmbGenero.Text.Contains("MASCULINO") ? false : true;

                    dtpFechaNacimiento.Enabled = false;
                    cmbGenero.Enabled = false;
                    //cmbEstadoCivil.Enabled = false;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = false;

                    cmbTiposDocumento.Enabled = false;
                    txtNumeroId.Enabled = false;
                    txtNumeroSerie.Enabled = false;

                    //cmbPaisNacimiento.Enabled = false;
                    cmbDeptoNacimiento.Enabled = false;
                    cmbMunicNacimiento.Enabled = false;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    cmbMunicResidencia.Enabled = true;

                    ///////////////PENDIENTE: ESTE CAMPO DEBE SER O NO DEBE SER EDITABLE, AUNQUE HAYA SIDO LEÍDO DEL DPI
                    txtNombrePadre.Enabled = txtApellidoPadre.Enabled = true;
                    txtNombreMadre.Enabled = txtApellidoMadre.Enabled = true;
                    chkDesconocido.Enabled = true;
                    chkDesconocida.Enabled = true;
                }

                if (opcion.Equals("WsRENAP"))
                {
                    txtCui.Enabled = false;
                    txtPrimerNombre.Enabled = false;
                    txtSegundoNombre.Enabled = false;
                    txtTercerNombre.Enabled = false;

                    txtPrimerApellido.Enabled = false;
                    txtSegundoApellido.Enabled = false;

                    txtApellidoCasada.Enabled = cmbGenero.Text.Contains("MASCULINO") ? false : true;

                    dtpFechaNacimiento.Enabled = false;
                    cmbGenero.Enabled = false;
                    //cmbEstadoCivil.Enabled = false;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = cmbTipoPasaporte.Text.Contains("MENOR") ? true : true;

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
                    txtSegundoNombre.Enabled = true;
                    txtTercerNombre.Enabled = true;

                    txtPrimerApellido.Enabled = true;
                    txtSegundoApellido.Enabled = true;
                    txtApellidoCasada.Enabled = true;
                    dtpFechaNacimiento.Enabled = true;
                    cmbGenero.Enabled = true;
                    cmbEstadoCivil.Enabled = true;
                    cmbOcupaciones.Enabled = true;

                    cmbTiposDocumento.Enabled = false;
                    lblNumeroId.Enabled = txtNumeroId.Enabled = true;
                    txtNumeroSerie.Enabled = true;

                    cmbPaisNacimiento.Enabled = true;
                    cmbDeptoNacimiento.Enabled = true;
                    //cmbMunicNacimiento.Enabled = true;

                    cmbPaisResidencia.Enabled = true;
                    cmbDeptoResidencia.Enabled = true;
                    //cmbMunicResidencia.Enabled = true;

                    txtApellidoPadre.Enabled = true;
                    txtApellidoMadre.Enabled = true;

                    cmbTipoIdPadre.Enabled = txtNumeroIdPadre.Enabled = cmbTipoIdMadre.Enabled = txtNumeroIdMadre.Enabled = true;
                    cmbNacionalidad.Enabled = false;
                }

                if (opcion.Equals("MOC"))
                {
                    txtCui.Enabled = true;
                    txtPrimerNombre.Enabled = true;
                    txtSegundoNombre.Enabled = true;
                    txtTercerNombre.Enabled = true;

                    txtPrimerApellido.Enabled = true;
                    txtSegundoApellido.Enabled = true;
                    txtApellidoCasada.Enabled = true;
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

                    ///////////////PENDIENTE: ESTE CAMPO DEBE SER O NO DEBE SER EDITABLE, AUNQUE HAYA SIDO LEÍDO DEL DPI
                    txtApellidoPadre.Enabled = true;
                    txtApellidoMadre.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("BloquearControles(). " + ex.Message);
            }
        }

        public void NuevaInstanciaEnrollment()
        {
            try
            {
                Application.Restart();   
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name + "(). " + ex.Message);
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
                //PROCESAMIENTO DE ROSTROS
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length > 1)
                    for (int i = 0; i < proc.Length; i++)
                    {
                        proc[i].Kill();
                        InvocarProcesamientoRostro();
                    }
                else if (proc.Length == 0)
                    InvocarProcesamientoRostro();

                lblNoDisparos.Text = "0";
                lblComprimir.Text = string.Empty;
                dpiTitular = new DPI();
                dpiPadre = new DPI();
                dpiMadre = new DPI();

                lbl_sede.Text = "SEDE (" + sedeEstacion.PAIS.ToUpper() + ")";
                hrsPasaporte = minPasaporte = segPasaporte = 0;

                pbxCheck.Image.Tag = "Check";
                pbxWarning.Image.Tag = "Warning";
                pbxLoad.Image.Tag = "Loading";
                pbxUsuario.Image.Tag = "FotoDefault";

                pbxCheckColor.Image.Tag = "Check";
                pbxWarningColor.Image.Tag = "Waning";

                intentosMOCTitular = intentosMOCPadre = intentosMOCMadre = 1;

                parametrizacion = new PARAMETRIZACION();

                cmbTipoPasaporte.Enabled = true;

                lblProcesarRostro.Text = string.Empty;
                lblEncriptar.Text = string.Empty;
                //CASO
                txtNoCaso.Text = string.Empty;
                //CARGANDO CATÁLOGOS
                CargarCmbTiposTramite(cmbTipoTramite, true);
                CargarCmbTiposPasaporte(cmbTipoPasaporte, false);
                txtNoRecibo.Text = string.Empty;
                txtNoPasaporte.Text = string.Empty;
                chkNoPasaporte.Checked = false;

                //IDENTIDAD
                lbl_dpi_info.Text = "CUI"; lbl_nombres_info.Text = "Nombres"; lbl_apellidos_info.Text = "Apellidos";
                lblEmisionDPI.Text = string.Empty;
                //lbl_etiqueta_usuario.Text = "Usuario";
                //lbl_usuario.Text = "Nombre usuario";

                txtPrimerNombre.Text = txtSegundoNombre.Text = txtTercerNombre.Text = txtPrimerApellido.Text = txtSegundoApellido.Text = txtApellidoCasada.Text = string.Empty;

                CargarCmbGenero(cmbGenero, false);
                CargarCmbEstadoCivil(cmbEstadoCivil, false);
                cmbEstadoCivil_SelectionChangeCommitted(new Object(), new EventArgs());
                CargarCmbOcupaciones(cmbOcupaciones, false);
                dtpFechaNacimiento.Value = DateTime.Today;

                //CARACTERÍSTICAS FÍSICAS
                //COMPROBAR ESTOS TRUE...
                CargarCmbOjos(cmbOjos, true);
                CargarCmbTez(cmbTez, true);
                CargarCmbCabellos(cmbCabello, true);
                //COMPROBAR ESTOS TRUE...
                txtEstatura.Text = string.Empty;

                //DOCUMENTO DE IDENTIFICACIÓN
                lblCui.Text = "CUI: ";

                CargarCmbTiposDocumento(cmbTiposDocumento, false, "TITULAR");
                txtNumeroId.Text = string.Empty;
                txtNumeroSerie.Text = string.Empty;
                txtCui.Text = string.Empty;

                pbxFotoDPITitular.Image = /*picb_usuario.Image =*/ pbxUsuario.Image;
                pbxDPI.Image = pbxWarning.Image;
                pbxMOCH.Image = pbxWarning.Image;
                pbxMOCF.Image = pbxWarning.Image;
                pbxAlertas.Image = pbxWarning.Image;

                pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                pbxNumeroRecibo.Image = pbxWarning.Image;
                contadorFotos = 0;

                //pbxFotoDPITitular.Image = imageList1.Images["persona.png"];

                //label5.ImageKey = "check-white-24.png";

                //pbxDPI.BackgroundImage = imageList1.Images["warning-white-32.png"];
                //pbxMOC.BackgroundImage = imageList1.Images["warning-white-32.png"];


                //LUGAR DE NACIMIENTO
                CargarCmbPaises(cmbPaisNacimiento, true, "320");
                cmbPaisNacimiento_SelectionChangeCommitted(new Object(), new EventArgs());

                CargarCmbPaises(cmbNacionalidad, true, "320");

                //DIRECCIÓN DE RESIDENCIA
                CargarCmbPaises(cmbPaisResidencia, true, "0");
                cmbPaisResidencia.SelectedValue = int.Parse(sedeEstacion.CODIGO_PAIS);
                cmbPaisResidencia_SelectionChangeCommitted(new Object(), new EventArgs());

                txtResidencia1.Text = string.Empty;
                txtResidencia2.Text = string.Empty;

                //SEDE DE ENTREGA
                CargarCmbPaisesSedeEntrega(cmbPaisSedeEntrega, true, sedeEstacion.PAIS);
                cmbPaisSedeEntrega_SelectionChangeCommitted(new Object(), new EventArgs());
                cmbPaisSedeEntrega.Enabled = sedeEstacion.CODIGO_PAIS.Equals("320") ? false : true;

                //CONTACTO
                txtTelCasa.Text = txtTelCelular.Text = txtTelTrabajo.Text = txtEmail.Text = string.Empty;

                //NOMBRE DE LOS PADRES
                txtNombrePadre.Text = txtApellidoPadre.Text = string.Empty;
                chkDesconocido.Checked = false;
                txtNombreMadre.Text = txtApellidoMadre.Text = string.Empty;
                chkDesconocida.Checked = false;

                txtNombrePadre.Enabled = txtApellidoPadre.Enabled = txtNombreMadre.Enabled = txtApellidoMadre.Enabled = true;
                chkDesconocido.Enabled = chkDesconocida.Enabled = true;

                //DATOS PARA MENORES
                pbxFotoDPIPadre.Image = pbxFotoDPIMadre.Image = pbxUsuario.Image;
                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;

                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;

                CargarCmbTiposDocumento(cmbTipoIdMenor, false, "MENOR");
                txtCuiMenor.Text = string.Empty;
                CargarCmbTiposDocumento(cmbTipoIdPadre, false, "PADRES");
                txtNumeroIdPadre.Text = string.Empty;
                txtNumeroIdPadre.Enabled = false;
                CargarCmbTiposDocumento(cmbTipoIdMadre, false, "PADRES");
                txtNumeroIdMadre.Text = string.Empty;
                txtNumeroIdMadre.Enabled = false;

                lblCuiPadre.Text = lblCuiMadre.Text = "CUI";

                lblNoDoctoPadre.Text = lblNoDoctoMadre.Text = string.Empty;

                grpDocumentoI.Enabled = true;
                grpDatosMenores.Enabled = false;

                //FOTOGRAFÍA       
                try { File.Delete(Application.StartupPath + "\\ENROL\\Fotos\\Rostro.JPG"); } catch { }
                try { File.Delete(Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG"); } catch { }
                try { File.Delete(Application.StartupPath + "\\ENROL\\ROSTRO\\SegmentedFace.jpeg"); } catch { }

                //faceView2.Face = null;
                pbxRostroIcao.Image = null;

                lblEstadoCapturaRostro.Text = string.Empty;

                LiveViewPicBox.Image = null;

                pbxFondoIcao.BackColor = Color.White;
                lblMatchFace.Text = string.Empty;

                ActivarControlesRostro(false);

                if (MainCamera != null)
                    btnActivarCapturaRostroAsync_Click(new Object(), new EventArgs());

                lblTimerCamara.Text = "Tiempo: 00 minuto(s) y  00 segundo(s)";

                btnActivarCapturaRostro.Text = "Activar";

                chkVistaEnVivo.Enabled = false;
                //chkVistaEnVivo.Text = "Detener vista en vivo";

                SetStatusText(Color.Red, "Extracción: Ninguna");
                chkIcao.Checked = true;
                chkIcao.Visible = false;
                txtObservacionesIcao.Text = string.Empty;
                txtObservacionesIcao.Visible = false;

                System.GC.Collect();
                lblVistaPreviaMin.Image = pbxUsuario.Image;
                lblVistaPreviaMin.Refresh();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                //HUELLAS
                CargarCmbDedos(cmbDedoDerecho, true, "DERECHA");
                cmbDedoDerecho.Enabled = false;

                CargarCmbDedos(cmbDedoIzquierdo, true, "IZQUIERDA");
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

                sigPlusNET1.ClearTablet();
                cmdActivarFirma.Enabled = true;
                btnCapturarFirma.Enabled = false;
                cmdLimpiarFirma.Enabled = false;

                txtNoCaso.Focus();

                DesbloquearControles("NUEVO INGRESO");

                btnLeerDPI.Enabled = false;
                pbxDPI.Enabled = pbxMOCH.Enabled = false;
                btnLeerDPIPadre.Enabled = false;
                btnLeerDPIMadre.Enabled = false;
                pbxDPIPadre.Enabled = pbxMOCHPadre.Enabled = pbxDPIMadre.Enabled = pbxMOCHMadre.Enabled = false;

                //CAMARAS
                await Iniciar_Cliente_Fotos(new Object(), new EventArgs());
                //ListarCamaras(cmbCamaras, true);

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

                ControlsBackColor("NEW");
                txtNoCaso.Focus();

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
            txtEstatura.Text = "1.80";

            txtResidencia1.Text = txtResidencia2.Text = "4ta. Avenida 3-08 zona 1, Guatemala";
            cmbDeptoResidencia.Text = "GUATEMALA";
            CargarCmbMunicipios(cmbMunicResidencia, true, cmbDeptoResidencia.SelectedValue.ToString());

            txtTelCelular.Text = txtTelCasa.Text = txtTelTrabajo.Text = "56265627";
            txtEmail.Text = "johny_073@hotmail.com";
            cmbCiudadSedeEntrega.Text = "GUATEMALA";
        }

        private void cmbCamaras_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                _biometricFaceClient.FaceCaptureDevice = cmbCamaras.SelectedItem as NCamera;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbCamaras_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbCamaras_SelectionChangeCommitted(). " + ex.Message);
            }
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

        private void cmbTipoPasaporte_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoPasaporte.SelectedIndex > -1)
                {
                    if (Properties.Settings.Default.ARTICULO_98 && cmbTipoPasaporte.Text.Trim().Contains("DIPLOMATICO Art. 98 Cod. Migración"))
                    {
                        MessageBox.Show("Use este tipo de pasaporte para cónyuges de Diplomáticos SIN NACIONALIDAD GUATEMALTECA. Por favor refiérase al Artículo 98 del Código de Migración, Decreto 44-2016 para mayor información.");
                        CargarCmbPaises(cmbNacionalidad, false, "");
                        CargarCmbPaises(cmbPaisNacimiento, false, "");
                        cmbNacionalidad.Enabled = true;
                    }

                    cmbTiposDocumento.Enabled = true;

                    grpDocumentoI.Enabled = true;                    

                    grpDatosMenores.Enabled = false;
                    CargarCmbTiposDocumento(cmbTipoIdPadre, false, "PADRES");
                    txtNumeroIdPadre.Text = string.Empty;
                    CargarCmbTiposDocumento(cmbTipoIdMadre, false, "PADRES");
                    txtNumeroIdMadre.Text = string.Empty;

                    cmbTipoPasaporte.Enabled = false;
                    if (cmbTipoPasaporte.Text.Trim().Equals("ORDINARIO MENOR") || cmbTipoPasaporte.Text.Trim().Equals("DIPLOMATICO MENOR"))
                    {
                        CargarCmbTiposDocumento(cmbTiposDocumento, false, "MENOR");
                        if (cmbTiposDocumento.Items.Count == 1)
                        {
                            cmbTiposDocumento.SelectedIndex = 0;
                            cmbTiposDocumento_SelectionChangeCommitted(sender, e);
                        }
                            
                        grpDatosMenores.Enabled = true;

                        btnLeerDPI.Enabled = false;
                        pbxDPI.Enabled = pbxMOCH.Enabled = false;
                        btnLeerDPIPadre.Enabled = btnLeerDPIMadre.Enabled = true;
                        pbxDPIPadre.Enabled = pbxMOCHPadre.Enabled = pbxDPIMadre.Enabled = pbxMOCHMadre.Enabled = true;

                        txtNombrePadre.Text = txtApellidoPadre.Text = string.Empty;
                        txtNombreMadre.Text = txtApellidoMadre.Text = string.Empty;

                        chkDesconocido.Checked = chkDesconocida.Checked = false;

                        ControlsBackColor("YOUNG");
                    }
                    else
                    {
                        CargarCmbTiposDocumento(cmbTiposDocumento, false, "TITULAR");
                        grpDatosMenores.Enabled = false;

                        btnLeerDPI.Enabled = true;
                        pbxDPI.Enabled = pbxMOCH.Enabled = true;
                        btnLeerDPIPadre.Enabled = btnLeerDPIMadre.Enabled = false;
                        pbxDPIPadre.Enabled = pbxMOCHPadre.Enabled = pbxDPIMadre.Enabled = pbxMOCHMadre.Enabled = false;

                        ControlsBackColor("ADULT");
                    }
                    cmbTiposDocumento_SelectionChangeCommitted(sender, e);
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbTipoPasaporte_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbTipoPasaporte_SelectionChangeCommitted(). " + ex.Message);
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
                    ControlesSedeResidenciaEntrega(false);

                    if (sCodigoPais.Equals("320"))//GUATEMALA
                    {
                        lblDeptoResidencia.Visible = lblMunicResidencia.Visible = true;
                        cmbDeptoResidencia.Visible = cmbMunicResidencia.Visible = true;

                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "0");
                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");

                        lblEstadoResidencia.Visible = false;
                        cmbEstadoResidencia.Visible = false;

                        CargarCmbEstados(cmbEstadoResidencia, false, "-1");

                        lblZipCodeResidencia.Visible = false;
                        cmbZipCodeResidencia.Visible = false;

                        CargarCmbZipCodes(cmbZipCodeResidencia, false, "-1");                        

                        lblCiudadResidencia.Visible = false;
                        cmbCiudadResidencia.Visible = false;

                        CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, "-1");
                        
                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "####-####";                        
                    }
                    else if (sCodigoPais.Equals("840"))//ESTADOS UNIDOS
                    {
                        lblDeptoResidencia.Visible = lblMunicResidencia.Visible = false;
                        cmbDeptoResidencia.Visible = cmbMunicResidencia.Visible = false;

                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1");

                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");

                        lblEstadoResidencia.Visible = lblZipCodeResidencia.Visible = lblCiudadResidencia.Visible = true;
                        cmbEstadoResidencia.Visible = cmbZipCodeResidencia.Visible = cmbCiudadResidencia.Visible = true;

                        CargarCmbEstados(cmbEstadoResidencia, false, "0");

                        cmbZipCodeResidencia.Enabled = cmbCiudadResidencia.Enabled = false;
                        CargarCmbZipCodes(cmbZipCodeResidencia, false, "-1");
                        CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, "-1");

                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "(###)###-####";                        
                    }
                    else
                    {
                        lblDeptoResidencia.Visible = cmbDeptoResidencia.Visible = false;
                        CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1"); ;

                        lblMunicResidencia.Visible = cmbMunicResidencia.Visible = false;
                        CargarCmbMunicipios(cmbMunicResidencia, false, "-1");

                        lblEstadoResidencia.Visible = cmbEstadoResidencia.Visible = false;
                        CargarCmbEstados(cmbEstadoResidencia, false, "-1");

                        lblZipCodeResidencia.Visible = cmbZipCodeResidencia.Visible = false;

                        lblCiudadResidencia.Visible = false;
                        cmbCiudadResidencia.Visible = false;
                        CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, "-1");

                        txtTelCelular.Mask = txtTelCasa.Mask = txtTelTrabajo.Mask = "############";
                    }
                    btnSedeDireccion.Text = "Sede de Entrega";
                    btnSedeDireccion_Click(sender, e);
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
            try
            {
                if (chkDesconocido.Checked && chkDesconocida.Checked)
                {
                    chkDesconocido.Checked = false;
                    throw new Exception("¡Debe existir por lo menos uno de los dos padres!");
                }


                if (chkDesconocido.Checked)
                {
                    txtNombrePadre.Text = txtApellidoPadre.Text = "DESCONOCIDO";
                    txtNombrePadre.Enabled = txtApellidoPadre.Enabled = false;

                    cmbTipoIdPadre.SelectedIndex = -1;
                    cmbTipoIdPadre.Enabled = false;

                    txtNumeroIdPadre.Enabled = false;
                    txtNumeroIdPadre.Text = string.Empty;
                }
                else
                {
                    txtNombrePadre.Enabled = txtApellidoPadre.Enabled = true;

                    cmbTipoIdPadre.SelectedIndex = -1;
                    cmbTipoIdPadre.Enabled = true;

                    txtNumeroIdPadre.Enabled = true;
                    txtNumeroIdPadre.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "chkDesconocido_CheckedChanged(). " + ex.Message;
                MessageBox.Show("chkDesconocido_CheckedChanged(). " + ex.Message);
            }
        }

        private void chkDesconocida_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkDesconocido.Checked && chkDesconocida.Checked)
                {
                    chkDesconocida.Checked = false;
                    throw new Exception("¡Debe existir por lo menos uno de los dos padres!");
                }

                if (chkDesconocida.Checked)
                {
                    txtNombreMadre.Text = txtApellidoMadre.Text = "DESCONOCIDA";
                    txtNombreMadre.Enabled = txtApellidoMadre.Enabled = false;

                    cmbTipoIdMadre.SelectedIndex = -1;
                    cmbTipoIdMadre.Enabled = false;

                    txtNumeroIdMadre.Enabled = false;
                    txtNumeroIdMadre.Text = string.Empty;
                }
                else
                {
                    txtNombreMadre.Enabled = txtApellidoMadre.Enabled = true;

                    cmbTipoIdMadre.SelectedIndex = -1;
                    cmbTipoIdMadre.Enabled = true;

                    txtNumeroIdMadre.Enabled = true;
                    txtNumeroIdMadre.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "chkDesconocida_CheckedChanged(). " + ex.Message;
                MessageBox.Show("chkDesconocida_CheckedChanged(). " + ex.Message);
            }
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


        private void cmbEstadoResidencia_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                cmbZipCodeResidencia.Enabled = false;
                CargarCmbZipCodes(cmbZipCodeResidencia, false, "-1");

                CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, "0");
                cmbCiudadResidencia.Enabled = false;

                if (cmbEstadoResidencia.SelectedValue != null)
                {
                    string sCodigoEstado = cmbEstadoResidencia.SelectedValue.ToString();

                    if (sCodigoEstado.Equals(String.Empty) || sCodigoEstado.Equals(""))
                        throw new Exception("Código de estado de residencia incorrecto. ");

                    CargarCmbZipCodes(cmbZipCodeResidencia, false, sCodigoEstado);
                    cmbZipCodeResidencia.Enabled = true;
                }
                //else
                    
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbEstadoResidencia_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbEstadoResidencia_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        internal static string foto, no_caso, tipo_pasaporte, nombres, apellidos, apellido_casada, direccion, tel_casa, tel_trabajo, tel_celular, correo, pais, sexo, estado_civil, nacionalidad, fecha_nacimiento,
            depto_nacimiento, muni_nacimiento, pais_nacimiento, identificacion, depto_emision, municipio_emision, color_ojos, color_tez, color_cabello, estatura, padre, madre, sede_entrega,
            partida_nacimiento, libro, folio, acta, pasaporte_autorizado, identificacion_padre, identificacion_madre, autorizado_dgm, usuario, estacion, lugar_fecha, cui_menor, tipo_entrega,
            direccion_entrega1, direccion_entrega2, direccion_entrega3;

        private void cmbEstadoEntrega_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {

                cmbZipCodeEntrega.Enabled = false;
                CargarCmbZipCodes(cmbZipCodeEntrega, false, "-1");

                CargarCmbCiudadesZipCode(cmbCiudadEntrega, false, "0");
                cmbCiudadEntrega.Enabled = false;

                if (cmbEstadoEntrega.SelectedValue != null)
                {
                    string sCodigoEstado = cmbEstadoEntrega.SelectedValue.ToString();

                    if (sCodigoEstado.Equals(String.Empty) || sCodigoEstado.Equals(""))
                        throw new Exception("Código de estado de entrega incorrecto. ");

                    CargarCmbZipCodes(cmbZipCodeEntrega, false, sCodigoEstado);
                    cmbZipCodeEntrega.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbEstadoEntrega_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbEstadoEntrega_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbZipCodeEntrega_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                CargarCmbCiudadesZipCode(cmbCiudadEntrega, false, "0");
                cmbCiudadEntrega.Enabled = false;

                if (cmbZipCodeEntrega.SelectedValue != null)
                {
                    string sZipCode = cmbZipCodeEntrega.SelectedValue.ToString();

                    if (sZipCode.Equals(String.Empty) || sZipCode.Equals(""))
                        throw new Exception("Zip Code de entrega incorrecto. ");

                    CargarCmbCiudadesZipCode(cmbCiudadEntrega, false, sZipCode);
                    cmbCiudadEntrega.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbZipCodeEntrega_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbZipCodeEntrega_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbTipoIdPadre_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtNumeroIdPadre.BackColor = Color.Yellow;
                pbxFotoDPIPadre.Image = pbxUsuario.Image;
                lblCuiPadre.Text = "CUI";
                txtNumeroIdPadre.Enabled = true;
                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;

                txtNumeroIdPadre.Text = string.Empty;

                try
                {
                    cmbTextBoxes.Text = txtNumeroIdPadre.Name;
                    txtNumeroIdPadre.MaxLength = int.Parse(cmbTextBoxes.SelectedValue.ToString());
                } catch
                {
                    txtNumeroIdPadre.MaxLength = 17;
                }
                
                btnLeerDPIPadre.Enabled = cmbTipoIdPadre.Text.Contains("DPI") ? true : false;

                switch (cmbTipoIdPadre.Text)
                {
                    case "PASAPORTE":
                        lblNoDoctoPadre.Text = "No. Pasaporte:";
                        break;

                    case "DPI":
                        lblNoDoctoPadre.Text = "CUI:";
                        txtNumeroIdPadre.MaxLength = txtCui.MaxLength;
                        break;

                    case "CERTIF. DE DEFUNCION":
                        lblNoDoctoPadre.Text = "No. Certificado:";
                        break;

                    case "DOCUMENTO EXTRANJERO":
                        lblNoDoctoPadre.Text = "No. de Documento:";
                        break;

                    case "PARTIDA CON CUI":
                        lblNoDoctoPadre.Text = "CUI:";
                        txtNumeroIdPadre.MaxLength = txtCui.MaxLength;
                        break;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbTipoIdPadre_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbTipoIdPadre_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbTipoIdMadre_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtNumeroIdMadre.BackColor = Color.Yellow;
                pbxFotoDPIMadre.Image = pbxUsuario.Image;
                lblCuiMadre.Text = "CUI";
                txtNumeroIdMadre.Enabled = true;
                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;

                txtNumeroIdMadre.Text = string.Empty;

                try
                {
                    cmbTextBoxes.Text = txtNumeroIdMadre.Name;
                    txtNumeroIdMadre.MaxLength = int.Parse(cmbTextBoxes.SelectedValue.ToString());
                }
                catch
                {
                    txtNumeroIdMadre.MaxLength = 17;

                }
                btnLeerDPIMadre.Enabled = cmbTipoIdMadre.Text.Contains("DPI") ? true : false;

                switch (cmbTipoIdMadre.Text)
                {
                    case "PASAPORTE":
                        lblNoDoctoMadre.Text = "No. Pasaporte:";
                        break;

                    case "DPI":
                        lblNoDoctoMadre.Text = "CUI:";
                        txtNumeroIdMadre.MaxLength = txtCui.MaxLength;
                        break;

                    case "CERTIF. DE DEFUNCION":
                        lblNoDoctoMadre.Text = "No. Certificado:";
                        break;

                    case "DOCUMENTO EXTRANJERO":
                        lblNoDoctoMadre.Text = "No. de Documento:";
                        break;

                    case "PARTIDA CON CUI":
                        lblNoDoctoMadre.Text = "CUI:";
                        txtNumeroIdMadre.MaxLength = txtCui.MaxLength;
                        break;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbTipoIdMadre_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbTipoIdMadre_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private async void txtCui_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    pbxValidacionWsRenap.Image = pbxLoadColor.Image;
                    string msgError = string.Empty;
                    txtCui.Text = txtCui.Text.Trim();

                    if (cmbTipoPasaporte.Text.Equals("") || cmbTipoPasaporte.Text.Equals(string.Empty))
                        msgError += "¡Seleccione un TIPO de PASAPORTE!. ";

                    if (cmbTiposDocumento.Text.Equals("") || cmbTiposDocumento.Text.Equals(string.Empty))
                        msgError += "¡Seleccione un TIPO de DOCUMENTO!. ";

                    if (txtCui.Text.Equals(string.Empty) == true || txtCui.Text.Equals("") == true)
                        msgError += "¡Ingrese un CUI!. ";

                    if ((txtCui.Text.Length == txtCui.MaxLength) == false)
                        msgError += "¡El CUI debe contener " + txtCui.MaxLength.ToString() + " números!. ";

                    if (txtCui.Text.All(char.IsDigit) == false)
                        msgError += "¡El CUI debe contener únicamente NÚMEROS!. ";

                    if (pbxDPI.Image.Tag.Equals("Check"))
                        msgError += "¡Ya se cuenta con la información del DPI cargada en la pantalla!. Clic en botón Nuevo para utilizar esta opción. ";

                    if (msgError.Equals(string.Empty) == false)
                        throw new Exception(msgError);

                    lbl_dpi_info.Text = txtCui.Text;
                    DataSet dsDeptoMunicDPI = Depto_Munic_EmisionDPI(lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 4, 2), lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 2, 2));

                    txtNoRecibo.Text = txtNoRecibo.Text.Trim();
                    if (txtNoRecibo.Text.Equals("") == false && txtNoRecibo.Text.Equals(string.Empty) == false)
                        txtNoRecibo_KeyPress(sender, e);

                    ValidacionWsRenap vRenap = new ValidacionWsRenap();
                    DataSet dsWsRenapCui = vRenap.ConsultaInformacionxCUIWsRenap(txtCui.Text);

                    if (bool.Parse(dsWsRenapCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsWsRenapCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    CuiWsRenap cuiWsRenap = (CuiWsRenap)(dsWsRenapCui.Tables[0].Rows[0]["DATOS"]);
                    DataWsRenap vDataWsRenap = (DataWsRenap)cuiWsRenap.data;

                    DateTime fechaNacimiento = DateTime.ParseExact(vDataWsRenap.fecha_nacimiento, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    int edad = DateTime.Now.Year - fechaNacimiento.Year;

                    //Obtengo la fecha de cumpleaños de este año.
                    DateTime nacimientoAhora = fechaNacimiento.AddYears(edad);
                    //Le resto un año si la fecha actual es anterior 
                    //al día de nacimiento.
                    if (DateTime.Now.CompareTo(nacimientoAhora) < 0)
                        edad--;

                    if (cmbTipoPasaporte.Text.Contains("MENOR") && edad >= 18)
                        throw new Exception("¡El CUI ingresado pertenece a una persona MAYOR de edad!");

                    if (cmbTipoPasaporte.Text.Contains("MENOR") == false && edad < 18)
                        MessageBox.Show("¡El CUI ingresado pertenece a una persona MENOR de edad!"); //throw new Exception("¡El CUI ingresado pertenece a una persona MENOR de edad!");

                    if (vDataWsRenap.foto != null && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                    {
                        // Convert Base64 String to byte[]
                        byte[] imageBytes = Convert.FromBase64String(vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", ""));
                        MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                        // Convert byte[] to Image
                        ms.Write(imageBytes, 0, imageBytes.Length);
                        Image image = Image.FromStream(ms, true);
                        ms.Dispose();

                        //Bitmap bitmap = new Bitmap(image);
                        //bitmap.Save(Application.StartupPath + "\\ENROL\\ROSTRO\\" + "FotoDpiTitular.png");

                        pbxFotoDPITitular.Image = image;
                        pbxFotoDPITitular.Image.Tag = "FotoTitular";
                    }
                    else
                    {
                        pbxFotoDPITitular.Image = pbxUsuario.Image;
                        pbxFotoDPITitular.Image.Tag = "FotoDefault";
                        MessageBox.Show("¡La consulta no devolvió fotografía!");
                    }

                    txtPrimerNombre.Text = vDataWsRenap.primer_nombre.ToUpper();
                    txtSegundoNombre.Text = (vDataWsRenap.segundo_nombre == null) ? "" : vDataWsRenap.segundo_nombre.ToUpper();
                    txtTercerNombre.Text = (vDataWsRenap.tercer_nombre == null) ? "" : vDataWsRenap.tercer_nombre.ToUpper();
                    lbl_nombres_info.Text = txtPrimerNombre.Text + " " + txtSegundoNombre.Text;
                    txtPrimerApellido.Text = vDataWsRenap.primer_apellido.ToUpper();
                    txtSegundoApellido.Text = (vDataWsRenap.segundo_apellido == null) ? "" : vDataWsRenap.segundo_apellido.ToUpper();
                    txtApellidoCasada.Text = (vDataWsRenap.apellido_casada == null) ? "" : vDataWsRenap.apellido_casada.ToUpper();
                    lbl_apellidos_info.Text = txtPrimerApellido.Text + " " + txtSegundoApellido.Text;
                    dtpFechaNacimiento.Value = DateTime.ParseExact(vDataWsRenap.fecha_nacimiento, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    if (vDataWsRenap.genero.ToUpper().Equals("M"))
                        cmbGenero.SelectedValue = "1";
                    else if (vDataWsRenap.genero.ToUpper().Equals("F"))
                        cmbGenero.SelectedValue = "2";
                    else
                        CmbIngresoManual(cmbGenero, true, vDataWsRenap.genero + "");// " - D");

                    switch (vDataWsRenap.estado_civil.ToUpper())
                    {
                        case "S":
                            cmbEstadoCivil.SelectedValue = "1";
                            break;
                        case "C":
                            cmbEstadoCivil.SelectedValue = "2";
                            break;
                        case "U":
                            cmbEstadoCivil.SelectedValue = "3";
                            break;
                        default:
                            CmbIngresoManual(cmbEstadoCivil, true, vDataWsRenap.estado_civil + "");// " - D");
                            break;
                    }

                    
                    if (vDataWsRenap.pais_nacimiento != "GUATEMALA")
                    {
                        if (cmbPaisNacimiento.FindStringExact(vDataWsRenap.pais_nacimiento) < 0)
                        {
                            CmbIngresoManual(cmbPaisNacimiento, true, vDataWsRenap.pais_nacimiento);
                        }
                        else
                            cmbPaisNacimiento.Text = vDataWsRenap.pais_nacimiento;
                        
                        cmbPaisNacimiento_SelectionChangeCommitted(sender, e);

                        if (vDataWsRenap.depto_nacimiento != null && vDataWsRenap.depto_nacimiento.Trim() != "" && vDataWsRenap.depto_nacimiento.Trim() != string.Empty)
                        {
                            txtDepartamentoNacimiento.Text = vDataWsRenap.depto_nacimiento.Trim();
                            if (vDataWsRenap.munic_nacimiento != "" || vDataWsRenap.munic_nacimiento != string.Empty)
                                txtDepartamentoNacimiento.Text += ", " + vDataWsRenap.munic_nacimiento;

                            //txtDepartamentoNacimiento.Enabled = false;
                            //SE HACE ESTE CAMBIO PQ ALGUNOS VALORES SON "," DE ESTA MANERA QUEDA A CRITERIO DEL CAPTURADOR
                            txtDepartamentoNacimiento.Enabled = true;
                        }
                        else
                        {
                            txtDepartamentoNacimiento.Text = string.Empty;
                            txtDepartamentoNacimiento.Enabled = true;
                        }
                    }
                    else
                    {

                        if (cmbDeptoNacimiento.FindStringExact(vDataWsRenap.depto_nacimiento) < 0)
                        {
                            CmbIngresoManual(cmbDeptoNacimiento, true, vDataWsRenap.depto_nacimiento);

                            CmbIngresoManual(cmbMunicNacimiento, true, vDataWsRenap.munic_nacimiento);
                        }
                        else
                        {
                            cmbDeptoNacimiento.Text = vDataWsRenap.depto_nacimiento;
                            CargarCmbMunicipios(cmbMunicNacimiento, false, cmbDeptoNacimiento.SelectedValue.ToString());

                            if (cmbMunicNacimiento.FindStringExact(vDataWsRenap.munic_nacimiento) < 0)
                                CmbIngresoManual(cmbMunicNacimiento, true, vDataWsRenap.munic_nacimiento);
                            else
                                cmbMunicNacimiento.Text = vDataWsRenap.munic_nacimiento;
                        }
                    }                                           
                    
                    if (cmbTipoPasaporte.Text.Contains("MENOR"))
                    {
                        //cmbOcupaciones.SelectedIndex = -1;
                        //cmbOcupaciones.DataSource = null;
                        //cmbOcupaciones.DisplayMember = null;
                        //cmbOcupaciones.ValueMember = null;
                        //cmbOcupaciones.Items.Clear();
                        //cmbOcupaciones.Enabled = false;
                    }
                    //    CmbIngresoManual(cmbOcupaciones, true);
                    //cmbOcupaciones.Text = vDataWsRenap.ocupacion;

                    txtNombrePadre.Text = vDataWsRenap.nombre_padre == null ? string.Empty : vDataWsRenap.nombre_padre.ToUpper().Split(',')[0].Trim();
                    txtApellidoPadre.Text = vDataWsRenap.nombre_padre == null ? string.Empty : vDataWsRenap.nombre_padre.ToUpper().Split(',')[1].Trim();

                    txtNombreMadre.Text = vDataWsRenap.nombre_madre == null ? string.Empty : vDataWsRenap.nombre_madre.ToUpper().Split(',')[0].Trim();
                    txtApellidoMadre.Text = vDataWsRenap.nombre_madre == null ? string.Empty : vDataWsRenap.nombre_madre.ToUpper().Split(',')[1].Trim();

                    if (txtApellidoPadre.Text.Trim().Equals("") || txtApellidoPadre.Text.Trim().Equals(string.Empty))
                        if(txtPrimerApellido.Text.Trim().Equals("") == false && txtPrimerApellido.Text.Trim().Equals(string.Empty) == false)
                            txtApellidoPadre.Text = txtPrimerApellido.Text;

                    if (txtApellidoMadre.Text.Trim().Equals("") || txtApellidoMadre.Text.Trim().Equals(string.Empty))
                        if (txtSegundoApellido.Text.Trim().Equals("") == false && txtSegundoApellido.Text.Trim().Equals(string.Empty) == false)
                            txtApellidoMadre.Text = txtSegundoApellido.Text;

                    dpiTitular.CUI = txtCui.Text;
                    dpiTitular.PRIMER_NOMBRE = txtPrimerNombre.Text;
                    dpiTitular.SEGUNDO_NOMBRE = txtSegundoNombre.Text;
                    dpiTitular.PRIMER_APELLIDO = txtPrimerApellido.Text;
                    dpiTitular.SEGUNDO_APELLIDO = txtSegundoApellido.Text;
                    dpiTitular.FECHA_NACIMIENTO = dtpFechaNacimiento.Text;
                    dpiTitular.SEXO = cmbGenero.Text;
                    dpiTitular.IMAGE = pbxFotoDPITitular.Image;
                    
                    pbxValidacionWsRenap.Image = pbxCheckColor.Image;

                    await Alertas(dpiTitular, pbxAlertas, "TITULAR");

                    BloquearControles("WsRENAP");
                }
            }
            catch (Exception ex)
            {
                pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                MessageBox.Show("txtCui_KeyPress(). " + ex.Message);
                txtMensaje.Text = "txtCui_KeyPress(). " + ex.Message;
            }
        }

        private async void btnLeerDPIPadre_Click(object sender, EventArgs e)
        {
            try
            {
                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;

                DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    panel_inferior.Enabled = picb_cerrar.Enabled = false;

                    pbxDPIPadre.Image = pbxLoad.Image;

                    dpiPadre = await LeerDPIAsync();

                    if (dpiPadre.INFORMACION_DPI_LEIDA == false)
                        throw new Exception(dpiPadre.MENSAJE_ERROR);

                    if (dpiPadre.SEXO.Contains("MASCULINO") == false && dpiPadre.SEXO.Equals("M") == false)
                        throw new Exception("¡El género del padre debe ser MASCULINO!");

                    cmbTipoIdPadre.Text = "DPI";
                    cmbTipoIdPadre_SelectionChangeCommitted(sender, e);
                    cmbTipoIdPadre.Enabled = txtNumeroIdPadre.Enabled = false;
                    txtNumeroIdPadre.Text = dpiPadre.CUI;
                    chkDesconocido.Checked = false;

                    dpiPadre.PARENTESCO = "Padre";
                    lblCuiPadre.Text = dpiPadre.CUI;
                    txtNombrePadre.Text = (dpiPadre.PRIMER_NOMBRE + ((dpiPadre.SEGUNDO_NOMBRE != null && dpiPadre.SEGUNDO_NOMBRE != "" && dpiPadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiPadre.SEGUNDO_NOMBRE : "") + ((dpiPadre.TERCER_NOMBRE != null && dpiPadre.TERCER_NOMBRE != "" && dpiPadre.TERCER_NOMBRE != string.Empty) ? " " + dpiPadre.TERCER_NOMBRE : "")).ToUpper();
                    txtApellidoPadre.Text = (dpiPadre.PRIMER_APELLIDO + ((dpiPadre.SEGUNDO_APELLIDO != null && dpiPadre.SEGUNDO_APELLIDO != "" && dpiPadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiPadre.SEGUNDO_APELLIDO : "")).ToUpper();
                    txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = false;

                    pbxFotoDPIPadre.Image = dpiPadre.IMAGE;
                    pbxFotoDPIPadre.Image.Tag = "FotoPadre";

                    MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                    pbxDPIPadre.Image = pbxCheck.Image;

                    for (int intentosMOCTitular = 1; intentosMOCTitular <= parametrizacion.INTENTOS_MOC_TITULAR; intentosMOCTitular++)
                    {
                        pbxMOCHPadre.Image = pbxWarning.Image;

                        if (intentosMOCTitular == 1)
                            MessageBox.Show("¡Inserte la tarjeta en el lector y coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + ")", "Match on Card", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("¡Coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);
                   
                        pbxMOCHPadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Cancel();

                        DataSet dsMOC = await MOC_DPI(lblCuiPadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCTitular = parametrizacion.INTENTOS_MOC_TITULAR + 1;

                            this.Enabled = tab_principal.Enabled = true;

                            pbxMOCHPadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            if (intentosMOCTitular < parametrizacion.INTENTOS_MOC_TITULAR)
                                MessageBox.Show("¡Las huellas no coinciden, se realizará un nuevo intento, ¡NO retire el DPI!", "Match on Card", MessageBoxButtons.OK);
                            else
                            {
                                MessageBox.Show("¡Límite de intentos MOC alcanzado (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                                this.Enabled = tab_principal.Enabled = true;
                            }

                            pbxMOCHPadre.Image = pbxWarning.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnLeerDPIPadre_Click(). " + ex.Message;
                MessageBox.Show("btnLeerDPIPadre_Click(). " + ex.Message);

                pbxDPIPadre.Image = pbxWarning.Image;
                pbxMOCHPadre.Image = pbxWarning.Image;
            }
            panel_inferior.Enabled = picb_cerrar.Enabled = true;
        }

        private void pbxFotoDPIPadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoIdPadre.Text.Contains("DPI") || cmbTipoIdPadre.Text.Contains("PARTIDA CON CUI"))
                {
                    if (dpiPadre.CUI.Equals("") == false && dpiPadre.CUI.All(char.IsDigit) == true)
                    {
                        VisorDatosDPI vDatosDpi = new VisorDatosDPI(dpiPadre);
                        vDatosDpi.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxFotoDPIPadre_Click(). " + ex.Message;
                MessageBox.Show("pbxFotoDPIPadre_Click(). " + ex.Message);
            }
        }

        private async void btnLeerDPIMadre_Click(object sender, EventArgs e)
        {
            try
            {
                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;

                DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    panel_inferior.Enabled = picb_cerrar.Enabled = false;

                    pbxDPIMadre.Image = pbxLoad.Image;

                    dpiMadre = await LeerDPIAsync();

                    if (dpiMadre.INFORMACION_DPI_LEIDA == false)
                        throw new Exception(dpiMadre.MENSAJE_ERROR);

                    if (dpiMadre.SEXO.Contains("FEMENINO") == false && dpiMadre.SEXO.Equals("F") == false)
                        throw new Exception("¡El género del madre debe ser FEMENINO!");

                    cmbTipoIdMadre.Text = "DPI";
                    cmbTipoIdMadre_SelectionChangeCommitted(sender, e);
                    cmbTipoIdMadre.Enabled = txtNumeroIdMadre.Enabled = false;
                    txtNumeroIdMadre.Text = dpiMadre.CUI;
                    chkDesconocida.Checked = false;

                    dpiMadre.PARENTESCO = "MADRE";
                    lblCuiMadre.Text = dpiMadre.CUI;
                    txtNombreMadre.Text = (dpiMadre.PRIMER_NOMBRE + ((dpiMadre.SEGUNDO_NOMBRE != null && dpiMadre.SEGUNDO_NOMBRE != "" && dpiMadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiMadre.SEGUNDO_NOMBRE : "") + ((dpiMadre.TERCER_NOMBRE != null && dpiMadre.TERCER_NOMBRE != "" && dpiMadre.TERCER_NOMBRE != string.Empty) ? " " + dpiMadre.TERCER_NOMBRE : "")).ToUpper();
                    txtApellidoMadre.Text = (dpiMadre.PRIMER_APELLIDO + ((dpiMadre.SEGUNDO_APELLIDO != null && dpiMadre.SEGUNDO_APELLIDO != "" && dpiMadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiMadre.SEGUNDO_APELLIDO : "")).ToUpper();
                    txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = false;

                    pbxFotoDPIMadre.Image = dpiMadre.IMAGE;
                    pbxFotoDPIMadre.Image.Tag = "FotoMadre";

                    MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                    pbxDPIMadre.Image = pbxCheck.Image;

                    for (int intentosMOCTitular = 1; intentosMOCTitular <= parametrizacion.INTENTOS_MOC_TITULAR; intentosMOCTitular++)
                    {
                        pbxMOCHMadre.Image = pbxWarning.Image;

                        if (intentosMOCTitular == 1)
                            MessageBox.Show("¡Inserte la tarjeta en el lector y coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + ")", "Match on Card", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("¡Coloque el dedo cuando la luz del sensor se encienda! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHMadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Cancel();

                        DataSet dsMOC = await MOC_DPI(lblCuiMadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCTitular = parametrizacion.INTENTOS_MOC_TITULAR + 1;

                            this.Enabled = tab_principal.Enabled = true;

                            pbxMOCHMadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            if (intentosMOCTitular < parametrizacion.INTENTOS_MOC_TITULAR)
                                MessageBox.Show("¡Las huellas no coinciden, se realizará un nuevo intento, ¡NO retire el DPI!", "Match on Card", MessageBoxButtons.OK);
                            else
                            {
                                MessageBox.Show("¡Límite de intentos MOC alcanzado (" + intentosMOCTitular + "/" + parametrizacion.INTENTOS_MOC_TITULAR + ")", "Match on Card", MessageBoxButtons.OK);
                                this.Enabled = tab_principal.Enabled = true;
                            }

                            pbxMOCHMadre.Image = pbxWarning.Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnLeerDPIMadre_Click(). " + ex.Message;
                MessageBox.Show("btnLeerDPIMadre_Click(). " + ex.Message);

                pbxDPIMadre.Image = pbxWarning.Image;
                pbxMOCHMadre.Image = pbxWarning.Image;
            }
            panel_inferior.Enabled = picb_cerrar.Enabled = true;
        }

        private void pbxFotoDPIMadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoIdMadre.Text.Contains("DPI") || cmbTipoIdMadre.Text.Contains("PARTIDA CON CUI"))
                {
                    if (dpiMadre.CUI.Equals("") == false && dpiMadre.CUI.All(char.IsDigit) == true)
                    {
                        VisorDatosDPI vDatosDpi = new VisorDatosDPI(dpiMadre);
                        vDatosDpi.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxFotoDPIMadre_Click(). " + ex.Message;
                MessageBox.Show("pbxFotoDPIMadre_Click(). " + ex.Message);
            }
        }

        private void txtNumeroIdPadre_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    string msgError = string.Empty;
                    txtNumeroIdPadre.Text = txtNumeroIdPadre.Text.Trim();

                    if(cmbTipoPasaporte.Text.Contains("MENOR") == false)
                        msgError += "¡Seleccione un TIPO de PASAPORTE para menores de edad!. ";

                    if (cmbTipoIdPadre.Text.Contains("DPI") == false && cmbTipoIdPadre.Text.Contains("PARTIDA CON CUI") == false)
                        msgError += "¡Seleccione un TIPO de documento DPI o PARTIDA CON CUI!. ";
                    
                    if (txtNumeroIdPadre.Text.Equals(string.Empty) == true || txtNumeroIdPadre.Text.Equals("") == true)
                        msgError += "¡Ingrese un CUI!. ";

                    if ((txtNumeroIdPadre.Text.Length == txtCui.MaxLength) == false)
                        msgError += "¡El CUI debe contener " + txtCui.MaxLength.ToString() + " números!. ";

                    if (txtNumeroIdPadre.Text.All(char.IsDigit) == false)
                        msgError += "¡El CUI debe contener únicamente NÚMEROS!. ";

                    if (pbxDPIPadre.Image.Tag.Equals("Check"))
                        msgError += "¡Ya se cuenta con la información del DPI cargada en la pantalla!. Clic en botón Nuevo para utilizar esta opción. ";

                    if (msgError.Equals(string.Empty) == false)
                        throw new Exception(msgError);

                    ValidacionWsRenap vRenap = new ValidacionWsRenap();
                    DataSet dsWsRenapCui = vRenap.ConsultaInformacionxCUIWsRenap(txtNumeroIdPadre.Text);

                    if (bool.Parse(dsWsRenapCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsWsRenapCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    CuiWsRenap cuiWsRenap = (CuiWsRenap)(dsWsRenapCui.Tables[0].Rows[0]["DATOS"]);
                    DataWsRenap vDataWsRenap = (DataWsRenap)cuiWsRenap.data;

                    if (vDataWsRenap.genero.ToUpper().Equals("M"))
                        dpiPadre.SEXO = "MASCULINO";
                    else if (vDataWsRenap.genero.ToUpper().Equals("F"))
                        dpiPadre.SEXO = "FEMENINO";
                    else
                        dpiPadre.SEXO = vDataWsRenap.genero + "";// " - D");

                    if (dpiPadre.SEXO.Contains("MASCULINO") == false && dpiPadre.SEXO.Equals("M") == false)
                        throw new Exception("¡El género del padre debe ser MASCULINO!");

                    if (vDataWsRenap.foto != null && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                    {
                        // Convert Base64 String to byte[]
                        byte[] imageBytes = Convert.FromBase64String(vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", ""));
                        MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                        // Convert byte[] to Image
                        ms.Write(imageBytes, 0, imageBytes.Length);
                        Image image = Image.FromStream(ms, true);
                        ms.Dispose();

                        pbxFotoDPIPadre.Image = image;
                        pbxFotoDPIPadre.Image.Tag = "FotoPadre";
                        dpiPadre.IMAGE = image;
                    }
                    else
                    {
                        pbxFotoDPIPadre.Image = pbxUsuario.Image;
                        MessageBox.Show("¡La consulta no devolvió fotografía!");
                    }

                    dpiPadre.CUI = txtNumeroIdPadre.Text;
                    dpiPadre.PARENTESCO = "Padre";
                    lblCuiPadre.Text = dpiPadre.CUI;

                    dpiPadre.DEPARTAMENTO_VECINDAD = "";
                    dpiPadre.MUNICIPIO_VECINDAD = "";
                    dpiPadre.FECHA_EMISION = "";
                    dpiPadre.FECHA_EXPIRA = "";
                    dpiPadre.SERIE_NUMERO = "";
                    dpiPadre.CEDULA_VECINDAD = "";
                    dpiPadre.DEPARTAMENTO_CEDULA = "";
                    dpiPadre.MUNICIPIO_CEDULA = "";
                    dpiPadre.PRIMER_NOMBRE = vDataWsRenap.primer_nombre;
                    dpiPadre.SEGUNDO_NOMBRE = vDataWsRenap.segundo_nombre;
                    dpiPadre.TERCER_NOMBRE = vDataWsRenap.tercer_nombre;
                    dpiPadre.PRIMER_APELLIDO = vDataWsRenap.primer_apellido;
                    dpiPadre.SEGUNDO_APELLIDO = vDataWsRenap.segundo_apellido;
                    dpiPadre.CASADA_APELLIDO = vDataWsRenap.apellido_casada;
                    dpiPadre.FECHA_NACIMIENTO = vDataWsRenap.fecha_nacimiento;
                    dpiPadre.PAIS_NACIMIENTO = vDataWsRenap.pais_nacimiento;
                    dpiPadre.DEPARTAMENTO_NACIMIENTO = vDataWsRenap.depto_nacimiento;
                    dpiPadre.MUNICIPIO_NACIMIENTO = vDataWsRenap.munic_nacimiento;

                    //dpiPadre.SEXO = vDataWsRenap.genero;
                    dpiPadre.OCUPACION = "";
                    dpiPadre.FOLIO = "";
                    dpiPadre.LIBRO = "";
                    dpiPadre.PARTIDA = "";

                    switch (vDataWsRenap.estado_civil.ToUpper())
                    {
                        case "S":
                            dpiPadre.ESTADO_CIVIL = "SOLTERO";
                            break;
                        case "C":
                            dpiPadre.ESTADO_CIVIL = "CASADO";
                            break;
                        case "U":
                            dpiPadre.ESTADO_CIVIL = "UNION DE HECHO";
                            break;
                        default:
                            dpiPadre.ESTADO_CIVIL = vDataWsRenap.estado_civil + "";// " - D");
                            break;
                    }
                    //dpiPadre.ESTADO_CIVIL = vDataWsRenap.estado_civil;
                    dpiPadre.MRZ = "";
                    dpiPadre.NACIONALIDAD = "";
                    dpiPadre.ETNIA = "";

                    txtNombrePadre.Text = (dpiPadre.PRIMER_NOMBRE + ((dpiPadre.SEGUNDO_NOMBRE != null && dpiPadre.SEGUNDO_NOMBRE != "" && dpiPadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiPadre.SEGUNDO_NOMBRE : "") + ((dpiPadre.TERCER_NOMBRE != null && dpiPadre.TERCER_NOMBRE != "" && dpiPadre.TERCER_NOMBRE != string.Empty) ? " " + dpiPadre.TERCER_NOMBRE : "")).ToUpper();
                    txtApellidoPadre.Text = (dpiPadre.PRIMER_APELLIDO + ((dpiPadre.SEGUNDO_APELLIDO != null && dpiPadre.SEGUNDO_APELLIDO != "" && dpiPadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiPadre.SEGUNDO_APELLIDO : "")).ToUpper();
                    txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = false;

                    txtNumeroIdPadre.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtNumeroIdPadre_KeyPress(). " + ex.Message);
                txtMensaje.Text = "txtNumeroIdPadre_KeyPress(). " + ex.Message;
            }
        }

        private void txtNumeroIdMadre_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13)
                {
                    string msgError = string.Empty;
                    txtNumeroIdMadre.Text = txtNumeroIdMadre.Text.Trim();

                    if (cmbTipoPasaporte.Text.Contains("MENOR") == false)
                        msgError += "¡Seleccione un TIPO de PASAPORTE para menores de edad!. ";

                    if (cmbTipoIdMadre.Text.Contains("DPI") == false && cmbTipoIdMadre.Text.Contains("PARTIDA CON CUI") == false)
                        msgError += "¡Seleccione un TIPO de documento DPI o PARTIDA CON CUI!. ";

                    if (txtNumeroIdMadre.Text.Equals(string.Empty) == true || txtNumeroIdMadre.Text.Equals("") == true)
                        msgError += "¡Ingrese un CUI!. ";

                    if ((txtNumeroIdMadre.Text.Length == txtCui.MaxLength) == false)
                        msgError += "¡El CUI debe contener " + txtCui.MaxLength.ToString() + " números!. ";

                    if (txtNumeroIdMadre.Text.All(char.IsDigit) == false)
                        msgError += "¡El CUI debe contener únicamente NÚMEROS!. ";

                    if (pbxDPIMadre.Image.Tag.Equals("Check"))
                        msgError += "¡Ya se cuenta con la información del DPI cargada en la pantalla!. Clic en botón Nuevo para utilizar esta opción. ";

                    if (msgError.Equals(string.Empty) == false)
                        throw new Exception(msgError);

                    ValidacionWsRenap vRenap = new ValidacionWsRenap();
                    DataSet dsWsRenapCui = vRenap.ConsultaInformacionxCUIWsRenap(txtNumeroIdMadre.Text);

                    if (bool.Parse(dsWsRenapCui.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsWsRenapCui.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    CuiWsRenap cuiWsRenap = (CuiWsRenap)(dsWsRenapCui.Tables[0].Rows[0]["DATOS"]);
                    DataWsRenap vDataWsRenap = (DataWsRenap)cuiWsRenap.data;

                    if (vDataWsRenap.genero.ToUpper().Equals("M"))
                        dpiMadre.SEXO = "MASCULINO";
                    else if (vDataWsRenap.genero.ToUpper().Equals("F"))
                        dpiMadre.SEXO = "FEMENINO";
                    else
                        dpiMadre.SEXO = vDataWsRenap.genero + "";// " - D");

                    if (dpiMadre.SEXO.Contains("FEMENINO") == false && dpiMadre.SEXO.Equals("F") == false)
                        MessageBox.Show("¡El género del madre debe ser FEMENINO!");//throw new Exception("¡El género del padre debe ser FEMENINO!");

                    if (vDataWsRenap.foto != null && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(string.Empty) && !vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", "").Equals(""))
                    {
                        // Convert Base64 String to byte[]
                        byte[] imageBytes = Convert.FromBase64String(vDataWsRenap.foto.ToString().Replace("data:image/png;base64,", ""));
                        MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                        // Convert byte[] to Image
                        ms.Write(imageBytes, 0, imageBytes.Length);
                        Image image = Image.FromStream(ms, true);

                        pbxFotoDPIMadre.Image = image;
                        pbxFotoDPIMadre.Image.Tag = "FotoPadre";
                        dpiMadre.IMAGE = image;
                    }
                    else
                    {
                        pbxFotoDPIMadre.Image = pbxUsuario.Image;
                        MessageBox.Show("¡La consulta no devolvió fotografía!");
                    }

                    dpiMadre.CUI = txtNumeroIdMadre.Text;
                    dpiMadre.PARENTESCO = "Padre";
                    lblCuiMadre.Text = dpiMadre.CUI;

                    dpiMadre.DEPARTAMENTO_VECINDAD = "";
                    dpiMadre.MUNICIPIO_VECINDAD = "";
                    dpiMadre.FECHA_EMISION = "";
                    dpiMadre.FECHA_EXPIRA = "";
                    dpiMadre.SERIE_NUMERO = "";
                    dpiMadre.CEDULA_VECINDAD = "";
                    dpiMadre.DEPARTAMENTO_CEDULA = "";
                    dpiMadre.MUNICIPIO_CEDULA = "";
                    dpiMadre.PRIMER_NOMBRE = vDataWsRenap.primer_nombre;
                    dpiMadre.SEGUNDO_NOMBRE = vDataWsRenap.segundo_nombre;
                    dpiMadre.TERCER_NOMBRE = vDataWsRenap.tercer_nombre;
                    dpiMadre.PRIMER_APELLIDO = vDataWsRenap.primer_apellido;
                    dpiMadre.SEGUNDO_APELLIDO = vDataWsRenap.segundo_apellido;
                    dpiMadre.CASADA_APELLIDO = vDataWsRenap.apellido_casada;
                    dpiMadre.FECHA_NACIMIENTO = vDataWsRenap.fecha_nacimiento;
                    dpiMadre.PAIS_NACIMIENTO = vDataWsRenap.pais_nacimiento;
                    dpiMadre.DEPARTAMENTO_NACIMIENTO = vDataWsRenap.depto_nacimiento;
                    dpiMadre.MUNICIPIO_NACIMIENTO = vDataWsRenap.munic_nacimiento;

                    //dpiPadre.SEXO = vDataWsRenap.genero;
                    dpiMadre.OCUPACION = "";
                    dpiMadre.FOLIO = "";
                    dpiMadre.LIBRO = "";
                    dpiMadre.PARTIDA = "";

                    switch (vDataWsRenap.estado_civil.ToUpper())
                    {
                        case "S":
                            dpiMadre.ESTADO_CIVIL = "SOLTERO";
                            break;
                        case "C":
                            dpiMadre.ESTADO_CIVIL = "CASADO";
                            break;
                        case "U":
                            dpiMadre.ESTADO_CIVIL = "UNION DE HECHO";
                            break;
                        default:
                            dpiMadre.ESTADO_CIVIL = vDataWsRenap.estado_civil + "";// " - D");
                            break;
                    }
                    //dpiPadre.ESTADO_CIVIL = vDataWsRenap.estado_civil;
                    dpiMadre.MRZ = "";
                    dpiMadre.NACIONALIDAD = "";
                    dpiMadre.ETNIA = "";

                    txtNombreMadre.Text = (dpiMadre.PRIMER_NOMBRE + ((dpiMadre.SEGUNDO_NOMBRE != null && dpiMadre.SEGUNDO_NOMBRE != "" && dpiMadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiMadre.SEGUNDO_NOMBRE : "") + ((dpiMadre.TERCER_NOMBRE != null && dpiMadre.TERCER_NOMBRE != "" && dpiMadre.TERCER_NOMBRE != string.Empty) ? " " + dpiMadre.TERCER_NOMBRE : "")).ToUpper();
                    txtApellidoMadre.Text = (dpiMadre.PRIMER_APELLIDO + ((dpiMadre.SEGUNDO_APELLIDO != null && dpiMadre.SEGUNDO_APELLIDO != "" && dpiMadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiMadre.SEGUNDO_APELLIDO : "")).ToUpper();
                    txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = false;

                    txtNumeroIdMadre.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtNumeroIdMadre_KeyPress(). " + ex.Message);
                txtMensaje.Text = "txtNumeroIdMadre_KeyPress(). " + ex.Message;
            }
        }

        private void cmbEstadoCivil_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtApellidoCasada.Enabled = true;
                txtApellidoCasada.Leave -= TextBoxLeave_Action;
                txtApellidoCasada.BackColor = Color.White;

                if (cmbEstadoCivil.Text.Contains("SOLTERO") || cmbEstadoCivil.Text.Contains("SOLTERA"))
                {
                    txtApellidoCasada.Text = string.Empty;
                    txtApellidoCasada.Enabled = false;
                }
                else if (cmbEstadoCivil.Text.Contains("CASADO") || cmbEstadoCivil.Text.Contains("CASADA"))
                {
                    if (cmbTipoPasaporte.Text.Equals("") == false && cmbTipoPasaporte.Text.Equals(string.Empty) == false)
                    {
                        if (cmbGenero.Text.ToUpper().Equals("FEMENINO"))
                        {
                            txtApellidoCasada.Leave += TextBoxLeave_Action;
                            txtApellidoCasada.BackColor = Color.Yellow;
                            txtApellidoCasada.Focus();
                        }
                    }
                }
                //txtApellidoCasada.Enabled = ((cmbEstadoCivil.Text.Contains("CASADO") || cmbEstadoCivil.Text.Contains("CASADA") || cmbEstadoCivil.Text.Contains("CASADO(A)") || cmbEstadoCivil.Text.Contains("UNION DE HECHO") || cmbEstadoCivil.Text.Contains("UNIDA")) && cmbGenero.Text.Equals("FEMENINO")) ? true : false;
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
            try
            {
                if (e.KeyChar == (char)13 && (txtNoCaso.Text.Equals(string.Empty) == false && txtNoCaso.Text.Equals("") == false))//)Keys.Enter)
                {
                    
                    string rutaPDF = Path.Combine(Application.StartupPath, "ENROL", "PDFs", "CasoPasaporte_" + txtNoCaso.Text.Trim() + ".pdf");
                    if (File.Exists(rutaPDF))
                        System.Diagnostics.Process.Start(rutaPDF);
                }
            }
            catch (Exception ex)
            {
                
                txtMensaje.Text = "txtNoRecibo_KeyPress(). " + ex.Message;
                MessageBox.Show("txtNoRecibo_KeyPress(). " + ex.Message);
            }
        }

        private async void pbxDPI_Click(object sender, EventArgs e)
        {
            try
            {

                if (pbxMOCH.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPI.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPI.Image = pbxWarning.Image;
                        pbxMOCH.Image = pbxWarning.Image;
                        pbxAlertas.Image = pbxWarning.Image;

                        cmbTipoPasaporte.Enabled = false;
                        txtNumeroId.Text = txtNumeroSerie.Text = txtCui.Text = string.Empty;

                        //panel_inferior.Enabled = picb_cerrar.Enabled = false;

                        pbxDPI.Image = pbxLoad.Image;
                        dpiTitular = await LeerDPIAsync();

                        if (dpiTitular.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiTitular.MENSAJE_ERROR);

                        txtCui.Text = dpiTitular.CUI;
                        lbl_dpi_info.Text = txtCui.Text;

                        DataSet dsDeptoMunicDPI = Depto_Munic_EmisionDPI(lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 4, 2), lbl_dpi_info.Text.Substring(lbl_dpi_info.Text.Length - 2, 2));

                        txtPrimerNombre.Text = dpiTitular.PRIMER_NOMBRE;
                        txtSegundoNombre.Text = dpiTitular.SEGUNDO_NOMBRE;
                        txtTercerNombre.Text = dpiTitular.TERCER_NOMBRE;
                        lbl_nombres_info.Text = txtPrimerNombre.Text + " " + txtSegundoNombre.Text;
                        txtPrimerApellido.Text = dpiTitular.PRIMER_APELLIDO;
                        txtSegundoApellido.Text = dpiTitular.SEGUNDO_APELLIDO;
                        txtApellidoCasada.Text = dpiTitular.CASADA_APELLIDO;
                        lbl_apellidos_info.Text = txtPrimerApellido.Text + " " + txtSegundoApellido.Text;
                        dtpFechaNacimiento.Value = DateTime.ParseExact(dpiTitular.FECHA_NACIMIENTO, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        if (cmbGenero.FindStringExact(dpiTitular.SEXO) < 0)
                            CmbIngresoManual(cmbGenero, true, dpiTitular.SEXO);
                        else
                            cmbGenero.Text = dpiTitular.SEXO;


                        if (cmbEstadoCivil.FindStringExact(dpiTitular.ESTADO_CIVIL) < 0)
                            CmbIngresoManual(cmbEstadoCivil, true, dpiTitular.ESTADO_CIVIL);
                        else
                            cmbEstadoCivil.Text = dpiTitular.ESTADO_CIVIL;

                        if (cmbOcupaciones.FindStringExact(dpiTitular.OCUPACION) < 0)
                            CmbIngresoManual(cmbOcupaciones, true, dpiTitular.OCUPACION);
                        else
                            cmbOcupaciones.Text = dpiTitular.OCUPACION;

                        cmbTiposDocumento.Text = "DPI";
                        txtNumeroSerie.Text = dpiTitular.SERIE_NUMERO;

                        if (dpiTitular.PAIS_NACIMIENTO != "GUATEMALA")
                        {
                            if (cmbPaisNacimiento.FindStringExact(dpiTitular.PAIS_NACIMIENTO) < 0)
                                CmbIngresoManual(cmbPaisNacimiento, true, dpiTitular.PAIS_NACIMIENTO);
                            else
                                cmbPaisNacimiento.Text = dpiTitular.PAIS_NACIMIENTO;

                            cmbPaisNacimiento_SelectionChangeCommitted(sender, e);

                            if (dpiTitular.DEPARTAMENTO_NACIMIENTO != null && dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim() != "" && dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim() != string.Empty)
                            {
                                txtDepartamentoNacimiento.Text = dpiTitular.DEPARTAMENTO_NACIMIENTO.Trim();
                                if (dpiTitular.MUNICIPIO_NACIMIENTO != "" || dpiTitular.MUNICIPIO_NACIMIENTO != string.Empty)
                                    txtDepartamentoNacimiento.Text += ", " + dpiTitular.MUNICIPIO_NACIMIENTO;

                                //txtDepartamentoNacimiento.Enabled = false;
                                //SE HACE ESTE CAMBIO PQ ALGUNOS VALORES SON "," DE ESTA MANERA QUEDA A CRITERIO DEL CAPTURADOR
                                txtDepartamentoNacimiento.Enabled = true;
                            }
                            else
                            {
                                txtDepartamentoNacimiento.Text = string.Empty;
                                txtDepartamentoNacimiento.Enabled = true;
                            }
                        }
                        else
                        {

                            //cmbPaisNacimiento.SelectedIndex = cmbPaisNacimiento.FindStringExact(dpiTitular.PAIS_NACIMIENTO);
                            cmbPaisNacimiento.Text = dpiTitular.PAIS_NACIMIENTO;

                            if (cmbDeptoNacimiento.FindStringExact(dpiTitular.DEPARTAMENTO_NACIMIENTO) < 0)
                            {
                                CmbIngresoManual(cmbDeptoNacimiento, true, dpiTitular.DEPARTAMENTO_NACIMIENTO);

                                CmbIngresoManual(cmbMunicNacimiento, true, dpiTitular.MUNICIPIO_NACIMIENTO);
                            }
                            else
                            {
                                cmbDeptoNacimiento.Text = dpiTitular.DEPARTAMENTO_NACIMIENTO;
                                CargarCmbMunicipios(cmbMunicNacimiento, false, cmbDeptoNacimiento.SelectedValue.ToString());

                                if (cmbMunicNacimiento.FindStringExact(dpiTitular.MUNICIPIO_NACIMIENTO) < 0)
                                    CmbIngresoManual(cmbMunicNacimiento, true, dpiTitular.MUNICIPIO_NACIMIENTO);
                                else
                                    cmbMunicNacimiento.Text = dpiTitular.MUNICIPIO_NACIMIENTO;
                            }
                        }

                        cmbPaisResidencia.Text = sedeEstacion.PAIS;
                        //cmbPaisResidencia_SelectionChangeCommitted(sender, e);

                        if (dpiTitular.DEPARTAMENTO_VECINDAD.Trim().Equals(string.Empty) || dpiTitular.DEPARTAMENTO_VECINDAD.Trim().Equals(""))
                        {
                            cmbDeptoResidencia.SelectedIndex = -1;
                            cmbMunicResidencia.SelectedIndex = -1;
                        }
                        else
                        {
                            try
                            {
                                cmbDeptoResidencia.Text = dpiTitular.DEPARTAMENTO_VECINDAD;
                                CargarCmbMunicipios(cmbMunicResidencia, false, cmbDeptoResidencia.SelectedValue.ToString());
                                cmbMunicResidencia.Text = dpiTitular.MUNICIPIO_VECINDAD;
                            }
                            catch
                            {
                                cmbDeptoResidencia.SelectedIndex = -1;
                                cmbMunicResidencia.SelectedIndex = -1;

                                CargarCmbDepartamentos(cmbDeptoResidencia, false, "-1");
                                cmbMunicResidencia.Items.Clear();
                            }
                        }

                        string departamentoVecindad = string.Empty;

                        if (dpiTitular.DEPARTAMENTO_CEDULA.Trim().Equals("") || dpiTitular.DEPARTAMENTO_CEDULA.Trim().Equals(string.Empty))
                            departamentoVecindad = funciones.DepartamentoDesdeCedula(dpiTitular.CEDULA_VECINDAD);
                        else
                            departamentoVecindad = dpiTitular.DEPARTAMENTO_VECINDAD;

                        txtApellidoPadre.Text = txtPrimerApellido.Text;
                        txtApellidoMadre.Text = txtSegundoApellido.Text;
                        pbxFotoDPITitular.Image = dpiTitular.IMAGE;
                        pbxFotoDPITitular.Image.Tag = "FotoTitular";

                        BloquearControles("MOC");
                        MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);

                        txtNoRecibo.Text = txtNoRecibo.Text.Trim();
                        if (txtNoRecibo.Text.Equals("") == false && txtNoRecibo.Text.Equals(string.Empty) == false)
                            txtNoRecibo_KeyPress(sender, new KeyPressEventArgs((char)13));

                        pbxDPI.Image = pbxCheck.Image;

                        pbxValidacionWsRenap.Image = pbxLoadColor.Image;
                        pbxAlertas.Image = pbxLoad.Image;

                        vValidacionWsRenap = new ValidacionWsRenap();
                        await vValidacionWsRenap.DatosDpi(txtCui.Text, pbxFotoDPITitular.Image, txtPrimerNombre.Text, txtSegundoNombre.Text, txtTercerNombre.Text, txtPrimerApellido.Text, txtSegundoApellido.Text, txtApellidoCasada.Text, dtpFechaNacimiento.Text, cmbGenero.Text, txtNombrePadre.Text + " " + txtApellidoPadre.Text, txtNombreMadre.Text + " " + txtApellidoMadre.Text, cmbEstadoCivil.Text);

                        if (bool.Parse(vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        {
                            MessageBox.Show("Error al consultar el Servicio Web de RENAP: " + vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                            pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                        }
                        else
                        {
                            if (vValidacionWsRenap.txtPadreRENAP.Text.Split(',').Length == 2)
                            {
                                txtNombrePadre.Text = vValidacionWsRenap.txtPadreDPI.Text = vValidacionWsRenap.txtPadreRENAP.Text.Split(',')[0].Trim();
                                txtApellidoPadre.Text = vValidacionWsRenap.txtPadreDPI.Text = vValidacionWsRenap.txtPadreRENAP.Text.Split(',')[1].Trim();
                                txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = false;
                            }
                            else
                                txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = true;

                            if (vValidacionWsRenap.txtMadreRENAP.Text.Split(',').Length == 2)
                            {
                                txtNombreMadre.Text = vValidacionWsRenap.txtMadreDPI.Text = vValidacionWsRenap.txtMadreRENAP.Text.Split(',')[0].Trim();
                                txtApellidoMadre.Text = vValidacionWsRenap.txtMadreDPI.Text = vValidacionWsRenap.txtMadreRENAP.Text.Split(',')[1].Trim();
                                txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = false;
                            }
                            else
                                txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = true;

                            pbxValidacionWsRenap.Image = pbxCheckColor.Image;
                        }

                        await Alertas(dpiTitular, pbxAlertas, "TITULAR");
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPI_Click(). " + ex.Message;
                MessageBox.Show("pbxDPI_Click(). " + ex.Message);
                pbxDPI.Image = pbxWarning.Image;
            }
            //panel_inferior.Enabled = picb_cerrar.Enabled = true;
            //tabHuellas.Enabled = true;
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

                        cmbTipoPasaporte.Enabled = false;

                        //panel_inferior.Enabled = picb_cerrar.Enabled = false;                                     

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

        private async void pbxDPIPadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHMadre.Image.Tag.Equals("Loading") || pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la madre!");

                if (pbxMOCHPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPIPadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPIPadre.Image = pbxWarning.Image;
                        pbxDPIPadre.Image = pbxLoad.Image;

                        dpiPadre = await LeerDPIAsync();

                        if (dpiPadre.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiPadre.MENSAJE_ERROR);

                        if (dpiPadre.SEXO.Contains("MASCULINO") == false && dpiPadre.SEXO.Equals("M") == false)
                            throw new Exception("¡El género del padre debe ser MASCULINO!");

                        cmbTipoIdPadre.Text = "DPI";
                        cmbTipoIdPadre_SelectionChangeCommitted(sender, e);
                        cmbTipoIdPadre.Enabled = txtNumeroIdPadre.Enabled = false;
                        txtNumeroIdPadre.Text = dpiPadre.CUI;
                        chkDesconocido.Checked = false;

                        dpiPadre.PARENTESCO = "Padre";
                        lblCuiPadre.Text = dpiPadre.CUI;
                        txtNombrePadre.Text = (dpiPadre.PRIMER_NOMBRE + ((dpiPadre.SEGUNDO_NOMBRE != null && dpiPadre.SEGUNDO_NOMBRE != "" && dpiPadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiPadre.SEGUNDO_NOMBRE : "") + ((dpiPadre.TERCER_NOMBRE != null && dpiPadre.TERCER_NOMBRE != "" && dpiPadre.TERCER_NOMBRE != string.Empty) ? " " + dpiPadre.TERCER_NOMBRE : "")).ToUpper();
                        txtApellidoPadre.Text = (dpiPadre.PRIMER_APELLIDO + ((dpiPadre.SEGUNDO_APELLIDO != null && dpiPadre.SEGUNDO_APELLIDO != "" && dpiPadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiPadre.SEGUNDO_APELLIDO : "")).ToUpper();
                        txtNombrePadre.Enabled = txtApellidoPadre.Enabled = chkDesconocido.Enabled = false;

                        pbxFotoDPIPadre.Image = dpiPadre.IMAGE;
                        pbxFotoDPIPadre.Image.Tag = "FotoPadre";

                        MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);
                        pbxDPIPadre.Image = pbxCheck.Image;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPIPadre_Click(). " + ex.Message;
                MessageBox.Show("pbxDPIPadre_Click(). " + ex.Message);
                pbxDPIPadre.Image = pbxWarning.Image;
            }
        }

        private async void pbxMOCHPadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHMadre.Image.Tag.Equals("Loading") || pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la madre!");

                if (pbxDPIPadre.Image.Tag.Equals("Warning"))
                    throw new Exception("¡Primero lea la información del DPI!");

                if (pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxMOCHPadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCPadre >= (parametrizacion.INTENTOS_MOC_PADRE + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_PADRE + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCHPadre.Image = pbxWarning.Image;
                        tabHuellas.Enabled = false;

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + intentosMOCPadre + "/" + parametrizacion.INTENTOS_MOC_PADRE.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHPadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Force();

                        DataSet dsMOC = await MOC_DPI(lblCuiPadre.Text.Trim());
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCPadre + "/" + parametrizacion.INTENTOS_MOC_PADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCPadre--;
                            pbxMOCHPadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCPadre) + "/" + parametrizacion.INTENTOS_MOC_PADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCHPadre.Image = pbxWarning.Image;
                        }
                        intentosMOCPadre++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCHPadre_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCHPadre_Click(). " + ex.Message);
                pbxMOCHPadre.Image = pbxWarning.Image;
            }
            tabHuellas.Enabled = true;
        }

        private async void pbxDPIMadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHPadre.Image.Tag.Equals("Loading") || pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la padre!");

                if (pbxMOCHMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxDPIMadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de lectura ya se encuentra en proceso!");
                else
                {
                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxDPIMadre.Image = pbxWarning.Image;
                        pbxDPIMadre.Image = pbxLoad.Image;

                        dpiMadre = await LeerDPIAsync();

                        if (dpiMadre.INFORMACION_DPI_LEIDA == false)
                            throw new Exception(dpiMadre.MENSAJE_ERROR);

                        if (dpiMadre.SEXO.Contains("FEMENINO") == false && dpiMadre.SEXO.Equals("F") == false)
                            throw new Exception("¡El género de la madre debe ser FEMENINO!");

                        cmbTipoIdMadre.Text = "DPI";
                        cmbTipoIdMadre_SelectionChangeCommitted(sender, e);
                        cmbTipoIdMadre.Enabled = txtNumeroIdMadre.Enabled = false;
                        txtNumeroIdMadre.Text = dpiMadre.CUI;
                        chkDesconocida.Checked = false;

                        dpiMadre.PARENTESCO = "Madre";
                        lblCuiMadre.Text = dpiMadre.CUI;
                        txtNombreMadre.Text = (dpiMadre.PRIMER_NOMBRE + ((dpiMadre.SEGUNDO_NOMBRE != null && dpiMadre.SEGUNDO_NOMBRE != "" && dpiMadre.SEGUNDO_NOMBRE != string.Empty) ? " " + dpiMadre.SEGUNDO_NOMBRE : "") + ((dpiMadre.TERCER_NOMBRE != null && dpiMadre.TERCER_NOMBRE != "" && dpiMadre.TERCER_NOMBRE != string.Empty) ? " " + dpiMadre.TERCER_NOMBRE : "")).ToUpper();
                        txtApellidoMadre.Text = (dpiMadre.PRIMER_APELLIDO + ((dpiMadre.SEGUNDO_APELLIDO != null && dpiMadre.SEGUNDO_APELLIDO != "" && dpiMadre.SEGUNDO_APELLIDO != string.Empty) ? " " + dpiMadre.SEGUNDO_APELLIDO : "")).ToUpper();
                        txtNombreMadre.Enabled = txtApellidoMadre.Enabled = chkDesconocida.Enabled = false;

                        pbxFotoDPIMadre.Image = dpiMadre.IMAGE;
                        pbxFotoDPIMadre.Image.Tag = "FotoMadre";

                        MessageBox.Show("¡Información leída correctamente!", "Lectura DPI", MessageBoxButtons.OK);
                        pbxDPIMadre.Image = pbxCheck.Image;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxDPIMadre_Click(). " + ex.Message;
                MessageBox.Show("pbxDPIMadre_Click(). " + ex.Message);
                pbxDPIMadre.Image = pbxWarning.Image;
            }
        }

        private async void pbxMOCHMadre_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbxMOCHPadre.Image.Tag.Equals("Loading") || pbxDPIPadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI de la padre!");

                if (pbxDPIMadre.Image.Tag.Equals("Warning"))
                    throw new Exception("¡Primero lea la información del DPI!");

                if (pbxDPIMadre.Image.Tag.Equals("Loading"))
                    throw new Exception("¡Espere que finalice la lectura del DPI!");

                if (pbxMOCHMadre.Image.Tag.Equals("Loading"))
                    MessageBox.Show("¡La operación de MOC ya se encuentra en proceso!");
                else
                {
                    if (intentosMOCMadre >= (parametrizacion.INTENTOS_MOC_MADRE + 1))
                        throw new Exception("¡Límite de intentos máximo (" + parametrizacion.INTENTOS_MOC_MADRE + ")alcanzado!");

                    DialogResult result = MessageBox.Show("¡Inserte la tarjeta en el lector y espere a que se realice la lectura!", "Lectura DPI", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        pbxMOCHMadre.Image = pbxWarning.Image;
                        tabHuellas.Enabled = false;

                        MessageBox.Show("¡Coloque el dedo en el sensor y espere hasta que la validación finalice! (" + intentosMOCMadre + "/" + parametrizacion.INTENTOS_MOC_MADRE.ToString() + "), No retire el DPI", "Match on Card", MessageBoxButtons.OK);

                        pbxMOCHMadre.Image = pbxLoad.Image;

                        _biometricFingerClient.Force();

                        DataSet dsMOC = await MOC_DPI(lblCuiMadre.Text);
                        bool MOC = bool.Parse(dsMOC.Tables[0].Rows[0]["RESULTADO"].ToString());
                        string msgError = dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString();

                        if (MOC == false && msgError.Equals(string.Empty) == false)
                            throw new Exception(dsMOC.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                        if (MOC)
                        {
                            MessageBox.Show("¡MOC exitoso! (" + intentosMOCMadre + "/" + parametrizacion.INTENTOS_MOC_MADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            intentosMOCMadre--;
                            pbxMOCHMadre.Image = pbxCheck.Image;
                        }
                        else
                        {
                            MessageBox.Show("¡Las huellas no coinciden! (" + (intentosMOCMadre) + "/" + parametrizacion.INTENTOS_MOC_MADRE + ")", "Match on Card", MessageBoxButtons.OK);
                            pbxMOCHMadre.Image = pbxWarning.Image;
                        }
                        intentosMOCMadre++;
                    }
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxMOCHMadre_Click(). " + ex.Message;
                MessageBox.Show("pbxMOCHMadre_Click(). " + ex.Message);
                pbxMOCHMadre.Image = pbxWarning.Image;
            }
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

        private void btnResumen_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaReporteResumen = Application.StartupPath + "\\ENROL\\ReporteCasos\\" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                if (!File.Exists(rutaReporteResumen))
                    throw new Exception("¡No existen casos! " + rutaReporteResumen);

                VisorReportes visorReportes = new VisorReportes();

                DataTable dt = new DataTable();

                DataSet dsEncabezado = new DataSet();
                dsEncabezado.Tables.Add();
                dsEncabezado.Tables[0].Columns.Add("sede", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("usuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("fecha", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("hora_primer_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("numero_primer_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("hora_ultimo_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("numero_ultimo_caso", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("total", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("horas", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("promedio", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("nombreusuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("estacioncaptura", typeof(string));

                DataRow dr = dsEncabezado.Tables[0].NewRow();
                dr["sede"] = sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                dr["nombreusuario"] = lblNombreUsuario.Text;
                dr["fecha"] = dtpFechaNacimiento.Value.ToShortDateString();// DateTime.Now.ToShortDateString();
                dr["estacioncaptura"] = Environment.MachineName;

                DataSet dsTemp = new DataSet();
                DataSet dsDetalle = new DataSet();
                dsTemp.ReadXml(rutaReporteResumen);
                try
                {
                    dsDetalle.Tables.Add(dsTemp.Tables[0].Select(" usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy") + "'").CopyToDataTable());
                    //dsDetalle.Tables.Add(dsTemp.Tables[0].Copy());
                }
                catch
                {
                    throw new Exception("¡No se encontraron registros! " + " usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy") + "'");
                }
                
                dr["hora_primer_caso"] = dsDetalle.Tables[0].Rows[0]["hora"].ToString();
                dr["numero_primer_caso"] = dsDetalle.Tables[0].Rows[0]["nocaso"].ToString();
                dr["hora_ultimo_caso"] = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["hora"].ToString();
                dr["numero_ultimo_caso"] = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["nocaso"].ToString();
                dr["total"] = dsDetalle.Tables[0].Rows.Count;

                string s = dsDetalle.Tables[0].Rows[0]["fechacaptura"].ToString() + " " + dsDetalle.Tables[0].Rows[0]["hora"].ToString();
                DateTime hIni = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                s = dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["fechacaptura"].ToString() + " " + dsDetalle.Tables[0].Rows[dsDetalle.Tables[0].Rows.Count - 1]["hora"].ToString();
                DateTime hFin = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                TimeSpan timeSpan = hFin - hIni;
                double totalHours = timeSpan.TotalHours;
                if (totalHours < 1)
                    totalHours = 1;

                dr["horas"] = Math.Round(totalHours, 2).ToString();

                double promedio = double.Parse(dsDetalle.Tables[0].Rows.Count.ToString()) / Math.Round(totalHours, 2);
                dr["promedio"] = Math.Round(promedio, 2).ToString();

                dsEncabezado.Tables[0].Rows.Add(dr);

                ReportDataSource RD = new ReportDataSource();
                RD.Value = dsEncabezado.Tables[0];
                RD.Name = "Encabezado";

                visorReportes.reportViewer1.LocalReport.DataSources.Clear();
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD);
                visorReportes.reportViewer1.LocalReport.ReportEmbeddedResource = "ENROLLMENT_V3.Reportes.rpt_resumen.rdlc";
                //visorReportes.reportViewer1.LocalReport.ReportPath = Environment.CurrentDirectory + "\\Reportes\\rpt_detalle.rdlc";
                visorReportes.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                visorReportes.reportViewer1.LocalReport.Refresh();

                visorReportes.Activate();
                visorReportes.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "btnResumen_Click()");
                txtMensaje.Text = "btnResumen_Click(). " + ex.Message;
            }
        }

        private void btnDetalle_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaReporteResumen = Application.StartupPath + "\\ENROL\\ReporteCasos\\" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                if (!File.Exists(rutaReporteResumen))
                    throw new Exception("¡No existen casos! " + rutaReporteResumen);

                VisorReportes visorReportes = new VisorReportes();

                DataTable dt = new DataTable();

                DataSet dsEncabezado = new DataSet();
                dsEncabezado.Tables.Add();
                dsEncabezado.Tables[0].Columns.Add("sede", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("nombreusuario", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("fecha", typeof(string));
                dsEncabezado.Tables[0].Columns.Add("estacioncaptura", typeof(string));

                DataRow dr = dsEncabezado.Tables[0].NewRow();
                dr["sede"] = sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                dr["nombreusuario"] = lblNombreUsuario.Text;
                dr["fecha"] = dtpFechaNacimiento.Value.ToShortDateString();// DateTime.Now.ToShortDateString();
                dr["estacioncaptura"] = Environment.MachineName;

                dsEncabezado.Tables[0].Rows.Add(dr);

                ReportDataSource RD = new ReportDataSource();
                RD.Value = dsEncabezado.Tables[0];
                RD.Name = "Encabezado";

                dsEncabezado = new DataSet();
                DataSet dsTemp = new DataSet();
                dsTemp.ReadXml(rutaReporteResumen);
                try
                {
                    dsEncabezado.Tables.Add(dsTemp.Tables[0].Select(" usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy") + "'").CopyToDataTable());
                    //dsEncabezado.Tables.Add(dsTemp.Tables[0].Copy());
                }
                catch
                {
                    throw new Exception("¡No se encontraron registros! " + " usuario = '" + lbl_usuario.Text + "' AND sedecaptura = '" + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS + "' AND fechacaptura = '" + dtpFechaNacimiento.Value.ToString("dd/MM/yyyy") + "'");
                }

                ReportDataSource RD2 = new ReportDataSource();
                RD2.Value = dsEncabezado.Tables[0];
                RD2.Name = "Detalles";
                visorReportes.reportViewer1.LocalReport.DataSources.Clear();
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD);
                visorReportes.reportViewer1.LocalReport.DataSources.Add(RD2);
                visorReportes.reportViewer1.LocalReport.ReportEmbeddedResource = "ENROLLMENT_V3.Reportes.rpt_detalle.rdlc";
                //visorReportes.reportViewer1.LocalReport.ReportPath = Environment.CurrentDirectory + "\\Reportes\\rpt_detalle.rdlc";
                visorReportes.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
                visorReportes.reportViewer1.LocalReport.Refresh();

                visorReportes.Activate();
                visorReportes.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "btnDetalle_Click()");
                txtMensaje.Text = "btnDetalle_Click(). " + ex.Message;
            }
        }

        //private void pbxVistaPrevia_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (pbxVistaPrevia.Image == null)
        //            throw new Exception("¡Capture una imagen!");

        //        ProcesarImagenRostro();
                
        //        pbxVistaPrevia.Image.Dispose();
        //        pbxVistaPrevia.Dispose();
        //        System.GC.Collect();
        //        pbxVistaPrevia = new PictureBox();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "pbxVistaPrevia_Click()");
        //        txtMensaje.Text = "pbxVistaPrevia_Click(). " + ex.Message;
        //    }
        //}

        private void cmbDeptoResidencia_TextChanged(object sender, EventArgs e)
        {
        }

        private void cmbEstadoResidencia_TextChanged(object sender, EventArgs e)
        {
        }

        private void cmbZipCodeResidencia_TextChanged(object sender, EventArgs e)
        {
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
            
        }

        private async void pbxValidacionWsRenap_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTiposDocumento.Text.ToUpper().Contains("DPI") || cmbTiposDocumento.Text.ToUpper().Contains("CUI"))
                {
                    //if (txtPrimerNombre.Enabled)
                    {
                        pbxValidacionWsRenap.Image = pbxWarningColor.Image;

                        bool vCamposValidos = ((txtCui.Text.Trim().Equals(string.Empty) == false || txtCui.Text.Trim().Equals("") == false) && (txtPrimerNombre.Text.Trim().Equals(string.Empty) == false || txtPrimerNombre.Text.Trim().Equals("") == false) && (txtPrimerApellido.Text.Trim().Equals(string.Empty) == false || txtPrimerApellido.Text.Trim().Equals("") == false) && (dtpFechaNacimiento.Text.Trim().Equals(string.Empty) == false || dtpFechaNacimiento.Text.Trim().Equals("") == false));

                        if (vCamposValidos)
                        {
                            pbxValidacionWsRenap.Image = pbxLoadColor.Image;

                            vValidacionWsRenap = new ValidacionWsRenap();
                            await vValidacionWsRenap.DatosDpi(txtCui.Text, pbxFotoDPITitular.Image, txtPrimerNombre.Text, txtSegundoNombre.Text, txtTercerNombre.Text, txtPrimerApellido.Text, txtSegundoApellido.Text, txtApellidoCasada.Text, dtpFechaNacimiento.Text, cmbGenero.Text, txtNombrePadre.Text + ", " + txtApellidoPadre.Text, txtNombreMadre.Text + ", " + txtApellidoMadre.Text, cmbEstadoCivil.Text);

                            if (bool.Parse(vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["RESULTADO"].ToString()))
                                pbxValidacionWsRenap.Image = pbxCheckColor.Image;
                            else
                            {
                                pbxValidacionWsRenap.Image = pbxWarningColor.Image;
                                MessageBox.Show("Error al consultar el Servicio Web de RENAP: " + vValidacionWsRenap.dsValidacion.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                            }
                        }
                    }

                    if (vValidacionWsRenap != null)
                        vValidacionWsRenap.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxValidacionWsRenap_Click(). " + ex.Message;
                MessageBox.Show("pbxValidacionWsRenap_Click(). " + ex.Message);
            }
        }

        private void txtNoRecibo_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoPasaporte.Text.Equals("") == false && cmbTipoPasaporte.Text.Equals(string.Empty) == false)
                {
                    txtNoRecibo.Text = txtNoRecibo.Text.Trim();
                    txtNoRecibo.BackColor = (txtNoRecibo.Text.Equals("") || txtNoRecibo.Text.Equals(string.Empty)) ? Color.Yellow : Color.White;

                    if (txtNoRecibo.BackColor == Color.White)
                        txtNoRecibo_KeyPress(sender, new KeyPressEventArgs((char)13));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtNoRecibo_Leave(). " + ex.Message);
                txtMensaje.Text = "txtNoRecibo_Leave(). " + ex.Message;
            }
        }

        private void txtObservacionesIcao_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoPasaporte.Text.Equals("") == false && cmbTipoPasaporte.Text.Equals(string.Empty) == false)
                {
                    txtObservacionesIcao.BackColor = (txtObservacionesIcao.Text.Trim().Equals("") || txtObservacionesIcao.Text.Trim().Equals(string.Empty)) ? Color.Yellow : Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("txtObservacionesIcao_TextChanged(). " + ex.Message);
                txtMensaje.Text = "txtObservacionesIcao_TextChanged(). " + ex.Message;
            }
        }

        private void lblVistaPreviaMin_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblVistaPreviaMin.Image.Tag == pbxUsuario.Image.Tag)
                    throw new Exception("¡Capture una imagen!");

                System.GC.Collect();
                lblVistaPreviaMin.Image = pbxUsuario.Image;
                lblVistaPreviaMin.Refresh();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                lblProcesarRostro.Text = string.Empty;

                //faceView2.Face = null;
                pbxRostroIcao.Image = null;

                //PROCESAMIENTO DE ROSTROS
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length <= 0)
                    InvocarProcesamientoRostro();
                ActivarProcesamientoRostro(Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG");

                //LA APLICACION DE PROCESAMIENTOX64 ENVIARÁ UN MENSAJE QUE ESCRIBIRÁ DONE EN lblProcesarRostro
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "lblVistaPrevia_Click()");
                txtMensaje.Text = "lblVistaPrevia_Click(). " + ex.Message;
            }
        }

        private void cmbGenero_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                if (cmbGenero.Text.Equals("MASCULINO"))
                {
                    txtApellidoCasada.Text = string.Empty;
                    txtApellidoCasada.Enabled = false;
                }
                else
                    txtApellidoCasada.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("cmbGenero_SelectionChangeCommitted(). " + ex.Message);
                txtMensaje.Text = "cmbGenero_SelectionChangeCommitted(). " + ex.Message;
            }
        }

        private void btnSedeDireccion_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnSedeDireccion.Text.Equals("Dirección de envío"))
                {
                    btnSedeDireccion.Text = "Sede de Entrega";
                    grpSedeEntrega.Text = "Dirección de envío";

                    ////ESTADOS UNIDOS
                    //if(cmbPaisResidencia.SelectedValue.Equals("840") || cmbPaisResidencia.SelectedValue.Equals("320"))
                    //    ControlesSedeResidenciaEntrega(true);

                    //else //RESTO DEL MUNDO
                    //    ControlesSedeResidenciaEntrega(false);

                    ControlesSedeResidenciaEntrega(true);

                    if (cmbPaisResidencia.SelectedValue.Equals("840") == false)
                        btnCopiarDireccion.Visible = false;

                    CargarCmbEstados(cmbEstadoEntrega, false, "0");

                    cmbZipCodeEntrega.Enabled = cmbCiudadEntrega.Enabled = false;
                    CargarCmbZipCodes(cmbZipCodeEntrega, false, "-1");
                    CargarCmbCiudadesZipCode(cmbCiudadEntrega, false, "-1");
                    //lblPaisSedeEntrega.Visible = lblCiudadSedeEntrega.Visible = cmbPaisSedeEntrega.Visible = cmbCiudadSedeEntrega.Visible = false;
                }
                else
                {
                    btnSedeDireccion.Text = "Dirección de envío";
                    grpSedeEntrega.Text = "Sede de Entrega";

                    ControlesSedeResidenciaEntrega(false);
                    //btnCopiarDireccion.Visible = false;
                    //lblPaisSedeEntrega.Visible = lblCiudadSedeEntrega.Visible = cmbPaisSedeEntrega.Visible = cmbCiudadSedeEntrega.Visible = true;

                    if (cmbPaisSedeEntrega.Items.Count <= 0)
                        CargarCmbPaisesSedeEntrega(cmbPaisSedeEntrega, false, "");
                    
                    cmbPaisSedeEntrega.SelectedValue = sedeEstacion.PAIS;
                    cmbPaisSedeEntrega_SelectionChangeCommitted(new object(), new EventArgs());
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnSedeDireccion_Click(). " + ex.Message;
                MessageBox.Show("btnSedeDireccion_Click(). " + ex.Message);
            }
        }

        private void LblComprimir_TextChanged(object sender, EventArgs e)
        {
            if (lblComprimir.Text.Equals(string.Empty) == false)
            {
                
            }
        }

        private void LblProcesarRostro_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblProcesarRostro.Text.Equals("ProcesarRostro_Done"))
                {
                    MostrarImagenSegmentada();
                }
                else if (lblProcesarRostro.Text.Equals("ProcesarRostro_Undone"))
                {
                }
                
                chkVistaEnVivo.Enabled = true;
                btnCapturarRostro.Enabled = true;
                //else if(lblProcesarRostro.Text.Equals("ProcesarRostro_Undone"))
                //{
                //    string rutaImagenProcesar = Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG";

                //    using (Bitmap bmp = LoadBitmapUnlocked(rutaImagenProcesar))
                //    {
                //        NFace face = new NFace
                //        {
                //            Image = NImage.FromBitmap((Bitmap)(bmp.Clone())),
                //            CaptureOptions = NBiometricCaptureOptions.Stream
                //        };

                //        bmp.Dispose();

                //        System.GC.Collect();
                //        System.GC.WaitForPendingFinalizers();

                //        NSubject _newSubject = new NSubject();
                //        _newSubject.Faces.Add(face);
                //        faceView2.Face = _newSubject.Faces.First();
                //        icaoWarningView1.Face = _newSubject.Faces.First();
                //    }
                //}
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "LblProcesarRostro_TextChanged(). " + ex.Message;
                MessageBox.Show("LblProcesarRostro_TextChanged(). " + ex.Message);
            }
            lblProcesarRostro.Text = string.Empty;
        }

        private async void pbxAlertas_Click(object sender, EventArgs e)
        {
            try
            {
                if (visorAlertas != null && dpiTitular != null)
                {
                    await Alertas(dpiTitular, pbxAlertas, "TITULAR");
                    visorAlertas.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "pbxAlertas_Click(). " + ex.Message;
                MessageBox.Show("pbxAlertas_Click(). " + ex.Message);
            }
        }

        private void Label31_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("A partir de la versión 27 se tienen los siguientes cambios: Parametrización del servidor FTP, modalidad de llave en disco. Utilización de utiliría copilada en .NET Core (YA NO JEJE). En la versión 28 (22/08/2019) se agregó: Actualización del MM SDK a versión 11, parametrización para Match de dactilar y facial en servidor remoto, se agregó emisión para caso especial DIPLOMATICO Art. 98 Cod. Migración. En la versión 29, se amplió la cantidad de memmoria que puede manejar la aplicación de 32 bits y se cambió el control IcaoWarningView a una PictureBox. En la versión 30 se parametrizó el nombre de la carpeta FTP.");
        }

        private void enrollment_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void CmbPaisNacimiento_Leave(object sender, EventArgs e)
        {
            try
            {
                
                if (cmbPaisNacimiento.Text.Trim().Equals("") == false)
                {
                    int i = cmbPaisNacimiento.FindStringExact(cmbPaisNacimiento.Text.ToUpper());
                    cmbPaisNacimiento.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //AGREGO ES CODIGO PARA REASIGNE EL DEPARTAMENTO Y MUNICIPIO ELEGIDOS SI ESTOS EXISTEN
                        string strDepto = cmbDeptoNacimiento.Text;
                        string strMunic = cmbMunicNacimiento.Text;

                        int intDepto = cmbDeptoNacimiento.FindStringExact(strDepto);
                        int intMunic = cmbMunicNacimiento.FindStringExact(strMunic);

                        if (intDepto < 0 && intMunic < 0)
                            cmbPaisNacimiento_SelectionChangeCommitted(sender, e);

                        //if (intDepto >= 0)
                        //{
                        //    cmbDeptoNacimiento.Text = strDepto;
                        //    CmbDeptoNacimiento_Leave(sender, e);
                        //}
                            
                        //if (intMunic >= 0)
                        //{
                        //    cmbMunicNacimiento.Text = strMunic;
                        //    CmbMunicNacimiento_Leave(sender, e);
                        //}
                            
                    }
                    else
                    {
                        cmbPaisNacimiento.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbDeptoNacimiento_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbDeptoNacimiento.Text.Trim().Equals("") == false)
                {
                    int i = cmbDeptoNacimiento.FindStringExact(cmbDeptoNacimiento.Text.ToUpper());
                    cmbDeptoNacimiento.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        string strMunic = cmbMunicNacimiento.Text;
                        cmbDeptoNacimiento_SelectionChangeCommitted(sender, e);

                        int intMunic = cmbMunicNacimiento.FindStringExact(strMunic);
                        if (intMunic >= 0)
                        {
                            cmbMunicNacimiento.Text = strMunic;
                        }
                    }
                    else
                    {
                        cmbDeptoNacimiento.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbMunicNacimiento_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbMunicNacimiento.Text.Trim().Equals("") == false)
                {
                    int i = cmbMunicNacimiento.FindStringExact(cmbMunicNacimiento.Text.ToUpper());
                    cmbMunicNacimiento.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbDeptoNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbMunicNacimiento.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbNacionalidad_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbNacionalidad.Text.Trim().Equals("") == false)
                {
                    int i = cmbNacionalidad.FindStringExact(cmbNacionalidad.Text.ToUpper());
                    cmbNacionalidad.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbDeptoNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbNacionalidad.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbTipoTramite_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoTramite.Text.Trim().Equals("") == false)
                {
                    int i = cmbTipoTramite.FindStringExact(cmbTipoTramite.Text.ToUpper());
                    cmbTipoTramite.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbDeptoNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbTipoTramite.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbGenero_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbGenero.Text.Trim().Equals("") == false)
                {
                    int i = cmbGenero.FindStringExact(cmbGenero.Text.ToUpper());
                    cmbGenero.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbGenero_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbGenero.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbEstadoCivil_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbEstadoCivil.Text.Trim().Equals("") == false)
                {
                    int i = cmbEstadoCivil.FindStringExact(cmbEstadoCivil.Text.ToUpper());
                    cmbEstadoCivil.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbEstadoCivil_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbEstadoCivil.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbOcupaciones_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbOcupaciones.Text.Trim().Equals("") == false)
                {
                    int i = cmbOcupaciones.FindStringExact(cmbOcupaciones.Text.ToUpper());
                    cmbOcupaciones.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbPaisNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbOcupaciones.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbOjos_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbOjos.Text.Trim().Equals("") == false)
                {
                    int i = cmbOjos.FindStringExact(cmbOjos.Text.ToUpper());
                    cmbOjos.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbPaisNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbOjos.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbTez_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbTez.Text.Trim().Equals("") == false)
                {
                    int i = cmbTez.FindStringExact(cmbTez.Text.ToUpper());
                    cmbTez.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbPaisNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbTez.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbCabello_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbCabello.Text.Trim().Equals("") == false)
                {
                    int i = cmbCabello.FindStringExact(cmbCabello.Text.ToUpper());
                    cmbCabello.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbPaisNacimiento_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbCabello.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbPaisResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbPaisResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbPaisResidencia.FindStringExact(cmbPaisResidencia.Text.ToUpper());
                    cmbPaisResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbPaisResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbPaisResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbDeptoResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbDeptoResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbDeptoResidencia.FindStringExact(cmbDeptoResidencia.Text.ToUpper());
                    cmbDeptoResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbDeptoResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbDeptoResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbEstadoResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbEstadoResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbEstadoResidencia.FindStringExact(cmbEstadoResidencia.Text.ToUpper());
                    cmbEstadoResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbEstadoResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbEstadoResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbMunicResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbMunicResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbMunicResidencia.FindStringExact(cmbMunicResidencia.Text.ToUpper());
                    cmbMunicResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbDeptoResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbMunicResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbZipCodeResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbZipCodeResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbZipCodeResidencia.FindStringExact(cmbZipCodeResidencia.Text.ToUpper());
                    cmbZipCodeResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbZipCodeResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbZipCodeResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbCiudadResidencia_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbCiudadResidencia.Text.Trim().Equals("") == false)
                {
                    int i = cmbCiudadResidencia.FindStringExact(cmbCiudadResidencia.Text.ToUpper());
                    cmbCiudadResidencia.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbZipCodeResidencia_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbCiudadResidencia.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbPaisSedeEntrega_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbPaisSedeEntrega.Text.Trim().Equals("") == false)
                {
                    int i = cmbPaisSedeEntrega.FindStringExact(cmbPaisSedeEntrega.Text.ToUpper());
                    cmbPaisSedeEntrega.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbPaisSedeEntrega_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbPaisSedeEntrega.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbCiudadSedeEntrega_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbCiudadSedeEntrega.Text.Trim().Equals("") == false)
                {
                    int i = cmbCiudadSedeEntrega.FindStringExact(cmbCiudadSedeEntrega.Text.ToUpper());
                    cmbCiudadSedeEntrega.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbCiudadSedeEntrega_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbCiudadSedeEntrega.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbEstadoEntrega_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbEstadoEntrega.Text.Trim().Equals("") == false)
                {
                    int i = cmbEstadoEntrega.FindStringExact(cmbEstadoEntrega.Text.ToUpper());
                    cmbEstadoEntrega.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbEstadoEntrega_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbEstadoEntrega.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbZipCodeEntrega_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbZipCodeEntrega.Text.Trim().Equals("") == false)
                {
                    int i = cmbZipCodeEntrega.FindStringExact(cmbZipCodeEntrega.Text.ToUpper());
                    cmbZipCodeEntrega.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        cmbZipCodeEntrega_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbZipCodeEntrega.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void Pic_logo_dgm_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //ProcesarRostrosNeuroStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void CmbCiudadEntrega_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cmbCiudadEntrega.Text.Trim().Equals("") == false)
                {
                    int i = cmbCiudadEntrega.FindStringExact(cmbCiudadEntrega.Text.ToUpper());
                    cmbCiudadEntrega.SelectedIndex = i >= 0 ? i : -1;

                    if (i >= 0)
                    {
                        //cmbCiudadEntrega_SelectionChangeCommitted(sender, e);
                    }
                    else
                    {
                        cmbCiudadEntrega.SelectedIndex = -1;
                        MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": ¡Valor no encontrado, intente nuevamente!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
            }
        }

        private void LblEncriptar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblEncriptar.Text.Equals("") == false)
                {
                    string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "SftpData", "CasoPasaporte_" + txtNoCaso.Text.Trim() + ".xml");

                    string horaCaptura = DateTime.Now.ToString("HH:mm:ss");
                    string fechaCaptura = DateTime.Now.ToString("dd/MM/yyyy");

                    DateTime fechaN = new DateTime();
                    fechaN = DateTime.Parse(dtpFechaNacimiento.Text);

                    StringReader strReader = new StringReader(lblEncriptar.Text);
                    DataSet dsParamEncript = new DataSet();
                    dsParamEncript.ReadXml(strReader);

                    if (dsParamEncript == null)
                        throw new Exception("¡El dataset es nulo!");

                    if (dsParamEncript.Tables.Count == 0)
                        throw new Exception("¡No existen tablas!");

                    if (dsParamEncript.Tables[0].Rows.Count == 0)
                        throw new Exception("¡No existen filas!");

                    if (bool.Parse(dsParamEncript.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                        throw new Exception(dsParamEncript.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    //if (bool.Parse(dsEncriptarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    //    throw new Exception("¡Error al encriptar el archivo, contacte al administrador del sistema!. " + dsEncriptarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    string rutaArchivoEncriptado = Path.Combine(Application.StartupPath, "ENROL", "XMLs") + "\\" + Path.GetFileNameWithoutExtension(rutaXML) + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt";

                    DataSet dsEnviarArchivo = new DataSet();
                    if (Properties.Settings.Default.ENVIAR_FTP)
                    {
                        //xml.crearXml(rutaXML, pasaporte_xml);

                        dsEnviarArchivo = funciones.EnvioFTPArchivo(rutaArchivoEncriptado);

                        if (bool.Parse(dsEnviarArchivo.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                            MessageBox.Show("¡Error al enviar el archivo, intente después con la opción correspondiente!. " + dsEnviarArchivo.Tables[0].Rows[0]["MSG_ERROR"].ToString());
                    }

                    tmrPasaporte.Enabled = false;

                    int iCorrelativo = 0;
                    try
                    {
                        string rutaReporteResumen = Application.StartupPath + "\\ENROL\\ReporteCasos\\" + DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "") + ".xml";

                        if (!File.Exists(rutaReporteResumen))
                        {
                            XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));

                            //Creamos el nodo raiz y lo añadimos al documento
                            XElement nodoRaiz = new XElement("Casos");
                            document.Add(nodoRaiz);
                            document.Save(rutaReporteResumen);
                            iCorrelativo = 1;
                        }

                        XDocument xmlDoc = XDocument.Load(rutaReporteResumen);

                        if (iCorrelativo == 0)
                            iCorrelativo = int.Parse(xmlDoc.Elements("Casos").Elements("Caso").Last().Element("Correlativo").Value) + 1;

                        xmlDoc.Elements("Casos")
                        .Last().Add(new XElement("Caso", new XElement("Correlativo", iCorrelativo.ToString()), new XElement("Hora", horaCaptura), new XElement("NoCaso", txtNoCaso.Text), new XElement("Nombres", txtPrimerNombre.Text + " " + txtSegundoNombre.Text + " " + txtTercerNombre.Text), new XElement("Apellidos", txtPrimerApellido.Text + " " + txtSegundoApellido.Text), new XElement("FechaNacimiento", fechaN.ToString("dd/MM/yyyy")), new XElement("Usuario", lbl_usuario.Text), new XElement("NombreUsuario", lblNombreUsuario.Text), new XElement("SedeCaptura", sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS), new XElement("FechaCaptura", fechaCaptura), new XElement("EstacionCaptura", Environment.MachineName)));
                        xmlDoc.Save(rutaReporteResumen);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("¡Error a bitacorizar! " + ex.Message);
                    }

                    //LimpiarBandejaProbatorios();

                    btnImprimir_Click(sender, e);

                    ProcesarRostrosNeuroStart();

                    MessageBox.Show("Almacenado con éxito! ");
                    txtMensaje.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "LblEncriptar_TextChanged(). " + ex.Message;
                MessageBox.Show("LblEncriptar_TextChanged(). " + ex.Message);
            }
            lblEncriptar.Text = string.Empty;
        }

        private void LiveViewPicBox_DoubleClick(object sender, EventArgs e)
        {
            //for (int i = 0; i <= 600; i++)
            //    MostrarImagenSegmentada();

            //MessageBox.Show("Ejecución terminada");
        }

        private void PbxFotoDPITitular_Click(object sender, EventArgs e)
        {
            ////NuevaInstanciaEnrollment();
            //try
            //{
            //    Image image = Image.FromFile("FotoDpiTitular.png");
            //    var ms = new MemoryStream();
            //    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            //    //var ms = new MemoryStream();
            //    //pbxFotoDPITitular.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            //    //ImageConverter converter = new ImageConverter();
            //    //byte[] bytes = (byte[]) converter.ConvertTo(pbxFotoDPITitular.Image, typeof(byte[]));

            //    MessageBox.Show("Conversion exitosa");
            //    ms.Dispose();

            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show("Error en la conversión. " + ex.Message);
            //}
        }

        private void PbxRostroIcao_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (chkIcao.Checked == false)
                {
                    if (lblVistaPreviaMin.Image != null)
                    {
                        RecortarImagen recortarImagen = new RecortarImagen(this);
                        //recortarImagen.Parent = this;
                        Image image = Bitmap.FromFile(Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG");
                        recortarImagen.pbxImagen.Image = (Image)(image.Clone());
                        image.Dispose();
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        recortarImagen.ShowDialog();
                        FotoCumpleIcao = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("PbxRostroIcao_DoubleClick(). " + ex.Message);
                txtMensaje.Text = "PbxRostroIcao_DoubleClick(). " + ex.Message;
            }
        }

        private void picb_logo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(txtMensaje.Text);
        }

        private void ControlesSedeResidenciaEntrega(bool valor)
        {
            btnCopiarDireccion.Visible = valor;

            txtResidenciaEntrega1.Visible = valor;
            txtResidenciaEntrega2.Visible = valor;
            lblEstadoEntrega.Visible = valor;
            cmbEstadoEntrega.Visible = valor;
            lblZipCodeEntrega.Visible = valor;
            cmbZipCodeEntrega.Visible = valor;
            cmbEstadoEntrega.Visible = valor;
            lblEstadoEntrega.Visible = valor;
            lblCiudadEntrega.Visible = valor;
            cmbCiudadEntrega.Visible = valor;

            lblPaisSedeEntrega.Visible = lblCiudadSedeEntrega.Visible = cmbPaisSedeEntrega.Visible = cmbCiudadSedeEntrega.Visible = !valor;
        }

        private void cmbPaisSedeEntrega_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtResidenciaEntrega1.Text = txtResidenciaEntrega2.Text = string.Empty;
                CargarCmbEstados(cmbEstadoEntrega, false, "-1");
                CargarCmbZipCodes(cmbZipCodeEntrega, false, "-1");
                CargarCmbCiudadesZipCode(cmbCiudadEntrega, false, "-1");
                
                if (cmbPaisSedeEntrega.SelectedValue != null)
                {
                    CargarCmbSedesCuidad(cmbCiudadSedeEntrega, false, cmbPaisSedeEntrega.SelectedValue.ToString());
                    btnSedeDireccion.Visible = cmbPaisSedeEntrega.SelectedValue.Equals("ESTADOS UNIDOS DE AMÉRICA") ? true : false;

                    
                }
                else
                    CargarCmbSedesCuidad(cmbCiudadSedeEntrega, false, "-1");
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "cmbPaisSedeEntrega_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbPaisSedeEntrega_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void btnCopiarDireccion_Click(object sender, EventArgs e)
        {
            try
            {
                txtResidencia1.Text = txtResidencia1.Text.Trim();
                txtResidencia2.Text = txtResidencia2.Text.Trim();

                if (txtResidencia1.Text.Equals("") == false && txtResidencia1.Text.Equals(string.Empty) == false)
                    txtResidenciaEntrega1.Text = txtResidencia1.Text;

                if (txtResidencia2.Text.Equals("") == false && txtResidencia2.Text.Equals(string.Empty) == false)
                    txtResidenciaEntrega2.Text = txtResidencia2.Text;

                if (cmbEstadoResidencia.SelectedIndex >= 0)
                {
                    cmbEstadoEntrega.SelectedIndex = cmbEstadoResidencia.SelectedIndex;
                    cmbEstadoEntrega_SelectionChangeCommitted(sender, e);
                }

                if (cmbZipCodeResidencia.SelectedIndex >= 0)
                {
                    cmbZipCodeEntrega.SelectedIndex = cmbZipCodeResidencia.SelectedIndex;
                    cmbZipCodeEntrega_SelectionChangeCommitted(sender, e);
                }

                if (cmbCiudadResidencia.SelectedIndex >= 0)
                    cmbCiudadEntrega.SelectedIndex = cmbCiudadResidencia.SelectedIndex;
            }
            catch (Exception ex)
            {
                txtMensaje.Text = "btnCopiarDireccion_Click(). " + ex.Message;
                MessageBox.Show("btnCopiarDireccion_Click(). " + ex.Message);
            }
        }

        private void cmbZipCodeResidencia_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, "0");
                cmbCiudadResidencia.Enabled = false;

                if (cmbZipCodeResidencia.SelectedValue != null)
                {
                    string sZipCode = cmbZipCodeResidencia.SelectedValue.ToString();

                    if (sZipCode.Equals(String.Empty) || sZipCode.Equals(""))
                        throw new Exception("Zip Code de residencia incorrecto. ");

                    CargarCmbCiudadesZipCode(cmbCiudadResidencia, false, sZipCode);
                    cmbCiudadResidencia.Enabled = true;
                }                   
            }
            catch(Exception ex)
            {
                txtMensaje.Text = "cmbZipCodeResidencia_SelectionChangeCommitted(). " + ex.Message;
                MessageBox.Show("cmbZipCodeResidencia_SelectionChangeCommitted(). " + ex.Message);
            }
        }

        private void cmbTiposDocumento_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtNumeroId.Text = txtNumeroSerie.Text = txtCui.Text = string.Empty;

                if (cmbTiposDocumento.Text.Equals(string.Empty) == false && cmbTiposDocumento.Text.Equals("") == false)
                {
                    lblCui.Text = "CUI: ";
                    txtCui.Enabled = true;
                    if (cmbTiposDocumento.Text.Contains("DPI") == false)
                    {
                        lblNumeroId.Enabled = txtNumeroId.Enabled = true;
                        lblNumeroSerie.Enabled = txtNumeroSerie.Enabled = false;

                        if (Properties.Settings.Default.ARTICULO_98)
                        {
                            if (cmbTiposDocumento.Text.Contains("PASAPORTE") == true)
                                lblCui.Text = "No. Pasaporte: ";
                        }                        
                    }
                    else
                    {
                        lblNumeroId.Enabled = txtNumeroId.Enabled = false;
                        lblNumeroSerie.Enabled = txtNumeroSerie.Enabled = true;
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
            try
            {
                Arraigos arraigo;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/arraigo_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    arraigo = JsonConvert.DeserializeObject<Arraigos>(json);
                }

                string s = JsonConvert.SerializeObject(arraigo.data);
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

                arraigo.informacionArraigos = new DataSet();
                arraigo.informacionArraigos.Tables.Add(dt);

                dsResultado.Tables[0].Rows[0]["DATOS"] = arraigo;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaArraigosxNombres(). " + ex.Message;
            }
            return dsResultado;
        }

        public DataSet ConsultaAlertasxNombres(string primerNombre, string segundoNombre, string primerApellido, string segundoApellido)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                Alertas alertas;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/alertas_nombres_apellidos?primer_nombre=" + primerNombre + "&segundo_nombre=" + segundoNombre + "&primer_apellido=" + primerApellido + "&segundo_apellido=" + segundoApellido);

                var user = "migracion-pasaportes-enrollment-3.0";
                var password = "abc123";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    alertas = JsonConvert.DeserializeObject<Alertas>(json);
                }
                string s = JsonConvert.SerializeObject(alertas.data);
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(s, (typeof(DataTable)));

                alertas.informacionAlerta = new DataSet();
                alertas.informacionAlerta.Tables.Add(dt);

                dsResultado.Tables[0].Rows[0]["DATOS"] = alertas;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaAlertaxNombres(). " + ex.Message;
            }
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

                            //20/08/2020 CAMBIO EN LA VALIDACIÓN DEL PAGO DE PASAPORTES POR TRANSICIÓN A INSTITUTO GUATEMALTECO DE MIGRACIÓN
                            //request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + "/recibo_banrural_pasaportes?recibo=" + valor);

                            string boleta = valor.Split('.')[0].ToString();
                            string recibo = valor.Split('.')[1].ToString();

                            //PUERTO DE DESARROLLO :8080
                            request = (HttpWebRequest)WebRequest.Create(@"" + Settings.Default.IP_SERVICIOS_WEB + ":8080/recibo_banrural_pasaportes_IGM?boleta="+boleta+"&recibo=" + recibo);

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
            try
            {
                if (e.KeyChar == (char)13 && (txtNoRecibo.Text.Equals(string.Empty) == false && txtNoRecibo.Text.Equals("") == false))//)Keys.Enter)
                {
                    if (txtNoRecibo.Text.Split('.').Length != 2)
                        throw new Exception("¡Por favor, ingrese los datos de pago en el formato correcto NoBoleta.NoRecibo, ejemplo: 12345.12345678!");

                    pbxNumeroRecibo.Image = pbxLoadColor.Image;
                    txtNoRecibo.ForeColor = Color.Black;
                    grpTipoBusquedaPago.Enabled = false;

                    //GUATEMALA
                    if (sedeEstacion.CODIGO_PAIS.Equals("320"))
                    {
                        if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI") && (txtCui.Text.Equals("") == false && txtCui.Text.Equals(string.Empty) == false))
                        {
                            if (true)//txtNoRecibo.Text.All(char.IsLetterOrDigit))
                            {
                                DataSet dsResultado = new DataSet();
                                if (rbnBoleta.Checked)
                                {
                                    dsResultado = await ConsultarDocumentoPago("BOLETA", txtNoRecibo.Text);
                                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    BOLETA vBoleta = (BOLETA)dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"];

                                    if (bool.Parse(vBoleta.mensaje))
                                    {
                                        if (vBoleta.data.documento.Contains(txtCui.Text))
                                        {
                                            txtNoRecibo.ForeColor = Color.Green;
                                            pbxNumeroRecibo.Image = pbxCheckColor.Image;
                                        }
                                        else
                                        {
                                            txtNoRecibo.ForeColor = Color.Orange;
                                            pbxNumeroRecibo.Image = pbxWarningColor.Image;
                                        }
                                    }
                                    else
                                        throw new Exception("Código: " + vBoleta.codigo + ", Mensaje: " + vBoleta.mensaje);

                                }
                                else if (rbnTransaccion.Checked)
                                {
                                    dsResultado = await ConsultarDocumentoPago("TRANSACCION", txtNoRecibo.Text);

                                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    TRANSACCION vTransaccion = (TRANSACCION)dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"];

                                    if (bool.Parse(vTransaccion.mensaje))
                                    {
                                        if (vTransaccion.data.documento.Contains(txtCui.Text))
                                        {
                                            txtNoRecibo.ForeColor = Color.Green;
                                            pbxNumeroRecibo.Image = pbxCheckColor.Image;
                                        }
                                        else
                                        {
                                            txtNoRecibo.ForeColor = Color.Orange;
                                            pbxNumeroRecibo.Image = pbxWarningColor.Image;
                                        }
                                    }
                                    else
                                        throw new Exception("Código: " + vTransaccion.codigo + ", Mensaje: " + vTransaccion.mensaje);
                                }
                                else if (rbnRecibo.Checked)
                                {
                                    dsResultado = await ConsultarDocumentoPago("RECIBO", txtNoRecibo.Text);

                                    if (bool.Parse(dsResultado.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsResultado.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    RECIBO vRecibo = (RECIBO)dsResultado.Tables[0].Rows[0]["DATOS_PAGO_PASAPORTE"];

                                    if (bool.Parse(vRecibo.mensaje))
                                    {
                                        string documentoPago = null;
                                        try
                                        {
                                            documentoPago = new String(vRecibo.data.documento.Where(Char.IsDigit).ToArray());

                                            if (documentoPago.Equals("") || documentoPago.Equals(string.Empty))
                                                documentoPago = vRecibo.data.documento;
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            documentoPago = vRecibo.data.documento;
                                        }

                                        if(txtCui.Text.Equals("") == false && txtCui.Text.Equals(string.Empty) == false && txtCui.Text.Equals(documentoPago))
                                        {
                                            txtNoRecibo.ForeColor = Color.Green;
                                            pbxNumeroRecibo.Image = pbxCheckColor.Image;
                                        }
                                        else
                                        {
                                            txtNoRecibo.ForeColor = Color.Orange;
                                            pbxNumeroRecibo.Image = pbxWarningColor.Image;
                                        }
                                    }
                                    else
                                        throw new Exception("Código: " + vRecibo.codigo + ", Mensaje: " + vRecibo.mensaje);
                                }
                            }
                            else
                            {
                                txtNoRecibo.ForeColor = Color.Red;
                                pbxNumeroRecibo.Image = pbxWarningColor.Image;
                            }
                        }
                        else
                        {
                            txtNoRecibo.ForeColor = Color.Black;
                            pbxNumeroRecibo.Image = pbxWarningColor.Image;
                        }
                    }
                    else
                    {
                        txtNoRecibo.ForeColor = Color.Blue;
                        pbxNumeroRecibo.Image = pbxCheckColor.Image;
                    }                        
                }
            }
            catch (Exception ex)
            {
                txtNoRecibo.ForeColor = Color.Red;
                pbxNumeroRecibo.Image = pbxWarningColor.Image;
                txtMensaje.Text = "txtNoRecibo_KeyPress(). " + ex.Message;
                //MessageBox.Show("txtNoRecibo_KeyPress(). " + ex.Message);
            }
            grpTipoBusquedaPago.Enabled = true;
        }        

        private void chkVistaEnVivo_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    if (chkVistaEnVivo.Checked)
                    {
                        //LiveViewPicBox.Paint += LiveViewPicBox_Paint;
                        if (!MainCamera.IsLiveViewOn) { MainCamera.StartLiveView(); /*chkVistaEnVivo.Text = "Detener vista en vivo";*/ }
                        btnCapturarRostro.Enabled = true;
                    }
                    else
                    {
                        //LiveViewPicBox.Paint -= LiveViewPicBox_Paint;
                        if (MainCamera.IsLiveViewOn) { MainCamera.StopLiveView(); /*chkVistaEnVivo.Text = "Iniciar vista en vivo";*/ }

                        btnCapturarRostro.Enabled = false;

                        //LiveViewPicBox.Image = pbxUsuario.Image;
                        //LiveViewPicBox.Refresh();

                        //LiveViewPicBox = new PictureBox();
                        //LiveViewPicBox.Image = pbxUsuario.Image;
                        //using (Graphics graph = Graphics.FromImage(LiveViewPicBox.Image))
                        //{
                        //    graph.Clear(Color.White);
                        //}

                        //LiveViewPicBox.Image = pbxUsuario.Image;
                        //LiveViewPicBox.Refresh();
                        //LiveViewPicBox.Image = 
                    }

                    //if (!MainCamera.IsLiveViewOn) { MainCamera.StartLiveView(); chkVistaEnVivo.Text = "Detener vista en vivo";  }
                    //else { MainCamera.StopLiveView(); chkVistaEnVivo.Text = "Iniciar vista en vivo"; LiveViewPicBox.Image = null; }
                }
                catch (Exception ex) { ReportError(ex.Message, false); }
            }
            catch (Exception ex)
            {

            }
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

        private void faceView2_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (chkIcao.Checked == false)
                {
                    if (lblVistaPreviaMin.Image != null)
                    {
                        RecortarImagen recortarImagen = new RecortarImagen(this);
                        //recortarImagen.Parent = this;
                        Image image = Bitmap.FromFile(Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG");
                        recortarImagen.pbxImagen.Image = (Image)(image.Clone());
                        image.Dispose();
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        recortarImagen.ShowDialog();
                        FotoCumpleIcao = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("faceView2_DoubleClick(). " + ex.Message);
                txtMensaje.Text = "faceView2_DoubleClick(). " + ex.Message;
            }
            
        }

        private void tmrHora_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToLongTimeString();
        }

        int hrsPasaporte, minPasaporte, segPasaporte = 0;
        private void tmrPasaporte_Tick(object sender, EventArgs e)
        {
            try
            {
                if (segPasaporte == 60)
                {
                    minPasaporte++;
                    segPasaporte = 0;
                }

                if (minPasaporte == 60)
                {
                    hrsPasaporte++;
                    minPasaporte = 0;
                }

                if (hrsPasaporte == 60 && minPasaporte == 00 && segPasaporte == 00)
                {
                    hrsPasaporte = 0;
                    //minPasaporte = 0;
                }

                lblTimerPasaporte.Text = hrsPasaporte.ToString().PadLeft(2, '0') + ":" +  minPasaporte.ToString().PadLeft(2, '0') + ":" + segPasaporte.ToString().PadLeft(2, '0');

                segPasaporte++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("tmrPasaporte_Tick(). " + ex.Message);
                txtMensaje.Text = "tmrPasaporte_Tick(). " + ex.Message;
            }
        }

        int minSesionCamara, segSesionCamara = 0;
        private void tmrSesionCamara_Tick(object sender, EventArgs e)
        {
            try
            {
                if (segSesionCamara == 60)
                {
                    minSesionCamara++;
                    segSesionCamara = 0;
                }
                
                if (minSesionCamara == 3)
                {
                    if (MainCamera != null)
                        btnActivarCapturaRostroAsync_Click(sender, e);
                }
                lblTimerCamara.Text = "Tiempo: " + minSesionCamara.ToString().PadLeft(2, '0') + " minuto(s) y  " + segSesionCamara.ToString().PadLeft(2, '0') + " segundo(s)";
                segSesionCamara++;
            }
            catch(Exception ex)
            {
                MessageBox.Show("tmrSesionCamara_Tick(). " + ex.Message);
                txtMensaje.Text = "tmrSesionCamara_Tick(). " + ex.Message;
                tmrSesionCamara.Stop();
            }
        }

        private void txtEstatura_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                e.Handled = !(char.IsDigit(e.KeyChar) || e.KeyChar == (char)System.Windows.Forms.Keys.Back);
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
            Settings.Default.scan = TwainLib.TwainOperations.GetScanSource();
            Settings.Default.Save();
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

        private async void MostrarImagenSegmentada()
        {
            try
            {
                FotoCumpleIcao = false;
                if (chkIcao.Checked)
                {
                    bool generalize = false;
                    bool fromFile = true;
                    bool fromCamera = false;
                    bool checkIcao = chkIcao.Checked;
                    int count = 3;
                    NBiometricCaptureOptions options = NBiometricCaptureOptions.None;

                    lblEstadoCapturaRostro.Visible = false;

                    NSubject _newSubject = new NSubject();
                    _newSubject.Clear();
                    //faceView2.Face = null;
                    pbxRostroIcao.Image = null;

                    string rutaImagenProcesar = Application.StartupPath + "\\ENROL\\ROSTRO\\SegmentedFace.jpeg";
                    if (File.Exists(rutaImagenProcesar) == false)
                        rutaImagenProcesar = Application.StartupPath + "\\ENROL\\ROSTRO\\Rostro.JPG";

                    using (Bitmap bmp = LoadBitmapUnlocked(rutaImagenProcesar))
                    {
                        NFace face = new NFace
                        {
                            Image = NImage.FromBitmap((Bitmap)(bmp.Clone())),
                            CaptureOptions = NBiometricCaptureOptions.Stream
                        };

                        bmp.Dispose();

                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();

                        if (rutaImagenProcesar.Contains("SegmentedFace.jpeg"))
                        {
                            _newSubject.Faces.Add(face);
                            //faceView2.Face = _newSubject.Faces.First();
                            pbxRostroIcao.Image = _newSubject.Faces.First().Image.ToBitmap();
                            icaoWarningView1.Face = _newSubject.Faces.First();

                            _biometricFaceClient.FacesCheckIcaoCompliance = checkIcao;
                            //NBiometricOperations operations = fromFile ? NBiometricOperations.CreateTemplate : NBiometricOperations.Capture | NBiometricOperations.CreateTemplate;
                            //if (checkIcao) operations |= NBiometricOperations.Segment;

                            NBiometricOperations operations = NBiometricOperations.Segment;

                            NBiometricTask biometricTask = _biometricFaceClient.CreateTask(operations, _newSubject);
                            SetStatusText(Color.Orange, "Extrayendo plantilla ...");

                            var performedTask = await _biometricFaceClient.PerformTaskAsync(biometricTask);
                            if (performedTask.Error != null)
                            {
                                SetStatusText(Color.Red, "Plantilla no extraída...");
                                throw new Exception("Error ejecutar la tarea biométrica" + performedTask.Error.Message);
                            }

                            if (performedTask.Status != NBiometricStatus.Ok)
                            {
                                SetStatusText(Color.Red, "Plantilla no extraída...");
                                throw new Exception("Estatus de operación biométrica no válido. " + performedTask.Status);
                            }

                            //faceView2.Face = _newSubject.Faces[1];
                            pbxRostroIcao.Image = _newSubject.Faces[1].Image.ToBitmap();
                            icaoWarningView1.Face = _newSubject.Faces[1];

                            //OnCreateTemplateCompleted(performedTask);
                            //OnCapturingCompleted(performedTask, _newSubject);

                            //if (performedTask.Status != NBiometricStatus.Ok)
                            //    throw new Exception("Error al procesar el rostro");

                            NLAttributes _attributes = _newSubject.Faces[1].Objects.ToArray().FirstOrDefault();
                            var warnings = _attributes.IcaoWarnings;

                            Color FaceDetected = Color.Green;
                            Color Expression = GetColorForConfidence(warnings, NIcaoWarnings.Expression, _attributes.ExpressionConfidence);
                            Color DarkGlasses = GetColorForConfidence(warnings, NIcaoWarnings.DarkGlasses, _attributes.DarkGlassesConfidence);
                            Color Blink = GetColorForConfidence(warnings, NIcaoWarnings.Blink, _attributes.BlinkConfidence);
                            Color MouthOpen = GetColorForConfidence(warnings, NIcaoWarnings.MouthOpen, _attributes.MouthOpenConfidence);
                            Color LookingAway = GetColorForConfidence(warnings, NIcaoWarnings.LookingAway, _attributes.LookingAwayConfidence);
                            Color RedEye = GetColorForConfidence(warnings, NIcaoWarnings.RedEye, _attributes.RedEyeConfidence);
                            Color FaceDarkness = GetColorForConfidence(warnings, NIcaoWarnings.FaceDarkness, _attributes.FaceDarknessConfidence);
                            Color UnnaturalSkinTone = GetColorForConfidence(warnings, NIcaoWarnings.UnnaturalSkinTone, _attributes.UnnaturalSkinToneConfidence);
                            Color ColorsWashedOut = GetColorForConfidence(warnings, NIcaoWarnings.WashedOut, _attributes.WashedOutConfidence);
                            Color Pixelation = GetColorForConfidence(warnings, NIcaoWarnings.Pixelation, _attributes.PixelationConfidence);
                            Color SkinReflection = GetColorForConfidence(warnings, NIcaoWarnings.SkinReflection, _attributes.SkinReflectionConfidence);
                            Color GlassesReflection = GetColorForConfidence(warnings, NIcaoWarnings.GlassesReflection, _attributes.GlassesReflectionConfidence);

                            Color Roll = GetColorForFlags(warnings, NIcaoWarnings.RollLeft, NIcaoWarnings.RollRight);
                            Color Yaw = GetColorForFlags(warnings, NIcaoWarnings.YawLeft, NIcaoWarnings.YawRight);
                            Color Pitch = GetColorForFlags(warnings, NIcaoWarnings.PitchDown, NIcaoWarnings.PitchUp);
                            Color TooClose = GetColorForFlags(warnings, NIcaoWarnings.TooNear);
                            Color TooFar = GetColorForFlags(warnings, NIcaoWarnings.TooFar);
                            Color TooNorth = GetColorForFlags(warnings, NIcaoWarnings.TooNorth);
                            Color TooSouth = GetColorForFlags(warnings, NIcaoWarnings.TooSouth);
                            Color TooWest = GetColorForFlags(warnings, NIcaoWarnings.TooWest);
                            Color TooEast = GetColorForFlags(warnings, NIcaoWarnings.TooEast);

                            Color Sharpness = GetColorForFlags(warnings, NIcaoWarnings.Sharpness);
                            Color Saturation = GetColorForFlags(warnings, NIcaoWarnings.Saturation);
                            Color GrayscaleDensity = GetColorForFlags(warnings, NIcaoWarnings.GrayscaleDensity);
                            Color BackgroundUniformity = GetColorForFlags(warnings, NIcaoWarnings.BackgroundUniformity);

                            FotoCumpleIcao = (
                                FaceDetected == Color.Green
                                && Expression == Color.Green
                                && DarkGlasses == Color.Green
                                && Blink == Color.Green
                                && MouthOpen == Color.Green
                                && LookingAway == Color.Green
                                && RedEye == Color.Green
                                && FaceDarkness == Color.Green
                                && UnnaturalSkinTone == Color.Green
                                && ColorsWashedOut == Color.Green
                                && Pixelation == Color.Green
                                && SkinReflection == Color.Green
                                && GlassesReflection == Color.Green

                                && Roll == Color.Green
                                && Yaw == Color.Green
                                && Pitch == Color.Green
                                && TooClose == Color.Green
                                && TooFar == Color.Green
                                && TooNorth == Color.Green
                                && TooSouth == Color.Green
                                && TooWest == Color.Green
                                && TooEast == Color.Green

                                && Sharpness == Color.Green
                                && Saturation == Color.Green
                                && GrayscaleDensity == Color.Green
                                && BackgroundUniformity == Color.Green
                                ) ? true : false;

                            if (FotoCumpleIcao)
                            {
                                DataSet dsValidarFoto = await ValidarFotografia();

                                if (bool.Parse(dsValidarFoto.Tables[0].Rows[0]["RESULTADO"].ToString()))
                                    tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";
                            }
                            SetStatusText(Color.Green, "¡Plantilla extraída!");
                        }
                    }
                    try { File.Delete(Application.StartupPath + "\\ENROL\\Fotos\\Rostro.JPG"); } catch { }
                    try { File.Delete(Application.StartupPath + "\\ENROL\\Rostro.JPG"); } catch { }
                    try { File.Delete(Application.StartupPath + "\\ENROL\\SegmentedFace.jpeg"); } catch { }
                }

            }
            catch (Exception ex)
            {
                //throw new Exception("ProcesarImagenRostro(). " + ex.Message);
                MessageBox.Show("ProcesarImagenRostro(). " + ex.Message);
                txtMensaje.Text = "ProcesarImagenRostro(). " + ex.Message;
            }
        }

        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);                
            }
        }

        public void ProcesarRostrosNeuroStart()
        {
            try
            {
                Process[] proc = Process.GetProcessesByName("ProcesarRostrosNeuro");

                proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length > 0)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();
                else if (proc.Length == 0)
                    InvocarProcesamientoRostro();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod().Name.Split('_')[0] + ": " + ex.Message);
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
                txtNoCaso.Text = txtNoCaso.Text.Trim();
                if ((txtNoCaso.Text.Length == txtNoCaso.MaxLength) == false)
                    msgError += "El número de caso debe contener " + txtNoCaso.MaxLength + " caracteres. ";
                else
                {
                    if (Properties.Settings.Default.DEVICE_MODE.Equals("SEDE"))
                    {
                        string iniciales = txtNoCaso.Text.Substring(0, 3);
                        string numero = txtNoCaso.Text.Substring(3, txtNoCaso.Text.Length - 3);

                        string sedeConfigurada = Properties.Settings.Default.SEDE;

                        if ((sedeConfigurada.Equals(iniciales) == false) || (numero.All(char.IsDigit) == false))
                            msgError += "Número de caso no válido para equipo SEDE (por ejemplo: " + Properties.Settings.Default.SEDE + "123456789)";
                    }
                    else if (Properties.Settings.Default.DEVICE_MODE.Equals("MOVIL"))
                    {
                        string iniciales = txtNoCaso.Text.Substring(0, 4);
                        string numero = txtNoCaso.Text.Substring(4, txtNoCaso.Text.Length - 4);

                        string sedeConfigurada = Properties.Settings.Default.SEDE;

                        if (((sedeConfigurada + "M").Equals(iniciales) == false) || (numero.All(char.IsDigit) == false))
                            msgError += "Número de caso no válido para equipo MOVIL (por ejemplo: " + Properties.Settings.Default.SEDE + "M12345678)";
                    }
                }
                
                //if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                {
                    if(cmbTipoTramite.SelectedIndex < 0)
                        msgError += "Seleccione un tipo de trámite. ";

                    int edad = DateTime.Now.Year - dtpFechaNacimiento.Value.Year;
                    if (DateTime.Now.Month < dtpFechaNacimiento.Value.Month || (DateTime.Now.Month == dtpFechaNacimiento.Value.Month && DateTime.Now.Day < dtpFechaNacimiento.Value.Day))
                        edad--;

                    if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                        if (edad < 18)
                            msgError += "Ingrese una fecha de nacimiento válida. ";
                    else if(cmbTipoPasaporte.Text == "ORDINARIO MENOR" || cmbTipoPasaporte.Text == "DIPLOMATICO MENOR")
                            if (edad >= 18)
                                msgError += "Ingrese una fecha de nacimiento válida. ";                    

                    txtNoRecibo.Text = txtNoRecibo.Text.Trim();
                    if (txtNoRecibo.Text.Equals(string.Empty) == true || txtNoRecibo.Text.Equals("") == true)
                        msgError += "Ingrese un número de recibo. ";

                    if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI"))
                    {
                        txtCui.Text = txtCui.Text.Trim();
                        if (txtCui.Text.Equals(string.Empty) == true || txtCui.Text.Equals("") == true)
                            msgError += "Ingrese un CUI. ";

                        if ((txtCui.Text.Length == txtCui.MaxLength) == false)
                            msgError += "El CUI debe contener " + txtCui.MaxLength.ToString() + " números. ";

                        if (txtCui.Text.All(char.IsDigit) == false)
                            msgError += "El CUI debe contener únicamente NÚMEROS. ";

                        if (txtCui.Text.Length > 0)
                        {
                            if (txtCui.Enabled)
                            {
                                DataSet dsDeptoMunicEmisionDPI = Depto_Munic_EmisionDPI(txtCui.Text.Substring(txtCui.Text.Length - 4, 2), txtCui.Text.Substring(txtCui.Text.Length - 2, 2));

                                if (bool.Parse(dsDeptoMunicEmisionDPI.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                    msgError += dsDeptoMunicEmisionDPI.Tables[0].Rows[0]["MSG_ERROR"].ToString();
                            }
                        }

                        txtCui.Text = txtCui.Text.Trim();
                        if (txtCui.Text.Equals(string.Empty) || txtCui.Text.Equals("") || txtCui.Text.Length != txtCui.MaxLength || (txtCui.Text.All(char.IsDigit) == false))
                            msgError += "Ingrese un CUI válido. ";
                    }                    

                    txtPrimerNombre.Text = txtPrimerNombre.Text.Trim();
                    if(txtPrimerNombre.Text.Equals("") || txtPrimerNombre.Text.Equals(string.Empty))
                        msgError += "Ingrese primer nombre. ";

                    txtSegundoNombre.Text = txtSegundoNombre.Text.Trim();
                    txtTercerNombre.Text = txtTercerNombre.Text.Trim();

                    txtPrimerApellido.Text = txtPrimerApellido.Text.Trim();
                    if (txtPrimerApellido.Text.Equals("") || txtPrimerApellido.Text.Equals(string.Empty))
                        msgError += "Ingrese primer apellido. ";

                    txtSegundoApellido.Text = txtSegundoApellido.Text.Trim();

                    txtApellidoCasada.Text = txtApellidoCasada.Text.Trim();

                    //if(cmbGenero.Text.Equals("FEMENINO") && cmbEstadoCivil.Text.Equals("CASADO") && (txtApellidoCasada.Text.Equals("") || txtApellidoCasada.Text.Equals(string.Empty)))
                    //{
                    //    txtApellidoCasada.Enabled = true;
                    //    msgError += "Ingrese un apellido de casada. ";
                    //}

                    if (cmbGenero.Text.Equals("MASCULINO") && !(txtApellidoCasada.Text.Equals("") || txtApellidoCasada.Text.Equals(string.Empty)))
                    {
                        txtApellidoCasada.Enabled = true;
                        msgError += "Borre apellido de casada. ";
                    }

                    if (cmbGenero.Enabled)
                        //if(cmbGenero.Text.Equals("") || cmbGenero.Text.Equals(string.Empty))
                        if (cmbGenero.SelectedIndex < 0)
                            msgError += "Seleccione un género. ";
                    
                    if (cmbEstadoCivil.Enabled)
                        //if (cmbEstadoCivil.Text.Equals("") || cmbEstadoCivil.Text.Equals(string.Empty))
                        if (cmbEstadoCivil.SelectedIndex < 0)
                            msgError += "Seleccione un estado civil. ";
                    
                    if (cmbOcupaciones.Enabled)
                        //if (cmbOcupaciones.Text.Equals("") || cmbOcupaciones.Text.Equals(string.Empty))
                        if (cmbOcupaciones.SelectedIndex < 0)
                            msgError += "Seleccione una ocupación. ";


                    txtNombrePadre.Text = txtNombrePadre.Text.Trim();
                    if (chkDesconocido.Checked == false)
                        if (txtNombrePadre.Text.Equals(string.Empty) == true || txtNombrePadre.Text.Equals("") == true)
                            msgError += "Ingrese nombre del padre. ";

                    txtNombreMadre.Text = txtNombreMadre.Text.Trim();
                    if (chkDesconocida.Checked == false)
                        if (txtNombreMadre.Text.Equals(string.Empty) == true || txtNombreMadre.Text.Equals("") == true)
                            msgError += "Ingrese nombre de la madre. ";

                    txtEstatura.Text = txtEstatura.Text.Trim();
                    if (txtEstatura.Text.Equals(string.Empty) == true || txtEstatura.Text.Equals("") == true)
                        msgError += "Ingrese estatura. ";
                    else
                    {
                        int estatura = 0;
                        try
                        {
                            int.TryParse(txtEstatura.Text, out estatura);

                            if (estatura <= 0 || estatura >= 300)
                                throw new Exception();
                        }
                        catch{msgError += "Estatura no válida (" + estatura + "). ";}
                    }

                    if(cmbTiposDocumento.SelectedIndex < 0)
                        msgError += "Seleccione un tipo de documento de identificación. ";

                    txtNumeroSerie.Text = txtNumeroSerie.Text.Trim();

                    if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                        if(pbxDPI.Image.Tag.Equals("Check"))
                            if (txtNumeroSerie.Text.Equals("") || txtNumeroSerie.Text.Equals(string.Empty))
                                msgError += "Ingrese un número de serie para el documento de identificación. ";

                    if (cmbPaisNacimiento.SelectedIndex < 0)
                        msgError += "Seleccione un país de nacimiento. ";
                    else
                    {
                        if (cmbPaisNacimiento.SelectedValue.Equals("320"))
                        {
                            msgError += (cmbDeptoNacimiento.SelectedIndex < 0) ? "Seleccione departamento de nacimiento. " : string.Empty;
                            msgError += (cmbMunicNacimiento.SelectedIndex < 0) ? "Seleccione municipio de nacimiento. " : string.Empty;
                        }
                        else
                        {
                            txtDepartamentoNacimiento.Text = txtDepartamentoNacimiento.Text.Trim();
                            msgError += (txtDepartamentoNacimiento.Equals("") || txtDepartamentoNacimiento.Equals(string.Empty)) ? "Ingrese un departamento de residencia. " : string.Empty;
                        }
                    }
                    
                    if (cmbNacionalidad.SelectedIndex < 0)
                        msgError += "Seleccione la nacionalidad. ";

                    txtResidencia1.Text = txtResidencia1.Text.Trim();
                    if (txtResidencia1.Text.Equals(string.Empty) == true || txtResidencia1.Text.Equals("") == true)
                        msgError += "Ingrese dirección de residencia. ";

                    if (cmbPaisResidencia.SelectedIndex < 0)
                        msgError += "Seleccione un país de residencia. ";
                    else
                    {
                        if (cmbPaisResidencia.SelectedValue.Equals("320"))
                        {
                            msgError += (cmbDeptoResidencia.SelectedIndex < 0) ? "Seleccione departamento de residencia. " : string.Empty;
                            msgError += (cmbMunicResidencia.SelectedIndex < 0) ? "Seleccione municipio de residencia. " : string.Empty;
                        }
                        else if (cmbPaisResidencia.SelectedValue.Equals("840"))
                        {
                            msgError += (cmbEstadoResidencia.SelectedIndex < 0) ? "Seleccione estado de residencia. " : string.Empty;
                            msgError += (cmbZipCodeResidencia.SelectedIndex < 0) ? "Seleccione zipcode de residencia. " : string.Empty;
                            msgError += (cmbCiudadResidencia.SelectedIndex < 0) ? "Seleccione ciudad de residencia. " : string.Empty;
                        }
                    }

                    //GUATEMALA U OTRO PAÍS
                    if (btnSedeDireccion.Visible == false)
                    {
                        if (cmbPaisSedeEntrega.SelectedIndex < 0)
                            msgError += "Seleccione un país de sede para entrega. ";

                        if (cmbCiudadSedeEntrega.SelectedIndex < 0)
                            msgError += "Seleccione una ciudad de sede para entrega. ";
                    }
                    else
                    {
                        if (btnSedeDireccion.Text.Equals("Dirección de envío"))
                        {
                            if (cmbPaisSedeEntrega.SelectedIndex < 0)
                                msgError += "Seleccione un país de sede para entrega. ";

                            if (cmbCiudadSedeEntrega.SelectedIndex < 0)
                                msgError += "Seleccione una ciudad de sede para entrega. ";
                        }
                        else
                        {
                            msgError += (cmbEstadoEntrega.SelectedIndex < 0) ? "Seleccione estado de entrega. " : string.Empty;
                            msgError += (cmbZipCodeEntrega.SelectedIndex < 0) ? "Seleccione zipcode de entrega. " : string.Empty;
                            msgError += (cmbCiudadEntrega.SelectedIndex < 0) ? "Seleccione ciudad de entrega. " : string.Empty;
                        }
                    }
                    //(###)###-#### = 13
                    //####-#### 9
                    //txtTelCelular.Text = txtTelCelular.Text.Trim();
                    //if (txtTelCelular.Text.Equals(string.Empty) == true || txtTelCelular.Text.Equals("") == true || txtTelCelular.MaskFull == false)
                    //    msgError += "Ingrese un número de celular valido (" + txtTelCelular.Mask + "). ";

                    if(txtTelCasa.Text.Any(char.IsDigit))
                        if(txtTelCasa.MaskFull == false)
                            msgError += "Ingrese un número de teléfono de casa valido (" + txtTelCasa.Mask + "). ";
                    
                    if (txtTelTrabajo.Text.Any(char.IsDigit))
                        if (txtTelTrabajo.MaskFull == false)
                            msgError += "Ingrese un número de teléfono de trabajo valido (" + txtTelTrabajo.Mask + "). ";

                    if (cmbTipoPasaporte.Text == "ORDINARIO MENOR" || cmbTipoPasaporte.Text == "DIPLOMATICO MENOR")
                    {
                        bool datosPadreVacios, datosMadreVacios;
                        datosPadreVacios = datosMadreVacios = false;

                        txtNombrePadre.Text = txtNombrePadre.Text.Trim();
                        txtApellidoPadre.Text = txtApellidoPadre.Text.Trim();
                        txtNumeroIdPadre.Text = txtNumeroIdPadre.Text.Trim();

                        if (txtNombrePadre.Text.Equals("") || txtNombrePadre.Text.Equals(string.Empty) ||
                            txtApellidoPadre.Text.Equals("") || txtApellidoPadre.Text.Equals(string.Empty) ||
                            cmbTipoIdPadre.SelectedIndex < 0 ||
                            txtNumeroIdPadre.Text.Equals("") || txtNumeroIdPadre.Equals(string.Empty))
                            datosPadreVacios = true;

                        txtNombreMadre.Text = txtNombreMadre.Text.Trim();
                        txtApellidoMadre.Text = txtApellidoMadre.Text.Trim();
                        txtNumeroIdMadre.Text = txtNumeroIdMadre.Text.Trim();

                        if (txtNombreMadre.Text.Equals("") || txtNombreMadre.Text.Equals(string.Empty) ||
                            txtApellidoMadre.Text.Equals("") || txtApellidoMadre.Text.Equals(string.Empty) ||
                            cmbTipoIdMadre.SelectedIndex < 0 ||
                            txtNumeroIdMadre.Text.Equals("") || txtNumeroIdMadre.Equals(string.Empty))
                            datosMadreVacios = true;

                        if (datosPadreVacios && datosMadreVacios)
                            msgError += "Ingrese datos de almenos unos de los padres. ";
                        else
                        {
                            if (datosPadreVacios == false)
                                if (cmbTipoIdPadre.SelectedValue.Equals("3") || cmbTipoIdPadre.SelectedValue.Equals("5"))
                                    if (txtNumeroIdPadre.Text.Length != txtNumeroIdPadre.MaxLength || (txtNumeroIdPadre.Text.All(char.IsDigit) == false))
                                        msgError += "Ingrese un CUI de padre válido. ";

                            if (datosMadreVacios == false)
                                if (cmbTipoIdMadre.SelectedValue.Equals("3") || cmbTipoIdMadre.SelectedValue.Equals("5"))
                                    if (txtNumeroIdMadre.Text.Length != txtNumeroIdMadre.MaxLength || (txtNumeroIdMadre.Text.All(char.IsDigit) == false))
                                        msgError += "Ingrese un CUI de madre válido. ";
                        }
                    }
                    
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

        private async Task <DataSet> ValidarFotografia()
        {
            //return Task.Run(() =>
            //{
            DataSet dsResultado = ArmarDsResultado();
            string msgError = string.Empty;

            try
            {
                //if (faceView2.Face == null)
                 if (pbxRostroIcao.Image == null)
                    msgError += "La captura de la fotografía es incorrecta. ";

                if (!pbxFotoDPITitular.Image.Tag.Equals("FotoDefault") && tab_principal.TabPages["tabFotografia"].ImageKey.Equals("check.bmp") && !pbxMOCF.Image.Tag.Equals("Check"))
                    tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";

                if (
                        tab_principal.TabPages["tabFotografia"].ImageKey == "warning.bmp" &&
                        msgError.Equals(string.Empty) &&
                        (cmbTipoPasaporte.Text.Equals("ORDINARIO") || cmbTipoPasaporte.Text.Equals("DIPLOMATICO") || cmbTipoPasaporte.Text.Equals("OFICIAL")) &&
                        chkIcao.Checked
                    )
                {
                    if (FotoCumpleIcao)
                    {
                        if (msgError.Equals(string.Empty) == false || msgError.Equals("") == false)
                            throw new Exception(msgError);

                        dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                        dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;

                        lblMatchFace.Text = "";
                        pbxFondoIcao.BackColor = Color.White;
                        pbxMOCF.Image = pbxWarning.Image;

                        if (pbxFotoDPITitular.Image.Tag.Equals("FotoDefault") == false)
                        {
                            try
                            {
                                pbxMOCF.Image = pbxLoad.Image;

                                NSubject vNSubjectDPI = new NSubject();
                                NSubject vNSubjectPasaporte = new NSubject();

                                Bitmap bitmap = new Bitmap(pbxFotoDPITitular.Image);
                                var ms = new MemoryStream();
                                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                                NFace faceDPI = new NFace() { SampleBuffer = NBuffer.FromArray(ms.ToArray()) };
                                //NFace facePasaporte = new NFace() { Image = faceView2.Face.Image };
                                
                                var ms2 = new MemoryStream();
                                pbxRostroIcao.Image.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                                NFace facePasaporte = new NFace() { Image = NImage.FromMemory(ms2.ToArray()) };
                                
                                var status = NBiometricStatus.None;

                                //ms.Dispose();

                                if (Properties.Settings.Default.MATCHING_MODE.Equals("SERVER"))
                                {
                                    byte[] imageBytes = ms.ToArray();
                                    string rostroDPI = Convert.ToBase64String(imageBytes);

                                    string rostroB = Convert.ToBase64String(facePasaporte.Image.Save(NImageFormat.Png).ToArray());
                                    DataSet dsBiometria = wsBiometricsDGM.CompararDosRostrosStrBase64IMG(rostroDPI, rostroB);

                                    ms.Dispose();
                                    ms2.Dispose();

                                    DataSet dsValidarDsBio = funciones.EsDsBiometriaValido(dsBiometria);
                                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    DataSet DsCoincidenciaAB = (DataSet)(dsBiometria.Tables[0].Rows[0]["METODO_RESULTADO"]);

                                    dsValidarDsBio = funciones.EsDsCoincidenciaABValido(DsCoincidenciaAB);
                                    if (bool.Parse(dsValidarDsBio.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                                        throw new Exception(dsValidarDsBio.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                                    if (bool.Parse(DsCoincidenciaAB.Tables[0].Rows[0]["COINCIDENCIA"].ToString()) == true)
                                        status = NBiometricStatus.Ok;
                                }
                                else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
                                {
                                    vNSubjectDPI.Faces.Add(faceDPI);
                                    vNSubjectPasaporte.Faces.Add(facePasaporte);

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
                                    }

                                    if (status != NBiometricStatus.Ok)
                                        msgError += "¡Los rostros no coinciden!";
                                }

                                if (msgError.Equals(string.Empty))
                                {
                                    if (status != NBiometricStatus.Ok)
                                    {
                                        //msgError += "Los rostros no coinciden. ";
                                        lblMatchFace.Text = "Los rostros no coinciden. ";
                                        pbxFondoIcao.BackColor = Color.Red;
                                        pbxMOCF.Image = pbxWarning.Image;
                                    }
                                    else
                                    {
                                        //int score = vNSubjectDPI.MatchingResults[0].Score;
                                        lblMatchFace.Text = "Coincidencia";

                                        tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";
                                        pbxMOCF.Image = pbxCheck.Image;
                                        pbxFondoIcao.BackColor = Color.Green;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                lblMatchFace.Text = msgError;
                                pbxFondoIcao.BackColor = Color.White;
                                pbxMOCF.Image = pbxWarning.Image;
                                msgError = string.Empty;
                            }
                        }
                    }
                    else
                        msgError += " La foto no cumple el estándar ICAO. ";                    
                }
                else
                {
                    if (cmbTipoPasaporte.Text.Contains("MENOR"))
                    {
                        pbxFondoIcao.BackColor = Color.White;
                        pbxMOCF.Image = pbxWarning.Image;
                    }
                    else if (!chkIcao.Checked)
                    {
                        txtObservacionesIcao.Text = txtObservacionesIcao.Text.Trim();
                        if (txtObservacionesIcao.Text.Equals("") == false && txtObservacionesIcao.Text.Equals(string.Empty) == false)
                        {
                            pbxFondoIcao.BackColor = Color.White;
                            lblMatchFace.Text = string.Empty;
                            pbxMOCF.Image = pbxWarning.Image;
                        }
                        else
                            msgError += " Ingrese observación ICAO. ";
                    }
                    else if (chkIcao.Checked)
                    {
                        if (FotoCumpleIcao == false)
                        {
                            msgError = "La foto no cumple estándar ICAO. ";
                            pbxFondoIcao.BackColor = Color.White;
                            pbxMOCF.Image = pbxWarning.Image;
                        }
                    }
                }

                if (msgError.Equals(string.Empty) == false || msgError.Equals("") == false)
                    throw new Exception(msgError);

                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //tab_principal.TabPages["tabFotografia"].ImageKey = "check.bmp";

            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ValidarFotografia(). " + ex.Message;
                //tab_principal.TabPages["tabFotografia"].ImageKey = "warning.bmp";
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
                if(cmbDedoDerecho.Text.ToUpper().Equals("NINGUNO") && cmbDedoIzquierdo.Text.ToUpper().Equals("NINGUNO"))
                    tab_principal.TabPages["tabHuellas"].ImageKey = "check.bmp";

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

                        if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
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

            } catch (Exception ex)
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

                } else if (Properties.Settings.Default.MATCHING_MODE.Equals("LOCAL"))
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

        private Task<DataSet> ValidarFirma()
        {
            return Task.Run(() =>
            {
                CheckForIllegalCrossThreadCalls = false;

                DataSet ds = ArmarDsResultado();
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
                            Image firma = sigPlusNET1.GetSigImage();
                            Bitmap bit = new Bitmap(firma, 500, 150);

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

                    int minimoProbatorios = int.Parse(Properties.Settings.Default.PROBATORIOS_GUA);
                    if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text == "DIPLOMATICO" || cmbTipoPasaporte.Text == "OFICIAL")
                        if (sedeEstacion.PAIS.Equals("GUATEMALA"))
                            if (edad >= 60)//TERCERA EDAD EN GUATEMALA
                                minimoProbatorios = int.Parse(Properties.Settings.Default.PROBATORIOS_GUA_3_EDAD);
                            else
                                minimoProbatorios = int.Parse(Properties.Settings.Default.PROBATORIOS_GUA);//NO TERCERA EDAD EN GUATEMALA
                        else
                            minimoProbatorios = int.Parse(Properties.Settings.Default.PROBATORIOS_EXTRANJERO);//RESTO DEL MUNDO

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
                string rutaXML = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "CasoPasaporte_" + txtNoCaso.Text.Trim() + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt");
                string rutaXML2 = Path.Combine(Application.StartupPath, "ENROL", "XMLs", "Done", "CasoPasaporte_" + txtNoCaso.Text.Trim() + "_" + Environment.MachineName + "_" + lbl_usuario.Text + "_" + Properties.Settings.Default.SUFIJO_ENCRIPTACION + ".txt");

                if (File.Exists(rutaXML) == false && File.Exists(rutaXML2) == false)
                    throw new Exception("¡Guarde el archivo!");

                string fotoJpegBase64 = "";
                using (var ms = new MemoryStream())
                {
                    pbxRostroIcao.Image.Save(ms, ImageFormat.Jpeg);
                    fotoJpegBase64 = Convert.ToBase64String(ms.ToArray());
                    ms.Dispose();
                }

                //enrollment.foto = chkIcao.Checked ? Convert.ToBase64String(faceView2.Face.Image.Save(NImageFormat.Jpeg).ToArray()) : Convert.ToBase64String(faceView2.Face.Image.Save(NImageFormat.Jpeg).ToArray());//foto = Convert.ToBase64String(_nFaceSegmented.Image.Save(NImageFormat.Png).ToArray());
                enrollment.foto = fotoJpegBase64;

                no_caso = txtNoCaso.Text;
                tipo_pasaporte = cmbTipoPasaporte.Text;
                nombres = txtPrimerNombre.Text.Trim() + " " + txtSegundoNombre.Text.Trim() + " " + txtTercerNombre.Text.Trim();
                apellidos = txtPrimerApellido.Text.Trim() + " " + txtSegundoApellido.Text.Trim();
                apellido_casada = txtApellidoCasada.Text;
                direccion = txtResidencia1.Text + Environment.NewLine + " " + txtResidencia2.Text;
                if (cmbPaisResidencia.Text.Equals("GUATEMALA"))
                    direccion += Environment.NewLine + cmbDeptoResidencia.Text + ", " + cmbMunicResidencia.Text;
                else if (cmbPaisResidencia.Text.Equals("ESTADOS UNIDOS DE AMÉRICA"))
                    direccion += Environment.NewLine + cmbCiudadResidencia.Text + ", " + cmbEstadoResidencia.SelectedValue + " " + cmbZipCodeResidencia.Text;

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
                nacionalidad = cmbNacionalidad.Text;
                
                fecha_nacimiento = dtpFechaNacimiento.Value.ToString("dd/MM/yyyy");
                depto_nacimiento = cmbDeptoNacimiento.Text;
                muni_nacimiento = cmbMunicNacimiento.Text;
                pais_nacimiento = cmbPaisNacimiento.Text;

                //CUANDO NO ES NACIDO EN GUATEMALA, AL PAÍS SE LE CONCATENARÁ EL ESTADO DONDE NACIO
                if (!cmbPaisNacimiento.SelectedValue.ToString().Equals("320") && !cmbPaisNacimiento.SelectedValue.ToString().Equals("840"))
                    pais_nacimiento = pais_nacimiento + ", " + txtDepartamentoNacimiento.Text;
                
                if (cmbTipoPasaporte.Text == "ORDINARIO" || cmbTipoPasaporte.Text.Contains("DIPLOMATICO") || cmbTipoPasaporte.Text == "OFICIAL")
                {
                    identificacion = string.IsNullOrEmpty(txtCui.Text) ? txtNumeroId.Text : txtCui.Text;

                    depto_emision = "";
                    municipio_emision = "";
                    if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI"))
                    {
                        depto_emision = cmbDeptoDPI.Text;
                        municipio_emision = cmbMunicipioDPI.Text;
                    }
                }
                else
                {
                    cui_menor = txtCui.Text;
                    depto_emision = "";
                    municipio_emision = "";
                    if (cmbTiposDocumento.Text.Contains("DPI") || cmbTiposDocumento.Text.Contains("CUI"))
                    {
                        depto_emision = cmbDeptoDPI.Text;
                        municipio_emision = cmbMunicipioDPI.Text;
                    }

                    int intLongitud = txtNumeroIdPadre.Text.Length >= 20 ? 20 : txtNumeroIdPadre.Text.Length;
                    identificacion_padre = txtNumeroIdPadre.Text.Substring(0, intLongitud);

                    intLongitud = txtNumeroIdMadre.Text.Length >= 20 ? 20 : txtNumeroIdMadre.Text.Length;
                    identificacion_madre = txtNumeroIdMadre.Text.Substring(0, intLongitud);
                }


                color_ojos = cmbOjos.Text;
                color_cabello = cmbCabello.Text;
                color_tez = cmbTez.Text;
                estatura = txtEstatura.Text;
                padre = txtNombrePadre.Text + " " + txtApellidoPadre.Text;
                madre = txtNombreMadre.Text + " " + txtApellidoMadre.Text;

                //GUATEMALA
                if (cmbPaisSedeEntrega.SelectedValue.Equals("GUATEMALA"))
                {
                    tipo_entrega = "Sede de Entrega";
                    direccion_entrega1 = sedeEstacion.MISION + " EN " + cmbPaisSedeEntrega.Text + ", " + cmbCiudadSedeEntrega.Text;


                }//ESTADOS UNIDOS DE AMÉRICA
                else if (cmbPaisSedeEntrega.SelectedValue.Equals("ESTADOS UNIDOS DE AMÉRICA"))
                {
                    if (cmbPaisSedeEntrega.Visible)
                    {
                        tipo_entrega = "Sede de Entrega";
                        direccion_entrega1 = sedeEstacion.MISION + " DE GUATEMALA EN " + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                    }
                    else
                    {
                        tipo_entrega = "Dirección de Entrega";
                        direccion_entrega1 = txtResidenciaEntrega1.Text;
                        direccion_entrega2 = txtResidenciaEntrega2.Text;
                        direccion_entrega3 = cmbCiudadEntrega.Text + ", " + cmbEstadoEntrega.SelectedValue.ToString() + " " + cmbZipCodeEntrega.SelectedValue.ToString();
                    }
                }
                else
                {
                    tipo_entrega = "Sede de Entrega";
                    direccion_entrega1 = sedeEstacion.MISION + " DE GUATEMALA EN " + sedeEstacion.CIUDAD + ", " + sedeEstacion.PAIS;
                }

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
                byte[] imageBytes = Convert.FromBase64String(enrollment.foto);

                DataRow row = dt.NewRow();
                row["foto"] = imageBytes;
                row["no_caso"] = enrollment.no_caso;
                row["tipo_pasaporte"] = enrollment.tipo_pasaporte;
                row["nombres"] = enrollment.nombres;
                row["apellidos"] = enrollment.apellidos;
                row["apellido_casada"] = enrollment.apellido_casada;
                row["direccion"] = enrollment.direccion;
                row["tel_casa"] = enrollment.tel_casa;
                row["tel_trabajo"] = enrollment.tel_trabajo;
                row["tel_celular"] = enrollment.tel_celular;
                row["correo"] = enrollment.correo;
                row["pais"] = enrollment.pais;
                row["sexo"] = enrollment.sexo;
                row["estado_civil"] = enrollment.estado_civil;
                row["nacionalidad"] = enrollment.nacionalidad;
                row["fecha_nacimiento"] = enrollment.fecha_nacimiento;
                row["depto_nacimiento"] = enrollment.depto_nacimiento;
                row["municipio_nacimiento"] = enrollment.muni_nacimiento;
                row["pais_nacimiento"] = enrollment.pais_nacimiento;
                row["identificacion"] = enrollment.identificacion;
                row["depto_emision"] = enrollment.depto_emision;
                row["municipio_emision"] = enrollment.municipio_emision;
                row["color_ojos"] = enrollment.color_ojos;
                row["color_tez"] = enrollment.color_tez;
                row["color_cabello"] = enrollment.color_cabello;
                row["estatura"] = enrollment.estatura;
                row["padre"] = enrollment.padre;
                row["madre"] = enrollment.madre;
                row["sede_entrega"] = enrollment.sede_entrega;
                row["partida_nacimiento"] = enrollment.partida_nacimiento;
                row["libro"] = enrollment.libro;
                row["folio"] = enrollment.folio;
                row["acta"] = enrollment.acta;
                row["pasaporte_autorizado"] = enrollment.pasaporte_autorizado;
                row["identificacion_padre"] = enrollment.identificacion_padre;
                row["identificacion_madre"] = enrollment.identificacion_madre;
                row["autorizado_dgm"] = enrollment.autorizado_dgm;
                row["usuario"] = enrollment.usuario;
                row["estacion"] = enrollment.estacion;
                row["lugar_fecha"] = enrollment.lugar_fecha;
                row["cui_menor"] = enrollment.cui_menor;
                row["tipo_entrega"] = enrollment.tipo_entrega;
                row["direccion_entrega1"] = enrollment.direccion_entrega1;
                row["direccion_entrega2"] = enrollment.direccion_entrega2;
                row["direccion_entrega3"] = enrollment.direccion_entrega3;
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

                string rutaPDF = Path.Combine(Application.StartupPath, "ENROL", "PDFs", "CasoPasaporte_" + enrollment.no_caso.Trim() + ".pdf");                

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
                if (cmbTipoPasaporte.Text.Equals("") == false && cmbTipoPasaporte.Text.Equals(string.Empty) == false)
                {
                    TextBox textBox = (TextBox)sender;
                    textBox.Text = textBox.Text.Trim();
                    textBox.BackColor = (textBox.Text.Equals("") || textBox.Text.Equals(string.Empty)) ? Color.Yellow : Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("TextBoxLeave(). " + ex.Message);
                txtMensaje.Text = "TextBoxLeave(). " + ex.Message;
            }
        }
    }
}
