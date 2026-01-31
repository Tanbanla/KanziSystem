using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using Dapper;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmNccNewRepository: BaseRepository<IM_NCC_NEW, int>, ITmNccNewRepository
    {
        public TmNccNewRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration) { }

        //Lấy danh sách nhà cung cấp  ALL
        public async Task<List<IM_NCC_NEW>> GetAllNccNew()
        {
            try
            {
                var sql = "SELECT * FROM IM_NCC_NEW";
                return (await _conn.QueryAsync<IM_NCC_NEW>(sql)).ToList();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in GetAllNccNew: {ex.Message}");
                return new List<IM_NCC_NEW>();
            }

        }
        // Lấy nhà cung cấp theo mã nhà cung cấp
        public async Task<IM_NCC_NEW> GetNccNewByCode(string MaNCC)
        {
            var sql = "SELECT * FROM IM_NCC_NEW WHERE MA_NCC = @MaNCC";
            var parameters = new { MaNCC };
            return await _conn.QueryFirstOrDefaultAsync<IM_NCC_NEW>(sql, parameters);
        }
        // Lây thông tin nhà cung cấp phân trang
        public async Task<List<IM_NCC_NEW>> GetNccNewPaging(string keyword, int pageIndex, int pageSize)
        {
            var sql = @"
                SELECT *
                FROM IM_NCC_NEW
                WHERE (@Keyword IS NULL OR MA_NCC LIKE '%' + @Keyword + '%' OR TEN_NCC LIKE '%' + @Keyword + '%')
                ORDER BY MA_NCC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";
            var parameters = new
            {
                Keyword = string.IsNullOrEmpty(keyword) ? null : keyword,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            };
            return (await _conn.QueryAsync<IM_NCC_NEW>(sql, parameters)).ToList();
        }
    }
}
