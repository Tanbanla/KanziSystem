using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaConfirmNameRepository : IBaseRepository<BaoGia_Confirm_Name_Quotation, int>
    {
        // search thông tin xác nhận tên hàng
        public Task<ListRequest<dynamic>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user, int pageIndex, int pageSize);
        // Luu thong tin
        public Task<bool> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User);
        // Them thong tin 
        public Task<bool> AddConfirmNameAsync(BaoGia_Confirm_Name_Quotation confirmName);
        // Approve ConfirmName
        public Task<bool> ApproveConfirmNameAsync(int id, string approvedBy);
        // Reject ConfirmName
        public Task<bool> RejectConfirmNameAsync(int id, string reason, string rejectedBy);

        // Insert thong tin danh sach
        public Task<bool> AddListAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames);
        // luu thong tin nhap file
        public Task<bool> SaveFromFileAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames, string user, string? Role);
        // Saves
        public Task<bool> SaveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Approvers
        public Task<bool> ApproveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Rejects Acc
        public Task<bool> RejectAccConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Export Code Cofirmed
        public Task<List<dynamic>> ExportCodeConfirmedAsync();
        // Từ chối xác nhận tên hàng
        public Task<bool> RejectConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Check mã đơn đã xác nhận tên hàng đã hoàn thành hay chưa
        public Task<List<ResultCheckCofirmName>> CheckDonHangConfirmedAsync(List<int> listCheck);
        // Cập nhật thông tin đơn báo giá sau khi trả lại
        public Task<bool> UpdateRequestFromFileAsync(List<BaoGia_Request_of_Quotation> baoGia, string user);
        // Cập nhật thông tin yêu cầu PIC PUR cần xác nhận lại báo giá
        public Task<bool> UpdateRequestForPICPURAsync(List<BaoGia_Confirm_Name_Quotation> baoGia, string user);
        //Export file ten hanh xac nhan
        Task<List<dynamic>> ExportConfirmedMaterialNamesAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user);
    }
}
