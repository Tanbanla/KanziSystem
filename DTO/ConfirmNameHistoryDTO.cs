using System;

namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class ConfirmNameHistoryDTO
    {
        public int Id { get; set; }
        public int? ConfirmId { get; set; }
        public int? QuotationId { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? ActionType { get; set; }
        public string? ActionBy { get; set; }
        public string? Section { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Note { get; set; }
    }
}
