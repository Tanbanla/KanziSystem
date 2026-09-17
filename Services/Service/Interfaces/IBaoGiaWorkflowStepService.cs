using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaWorkflowStepService: IBaseService<BaoGia_WorkflowStep, int, BaoGia_WorkflowStepDTO>
    {
        Task<GenericResponse<bool>> UpdateWFStep(BaoGia_WorkflowStepDTO entity);
        Task<GenericResponse<bool>> DeleteWFStep(int stepID);
        Task<GenericResponse<bool>> CreateWFStep(BaoGia_WorkflowStepDTO entity);

    }
}
