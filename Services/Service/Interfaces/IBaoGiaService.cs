using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaService : IBaseService<BaoGia_Request_of_Quotation, int, BaoGia_Request_of_QuotationDTO>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> GetByMaBaoGiaAsync(string maBaoGia);
        // Tìm kiếm thông tin báo giá và phân trang 
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);
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
        // Xuất báo giá
        public Task<GenericResponse<List<int>>> ExportBaoGiaAsync(string? maDon);
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public Task<GenericResponse<ListRequest<dynamic>>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, string? user, int pageIndex, int pageSize);
        // Lấy thông tin kèm chi tiết báo giá
        public Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, int pageIndex, int pageSize);
        // lấy mã đơn theo Adid
        public Task<GenericResponse<List<string>>> GetMaDonByAdidAsync(string adid);
    }
}
