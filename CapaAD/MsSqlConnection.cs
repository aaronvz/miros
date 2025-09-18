using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaAD
{     
    public class MsSqlConnection
    {
        private string connectionString = @"Data Source=10.200.1.223;Initial Catalog=PasaportesGuatemala;User ID=ws_entrega;Password=wsentrega2022";
        private SqlConnection sqlConnection;
        private ConnectionState connectionState;
        private string error;

        public MsSqlConnection(string cadena)
        {
            if(cadena != null) connectionString = @cadena;
            sqlConnection = new SqlConnection(connectionString);            
            connectionState = ConnectionState.Closed;
            error = string.Empty;
        }

        public void Open()
        {
            try
            {
                sqlConnection.Open();
                connectionState = sqlConnection.State;
                error = string.Empty;
            }
            catch (Exception ex)
            {

                error = "Open(). " + ex.Message;
            }            
        }

        public void Close()
        {
            try
            {
                if (sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
                connectionState = sqlConnection.State;
                error = string.Empty;
            }
            catch (Exception ex)
            {

                error = "Close(). " + ex.Message;
            }
        }

        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }

        public DataSet Select(string sql)
        {
            DataSet ds = new DataSet();
            try
            {
                
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(sql, sqlConnection);
                adapter.Fill(ds);
                error = string.Empty;
            }
            catch (Exception ex)
            {
                ds = null;
                error = "Select(). " + ex.Message;
            }            
            return ds;
        }

        public ConnectionState GetState()
        {
            return connectionState;
        }
   
        public string GetError()
        {
            return error;
        }
    }
}
