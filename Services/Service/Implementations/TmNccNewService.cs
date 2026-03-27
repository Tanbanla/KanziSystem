using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmNccNewService:  BaseService<IM_NCC_NEWDTO, int, IM_NCC_NEW>, ITmNccNewService
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
            }
            
            return result;
        }
        // Lây thông tin nhà cung cấp phân trang
        public async Task<GenericResponse<List<IM_NCC_NEWDTO>>> GetNccNewPaging(string? CodeNcc, string? NameNcc, int pageIndex, int pageSize)
        {
            var result = new GenericResponse<List<IM_NCC_NEWDTO>>();
            try
            {
                var nccNews = await _repo.GetNccNewPaging(CodeNcc, NameNcc, pageIndex, pageSize);
                result.Data = _mapper.Map<List<IM_NCC_NEWDTO>>(nccNews);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Xoa thong tin nha cung cap
        public async Task<GenericResponse<bool>> DeleteNccNewByCode(int id, string userAction)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var isDeleted = await _repo.DeleteNccNewByCode(id, userAction);
                result.Data = isDeleted;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Thêm thông tin nhà cung cấp 
        public async Task<GenericResponse<bool>> AddNccNew(IM_NCC_NEWDTO nccNew)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var nccNewEntity = _mapper.Map<IM_NCC_NEW>(nccNew);
                var isAdded = await _repo.AddNccNew(nccNewEntity);
                result.Data = isAdded;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update thong tin nha cung cap
        public async Task<GenericResponse<bool>> UpdateNccNew(IM_NCC_NEWDTO nccNew)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var nccNewEntity = _mapper.Map<IM_NCC_NEW>(nccNew);
                var isUpdated = await _repo.UpdateNccNew(nccNewEntity);
                result.Data = isUpdated;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // thêm danh sách nhà cung cấp
        public async Task<GenericResponse<bool>> AddListNccNew(List<IM_NCC_NEWDTO> listNccNew)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var listNccNewEntity = _mapper.Map<List<IM_NCC_NEW>>(listNccNew);
                var isAdded = await _repo.AddListNccNew(listNccNewEntity);
                result.Data = isAdded;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Update Short Name Suppelier
        public async Task<GenericResponse<bool>> UpdateShortNames(List<IM_NCC_NEW> listUpate)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var listNccNewEntity = _mapper.Map<List<IM_NCC_NEW>>(listUpate);
                var isUpdated = await _repo.UpdateShortNames(listNccNewEntity);
                result.Data = isUpdated;
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
