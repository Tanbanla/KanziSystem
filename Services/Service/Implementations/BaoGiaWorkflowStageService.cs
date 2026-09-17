using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWorkflowStageService: BaseService<BaoGia_WorkflowStage, int, BaoGia_WorkflowStageDTO>, IBaoGiaWorkflowStageService
    {
        private readonly IBaoGiaWorkflowStageRepository _repo;
        private readonly IMapper _mapper;
        public BaoGiaWorkflowStageService(IBaoGiaWorkflowStageRepository repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<GenericResponse<bool>> UpdateWFStageAsync(BaoGia_WorkflowStageDTO entity)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var d = _mapper.Map<BaoGia_WorkflowStage>(entity);
                result.Data = await _repo.UpdateWFStageAsync(d);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public async Task<GenericResponse<bool>> DeleteWFStageSoftAsync(int StageId)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repo.DeleteWFStageSoftAsync(StageId);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public async Task<GenericResponse<bool>> AddWFStageAsync(BaoGia_WorkflowStageDTO entity)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var d = _mapper.Map<BaoGia_WorkflowStage>(entity);
                result.Data = await _repo.AddWFStageAsync(d);
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
