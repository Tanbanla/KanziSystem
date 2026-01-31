using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaStepService : BaseService<BaoGia_Step, int, BaoGia_StepDTO>, IBaoGiaStepService
    {
        private readonly IBaoGiaStepRepository _repo;
        public BaoGiaStepService(IBaoGiaStepRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }
        // Lấy danh sách bước báo giá theo mã quy trình
        public async Task<GenericResponse<List<BaoGia_StepDTO>>> GetStepsByNodeAsync(string note)
        {
            var response = new GenericResponse<List<BaoGia_StepDTO>>();
            try
            {
                var data = await _repo.GetStepsByNodeAsync(note);
                response.Data = _mapper.Map<List<BaoGia_StepDTO>>(data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetStepsByNodeAsync: {ex.Message}";
            }
            return response;
        }
        // Lay cach phuong thuc gui mail
        public async Task<GenericResponse<List<BaoGia_StepDTO>>> GetStepsApproverAsync()
        {
            var response = new GenericResponse<List<BaoGia_StepDTO>>();
            try
            {
                var data = await _repo.GetStepsApproverAsync();
                response.Data = _mapper.Map<List<BaoGia_StepDTO>>(data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error in GetStepsApproverAsync: {ex.Message}";
            }
            return response;
        }

    }
}
