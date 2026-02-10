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
    }
}
