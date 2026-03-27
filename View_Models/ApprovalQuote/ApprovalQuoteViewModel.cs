using PRJ_WAREHOUSE_BIVN.DTO;

namespace PRJ_WAREHOUSE_BIVN.View_Models.ApprovalQuote
{
    public class ApprovalQuoteViewModel
    {
        public List<DEPARTMENTDTO> listNhomVitri { get; set; } = new List<DEPARTMENTDTO>();
        public List<string> listSoDon { get; set; } = new List<string>();
        public List<MATERIALDTO> listMaterial { get; set; } = new List<MATERIALDTO>();
        public List<BaoGia_Request_of_QuotationDTO> listBaoGia { get; set; } = new List<BaoGia_Request_of_QuotationDTO>();
        public List<BaoGia_StatusDTO> listStatusBaoGia { get; set; } = new List<BaoGia_StatusDTO>();
        public List<BaoGia_StepDTO> listStepBaoGia { get; set; } = new List<BaoGia_StepDTO>();
        // list approvel
        public List<BaoGia_Master_Approver_Send_MailDTO> ListApprovel { get; set; } = new List<BaoGia_Master_Approver_Send_MailDTO>();
    }
    public class ApprovalQuoteSearchViewModel
    {
        public string? SoDon { get; set; }
        public string? MaHang { get; set; }
        public string? Section { get; set; }
        public string? StatusApprover { get; set; }
    }
}
