using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaDetailRepository: IBaseRepository<BaoGia_Detail_of_Quotation , int>
    {
        // Tìm kiếm thông tin liên quan đến báo giá
        public Task<List<dynamic>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, DateTime? dayMM, int? PageSize, int? PageIndex);
        // Insert danh sách báo giá
        public Task<bool> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_Quotation> listDto);
        // Update lua chon NCC
        public Task<bool> UpdateLuaChonNCCBaoGiaDetailAsync(List<dynamic> listUp, string user, string name);
        // Lấy thông tin theo ID_RequestQuote
        public Task<BaoGia_Detail_of_Quotation> GetByIdRequestQuoteAsync(int idRequest);
    }
}
