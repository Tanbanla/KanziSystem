using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

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
                if (check.Rows.Count > 0)
                {
                    str2 = str1 + index.ToString();
                    break;
                }
            }
            return str2;
        }
        public static string Insert_warehouse(string CHR_WAREHOUSE, string CHR_DEPT_USE, string CHR_FACTORY, string CHR_NOTE,string CHR_USER)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var sql = $@"
                        IF NOT EXISTS (
                            SELECT 1 FROM [MST_WAREHOUSE] 
                            WHERE CHR_WAREHOUSE = N'{CHR_WAREHOUSE}' 
                              AND CHR_DEPT_USE = N'{CHR_DEPT_USE}' 
                              AND CHR_FACTORY = '{CHR_FACTORY}'
                        )
                        BEGIN 
                            INSERT INTO [MST_WAREHOUSE] (CHR_WAREHOUSE, CHR_DEPT_USE, CHR_FACTORY, DTM_UPDATE, CHR_USER, CHR_NOTE)
                            VALUES (N'{CHR_WAREHOUSE}', N'{CHR_DEPT_USE}', '{CHR_FACTORY}', GETDATE(), '{CHR_USER}', N'{CHR_NOTE}');
        
                            SELECT 'OK' AS Status;
                        END 
                        ELSE 
                        BEGIN 
                            SELECT 'DUPLICATED' AS Status;
                        END";

            var insert = _db.GET_DATA_FROM_SQL(sql);
            if (insert.Rows[0][0].ToString() == "DUPLICATED")
            {
                return "Mã kho đã tồn tại";
            }
            else
            {
                return "Thêm kho thành công";
            }
            
        }
        public static string delete_wh(string id, string tenkho)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            var checkkho = _db.GET_DATA_FROM_SQL($"select * from [KHO] where Kho = '" + tenkho + "'");
            if (checkkho.Rows.Count > 0) {
                return "Vẫn còn mã hàng trong kho";
            }
            else
            {
                var del = _db.GET_DATA_FROM_SQL($"delete from [MST_WAREHOUSE] where ID = '" + id + "'");
                return "Xóa kho thành công !";
            }
          
        }
       
    }
    public class SECTION
    {
        public int ID { get; set; }
        public string? CHR_CODE { get; set; }
        public string? CHR_NAME { get; set; }
        public string? CHR_SECTION_NAME { get; set; }
        public static List<string> _load_sec()
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            var load = sql.GET_DATA_FROM_SQL("select * from [Section]");
            List<string> ds = new List<string>();
            for(int i = 0; i < load.Rows.Count; i++)
            {
                ds.Add(load.Rows[i]["CHR_SECTION_NAME"].ToString()!);
            }
            return ds;
        }
    }
}
