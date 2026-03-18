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
            return await Task.FromResult(_context.ACC_NHOMVITRIs.ToList());
        }
        // Insert list Section
        public async Task<bool> InsertListSectionAsync(List<ACC_NHOMVITRI> nhomViTriList)
        {
            if(nhomViTriList == null)
            {
                return false;
            }
            var listTem = new List<ACC_NHOMVITRI>();
            foreach (var item in nhomViTriList)
            {
                var check = _context.ACC_NHOMVITRIs.FirstOrDefault(x => x.Mahangmuctheovitri == item.Mahangmuctheovitri);
                var checkList = listTem.Where(c => c.Mahangmuctheovitri == item.Mahangmuctheovitri).FirstOrDefault();
                if(checkList != null || check != null)
                {
                    continue;
                }
                listTem.Add(item);
            }
            await _context.ACC_NHOMVITRIs.AddRangeAsync(listTem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
