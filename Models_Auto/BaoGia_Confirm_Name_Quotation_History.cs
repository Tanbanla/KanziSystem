namespace PRJ_WAREHOUSE_BIVN.Models_Auto
{
    public class BaoGia_Confirm_Name_Quotation_History
    {
        public int ID { get; set; }
        public int? QuotationID { get; set; }
        public int? ConfirmID { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

    }
}
