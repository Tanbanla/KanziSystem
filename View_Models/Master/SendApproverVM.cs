using PRJ_WAREHOUSE_BIVN.DTO;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Master
{
    public class SendApproverVM
    {
        // danh sach phong ban
        public List<TM_SECTIONDTO> SectionCodes { get; set; }
        // danh sách step
        public List<BaoGia_StepDTO> baoGiaSteps { get; set; }
        public List<ACC_NHOMVITRIDTO> NhomViTris { get; set; }
        // Dữ liệu 
        public List<BaoGia_Master_Approver_Send_MailDTO> listMasterApprover { get; set; }
        // phân trang
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class DeleteApproverRequest
    {
        public int Id { get; set; }
    }
}
