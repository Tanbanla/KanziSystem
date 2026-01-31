using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Reflection.Metadata;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IMasterApproverSendMailService: IBaseService<BaoGia_Master_Approver_Send_Mail, int, BaoGia_Master_Approver_Send_MailDTO>
    {
        // lấy dữ liệu theo điều kiện và phân trang 
        public Task<GenericResponse<List<BaoGia_Master_Approver_Send_MailDTO>>> GetByConditionAsync(string? sectionCode, string? adid, int? IdStep, int pageIndex, int pageSize);
        // Lưu thông tin
        public Task<GenericResponse<bool>> SaveMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_MailDTO obj);
        // Sửa thông tin 
        public Task<GenericResponse<bool>> UpdateMasterApproverSendMailAsync(BaoGia_Master_Approver_Send_MailDTO obj);
        // Xóa thông tin 
        public Task<GenericResponse<bool>> DeleteMasterApproverSendMailAsync(int id, string userAction);
    }
}
