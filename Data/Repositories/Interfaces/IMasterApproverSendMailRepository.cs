using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IMasterApproverSendMailRepository:IBaseRepository<BaoGia_Master_Approver_Send_Mail, int>
    {
        // lấy dữ liệu theo điều kiện và phân trang 
        public Task<List<BaoGia_Master_Approver_Send_Mail>> GetByConditionAsync(string? sectionCode, string? adid,int? IdStep , int pageIndex, int pageSize);
        // Lưu thông tin
        public Task<bool> SaveMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj);
        // Sửa thông tin 
        public Task<bool> UpdateMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_Mail obj);
        // Xóa thông tin 
        public Task<bool> DeleteMasterApproverSendMailAsync(int id, string userAction);
        // Lấy thông tin phê duyệt step của phòng ban
        public Task<List<BaoGia_Master_Approver_Send_Mail>> GetApproverByStepAndSectionAsync(int idStep, string sectionCode);
        // Inser thông tin và đăng ký user đăng nhập
        public Task<bool> InsertMasterApproverSendMailAsync(List<BaoGia_Master_Approver_Send_Mail> dtos);
    }
}
