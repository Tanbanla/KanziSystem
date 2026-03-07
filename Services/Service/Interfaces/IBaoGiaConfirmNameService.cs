using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaConfirmNameService: IBaseService<BaoGia_Confirm_Name_Quotation, int , BaoGia_Confirm_Name_QuotationDTO>
    {
        // search thông tin xác nhận tên hàng
        public Task<GenericResponse<ListRequest<dynamic>>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section,int pageIndex, int pageSize);
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
    }
}
