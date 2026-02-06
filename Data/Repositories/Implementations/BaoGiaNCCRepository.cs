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
        // lay thong tin san pham lien quan den nha cung cap
        public async Task<List<BaoGia_NCC>> GetBaoGiaNCCByNCC(string maNCC)
        {
            var result = await _context.BaoGia_NCCs
            .Where(b => b.CHR_MaNCC == maNCC)
            .ToListAsync();
            return result;
        }
        // them thong tin
        public async Task<bool> AddBaoGiaNCC(BaoGia_NCC baoGiaNCC)
        {
            await _context.BaoGia_NCCs.AddAsync(baoGiaNCC);
            await _context.SaveChangesAsync();
            return true;
        }
        // xoa thong tin    
        public async Task<bool> DeleteBaoGiaNCC(int id)
        {
            var entity = await _context.BaoGia_NCCs.FindAsync(id);
            if (entity == null) return false;

            _context.BaoGia_NCCs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        // update thong tin
        public async Task<bool> UpdateBaoGiaNCC(BaoGia_NCC baoGiaNCC)
        {
            var entity = await _context.BaoGia_NCCs.FindAsync(baoGiaNCC.ID);
            if (entity == null) return false;
            entity.CHR_MaHang = baoGiaNCC.CHR_MaHang;
            entity.CHR_MaNCC = baoGiaNCC.CHR_MaNCC;
            entity.NVCHAR_TenNCC = baoGiaNCC.NVCHAR_TenNCC;
            entity.CHR_UpdateBY = baoGiaNCC.CHR_UpdateBY;
            entity.DTM_UpdateDate = DateTime.Now;
            entity.NVCHR_CodeByNCC = baoGiaNCC.NVCHR_CodeByNCC;
            entity.NVCHR_MakeIn = baoGiaNCC.NVCHR_MakeIn;
            _context.BaoGia_NCCs.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        // update list thong tin
        public async Task<bool> UpdateListBaoGiaNCC(List<BaoGia_NCC> listBaoGia){
            _context.BaoGia_NCCs.UpdateRange(listBaoGia);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
