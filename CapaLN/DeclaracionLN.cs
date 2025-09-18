using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaAD;
using CapaEN;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace CapaLN
{
    public class DeclaracionLN
    {
        private string url;
        private bool resultado;
        private string error;

        private LoginData loginData;
        private DeclaracionData declaracionData;
        private string declaracionToken;

        public DeclaracionLN(string _url, LoginData _loginData, string _declaracionToken)
        {
            url = _url;
            loginData = _loginData;
            declaracionToken = _declaracionToken;

            error = string.Empty;
            resultado = false;
        }

        public bool GetByLibreta(string libreta, string fechaNac, string tipoMov, string nacionalidad, string fechaMov, int delegacion)
        {
            resultado = false;
            try
            {
                DeclaracionByLibretaRequest declaracionRequest = new DeclaracionByLibretaRequest();
                declaracionRequest.libreta = libreta;
                declaracionRequest.fechaNac = fechaNac;
                declaracionRequest.tipoMov = tipoMov;
                declaracionRequest.nacionalidad = nacionalidad;
                declaracionRequest.fechaMov = fechaMov;
                declaracionRequest.delegacion = delegacion;

                DeclaracionAD declaracionAD = new DeclaracionAD(url, loginData, declaracionToken);

                if (!declaracionAD.GetByLibreta(declaracionRequest))
                    throw new Exception(declaracionAD.GetError());

                declaracionData = declaracionAD.GetData();

                resultado = true;
            }
            catch (Exception ex)
            {

                error = "GetByLibreta(). " + ex.Message;
            }

            return resultado;
        }

        public bool GetByNumero(string numero)
        {
            resultado = false;
            try
            {
                DeclaracionByNumeroRequest declaracionRequest = new DeclaracionByNumeroRequest();
                declaracionRequest.numeroDeclaracion = numero;

                DeclaracionAD declaracionAD = new DeclaracionAD(url, loginData, declaracionToken);

                if (!declaracionAD.GetByNumero(declaracionRequest))
                    throw new Exception(declaracionAD.GetError());

                declaracionData = declaracionAD.GetData();

                resultado = true;
            }
            catch (Exception ex)
            {

                error = "GetByLibreta(). " + ex.Message;
            }

            return resultado;
        }

        public void SetUrl(string _url)
        { this.url = _url; }

        public DeclaracionData GetData()
        { return this.declaracionData; }
        public string GetError()
        { return this.error; }

    }
}
