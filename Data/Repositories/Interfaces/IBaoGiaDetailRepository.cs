using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.QuoteResult;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaDetailRepository : IBaseRepository<BaoGia_Detail_of_Quotation, int>
    {
        // Tìm kiếm thông tin liên quan đến báo giá
        public Task<ListRequest<dynamic>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, string? user, DateTime? dayMM, string? status, string role, int? PageSize, int? PageIndex);
        // Insert danh sách báo giá
        public Task<bool> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> listDto);
        // Update lua chon NCC
        public Task<bool> UpdateLuaChonNCCBaoGiaDetailAsync(List<dynamic> listUp, string user, string name);
        // Lấy thông tin theo ID_RequestQuote
        public Task<BaoGia_Detail_of_Quotation> GetByIdRequestQuoteAsync(int idRequest);
        // Lấy lịch sử thay đổi chi tiết báo giá theo mã đơn
        public Task<List<BaoGia_History_Detail_Request>> GetInputQuoteHistoryAsync(int idDetail);
        // Update infor input bao gia
        public Task<bool> UpdateListThongTinNhapBaoGiaAsync(List<BaoGia_Detail_of_Quotation> listDto);

        // update infor
        Task<bool> UpdateQuotationNotRQAsync(List<BaoGia_Detail_of_Quotation> listDto);
        // lấy id của đơn báo giá
        public Task<int?> GetIdOfQuotationAsync(string maDon, string maVatTu, string maNB, string maNcc, string NameHQ);
        // Lấy giá gần nhất của các mã hàng đã từng được chọn nhà cung cấp
        public Task<List<LatestSelectedQuotationPriceDTO>> GetLatestSelectedPricesAsync(List<string> maHangNoiBo);
        // update thông tin lựa chọn nhà  cung cấp
        public Task<BaoGia_Request_of_Quotation> UpdatePickSupplierDetailAsync(List<BaoGia_Detail_of_Quotation> dtos, string userApproverNext, string userUpdate);
        // Lấy id detail theo ID RequestQuote
        public Task<int> GetIdDetailAsync(int? idRequest);
        // Cập nhật thông tin status của đơn báo giá
        Task<bool> UpdateStatusAsync(List<int> ids);
        // Lấy thông tin các file cần chuyển
        Task<List<UpdateFile>> GetFilesToTransferAsync();
        // update thông tin link báo giá trên hệ thống
        Task<bool> UpdateLinkBaoGiaAsync(List<UpdateFile> listDto);
        // Lấy file thông tin đã nhập lên hệ thống
        Task<List<dynamic>> GetFilesToImportAsync(string keywork);
        // Lấy thông tin  cho màn hình master báo giá
        Task<List<dynamic>> SearchMasterQuoteInfoAsync(SearchQuoteResultViewModel vm);
        // count số lượng cho màn hình master báo giá
        Task<int> CountMasterQuoteInfoAsync(SearchQuoteResultViewModel vm);
    }
}
