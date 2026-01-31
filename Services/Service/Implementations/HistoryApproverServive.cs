using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class HistoryApproverServive: BaseService<BaoGia_History_Approver_of_Quotation, int, BaoGia_History_Approver_of_QuotationDTO>, IHistoryApproverServive
    {
        private readonly IHistoryApproverRepository _repo;
        private readonly IMapper _mapper;
        public HistoryApproverServive(IHistoryApproverRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // Lấy thông tin lịch sử phê duyệt báo giá theo mã báo giá
        public async Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> GetHistoryByQuotationIdAsync(int quotationId)
        {
            var result = new GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>();
            try
            {
                var history = await _repo.GetHistoryByQuotationIdAsync(quotationId);
                result.Data = _mapper.Map<List<BaoGia_History_Approver_of_QuotationDTO>>(history);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Lấy thông tin lịch sử phê duyệt báo giá theo số đơn
        public async Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> GetHistoryBySoDonAsync(string soDon)
        {
            var result = new GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>();
            try
            {
                var history = await _repo.GetHistoryBySoDonAsync(soDon);
                result.Data = _mapper.Map<List<BaoGia_History_Approver_of_QuotationDTO>>(history);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;

        }
        // Tìm kiếm lịch sử phê duyệt báo giá
        public async Task<GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>> SearchHistoryAsync(int? quotationId, string? soDon, int? buoc, DateTime? fromDate, DateTime? toDate, string? approverName)
        {
            var result = new GenericResponse<List<BaoGia_History_Approver_of_QuotationDTO>>();
            try
            {
                var history = await _repo.SearchHistoryAsync(quotationId, soDon, buoc, fromDate, toDate, approverName);
                result.Data = _mapper.Map<List<BaoGia_History_Approver_of_QuotationDTO>>(history);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Thêm mới lịch sử phê duyệt báo giá
        public async Task<GenericResponse<bool>> AddHistoryAsync(BaoGia_History_Approver_of_QuotationDTO history)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<BaoGia_History_Approver_of_Quotation>(history);
                result.Data = await _repo.AddHistoryAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Thêm mới danh sách lịch sử phê duyệt báo giá 
        public async Task<GenericResponse<bool>> AddHistoryListAsync(List<BaoGia_History_Approver_of_QuotationDTO> historyList)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<List<BaoGia_History_Approver_of_Quotation>>(historyList);
                result.Data = await _repo.AddHistoryListAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Sửa thông tin lịch sử phê duyệt báo giá
        public async Task<GenericResponse<bool>> UpdateHistoryAsync(BaoGia_History_Approver_of_QuotationDTO history)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<BaoGia_History_Approver_of_Quotation>(history);
                result.Data = await _repo.UpdateHistoryAsync(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Lấy danh sách phê duyệt của người dùng 
        public async Task<GenericResponse<List<BaoGia_Request_of_QuotationDTO>>> GetListApprover(string adid, string? soDon, string? maHang, string? section, string? statusApprover)
        {
            var result = new GenericResponse<List<BaoGia_Request_of_QuotationDTO>>();
            try
            {
                var list = await _repo.GetListApprover(adid, soDon, maHang, section, statusApprover);
                result.Data = _mapper.Map<List<BaoGia_Request_of_QuotationDTO>>(list);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
