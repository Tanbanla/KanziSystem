using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IHistoryApproverServive: IBaseService<BaoGia_History_Approver_of_Quotation, int , BaoGia_History_Approver_of_QuotationDTO>
    {
        // Lấy thông tin lịch sử phê duyệt báo giá theo mã báo giá
        Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> GetHistoryByQuotationIdAsync(int quotationId);
        // Lấy thông tin lịch sử phê duyệt báo giá theo số đơn
        Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> GetHistoryBySoDonAsync(string soDon);
        // Tìm kiếm lịch sử phê duyệt báo giá
        Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> SearchHistoryAsync(int? quotationId, string? soDon, int? buoc, DateTime? fromDate, DateTime? toDate, string? approverName);
        // Thêm mới lịch sử phê duyệt báo giá
        Task<GenericResponse<bool>> AddHistoryAsync(BaoGia_History_Approver_of_QuotationDTO history);
        // Thêm mới danh sách lịch sử phê duyệt báo giá 
        Task<GenericResponse<bool>> AddHistoryListAsync(List<BaoGia_History_Approver_of_QuotationDTO> historyList);
        // Sửa thông tin lịch sử phê duyệt báo giá
        Task<GenericResponse<bool>> UpdateHistoryAsync(BaoGia_History_Approver_of_QuotationDTO history);
        // Lấy danh sách phê duyệt của người dùng 
        Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> GetListApprover(string adid, string? soDon, string? maHang, string? section, string? statusApprover);
    }
}
