namespace PRJ_WAREHOUSE_BIVN.Models
{
    public class GiaoHangMasterViewModel
    {
        public int Id { get; set; }
        public string? Mahang { get; set; }
        public string? Tenhang { get; set; }
        public string? Vendor_Code { get; set; }
        public string? Vendor { get; set; }
        public string? Maker { get; set; }
        public string? MOQ { get; set; }
        public string? Tansuatgiaohang { get; set; }
        public double? Leadtimegiaohang { get; set; }

        public double? TiLeTonKhoAnToan { get; set; }

        public double? Songaytonkhoantoan { get; set; }
        public string? Donvi { get; set; }
        public double? Soluongtonkhoantoan { get; set; }

        // Các cột bổ sung theo hình ảnh (Lấy từ bảng Using/Stock hoặc tính toán)
        public double? UsingThangHienTai { get; set; }
        public double? StockHienTai { get; set; }
        public double? SoNgaySuDungHienTai { get; set; }
        public double? TiLeTonKhoHienTai { get; set; } // %
        public string? DiemGoiHang { get; set; }       // "OK", "ĐỐI ỨNG GẤP", "GỌI HÀNG", "CHƯA DÙNG"
    }
}
