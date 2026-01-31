using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaDetailService: IBaseService<BaoGia_Detail_of_Quotation, int, BaoGia_Detail_of_QuotationDTO>
    {
        // Tìm kiếm thông tin liên quan đến báo giá
        public Task<GenericResponse<List<dynamic>>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, DateTime? dayMM, int? PageSize, int? PageIndex);
        // Insert danh sách báo giá
        public Task<GenericResponse<bool>> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_QuotationDTO> listDto);
    }
}
