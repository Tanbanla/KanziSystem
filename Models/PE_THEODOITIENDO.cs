namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class PE_THEODOITIENDO
    {
        public string? Ngayphathanh { get; set; }
        public int Id { get; set; }
        public int? Id_PO { get; set; }
        public string SoPO { get; set; }
        public string Id_Detail_PO { get; set; }
        public string? Ngay_gui_PO { get; set; }
        public string Anh_huong_SX { get; set; }
        public string? Ngay_NCC_xacnhanGH { get; set; }
        public string? Ngay_GHchinhthuc { get; set; }
        public string? Gio_GH { get; set; } // Hoặc string tuỳ kiểu dữ liệu SQL
        public string Cua_GH { get; set; }
        public string Cong_Nhanhang { get; set; }
        public string Nguoi_Nhanhang { get; set; }
        public string Nhacungcap { get; set; }
        public decimal? SL_Thucte { get; set; } // Dùng decimal hoặc float/int tuỳ SQL
        public string So_DNTT { get; set; }
        public string So_hoadon { get; set; }
        public string? Soluongantoan { get; set; }
        public string? Mahang { get; set; }
        public string? Tentiengviet { get; set; }
        public decimal? Soluong { get; set; }
        public string? Dovi { get; set; }
        public string? MaNCC { get; set; }
        public string? TenNCC { get; set; }
    }
}
