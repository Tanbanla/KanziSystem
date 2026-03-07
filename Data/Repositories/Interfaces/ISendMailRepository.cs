using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Drawing.Printing;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface ISendMailRepository : IBaseRepository<TM_MASTER_MAIL, int>
    {
        // lấy mail theo ID
        public Task<TM_MASTER_MAIL?> GetMailByIdAsync(int id);
        // Lay danh sach NCC can gui mail
        public Task<List<string>> GetSuppliersToNotifyAsync();
        // Lay thong tin don bao gia cua nha cung cap
        public Task<List<BaoGia_Request_of_Quotation>> GetBaoGiaRequestBySupplierAsync(string supplierCode);
        // Lay email nha cung cap
        public Task<string?> GetSupplierEmailAsync(string supplierCode);
        // Cập nhật trạng thái đã gửi mail cho đơn hàng
        public Task<bool> UpdateMailSentStatusAsync(List<int> listRq);

        // Lay thong tin nha cung cap theo ma don hang
        public Task<List<BaoGia_Request_of_Quotation>> GetNotifyRequestCodeAsync(string requestCode);
    }
}
