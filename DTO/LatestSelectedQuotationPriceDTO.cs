namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class LatestSelectedQuotationPriceDTO
    {
        public string MaHangNoiBo { get; set; } = string.Empty;
        public double? FL_VND { get; set; }
        public double? FL_USD { get; set; }
        public DateTime DTM_CreateDate { get; set; }
    }
}
