using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmNccNewService:  BaseService<IM_NCC_NEW, int, IM_NCC_NEWDTO>, ITmNccNewService
    {
        private readonly ITmNccNewRepository _repo;
        public TmNccNewService(ITmNccNewRepository repository, IMapper mapper): base(repository, mapper)
        {
            _repo = repository;
        }
        //Lấy danh sách nhà cung cấp  ALL
        public async Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetAllNccNew()
        {
            var result = new GenericResponse<List<IM_NCC_NEWDTO>>();
            try
            {
               var nccNews = await _repo.GetAllNccNew();
                result.Data = _mapper.Map<List<IM_NCC_NEWDTO>>(nccNews);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = null;
            }
            return result;
        }
        // Lấy nhà cung cấp theo mã nhà cung cấp
        public async Task<GenericResponse<IM_NCC_NEWDTO>> GetNccNewByCode(string MaNCC)
        {
            var result = new GenericResponse<IM_NCC_NEWDTO>();
            try
            {
                var nccNew = await _repo.GetNccNewByCode(MaNCC);
                result.Data = _mapper.Map<IM_NCC_NEWDTO>(nccNew);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = null;
            }
            
            return result;
        }
        // Lây thông tin nhà cung cấp phân trang
        public async Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetNccNewPaging(string keyword, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<List<IM_NCC_NEWDTO>>();
            try
            {
                var nccNews = await _repo.GetNccNewPaging(keyword, pageIndex, pageSize);
                result.Data = _mapper.Map<List<IM_NCC_NEWDTO>>(nccNews);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = null;
            }
            return result;
        }
    }
}
