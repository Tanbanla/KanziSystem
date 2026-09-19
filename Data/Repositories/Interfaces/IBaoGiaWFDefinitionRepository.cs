using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces
{
    public interface IBaoGiaWFDefinitionRepository: IBaseRepository<BaoGia_WorkflowDefinition, int>
    {
        // lay list thong tin WorkflowID
        Task<List<dynamic>> GetWorkflowIDs();

    }
}
