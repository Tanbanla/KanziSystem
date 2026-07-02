using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

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
        public async Task<List<MATERIAL>> SearchAsync(
            string? MaHang,
            string? Name,
            string? NhomHang,
            int? pageIndex,
            int? pageSize)
        {
            var sql = new StringBuilder("SELECT * FROM MATERIAL");
            var where = new List<string>();
            var parameters = new DynamicParameters();

            // ===== FILTER =====
            if (!string.IsNullOrWhiteSpace(MaHang))
            {
                where.Add("Material_Code LIKE '%' + @MaterialCode + '%'");
                parameters.Add("MaterialCode", MaHang);
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                where.Add(@"(
                    Material_Name_VN LIKE '%' + @MaterialName + '%' 
                    OR Material_Name_EN LIKE '%' + @MaterialName + '%' 
                    OR Material_Name_JP LIKE '%' + @MaterialName + '%'
                )");
                parameters.Add("MaterialName", Name);
            }

            if (!string.IsNullOrWhiteSpace(NhomHang))
            {
                where.Add(@"(
                    Category_VN LIKE '%' + @MaterialType + '%' 
                    OR Category_EN LIKE '%' + @MaterialType + '%' 
                    OR Category_JP LIKE '%' + @MaterialType + '%'
                )");
                parameters.Add("MaterialType", NhomHang);
            }

            if (where.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", where));
            }

            if (pageSize.HasValue && pageSize.Value > 0)
            {
                var page = pageIndex.GetValueOrDefault(1);
                var offset = (page - 1) * pageSize.Value;

                sql.Append(" ORDER BY Material_Code OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                parameters.Add("Offset", offset);
                parameters.Add("PageSize", pageSize.Value);
            }
            else
            {
                sql.Append(" ORDER BY Material_Code");
            }
            return (await _conn.QueryAsync<MATERIAL>(sql.ToString(), parameters)).ToList();
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
                material.Material1 = dto.Material;
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
        // Lay ma hang lon nhat hien tai
        public async Task<string> MaterialCodeLater(string type)
        {
            var result = await _context.MATERIALs
                .Where(m => m.Material_Code != null && m.Material_Code.StartsWith(type))
                .OrderByDescending(m => m.Material_Code)
                .Select(m => m.Material_Code)
                .FirstOrDefaultAsync();

            return result ?? string.Empty;
        }
        // check ma hang
        public async Task<string> CheckMaterialCode(string keyword, string category)
        {
            var result = await _context.MATERIALs
             .Where(m => m.Material_Code != null
             && (m.Material_Name_VN == keyword || m.Code_Suppiler == keyword || m.Material_Name_EN == keyword)
             && m.Category_VN.ToLower().Contains(category.ToLower()))
             .OrderByDescending(m => m.Material_Code)
             .Select(m => m.Material_Code)
             .FirstOrDefaultAsync();

            return result ?? string.Empty;
        }
        // Search date by Material View
        public async Task<ListRequest<MATERIAL>> SearchDateByMaterialViewAsync(SearchMaterialVM search)
        {
            var sql = new StringBuilder();
            var where = new List<string>();
            var parameters = new DynamicParameters();

            sql.Append("SELECT * FROM MATERIAL");

            // ===== FILTER =====
            if (!string.IsNullOrWhiteSpace(search.MaterialCode))
            {
                where.Add("Material_Code LIKE '%' + @MaterialCode + '%'");
                parameters.Add("MaterialCode", search.MaterialCode);
            }

            if (!string.IsNullOrWhiteSpace(search.MaterialName))
            {
                where.Add(@"(
                    Material_Name_VN LIKE '%' + @MaterialName + '%' 
                    OR Material_Name_EN LIKE '%' + @MaterialName + '%' 
                    OR Material_Name_JP LIKE '%' + @MaterialName + '%' or Material_Code LIKE '%' + @MaterialName + '%'
                )");
                parameters.Add("MaterialName", search.MaterialName);
            }

            if (!string.IsNullOrWhiteSpace(search.MaterialCatergory))
            {
                where.Add(@"(
                    Category_VN LIKE '%' + @MaterialType + '%'
                    OR Category_EN LIKE '%' + @MaterialType + '%'
                    OR Category_JP LIKE '%' + @MaterialType + '%'
                )");
                parameters.Add("MaterialType", search.MaterialCatergory);
            }

            if (!string.IsNullOrWhiteSpace(search.MaterialGroup))
            {
                where.Add("Group_Code = @MaterialGroup");
                parameters.Add("MaterialGroup", search.MaterialGroup);
            }

            if (where.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", where));
            }

            var countSql = $"SELECT COUNT(1) FROM MATERIAL {(where.Any() ? "WHERE " + string.Join(" AND ", where) : "")}";
            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql, parameters);


            if (search.pageSize.HasValue && search.pageSize.Value > 0)
            {
                var page = search.pageIndex.GetValueOrDefault(1);
                var offset = (page - 1) * search.pageSize.Value;

                sql.Append(" ORDER BY Material_Code OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                parameters.Add("Offset", offset);
                parameters.Add("PageSize", search.pageSize.Value);
            }
            else
            {
                sql.Append(" ORDER BY Material_Code");
            }

            var data = (await _conn.QueryAsync<MATERIAL>(sql.ToString(), parameters)).ToList();

            return new ListRequest<MATERIAL>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        // Delete Material
        public async Task<bool> DeleteMaterialAsync(string codeMaterial)
        {
            var material = await _context.MATERIALs.FirstOrDefaultAsync(m => m.Material_Code == codeMaterial);
            if (material == null)
                return false;
            _context.MATERIALs.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }
        // Update Material
        public async Task<bool> UpdateMaterialAsync(MATERIAL mt)
        {
            var material = await _context.MATERIALs.FirstOrDefaultAsync(m => m.Material_Code == mt.Material_Code);
            if (material == null)
                return false;

            // Update properties
            material.Material_Code = mt.Material_Code;
            material.Material_Name_VN = mt.Material_Name_VN;
            material.Material_Name_EN = mt.Material_Name_EN;
            material.Material_Name_JP = mt.Material_Name_JP;
            material.Category_VN = mt.Category_VN;
            material.Category_EN = mt.Category_EN;
            material.Category_JP = mt.Category_JP;
            material.Group_Code = mt.Group_Code;
            material.Shape = mt.Shape;
            material.Unit = mt.Unit;
            material.Material1 = mt.Material1;
            material.Composition = mt.Composition;
            material.Dimension = mt.Dimension;
            material.UsedFor = mt.UsedFor;
            material.Purpose = mt.Purpose;
            material.Code_Suppiler = mt.Code_Suppiler;

            _context.MATERIALs.Update(material);
            await _context.SaveChangesAsync();
            return true;
        }   
    }
}
