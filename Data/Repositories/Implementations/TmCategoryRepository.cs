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
        public async Task<ListRequest<TM_Category>> SearchCategoryByName(string name, int? pageIndex, int? pageSize)
        {
            var param = new DynamicParameters();
            var nameParam = string.IsNullOrWhiteSpace(name) ? null : $"%{name}%";
            param.Add("Name", nameParam);

            var whereClause = "WHERE (@Name IS NULL OR @Name = '' OR NVCHR_Category LIKE @Name)";

            if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
            {
                var offset = (pageIndex.Value - 1) * pageSize.Value;
                var countSql = $"SELECT COUNT(1) FROM TM_Category {whereClause}";
                var total = await _conn.ExecuteScalarAsync<int>(countSql, param);

                var sql = $@"SELECT *
                    FROM TM_Category
                    {whereClause}
                    ORDER BY NVCHR_Category
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                param.Add("Offset", offset);
                param.Add("PageSize", pageSize.Value);

                var items = (await _conn.QueryAsync<TM_Category>(sql, param)).ToList();

                return new ListRequest<TM_Category>
                {
                    Data = items,
                    TotalCount = total
                };
            }
            var sqlAll = $@"SELECT *
                FROM TM_Category
                {whereClause}
                ORDER BY NVCHR_Category";

            var allItems = (await _conn.QueryAsync<TM_Category>(sqlAll, param)).ToList();
            return new ListRequest<TM_Category>
            {
                Data = allItems,
                TotalCount = allItems.Count
            };
        }
    }
}
