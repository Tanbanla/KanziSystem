using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class NhomViTriRepository: BaseRepository<ACC_NHOMVITRI, int>, INhomViTriRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public NhomViTriRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy danh sách nhóm vị trí
        public async Task<List<ACC_NHOMVITRI>> GetAllNhomViTriAsync()
        {
            try
            {
                return await Task.FromResult(_context.ACC_NHOMVITRIs.ToList());
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách nhóm vị trí: " + ex.Message);
            }
        }
    }
}
