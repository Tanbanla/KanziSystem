using System.Data;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class KHO_TMP
    {
        public int ID { get; set; }
        public string CHR_CODE_MATERIAL { get; set; }
        public string CHR_WAREHOUSE { get; set; }
        public double QUANTITY { get; set; }
        public string CHR_GROUP_CODE { get; set; }

        public static void InsertDataImport(KHO_TMP data)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            string cmd = "INSERT INTO [COST_MANAGEMENT].[dbo].[KHO_TMP]([CHR_CODE_MATERIAL],[CHR_WAREHOUSE],[QUANTITY],[CHR_GROUP_CODE]) ";
            cmd += $"VALUES('{data.CHR_CODE_MATERIAL}','{data.CHR_WAREHOUSE}','{data.QUANTITY}','{data.CHR_GROUP_CODE}') ";
            db.GET_DATA_FROM_SQL(cmd);
        }
        public static List<KHO_TMP> GetDataTemp(string warehouse)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();

            string cmd = "SELECT [ID],[CHR_CODE_MATERIAL],[CHR_WAREHOUSE],[QUANTITY],[CHR_GROUP_CODE] ";
            cmd += "FROM [COST_MANAGEMENT].[dbo].[KHO_TMP] ";
            cmd += $"WHERE [CHR_WAREHOUSE] = '{warehouse}' ";

            DataTable dataGet = db.GET_DATA_FROM_SQL(cmd);
            List<KHO_TMP> dataResult = new List<KHO_TMP>();
            for (int idx = 0; idx < dataGet.Rows.Count; idx++)
            {
                KHO_TMP data = new KHO_TMP()
                {
                    CHR_CODE_MATERIAL = dataGet.Rows[idx][""].ToString() ?? "null",
                    CHR_GROUP_CODE = dataGet.Rows[idx][""].ToString() ?? "null",
                    CHR_WAREHOUSE = dataGet.Rows[idx][""].ToString() ?? "null",
                    ID = idx,
                    QUANTITY = double.Parse(dataGet.Rows[idx][""].ToString() ?? "0.0")
                };
                dataResult.Add(data);
            }

            return dataResult;
        }
    }

}
