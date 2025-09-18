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

namespace CapaAD
{     
    public class SolicitudAD
    {
        private bool resultado;    
        private string error;
        private int noSolicitud;
        private string estadoSolicitud;

        private SolicitudEN solicitudEN;
        private SolicitudEN fotografia;
        private SolicitudEN huellas;

        private SqlConnection sqlConnection;

        public SolicitudAD(SqlConnection _SqlConnection)
        {
            sqlConnection = _SqlConnection;
            noSolicitud = -1;
            estadoSolicitud = "-1";
            solicitudEN = null;
            resultado = false;
        }        

        public bool ExisteLibreta(string _pNoLibreta)
        {
            resultado = false;
            try
            {
                if (sqlConnection == null)
                    throw new Exception("El objeto de conexión es nulo. ");

                if (sqlConnection.State != ConnectionState.Open)
                    throw new Exception("Estado de conexión incorrecto (" + sqlConnection.State + "). ");

                string commandText = "SELECT TOP 1 id FROM solicitud s WHERE s.numero_libreta = @pNoLibreta;";
                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoLibreta", SqlDbType.NVarChar));
                command.Parameters["@pNoLibreta"].Value = _pNoLibreta;

                var v = command.ExecuteScalar();                                    
                noSolicitud = v == null ? -1 : int.Parse(v.ToString());

                resultado = true;
            }catch(Exception ex)
            {
                error = "ExisteLibreta(). " + ex.Message;
            }
            return resultado;
        }

        public int GetNoSolicitud()
        {
            return noSolicitud;
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

                string commandText = "SELECT TOP 1 s.estado FROM solicitud s WHERE s.id = @pNoSolicitud;";
                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoSolicitud", SqlDbType.NVarChar));
                command.Parameters["@pNoSolicitud"].Value = _pNoSolicitud;

                var v = command.ExecuteScalar();                   
                estadoSolicitud = v == null ? null : v.ToString();

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
            return estadoSolicitud;
        }

        public bool ConsultarDatosBiograficos(int _pNoSolicitud)
        {
            resultado = false;
            try
            {
                if (sqlConnection == null)
                    throw new Exception("Objeto de conexión nulo. ");

                if (sqlConnection.State != ConnectionState.Open)
                    throw new Exception("Estado de conexión incorrecto (" + sqlConnection.State + "). ");

                string commandText = "" +
                    "SELECT " +
                        "TOP 1 s.cui, " +
                        "s.id, " +
                        "s.caso, " +
                        "s.numero_libreta, " +
                        "s.primer_nombre, " +
                        "s.segundo_nombre, " +
                        "s.primer_apellido, " +
                        "s.segundo_apellido, " +
                        "s.apellido_casada, " +
                        "s.fecha_nacimiento, " +
                        "s.correo_electronico, " +
                        "s.telefono_celular, " +
                        "SUBSTRING(s.sexo, 1, 1) sexo, " +
                        "s.estado, " +
                        "s.sede_texto, " +
                        "(SELECT descripcion FROM tipo_pasaporte WHERE id = s.tipo_pasaporte_id) as tipo_pasaporte, " +
                        "(SELECT pa.id FROM pais pa, sede se WHERE pa.id = se.pais_id AND se.id = s.sede_id) as id_pais_sede " +
                    "FROM solicitud s " +
                    "WHERE s.id = @pNoSolicitud";

                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoSolicitud", SqlDbType.NVarChar));
                command.Parameters["@pNoSolicitud"].Value = _pNoSolicitud;

                SqlDataReader sqlDataReader = command.ExecuteReader();
                solicitudEN = null;
                if(sqlDataReader.HasRows)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);

                    //PARA CONVERTIR UN CONJUNTO DE REGISTROS EN JSON USAR ESTA FORMA
                    //string JSONString = string.Empty;
                    //JSONString = JsonConvert.SerializeObject(dataTable);

                    //PARA CONVERTIR UN ÚNICO REGISTRO EN JSON USAR ESTA FORMA
                    string jsonString = new JObject(dataTable.Columns.Cast<DataColumn>().Select(c => new JProperty(c.ColumnName, JToken.FromObject(dataTable.Rows[0][c])))).ToString(Formatting.None);
                    solicitudEN = JsonConvert.DeserializeObject<SolicitudEN>(jsonString);
                }
                    
                resultado = true;
            }
            catch (Exception ex)
            {
                error = "ConsultarDatosBiograficos(). " + ex.Message;
            }
            return resultado;
        }

        public SolicitudEN GetDatosBiograficos()
        {
            return solicitudEN;
        }

        public bool ConsultarFotografia(int _pNoSolicitud)
        {
            resultado = false;
            try
            {
                if (sqlConnection == null)
                    throw new Exception("Objeto de conexión nulo. ");

                if (sqlConnection.State != ConnectionState.Open)
                    throw new Exception("Estado de conexión incorrecto (" + sqlConnection.State + "). ");

                string commandText = "" +
                    "SELECT " +
                        "TOP 1 sm.foto " +                        
                    "FROM  " +
                        "solicitud s INNER JOIN solicitud_imagenes sm ON sm.solicitud_id=s.id  " +
                    "WHERE s.id = @pNoSolicitud";

                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoSolicitud", SqlDbType.NVarChar));
                command.Parameters["@pNoSolicitud"].Value = _pNoSolicitud;

                SqlDataReader sqlDataReader = command.ExecuteReader();
                solicitudEN = null;
                if (sqlDataReader.HasRows)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);

                    //PARA CONVERTIR UN CONJUNTO DE REGISTROS EN JSON USAR ESTA FORMA
                    //string JSONString = string.Empty;
                    //JSONString = JsonConvert.SerializeObject(dataTable);

                    //PARA CONVERTIR UN ÚNICO REGISTRO EN JSON USAR ESTA FORMA
                    string jsonString = new JObject(dataTable.Columns.Cast<DataColumn>().Select(c => new JProperty(c.ColumnName, JToken.FromObject(dataTable.Rows[0][c])))).ToString(Formatting.None);
                    fotografia = JsonConvert.DeserializeObject<SolicitudEN>(jsonString);
                }

                resultado = true;
            }
            catch (Exception ex)
            {
                error = "ConsultarFotografia(). " + ex.Message;
            }
            return resultado;
        }

        public string GetFotografia()
        {
            if (fotografia == null)
                return null;

            return fotografia.foto;
        }

        public bool ConsultarHuellas(int _pNoSolicitud)
        {
            resultado = false;
            try
            {
                if (sqlConnection == null)
                    throw new Exception("Objeto de conexión nulo. ");

                if (sqlConnection.State != ConnectionState.Open)
                    throw new Exception("Estado de conexión incorrecto (" + sqlConnection.State + "). ");

                string commandText = "" +
                    "SELECT " +
                        "TOP 1 sm.huella_png1, " +
                        "sm.huella_png2, " +
                        "sm.huella_pos1, " +
                        "sm.huella_pos2, " +
                        "sm.huella_wsq1, " +
                        "sm.huella_wsq2 " +
                    "FROM  " +
                        "solicitud s INNER JOIN solicitud_imagenes sm ON sm.solicitud_id=s.id  " +
                    "WHERE s.id = @pNoSolicitud";

                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                command.Parameters.Add(new SqlParameter("@pNoSolicitud", SqlDbType.NVarChar));
                command.Parameters["@pNoSolicitud"].Value = _pNoSolicitud;

                SqlDataReader sqlDataReader = command.ExecuteReader();
                solicitudEN = null;
                if (sqlDataReader.HasRows)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);

                    //PARA CONVERTIR UN CONJUNTO DE REGISTROS EN JSON USAR ESTA FORMA
                    //string JSONString = string.Empty;
                    //JSONString = JsonConvert.SerializeObject(dataTable);

                    //PARA CONVERTIR UN ÚNICO REGISTRO EN JSON USAR ESTA FORMA
                    string jsonString = new JObject(dataTable.Columns.Cast<DataColumn>().Select(c => new JProperty(c.ColumnName, JToken.FromObject(dataTable.Rows[0][c])))).ToString(Formatting.None);
                    huellas = JsonConvert.DeserializeObject<SolicitudEN>(jsonString);
                }

                resultado = true;
            }
            catch (Exception ex)
            {
                error = "ConsultarFotografia(). " + ex.Message;
            }
            return resultado;
        }

        public SolicitudEN GetHuellas()
        {
            return huellas;
        }

        public string GetError()
        {
            return error;
        }
    }
}
