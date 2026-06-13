using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

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
        public async Task<bool> AddListBaoGiaNccCategoryOld(List<BaoGia_NCC_Category> listBaoGiaNccCategory)
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
        public async Task<bool> AddListBaoGiaNccCategory(List<BaoGia_NCC_Category> listBaoGiaNccCategory)
        {
            if (listBaoGiaNccCategory == null || !listBaoGiaNccCategory.Any())
                return false;

            var existingEntities = await _context.BaoGia_NCC_Categories
                .Where(c => listBaoGiaNccCategory.Select(x => x.CHR_MaNCC).Contains(c.CHR_MaNCC))
                .ToListAsync();

            var existingDict = existingEntities
                .ToDictionary(e => (e.CHR_MaNCC, e.NVCHR_ChungLoai));

            var toAdd = new List<BaoGia_NCC_Category>();

            foreach (var item in listBaoGiaNccCategory)
            {
                var key = (item.CHR_MaNCC, item.NVCHR_ChungLoai);

                if (existingDict.TryGetValue(key, out var existingEntity))
                {
                    existingEntity.CHR_PIC = item.CHR_PIC;
                    existingEntity.CHR_Mail = item.CHR_Mail;
                }
                else
                {
                    toAdd.Add(item);
                }
            }

            if (toAdd.Any())
            {
                await _context.BaoGia_NCC_Categories.AddRangeAsync(toAdd);
            }
            // update short name
            var shortNameDict = listBaoGiaNccCategory
                .Where(x => !string.IsNullOrEmpty(x.NVCHR_SanXuat))
                .GroupBy(x => x.CHR_MaNCC)
                .ToDictionary(g => g.Key, g => g.First().NVCHR_SanXuat);

            var listUpdate = await _context.IM_NCC_NEWs
                .Where(c => shortNameDict.Keys.Contains(c.Ma))
                .ToListAsync();

            foreach (var item in listUpdate)
            {
                if (shortNameDict.TryGetValue(item.Ma, out var shortName))
                {
                    item.ShortName = shortName;
                }
            }
            // update Catergory
            var categoryKeys = listBaoGiaNccCategory
                .Where(x => !string.IsNullOrWhiteSpace(x.NVCHR_ChungLoai))
                .Select(x => x.NVCHR_ChungLoai!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingCategoryNames = await _context.TM_Categories
                .Where(c => categoryKeys.Contains(c.NVCHR_Category))
                .Select(c => c.NVCHR_Category)
                .ToListAsync();
            // Thêm mới 
            var existingSet = new HashSet<string>(existingCategoryNames, StringComparer.OrdinalIgnoreCase);

            var newCategories = categoryKeys
                .Where(key => !existingSet.Contains(key))
                .Select(key => new TM_Category
                {
                    NVCHR_Category = key
                })
                .ToList();

            if (newCategories.Any())
            {
                await _context.TM_Categories.AddRangeAsync(newCategories);
            }
            return await _context.SaveChangesAsync() > 0;
        }
        // update thong tin
        public async Task<bool> UpdateBaoGiaNccCategory(BaoGia_NCC_Category baoGiaNccCategory)
        {
            _context.BaoGia_NCC_Categories.Update(baoGiaNccCategory);
            return await _context.SaveChangesAsync() > 0;
        }
        // Check Supperlier and Catergory
        public async Task<bool> CheckSupperlier(string codeSupperlier, string catergory)
        {
            var result = await _context.BaoGia_NCC_Categories.AnyAsync(x => x.CHR_MaNCC == codeSupperlier && x.NVCHR_ChungLoai == catergory);
            return result;
        }
        public async Task<List<CheckSupplierByCategoryModel>> CheckSupperlierByCategory(List<CheckSupplierByCategoryModel> request)
        {
            var missing = new List<CheckSupplierByCategoryModel>();
            if (request == null || request.Count == 0)
                return missing;

            var normalizedRequest = request
                .Select(r => new CheckSupplierByCategoryModel
                {
                    MaDon = (r.MaDon ?? string.Empty).Trim(),
                    ChungLoai = (r.ChungLoai ?? string.Empty).Trim()
                })
                .Where(r => !string.IsNullOrEmpty(r.MaDon))
                .DistinctBy(r => (r.MaDon, r.ChungLoai))
                .ToList();

            if (normalizedRequest.Count == 0)
                return missing;

            var maDonList = normalizedRequest
                .Select(r => r.MaDon)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = await _context.BaoGia_NCC_Categories
                .AsNoTracking()
                .Where(c => maDonList.Contains(c.CHR_MaNCC))
                .Select(c => new { Ma = (c.CHR_MaNCC ?? string.Empty).Trim(), ChungLoai = (c.NVCHR_ChungLoai ?? string.Empty).Trim() })
                .ToListAsync();

            var existingSet = new HashSet<string>(existing.Select(e => e.Ma + "|" + e.ChungLoai), StringComparer.OrdinalIgnoreCase);

            missing = normalizedRequest
                .Where(r => !existingSet.Contains(r.MaDon + "|" + r.ChungLoai))
                .ToList();

            return missing;
        }
    }
}
