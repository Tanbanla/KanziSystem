using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaWorkflowStepRepository: BaseRepository<BaoGia_WorkflowStep, int>, IBaoGiaWorkflowStepRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaWorkflowStepRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration)
        {
            _context = context;
        }
        public async Task<bool> UpdateWFStep(BaoGia_WorkflowStep entity)
        {
            if (entity == null || entity.StageID <= 0)
                throw new ArgumentNullException(nameof(entity));

            var data = await _context.BaoGia_WorkflowSteps
                .FindAsync(entity.StepID);

            if (data == null)
                throw new InvalidOperationException(
                    $"Workflow step with ID {entity.StepID} not found.");

            // Check StepCode bị trùng trong cùng StageID
            var isDuplicateStepCode = await _context.BaoGia_WorkflowSteps
                .AnyAsync(x =>
                    x.StageID == entity.StageID &&
                    x.StepCode == entity.StepCode &&
                    x.StepID != entity.StepID);

            if (isDuplicateStepCode)
                throw new InvalidOperationException(
                    $"StepCode '{entity.StepCode}' đã tồn tại trong StageID = {entity.StageID}.");

            // Update
            data.StepOrder = entity.StepOrder;
            data.StepCode = entity.StepCode;
            data.StepNameEN = entity.StepNameEN;
            data.StepName = entity.StepName;
            data.Description = entity.Description;
            data.DefaultDurationHours = entity.DefaultDurationHours;
            data.IsActive = entity.IsActive;
            data.StageID = entity.StageID;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteWFStep(int stepID)
        {
            var data = await _context.BaoGia_WorkflowSteps.FindAsync(stepID);

            if (data == null) throw new InvalidOperationException($"Workflow stage with ID {stepID} not found.");

            _context.BaoGia_WorkflowSteps.Remove(data);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> CreateWFStep(BaoGia_WorkflowStep entity)
        {
            if (entity == null || entity.StageID <= 0) throw new ArgumentNullException(nameof(entity));

            var checkDate = await _context.BaoGia_WorkflowSteps
                .Where(c => c.StageID == entity.StageID && (c.StepCode == entity.StepCode || c.StepOrder == entity.StepOrder))
                .FirstOrDefaultAsync();

            if(checkDate != null) throw new InvalidOperationException($"Workflow step with Code {entity.StepCode} or Order {entity.StepOrder} already exists in Stage {entity.StageID}.");

            _context.BaoGia_WorkflowSteps.Add(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
