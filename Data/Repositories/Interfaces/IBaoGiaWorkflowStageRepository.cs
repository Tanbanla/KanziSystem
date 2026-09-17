using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaWorkflowStageRepository: IBaseRepository<BaoGia_WorkflowStage, int >
    {
        Task<bool> UpdateWFStageAsync (BaoGia_WorkflowStage entity);
        Task<bool> DeleteWFStageSoftAsync(int StageId);
        Task<bool> AddWFStageAsync(BaoGia_WorkflowStage entity);
    }
}
