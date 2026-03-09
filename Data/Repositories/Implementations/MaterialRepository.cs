using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class MaterialRepository : BaseRepository<MATERIAL, int>, IMaterialRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public MaterialRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lay thong tin thao ma hang hoa
        public async Task<MATERIAL> GetByMaHangAsync(string maHang)
        {
            try
            {
                var result = await _context.MATERIALs
                .FirstOrDefaultAsync(m => m.Material_Code == maHang);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByMaHangAsync: {ex.Message}");
                return null;
            }
        }
        // Tim kiem thong tin hang hoa va phan trang
        public async Task<List<MATERIAL>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize)
        {
            var sql = @"
                SELECT *
                FROM MATERIAL
                WHERE (@MaterialName IS NULL OR Material_Name_VN LIKE '%' + @MaterialName + '%' OR Material_Name_EN LIKE '%' + @MaterialName + '%'
				OR Material_Name_JP LIKE '%' + @MaterialName + '%'
				)
                  AND (@MaterialType IS NULL OR Category_VN LIKE '%' + @MaterialType 
                    + '%'OR Category_EN LIKE '%' + @MaterialType + '%' OR Category_JP LIKE '%' + @MaterialType + '%' OR @MaterialType = '')
				  AND(@MaterialCode IS NULL OR Material_Code LIKE '%' + @MaterialCode + '%')
            ";
            if (pageSize > 0 && pageIndex > 0)
            {
                sql += @"
                    ORDER BY MATERIAL_CODE
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";
            }else
            {
                sql += @"
                    ORDER BY MATERIAL_CODE
                ";
            }

            var parameters = new
                {
                    MaterialCode = string.IsNullOrEmpty(MaHang) ? null : MaHang,
                    MaterialName = string.IsNullOrEmpty(Name) ? null : Name,
                    MaterialType = string.IsNullOrEmpty(NhomHang) ? null : NhomHang,
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                };
            return (await _conn.QueryAsync<MATERIAL>(sql, parameters)).ToList();
        }
        // Lay danh sach hang hoa theo ten hoac ma
        public async Task<List<MATERIAL>> GetMaterialsByNameOrCodeAsync(string keyword)
        {
            var sql = @"
                SELECT *
                FROM MATERIAL
                WHERE Material_Name_VN LIKE '%' + @Keyword + '%'
                   OR Material_Name_EN LIKE '%' + @Keyword + '%'
                   OR Material_Name_JP LIKE '%' + @Keyword + '%'
                   OR Material_Code LIKE '%' + @Keyword + '%' OR @Keyword IS NULL OR @Keyword = '';
            ";
            var parameters = new { Keyword = keyword };
            return (await _conn.QueryAsync<MATERIAL>(sql, parameters)).ToList();
        }
        // danh sach thong tin chung loai
        public async Task<List<dynamic>> GetListMaterial()
        {
            var result = new List<dynamic>();

            var sql = "SELECT DISTINCT Category_VN, Category_EN,Category_JP FROM MATERIAL where Category_VN is not null";
            var groups = await _conn.QueryAsync<dynamic>(sql);
            result.AddRange(groups);

            return result;
        }
        // update danh sách linh kiện
        public async Task<bool> UpdateMaterialAsync(List<MATERIAL> materials)
        {
            if(materials.Count == 0) return false;
            foreach (var material in materials)
            {
                var data = _context.MATERIALs.Where(c => c.Material_Code == material.Material_Code).FirstOrDefault();
                if (data == null) continue;

                data.Category_VN = material.Category_VN;
                data.Category_EN = material.Category_EN;
                data.Category_JP = material.Category_JP;
                data.Shape = material.Shape;
                data.Material1 = material.Material1;
                data.Composition = material.Composition;
                data.Dimension = material.Dimension;
                data.UsedFor = material.UsedFor;
                data.Purpose = material.Purpose;

            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
