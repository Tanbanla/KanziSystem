using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaStatusService: BaseService<BaoGia_Status, int, BaoGia_StatusDTO>, IBaoGiaStatusService
    {
        private readonly IBaoGiaStatusRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaStatusService(IBaoGiaStatusRepository repo, IMapper mapper) : base (repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<GenericResponse<List<BaoGia_StatusDTO>>> GetListStatusAsync()
        {
            var response = new GenericResponse<List<BaoGia_StatusDTO>>();
            try
            {
                var statuses = await _repo.GetAllAsync();
                response.Data = _mapper.Map<List<BaoGia_StatusDTO>>(statuses);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
