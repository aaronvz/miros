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
using System.Diagnostics;

using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Licensing;


namespace ENROLLMENT_V3
{
    public partial class Bienvenida : Form
    {
        public Bienvenida()
        {
            InitializeComponent();
           
        }
        private NBiometricClient _biometricClient;

        private void Ingreso_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Bienvenida_Load(object sender, EventArgs e)
        {
            //_biometricClient = new NBiometricClient { UseDeviceManager = true, BiometricTypes = NBiometricType.Finger };

            //await _biometricClient.InitializeAsync();

            //MessageBox.Show("Dispositivos encontrados: " + _biometricClient.DeviceManager.Devices.Count);
            
            FUNCIONES funciones = new FUNCIONES();
            funciones.CargarLogo(pic_txt_dgm);
        }

        private async void BtnAccion_Click(object sender, EventArgs e)
        {
            lblListo.Text = await ValidarLicencias();
        }

        private void Bienvenida_Shown(object sender, EventArgs e)
        {
            try
            {
                Process[] proc = Process.GetProcessesByName("ENROLLMENT_V3");

                if (proc.Length > 1)
                    for(int i = 0; i < proc.Length; i++)
                        proc[i].Kill();                

                proc = Process.GetProcessesByName("ProcesarRostrosNeuro");
                if (proc.Length > 0)
                    for (int i = 0; i < proc.Length; i++)
                        proc[i].Kill();
                else if (proc.Length == 0)
                    InvocarProcesamientoRostro();

                BtnAccion_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bienvenida_Load(). " + ex.Message);
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

        private Task<string> ValidarLicencias()
        {
            return Task.Run(() =>
            {
                try
                {
                    CheckForIllegalCrossThreadCalls = false;

                    string Components = "Biometrics.FingerExtraction,Devices.FingerScanners,Images.WSQ,Biometrics.FingerSegmentation,Biometrics.FingerQualityAssessmentBase";
                    Components += ",Biometrics.FaceExtraction,Biometrics.FaceDetection,Devices.Cameras,Biometrics.FaceSegmentsDetection";

                    //string Components = "FingerClient,";
                    //Components += "FaceClient";

                    //NLicense.ObtainComponents("/local", 5000, Components);

                    //MessageBox.Show("NLicenseManager.TrialMode: " + NLicenseManager.TrialMode);
                    foreach (string component in Components.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                         
                        lblListo.Text = component;
                        //NLicense.ObtainComponents("/local", 5000, component);
                        NLicense.ObtainComponents(Properties.Settings.Default.SERVIDOR_NEURO, 5000, component);
                        lblListo.Text += " -> " + NLicense.IsComponentActivated(component);
                        //NLicense.ObtainComponents(Properties.Settings.Default.SERVIDOR_NEURO, 5000, component);
                    }
                    return "¡Listo!";
                }
                catch (Exception ex)
                {
                    return "ValidarLicencias(). " + ex.Message;
                }
            });
        }

        private void LblListo_TextChanged(object sender, EventArgs e)
        {
            if (lblListo.Text.Equals("¡Listo!"))
            {
                Verificacion verificacion = new Verificacion();
                this.Hide();
                verificacion.ShowDialog();
                this.Close();
            }
        }
    }
}
