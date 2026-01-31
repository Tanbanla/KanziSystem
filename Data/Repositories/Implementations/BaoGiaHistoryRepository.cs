using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaHistoryRepository : BaseRepository<BaoGia_History_Request_of_Quotation, int>, IBaoGiaHistoryRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaHistoryRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy lịch sử báo giá theo ID_RequestQuote
        public async Task<List<BaoGia_History_Request_of_Quotation>> GetByRequestQuoteIdAsync(int idRequestQuote)
        {
            try
            {
                var result = await _context.BaoGia_History_Request_of_Quotations
                .Where(h => h.ID_RequestQuote == idRequestQuote)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByRequestQuoteIdAsync: {ex.Message}");
                return new List<BaoGia_History_Request_of_Quotation>();
            }
        }
        // Tìm kiếm danh sách thông tin lịch sử báo giá theo số đơn
        public async Task<List<BaoGia_History_Request_of_Quotation>> SearchBySoDonAsync(string soDon)
        {
            try
            {
                var result = await _context.BaoGia_History_Request_of_Quotations
                .Where(h => h.CHR_MaDon == soDon)
                .OrderBy(h => h.ID_RequestQuote)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchBySoDonAsync: {ex.Message}");
                return new List<BaoGia_History_Request_of_Quotation>();
            }
        }
        // Tìm kiếm lịch sử báo giá và phân trang
        public async Task<List<BaoGia_History_Request_of_Quotation>> SearchAsync(int? idRequestQuote, string? soDon, int? pageIndex, int? pageSize)
        {
            try
            {
                var sql = @"
                    SELECT *
                    FROM BaoGia_History_Request_of_Quotation
                    WHERE (@soDon IS NULL OR @soDon = '' OR CHR_MaDon LIKE '%' + @soDon + '%') 
                    AND (ID_RequestQuote = @IdRequestQuote OR @IdRequestQuote IS NULL OR @IdRequestQuote = '')
                    ";
                if (pageSize > 0 && pageIndex > 0)
                {
                    sql += @"
                    ORDER BY ID_RequestQuote
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                    ";
                }
                else
                {
                    sql += @"
                    ORDER BY ID_RequestQuote
                    ";
                }

                var parameters = new
                {
                    soDon = string.IsNullOrEmpty(soDon) ? null : soDon,
                    IdRequestQuote = idRequestQuote,
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                };
                return (await _conn.QueryAsync<BaoGia_History_Request_of_Quotation>(sql, parameters)).ToList();

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in SearchAsync: {ex.Message}");
                return new List<BaoGia_History_Request_of_Quotation>();
            }
        }
        // Insert thông tin lịch sử báo giá
        public async Task<bool> InsertHistoryAsync(BaoGia_History_Request_of_Quotation history)
        {
            try
            {
                await _context.BaoGia_History_Request_of_Quotations.AddAsync(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertHistoryAsync: {ex.Message}");
                return false;
            }
        }
        // Insert danh sách lịch sử báo giá
        public async Task<bool> InsertHistoryListAsync(List<BaoGia_History_Request_of_Quotation> historyList)
        {
            try
            {
                await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertHistoryListAsync: {ex.Message}");
                return false;
            }
        }
        // Sửa thông tin lịch sử báo giá
        public async Task<bool> UpdateHistoryAsync(BaoGia_History_Request_of_Quotation history)
        {
            try
            {
                _context.BaoGia_History_Request_of_Quotations.Update(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateHistoryAsync: {ex.Message}");
                return false;
            }
        }
    }
}
