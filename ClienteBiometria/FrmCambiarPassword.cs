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
    public partial class FrmCambiarPassword : Form
    {
        private const string DEVICE_API_KEY = "Z3pVbU9oT1pVbW9Yb2pRZlZrWk5aV1E9";
        FUNCIONES funciones = new FUNCIONES();
        private LoginData loginData;

        public FrmCambiarPassword(LoginData _loginData)
        {
            InitializeComponent();
            this.loginData = _loginData;
            InitializeForm();
        }

        private void InitializeForm()
        {
            lblTitle.Text = "CAMBIAR CONTRASEÑA";
            lblUsuario.Text = "Usuario: " + loginData.USUARIO;
            txtNuevaPassword.Text = string.Empty;
            txtConfirmarPassword.Text = string.Empty;
            btnCambiar.Enabled = false;
        }

        private void txtNuevaPassword_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswords();
        }

        private void txtConfirmarPassword_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswords();
        }

        private void ValidatePasswords()
        {
            string msgError = string.Empty;

            if (string.IsNullOrWhiteSpace(txtNuevaPassword.Text))
            {
                lblValidacion.Text = "Ingrese la nueva contraseña";
                lblValidacion.ForeColor = System.Drawing.Color.Orange;
                btnCambiar.Enabled = false;
                return;
            }

            if (txtNuevaPassword.Text.Length < 6)
            {
                lblValidacion.Text = "La contraseña debe tener al menos 6 caracteres";
                lblValidacion.ForeColor = System.Drawing.Color.Red;
                btnCambiar.Enabled = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtConfirmarPassword.Text))
            {
                lblValidacion.Text = "Confirme la nueva contraseña";
                lblValidacion.ForeColor = System.Drawing.Color.Orange;
                btnCambiar.Enabled = false;
                return;
            }

            if (txtNuevaPassword.Text != txtConfirmarPassword.Text)
            {
                lblValidacion.Text = "Las contraseñas no coinciden";
                lblValidacion.ForeColor = System.Drawing.Color.Red;
                btnCambiar.Enabled = false;
                return;
            }

            lblValidacion.Text = "Contraseñas válidas";
            lblValidacion.ForeColor = System.Drawing.Color.Green;
            btnCambiar.Enabled = true;
        }

        private async void btnCambiar_Click(object sender, EventArgs e)
        {
            btnCambiar.Enabled = false;
            btnCancelar.Enabled = false;

            try
            {
                DataSet dsResult = await ChangePassword();
                if (!bool.Parse(dsResult.Tables[0].Rows[0]["RESULTADO"].ToString()))
                    throw new Exception(dsResult.Tables[0].Rows[0]["MSG_ERROR"].ToString());

                funciones.CajaMensaje("Contraseña actualizada correctamente.");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                funciones.CajaMensaje("Error: " + ex.Message);
                btnCambiar.Enabled = true;
                btnCancelar.Enabled = true;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async System.Threading.Tasks.Task<DataSet> ChangePassword()
        {
            DataSet dsResultado = ArmarDsResultado();
            try
            {
                ChangePasswordRequest request = new ChangePasswordRequest();
                request.username = loginData.USUARIO;
                request.newPassword = txtNuevaPassword.Text;

                string postString = JsonConvert.SerializeObject(request);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest webRequest;
                string url = Settings.Default.API_REST_MIROS + "/core/auth/reset-password-device";
                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Timeout = 10 * 1000;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Headers.Add("x-device-api-key", DEVICE_API_KEY);
                webRequest.ContentLength = data.Length;

                System.IO.Stream postStream = webRequest.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();

                ChangePasswordResponse changeResponse = JsonConvert.DeserializeObject<ChangePasswordResponse>(body);

                if (changeResponse.codigo != 200)
                    throw new Exception("Error al cambiar contraseña: " + changeResponse.codigo + ", Mensaje: " + changeResponse.mensaje);

                dsResultado.Tables[0].Rows[0]["DATOS"] = changeResponse;
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = true;
            }
            catch (Exception ex)
            {
                dsResultado.Tables[0].Rows[0]["RESULTADO"] = false;
                dsResultado.Tables[0].Rows[0]["MSG_ERROR"] = "ChangePassword(): " + ex.Message;
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

        public class ChangePasswordRequest
        {
            public string username { get; set; }
            public string newPassword { get; set; }
        }

        public class ChangePasswordResponse
        {
            public int codigo { get; set; }
            public string mensaje { get; set; }
        }
    }
}