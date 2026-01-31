using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaDetailService: BaseService<BaoGia_Detail_of_Quotation, int, BaoGia_Detail_of_QuotationDTO>, IBaoGiaDetailService
    {
        private readonly IBaoGiaDetailRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaDetailService(IBaoGiaDetailRepository repo, IMapper mapper): base (repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // Tìm kiếm thông tin liên quan đến báo giá
        public async Task<GenericResponse<List<dynamic>>> SearchBaoGiaAsync(int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, DateTime? dayMM, int? PageSize, int? PageIndex)
        {
            var result = new GenericResponse<List<dynamic>>();
            try
            {
                var data = await _repo.SearchBaoGiaAsync(idRequest, maDon, maVatTu, maNcc, section,dayMM, PageSize, PageIndex);
                result.Data = data;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Insert danh sách báo giá
        public async Task<GenericResponse<bool>> InsertListBaoGiaDetailAsync(List<BaoGia_Detail_of_QuotationDTO> listDto)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var listModel = _mapper.Map<List<BaoGia_Detail_of_Quotation>>(listDto);
                var isSuccess = await _repo.InsertListBaoGiaDetailAsync(listModel);
                result.Data = isSuccess;
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
