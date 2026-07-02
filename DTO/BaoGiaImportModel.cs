namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class BaoGiaImportModel
    {
        public int Row { get; set; }

        public string? MaDon { get; set; }
        public int ID { get; set; }
        public string? MaThietBi { get; set; }
        public string? MaHangNoiBo { get; set; }
        public string? CodeVender { get; set; }
        public string? MaHangNCC_BIVN { get; set; }
        public string? MaHangNCC_Vendor { get; set; }
        public string? TenHangVN { get; set; }
        public string? TenHangEng { get; set; }
        public string? ChungLoaiHang { get; set; }

        public double? DonGiaUSD { get; set; }
        public double? DonGiaVND { get; set; }
        public double? SoLuong { get; set; }

        public string? DonVi { get; set; }
        public string? NhaSanXuat { get; set; }
        public string? BIT_Select { get; set; }
        public string? NVCHR_ReasonPick { get; set; }
        public string? NVCHR_Note { get; set; }
    }
}
