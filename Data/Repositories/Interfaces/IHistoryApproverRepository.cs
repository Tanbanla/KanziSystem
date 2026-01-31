using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IHistoryApproverRepository: IBaseRepository<BaoGia_History_Approver_of_Quotation, int>
    {
        // Lấy thông tin lịch sử phê duyệt báo giá theo mã báo giá
        Task<List<BaoGia_History_Approver_of_Quotation>> GetHistoryByQuotationIdAsync(int quotationId);
        // Lấy thông tin lịch sử phê duyệt báo giá theo số đơn
        Task<List<BaoGia_History_Approver_of_Quotation>> GetHistoryBySoDonAsync(string soDon);
        // Tìm kiếm lịch sử phê duyệt báo giá 
        Task<List<BaoGia_History_Approver_of_Quotation>> SearchHistoryAsync(int? quotationId, string? soDon, int? buoc, DateTime? fromDate, DateTime? toDate, string? approverName);
        // Thêm mới lịch sử phê duyệt báo giá
        Task<bool> AddHistoryAsync(BaoGia_History_Approver_of_Quotation history);
        // Thêm mới danh sách lịch sử phê duyệt báo giá 
        Task<bool> AddHistoryListAsync(List<BaoGia_History_Approver_of_Quotation> historyList);
        // Sửa thông tin lịch sử phê duyệt báo giá
        Task<bool> UpdateHistoryAsync(BaoGia_History_Approver_of_Quotation history);
        // Lấy danh sách phê duyệt của người dùng 
        Task<List<BaoGia_Request_of_Quotation>> GetListApprover(string adid, string? soDon, string? maHang, string? section, string? statusApprover);
    }
}
