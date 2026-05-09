using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaService : IBaseService<BaoGia_Request_of_Quotation, int, BaoGia_Request_of_QuotationDTO>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> GetByMaBaoGiaAsync(string maBaoGia);

        // Tìm kiếm thông tin báo giá và phân trang
        public Task<GenericResponse<ListRequest<BaoGia_Request_of_QuotationDTO>>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);

        // Nhập báo giá
        public Task<GenericResponse<bool>> NhapBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia);

        // Nhập danh sách báo giá - trả về danh sách DTO có ID sau khi insert
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> danhSachBaoGia);

        // Cập nhật thông tin báo giá
        public Task<GenericResponse<bool>> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_QuotationDTO baoGia);

        // Lấy danh sách mã đơn báo giá
        public Task<GenericResponse<List<string>>> GetListMaDonBGAsync();

        // Cập nhập danh sách mã đơn báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> CapNhatDanhSachBGAsync(List<BaoGia_Request_of_QuotationDTO> danhSachMaDonBG);

        // Cập nhật đơn báo giá
        public Task<GenericResponse<BaoGia_Request_of_QuotationDTO>> CapNhatDonBaoGiaAsync(BaoGia_Request_of_QuotationDTO baogia);

        // Lấy thông tin báo giá gom nhóm
        public Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string? status, string user, int pageIndex, int pageSize);

        // Xuất báo giá
        public Task<GenericResponse<List<int>>> ExportBaoGiaAsync(string? maDon);

        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public Task<GenericResponse<ListRequest<dynamic>>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, string? user, int pageIndex, int pageSize);

        // Lấy thông tin kèm chi tiết báo giá
        public Task<GenericResponse<ListRequest<dynamic>>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize);

        // Lấy mã đơn theo Adid
        public Task<GenericResponse<List<string>>> GetMaDonByAdidAsync(string adid, int step);

        // Update thông tin màn hình lịch sử báo giá
        public Task<GenericResponse<UpdateHistoryResult>> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> baoGias);

        // Get thông tin đơn phê duyệt lựa chọn ncc
        public Task<GenericResponse<List<dynamic>>> GetSupplierApprovalInfoAsync(string maDon);

        // Xuất file phê duyệt báo giá
        public Task<GenericResponse<List<dynamic>>> GetExportApprovalInfoAsync(List<string> listMaDon);

        // Phê duyệt thông tin lựa chọn nhà cung cấp
        public Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprovarOK(string maDon, string userNext, string userUpdate);

        // Phê duyệt thông tin lựa chọn nhà cung cấp (từ chối)
        public Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprovarNG(string maDon, string Reason, string userUpdate);

        public Task<GenericResponse<ListRequest<dynamic>>> SearchRequestDone(string? maDon, string? section, string? maHang, string? maNCC, string user, int pageIndex, int pageSize);

        // Update người phê duyệt cho đơn
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> UpdateUserApprovalHistory(UpdateHistoryResult update);
        // update ma hang noi bo
        public Task<GenericResponse<bool>> UpdateCodeMaterialBIVN(List<ConfirmNameDTO> list);
        // Phê duyệt list lựa chọn nhà cung cấp
        public Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> UpdateApprover(List<ApproverDTO> dataApprovers, string userNext, string userUpdate);
        // Xóa đơn xin báo giá
        public Task<GenericResponse<bool>> DeleteDonXinBaoGiaAsync(string maDon, string reason, string userUpdate);
        // Xóa từng đơn
        public Task<GenericResponse<bool>> DeleteDonBaoGiaAsync(int id, string reason, string userUpdate);
        public Task<GenericResponse<List<BaoGia_Request_of_Quotation>>> TraLaiDonBaoGiaAsync(string maDon, string userUpdate);
        // lấy danh sách đơn yêu cầu hàng hóa
        public Task<GenericResponse<List<string>>> GetMaDonYeuCauHangHoaAsync();
        // update phê duyệt đơn báo giá
        public Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> UpdatePheDuyetDonBaoGiaAsync(List<BaoGia_Request_of_QuotationDTO> baoGias);
    }
}
