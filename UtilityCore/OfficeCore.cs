using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace UtilityCore
{
    public partial class OfficeCore
    {
        public static DataTable ConvertCSVtoDataTable(string strFilePath,char splitCharacter)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(splitCharacter);
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(splitCharacter);
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public static void SaveDataTableToCSV(DataTable dt,string file ,string delimiter)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(delimiter, columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(delimiter, fields));
            }

            File.WriteAllText(file, sb.ToString());
        }
    }
}
