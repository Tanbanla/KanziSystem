using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaWFDefinitionRepository: BaseRepository<BaoGia_WorkflowDefinition, int>, IBaoGiaWFDefinitionRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaWFDefinitionRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration)
        {
            _context = context;
        }

    }
}
