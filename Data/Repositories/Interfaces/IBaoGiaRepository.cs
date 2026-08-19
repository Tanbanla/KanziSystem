using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaRepository : IBaseRepository<BaoGia_Request_of_Quotation, int>
    {
        // Lấy thông tin báo giá theo mã báo giá
        public Task<List<BaoGia_Request_of_Quotation>> GetByMaBaoGiaAsync(string maBaoGia);
        // Tìm kiếm thông tin báo giá và phân trang 
        public Task<ListRequest<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai);
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
        public Task<ListRequest<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string? status, string user, int pageIndex, int pageSize);
        // Xuất báo giá
        public Task<List<int>> ExportBaoGiaAsync(string? maDon);
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public Task<ListRequest<dynamic>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, string? user, string? status, int pageIndex, int pageSize);

        // Lấy thông tin kèm chi tiết báo giá
        public Task<ListRequest<dynamic>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize);

        // lấy mã đơn theo Adid
        public Task<List<string>> GetMaDonByAdidAsync(string adid, int step);
        // update thông tin màn hình lịch sử báo giá
        public Task<UpdateHistoryResult> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias);
        // Get thông tin đơn phê duyệt lựa chọn ncc
        public Task<List<dynamic>> GetSupplierApprovalInfoAsync(string maDon, string user);
        // Xuất file phê duyệt báo giá 
        public Task<List<dynamic>> GetExportApprovalInfoAsync(List<string> listMaDon, string adid);
        // Phê duyệt thông tin lựa chọn nhà cung cấp
        public Task<List<BaoGia_Request_of_Quotation>> UpdateApprovarOK(string maDon, string userNext, string userUpdate);
        public Task<List<BaoGia_Request_of_Quotation>> UpdateApprovarNG(string maDon, string Reason, string userUpdate);
        public Task<ListRequest<dynamic>> SearchRequestDone(string? maDon, string? section, string? maHang, string? maNCC, string user, int pageIndex, int pageSize);
        // update người phê duyệt cho đơn
        public Task<List<BaoGia_Request_of_Quotation>> UpdateUserApprovalHistory(UpdateHistoryResult update);
        // update ma hang noi bo
        public Task<bool> UpdateCodeMaterialBIVN(List<ConfirmNameDTO> list);
        // Phê duyệt list lựa chọn nhà cung cấp
        public Task<List<BaoGia_Request_of_Quotation>> UpdateApprover(List<ApproverDTO> dataApprovers, string userNext, string userUpdate);
        // Xóa đơn xin báo giá
        public Task<bool> DeleteDonXinBaoGiaAsync(string maDon, string reason, string userUpdate, string role);
        // Xóa từng đơn
        public Task<bool> DeleteDonBaoGiaAsync(int id, string reason, string userUpdate);
        // Trả lại đơn báo giá
        public Task<List<BaoGia_Request_of_Quotation>> TraLaiDonBaoGiaAsync(string maDon, string userUpdate,string reason);
        // lấy danh sách đơn yêu cầu hàng hóa
        public Task<List<string>> GetMaDonYeuCauHangHoaAsync();
        // update phê duyệt đơn báo giá
        public Task<List<BaoGia_Request_of_Quotation>> UpdatePheDuyetDonBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias);
        // Check đơn return
        Task<bool> CheckDonReturnAsync(List<string> maDons);
        // Export history báo giá
        Task<List<dynamic>> ExportHistoryBaoGiaAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang,
            string? status, int? step, string? user, string? chungLoai, DateTime? to, DateTime? from);
        // update thời hạn lựa chọn nhà cung cấp
        Task<List<BaoGia_Request_of_Quotation>> UpdateDeadlineAsync(List<BaoGia_Request_of_Quotation> baoGias);
        // Lấy danh sách NCC k cần xác nhận tên hàng
        Task<List<string>> GetListNccNotConfirmNameAsync();
        // kiểm tra đơn + mã hàng đã được quyền lựa chọn nhà cung cấp hay chưa
        Task<List<BaoGiaImportModel>> CheckPermissionSelectSupplierAsync(List<BaoGiaImportModel> baoGiaImportModels);
        // check trạng thái đơn báo giá
        Task<List<int>> CheckStepAsync(List<int> ids, List<int> stepCheck);
        // check infor báo giá theo mã đơn, mã hàng, mã ncc
        Task<List<BaoGia_Request_of_Quotation>> GetOrderInfoAsync(string? maDon, string? maHangNCC, string? maHangNB, string? NameEn);
        // get detail bao gia Done
        Task<List<dynamic>> SearchRequestDoneDetail(string? maDon, string? maHang, string? user);

        // Export Excel báo giá Done
        Task<List<dynamic>> ExportExcelBaoGiaDoneAsync(string? maDon, string? section, string? maHang, string? maNCC, string user);

        // Search báo giá còn hiệu lực
        Task<ListRequest<dynamic>> SearchBaoGiaConHieuLucAsync(SearchQuotationResultsModel search, string? user);
    }
}
