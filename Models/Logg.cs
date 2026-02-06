namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class KHO_NHAPXUAT
    {
        public int? Id_Lichsu { get; set; }
        public string? MaNguyenLieu { get; set; }
        public string? Hanhdong { get; set; }
        public int? Soluong { get; set; }
        public string? Loai { get; set; }
        public string? Ngaynhaokho { get; set; }
        public string? Thoigian { get; set; }
        public string? Nguoicapnhat { get; set; }
        public string? Kho { get; set; }
        public string? Khoi { get; set; }
        public string? Phong { get; set; }
        public string? Vitri { get; set; }
        public static List<KHO_NHAPXUAT> _logg(string madon, string ngay_tu, string ngay_den, string kho, string manguyenlieu, string loai, string phong)
        {
            string timngay = "";
            if (ngay_tu != null || ngay_den != null)
            {
                timngay  = $"and Ngaynhaokho >= '{ngay_tu}' and Ngaynhaokho <= '{ngay_den}'";
            }
            
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<KHO_NHAPXUAT> lis_kho = new List<KHO_NHAPXUAT>();
            var lst = _db.GET_DATA_FROM_SQL($"SELECT TOP(2000) * FROM [COST_MANAGEMENT].[dbo].[KHO_NHAPXUAT] where Hanhdong like '%{madon}%' {timngay} and Kho like '%{kho}%' and MaNguyenLieu like '%{manguyenlieu}%' and Loai like '%{loai}%' and Phong like '%{phong}%' order by Id_Lichsu desc");
            for (int i = 0; i < lst.Rows.Count; i++)
            {
                lis_kho.Add(new KHO_NHAPXUAT
                {
                    Id_Lichsu = int.Parse(lst.Rows[i]["Id_Lichsu"].ToString()!),
                    MaNguyenLieu = lst.Rows[i]["MaNguyenLieu"].ToString()!,
                    Hanhdong = lst.Rows[i]["Hanhdong"].ToString()!,
                    Soluong = int.Parse(lst.Rows[i]["Soluong"].ToString()!),
                    Loai = lst.Rows[i]["Loai"].ToString()!,
                    Ngaynhaokho = lst.Rows[i]["Ngaynhaokho"].ToString()!,
                    Thoigian = lst.Rows[i]["Thoigian"].ToString()!,
                    Nguoicapnhat = lst.Rows[i]["Nguoicapnhat"].ToString()!,
                    Kho = lst.Rows[i]["Kho"].ToString()!,
                    Khoi = lst.Rows[i]["Khoi"].ToString()!,
                    Phong = lst.Rows[i]["Phong"].ToString()!,
                    Vitri = lst.Rows[i]["Vitri"].ToString()!,
                });
            }
            return lis_kho;
        }

    }
}
