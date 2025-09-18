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
    public class SolicitudLN
    {
        private bool resultado;
        private string error;
        private int noLibreta;

        private SolicitudEN solicitudEN;
        private string fotografia;
        private SolicitudEN huellas;
        private string cadenaConexion;
        private string estado;

        MsSqlConnection msSqlConnection;
        System.Data.SqlClient.SqlConnection connection;
        System.Data.SqlClient.SqlTransaction transaction;

        public SolicitudLN(string _cadenaConexion)
        {
            cadenaConexion = _cadenaConexion;            
            solicitudEN = null;
            fotografia = null;
            huellas = null;
            estado = null;
            resultado = false;
        }

        public bool ConsultarLibreta(string noLibreta)
        {
            resultado = false;
            try
            {
                if (cadenaConexion == null)
                    throw new Exception("Cadena de conexión nula. ");

                msSqlConnection = new MsSqlConnection(cadenaConexion);
                msSqlConnection.Open();

                if (!msSqlConnection.GetError().Equals(string.Empty))
                    throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());

                SolicitudAD solicitudAD = new SolicitudAD(msSqlConnection.GetConnection());

                if (!solicitudAD.ExisteLibreta(noLibreta))
                    throw new Exception(solicitudAD.GetError());

                if (solicitudAD.GetNoSolicitud() == -1)
                    throw new Exception("Libreta " + noLibreta + " no encontrada. ");

                int noSolicitud = solicitudAD.GetNoSolicitud();                               

                InventarioAD inventarioAD = new InventarioAD(msSqlConnection.GetConnection());

                if(!inventarioAD.ConsultarEstado(noSolicitud))
                    throw new Exception(inventarioAD.GetError());

                if(inventarioAD.GetEstado() == null)
                    throw new Exception("Estado inventario no encontrado. ");

                string estadoInventario = inventarioAD.GetEstado().Trim().ToUpper();
                if (!estadoInventario.Equals("CCalidadOk".ToUpper()))
                    throw new Exception("Estado inventario esperado: CCalidadOk. Encontrado: " + estadoInventario);

                if (!solicitudAD.ConsultarDatosBiograficos(noSolicitud))
                    throw new Exception(solicitudAD.GetError());

                if(solicitudAD.GetDatosBiograficos() == null)
                    throw new Exception("Solicitud " + noSolicitud + " no encontrada. ");

                solicitudEN = solicitudAD.GetDatosBiograficos();                

                resultado = true;
            }
            catch (Exception ex)
            {

                error = "ConsultarLibreta(). " + ex.Message;
            }

            return resultado;
        }        

        public SolicitudEN GetDatosBiograficos()
        {
            return solicitudEN;
        }

        public bool ConsultarFotografia(int noSolicitud)
        {
            resultado = false;
            try
            {
                if (cadenaConexion == null)
                    throw new Exception("Cadena de conexión nula. ");

                msSqlConnection = new MsSqlConnection(cadenaConexion);
                msSqlConnection.Open();

                if (!msSqlConnection.GetError().Equals(string.Empty))
                    throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());

                SolicitudAD solicitudAD = new SolicitudAD(msSqlConnection.GetConnection());                

                if (!solicitudAD.ConsultarFotografia(noSolicitud))
                    throw new Exception(solicitudAD.GetError());

                if (solicitudAD.GetFotografia() == null)
                    throw new Exception("Fotografía no encontrada (Solicitud " + noSolicitud + "). ");                

                fotografia = solicitudAD.GetFotografia();

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
            return fotografia;
        }

        public bool ConsultarEstado(int noSolicitud)
        {
            resultado = false;
            try
            {
                if (cadenaConexion == null)
                    throw new Exception("Cadena de conexión nula. ");

                msSqlConnection = new MsSqlConnection(cadenaConexion);
                msSqlConnection.Open();

                if (!msSqlConnection.GetError().Equals(string.Empty))
                    throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());

                SolicitudAD solicitudAD = new SolicitudAD(msSqlConnection.GetConnection());

                if (!solicitudAD.ConsultarEstado(noSolicitud))
                    throw new Exception(solicitudAD.GetError());

                if (solicitudAD.GetEstado() == null)
                    throw new Exception("Estado solicitud no encontrado. ");

                //string estadoSolicitud = solicitudAD.GetEstado().Trim().ToUpper();
                //if (!estadoSolicitud.Equals("PostImpresion".ToUpper()))
                //throw new Exception("Estado solicitud esperado: PostImpresion. Encontrado: " + estadoSolicitud);

                estado = solicitudAD.GetEstado();

                resultado = true;
            }
            catch (Exception ex)
            {

                error = "ConsultarEstado(). " + ex.Message;
            }

            return resultado;
        }

        public string GetEstado()
        {
            return estado;
        }

        public bool ConsultarHuellas(int noSolicitud)
        {
            resultado = false;
            try
            {
                if (cadenaConexion == null)
                    throw new Exception("Cadena de conexión nula. ");

                msSqlConnection = new MsSqlConnection(cadenaConexion);
                msSqlConnection.Open();

                if (!msSqlConnection.GetError().Equals(string.Empty))
                    throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());

                SolicitudAD solicitudAD = new SolicitudAD(msSqlConnection.GetConnection());

                if (!solicitudAD.ConsultarHuellas(noSolicitud))
                    throw new Exception(solicitudAD.GetError());

                if (solicitudAD.GetHuellas() == null)
                    throw new Exception("Huellas no encontradas (Solicitud " + noSolicitud + "). ");

                huellas = solicitudAD.GetHuellas();

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

        public Task<DataSet> EntregarAsync(string cadena, PasaporteEntregaEN datosEntrega)
        {
            return Task.Run(() =>
            {
                return EntregarSync(cadena, datosEntrega);
                //DataSet ds = ArmarDsResultado();
                //try
                //{
                //    msSqlConnection = new MsSqlConnection(cadena);
                //    msSqlConnection.Open();
                //    //MessageBox.Show("Open(): Estado: " + msSqlConnection.GetState() + ". Errores: " + (msSqlConnection.GetError() == string.Empty ? "Ninguno." : msSqlConnection.GetError()));

                //    //string sql = "SELECT s.estado FROM solicitud s WHERE id = 4505160;";
                //    //System.Data.DataSet ds = msSqlConnection.Select(sql);
                //    ////MessageBox.Show("Sql:" + sql + ". Estado: " + msSqlConnection.GetState() + ". Errores: " + (msSqlConnection.GetError() == string.Empty ? "Ninguno." : msSqlConnection.GetError()));
                //    if (!msSqlConnection.GetError().Equals(string.Empty)) throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());
                //    //{
                //    //MessageBox.Show("Cantidad de filas: " + ds.Tables[0].Rows.Count + ". Registro: " + ds.Tables[0].Rows[0][0].ToString());
                //    //}

                //    connection = msSqlConnection.GetConnection();
                //    transaction = connection.BeginTransaction();

                //    //1. Tabla Solicitud actualizando los campos
                //    System.Data.SqlClient.SqlCommand command = connection.CreateCommand();
                //    command.Transaction = transaction;
                //    command.CommandText = "UPDATE solicitud SET estado = 'Entregada', updated_by = '" + datosEntrega.usuario + "', updated_at = getdate() WHERE id = " + datosEntrega.solicitud_id + ";";
                //    command.CommandType = System.Data.CommandType.Text;
                //    int n = command.ExecuteNonQuery();
                //    //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //    //2. Tabla solicitud_bitacora  insertar de registros llenando los campos
                //    command.CommandText = "INSERT INTO solicitud_bitacora (solicitud_id, estado, observaciones, created_by, updated_by, created_at, updated_at) VALUES(" + datosEntrega.solicitud_id + ", 'Entregada', 'Pasaporte Entregado', '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "', getdate(), getdate());";
                //    n = command.ExecuteNonQuery();
                //    //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //    //3. Tabla solicitud_imagenes actualizando los registros
                //    string complemento = JsonConvert.SerializeObject(datosEntrega.complemento);
                //    string imagen_postal1 = complemento;
                //    command.CommandText = "UPDATE solicitud_imagenes SET imagen_postal3 = '" + datosEntrega.imagen_postal3 + "', updated_at = getdate(), imagen_postal1 = '" + imagen_postal1 + "', imagen_postal2 = '" + datosEntrega.imagen_postal2 + "' WHERE solicitud_id = " + datosEntrega.solicitud_id + ";";
                //    n = command.ExecuteNonQuery();
                //    //MessageBox.Show("Comando: UPDATE solicitud_imagenes. Cantidad de registro afectados: " + n);

                //    //4. Tabla solicitud_bitacora  insertar de registros llenando los campos
                //    command.CommandText = "INSERT INTO bitacora_acceso (usuario, estacion, fecha, valido, ingreso, created_at, updated_at, created_by, updated_by) VALUES('" + datosEntrega.usuario + "', '" + datosEntrega.estacion + "', getdate(), 1, 1, getdate(), getdate(), '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "');";
                //    n = command.ExecuteNonQuery();
                //    //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //    //5. Tabla bitacora_seguridad  insertar registros
                //    command.CommandText = "INSERT INTO bitacora_seguridad (usuario_id, Accion, descripcion, observaciones, created_at, updated_at, created_by, updated_by) VALUES((SELECT us.id FROM usuario us WHERE usuario = '" + datosEntrega.usuario + "'), 'Entrega', 'Modulo de Entrega', '" + datosEntrega.no_libreta + "', getdate(), getdate(), '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "');";
                //    n = command.ExecuteNonQuery();
                //    //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //    transaction.Commit();
                //    ds.Tables[0].Rows[0]["RESULTADO"] = true;
                //    ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
                //}
                //catch (Exception ex)
                //{
                //    if (transaction != null) transaction.Rollback();
                //    ds.Tables[0].Rows[0]["RESULTADO"] = false;
                //    ds.Tables[0].Rows[0]["MSG_ERROR"] = "Entregar(). " + ex.Message;
                //    //MessageBox.Show("Error al ejecutar la transacción. Detalles: " + ex.Message);
                //}

                //msSqlConnection.Close();
                //return ds;
            });            
        }

        public DataSet EntregarSync(string cadena, PasaporteEntregaEN datosEntrega)
        {
            DataSet ds = ArmarDsResultado();
            try
            {
                msSqlConnection = new MsSqlConnection(cadena);
                msSqlConnection.Open();
                //MessageBox.Show("Open(): Estado: " + msSqlConnection.GetState() + ". Errores: " + (msSqlConnection.GetError() == string.Empty ? "Ninguno." : msSqlConnection.GetError()));

                //string sql = "SELECT s.estado FROM solicitud s WHERE id = 4505160;";
                //System.Data.DataSet ds = msSqlConnection.Select(sql);
                ////MessageBox.Show("Sql:" + sql + ". Estado: " + msSqlConnection.GetState() + ". Errores: " + (msSqlConnection.GetError() == string.Empty ? "Ninguno." : msSqlConnection.GetError()));
                if (!msSqlConnection.GetError().Equals(string.Empty)) throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());
                //{
                //MessageBox.Show("Cantidad de filas: " + ds.Tables[0].Rows.Count + ". Registro: " + ds.Tables[0].Rows[0][0].ToString());
                //}

                connection = msSqlConnection.GetConnection();
                transaction = connection.BeginTransaction();

                //1. Tabla Solicitud actualizando los campos
                System.Data.SqlClient.SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "UPDATE solicitud SET estado = 'Entregada', updated_by = '" + datosEntrega.usuario + "', updated_at = getdate() WHERE id = " + datosEntrega.solicitud_id + ";";
                command.CommandType = System.Data.CommandType.Text;
                int n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //2. Tabla solicitud_bitacora  insertar de registros llenando los campos
                command.CommandText = "INSERT INTO solicitud_bitacora (solicitud_id, estado, observaciones, created_by, updated_by, created_at, updated_at) VALUES(" + datosEntrega.solicitud_id + ", 'Entregada', 'Pasaporte Entregado', '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "', getdate(), getdate());";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //3. Tabla solicitud_imagenes actualizando los registros
                string complemento = JsonConvert.SerializeObject(datosEntrega.complemento);
                string imagen_postal1 = complemento;
                command.CommandText = "UPDATE solicitud_imagenes SET imagen_postal3 = '" + datosEntrega.imagen_postal3 + "', updated_at = getdate(), imagen_postal1 = '" + imagen_postal1 + "', imagen_postal2 = '" + datosEntrega.imagen_postal2 + "' WHERE solicitud_id = " + datosEntrega.solicitud_id + ";";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: UPDATE solicitud_imagenes. Cantidad de registro afectados: " + n);

                //4. Tabla solicitud_bitacora  insertar de registros llenando los campos
                command.CommandText = "INSERT INTO bitacora_acceso (usuario, estacion, fecha, valido, ingreso, created_at, updated_at, created_by, updated_by) VALUES('" + datosEntrega.usuario + "', '" + datosEntrega.estacion + "', getdate(), 1, 1, getdate(), getdate(), '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "');";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //5. Tabla bitacora_seguridad  insertar registros
                command.CommandText = "INSERT INTO bitacora_seguridad (usuario_id, Accion, descripcion, observaciones, created_at, updated_at, created_by, updated_by) VALUES((SELECT us.id FROM usuario us WHERE usuario = '" + datosEntrega.usuario + "'), 'Entrega', 'Modulo de Entrega', '" + datosEntrega.no_libreta + "', getdate(), getdate(), '" + datosEntrega.usuario + "', '" + datosEntrega.usuario + "');";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                transaction.Commit();
                ds.Tables[0].Rows[0]["RESULTADO"] = true;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = string.Empty;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                ds.Tables[0].Rows[0]["RESULTADO"] = false;
                ds.Tables[0].Rows[0]["MSG_ERROR"] = "Entregar(). " + ex.Message;
                //MessageBox.Show("Error al ejecutar la transacción. Detalles: " + ex.Message);
            }

            msSqlConnection.Close();
            return ds;
        }

        private DataSet ArmarDsResultado()
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("RESULTADO", typeof(bool));
            ds.Tables[0].Columns.Add("MSG_ERROR", typeof(string));

            ds.Tables[0].Columns.Add("DATOS", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_PAGO_PASAPORTE", typeof(object));
            ds.Tables[0].Columns.Add("DATOS_SIBIO", typeof(object));

            DataRow dr = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(dr);

            return ds;
        }
    }
}
