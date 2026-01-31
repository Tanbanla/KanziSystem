using PRJ_WAREHOUSE_BIVN.Models_Auto;

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
    }
}
