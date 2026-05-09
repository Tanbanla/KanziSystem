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
        public Task<GenericResponse<List<ReasonQuotition>>> GetReasonsAsync(List<int> ids);
    }
}
