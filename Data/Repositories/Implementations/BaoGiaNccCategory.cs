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
        //public async Task<bool> AddListBaoGiaNccCategory(List<BaoGia_NCC_Category> listBaoGiaNccCategory)
        //{
        //    var listInsert = new List<BaoGia_NCC_Category>();
        //    foreach (var item in listBaoGiaNccCategory)
        //    {
        //        var checklist = listInsert.Where(c => c.CHR_MaNCC == item.CHR_MaNCC && c.NVCHR_ChungLoai == item.NVCHR_ChungLoai);
        //        if (checklist != null) continue;
        //        var a = await _context.BaoGia_NCC_Categories.Where(c => c.CHR_MaNCC == item.CHR_MaNCC && c.NVCHR_ChungLoai == item.NVCHR_ChungLoai).FirstOrDefaultAsync();
        //        if (a != null) continue;

        //        listInsert.Add(item);
        //    }
        //    if(listInsert.Count == 0)
        //    {
        //        return false;
        //    }
        //    await _context.BaoGia_NCC_Categories.AddRangeAsync(listInsert);
        //    return await _context.SaveChangesAsync() > 0;
        //}
        public async Task<bool> AddListBaoGiaNccCategory(List<BaoGia_NCC_Category> listBaoGiaNccCategory)
        {
            if (listBaoGiaNccCategory == null || !listBaoGiaNccCategory.Any())
                return false;

            // Lấy tất cả các cặp (MaNCC, ChungLoai) đã tồn tại trong database
            var existingKeys = await _context.BaoGia_NCC_Categories
                .Where(c => listBaoGiaNccCategory.Select(x => x.CHR_MaNCC).Contains(c.CHR_MaNCC))
                .Select(c => new { c.CHR_MaNCC, c.NVCHR_ChungLoai })
                .ToListAsync();

            // Tạo HashSet để kiểm tra nhanh
            var existingSet = new HashSet<(string MaNCC, string ChungLoai)>(
                existingKeys.Select(k => (k.CHR_MaNCC, k.NVCHR_ChungLoai))
            );

            // Lọc các item chưa tồn tại trong database và không trùng trong list
            var uniqueItems = listBaoGiaNccCategory
                .Where(item => !existingSet.Contains((item.CHR_MaNCC, item.NVCHR_ChungLoai)))
                .GroupBy(item => new { item.CHR_MaNCC, item.NVCHR_ChungLoai })
                .Select(g => g.First()) // Lấy item đầu tiên nếu có trùng trong list
                .ToList();

            if (!uniqueItems.Any())
            {
                return false;
            }

            await _context.BaoGia_NCC_Categories.AddRangeAsync(uniqueItems);
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
