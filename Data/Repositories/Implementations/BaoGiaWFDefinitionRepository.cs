using Microsoft.Extensions.Options;
using Dapper;
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
        public async Task<List<dynamic>> GetWorkflowIDs()
        {
            const string sql = @"
                SELECT w.[WorkflowID],
                       w.[RequestTypeID],
                       w.[FlowCode],
                       w.[WorkflowName],
                       r.[CHR_Code]
                FROM [BaoGia_WorkflowDefinition] AS w
                LEFT JOIN [BaoGia_RequestType] AS r
                    ON w.[RequestTypeID] = r.[ID]
                WHERE w.[IsActive] = 1";

            var result = await _conn.QueryAsync<dynamic>(sql);
            return result.ToList();
        }
    }
}
