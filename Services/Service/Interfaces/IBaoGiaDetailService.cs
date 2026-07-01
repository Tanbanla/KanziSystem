using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaDetailService : IBaseService<BaoGia_Detail_of_Quotation, int, BaoGia_Detail_of_QuotationDTO>
    {
        // Tìm kiếm thông tin liên quan đến báo giá
        public Task<GenericResponse<ListRequest<dynamic>>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, string? user, DateTime? dayMM, int? PageSize, int? PageIndex);
        // Insert danh sách báo giá
        public Task<GenericResponse<bool>> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_QuotationDTO> listDto);
        // Update lua chon NCC
        public Task<GenericResponse<bool>> UpdateLuaChonNCCBaoGiaDetailAsync(List<dynamic> listUp, string user, string name);
        // Lấy thông tin theo ID_RequestQuote
        public Task<GenericResponse<BaoGia_Detail_of_QuotationDTO>> GetByIdRequestQuoteAsync(int idRequest);
        // Update list thông tin ghi nhập báo giá
        public Task<GenericResponse<bool>> UpdateListThongTinNhapBaoGiaAsync(List<BaoGia_Detail_of_QuotationDTO> listDto);
        // lấy id của đơn báo giá
        public Task<GenericResponse<int?>> GetIdOfQuotationAsync(string maDon, string maVatTu, string maNB, string maNcc, string NameHQ);
        // update thông tin lựa chọn nhà  cung cấp
        public Task<GenericResponse<BaoGia_Request_of_Quotation>> UpdatePickSupplierDetailAsync(List<BaoGia_Detail_of_QuotationDTO> dtos, string userApproverNext);
        // Lấy id detail theo ID RequestQuote
        public Task<GenericResponse<int>> GetIdDetailAsync(int? idRequest);
        // Cập nhật thông tin status của đơn báo giá
        Task<GenericResponse<bool>> UpdateStatusAsync(List<int> ids);
        // Cập nhật thông tin link báo giá trên hệ thống
        Task<GenericResponse<bool>> UpdateLinkBaoGiaAsync();
    }
}
