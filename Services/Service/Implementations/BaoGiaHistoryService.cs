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
    
    }
}
