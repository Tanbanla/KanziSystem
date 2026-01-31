using Dapper;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaStatusRepository: BaseRepository<BaoGia_Status, int>, IBaoGiaStatusRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaStatusRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration) {
            _context = context;
        }
        // Lấy danh sach trang thai
        public async Task<List<BaoGia_Status>> GetListStatusAsync()
        {

            var sql = "SELECT * FROM BaoGia_Status where CHR_Flag = 1";
            return (await _conn.QueryAsync<BaoGia_Status>(sql)).ToList();
        }
    }
}
