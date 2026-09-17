using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaWFDefinitionStepRepostitory: BaseRepository<BaoGia_WorkflowDefinitionStep, int>, IBaoGiaWFDefinitionStepRepostitory
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaWFDefinitionStepRepostitory(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {

            _context = context;

        }
    }
}
