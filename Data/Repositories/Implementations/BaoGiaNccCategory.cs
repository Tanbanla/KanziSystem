using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaNccCategory: BaseRepository<BaoGia_NCC_Category, int>, IBaoGiaNccCategory
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaNccCategory(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // lấy danh sách theo mã NCC
        public async Task<List<BaoGia_NCC_Category>> GetBaoGiaNccCategoryByMaNCC(string maNCC)
        {
            var result = await _context.BaoGia_NCC_Categories.Where(x => x.CHR_MaNCC == maNCC).ToListAsync();
            return result;

        }
        // lấy danh sách theo chung loai
        public async Task<List<BaoGia_NCC_Category>> GetBaoGiaNccCategoryByChungLoai(string chungLoai)
        {
            var result = await _context.BaoGia_NCC_Categories.Where(x => x.NVCHR_ChungLoai == chungLoai).ToListAsync();
            return result;
        }
        // them thong tin
        public async Task<bool> AddBaoGiaNccCategory(BaoGia_NCC_Category baoGiaNccCategory)
        {
            await _context.BaoGia_NCC_Categories.AddAsync(baoGiaNccCategory);
            return await _context.SaveChangesAsync() > 0;
        }
        // xoa thong tin
        public async Task<bool> DeleteBaoGiaNccCategory(int id)
        {
            var entity = await _context.BaoGia_NCC_Categories.FindAsync(id);
            if (entity == null) return false;

            _context.BaoGia_NCC_Categories.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        // them danh sach
        public async Task<bool> AddListBaoGiaNccCategory(List<BaoGia_NCC_Category> listBaoGiaNccCategory)
        {
            await _context.BaoGia_NCC_Categories.AddRangeAsync(listBaoGiaNccCategory);
            return await _context.SaveChangesAsync() > 0;
        }
        // update thong tin
        public async Task<bool> UpdateBaoGiaNccCategory(BaoGia_NCC_Category baoGiaNccCategory)
        {
            _context.BaoGia_NCC_Categories.Update(baoGiaNccCategory);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
