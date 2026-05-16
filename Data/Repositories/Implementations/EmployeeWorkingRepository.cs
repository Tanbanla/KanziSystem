using Dapper;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using System.Data.SqlClient;
using System.Data;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class EmployeeWorkingRepository: IEmployeeWorkingRepository
    {
        public WorkingSystemContext _context;
        public readonly string _connectionString;

        public EmployeeWorkingRepository( WorkingSystemContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        {
            _context = context;
            _connectionString = options.Value.AgentConnection;
        }
        // Lấy thông tin nhân viên theo ADID or MNV
        public async Task<IEnumerable<dynamic>> GetEmployeeWorkingByIdAsync(string adidOrMnv)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Call stored procedure GetEmployeeWorkingById
                var parameters = new { adid = adidOrMnv };

                try
                {
                    var result = await connection.QueryAsync<dynamic>(
                        "GetEmployeeWorkingById",
                        parameters,
                        commandType: CommandType.StoredProcedure);
                    return result;
                }
                catch (Exception ex)
                {
                    // Optionally log exception here
                    var errorMessage = ex.Message;
                    return null;
                }

            }
        }
        // lay  CHR_CODE_CENTER
        public async Task<List<string>> GetCodeCenterBySec(string codeSecion)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new { codeSecion };
                var result = await connection.QueryAsync<string>(
                    "SELECT [CHR_CODE_CENTER] FROM [Working_Control].[dbo].[TM_CENTER] WHERE CHR_CODE_SEC = @codeSecion",
                    parameters);
                return result.ToList();
            }
        }
        // Lay code phong ban
        public async Task<string> GetCodeSec(string sectionName)
        {
             using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new { sectionName };
                var result = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT [CHR_CODE_SEC] FROM [TM_SECTION] WHERE NVCHR_SEC = @sectionName",
                    parameters);
                return result;
            }
        }
    }
}
