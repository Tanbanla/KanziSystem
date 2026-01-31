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
            _connectionString = options.Value.WorkingControlConnection;
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
    }
}
