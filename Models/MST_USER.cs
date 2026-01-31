using System.Data;
using System.Globalization;
using System.Xml.Linq;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class Search_param
    {
        public string? us_code { get; set; }
        public string? us_dept { get; set; }
        public string? us_adid { get; set; }
    }
    public class MST_USER
    {    
        public int Id { get; set; }
        public required string? CHR_NAME { get; set; }
        public required string? CHR_ADID { get; set; }
        public required string? CHR_STAFF_CODE { get; set; }
        public required string? CHR_DEPT { get;set; }
        public required string? ROLE { get;set; }
        public required string? CHR_MAIL { get;set; }
        public required string? DTM_UPDATED { get; set; }    
        public required string? CHR_UPDATE_BY { get; set; }   
        public required string? DTM_CREARTED { get; set; }
        public required string? DTM_LAST_LOGIN { get; set; }
        public required string? IS_ENABLE { get; set; }
        public static List<MST_USER> user_process(Search_param para)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            para.us_dept = para.us_dept is null? "NULL" : para.us_dept!.ToString().Length > 0 ? $"N'{para.us_dept}'" : "NULL";
            para.us_code = para.us_code is null ? "NULL" : para.us_code!.ToString().Length > 0 ? $"N'{para.us_code}'" : "NULL";
            para.us_adid = para.us_adid is null ? "NULL" : para.us_adid!.ToString().Length > 0 ? $"N'{para.us_adid}'" : "NULL";

            var _cmd = _context.GET_DATA_FROM_SQL($"EXEC [dbo].[PE_TM_USER_GetData] {para.us_dept}, {para.us_code}, {para.us_adid} ");          
            List<MST_USER> _users = new List<MST_USER>();
            for(int i = 0; i < _cmd.Rows.Count; i++)
            {
                _users.Add(new MST_USER
                {
                    Id = int.Parse(_cmd.Rows[i]["ID"].ToString()!),
                    CHR_NAME =  _cmd.Rows[i]["CHR_NAME"].ToString(),
                    CHR_ADID = _cmd.Rows[i]["CHR_ADID"].ToString(),
                    CHR_STAFF_CODE = _cmd.Rows[i]["CHR_STAFF_CODE"].ToString(),
                    CHR_DEPT = _cmd.Rows[i]["CHR_DEPT"].ToString(),
                    ROLE = _cmd.Rows[i]["ROLE"].ToString(),
                    CHR_MAIL = _cmd.Rows[i]["CHR_MAIL"].ToString(),
                    DTM_UPDATED = _cmd.Rows[i]["DTM_UPDATED"].ToString(),
                    CHR_UPDATE_BY = _cmd.Rows[i]["CHR_UPDATE_BY"].ToString(),
                    DTM_CREARTED = _cmd.Rows[i]["DTM_CREARTED"].ToString(),
                    DTM_LAST_LOGIN = _cmd.Rows[i]["DTM_LAST_LOGIN"].ToString(),
                    IS_ENABLE = _cmd.Rows[i]["IS_ENABLE"].ToString(),
                });
            }
            return _users;
        }
        public static string insert_update_users(string name, string adid, string staffCode, string dept, string role, string mail, string updateBy, string isEnable)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            var sql = $@"EXEC [dbo].[PE_TM_USER_Update_Or_Insert] N'{name}','{adid}','{staffCode}','{dept}','{role}','{mail}','{updateBy}','{isEnable}'";               
            var result = _context.GET_DATA_FROM_SQL(sql);
            return result.Rows[0][0].ToString()!;
        }
    }
}
