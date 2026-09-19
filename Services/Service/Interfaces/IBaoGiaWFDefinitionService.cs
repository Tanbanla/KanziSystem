using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaWFDefinitionService: IBaseService<BaoGia_WorkflowDefinition, int, BaoGia_WorkflowDefinitionDTO>
    {
        // lay list thong tin WorkflowID
        Task<GenericResponse<List<dynamic>>> GetWorkflowIDs();
    }
}
