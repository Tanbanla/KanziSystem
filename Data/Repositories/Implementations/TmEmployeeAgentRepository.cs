using Dapper;
using Microsoft.Data.SqlClient;
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
        private readonly string _AgentDBConnectString;

        public TmEmployeeAgentRepository(AgentContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
            _AgentDBConnectString = options.Value.AgentConnection;
        }
        public async Task<TM_EMPLOYEE> GetInforEmployeeByMail(string mail)
        {
            var result = await _context.TM_EMPLOYEE.Where(e => e.CHR_EMPLOYEE_ADID == mail).FirstOrDefaultAsync();
            return result;
        }
        // Lấy thông tin nhân viên theo phòng ban
        public async Task<List<dynamic>> GetApproverBySection(string sectionCode)
        {
            if (string.IsNullOrWhiteSpace(sectionCode))
            {
                return new List<dynamic>();
            }
            using var connection = new SqlConnection(_AgentDBConnectString);
            var sql = @"  Select 
                  CHR_EMPLOYEE_ID  as StaffId,
                  CHR_EMPLOYEE_NAME as FullName,
                  CHR_SEC_CODE as DepartmentCode,
                  CHR_SEC_NAME as DepartmentName,
                  CHR_POSITION_GROUP as Position,
                  CHR_EMPLOYEE_ADID as Adid
                  FROM [AGENTDB].[dbo].[TM_EMPLOYEE]
                  where CHR_SEC_CODE LIKE '%' + @sectionCode + '%' 
                  and CHR_NOTE is null and DTM_LEAVE_DATE is null
                  and CHR_POSITION_GROUP in ('Chief','Expert','Section Manager')
                  ORDER BY CHR_EMPLOYEE_NAME";
            var parameters = new { sectionCode };
            var result = await connection.QueryAsync<dynamic>(sql, parameters);
            return result.AsList();
        }
    }
}
