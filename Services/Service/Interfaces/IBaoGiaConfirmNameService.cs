using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaConfirmNameService : IBaseService<BaoGia_Confirm_Name_Quotation, int, BaoGia_Confirm_Name_QuotationDTO>
    {
        // search thông tin xác nhận tên hàng
        public Task<GenericResponse<ListRequest<dynamic>>> SearchAsync(ConfirmNameSearchRequest req, string user, string? role);
        // Luu thong tin
        public Task<GenericResponse<bool>> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User);
        public Task<GenericResponse<bool>> EditConfirmNameAsync(ConfirmNameEditRequest request, string user, string? role);
        public Task<GenericResponse<bool>> ConfirmNameShipAsync(ConfirmNameEditRequest request, string user);
        // Them thong tin 
        public Task<GenericResponse<bool>> AddConfirmNameAsync(BaoGia_Confirm_Name_QuotationDTO confirmName);
        // Approve ConfirmName
        public Task<GenericResponse<bool>> ApproveConfirmNameAsync(int id, string approvedBy);
        // Reject ConfirmName
        public Task<GenericResponse<bool>> RejectConfirmNameAsync(int id, string reason, string rejectedBy);
        // Insert thong tin danh sach
        public Task<GenericResponse<List<BaoGia_Confirm_Name_QuotationDTO>>> AddListAsync(List<BaoGia_Confirm_Name_QuotationDTO> confirmNames);
        // luu thong tin nhap file
        public Task<GenericResponse<bool>> SaveFromFileAsync(List<ConfirmNameInputExcel> confirmNames, string user, string? Role);
        // luu thong tin 
        public Task<GenericResponse<bool>> SaveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Approvers
        public Task<GenericResponse<bool>> ApproveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Rejects Ship
        public Task<GenericResponse<bool>> RejectShipConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Export Code Cofirmed
        public Task<GenericResponse<List<dynamic>>> ExportCodeConfirmedAsync();
        // Từ chối xác nhận tên hàng
        public Task<GenericResponse<bool>> RejectConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role);
        // Lấy thông tin gửi mail 
        public Task<GenericResponse<List<ResultCheckCofirmName>>> SearchSendMailConfirmNameAsync(List<int> listCheck);
        // Cập nhật thông tin đơn báo giá sau khi trả lại
        public Task<GenericResponse<bool>> UpdateRequestFromFileAsync(List<BaoGia_Request_of_QuotationDTO> baoGia, string user);
        // Cập nhật thông tin yêu cầu PIC PUR cần xác nhận lại báo giá
        public Task<GenericResponse<bool>> UpdateRequestForPICPURAsync(List<ConfirmNameInputExcel> baoGia, string user);
        //Export file ten hanh xac nhan
        Task<GenericResponse<List<dynamic>>> ExportConfirmedMaterialNamesAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user);
        // Update Name HQ role PIC PUR
        Task<GenericResponse<bool>> UpdateNameHQRolePICPURAsync(List<ConfirmNameInputExcel> baoGia, string user);
        // Done
        Task<GenericResponse<List<ResultCheckCofirmName>>> DoneConfirmNameAsync(List<int> listDone);
        // Check đơn đã hoàn thành hay chưa
        Task<GenericResponse<List<int>>> CheckConfirmNameDoneAsync(List<int> listCheck);

        // Lấy lịch sử thay đổi xác nhận tên
        Task<GenericResponse<List<ConfirmNameHistoryDTO>>> GetConfirmNameHistoryAsync(int confirmId);
        // Count ConfirName
        Task<GenericResponse<CountCofirmName>> GetCountCofirmNames(ConfirmNameSearchRequest req, string user, string? role);

    }
}
