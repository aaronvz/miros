using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

using CapaAD;
using CapaEN.TextNotificationEN;
using CapaEN;

using Newtonsoft.Json;


namespace CapaLN
{
    public class TextNotificationLN
    {
        private string mode;

        private bool result;
        public String error { get; set; }

        public TextNotificationLN(string _mode)
        {
            mode = _mode;

            result = false;
            error = string.Empty;
        }

        public bool Send(string url, string msisdnTemp, string body_sms)
        {
            result = false;
            try
            {
                string msisdnFiltrado = string.Empty;
                for (int i = 0; i < msisdnTemp.Length; i++)
                    if (Char.IsDigit(msisdnTemp.ElementAt(i)))
                        msisdnFiltrado += msisdnTemp.ElementAt(i);

                if(msisdnFiltrado.Length == 0)
                    throw new Exception("Número incorrecto (" + msisdnFiltrado + "). ");

                CredentialAD credential = new CredentialAD();
                credential.GetTextNotification(mode);
                
                RequestParameters requestParameters = new RequestParameters();
                requestParameters.msisdn = long.Parse(msisdnFiltrado);
                requestParameters.body_sms = body_sms;

                string postString = JsonConvert.SerializeObject(requestParameters);

                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                HttpWebRequest request;
                request = WebRequest.Create(@"" + url) as HttpWebRequest;
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";

                CredentialAD credentialAD = new CredentialAD();
                CredentialEN credentialEN = credentialAD.GetTextNotification(mode);
                if (!credentialEN.resultado)
                    throw new Exception(credentialEN.error);

                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentialEN.user}:{credentialEN.password}"));
                request.Headers.Add("Authorization", "Basic " + credentials);

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string body = reader.ReadToEnd();

                ResponseParameters responseParameters;
                try
                {
                    responseParameters = JsonConvert.DeserializeObject<ResponseParameters>(body);
                }
                catch (Exception)
                {

                    throw new Exception("Error al convertir el respuesta de servicio de envío de mensajes de texto: " + body);
                }
                
                
                if (!(responseParameters.codigo == 200))
                    throw new Exception("Error, código: " + responseParameters.codigo + ", mensaje: " + responseParameters.mensaje);

                result = true;                                
            }
            catch (Exception ex)
            {
                error = this.GetType().FullName +".Send(). " + ex.Message;
            }

            return result;
        }
    }
}
