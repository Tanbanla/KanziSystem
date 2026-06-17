using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaHistoryService : BaseService<BaoGia_History_Request_of_Quotation, int, BaoGia_History_Request_of_QuotationDTO>, IBaoGiaHistoryService
    {
        private readonly IBaoGiaHistoryRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaHistoryService(IBaoGiaHistoryRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }
        // Lấy lịch sử báo giá theo ID_RequestQuote
        public async Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> GetByRequestQuoteIdAsync(int idRequestQuote)
        {
            var result = new GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>();
            try
            {
                var histories = await _repo.GetByRequestQuoteIdAsync(idRequestQuote);
                result.Data = _mapper.Map<List<BaoGia_History_Request_of_QuotationDTO>>(histories);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Tìm kiếm danh sách thông tin lịch sử báo giá theo số đơn
        public async Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> SearchBySoDonAsync(string soDon)
        {
            var result = new GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>();
            try
            {
                var histories = await _repo.SearchBySoDonAsync(soDon);
                result.Data = _mapper.Map<List<BaoGia_History_Request_of_QuotationDTO>>(histories);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Thêm lịch sử báo giá mới
        public async Task<GenericResponse<bool>> InsertHistoryAsync(BaoGia_History_Request_of_QuotationDTO history)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entity = _mapper.Map<BaoGia_History_Request_of_Quotation>(history);
                result.Data = await _repo.InsertHistoryAsync(entity);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Thêm danh sách lịch sử báo giá mới
        public async Task<GenericResponse<bool>> InsertHistoryListAsync(List<BaoGia_History_Request_of_QuotationDTO> historyList)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entities = _mapper.Map<List<BaoGia_History_Request_of_Quotation>>(historyList);
                result.Data = await _repo.InsertHistoryListAsync(entities);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Sửa thông tin lịch sử báo giá
        public async Task<GenericResponse<bool>> UpdateHistoryAsync(BaoGia_History_Request_of_QuotationDTO history)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entity = _mapper.Map<BaoGia_History_Request_of_Quotation>(history);
                result.Data = await _repo.UpdateHistoryAsync(entity);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Search lịch sử báo giá và phân trang
        public async Task<GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>> SearchAsync(int? idRequestQuote, string? soDon, int? pageIndex, int? pageSize)
        {
            var result = new GenericResponse<List<BaoGia_History_Request_of_QuotationDTO>>();
            try
            {
                var histories = await _repo.SearchAsync(idRequestQuote, soDon, pageIndex, pageSize);
                result.Data = _mapper.Map<List<BaoGia_History_Request_of_QuotationDTO>>(histories);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy lý do trả lại đơn báo giá
        public async Task<GenericResponse<string>>GetReturnReasonAsync(int idRequestQuote)
        {
            var result = new GenericResponse<string>();
            try
            {
                var reason = await _repo.GetReturnReasonAsync(idRequestQuote);
                result.Data = reason;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        public async Task<GenericResponse<List<ReasonQuotition>>> GetReasonsAsync(List<dynamic> ids)
        {
            var result = new GenericResponse<List<ReasonQuotition>>();
            try
            {
                var reasons = await _repo.GetReasonsAsync(ids);
                result.Data = reasons;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Sreach lịch sử bảo giá
        public async Task<GenericResponse<ListRequest<dynamic>>> SearchHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.SearchHistoryAsync(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, user, pageIndex, pageSize, date, chungLoai);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy thông tin phê duyệt báo giá của các đơn hàng
        public async Task<GenericResponse<List<dynamic>>> GetHistoryApprover(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetHistoryApprover(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, user, chungLoai);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy thông tin lịch sử báo giá theo mã hàng nội bộ và số đơn
        public async Task<GenericResponse<List<dynamic>>> GetHistoryByMaterialCode(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetHistoryByMaterialCode(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, step, user, chungLoai);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // tính tổng số đơn đến hạn
        public async Task<GenericResponse<List<dynamic>>> GetCountQuotation(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? user)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetCountQuotation(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, user);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy thông tin lịch sử báo giá
        public async Task<GenericResponse<ListRequest<dynamic>>> GetHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, string? user, int pageIndex, int pageSize, DateTime? dateTo, DateTime? dateFrom, string? chungLoai)
        {
            var result = new GenericResponse<ListRequest<dynamic>>();
            try
            {
                result.Data = await _repo.GetHistoryAsync(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, status, user, pageIndex, pageSize, dateTo, dateFrom, chungLoai);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Tính tổng theo trạng thái đơn
        public async Task<GenericResponse<List<dynamic>>> GetCountStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? user)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetCountStatus(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, user);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Tính tình trạng xử lý đơn hàng
        public async Task<GenericResponse<List<dynamic>>> GetProcessingStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetProcessingStatus(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, user);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Tính các đơn hàng đang chờ chọn nhà cung cấp
        public async Task<GenericResponse<List<dynamic>>> GetWaitingForSupplier(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                result.Data = await _repo.GetWaitingForSupplier(MaDon, MaNcc, Section, nguoiYeuCau, MaHang, user);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Lấy lịch sử của đơn hành
        public async Task<GenericResponse<List<BaoGia_History_Request_of_Quotation>>> GetOrderHistoryAsync(string? maDon, string? maHang, string? maHangNCC)
        {
            var result = new GenericResponse<List<BaoGia_History_Request_of_Quotation>>();
            try
            {
                var histories = await _repo.GetOrderHistoryAsync(maDon, maHang, maHangNCC);
                result.Data = histories;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
    }
}
