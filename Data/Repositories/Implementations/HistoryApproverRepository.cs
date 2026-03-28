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
            var query = _context.BaoGia_History_Approver_of_Quotations.AsQueryable();
            if (quotationId > 0)
            {
                query = query.Where(x => x.ID_RequestQuote == quotationId);
            }
            return await query
                .OrderByDescending(x => x.ID_RequestQuote)
                .ToListAsync();
        }
        // Lấy thông tin lịch sử phê duyệt báo giá theo số đơn
        public async Task<List<BaoGia_History_Approver_of_Quotation>> GetHistoryBySoDonAsync(string soDon)
        {
            var sql = @"SELECT * FROM [BaoGia_History_Approver_of_Quotation] as h
                left join BaoGia_Request_of_Quotation as  q on h.ID_RequestQuote = q.ID
                where q.CHR_MaDon = @SoDon";
            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@SoDon", soDon);
            return (await _conn.QueryAsync<BaoGia_History_Approver_of_Quotation>(sql, parameter)).ToList();
        }
        // Tìm kiếm lịch sử phê duyệt báo giá 
        public async Task<List<BaoGia_History_Approver_of_Quotation>> SearchHistoryAsync(int? quotationId, string? soDon, int? buoc, DateTime? fromDate, DateTime? toDate, string? approverName)
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
        // Thêm mới lịch sử phê duyệt báo giá
        public async Task<bool> AddHistoryAsync(BaoGia_History_Approver_of_Quotation history)
        {
            await _context.BaoGia_History_Approver_of_Quotations.AddAsync(history);
            await _context.SaveChangesAsync();
            return true;
        }
        // Thêm mới danh sách lịch sử phê duyệt báo giá
        public async Task<bool> AddHistoryListAsync(List<BaoGia_History_Approver_of_Quotation> historyList)
        {
            await _context.BaoGia_History_Approver_of_Quotations.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return true;
        }
        // Sửa thông tin lịch sử phê duyệt báo giá
        public async Task<bool> UpdateHistoryAsync(BaoGia_History_Approver_of_Quotation history)
        {
            _context.BaoGia_History_Approver_of_Quotations.Update(history);
            await _context.SaveChangesAsync();
            return true;
        }
        // Lấy danh sách phê duyệt của người dùng
        public async Task<List<BaoGia_Request_of_Quotation>> GetListApprover(string adid, string? soDon, string? maHang, string? section, string? statusApprover)
        {
            var sql = @"SELECT DISTINCT r.* FROM [BaoGia_Request_of_Quotation] as r
                LEFT JOIN BaoGia_Master_Approver_Send_Mail as m ON m.ID_BaoGiaStep = r.ID_StepBaoGia
                WHERE (r.CHR_UserApproval = @Adid OR (r.ID_StepBaoGia = 4 AND m.CHR_UserAdid = @Adid)) 
                  AND r.ID_StepBaoGia < 6 
                  AND r.ID_StepBaoGia > 1
                ";
            // Thêm điều kiện tìm theo số đơn
            if (!string.IsNullOrEmpty(soDon))
            {
                sql += @" AND r.CHR_MaDon LIKE '%' + @SoDon + '%'";
            }

            // Thêm điều kiện tìm theo mã hàng
            if (!string.IsNullOrEmpty(maHang))
            {
                sql += @" AND r.CHR_MaHangNoiBo LIKE '%' + @MaHang + '%'";
            }

            // Thêm điều kiện tìm theo section
            if (!string.IsNullOrEmpty(section))
            {
                sql += @" AND r.CHR_SectionCode LIKE '%' + @Section + '%'";
            }

            // Thêm điều kiện tìm theo status
            if (!string.IsNullOrEmpty(statusApprover) && int.TryParse(statusApprover, out int status))
            {
                sql += @" AND r.ID_StepBaoGia = @Status";
            }
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
    }
}
