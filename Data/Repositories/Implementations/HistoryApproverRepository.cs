using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class HistoryApproverRepository : BaseRepository<BaoGia_History_Approver_of_Quotation, int>, IHistoryApproverRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public HistoryApproverRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
            : base(context, options, configuration)
        {
            _context = context;
        }
        // Lấy thông tin lịch sử phê duyệt báo giá theo mã báo giá
        public async Task<List<BaoGia_History_Approver_of_Quotation>> GetHistoryByQuotationIdAsync(int quotationId)
        {
            try
            {
                var query = _context.BaoGia_History_Approver_of_Quotations.AsQueryable();
                if (quotationId > 0)
                {
                    query = query.Where(x => x.ID_RequestQuote == quotationId);
                }
                return await query
                    .OrderByDescending(x => x.ID_RequestQuote)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHistoryByQuotationIdAsync: {ex.Message}");
                return new List<BaoGia_History_Approver_of_Quotation>();
            }
        }
        // Lấy thông tin lịch sử phê duyệt báo giá theo số đơn
        public async Task<List<BaoGia_History_Approver_of_Quotation>> GetHistoryBySoDonAsync(string soDon)
        {
            try
            {
                var sql = @"SELECT * FROM [COST_MANAGEMENT].[dbo].[BaoGia_History_Approver_of_Quotation] as h
                  left join BaoGia_Request_of_Quotation as  q on h.ID_RequestQuote = q.ID
                  where q.CHR_MaDon = @SoDon";
                var parameter = new Microsoft.Data.SqlClient.SqlParameter("@SoDon", soDon);
                return (await _conn.QueryAsync<BaoGia_History_Approver_of_Quotation>(sql, parameter)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHistoryBySoDonAsync: {ex.Message}");
                return new List<BaoGia_History_Approver_of_Quotation>();
            }
        }
        // Tìm kiếm lịch sử phê duyệt báo giá 
        public async Task<List<BaoGia_History_Approver_of_Quotation>> SearchHistoryAsync(int? quotationId, string? soDon, int? buoc, DateTime? fromDate, DateTime? toDate, string? approverName)
        {
            try
            {
                var query = _context.BaoGia_History_Approver_of_Quotations.AsQueryable();
                if (quotationId.HasValue)
                {
                    query = query.Where(x => x.ID_RequestQuote == quotationId.Value);
                }
                //if (!string.IsNullOrEmpty(soDon))
                //{
                //    query = query.Where(x => x.CHR_SoDon == soDon);
                //}
                if (buoc.HasValue)
                {
                    query = query.Where(x => x.ID_BaoGiaStep == buoc.Value);
                }
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.DTM_UserApprover >= fromDate.Value);
                }
                if (!string.IsNullOrEmpty(approverName))
                {
                    query = query.Where(x => x.CHR_UserApprover.Contains(approverName));
                }
                return await query
                    .OrderByDescending(x => x.DTM_UserSendApprover)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchHistoryAsync: {ex.Message}");
                return new List<BaoGia_History_Approver_of_Quotation>();
            }
        }
        // Thêm mới lịch sử phê duyệt báo giá
        public async Task<bool> AddHistoryAsync(BaoGia_History_Approver_of_Quotation history)
        {
            try
            {
                await _context.BaoGia_History_Approver_of_Quotations.AddAsync(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddHistoryAsync: {ex.Message}");
                return false;
            }
        }
        // Thêm mới danh sách lịch sử phê duyệt báo giá
        public async Task<bool> AddHistoryListAsync(List<BaoGia_History_Approver_of_Quotation> historyList)
        {
            try
            {
                await _context.BaoGia_History_Approver_of_Quotations.AddRangeAsync(historyList);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddHistoryListAsync: {ex.Message}");
                return false;
            }
        }
        // Sửa thông tin lịch sử phê duyệt báo giá
        public async Task<bool> UpdateHistoryAsync(BaoGia_History_Approver_of_Quotation history)
        {
            try
            {
                _context.BaoGia_History_Approver_of_Quotations.Update(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateHistoryAsync: {ex.Message}");
                return false;
            }
        }
        // Lấy danh sách phê duyệt của người dùng
        public async Task<List<BaoGia_Request_of_Quotation>> GetListApprover(string adid, string? soDon, string? maHang, string? section, string? statusApprover)
        {
            try
            {
                var sql = @"SELECT DISTINCT r.* FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] as r 
                  left join BaoGia_Master_Approver_Send_Mail as s  on r.ID_StepBaoGia = s.ID_BaoGiaStep and CHR_SectionCode = s.CHR_CodeSection
                  where CHR_UserAdid = @Adid and
                   (@SoDon is null or CHR_MaDon like '%' + @SoDon + '%' or @SoDon = '' ) and 
                   (@MaHang is null or CHR_MaHangNoiBo like '%' + @MaHang + '%' or @MaHang = '' ) and 
                   (@Section is null or CHR_SectionCode like '%' + @Section + '%' or @Section = '' ) and 
                   (@Status is null or ID_StepBaoGia = @Status or @Status = '' )
                  ";
                var parameter = new
                {
                    Adid = adid,
                    SoDon = soDon,
                    MaHang = maHang,
                    Section = section,
                    Status = statusApprover
                };
                var re = (await _conn.QueryAsync<BaoGia_Request_of_Quotation>(sql, parameter)).ToList();
                return re;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetListApprover: {ex.Message}");
                return new List<BaoGia_Request_of_Quotation>();
            }
        }
    }
}
