using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaRepository : IBaseRepository<BaoGia_Request_of_Quotation, int>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<List<BaoGia_Request_of_Quotation>> GetByMaBaoGiaAsync(string maBaoGia);
        // Tìm kiếm thông tin báo giá và phân trang 
        public Task<List<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);
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
        public Task<ListRequest<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string user, int pageIndex, int pageSize);
        // Xuất báo giá
        public Task<List<int>> ExportBaoGiaAsync(string? maDon);
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public Task<ListRequest<dynamic>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, string? user, int pageIndex, int pageSize);

        // Lấy thông tin kèm chi tiết báo giá
        public Task<ListRequest<dynamic>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize);
    
        // lấy mã đơn theo Adid
        public Task<List<string>> GetMaDonByAdidAsync(string adid);
        // update thông tin màn hình lịch sử báo giá
        public Task<bool> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias);
        // Get thông tin đơn phê duyệt lựa chọn ncc
        public Task<List<dynamic>> GetSupplierApprovalInfoAsync(string maDon);
    }
}
