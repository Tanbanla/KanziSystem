using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmCategoryRepository: BaseRepository<TM_Category, int>, ITmCategoryRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public TmCategoryRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
        }
        // lấy danh sách chủng loại
        public async Task<List<string>> GetListCategory()
        {
            var sql = "SELECT Distinct NVCHR_Category FROM TM_Category";
            return (await _conn.QueryAsync<string>(sql)).ToList();
        }
        // Tìm kiếm chủng loại theo tên
        public async Task<List<TM_Category>> SearchCategoryByName(string name)
        {
            var sql = "SELECT * FROM TM_Category WHERE (@Name = null or @Name = '') or NVCHR_Category LIKE @Name";
            return (await _conn.QueryAsync<TM_Category>(sql, new { Name = $"%{name}%" })).ToList();
        }
    }
}
