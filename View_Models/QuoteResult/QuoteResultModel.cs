using PRJ_WAREHOUSE_BIVN.Models_Auto;

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
    public class PriceMasterHistoryExportRequest
    {
        public string InternalPartCode { get; set; } = string.Empty;
        public string? VendorCode { get; set; }
        public string? QuotationRequestNo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public sealed class PriceMasterImportRow
    {
        public int ExcelRow { get; init; }
        public string[] Values { get; init; } = Array.Empty<string>();
        public string Errors { get; init; } = string.Empty;
        public TM_PRICE_MASTER? Entity { get; init; }
    }
}
