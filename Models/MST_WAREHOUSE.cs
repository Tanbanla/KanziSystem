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
        public static string Chuyenkho(string mahang, string khohientai, string tonkho, string phongban, string denkho, string soluong, string nguoichuyen, string khoi)
        {
            SQL_Connect_DB20 _sql = new SQL_Connect_DB20();
            var chuyenkho = _sql.GET_DATA_FROM_SQL($"" +
                // trừ xuất kho
                $"UPDATE KHO SET Hientai = Hientai - {soluong} " +
                $"WHERE MaNguyenLieu = N'{mahang}' AND Kho = '{khohientai}'; " +
               // Nhập vào kho mới
                $"IF EXISTS (SELECT 1 FROM KHO WHERE MaNguyenLieu = '{mahang}' AND Kho = '{denkho}') " +
                $"BEGIN " +
                $"UPDATE KHO SET Hientai = Hientai + {soluong} " +
                $"WHERE MaNguyenLieu = '{mahang}' AND Kho = '{denkho}'; " +
                $"END ELSE BEGIN " +
                $"INSERT INTO KHO (MaNguyenLieu, Kho, Hientai, Group_Code) VALUES ('{mahang}', '{denkho}', '{soluong}','{khoi}'); END " +
                // ghi log
                $"INSERT INTO [KHO_NHAPXUAT] ([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Ngaynhaokho],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[Phong]) " +
                $"VALUES ('{mahang}',N'Chuyển kho: {khohientai} -> {denkho} : {mahang}', '{soluong}','CHUYENKHO','{DateTime.Now.ToString("yyyy-MM-dd")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{nguoichuyen}','{khohientai}','{khoi}', N'{phongban}')");
            return "Chuyển kho thành công";
        }
        public static string TaiNhapkho(string malinhkien, string soluong, string kho, string vitri, string thoigian, string giatien, string ghichu, string khoi, string nguoichuyen, string phongban)
        {
            SQL_Connect_DB20 _db =  new SQL_Connect_DB20();
            string update = $@"IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[KHO] WHERE [MaNguyenLieu] = '{malinhkien}' AND [Kho] = '{kho}')
                                BEGIN
                                    UPDATE [COST_MANAGEMENT].[dbo].[KHO]
                                    SET [QTY_RE_IMPORT] = '{soluong}',
                                        [GIA_TAI_NHAP] = '{giatien}',
                                        [QTY_NEW] = [Hientai] + {soluong},
                                        [DTM_UPDATE] = GETDATE()
                                    WHERE [MaNguyenLieu] = '{malinhkien}' AND [Kho] = '{kho}';   
                                    PRINT N'Đã cập nhật dữ liệu cho mã: ' + '{malinhkien}';
                                END ELSE
                                BEGIN
                                    INSERT INTO [COST_MANAGEMENT].[dbo].[KHO] 
                                        ([MaNguyenLieu], [Kho],[Hientai],[QTY_RE_IMPORT],[Group_Code],[nvchr_note], [GIA_TAI_NHAP], [DTM_UPDATE],[IS_SAVE_WH],[QTY_NEW])
                                    VALUES 
                                        ('{malinhkien}', '{kho}','0', '{soluong}', '{khoi}', N'{ghichu}', '{giatien}', GETDATE(),'0', '{soluong}');
                                    PRINT N'Đã thêm mới mã nguyên liệu: ' + '';
                                END";
            var _cmd = _db.GET_DATA_FROM_SQL(update);
            _db.GET_DATA_FROM_SQL($"INSERT INTO [KHO_NHAPXUAT] ([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Ngaynhaokho],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[Phong],[Vitri]) " +
                $"VALUES ('{malinhkien}',N'Tái nhập: {malinhkien}', '{soluong}','TAINHAP','{DateTime.Now.ToString("yyyy-MM-dd")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{nguoichuyen}','{kho}','{khoi}', N'{phongban}','{vitri}')");
            return "Tái nhập thành công !"; 
        }
        public static List<string> Get_location()
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20() ;
            List<string> _locations = new List<string>() ;
            var get = _db.GET_DATA_FROM_SQL("SELECT MaCost,MaChuyen FROM DEPARTMENT_VITRI ORDER BY [MaCost],[MaChuyen] ");
            for (int i = 0; i < get.Rows.Count; i++)
            {
                _locations.Add(get.Rows[i][0].ToString()! +":" + get.Rows[i][1].ToString()!);
            }
            return _locations;
        }
        public static string Del_Tainhap(string id)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            db.GET_DATA_FROM_SQL("  update [KHO] set QTY_NEW = QTY_NEW - QTY_RE_IMPORT, QTY_RE_IMPORT = '0', GIA_TAI_NHAP = '0' where Id_Kho = '" + id + "'");
            return "Xóa tái nhập thành công !";
        }
        public static string edit_tainhap(string id, string soluong, string donvi, string giatien, string kho)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            db.GET_DATA_FROM_SQL($"update [KHO] set QTY_NEW = (QTY_NEW - QTY_RE_IMPORT) + {soluong}, GIA_TAI_NHAP = '{giatien}',QTY_RE_IMPORT = '{soluong}' where Id_Kho = '{id}'");
            return "Sửa thành công !";
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
