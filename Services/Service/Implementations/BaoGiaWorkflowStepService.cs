using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Implementations
{
    public class BaoGiaWorkflowStepService: BaseService<BaoGia_WorkflowStep, int, BaoGia_WorkflowStepDTO>, IBaoGiaWorkflowStepService
    {
        private readonly IBaoGiaWorkflowStepRepository _baoGiaWorkflowStepRepository;
        private readonly IMapper _mapper;
        public BaoGiaWorkflowStepService(IBaoGiaWorkflowStepRepository baoGiaWorkflowStepRepository, IMapper mapper)
            : base(baoGiaWorkflowStepRepository, mapper)
        {
            _baoGiaWorkflowStepRepository = baoGiaWorkflowStepRepository;
            _mapper = mapper;
        }
        public async Task<GenericResponse<bool>> UpdateWFStep(BaoGia_WorkflowStepDTO entity)
        {
             var result =  new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<BaoGia_WorkflowStep>(entity);
                result.Data = await _baoGiaWorkflowStepRepository.UpdateWFStep(data);
                result.Success = true;

            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating workflow step: " + ex.Message;
            }

            return result;
        }
        public async Task<GenericResponse<bool>> DeleteWFStep(int stepID)
        {
            var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _baoGiaWorkflowStepRepository.DeleteWFStep(stepID);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting workflow step: " + ex.Message;
            }
            return result;
        }
        public async Task<GenericResponse<bool>> CreateWFStep(BaoGia_WorkflowStepDTO entity)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var data = _mapper.Map<BaoGia_WorkflowStep>(entity);
                result.Data = await _baoGiaWorkflowStepRepository.CreateWFStep(data);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error creating workflow step: " + ex.Message;
            }
            return result;
        }
    }
}
