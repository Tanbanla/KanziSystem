using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaConfirmNameRepository: IBaseRepository<BaoGia_Confirm_Name_Quotation, int>
    {
        // search thông tin xác nhận tên hàng
        public Task<List<BaoGia_Confirm_Name_Quotation>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, int pageIndex, int pageSize);
        // Luu thong tin
        public Task<bool> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo,string? Role, string User);
        // Them thong tin 
        public Task<bool> AddConfirmNameAsync(BaoGia_Confirm_Name_Quotation confirmName);
        // Approve ConfirmName
        public Task<bool> ApproveConfirmNameAsync(int id, string approvedBy);
        // Reject ConfirmName
        public Task<bool> RejectConfirmNameAsync(int id, string reason, string rejectedBy);
    }
}
