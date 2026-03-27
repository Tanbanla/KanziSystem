using Dapper;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
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
            var result = await _context.MATERIALs
            .FirstOrDefaultAsync(m => m.Material_Code == maHang);
            return result;
        }
        // Tim kiem thong tin hang hoa va phan trang
        public async Task<List<MATERIAL>> SearchAsync(string? MaHang, string? Name, string? NhomHang, int? pageIndex, int? pageSize)
        {
            var sql = "SELECT * FROM MATERIAL WHERE 1=1";
            var parameters = new Dapper.DynamicParameters();

            // Filter by code
            if (!string.IsNullOrWhiteSpace(MaHang))
            {
                sql += " AND Material_Code LIKE '%' + @MaterialCode + '%'";
                parameters.Add("MaterialCode", MaHang);
            }

            // Filter by name (search across multiple name columns)
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sql += " OR (Material_Name_VN LIKE '%' + @MaterialName + '%' OR Material_Name_EN LIKE '%' + @MaterialName + '%' OR Material_Name_JP LIKE '%' + @MaterialName + '%')";
                parameters.Add("MaterialName", Name);
            }

            // Filter by category/type
            if (!string.IsNullOrWhiteSpace(NhomHang))
            {
                sql += " AND (Category_VN LIKE @MaterialType + '%' OR Category_EN LIKE '%' + @MaterialType + '%' OR Category_JP LIKE '%' + @MaterialType + '%')";
                parameters.Add("MaterialType", NhomHang);
            }

            // Pagination
            if (pageSize.HasValue && pageSize.Value > 0)
            {
                var page = pageIndex.GetValueOrDefault(1);
                var offset = (page - 1) * pageSize.Value;
                sql += " ORDER BY Material_Code OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", offset);
                parameters.Add("PageSize", pageSize.Value);
            }
            else
            {
                sql += " ORDER BY Material_Code";
            }

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
                data.Material = material.Material;
                data.Composition = material.Composition;
                data.Dimension = material.Dimension;
                data.UsedFor = material.UsedFor;
                data.Purpose = material.Purpose;

            }
            await _context.SaveChangesAsync();
            return true;
        }
        // check mã linh kiện 
        public async Task<bool> CheckMaHangExistsAsync(string codeMaterial)
        {
            var result = await _context.MATERIALs.AnyAsync(m => m.Material_Code == codeMaterial);
            //
            var checkTableConfirm = await _context.BaoGia_Confirm_Name_Quotations.AnyAsync(t => t.VCHR_MaHangNoiBo == codeMaterial);

            if (result || checkTableConfirm)
            {
                return true;
            }
            return false;
        }
        // Insert 
        public async Task<bool> InsertMaterial(MATERIAL mt)
        {
            if (mt == null) return false;
            await _context.MATERIALs.AddAsync(mt);
            await _context.SaveChangesAsync();
            return true;
        }

        //insert nhiều
        public async Task<GenericResponse<bool>> UpdateListThongTin(List<MATERIALDTO> listDTO)
        {
            if (listDTO == null || listDTO.Count == 0)
                return new GenericResponse<bool> { Data = false, Success = false, Message = "Input list is empty." };

            foreach (var dto in listDTO)
            {
                var material = await _context.MATERIALs.FirstOrDefaultAsync(m => m.Material_Code == dto.Material_Code);
                if (material == null)
                    continue;

                material.Category_VN = dto.Category_VN;
                material.Category_EN = dto.Category_EN;
                material.Category_JP = dto.Category_JP;
                material.Shape = dto.Shape;
                material.Material = dto.Material;
                material.Composition = dto.Composition;
                material.Dimension = dto.Dimension;
                material.UsedFor = dto.UsedFor;
                material.Purpose = dto.Purpose;
                // Add other fields as needed from MATERIALDTO
            }

            await _context.SaveChangesAsync();
            return new GenericResponse<bool> { Data = true, Success = true, Message = "Update successful." };
        }
        // Insert nhiều
        public async Task<bool> UpdateListThongTinNoList(List<MATERIAL> listMT)
        {
            if (listMT == null || !listMT.Any())
                return false;

            // Lấy danh sách Material_Code đã tồn tại trong database
            var existingMaterialCodes = await _context.MATERIALs
                .Where(m => listMT.Select(x => x.Material_Code).Contains(m.Material_Code))
                .Select(m => m.Material_Code)
                .ToListAsync();

            // Lọc ra các vật tư chưa tồn tại
            var newMaterials = listMT
                .Where(m => !existingMaterialCodes.Contains(m.Material_Code))
                .ToList();

            if (!newMaterials.Any())
                return false;

            await _context.MATERIALs.AddRangeAsync(newMaterials);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
