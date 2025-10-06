using CapaEN;
using ENROLLMENT_V3.Properties;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace ENROLLMENT_V3
{
    public partial class FrmLogin2FA : Form
    {
        private const string DEVICE_API_KEY = "Z3pVbU9oT1pVbW9Yb2pRZlZrWk5aV1E9";
        FUNCIONES funciones = new FUNCIONES();

        public LoginData loginData;
        public EquipoData equipoData;
        public SedeData sedeDataEquipo;

        private string username;
        private string password;
        private int currentStep = 1;

        public FrmLogin2FA()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            currentStep = 1;
            SetStep1Visible();
            lblStepTitle.Text = "Paso 1: Credenciales de usuario";
            lblCodeInfo.Text = "";
            btnNext.Text = "Siguiente";
            btnNext.Enabled = false;
            btnCancel.Enabled = true;
            txtUsername.Focus();
        }

        private void SetStep1Visible()
        {
            // Step 1 controls
            lblUsername.Visible = true;
            txtUsername.Visible = true;
            lblPassword.Visible = true;
            txtPassword.Visible = true;

            // Step 2 controls
            lblCode.Visible = false;
            txtCode.Visible = false;
            lblCodeInfo.Visible = false;
        }

        private void SetStep2Visible()
        {
            // Step 1 controls
            lblUsername.Visible = false;
            txtUsername.Visible = false;
            lblPassword.Visible = false;
            txtPassword.Visible = false;

            // Step 2 controls
            lblCode.Visible = true;
            txtCode.Visible = true;
            lblCodeInfo.Visible = true;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            ValidateStep1();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateStep1();
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = !string.IsNullOrWhiteSpace(txtCode.Text.Trim());
        }

        private void ValidateStep1()
        {
            btnNext.Enabled = !string.IsNullOrWhiteSpace(txtUsername.Text.Trim()) &&
                              !string.IsNullOrWhiteSpace(txtPassword.Text);
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                if (currentStep == 1)
                {
                    await ValidateCredentialsAndRequestCode();
                }
                else if (currentStep == 2)
                {
                    await VerifyCodeAndCompleteLogin();
                }
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error: " + ex.Message);
                btnNext.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private async System.Threading.Tasks.Task ValidateCredentialsAndRequestCode()
        {
            username = txtUsername.Text.Trim();
            password = txtPassword.Text;

            // 1. Validar credenciales primero
            DataSet dsLogin = await ConsultaInformacionxUsuario(username, password);
            if (!bool.Parse(dsLogin.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception("Usuario o contraseña incorrectos");

            LoginResponse loginResponse = (LoginResponse)dsLogin.Tables[0].Rows[0]["DATOS"];
            if (loginResponse.data.Length < 1)
                throw new Exception("Usuario o contraseña incorrectos");

            loginData = loginResponse.data[0];

            if (loginData.STATUS.ToString().Equals("0"))
                throw new Exception("Usuario inactivo");

            // 2. Solicitar código 2FA
            DataSet dsCode = await Request2FACode(username);
            if (!bool.Parse(dsCode.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsCode.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            dynamic response = dsCode.Tables[0].Rows[0]["DATOS"];
            string deliveryInfo = response.data[0].delivery + ": " + response.data[0].to;

            // 3. Cambiar a paso 2
            currentStep = 2;
            SetStep2Visible();
            txtUsername.Enabled = false;
            txtPassword.Enabled = false;
            lblCodeInfo.Text = "Código enviado a " + deliveryInfo;
            lblStepTitle.Text = "Paso 2: Verificar código";
            btnNext.Text = "Verificar";
            btnNext.Enabled = false;
            txtCode.Focus();
            btnCancel.Enabled = true;
        }

        private async System.Threading.Tasks.Task VerifyCodeAndCompleteLogin()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
                throw new Exception("Ingrese el código de verificación");

            // Verificar código 2FA
            DataSet dsVerify = await Verify2FACode(username, txtCode.Text.Trim());
            if (!bool.Parse(dsVerify.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsVerify.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            dynamic verifyResponse = dsVerify.Tables[0].Rows[0]["DATOS"];
            if (verifyResponse.data[0].verified != true)
                throw new Exception("Código inválido o expirado");

            // Ya tenemos loginData del paso 1, obtener equipoData y sedeData
            DataSet dsBios = funciones.GetBios();
            if (!bool.Parse(dsBios.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsBios.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            string bios = dsBios.Tables[0].Rows[0]["DATOS"].ToString();
            loginData.biosestacion = bios;

            DataSet dsEquipo = await GetEquipoByBios(bios);
            if (!bool.Parse(dsEquipo.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsEquipo.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            equipoData = (EquipoData)dsEquipo.Tables[0].Rows[0]["DATOS"];

            DataSet dsSede = await GetSedeById(equipoData.sede_id);
            if (!bool.Parse(dsSede.Tables[0].Rows[0]["RESULTADO"].ToString()))
                throw new Exception(dsSede.Tables[0].Rows[0]["MSG_ERROR"].ToString());

            sedeDataEquipo = (SedeData)dsSede.Tables[0].Rows[0]["DATOS"];

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async System.Threading.Tasks.Task<DataSet> ConsultaInformacionxUsuario(string usuario, string password)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                LoginRequest request = new LoginRequest();
                request.username = usuario;
                request.password = password;

                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest = WebRequest.Create(Settings.Default.API_REST_MIROS + Settings.Default.API_LOGIN_USUARIO) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);
                webRequest.ContentLength = data.Length;

                Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(body);

                if (loginResponse.codigo != 200)
                    throw new Exception("Error en login: " + loginResponse.codigo + ", Mensaje: " + loginResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = loginResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ConsultaInformacionxUsuario(): " + ex.Message;
            }

            return dsResultado;
        }

        private async System.Threading.Tasks.Task<DataSet> Request2FACode(string username)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                var request = new { username = username };
                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest = WebRequest.Create(Settings.Default.API_REST_MIROS + "/core/auth/request-2fa-code") as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);
                webRequest.ContentLength = data.Length;

                Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                dynamic codeResponse = JsonConvert.DeserializeObject(body);

                if (codeResponse.codigo != 200)
                    throw new Exception("Error al solicitar código: " + codeResponse.codigo + ", Mensaje: " + codeResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = codeResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "Request2FACode(): " + ex.Message;
            }

            return dsResultado;
        }

        private async System.Threading.Tasks.Task<DataSet> Verify2FACode(string username, string code)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                var request = new { username = username, code = code };
                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest = WebRequest.Create(Settings.Default.API_REST_MIROS + "/core/auth/verify-2fa-code") as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);
                webRequest.ContentLength = data.Length;

                Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                dynamic verifyResponse = JsonConvert.DeserializeObject(body);

                if (verifyResponse.codigo != 200)
                    throw new Exception("Error al verificar código: " + verifyResponse.codigo + ", Mensaje: " + verifyResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = verifyResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "Verify2FACode(): " + ex.Message;
            }

            return dsResultado;
        }

        private async System.Threading.Tasks.Task<DataSet> GetEquipoByBios(string bios)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                HttpWebRequest webRequest = WebRequest.Create(Settings.Default.API_REST_MIROS + Settings.Default.API_EQUIPO_BY_BIOS + "?bios=" + bios) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "GET";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                EquipoResponse equipoResponse = JsonConvert.DeserializeObject<EquipoResponse>(body);

                if (equipoResponse.codigo != 200)
                    throw new Exception("Error al obtener equipo: " + equipoResponse.codigo + ", Mensaje: " + equipoResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = equipoResponse.data;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetEquipoByBios(): " + ex.Message;
            }

            return dsResultado;
        }

        private async System.Threading.Tasks.Task<DataSet> GetSedeById(int sedeId)
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                HttpWebRequest webRequest = WebRequest.Create(Settings.Default.API_REST_MIROS + Settings.Default.API_SEDE_BY_ID + "?id=" + sedeId) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "GET";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                SedeResponse sedeResponse = JsonConvert.DeserializeObject<SedeResponse>(body);

                if (sedeResponse.codigo != 200)
                    throw new Exception("Error al obtener sede: " + sedeResponse.codigo + ", Mensaje: " + sedeResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = sedeResponse.data;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "GetSedeById(): " + ex.Message;
            }

            return dsResultado;
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

        private void txtUsername_Enter(object sender, EventArgs e)
        {
            txtUsername.SelectAll();
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            txtPassword.SelectAll();
        }

        private void txtCode_Enter(object sender, EventArgs e)
        {
            txtCode.SelectAll();
        }
    }
}