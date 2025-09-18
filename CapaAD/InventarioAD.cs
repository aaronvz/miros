using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

using CapaEN;
using Newtonsoft.Json;

namespace CapaAD
{     
    public class InventarioAD
    {
        private bool resultado;    
        private string error;
        private string estadoInventario;

        private SqlConnection sqlConnection;

        public InventarioAD(SqlConnection _SqlConnection)
        {
            sqlConnection = _SqlConnection;
            estadoInventario = "-1";
            resultado = false;
        }                        

        public bool ConsultarEstado(int _pNoSolicitud)
        {
            resultado = false;
            try
            {
                if (sqlConnection == null)
                    throw new Exception("Objeto de conexión nulo. ");

                if (sqlConnection.State != ConnectionState.Open)
                    throw new Exception("Estado de conexión incorrecto (" + sqlConnection.State + "). ");

                string commandText = "SELECT TOP 1 ii.estado FROM inventario_item ii WHERE ii.solicitud_id = @pNoSolicitud AND ii.estado = 'CCalidadOk';";
                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoSolicitud", SqlDbType.NVarChar));
                command.Parameters["@pNoSolicitud"].Value = _pNoSolicitud;

                var v = command.ExecuteScalar();                   
                estadoInventario = v == null ? null : v.ToString();

                resultado = true;
            }
            catch (Exception ex)
            {
                error = "ConsultarEstadoSolicitud(). " + ex.Message;
            }
            return resultado;
        }

        public string GetEstado()
        {
            return estadoInventario;
        }
        
        public string GetError()
        {
            return error;
        }
    }
}
