using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Agent;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmEmployeeAgentRepository : BaseRepository<TM_EMPLOYEE, decimal>, ITmEmployeeAgentRepository
    {
        private readonly AgentContext _context;

        public TmEmployeeAgentRepository(AgentContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
        }
        public async Task<TM_EMPLOYEE> GetInforEmployeeByMail(string mail)
        {
            var result = await _context.TM_EMPLOYEE.Where(e => e.CHR_EMPLOYEE_MAIL == mail).FirstOrDefaultAsync();
            return result;
        }
    }
}
