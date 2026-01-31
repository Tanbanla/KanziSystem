using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmSectionRepository: BaseRepository<TM_SECTION, string>, ITmSectionRepository
    {
        private readonly WorkingSystemContext _context;
        public TmSectionRepository(WorkingSystemContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy danh sách phòng ban
        public async Task<List<TM_SECTION>> GetAllSectionsAsync()
        {
            try
            {
                return await _context.TM_SECTION
                .Distinct()
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetAllSectionsAsync: {ex.Message}");
            }
        }
    }
}
