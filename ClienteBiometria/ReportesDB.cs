using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Windows.Forms;

using CapaEN;
using System.Data;

namespace ENROLLMENT_V3
{
    public class MovimientoDiagnostico
    {
        public int id_consumo_servicio { get; set; }
        public string fecha { get; set; }
        public string hora { get; set; }
        public string id_movimiento { get; set; }
        public string tipo_documento { get; set; }
        public string numero_documento { get; set; }
        public string nombre { get; set; }
        public string fecha_nacimiento { get; set; }
        public string comando { get; set; }
        public double segundos { get; set; }
        public DateTime fecha_ini { get; set; }
        public DateTime fecha_fin { get; set; }
        public string request { get; set; }
        public string response { get; set; }
    }
    class ReportesDB
    {
        public string id;
        private string rutaDB = Application.StartupPath + "\\ENROL\\diagnostico\\diagnostico.db";
        private SQLiteConnection sqlite_conn = new SQLiteConnection();
        public string error = string.Empty;
        public ReportesDB()
        {
        }
        public ReportesDB(string _rutaDB)
        {
            rutaDB = _rutaDB;
        }
        public bool CreateConnection()
        {
            error = string.Empty;
            
            // Create a new database connection:
            //sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            sqlite_conn = new SQLiteConnection("Data Source=" + rutaDB + ";Version=3;New=False;");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                error = "CreateConnection(). " + ex.Message;
                return false;
            }
            return true;
        }

        public bool InsertarRegistro(MovimientoDiagnostico movimiento)
        {
            error = string.Empty;

            if(sqlite_conn.State != ConnectionState.Open)
                if(CreateConnection() == false)
                    throw new Exception(error);

            using (var cmd = new SQLiteCommand())
            {
                error = string.Empty;
                cmd.Connection = sqlite_conn;
                try
                {
                    cmd.CommandText = $"" +
                        $"INSERT INTO " +
                        $"consumo_servicio (fecha, hora, id_movimiento, tipo_documento, numero_documento, nombre, fecha_nacimiento, comando, segundos, fecha_ini, fecha_fin, request, response)" +
                        $"VALUES (date(), time('now', 'localtime'), @id_movimiento, @tipo_documento, @numero_documento, @nombre, @fecha_nacimiento, @comando, @segundos, @fecha_ini, @fecha_fin, @request, @response)";

                    cmd.Parameters.Add("@id_movimiento", DbType.String).Value = movimiento.id_movimiento;
                    cmd.Parameters.Add("@tipo_documento", DbType.String).Value = movimiento.tipo_documento;
                    cmd.Parameters.Add("@numero_documento", DbType.String).Value = movimiento.numero_documento;
                    cmd.Parameters.Add("@nombre", DbType.String).Value = movimiento.nombre;

                    //string fechaNacimiento = movimiento.fecha_nacimiento.Split('/')[2] + "-" + movimiento.fecha_nacimiento.Split('/')[1] + "-" + movimiento.fecha_nacimiento.Split('/')[0];
                    cmd.Parameters.Add("@fecha_nacimiento", DbType.String).Value = string.Empty;

                    cmd.Parameters.Add("@comando", DbType.String).Value = movimiento.comando;

                    TimeSpan timeSpan = movimiento.fecha_fin - movimiento.fecha_ini;
                    movimiento.segundos = timeSpan.TotalSeconds;

                    cmd.Parameters.Add("@segundos", DbType.Double).Value = movimiento.segundos;

                    string fecha = movimiento.fecha_ini.ToString("dd/MM/yyyy");
                    cmd.Parameters.Add("@fecha_ini", DbType.String).Value = movimiento.fecha_ini.ToString("yyyy-MM-dd HH:mm:ss.ffff");

                    fecha = movimiento.fecha_fin.ToString("dd/MM/yyyy");
                    cmd.Parameters.Add("@fecha_fin", DbType.String).Value = movimiento.fecha_fin.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    cmd.Parameters.Add("@request", DbType.String).Value = movimiento.request;
                    cmd.Parameters.Add("@response", DbType.String).Value = movimiento.response;

                    cmd.ExecuteNonQuery();
                }

                catch (Exception ex)
                {
                    error = "InsertarCaso(). " + ex.Message;
                    sqlite_conn.Close();
                    return false;
                }
                sqlite_conn.Close();
            }
            
            return true;
        }

        public DataTable GetVuelosByFecha(string usuario, string sede, string fecha)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("descripcion_vuelo", typeof(string));
            
            error = string.Empty;

            if (sqlite_conn.State != ConnectionState.Open)
                if (CreateConnection() == false)
                    throw new Exception(error);

            SQLiteDataAdapter ad;
            using (var cmd = new SQLiteCommand())
            {
                error = string.Empty;
                cmd.Connection = sqlite_conn;
                try
                {
                    cmd.CommandText = "" +
                        //"SELECT 'TODOS' descripcion_vuelo" + Environment.NewLine +
                        //"UNION ALL" + Environment.NewLine +
                        "SELECT DISTINCT(descripcion_vuelo)  " + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" + Environment.NewLine +
                        "	usuario = @usuario AND sede_captura = @sede AND fecha = @fecha" + Environment.NewLine +
                        "ORDER BY descripcion_vuelo DESC" + Environment.NewLine +
                        "" +
                        "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;

                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                }

                catch (Exception ex)
                {
                    error = "GetVuelosByFecha(). " + ex.Message;
                    sqlite_conn.Close();
                    return null;
                }
                sqlite_conn.Close();
            }

            return dt;
        }

        public DataTable ResumenCasosDiario(string usuario, string sede, string fecha)
        {
            DataTable dtResumen = new DataTable();
            dtResumen.Columns.Add("hora_primer_caso", typeof(string));
            dtResumen.Columns.Add("identificacion_primer_caso", typeof(string));
            dtResumen.Columns.Add("hora_ultimo_caso", typeof(string));
            dtResumen.Columns.Add("identificacion_ultimo_caso", typeof(string));
            dtResumen.Columns.Add("total", typeof(Int32));
            DataRow dr = dtResumen.NewRow();

            error = string.Empty;

            if (sqlite_conn.State != ConnectionState.Open)
                if (CreateConnection() == false)
                    throw new Exception(error);

            SQLiteDataAdapter ad;
            DataTable dt = new DataTable();

            using (var cmd = new SQLiteCommand())
            {
                error = string.Empty;
                cmd.Connection = sqlite_conn;
                try
                {
                    cmd.CommandText = "" +
                        "SELECT " + Environment.NewLine +
                        "	hora," + Environment.NewLine +
                        "	identificacion" + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" + Environment.NewLine +
                        "	id_caso = (SELECT MAX(id_caso) FROM casos WHERE usuario = @usuario AND sede_captura = @sede AND fecha = @fecha)" + Environment.NewLine +
                        "ORDER BY id_caso DESC" + Environment.NewLine +
                        "" +
                        "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;


                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                    if(dt.Rows.Count > 0)
                    {
                        dr["hora_ultimo_caso"] = dt.Rows[0]["hora"];
                        dr["identificacion_ultimo_caso"] = dt.Rows[0]["identificacion"];
                    }

                    cmd.CommandText = "" +
                        "SELECT " + Environment.NewLine +
                        "	hora," + Environment.NewLine +
                        "	identificacion" + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" + Environment.NewLine +
                        "	id_caso = (SELECT MIN(id_caso) FROM casos WHERE usuario = @usuario AND sede_captura = @sede AND fecha = @fecha)" + Environment.NewLine +
                        "ORDER BY id_caso DESC" + Environment.NewLine +
                        "" +
                        "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;

                    dt = new DataTable();
                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                    if (dt.Rows.Count > 0)
                    {
                        dr["hora_primer_caso"] = dt.Rows[0]["hora"];
                        dr["identificacion_primer_caso"] = dt.Rows[0]["identificacion"];
                    }

                    cmd.CommandText = "" +
                        "SELECT " + Environment.NewLine +
                        "	COUNT(id_caso) total" + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" + Environment.NewLine +
                        "	usuario = @usuario AND sede_captura = @sede AND fecha = @fecha" + Environment.NewLine +
                        "--ORDER BY id_caso DESC" + Environment.NewLine +
                        "" +
                        "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;

                    dt = new DataTable();
                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                    if (dt.Rows.Count > 0)
                    {
                        dr["total"] = dt.Rows[0]["total"];
                    }

                    dtResumen.Rows.Add(dr);
                }

                catch (Exception ex)
                {
                    error = "ResumenCasosDiario(). " + ex.Message;
                    sqlite_conn.Close();
                    return null;
                }
                sqlite_conn.Close();
            }

            return dtResumen;
        }

        public DataTable DetalleCasosDiario(string usuario, string sede, string fecha)
        {
            error = string.Empty;

            if (sqlite_conn.State != ConnectionState.Open)
                if (CreateConnection() == false)
                    throw new Exception(error);

            SQLiteDataAdapter ad;
            DataTable dt = new DataTable();

            using (var cmd = new SQLiteCommand())
            {
                error = string.Empty;
                cmd.Connection = sqlite_conn;
                try
                {
                    cmd.CommandText = "" +
                        "SELECT " + Environment.NewLine +
                        "	id_caso," + Environment.NewLine +
                        "	fecha," + Environment.NewLine +
                        "	hora," + Environment.NewLine +
                        "	correlativo," + Environment.NewLine +
                        "	identificacion," + Environment.NewLine +
                        "	nombres," + Environment.NewLine +
                        "	apellidos," + Environment.NewLine +
                        "	sexo," + Environment.NewLine +
                        "	fecha_nacimiento," + Environment.NewLine +
                        "	strftime('%d', fecha_nacimiento) ||'/'|| strftime('%m', fecha_nacimiento) ||'/'|| strftime('%Y', fecha_nacimiento) fechanacimiento," + Environment.NewLine +
                        "	edad," + Environment.NewLine +
                        "	municipio_nacimiento," + Environment.NewLine +
                        "	departamento_nacimiento," + Environment.NewLine +
                        "	direccion," + Environment.NewLine +
                        "	comunidad_etnica," + Environment.NewLine +
                        "	telefono," + Environment.NewLine +
                        "	usuario," + Environment.NewLine +
                        "	nombre_usuario," + Environment.NewLine +
                        "	sede_captura," + Environment.NewLine +
                        "	fecha_captura," + Environment.NewLine +
                        "	estacion_captura," + Environment.NewLine +
                        "	descripcion_vuelo" + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" +
                        "	usuario = @usuario AND" +
                        "	sede_captura = @sede AND" +
                        "	fecha = @fecha" +
                        "" +
                        "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;


                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                }

                catch (Exception ex)
                {
                    error = "ResumenCasosDiario(). " + ex.Message;
                    sqlite_conn.Close();
                    return null;
                }
                sqlite_conn.Close();
            }

            return dt;
        }

        public DataTable DetalleCasosDiarioXVuelo(string usuario, string sede, string fecha, string descripcion_vuelo)
        {
            error = string.Empty;

            if (sqlite_conn.State != ConnectionState.Open)
                if (CreateConnection() == false)
                    throw new Exception(error);

            SQLiteDataAdapter ad;
            DataTable dt = new DataTable();

            using (var cmd = new SQLiteCommand())
            {
                error = string.Empty;
                cmd.Connection = sqlite_conn;
                try
                {
                    cmd.CommandText = "" +
                        "SELECT " + Environment.NewLine +
                        "	id_caso," + Environment.NewLine +
                        "	fecha," + Environment.NewLine +
                        "	hora," + Environment.NewLine +
                        "	correlativo," + Environment.NewLine +
                        "	identificacion," + Environment.NewLine +
                        "	nombres," + Environment.NewLine +
                        "	apellidos," + Environment.NewLine +
                        "	sexo," + Environment.NewLine +
                        "	fecha_nacimiento," + Environment.NewLine +
                        "	strftime('%d', fecha_nacimiento) ||'/'|| strftime('%m', fecha_nacimiento) ||'/'|| strftime('%Y', fecha_nacimiento) fechanacimiento," + Environment.NewLine +
                        "	edad," + Environment.NewLine +
                        "	municipio_nacimiento," + Environment.NewLine +
                        "	departamento_nacimiento," + Environment.NewLine +
                        "	direccion," + Environment.NewLine +
                        "	comunidad_etnica," + Environment.NewLine +
                        "	telefono," + Environment.NewLine +
                        "	usuario," + Environment.NewLine +
                        "	nombre_usuario," + Environment.NewLine +
                        "	sede_captura," + Environment.NewLine +
                        "	fecha_captura," + Environment.NewLine +
                        "	estacion_captura," + Environment.NewLine +
                        "	descripcion_vuelo" + Environment.NewLine +
                        "FROM casos" + Environment.NewLine +
                        "WHERE" + Environment.NewLine +
                        "	usuario = @usuario AND" + Environment.NewLine +
                        "	sede_captura = @sede AND" + Environment.NewLine +
                        "	fecha = @fecha AND" + Environment.NewLine +
                        "";

                    if(descripcion_vuelo.Equals("TODOS") == false)
                        cmd.CommandText += "	descripcion_vuelo = @descripcion_vuelo" + Environment.NewLine +
                            "" +
                            "";  //set the passed query

                    cmd.Parameters.Add("@usuario", DbType.String).Value = usuario;
                    cmd.Parameters.Add("@sede", DbType.String).Value = sede;
                    cmd.Parameters.Add("@fecha", DbType.String).Value = fecha;
                    cmd.Parameters.Add("@descripcion_vuelo", DbType.String).Value = descripcion_vuelo;

                    ad = new SQLiteDataAdapter(cmd);
                    ad.Fill(dt); //fill the datasource

                }

                catch (Exception ex)
                {
                    error = "ResumenCasosDiario(). " + ex.Message;
                    sqlite_conn.Close();
                    return null;
                }
                sqlite_conn.Close();
            }

            return dt;
        }

        public ConnectionState Status()
        {
            if (sqlite_conn != null)
                return ConnectionState.Closed;

            return sqlite_conn.State;
        }
    }
}
