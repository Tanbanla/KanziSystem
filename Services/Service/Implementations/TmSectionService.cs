using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class TmSectionService: BaseService<TM_SECTION,string, TM_SECTIONDTO> , ITmSectionService
    {
        private readonly ITmSectionRepository _repo;
        public TmSectionService(ITmSectionRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }
        // Lấy danh sách phòng ban
        public async Task<GenericResponse<List<TM_SECTIONDTO>>> GetAllSectionsAsync()
        {
            var response = new GenericResponse<List<TM_SECTIONDTO>>();
            try
            {
                var data = await _repo.GetAllSectionsAsync();
                response.Data = _mapper.Map<List<TM_SECTIONDTO>>(data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetAllSectionsAsync: {ex.Message}";
            }
            return response;
        }
    }
}
