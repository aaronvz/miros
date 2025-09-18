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
    public class PasaporteEntregaAD
    {
        private PasaporteEntregaEN pasaporteEntregaEN;
        private bool resultado;
        private string error;        

        MsSqlConnection msSqlConnection;
        System.Data.SqlClient.SqlConnection connection;
        System.Data.SqlClient.SqlTransaction transaction;

        public PasaporteEntregaAD()
        {
        }

        public PasaporteEntregaAD(PasaporteEntregaEN _pasaporteEntregaEN)
        {
            pasaporteEntregaEN = _pasaporteEntregaEN;
        }


        public bool GuardarAsync(string cadena)
        {
            resultado = false;
            try
            {
                if (pasaporteEntregaEN == null)
                    throw new Exception("No se encontraron datos de pasaporte para entregar. ");

                msSqlConnection = new MsSqlConnection(cadena);
                msSqlConnection.Open();

                if (!msSqlConnection.GetError().Equals(string.Empty))
                    throw new Exception("Error al abrir la conexión. Detalles: " + msSqlConnection.GetError());
                
                connection = msSqlConnection.GetConnection();
                transaction = connection.BeginTransaction();

                //1. Tabla Solicitud actualizando los campos
                System.Data.SqlClient.SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "UPDATE solicitud SET estado = 'Entregada', updated_by = '" + pasaporteEntregaEN.usuario + "', updated_at = getdate() WHERE id = " + pasaporteEntregaEN.solicitud_id + ";";
                command.CommandType = System.Data.CommandType.Text;
                int n = command.ExecuteNonQuery();
                
                //2. Tabla solicitud_bitacora  insertar de registros llenando los campos
                command.CommandText = "INSERT INTO solicitud_bitacora (solicitud_id, estado, observaciones, created_by, updated_by, created_at, updated_at) VALUES(" + pasaporteEntregaEN.solicitud_id + ", 'Entregada', 'Pasaporte Entregado', '" + pasaporteEntregaEN.usuario + "', '" + pasaporteEntregaEN.usuario + "', getdate(), getdate());";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //3. Tabla solicitud_imagenes actualizando los registros
                string complemento = JsonConvert.SerializeObject(pasaporteEntregaEN.complemento);
                string imagen_postal1 = complemento;
                command.CommandText = "UPDATE solicitud_imagenes SET imagen_postal3 = '" + pasaporteEntregaEN.imagen_postal3 + "', updated_at = getdate(), imagen_postal1 = '" + imagen_postal1 + "', imagen_postal2 = '" + pasaporteEntregaEN.imagen_postal2 + "' WHERE solicitud_id = " + pasaporteEntregaEN.solicitud_id + ";";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: UPDATE solicitud_imagenes. Cantidad de registro afectados: " + n);

                //4. Tabla solicitud_bitacora  insertar de registros llenando los campos
                command.CommandText = "INSERT INTO bitacora_acceso (usuario, estacion, fecha, valido, ingreso, created_at, updated_at, created_by, updated_by) VALUES('" + pasaporteEntregaEN.usuario + "', '" + pasaporteEntregaEN.estacion + "', getdate(), 1, 1, getdate(), getdate(), '" + pasaporteEntregaEN.usuario + "', '" + pasaporteEntregaEN.usuario + "');";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                //5. Tabla bitacora_seguridad  insertar registros
                command.CommandText = "INSERT INTO bitacora_seguridad (usuario_id, Accion, descripcion, observaciones, created_at, updated_at, created_by, updated_by) VALUES((SELECT us.id FROM usuario us WHERE usuario = '" + pasaporteEntregaEN.usuario + "'), 'Entrega', 'Modulo de Entrega', '" + pasaporteEntregaEN.no_libreta + "', getdate(), getdate(), '" + pasaporteEntregaEN.usuario + "', '" + pasaporteEntregaEN.usuario + "');";
                n = command.ExecuteNonQuery();
                //MessageBox.Show("Comando: " + command.CommandText + ". Cantidad de registro afectados: " + n);

                transaction.Commit();
                resultado = true;
                error = string.Empty;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    transaction.Rollback();

                error = "Entregar(). " + ex.Message;
            }

            msSqlConnection.Close();
            return resultado;
        }
        
        public string GetError()
        {
            return error;
        }
    }
}
