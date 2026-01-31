namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class MST_WAREHOUSE
    {
        public int ID { get; set; }
        public required string? CHR_WAREHOUSE { get; set; }  
        public required string? CHR_DEPT_USE { get; set; }
        public required string? CHR_FACTORY { get; set; }
        public required string? DTM_UPDATE { get; set; }
        public required string? CHR_USER { get; set; }
        public required string? CHR_NOTE { get; set; }
        public static List<MST_WAREHOUSE> warehouse_process()
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();

            var _cmd = _context.GET_DATA_FROM_SQL("[dbo].[PE_MST_WAREHOUSE_GetData]");
            List<MST_WAREHOUSE> _wh = new List<MST_WAREHOUSE>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                _wh.Add(new MST_WAREHOUSE
                {
                    ID = int.Parse(_cmd.Rows[i]["ID"].ToString()!),
                    CHR_WAREHOUSE = _cmd.Rows[i]["CHR_WAREHOUSE"].ToString(),
                    CHR_DEPT_USE = _cmd.Rows[i]["CHR_DEPT_USE"].ToString(),
                    CHR_FACTORY = _cmd.Rows[i]["CHR_FACTORY"].ToString(),
                    CHR_USER = _cmd.Rows[i]["CHR_USER"].ToString(),
                    CHR_NOTE = _cmd.Rows[i]["CHR_NOTE"].ToString(),
                    DTM_UPDATE = _cmd.Rows[i]["DTM_UPDATE"].ToString()
                });
            }
            return _wh;
        }
        public string ReturnCode(string CostCenter)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            string str1 = CostCenter + "." + DateTime.Now.ToString("yyyyMMdd") + "-";
            string str2 = "";
            for (int index = 1; index < 1000; ++index)
            {
                var check = _context.GET_DATA_FROM_SQL("SELECT * FROM [REQUEST] WHERE Code_Request = '" + str1 + index.ToString() + "'");
                if(check.Rows.Count > 0)
                {
                    str2 = str1 + index.ToString();
                    break;
                }             
            }
            return str2;
        }
    }
}
