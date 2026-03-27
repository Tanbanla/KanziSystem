namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class VENDER
    {
        public string? Ncc_Id { get; set; }
        public string? Ma { get; set; }
        public string? Ten { get; set; }
        public string? Diachi { get; set; }
        public string? Sodienthoai { get; set; }
        public string? Fax { get; set; }
        public string? Khuvuc { get; set; }
        public string? Ghichu { get; set; }
        public string? Hinhthucmotk { get; set; }
        public string? Dieukienthanhtoan { get; set; }
        public string? Masothue { get; set; }
        public string? Nhanvienkinhdoand { get; set; }
        public string? Nhanvienketoan { get; set; }
        public string? Canphaixacnhanlamthutuchaiquan { get; set; }
        public string? nhom { get; set; }
        public string? nguoi_cap_nhat { get; set; }

    }
    public class Vender_process
    {
        public static List<VENDER> _listVender(VENDER vder)
        {
            vder.Ma = vder.Ma is null ? "NULL" : vder.Ma!.ToString().Length > 0 ? $"N'{vder.Ma}'" : "NULL";
            vder.Ten = vder.Ten is null ? "NULL" : vder.Ten!.ToString().Length > 0 ? $"N'{vder.Ten}'" : "NULL";
            vder.nhom = vder.nhom is null ? "NULL" : vder.nhom!.ToString().Length > 0 ? $"N'{vder.nhom}'" : "NULL";
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            List<VENDER> lst = new List<VENDER>();
            var get_data = sql.GET_DATA_FROM_SQL($"EXEC [dbo].[PE_IM_NCC_NEW_GetData] {vder.Ma}, {vder.Ten}, {vder.nhom}");
            for(int i = 0; i < get_data.Rows.Count; i++)
            {
                lst.Add(new VENDER
                {
                    Ncc_Id = get_data.Rows[i]["Ncc_Id"].ToString(),
                    Ma = get_data.Rows[i]["Ma"].ToString(),
                    Ten = get_data.Rows[i]["Ten"].ToString(),
                    Diachi = get_data.Rows[i]["Diachi"].ToString(),
                    Sodienthoai = get_data.Rows[i]["Sodienthoai"].ToString(),
                    Fax = get_data.Rows[i]["Fax"].ToString(),
                    Khuvuc = get_data.Rows[i]["Khuvuc"].ToString(),
                    Ghichu = get_data.Rows[i]["Ghichu"].ToString(),
                    Hinhthucmotk = get_data.Rows[i]["Hinhthucmotk"].ToString(),
                    Dieukienthanhtoan = get_data.Rows[i]["Dieukienthanhtoan"].ToString(),
                    Masothue = get_data.Rows[i]["Masothue"].ToString(),
                    Nhanvienketoan = get_data.Rows[i]["Nhanvienketoan"].ToString(),
                    Canphaixacnhanlamthutuchaiquan = get_data.Rows[i]["Canphaixacnhanlamthutuchaiquan"].ToString(),
                    nhom = get_data.Rows[i]["nhom"].ToString(),
                    nguoi_cap_nhat = get_data.Rows[i]["nguoi_cap_nhat"].ToString()
                });     
            }
            return lst;
        }
    }
}
