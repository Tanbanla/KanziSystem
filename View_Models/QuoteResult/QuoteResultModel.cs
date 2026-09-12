namespace PRJ_WAREHOUSE_BIVN.View_Models.QuoteResult
{
    public class QuoteResultModel
    {
    }

    public class SearchQuoteResultViewModel
    {
        public string? MaDon { get; set; }
        public string? MaNcc { get; set; }
        public string? MaThietBi { get; set; }
        public string? MaHangNoiBo { get; set; }
        public string? MaHangNcc { get; set; }
        public string? TrangThai { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? NhomHang { get; set; }
        public DateTime? to { get; set; }
        public DateTime? from { get; set; }
        public string? ChungLoai { get; set; }
    }
}
