using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Text;

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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var maNccs = listBaoGiaNccCategory
                    .Select(x => x.CHR_MaNCC)
                    .Distinct()
                    .ToList();

                var existingEntities = await _context.BaoGia_NCC_Categories
                    .Where(x => maNccs.Contains(x.CHR_MaNCC))
                    .ToListAsync();

                var existingDict = existingEntities
                    .GroupBy(x => new { x.CHR_MaNCC, x.NVCHR_ChungLoai })
                    .ToDictionary(
                        g => (g.Key.CHR_MaNCC, g.Key.NVCHR_ChungLoai),
                        g => g.First());

                var toAdd = new List<BaoGia_NCC_Category>();

                foreach (var item in listBaoGiaNccCategory)
                {
                    var key = (item.CHR_MaNCC, item.NVCHR_ChungLoai);

                    if (existingDict.TryGetValue(key, out var existing))
                    {
                        existing.CHR_PIC = item.CHR_PIC;
                        existing.CHR_Mail = item.CHR_Mail;
                    }
                    else
                    {
                        toAdd.Add(item);
                    }
                }

                if (toAdd.Any())
                    await _context.BaoGia_NCC_Categories.AddRangeAsync(toAdd);

                // Update ShortName NCC
                var shortNameDict = listBaoGiaNccCategory
                    .Where(x => !string.IsNullOrWhiteSpace(x.NVCHR_SanXuat))
                    .GroupBy(x => x.CHR_MaNCC)
                    .ToDictionary(g => g.Key, g => g.First().NVCHR_SanXuat);

                var nccs = await _context.IM_NCC_NEWs
                    .Where(x => shortNameDict.Keys.Contains(x.Ma))
                    .ToListAsync();

                foreach (var ncc in nccs)
                {
                    ncc.ShortName = shortNameDict[ncc.Ma];
                }

                // Add Category mới
                var categoryKeys = listBaoGiaNccCategory
                    .Where(x => !string.IsNullOrWhiteSpace(x.NVCHR_ChungLoai))
                    .Select(x => x.NVCHR_ChungLoai.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingCategories = await _context.TM_Categories
                    .Where(x => categoryKeys.Contains(x.NVCHR_Category))
                    .Select(x => x.NVCHR_Category)
                    .ToListAsync();

                var existingSet = new HashSet<string>(
                    existingCategories,
                    StringComparer.OrdinalIgnoreCase);

                var newCategories = categoryKeys
                    .Where(x => !existingSet.Contains(x))
                    .Select(x => new TM_Category
                    {
                        NVCHR_Category = x
                    })
                    .ToList();

                if (newCategories.Any())
                    await _context.TM_Categories.AddRangeAsync(newCategories);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
            static string NormalizeText(string? input)
            {
                return (input ?? string.Empty).Trim().Normalize(NormalizationForm.FormC);
            }

            var missing = new List<CheckSupplierByCategoryModel>();
            if (request == null || request.Count == 0)
                return missing;

            var normalizedRequest = request
                .Select(r => new CheckSupplierByCategoryModel
                {
                    MaDon = NormalizeText(r.MaDon),
                    ChungLoai = NormalizeText(r.ChungLoai)
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

            var existingSet = new HashSet<string>(
                existing.Select(e => NormalizeText(e.Ma) + "|" + NormalizeText(e.ChungLoai)),
                StringComparer.OrdinalIgnoreCase
            );

            missing = normalizedRequest
                .Where(r => !existingSet.Contains(r.MaDon + "|" + r.ChungLoai))
                .ToList();

            return missing;
        }

        // Thêm chủng loại nhà cung cấp
        public async Task<bool> InsertCategoryNccAsync(BaoGia_NCC_Category dto)
        {
            // Lấy (MaNCC, ChungLoai) đã tồn tại trong database
            var existingKeys = await _context.BaoGia_NCC_Categories
                .Where(c => c.NVCHR_ChungLoai == dto.NVCHR_ChungLoai && c.CHR_MaNCC == dto.CHR_MaNCC)
                .ToListAsync();

            if (existingKeys.Any())
            {
                throw new Exception("Đã tồn tại, không thêm");
            }

            // Kiểm tra xem nhà cung cấp có tồn tại trong bảng IM_NCC_NEW hay không

            var infor = await _context.IM_NCC_NEWs.FirstOrDefaultAsync(x => x.Ma == dto.CHR_MaNCC);
            if (infor == null)
            {
                throw new Exception("Không tìm thấy thông tin nhà cung cấp");
            }

            // Kiểm tra xem chủng loại có tồn tại trong bảng TM_Category hay không
            var category = await _context.TM_Categories.FirstOrDefaultAsync(x => x.NVCHR_Category == dto.NVCHR_ChungLoai);
            if (category == null)
            {
                var newCategory = new TM_Category
                {
                    NVCHR_Category = dto.NVCHR_ChungLoai ?? "",
                    DTM_CreateBy = DateTime.Now,
                    CHR_CreateBy = dto.CHR_CreateBy
                };
                await _context.TM_Categories.AddAsync(newCategory);
            }

            await _context.BaoGia_NCC_Categories.AddAsync(dto);
            return await _context.SaveChangesAsync() > 0;
        }

        // Xóa chủng loại theo mã nhà cung cấp và chủng loại
        public async Task<bool> DeleteCategoryNccAsync(List<BaoGia_NCC_Category> listDelete)
        {
            if (listDelete == null || !listDelete.Any())
                throw new Exception("Không có dữ liệu để xóa");

            static string NormalizeText(string? value)
            {
                return (value ?? string.Empty).Trim().Normalize(NormalizationForm.FormC);
            }

            var keys = listDelete
                .Select(x => $"{NormalizeText(x.CHR_MaNCC)}|{NormalizeText(x.NVCHR_ChungLoai)}")
                .Where(x => !x.StartsWith("|", StringComparison.Ordinal))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (keys.Count == 0)
                return false;

            var maNccList = listDelete
                .Select(x => NormalizeText(x.CHR_MaNCC))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var entitiesToDelete = await _context.BaoGia_NCC_Categories
                .Where(c => c.CHR_MaNCC != null
                    && maNccList.Contains(c.CHR_MaNCC.Trim()))
                .ToListAsync();

            entitiesToDelete = entitiesToDelete
                .Where(c => keys.Contains(
                    $"{NormalizeText(c.CHR_MaNCC)}|{NormalizeText(c.NVCHR_ChungLoai)}"))
                .ToList();

            if (!entitiesToDelete.Any())
                return false;

            _context.BaoGia_NCC_Categories.RemoveRange(entitiesToDelete);

            return await _context.SaveChangesAsync() > 0;
        }
        // Xóa Nhà cung cấp
        public async Task<bool> DeleteSupplierAsync(List<BaoGia_NCC_Category> listDelete)
        {
            if (listDelete == null || !listDelete.Any())
                throw new Exception("Không có dữ liệu để xóa");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var maNCCs = listDelete
                    .Select(x => x.CHR_MaNCC)
                    .Distinct()
                    .ToList();

                // Lấy dữ liệu Category cần xóa
                var entitiesToDelete = await _context.BaoGia_NCC_Categories
                    .Where(x => maNCCs.Contains(x.CHR_MaNCC))
                    .ToListAsync();

                if (!entitiesToDelete.Any())
                    return false;

                // Lấy thông tin NCC
                var nccToUpdate = await _context.IM_NCC_NEWs
                    .Where(x => maNCCs.Contains(x.Ma))
                    .ToListAsync();

                // Soft delete NCC
                foreach (var ncc in nccToUpdate)
                {
                    ncc.Xoa = true;
                }

                _context.IM_NCC_NEWs.UpdateRange(nccToUpdate);

                // Xóa Category
                _context.BaoGia_NCC_Categories.RemoveRange(entitiesToDelete);

                var result = await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
