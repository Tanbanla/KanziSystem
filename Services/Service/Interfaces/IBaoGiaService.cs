using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaService: IBaseService<BaoGia_Request_of_Quotation, int, BaoGia_Request_of_QuotationDTO>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<GenericResponse<BaoGia_Request_of_QuotationDTO>> GetByMaBaoGiaAsync(string maBaoGia);
        // Tìm kiếm thông tin báo giá và phân trang 
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang,string? status, int? step, int pageIndex, int pageSize, DateTime? date);
        // Nhap bao gia
        public Task<GenericResponse<bool>> NhapBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia);
        // Nhap danh sach bao gia - trả về danh sách DTO có ID sau khi insert
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> danhSachBaoGia);
        // Cap nhat thông tin báo giá
        public Task<GenericResponse<bool>> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia);
        // Lấy danh sách mã đơn báo giá 
        public Task<GenericResponse<List<string>>> GetListMaDonBGAsync();
        // Cập nhâp danh sách mã đơn báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> CapNhatDanhSachBGAsync(List<BaoGia_Request_of_QuotationDTO> danhSachMaDonBG);
        // Cập nhật đơn báo giá
        public Task<GenericResponse<BaoGia_Request_of_QuotationDTO>> CapNhatDonBaoGiaAsync(BaoGia_Request_of_QuotationDTO baogia);
        // Lấy thông tin báo giá gom nhóm
        public Task<GenericResponse<List<dynamic>>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize);
    }
}
