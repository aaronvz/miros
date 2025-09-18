using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient; 

using CapaEN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace CapaAD
{     
    public class DeclaracionAD
    { 
        private string url = null;
        private LoginData loginData = null;
        private string declaracionToken = null;

        string error = string.Empty; 

        private DeclaracionData declaracionData;
        public DeclaracionAD(string _url, LoginData _loginData, string _declaracionToken)
        {
            url = _url;
            loginData = _loginData;
            declaracionToken = _declaracionToken;
        }

        public bool GetByLibreta(DeclaracionByLibretaRequest declaracionRequest)
        {
            bool resultado = false;
            try
            { 
                if (url == null)
                    throw new Exception("Url es nulo. ");

                if (loginData == null)
                    throw new Exception("LoginData es nulo. ");

                if (declaracionToken == null)
                    throw new Exception("DeclarlacionData es nulo.");

                string postString = JsonConvert.SerializeObject(declaracionRequest);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + url);
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", $"Bearer {declaracionToken}");

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    DeclaracionResponse declaracionResponse = JsonConvert.DeserializeObject<DeclaracionResponse>(json);

                    if (declaracionResponse.codigo != 200)
                        throw new Exception("Error al guardar la entrega. Código: " + declaracionResponse.codigo + ", Mensaje: " + declaracionResponse.mensaje);

                    if (declaracionResponse.data == null)
                        throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                    declaracionData = declaracionResponse.data;
                    resultado = true;
                }
            }
            catch (Exception ex)
            { 
                error = "GetByLibreta(). " + ex.Message;
            }

            return resultado;
        }

        public bool GetByNumero(DeclaracionByNumeroRequest declaracionRequest)
        {
            bool resultado = false;
            try
            {
                if (url == null)
                    throw new Exception("Url es nulo. ");

                if (loginData == null)
                    throw new Exception("LoginData es nulo. ");

                if (declaracionToken == null)
                    throw new Exception("DeclarlacionData es nulo.");

                string postString = JsonConvert.SerializeObject(declaracionRequest);
                byte[] data = UTF8Encoding.UTF8.GetBytes(postString);

                ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"" + url);
                request.Timeout = 10 * 1000;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", $"Bearer {declaracionToken}");

                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    DeclaracionResponse declaracionResponse = JsonConvert.DeserializeObject<DeclaracionResponse>(json);

                    if (declaracionResponse.codigo != 200)
                        throw new Exception("Error al guardar la entrega. Código: " + declaracionResponse.codigo + ", Mensaje: " + declaracionResponse.mensaje);

                    if (declaracionResponse.data == null)
                        throw new Exception("Error de comunicación del API, intente guardar nuevamente y repórtelo al administrador");

                    declaracionData = declaracionResponse.data;
                    resultado = true;
                }
            }
            catch (Exception ex)
            {
                error = "GetByNumero(). " + ex.Message;
            }

            return resultado;
        }

        public DeclaracionData GetData()
        {
            return this.declaracionData;
        }

        public string GetError()
        {
            return this.error;
        }
    }
}
