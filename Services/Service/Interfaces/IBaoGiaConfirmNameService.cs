using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaConfirmNameService : IBaseService<BaoGia_Confirm_Name_Quotation, int, BaoGia_Confirm_Name_QuotationDTO>
    {
        // search thông tin xác nhận tên hàng
        public Task<GenericResponse<ListRequest<dynamic>>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user, int pageIndex, int pageSize);
        // Luu thong tin
        public Task<GenericResponse<bool>> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User);
        // Them thong tin 
        public Task<GenericResponse<bool>> AddConfirmNameAsync(BaoGia_Confirm_Name_QuotationDTO confirmName);
        // Approve ConfirmName
        public Task<GenericResponse<bool>> ApproveConfirmNameAsync(int id, string approvedBy);
        // Reject ConfirmName
        public Task<GenericResponse<bool>> RejectConfirmNameAsync(int id, string reason, string rejectedBy);
        // Insert thong tin danh sach
        public Task<GenericResponse<bool>> AddListAsync(List<BaoGia_Confirm_Name_QuotationDTO> confirmNames);
        // luu thong tin nhap file
        public Task<GenericResponse<bool>> SaveFromFileAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames, string user, string? Role);
        // luu thong tin 
        public Task<GenericResponse<bool>> SaveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Approvers
        public Task<GenericResponse<bool>> ApproveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Rejects Acc
        public Task<GenericResponse<bool>> RejectAccConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Export Code Cofirmed
        public Task<GenericResponse<List<dynamic>>> ExportCodeConfirmedAsync();
        // Từ chối xác nhận tên hàng
        public Task<GenericResponse<bool>> RejectConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Check mã đơn đã xác nhận tên hàng đã hoàn thành hay chưa
        public Task<GenericResponse<List<ResultCheckCofirmName>>> CheckDonHangConfirmedAsync(List<int> listCheck);
        // Cập nhật thông tin đơn báo giá sau khi trả lại
        public Task<GenericResponse<bool>> UpdateRequestFromFileAsync(List<BaoGia_Request_of_QuotationDTO> baoGia, string user);
        // Cập nhật thông tin yêu cầu PIC PUR cần xác nhận lại báo giá
        public Task<GenericResponse<bool>> UpdateRequestForPICPURAsync(List<int> baoGia, string user);
    }
}
