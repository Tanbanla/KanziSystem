namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class Quanlythanhtoan
    {
        public long PO_Detail_Id { get; set; }
        public string? Id_Goc { get; set; }
        public string? Benxacnhantruoc { get; set; }
        public string? SoPO { get; set; }
        public string? Mahang { get; set; }
        public string? MaSanPham { get; set; } // Từ [Good_Code] as MaSanPham
        public string? Tentienganh { get; set; }
        public string? Tentiengviet { get; set; }
        public decimal? Soluong { get; set; }
        public decimal? Luongvethucte { get; set; }
        public decimal? Luongvekho { get; set; }
        public decimal? Luongvekhocu { get; set; } // Từ Luongvekho as Luongvekhocu
        public DateTime? LuongvekhoNgaynhap { get; set; }
        public string? InvoicePO { get; set; }
        public string? InvoicePODenghithanhtoan { get; set; }
        public DateTime? InvoicePONgaynhap { get; set; }
        public string? InvoicePONguoinhap { get; set; }
        public string? Dovi { get; set; }
        public decimal? Dongia { get; set; }
        public string? Dieukiengiaohang { get; set; }
        public string? Diadiemgiaohang { get; set; }
        public string? Phuongthucvanchuyen { get; set; }
        public decimal? Sotien { get; set; }
        public decimal? Vat { get; set; }
        public string? Maphongyeucau { get; set; }
        public string? Tenphongyeucau { get; set; }
        public DateTime? Ngaygiaohangdukien { get; set; }
        public string? Noigiaodukien { get; set; }
        public string? Thoigianthanhtoan { get; set; }
        public string? Id_RequestDetail { get; set; }
        public string? Loaitien { get; set; }
        public decimal? Tygia { get; set; }
        public decimal? DoisangUSD { get; set; }
        public string? Danhmuc { get; set; }
        public string? Code_Request { get; set; }
        public string? TinhtrangPO { get; set; }
        public string? Tinhtrangtokhai { get; set; }
    }
}
