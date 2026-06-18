using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaHistoryService: IBaseService<BaoGia_History_Request_of_Quotation, int, BaoGia_History_Request_of_QuotationDTO>
    {
        // Lấy lịch sử báo giá theo ID_RequestQuote
        public Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> GetByRequestQuoteIdAsync(int idRequestQuote);
        // Tìm kiếm danh sách thông tin lịch sử báo giá theo số đơn
        public Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> SearchBySoDonAsync(string soDon);
        // Tìm kiếm lịch sử báo giá và phân trang
        public Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> SearchAsync(int? idRequestQuote, string? soDon, int? pageIndex, int? pageSize);
        // Insert thông tin lịch sử báo giá
        public Task<GenericResponse<bool>> InsertHistoryAsync(BaoGia_History_Request_of_QuotationDTO history);
        // Insert danh sách lịch sử báo giá
        public Task<GenericResponse<bool>> InsertHistoryListAsync(List<BaoGia_History_Request_of_QuotationDTO> historyList);
        // Sửa thông tin lịch sử báo giá
        public Task<GenericResponse<bool>> UpdateHistoryAsync(BaoGia_History_Request_of_QuotationDTO history);
        // Lấy lý do trả lại đơn báo giá
        public Task<GenericResponse<string>> GetReturnReasonAsync(int idRequestQuote);
        public Task<GenericResponse<List<ReasonQuotition>>> GetReasonsAsync(List<dynamic> ids);
        // Sreach lịch sử bảo giá
        Task<GenericResponse<ListRequest<dynamic>>> SearchHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);
        // Lấy thông tin phê duyệt báo giá của các đơn hàng
        Task<GenericResponse<List<dynamic>>> GetHistoryApprover(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai);
        // Lấy thông tin lịch sử báo giá theo mã hàng nội bộ và số đơn
        Task<GenericResponse<List<dynamic>>> GetHistoryByMaterialCode(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai);
        // tính tổng số đơn đến hạn
        Task<GenericResponse<List<dynamic>>> GetCountQuotation(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? user);
        // Lấy thông tin lịch sử báo giá
        Task<GenericResponse<ListRequest<dynamic>>> GetHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, string? user, int pageIndex, int pageSize, DateTime? dateTo, DateTime? dateFrom, string? chungLoai);
        // Tính tổng theo trạng thái đơn
        Task<GenericResponse<List<dynamic>>> GetCountStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? user);
        // Tính tình trạng xử lý đơn hàng
        Task<GenericResponse<List<dynamic>>> GetProcessingStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user);
        // Tính các đơn hàng đang chờ chọn nhà cung cấp
        Task<GenericResponse<List<dynamic>>> GetWaitingForSupplier(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user);
        // Lấy lịch sử của đơn hành
        Task<GenericResponse<List<BaoGia_History_Request_of_Quotation>>> GetOrderHistoryAsync(string? maDon, string? maHang, string? maHangNCC);

    }
}
