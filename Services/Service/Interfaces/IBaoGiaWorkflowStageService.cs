using AutoMapper;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces
{
    public interface IBaoGiaWorkflowStageService: IBaseService<BaoGia_WorkflowStage, int, BaoGia_WorkflowStageDTO>
    {
        Task<GenericResponse<bool>> UpdateWFStageAsync(BaoGia_WorkflowStageDTO entity);
        Task<GenericResponse<bool>> DeleteWFStageSoftAsync(int StageId);
        Task<GenericResponse<bool>> AddWFStageAsync(BaoGia_WorkflowStageDTO entity);
    }
}
