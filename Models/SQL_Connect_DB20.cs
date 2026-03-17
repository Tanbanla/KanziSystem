using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class SQL_Connect_DB20
    {
        public DataTable GET_DATA_FROM_SQL(string DATASELECT)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(@"data source=apbivndb20;initial catalog=COST_MANAGEMENT;user id=whs;password=147258@;"))
                {
                    if (cn.State != ConnectionState.Open) { cn.Open(); }
                    string CommandText = DATASELECT;
                    using ( SqlCommand cmd = new SqlCommand(CommandText, cn))
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
        public string ReturnString(string command)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(@"data source=apbivndb20;initial catalog=COST_MANAGEMENT;user id=whs;password=147258@;"))
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

        public int ExecuteSP(string spName, object? param = null)
        {
            string connectionString = @"data source=apbivndb20;initial catalog=COST_MANAGEMENT;user id=whs;password=147258@;";

            using var conn = new SqlConnection(connectionString);
            return conn.Execute(spName, param, commandType: CommandType.StoredProcedure);
        }
    }   

}
