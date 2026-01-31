using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmUserRepository: BaseRepository<TM_USER, int>, ITmUserRepository  
    {
        private readonly COST_MANAGEMENTContext _context;
        public TmUserRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Login
        public async Task<TM_USER> Login(string username, string password)
        {
            try
            {
                var user = await _context.TM_USERs
                    .FirstOrDefaultAsync(u => u.CHR_USERID == username && u.VCHR_PASSWORD == password);
                return user!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Login: {ex.Message}");
                return null!;
            }
        }
        // Lấy thông tin user theo ADID
        public async Task<TM_USER> GetUserByAdId(string adId)
        {
            try
            {
                var user = await _context.TM_USERs
                    .FirstOrDefaultAsync(u => u.CHR_USERID == adId);
                return user!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserByAdId: {ex.Message}");
                return null!;
            }
        }
    }
}
