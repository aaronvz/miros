using CapaEN;
using ENROLLMENT_V3.Properties;
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.Devices;
using Neurotec.Images;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ENROLLMENT_V3
{
    public partial class FrmResetPasswordHuella : Form
    {
        FUNCIONES funciones = new FUNCIONES();
        private NBiometricClient biometricClientFinger;
        private int currentStep = 1;
        private string resetToken;
        private string userName;
        private bool fingerprintCaptured = false;
        private string capturedFingerprint;

        public FrmResetPasswordHuella()
        {
            InitializeComponent();
            InitializeForm();
        }

        public FrmResetPasswordHuella(NBiometricClient biometricClient)
        {
            InitializeComponent();
            this.biometricClientFinger = biometricClient;
            InitializeForm();
        }

        private void InitializeForm()
        {
            SetStep1Visible();
            lblTitle.Text = "RESTABLECIMIENTO DE CONTRASEÑA";
            lblStepTitle.Text = "Paso 1: Usuario y captura de huella";
            btnNext.Text = "Solicitar Token";
            btnNext.Enabled = false;
            btnBack.Visible = false;
            txtUsuario.Text = string.Empty;
            txtNewPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
            cmbEscaners.Enabled = false;

            progressBar.Value = 50;
            lblProgress.Text = "Paso 1 de 2";

            // Load fingerprint scanners on initialization
            if (biometricClientFinger != null)
            {
                LoadFingerScanners();
            }
        }

        private void SetStep1Visible()
        {
            // Step 1 controles
            lblUsername.Visible = false; // Ocultado porque lblFingerScan ahora actúa como label de Usuario
            txtUsuario.Visible = true;

            // Captura de huella controles
            lblFingerScan.Visible = true; 
            panelFingerView.Visible = true;
            cmbEscaners.Visible = true;
            lblScanner.Visible = true;

            // Step 2/3 controles
            lblToken.Visible = false;
            txtToken.Visible = false;
            lblNewPassword.Visible = false;
            txtNewPassword.Visible = false;
            lblConfirmPassword.Visible = false;
            txtConfirmPassword.Visible = false;
        }

        private void SetStep2Visible()
        {
            // Paso 1 controles
            lblUsername.Visible = false; // Siempre oculto
            txtUsuario.Visible = false;

            // Controles de huella
            lblFingerScan.Visible = false;
            panelFingerView.Visible = false;
            cmbEscaners.Visible = false;
            lblScanner.Visible = false;

            // Paso dos, establecer contraseña nueva
            lblToken.Visible = false;
            txtToken.Visible = false;
            lblNewPassword.Visible = true;
            txtNewPassword.Visible = true;
            lblConfirmPassword.Visible = true;
            txtConfirmPassword.Visible = true;
        }


        private async void btnNext_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            btnBack.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                switch (currentStep)
                {
                    case 1:
                        await ProcessStep1();
                        break;
                    case 2:
                        await ProcessStep2();
                        break;
                }
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error: " + ex.Message);
            }
            finally
            {
                btnNext.Enabled = true;
                btnBack.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private async Task ProcessStep1()
        {
            // hacer el request con huella capturada
            await RequestTokenWithFingerprint();
        }


        private async Task RequestTokenWithFingerprint()
        {
            string msgError = string.Empty;

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                msgError += "Ingrese un usuario válido. ";

            if (!fingerprintCaptured || string.IsNullOrEmpty(capturedFingerprint))
                msgError += "Debe capturar la huella dactilar primero. ";

            if (!string.IsNullOrEmpty(msgError))
                throw new Exception(msgError);

            userName = txtUsuario.Text.Trim();

            DataSet dsResult = await RequestResetToken(userName);
            if (!bool.Parse(dsResult.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsResult.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            ResetTokenResponse response = (ResetTokenResponse)dsResult.Tables[0].Rows[0]["DATOS"];
            if (response.data != null && response.data.Length > 0)
            {
                resetToken = response.data[0].token;
            }

            currentStep = 2;
            SetStep2Visible();
            lblStepTitle.Text = "Paso 2: Nueva contraseña";
            btnNext.Text = "Finalizar";
            btnBack.Visible = true;
            progressBar.Value = 100;
            lblProgress.Text = "Paso 2 de 2";
        }

        private async Task ProcessStep2()
        {
            // Paso final, nuevo password
            string msgError = string.Empty;

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
                msgError += "Ingrese la nueva contraseña. ";

            if (string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
                msgError += "Confirme la nueva contraseña. ";

            if (txtNewPassword.Text != txtConfirmPassword.Text)
                msgError += "Las contraseñas no coinciden. ";

            if (txtNewPassword.Text.Length < 6)
                msgError += "La contraseña debe tener al menos 6 caracteres. ";

            if (!string.IsNullOrEmpty(msgError))
                throw new Exception(msgError);

            DataSet dsReset = await ResetPassword(resetToken, txtNewPassword.Text);
            if (!bool.Parse(dsReset.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsReset.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            funciones.CajaMensaje("Contraseña restablecida exitosamente.");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                switch (currentStep)
                {
                    case 2:
                        currentStep = 1;
                        SetStep1Visible();
                        lblStepTitle.Text = "Paso 1: Usuario y captura de huella";
                        btnBack.Visible = false;
                        btnNext.Text = "Solicitar Token";
                        // Habilitar boton solo si ambos usuario y huella están listos
                        btnNext.Enabled = !string.IsNullOrWhiteSpace(txtUsuario.Text.Trim()) && fingerprintCaptured;
                        progressBar.Value = 50;
                        lblProgress.Text = "Paso 1 de 2";
                        break;
                }
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async Task<DataSet> RequestResetToken(string usernameOrEmail)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ResetTokenRequest request = new ResetTokenRequest();
                request.usernameOrEmail = usernameOrEmail;

                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest;
                string url = Settings.Default.API_REST_MIROS + "/core/auth/request-reset-footprint";

                // DEBUG: Mostrar URL y datos que se envían
                //funciones.CajaMensaje($"DEBUG - URL: {url}\nJSON Body: {postString}\nAPI Key: Z3pVbU9oT1pVbW9Yb2pRZlZrWk5aV1E9");

                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", "Z3pVbU9oT1pVbW9Yb2pRZlZrWk5aV1E9");
                webRequest.ContentLength = data.Length;

                Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                // DEBUG: Mostrar respuesta completa
                //funciones.CajaMensaje($"DEBUG - Response Status: {response.StatusCode}\nResponse Body: {body}");

                ResetTokenResponse resetResponse = JsonConvert.DeserializeObject<ResetTokenResponse>(body);

                if (resetResponse.codigo != 200)
                    throw new Exception("Error al solicitar token: " + resetResponse.codigo + ", Mensaje: " + resetResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = resetResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (WebException webEx)
            {
                // DEBUG: Capturar respuesta de error HTTP
                if (webEx.Response != null)
                {
                    using (StreamReader errorReader = new StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string errorBody = errorReader.ReadToEnd();
                        funciones.CajaMensaje($"DEBUG - WebException Status: {webEx.Status}\nError Response: {errorBody}");
                    }
                }
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "RequestResetToken(): " + webEx.Message;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "RequestResetToken(): " + ex.Message;
            }

            return dsResultado;
        }


        private async Task<DataSet> ResetPassword(string token, string newPassword)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ResetPasswordRequest request = new ResetPasswordRequest();
                request.token = token;
                request.newPassword = newPassword;

                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest;
                string url = Settings.Default.API_REST_MIROS + Settings.Default.API_RESET_PASSWORD;
                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.ContentLength = data.Length;

                Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                ResetPasswordResponse resetResponse = JsonConvert.DeserializeObject<ResetPasswordResponse>(body);

                if (resetResponse.codigo != 200)
                    throw new Exception("Error al restablecer contraseña: " + resetResponse.codigo + ", Mensaje: " + resetResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = resetResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ResetPassword(): " + ex.Message;
            }

            return dsResultado;
        }

        private void LoadFingerScanners()
        {
            try
            {
                if (biometricClientFinger != null)
                {
                    DataSet ds = funciones.ListarEscanersHuellas(cmbEscaners, true, Settings.Default.FILTRAR_ESCANER_HUELLAS, PARAMETRIZACION.TipoEscanerHuellas.Unidactilar, biometricClientFinger.DeviceManager);
                    bool resultado = bool.Parse(ds.Tables[0].Rows[0]["RESULTADO"].ToString());
                    if (!resultado)
                        throw new Exception(ds.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                    cmbEscaners.Enabled = true;
                    if (cmbEscaners.Items.Count > 1)
                    {
                        cmbEscaners.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error al cargar escáneres: " + ex.Message);
            }
        }

        private async void cmbEscaners_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                funciones.MostrarHuellaDesdeBytes(null, nFVFinger);

                biometricClientFinger.FingerScanner = cmbEscaners.SelectedItem as NFScanner;

                DataSet dsEscanerHuella = await funciones.EscanearHuella(NFPosition.RightIndex, nFVFinger, biometricClientFinger);
                if (bool.Parse(dsEscanerHuella.Tables[0].Rows[0]["RESULTADO"].ToString()) == false)
                    throw new Exception(dsEscanerHuella.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                // marcar que la huella fue capturada
                fingerprintCaptured = true;
                if (nFVFinger.Finger != null && nFVFinger.Finger.Image != null)
                {
                    capturedFingerprint = Convert.ToBase64String(nFVFinger.Finger.Image.Save(NImageFormat.Wsq).ToArray());
                }

                // habiliar boton solo si ambos usuario y huella están listos
                btnNext.Enabled = !string.IsNullOrWhiteSpace(txtUsuario.Text.Trim()) && fingerprintCaptured;
            }
            catch(Exception ex)
            {
                // MessageBox.Show("CmbEscaners_SelectedIndexChanged(). " + ex.Message);
                funciones.CajaMensaje(ex.Message);
                btnNext.Enabled = false;
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

        private void FrmResetPasswordHuella_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (biometricClientFinger != null) biometricClientFinger.Cancel();
        }


        private void txtUsuario_Enter(object sender, EventArgs e)
        {
            txtUsuario.SelectAll();
        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {
            // habilitar boton solo si ambos usuario y huella están listos
            btnNext.Enabled = !string.IsNullOrWhiteSpace(txtUsuario.Text.Trim()) && fingerprintCaptured;
        }

        private void txtToken_Enter(object sender, EventArgs e)
        {
            txtToken.SelectAll();
        }

        private void txtNewPassword_Enter(object sender, EventArgs e)
        {
            txtNewPassword.SelectAll();
        }

        private void txtConfirmPassword_Enter(object sender, EventArgs e)
        {
            txtConfirmPassword.SelectAll();
        }
    }
}