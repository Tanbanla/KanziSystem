using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class SQL_Connect_DB20
    {
        public readonly string connectString = @"data source=apbivndb14;initial catalog=COST_MANAGEMENT;user id=Kanzaisystem;password=Kanzaisystem;";
        //public readonly string connectString = @"data source=apbivndb20;initial catalog=COST_MANAGEMENT;user id=tuyenmt;password=123456a@;";
        public DataTable GET_DATA_FROM_SQL(string DATASELECT)
         {
            try
            {
                using ( SqlConnection cn = new SqlConnection(connectString))
                {
                    if (cn.State != ConnectionState.Open) { cn.Open(); }
                    string CommandText = DATASELECT;
                    using (  SqlCommand cmd = new SqlCommand(CommandText, cn))
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adp.Fill(dt);
                                return dt;
                            };
                        }
                    }
                }
            }
            catch { return new DataTable(); }
        }
   
        public string ReturnString(string command)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectString))
                {
                    if (cn.State != ConnectionState.Open) { cn.Open(); }
                    string CommandText = command;

                    using (SqlCommand cmd = new SqlCommand(CommandText, cn))
                    {
                        cmd.CommandTimeout = 5000;
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adp.Fill(dt);
                                return dt.Rows[0][0].ToString();
                            }
                        }
                    }       
                }
            }
            catch { return ""; }
        }
        // Thêm vào trong lớp SQL_Connect_DB20 của bạn
        public readonly string connectString_Test = @"data source=apbivndb20;initial catalog=COST_MANAGEMENT;user id=tuyenmt;password=123456a@;";
        public DataTable GET_DATA_FROM_SQL_TEST(string DATASELECT)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectString_Test))
                {
                    if (cn.State != ConnectionState.Open) { cn.Open(); }
                    string CommandText = DATASELECT;
                    using (SqlCommand cmd = new SqlCommand(CommandText, cn))
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adp.Fill(dt);
                                return dt;
                            }
                            ;
                        }
                    }
                }
            }
            catch { return new DataTable(); }
        }
        public bool EXECUTE_SQL(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectString_Test))
                {
                    int rowsAffected = cn.Execute(query, parameters);
                    return rowsAffected > 0;
                }
            }
            catch { return false; }
        }
        public int ExecuteSP(string spName, object? param = null)
        {
            using var conn = new SqlConnection(connectString);
            return conn.Execute(spName, param, commandType: CommandType.StoredProcedure);
        }

        public SqlDataReader sqlreader;
        public System.Data.DataTable Getdatatable(string strsql, string name)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.TableName = name;
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();
                SqlCommand sqlCommand = new SqlCommand(strsql, connection);
                sqlCommand.CommandTimeout = 18000;
                try
                {
                    this.sqlreader = sqlCommand.ExecuteReader();
                    dataTable.Load((IDataReader)this.sqlreader);
                }
                catch
                {
                }
                connection.Close();
                return dataTable;
            }
        }
    }   

}
