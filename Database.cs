using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Projekt1
{
    public class Database
    {
        static private Database DB = new Database();
        private SqlConnection DbCon = null;

        Database()
        {
            this.DbCon = Database.CreateConnection();
        }
        private static SqlConnection CreateConnection()
        {
            return new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\Tekoppar\source\repos\Backend Uppgift 2\Database1.mdf';Integrated Security=True");
        }

        static public int InsertData(string table, List<string> columns, List<string> values)
        {
            return Database.DB._InsertData(table, columns, values);
        }

        private int _InsertData(string table, List<string> columns, List<string> values)
        {
            using (this.DbCon = Database.CreateConnection())
            {
                this.DbCon.Open();
                string insertValues = "'" + string.Join("', '", values.ToArray()) + "'";
                string columnNames = string.Join(", ", columns.ToArray());

                Console.WriteLine(insertValues);
                Console.WriteLine(columnNames);
                SqlCommand command;
                int rowId = -1;
                if (table != "customer")
                {
                    command = new SqlCommand("INSERT INTO " + table + " (" + columnNames + ") OUTPUT INSERTED.Id VALUES (" + insertValues + ")", this.DbCon);
                    rowId = (int)command.ExecuteScalar();
                }
                else
                {
                    command = new SqlCommand("INSERT INTO " + table + " (" + columnNames + ") VALUES (" + insertValues + ")", this.DbCon);
                    command.ExecuteNonQuery();
                    command = new SqlCommand("SELECT max(Id) FROM customer", this.DbCon);
                    rowId = (int)command.ExecuteScalar();
                }

                this.DbCon.Close();
                return rowId;
            }
        }

        static public int UpdateData(string table, List<string> columns, List<string> values, string needle, string haystack)
        {
            return Database.DB._UpdateData(table, columns, values, needle, haystack);
        }

        private Dictionary<string, System.Data.SqlDbType> GetColumnTypes(string table, List<string> columns)
        {
            using (this.DbCon = Database.CreateConnection())
            {
                Dictionary<string, System.Data.SqlDbType> columnTypes = new Dictionary<string, System.Data.SqlDbType>();
                this.DbCon.Open();

                string sqlString = "SELECT @COLUMNNAME FROM " + table;
                SqlCommand command = new SqlCommand(sqlString, this.DbCon);

                SqlParameter param = new SqlParameter("@COLUMNNAME", System.Data.SqlDbType.VarChar, columns[0].Length);
                param.Value = columns[0];
                command.Parameters.Add(param);

                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                System.Data.DataTable tables = reader.GetSchemaTable();
                System.Data.DataRow row = tables.Rows[0];
                columnTypes.Add(columns[0], (System.Data.SqlDbType)(int)row["ProviderType"]);
                reader.Close();

                for (int i = 1; i < columns.Count(); i++)
                {
                    command.Parameters[0].Value = columns[i];
                    reader = command.ExecuteReader();
                    tables = reader.GetSchemaTable();
                    row = tables.Rows[0];
                    columnTypes.Add(columns[i], (System.Data.SqlDbType)(int)row["ProviderType"]);
                    reader.Close();
                }

                this.DbCon.Close();
                return columnTypes;
            }
        }

        private int _UpdateData(string table, List<string> columns, List<string> values, string needle, string haystack)
        {
            using (this.DbCon = Database.CreateConnection())
            {
                this.DbCon.Open();
                string sqlString = "UPDATE " + table + " SET ";
                for (int i = 0; i < columns.Count(); i++)
                {
                    sqlString += columns[i] + " = @" + columns[i] + "";

                    if (i < columns.Count() - 1)
                        sqlString += ", ";
                }
                sqlString += " WHERE " + haystack + " = '" + needle + "'";
                SqlCommand command = new SqlCommand(sqlString, this.DbCon);

                Dictionary<string, System.Data.SqlDbType> columnTypes = this.GetColumnTypes(table, columns);
                for (int i = 0; i < values.Count(); i++)
                {
                    SqlParameter param = new SqlParameter("@" + columns[i], columnTypes[columns[i]], 256);
                    param.Value = values[i] == "" ? DBNull.Value : values[i];
                    command.Parameters.Add(param);
                }

                command.Prepare();
                //command.ExecuteNonQuery();

                int rows = command.ExecuteNonQuery();
                this.DbCon.Close();
                return rows;
            }
        }

        static public int RemoveData(string table, string needle, string haystack)
        {
            return Database.DB._RemoveData(table, needle, haystack);
        }

        private int _RemoveData(string table, string needle, string haystack)
        {
            using (this.DbCon = Database.CreateConnection())
            {
                this.DbCon.Open();
                SqlCommand command = new SqlCommand("DELETE FROM " + table + " WHERE " + needle + " = '" + haystack + "'", this.DbCon);
                int rows = command.ExecuteNonQuery();
                this.DbCon.Close();
                return rows;
            }
        }

        static public List<Dictionary<string, string>> GetData(string table, string needle, List<string> haystack)
        {
            return Database.DB._GetData(table, needle, haystack);
        }

        private List<Dictionary<string, string>> _GetData(string table, string needle, List<string> haystack)
        {
            using (this.DbCon = Database.CreateConnection())
            {
                this.DbCon.Open();
                string sqlString = "SELECT * FROM " + table + (needle != "*" ? " WHERE " : "");
                for (int i = 0; i < haystack.Count(); i++)
                {
                    sqlString += haystack[i] + " = '" + needle + "'";

                    if (i < haystack.Count() - 1)
                        sqlString += " OR ";
                }
                SqlCommand command = new SqlCommand(sqlString, this.DbCon);
                List<Dictionary<string, string>> values = this._GetColumns(command.ExecuteReader());
                this.DbCon.Close();
                return values;
            }
        }

        private List<Dictionary<string, string>> _GetColumns(SqlDataReader reader)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            while (reader.IsClosed == false && reader.Read())
            {
                int columns = reader.FieldCount;
                Dictionary<string, string> values = new Dictionary<string, string>();
                for (int i = 0; i < columns; i++)
                {
                    string value = reader.GetValue(i).ToString();
                    Type fieldType = reader.GetFieldType(i);
                    values.Add(reader.GetName(i), value);
                }
                list.Add(values);
            }

            reader.Close();
            return list;
        }

        private void CreateQuery(string queryString)
        {
            using (this.DbCon)
            {
                this.DbCon.Open();
                SqlCommand command = new SqlCommand(queryString, this.DbCon);
                //command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int columns = reader.FieldCount;
                    for (int i = 0; i < columns; i++)
                    {
                        string value = reader.GetValue(i).ToString();
                        Type fieldType = reader.GetFieldType(i);
                        Console.WriteLine(value + " - " + fieldType.Name);
                    }
                }
            }
        }
    }
}
