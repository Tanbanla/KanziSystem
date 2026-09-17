using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaWorkflowStepRepository: IBaseRepository<BaoGia_WorkflowStep, int>
    {
        Task<bool> UpdateWFStep(BaoGia_WorkflowStep entity);
        Task<bool> DeleteWFStep(int stepID);
        Task<bool> CreateWFStep(BaoGia_WorkflowStep entity);
    }
}
