using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmCategoryService: BaseService<TM_Category, int , TM_CategoryDTO> , ITmCategoryService
    {
        private readonly ITmCategoryRepository _repo;
        private readonly IMapper _mapper;
        public TmCategoryService(ITmCategoryRepository repo, IMapper mapper) : base (repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // lấy danh sách chủng loại
        public async Task<GenericResponse<List<string>>> GetListCategory()
        {
            var result = new GenericResponse<List<string>>();
            try
            {
                result.Data = await _repo.GetListCategory();
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Tìm kiếm chủng loại theo tên
        public async Task<GenericResponse<List<TM_CategoryDTO>>> SearchCategoryByName(string name, int? pageIndex, int? pageSize)
        {
            var result = new GenericResponse<List<TM_CategoryDTO>>();
            try
            {
                var categories = await _repo.SearchCategoryByName(name, pageIndex, pageSize);
                result.Data = _mapper.Map<List<TM_CategoryDTO>>(categories);
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Thêm mới chủng loại
        public async Task<GenericResponse<bool>> AddCategory(TM_CategoryDTO categoryDTO)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var category = _mapper.Map<TM_Category>(categoryDTO);
                var a = await _repo.AddAsync(category);
                result.Data = true;
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Thêm nhiều chủng loại 
        public async Task<GenericResponse<List<TM_CategoryDTO>>> AddListCategory(List<TM_CategoryDTO> categoryDTOs)
        {
            var result = new GenericResponse<List<TM_CategoryDTO>>();
            try
            {
                var categories = _mapper.Map<List<TM_Category>>(categoryDTOs);
                var addedCategories = await _repo.AddMultiAsync(categories);
                result.Data = categoryDTOs;
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        // Xóa thông tin chung loại
        public async Task<GenericResponse<bool>> DeleteCategory(int id)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var isDeleted = await _repo.DeleteAsync(id);
                result.Data = true;
                result.Success = true;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
