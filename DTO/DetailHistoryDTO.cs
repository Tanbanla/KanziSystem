namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class DetailHistoryDTO
    {
        public int ID { get; set; }

        public int ID_RequestQuote { get; set; }

        public string CHR_MaDon { get; set; } = null!;

        public string CHR_UpdateBy { get; set; } = null!;

        public string? NVCHR_UpdateName { get; set; }

        public DateTime CHR_Updatedate { get; set; }

        public string? CHR_OldData { get; set; }

        public string? CHR_NewData { get; set; }

        public string? NVCHR_LyDo { get; set; }

        public string? CHR_ActionType { get; set; }

        public string? NameVN { get; set; }
        public string? NameEN { get; set; }
        public string? NameJP { get; set; }
    }
}
