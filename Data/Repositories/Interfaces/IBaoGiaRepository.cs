using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaRepository: IBaseRepository<BaoGia_Request_of_Quotation , int>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<BaoGia_Request_of_Quotation> GetByMaBaoGiaAsync(string maBaoGia);
        // Tìm kiếm thông tin báo giá và phân trang 
        public Task<List<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, int pageIndex, int pageSize, DateTime? date);
        // Nhap bao gia
        public Task<bool> NhapBaoGiaAsync(BaoGia_Request_of_Quotation baoGia);
        // Nhap danh sach bao gia - trả về danh sách entity đã được gán ID
        public Task<List<BaoGia_Request_of_Quotation>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_Quotation> danhSachBaoGia);
        // Cap nhat thông tin báo giá
        public Task<bool> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_Quotation baoGia);
        // Lấy danh sách mã đơn báo giá 
        public Task<List<string>> GetListMaDonBGAsync();
        // Cập nhâp danh sách mã đơn báo giá
        public Task<List<BaoGia_Request_of_Quotation>> CapNhatDanhSachBGAsync(List<BaoGia_Request_of_Quotation> danhSachMaDonBG); 
        // Cập nhật đơn báo giá
        public Task<BaoGia_Request_of_Quotation> CapNhatDonBaoGiaAsync(BaoGia_Request_of_Quotation baogia);
        // Lấy thông tin báo giá gom nhóm
        public Task<List<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize);
    }
}
