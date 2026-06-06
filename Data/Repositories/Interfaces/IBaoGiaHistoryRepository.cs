using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaHistoryRepository: IBaseRepository<BaoGia_History_Request_of_Quotation,int>
    {
        // Lấy lịch sử báo giá theo ID_RequestQuote
        public Task<List<BaoGia_History_Request_of_Quotation>> GetByRequestQuoteIdAsync(int idRequestQuote);
        // Tìm kiếm danh sách thông tin lịch sử báo giá theo số đơn
        public Task<List<BaoGia_History_Request_of_Quotation>> SearchBySoDonAsync(string soDon);
        // Tìm kiếm lịch sử báo giá và phân trang
        public Task<List<BaoGia_History_Request_of_Quotation>> SearchAsync(int? idRequestQuote, string? soDon, int? pageIndex, int? pageSize);
        // Insert thông tin lịch sử báo giá
        public Task<bool> InsertHistoryAsync(BaoGia_History_Request_of_Quotation history);
        // Insert danh sách lịch sử báo giá
        public Task<bool> InsertHistoryListAsync(List<BaoGia_History_Request_of_Quotation> historyList);
        // Sửa thông tin lịch sử báo giá
        public Task<bool> UpdateHistoryAsync(BaoGia_History_Request_of_Quotation history);
        // Lấy lý do trả lại đơn báo giá
        public Task<string> GetReturnReasonAsync(int idRequestQuote);

        public Task<List<ReasonQuotition>> GetReasonsAsync(List<dynamic> ids);
        // Sreach lịch sử bảo giá
        Task<ListRequest<dynamic>> SearchHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);
        // Lấy thông tin phê duyệt báo giá của các đơn hàng
        Task<List<dynamic>> GetHistoryApprover(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai);
        // Lấy thông tin lịch sử báo giá theo mã hàng nội bộ và số đơn
        Task<List<dynamic>> GetHistoryByMaterialCode(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai);
        // tính tổng số đơn đến hạn
        Task<List<dynamic>> GetCountQuotation(string user);

    }
}
