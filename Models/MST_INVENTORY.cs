using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Drawing;

namespace PRJ_WAREHOUSE_BIVN.Models

{
    public class MST_INVENTORY
    {
        public int? Id_Kho { get; set; }
        public string? MaNguyenLieu { get; set; }
        public double? Hientai { get; set; }
        public string? ToiThieu { get; set; }
        public string? ToiDa { get; set; }
        public string? Group_Code { get; set; }
        public string? Kho { get; set; }
        public string? nvchr_note { get; set; }
        public string? NVCHR_COST { get; set; }
        public string? DTM_UPDATE { get; set; }
        public string? IS_SAVE_WH { get; set; }
        public string? Material_Name { get; set; }
        public double? QTY_NEW { get; set; }
        public double? QTY_RE_IMPORT { get; set; }
        public string? GIA_TAI_NHAP { get; set; }
        public string? Unit { get; set; }
        public string? Unit_Note { get; set; }
        public string? UserName { get; set; }
        public static List<MST_INVENTORY> inventory_process(MST_INVENTORY para)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            //para.MaNguyenLieu = para.MaNguyenLieu is null ? "NULL" : para.MaNguyenLieu!.ToString().Length > 0 ? $"N'{para.MaNguyenLieu}'" : "NULL";
            //para.Kho = para.Kho is null ? "NULL" : para.Kho!.ToString().Length > 0 ? $"N'{para.Kho}'" : "NULL";
            //para.NVCHR_COST = para.NVCHR_COST is null ? "NULL" : para.NVCHR_COST!.ToString().Length > 0 ? $"N'{para.NVCHR_COST}'" : "NULL";
            //para.IS_SAVE_WH = para.IS_SAVE_WH is null ? "NULL" : para.IS_SAVE_WH!.ToString().Length > 0 ? $"N'{para.IS_SAVE_WH}'" : "NULL";
            var khoi = _context.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + para.UserName + "'");

            var _cmd = _context.GET_DATA_FROM_SQL($" select * from KHO as a left join MATERIAL as b on a.MaNguyenLieu =  b.Material_Code where MaNguyenLieu like '%{para.MaNguyenLieu}%' and a.Group_Code = '{khoi}' and Kho like '%{para.Kho}%'");
            List<MST_INVENTORY> _inv = new List<MST_INVENTORY>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                _inv.Add(new MST_INVENTORY
                {
                    Id_Kho = int.Parse(_cmd.Rows[i]["Id_Kho"].ToString()!),
                    MaNguyenLieu = _cmd.Rows[i]["MaNguyenLieu"].ToString(),
                    Hientai = double.Parse(_cmd.Rows[i]["Hientai"].ToString()!),
                    ToiThieu = _cmd.Rows[i]["ToiThieu"].ToString(),
                    ToiDa = _cmd.Rows[i]["ToiDa"].ToString(),
                    Group_Code = _cmd.Rows[i]["Group_Code"].ToString(),
                    Kho = _cmd.Rows[i]["Kho"].ToString(),
                    nvchr_note = _cmd.Rows[i]["nvchr_note"].ToString(),
                    NVCHR_COST = _cmd.Rows[i]["NVCHR_COST"].ToString(),
                    DTM_UPDATE = _cmd.Rows[i]["DTM_UPDATE"].ToString(),
                    IS_SAVE_WH = _cmd.Rows[i]["IS_SAVE_WH"].ToString(),
                    Material_Name = _cmd.Rows[i]["Material_Name_VN"].ToString(),
                    QTY_NEW = double.Parse(_cmd.Rows[i]["QTY_NEW"].ToString()!),
                    QTY_RE_IMPORT = double.Parse(_cmd.Rows[i]["QTY_RE_IMPORT"].ToString()!),
                    GIA_TAI_NHAP = _cmd.Rows[i]["GIA_TAI_NHAP"].ToString(),
                    Unit = _cmd.Rows[i]["Unit"].ToString(),
                    Unit_Note = _cmd.Rows[i]["Unit_Note"].ToString(),
                });
            }
            return _inv;
        }
        public static List<MST_INVENTORY> exportExcel_kho()
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
         
            var _cmd = _context.GET_DATA_FROM_SQL($" select * from KHO as a left join MATERIAL as b on a.MaNguyenLieu =  b.Material_Code where Hientai is not null ");
            List<MST_INVENTORY> _inv = new List<MST_INVENTORY>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                _inv.Add(new MST_INVENTORY
                {
                    //Id_Kho = int.Parse(_cmd.Rows[i]["Id_Kho"].ToString()!),
                    MaNguyenLieu = _cmd.Rows[i]["MaNguyenLieu"].ToString(),
                    Hientai = double.Parse(_cmd.Rows[i]["Hientai"].ToString()!),
                    
                    Group_Code = _cmd.Rows[i]["Group_Code"].ToString(),
                    Kho = _cmd.Rows[i]["Kho"].ToString(),
                    nvchr_note = _cmd.Rows[i]["nvchr_note"].ToString(),
                    NVCHR_COST = _cmd.Rows[i]["NVCHR_COST"].ToString(),
                    DTM_UPDATE = _cmd.Rows[i]["DTM_UPDATE"].ToString(),
                    IS_SAVE_WH = _cmd.Rows[i]["IS_SAVE_WH"].ToString(),
                    Material_Name = _cmd.Rows[i]["Material_Name_VN"].ToString(),
                    //GIA_TAI_NHAP = _cmd.Rows[i]["GIA_TAI_NHAP"].ToString(),
                    Unit = _cmd.Rows[i]["Unit"].ToString(),
                    Unit_Note = _cmd.Rows[i]["Unit_Note"].ToString(),
                });
            }
            return _inv;
        }
        public static List<string> _getname_material(string group_code, string loaichiphi)
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            var _cmd = _context.GET_DATA_FROM_SQL("SELECT distinct(MaNguyenLieu), Material_Name_VN FROM [COST_MANAGEMENT].[dbo].[KHO] as a left join MATERIAL as b on a.MaNguyenLieu = b.Material_Code where a.Group_Code = '" + group_code + "' and a.MaNguyenLieu like '"+ loaichiphi +"%'");
            List<string> material = new List<string>();

            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                material.Add(_cmd.Rows[i]["MaNguyenLieu"].ToString() + ":" + _cmd.Rows[i]["Material_Name_VN"].ToString());
            }
            return material;
        }
        public static List<string> _getname_material()
        {
            SQL_Connect_DB20 _context = new SQL_Connect_DB20();
            var _cmd = _context.GET_DATA_FROM_SQL("SELECT distinct(MaNguyenLieu), Material_Name_VN FROM [COST_MANAGEMENT].[dbo].[KHO] as a left join MATERIAL as b on a.MaNguyenLieu = b.Material_Code where a.Hientai > 0");
            List<string> material = new List<string>();

            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                material.Add(_cmd.Rows[i]["MaNguyenLieu"].ToString() + ":" + _cmd.Rows[i]["Material_Name_VN"].ToString());
            }
            return material;
        }
        public static string _chuyenkho(string malinhkien, string soluonghientai, string khochuyen, string khonhan, string vitri, string soluongchuyen, string ngaychuyen, string us)
        {
            SQL_Connect_DB20 con = new SQL_Connect_DB20();
            var khoi = con.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");

            con.ReturnString("UPDATE KHO SET [Hientai] = [Hientai] - " + Convert.ToDouble(soluongchuyen) + " WHERE [MaNguyenLieu] =  N'" + malinhkien + "'  AND [Kho] = '" + khochuyen + "' AND [Group_Code] = '" + khoi + "'");
            string trt =vitri;
            string[] Vt = trt.Split(':');
            con.ReturnString("INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[Ngaynhaokho],[Phong],[Vitri],[Soluongtruocthaydoi],[Soluongsauthaydoi]) VALUES(N'" + malinhkien + "',N'Chuyển từ kho " + khochuyen + " sang kho " + khonhan + "','" + soluongchuyen + "','XUAT',GETDATE(),'" + us + "','" + khochuyen + "','" + khoi + "','" + ngaychuyen + "','" + Vt[0] + "','" + Vt[1] + "','" + soluonghientai + "','" + (Convert.ToDouble(soluonghientai) - Convert.ToDouble(soluongchuyen)) + "')");
            // Nhập kho
            string Soluonghientai = con.ReturnString("SELECT Hientai FROM KHO WHERE [MaNguyenLieu] =  N'" + malinhkien + "' AND [Kho] = '" + khonhan + "' AND [Group_Code] = '" + khoi + "' ");
            double SoluongTruocthaydoi = 0;
            if (Soluonghientai.Trim() == "")
            {
                con.ReturnString("INSERT INTO KHO(MaNguyenLieu,Hientai,Group_Code,Kho) VALUES (N'" + malinhkien + "','" + Convert.ToDouble(soluongchuyen) + "','" + khoi + "','" + khonhan + "')");
            }
            else
            {
                con.ReturnString("UPDATE KHO SET [Hientai] = [Hientai] + " + Convert.ToDouble(soluongchuyen) + " WHERE [MaNguyenLieu] =  N'" + malinhkien + "' AND [Kho] = '" + khonhan + "' AND [Group_Code] = '" + khoi + "'");
                SoluongTruocthaydoi = Convert.ToDouble(Soluonghientai);
            }
            //**************
            con.ReturnString("INSERT INTO [KHO_NHAPXUAT]([MaNguyenLieu],[Hanhdong],[Soluong],[Loai],[Thoigian],[Nguoicapnhat],[Kho],[Khoi],[Ngaynhaokho],[Soluongtruocthaydoi],[Soluongsauthaydoi]) VALUES(N'" + malinhkien + "',N'Nhận từ kho " + khochuyen + " sang kho " + khonhan + "','" + Convert.ToDouble(soluongchuyen) + "','NHAP',GETDATE(),'" + us + "','" + khonhan + "','" + khoi + "','" + ngaychuyen + "','" + SoluongTruocthaydoi + "','" + (Convert.ToDouble(soluongchuyen) + SoluongTruocthaydoi) + "')");
            return "OK";
        }
    }
    public class User_Info
    {
        public int Id_User_Dept { get; set; }
        public string? CHR_USERID { get; set; }
        public string? Cost_Center { get; set; }
        public int Id_Dept { get; set; }
        public string? Name { get; set; }
        public string? Name_Jp { get; set; }
        public string? Cost_Center_Group { get; set; }
        public string? Active { get; set; }
        public string? AcceptRequest { get; set; }
        public static List<User_Info> _info_adid(string us)
        {
            SQL_Connect_DB20 _db20 = new SQL_Connect_DB20();
            var _cmd = _db20.GET_DATA_FROM_SQL("select * from USER_DEPT as a left join [DEPARTMENT] as b on a.Cost_Center = b.Cost_Center where a.CHR_USERID = '" + us + "'");
            List<User_Info> us_f = new List<User_Info>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                us_f.Add(new User_Info
                {
                    Id_User_Dept = int.Parse(_cmd.Rows[i]["Id_User_Dept"].ToString()!),
                    CHR_USERID = _cmd.Rows[i]["CHR_USERID"].ToString(),
                    Id_Dept = int.Parse(_cmd.Rows[i]["Id_Dept"].ToString()!),
                    Cost_Center = _cmd.Rows[i]["Cost_Center"].ToString(),
                    Name = _cmd.Rows[i]["Name"].ToString(),
                    Name_Jp = _cmd.Rows[i]["Name_Jp"].ToString(),
                    Cost_Center_Group = _cmd.Rows[i]["Cost_Center_Group"].ToString(),
                    Active = _cmd.Rows[i]["Active"].ToString(),
                    AcceptRequest = _cmd.Rows[i]["AcceptRequest"].ToString(),
                });
            }
            return us_f;
        }
        public static List<string> _GetCostCenter()
        {
            SQL_Connect_DB20 _db20 = new SQL_Connect_DB20();
            var _cmd = _db20.GET_DATA_FROM_SQL("SELECT [Cost_Center], [Name] FROM [COST_MANAGEMENT].[dbo].[DEPARTMENT]");
            List<string> result = new List<string>();
            for (int i = 0; i < _cmd.Rows.Count; i++)
            {
                string data = _cmd.Rows[i]["Cost_Center"].ToString() + ":" + _cmd.Rows[i]["Name"].ToString();
                result.Add(data);
            }
            return result;
        }
        public static List<string> _GetLocation()
        {
            string command = "SELECT MaChuyen FROM [COST_MANAGEMENT].[dbo].[DEPARTMENT_VITRI] ";
            SQL_Connect_DB20 _db20 = new SQL_Connect_DB20();
            List<string> result = new List<string>();
            var getData = _db20.GET_DATA_FROM_SQL(command);
            for (int idx = 0; idx < getData.Rows.Count; idx++)
            {
                string data = getData.Rows[idx]["MaChuyen"].ToString()!;
                result.Add(data);
            }
            return result;
        }
    }
}
