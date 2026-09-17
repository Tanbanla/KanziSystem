using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaWorkflowStageRepository: BaseRepository<BaoGia_WorkflowStage, int>, IBaoGiaWorkflowStageRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaWorkflowStageRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {

            _context = context;

        }
        public async Task<bool> UpdateWFStageAsync(BaoGia_WorkflowStage entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var existingEntity = await _context.BaoGia_WorkflowStages.FindAsync(entity.StageID);
            if (existingEntity == null) {
                throw new InvalidOperationException($"Workflow stage with ID {entity.StageID} not found.");
            }
            existingEntity.StageOrder = entity.StageOrder;
            existingEntity.StageCode = entity.StageCode;
            existingEntity.StageName = entity.StageName;
            existingEntity.IsActive = entity.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWFStageSoftAsync(int StageId)
        {
            if(StageId <0) throw new ArgumentException("StageId must be a positive integer.", nameof(StageId));

            var existingEntity = await _context.BaoGia_WorkflowStages.FindAsync(StageId);
            if (existingEntity == null) throw new InvalidOperationException($"Workflow stage with ID {StageId} not found.");

            existingEntity.IsActive = false;

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> AddWFStageAsync(BaoGia_WorkflowStage entity)
        {
            if(entity == null) throw new ArgumentNullException(nameof(entity));

            await _context.BaoGia_WorkflowStages.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
