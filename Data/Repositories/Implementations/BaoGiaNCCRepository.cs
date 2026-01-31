using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaNCCRepository: BaseRepository<BaoGia_NCC,int>, IBaoGiaNCCRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaNCCRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lay thong tin nha cung cap theo ma hang
        public async Task<List<BaoGia_NCC>> GetBaoGiaNCCByMaHang(string maHang)
        {
            try
            {
                var result = await _context.BaoGia_NCCs
                .Where(b => b.CHR_MaHang == maHang)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBaoGiaNCCByMaHang: {ex.Message}");
                return new List<BaoGia_NCC>();
            }
        }
    }
}
