using DocumentFormat.OpenXml.Spreadsheet;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class KHO_NHAPXUAT
    {
        public int? Id_Lichsu { get; set; }
        public string? MaNguyenLieu { get; set; }
        public string? Hanhdong { get; set; }
        public double? Soluong { get; set; }
        public string? Loai { get; set; }
        public string? Ngaynhaokho { get; set; }
        public string? Thoigian { get; set; }
        public string? Nguoicapnhat { get; set; }
        public string? Kho { get; set; }
        public string? Khoi { get; set; }
        public string? Phong { get; set; }
        public string? Vitri { get; set; }
        public string? TenNguyenlieu { get; set; }
        public string? NCC { get; set; }
        public string? Donvi { get; set; }
        public string? MaNguoinhap { get; set; }
        public string? Gia { get; set; }
        public string? SoPO { get; set; }
        public string? SoluongPO { get; set; }
        public string? DonviPO { get; set; }
        public string? Soluongconlai { get; set; }
        public string? Sotaikhoan { get; set; }
        public string? Soluongtruocthaydoi { get; set; }
        public string? Soluongsauthaydoi { get; set; }

        public static List<KHO_NHAPXUAT> _logg(string madon, string ngay_tu, string ngay_den, string kho, string manguyenlieu, string loai,  string us)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();

            string khoii = _db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");

            string timngay = "";
            if (ngay_tu != null || ngay_den != null)
            {
                timngay  = $"and Ngaynhaokho >= '{ngay_tu}' and Ngaynhaokho <= '{ngay_den}'";
            }
            string timloai = $"Loai like '%{loai}%'";
            if(!string.IsNullOrWhiteSpace(loai))
            {
                timloai = $"Loai = '{loai.Trim()}'";
            }
           
            List<KHO_NHAPXUAT> lis_kho = new List<KHO_NHAPXUAT>();
            var lst = _db.GET_DATA_FROM_SQL($"SELECT TOP(200) * FROM [COST_MANAGEMENT].[dbo].[KHO_NHAPXUAT] where Hanhdong like '%{madon}%' {timngay} and Kho like '%{kho}%' and Khoi = '{khoii}' and MaNguyenLieu like '%{manguyenlieu}%' and {timloai} order by Id_Lichsu desc");
            for (int i = 0; i < lst.Rows.Count; i++)
            {
                lis_kho.Add(new KHO_NHAPXUAT
                {
                    Id_Lichsu = int.Parse(lst.Rows[i]["Id_Lichsu"].ToString()!),
                    MaNguyenLieu = lst.Rows[i]["MaNguyenLieu"].ToString()!,
                    Hanhdong = lst.Rows[i]["Hanhdong"].ToString()!,
                    Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!),
                    Loai = lst.Rows[i]["Loai"].ToString()!,
                    Ngaynhaokho = lst.Rows[i]["Ngaynhaokho"].ToString()!,
                    Thoigian = lst.Rows[i]["Thoigian"].ToString()!,
                    Nguoicapnhat = lst.Rows[i]["Nguoicapnhat"].ToString()!,
                    Kho = lst.Rows[i]["Kho"].ToString()!,
                    Khoi = lst.Rows[i]["Khoi"].ToString()!,
                    Phong = lst.Rows[i]["Phong"].ToString()!,
                    Vitri = lst.Rows[i]["Vitri"].ToString()!,
                    SoPO = lst.Rows[i]["SoPO"].ToString()!,
                    SoluongPO = lst.Rows[i]["SoluongPO"].ToString()!,
                    DonviPO = lst.Rows[i]["DonviPO"].ToString()!,
                });
            }
            return lis_kho;
        }
        
        public static List<KHO_NHAPXUAT> _truyxuat(string malinhkien, string kho)
        {
            SQL_Connect_DB20 _db = new SQL_Connect_DB20();
            List<KHO_NHAPXUAT> lis_kho = new List<KHO_NHAPXUAT>();
            var lst = _db.GET_DATA_FROM_SQL($"SELECT TOP (200) * FROM [COST_MANAGEMENT].[dbo].[KHO_NHAPXUAT] where MaNguyenLieu = '{malinhkien}' and Kho = '{kho}' order by Id_Lichsu desc");
            for (int i = 0; i < lst.Rows.Count; i++)
            {
                lis_kho.Add(new KHO_NHAPXUAT
                {
                    Id_Lichsu = int.Parse(lst.Rows[i]["Id_Lichsu"].ToString()!),
                    MaNguyenLieu = lst.Rows[i]["MaNguyenLieu"].ToString()!,
                    Hanhdong = lst.Rows[i]["Hanhdong"].ToString()!,
                    Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!),
                    Loai = lst.Rows[i]["Loai"].ToString()!,
                    Ngaynhaokho = lst.Rows[i]["Ngaynhaokho"].ToString()!,
                    Thoigian = lst.Rows[i]["Thoigian"].ToString()!,
                    Nguoicapnhat = lst.Rows[i]["Nguoicapnhat"].ToString()!,
                    Kho = lst.Rows[i]["Kho"].ToString()!,
                    Khoi = lst.Rows[i]["Khoi"].ToString()!,
                    Phong = lst.Rows[i]["Phong"].ToString()!,
                    Vitri = lst.Rows[i]["Vitri"].ToString()!,
                    TenNguyenlieu = lst.Rows[i]["TenNguyenlieu"].ToString()!,
                    NCC = lst.Rows[i]["NCC"].ToString()!,
                    Donvi = lst.Rows[i]["Donvi"].ToString()!,
                    MaNguoinhap = lst.Rows[i]["MaNguoinhap"].ToString()!,
                    Gia = lst.Rows[i]["Gia"].ToString()!,                  
                    SoPO = lst.Rows[i]["SoPO"].ToString()!,
                    SoluongPO = lst.Rows[i]["SoluongPO"].ToString()!,
                    DonviPO = lst.Rows[i]["DonviPO"].ToString()!,
                    Soluongconlai = lst.Rows[i]["Soluongconlai"].ToString()!,
                    Sotaikhoan = lst.Rows[i]["Sotaikhoan"].ToString()!,
                    Soluongtruocthaydoi = lst.Rows[i]["Soluongtruocthaydoi"].ToString()!,
                    Soluongsauthaydoi = lst.Rows[i]["Soluongsauthaydoi"].ToString()!,
                });
            }
            return lis_kho;
        }
    }
}
