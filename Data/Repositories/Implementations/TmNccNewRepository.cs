using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using Dapper;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class TmNccNewRepository: BaseRepository<IM_NCC_NEW, int>, ITmNccNewRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public TmNccNewRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration) { 
            _context = context;
        }

        //Lấy danh sách nhà cung cấp  ALL
        public async Task<List<IM_NCC_NEW>> GetAllNccNew()
        {
            try
            {
                var sql = "SELECT * FROM IM_NCC_NEW where Xoa <> 1";
                return (await _conn.QueryAsync<IM_NCC_NEW>(sql)).ToList();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in GetAllNccNew: {ex.Message}");
                return new List<IM_NCC_NEW>();
            }

        }
        // Lấy nhà cung cấp theo mã nhà cung cấp
        public async Task<IM_NCC_NEW> GetNccNewByCode(string MaNCC)
        {
            var sql = "SELECT * FROM IM_NCC_NEW WHERE MA_NCC = @MaNCC";
            var parameters = new { MaNCC };
            return await _conn.QueryFirstOrDefaultAsync<IM_NCC_NEW>(sql, parameters);
        }
        // Lây thông tin nhà cung cấp phân trang
        public async Task<List<IM_NCC_NEW>> GetNccNewPaging(string? CodeNcc, string? NameNcc, int pageIndex, int pageSize)
        {
            var sql = @"
                SELECT *
                FROM IM_NCC_NEW
                WHERE (@CodeNcc IS NULL OR @CodeNcc = '' OR Ma LIKE '%' + @CodeNcc + '%') 
                AND (@NameNcc IS NULL OR @NameNcc = '' OR Ten LIKE '%' + @NameNcc + '%')
                AND (Xoa is null OR Xoa = 0)
                ORDER BY Ma
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";
            var parameters = new
            {
                CodeNcc = string.IsNullOrEmpty(CodeNcc) ? null : CodeNcc,
                NameNcc = string.IsNullOrEmpty(NameNcc) ? null : NameNcc,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            };
            return (await _conn.QueryAsync<IM_NCC_NEW>(sql, parameters)).ToList();
        }
        // Xoa thong tin nha cung cap
        public async Task<bool> DeleteNccNewByCode(int id, string userAction)
        {
            var sql = "UPDATE IM_NCC_NEW set Xoa = 1, nguoi_cap_nhat = @UserAction WHERE Ncc_Id = @Id";
            var parameters = new { Id = id, UserAction = userAction };
            var rowsAffected = await _conn.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        // Thêm thông tin nhà cung cấp 
        public async Task<bool> AddNccNew(IM_NCC_NEW nccNew)
        {
            await _context.IM_NCC_NEWs.AddAsync(nccNew);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        // Update thong tin nha cung cap
        public async Task<bool> UpdateNccNew(IM_NCC_NEW nccNew)
        {
            var result = _context.IM_NCC_NEWs.Where(c=> c.Ncc_Id == nccNew.Ncc_Id).FirstOrDefault();
            if (result == null)
            {
                return false;
            }
            result.Ma = nccNew.Ma;
            result.Ten = nccNew.Ten;
            result.Diachi = nccNew.Diachi;
            result.Sodienthoai = nccNew.Sodienthoai;
            result.Fax = nccNew.Fax;
            result.Khuvuc = nccNew.Khuvuc;
            result.Ghichu = nccNew.Ghichu;  
            result.Hinhthucmotk = nccNew.Hinhthucmotk;
            result.Dieukienthanhtoan = nccNew.Dieukienthanhtoan;
            result.Masothue = nccNew.Masothue;
            result.Nhanvienkinhdoand = nccNew.Nhanvienkinhdoand;
            result.Nhanvienketoan = nccNew.Nhanvienketoan;
            result.Canphaixacnhanlamthutuchaiquan = nccNew.Canphaixacnhanlamthutuchaiquan;
            result.Xoa = nccNew.Xoa;
            result.nhom = nccNew.nhom;
            result.nguoi_cap_nhat = nccNew.nguoi_cap_nhat;
            await _context.SaveChangesAsync();
            return true;
        }
        // thêm danh sách nhà cung cấp
        public async Task<bool> AddListNccNew(List<IM_NCC_NEW> listNccNew)
        {
            await _context.IM_NCC_NEWs.AddRangeAsync(listNccNew);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
