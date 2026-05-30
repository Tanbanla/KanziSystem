using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
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
            var result = await _context.BaoGia_History_Request_of_Quotations
            .Where(h => h.ID_RequestQuote == idRequestQuote)
            .OrderBy(h => h.ID)
            .ToListAsync();
            return result;
        }
        // Tìm kiếm danh sách thông tin lịch sử báo giá theo số đơn
        public async Task<List<BaoGia_History_Request_of_Quotation>> SearchBySoDonAsync(string soDon)
        {
            var result = await _context.BaoGia_History_Request_of_Quotations
            .Where(h => h.CHR_MaDon == soDon)
            .OrderBy(h => h.ID_RequestQuote)
            .ThenBy(h => h.ID)
            .ToListAsync();
            return result;
        }
        // Tìm kiếm lịch sử báo giá và phân trang
        public async Task<List<BaoGia_History_Request_of_Quotation>> SearchAsync(int? idRequestQuote, string? soDon, int? pageIndex, int? pageSize)
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
        // Insert thông tin lịch sử báo giá
        public async Task<bool> InsertHistoryAsync(BaoGia_History_Request_of_Quotation history)
        {
            await _context.BaoGia_History_Request_of_Quotations.AddAsync(history);
            await _context.SaveChangesAsync();
            return true;
        }
        // Insert danh sách lịch sử báo giá
        public async Task<bool> InsertHistoryListAsync(List<BaoGia_History_Request_of_Quotation> historyList)
        {
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return true;
        }
        // Sửa thông tin lịch sử báo giá
        public async Task<bool> UpdateHistoryAsync(BaoGia_History_Request_of_Quotation history)
        {
            _context.BaoGia_History_Request_of_Quotations.Update(history);
            await _context.SaveChangesAsync();
            return true;
        }
        // Lấy lý do trả lại đơn báo giá
        public async Task<string> GetReturnReasonAsync(int idRequestQuote)
        {
            var result = await _context.BaoGia_History_Request_of_Quotations
                .Where(h => h.ID_RequestQuote == idRequestQuote
                         && h.CHR_ActionType != null
                         && h.CHR_ActionType.Contains("RETURN"))
                .OrderByDescending(h => h.ID)
                .Select(h => h.NVCHR_LyDo)
                .FirstOrDefaultAsync();
            return result ?? string.Empty;
        }
        public async Task<List<ReasonQuotition>> GetReasonsAsync(List<dynamic> ids)
        {
            var results = await _context.BaoGia_History_Request_of_Quotations
                .Where(h => ids.Contains(h.ID_RequestQuote) && h.CHR_ActionType != null && h.CHR_ActionType.Contains("RETURN"))
                .GroupBy(h => h.ID_RequestQuote)
                .Select(g => new ReasonQuotition
                {
                    Id = g.Key,
                    Reason = g.OrderByDescending(h => h.ID).FirstOrDefault().NVCHR_LyDo
                })
                .ToListAsync();
            return results;
        }
        public async Task<ListRequest<dynamic>> SearchHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
            string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var sql = @"
                SELECT 
                    q.CHR_MaDon,
                    q.CHR_CreateBy,
                    MAX(q.DTM_CreateDate) AS DTM_CreateDate,
                        CASE 
                        WHEN COUNT(CASE WHEN q.BIT_LayBaoGia = 1 THEN 1 END) > 0 
                        THEN MAX(CASE WHEN q.BIT_LayBaoGia = 1 THEN q.ID_Status END)
                        ELSE MIN(q.ID_Status)
                    END AS ID_Status,
                    COUNT(DISTINCT q.ID) AS TongSoDon,
                    COUNT(DISTINCT CASE 
                        WHEN q.ID_StepBaoGia = 7 AND q.BIT_LayBaoGia = 1 
                        THEN q.CHR_MaNCC 
                        ELSE NULL 
                    END) AS SupperlierSened,
                    COUNT(DISTINCT CASE 
                        WHEN q.BIT_LayBaoGia = 1 
                        THEN q.CHR_MaNCC 
                        ELSE NULL 
                    END) AS SupperlierSum,
                    STUFF((
                        SELECT DISTINCT ', ' + q2.CHR_MaNCC
                        FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] q2
                        WHERE q2.CHR_MaDon = q.CHR_MaDon
                        FOR XML PATH('')
                    ), 1, 2, '') AS Suppliers
                FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] AS q
                INNER JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] AS s 
                    ON q.CHR_SectionCode = s.CHR_CodeSection
                WHERE (@MaDon IS NULL OR CHR_MaDon = @MaDon)
                  AND (@MaNcc IS NULL OR CHR_MaNCC LIKE '%' + @MaNcc + '%')
                  AND (@ChungLoai IS NULL OR NVCHR_ChungLoai LIKE '%' + @ChungLoai + '%')
                  AND (@Section IS NULL OR CHR_SectionCode LIKE '%' + @Section + '%')
                  AND (@NguoiYeuCau IS NULL OR q.CHR_CreateBy LIKE '%' + @NguoiYeuCau + '%')
                  AND (@MaHang IS NULL OR CHR_MaHangNoiBo LIKE '%' + @MaHang + '%')
                  AND (@Step IS NULL OR ID_StepBaoGia = @Step)
                  AND (@Date IS NULL OR CAST(DTM_CreateDate AS DATE) = CAST(@Date AS DATE))
                  AND ( s.CHR_UserAdid = @Adid)
            ";

            var statusSql = "1=1";
            switch (status)
            {
                case "RETURN":
                    statusSql = "ID_Status LIKE '%RETURN%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DONE":
                    statusSql = "ID_Status = 'DONE' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "APPROVAL":
                    statusSql = "ID_Status LIKE 'APPROVAL%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "WAIT":
                    statusSql = "ID_Status LIKE '%WAIT%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DELETE":
                    statusSql = "ID_Status LIKE 'DELETE'";
                    break;
                default:
                    statusSql = "ID_Status NOT LIKE 'DELETE'";
                    break;
            }

            // append status filter into WHERE before GROUP BY
            sql += " AND (" + statusSql + ")";

            // add grouping
            sql += @"
                GROUP BY q.CHR_MaDon, q.CHR_CreateBy
            ";

            // Build count query first
            var countSql = @"
                SELECT COUNT(distinct q.CHR_MaDon)
                FROM BaoGia_Request_of_Quotation as q
				  inner join [BaoGia_Master_Approver_Send_Mail] as s 
				on q.CHR_SectionCode = s.CHR_CodeSection
                WHERE (@MaDon IS NULL OR CHR_MaDon = @MaDon)
                  AND (@MaNcc IS NULL OR CHR_MaNCC LIKE '%' + @MaNcc + '%')
                  AND (@ChungLoai IS NULL OR NVCHR_ChungLoai LIKE '%' + @ChungLoai + '%')
                  AND (@Section IS NULL OR CHR_SectionCode LIKE '%' + @Section + '%')
                  AND (@NguoiYeuCau IS NULL OR q.CHR_CreateBy LIKE '%' + @NguoiYeuCau + '%')
                  AND (@MaHang IS NULL OR CHR_MaHangNoiBo LIKE '%' + @MaHang + '%')
                  AND (@Step IS NULL OR ID_StepBaoGia = @Step)
                  AND (@Date IS NULL OR CAST(DTM_CreateDate AS DATE) = CAST(@Date AS DATE))
                  AND ( s.CHR_UserAdid = @Adid)
                  AND (" + statusSql + ")";

            // Add ordering and paging to main query 
            if (pageSize > 0 && pageIndex > 0)
            {
                sql += @"
                    ORDER BY DTM_CreateDate DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";
            }
            else
            {
                sql += @"
                    ORDER BY DTM_CreateDate DESC
                ";
            }

            var parameters = new
            {
                MaDon = string.IsNullOrEmpty(MaDon) ? null : MaDon,
                MaNcc = string.IsNullOrEmpty(MaNcc) ? null : MaNcc,
                Section = string.IsNullOrEmpty(Section) ? null : Section,
                NguoiYeuCau = string.IsNullOrEmpty(nguoiYeuCau) ? null : nguoiYeuCau,
                MaHang = string.IsNullOrEmpty(MaHang) ? null : MaHang,
                Step = step,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize,
                Date = date,
                ChungLoai = string.IsNullOrEmpty(chungLoai) ? null : chungLoai,
                Adid = user
            };

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            var totalCount = await _conn.ExecuteScalarAsync<long>(countSql, parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        // Lấy thông tin phê duyệt báo giá của các đơn hàng
        public async Task<List<dynamic>> GetHistoryApprover(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai)
        {
            var sql = @"
                WITH Ranked AS (
                    SELECT h.*, q.ID_Status,
                        ROW_NUMBER() OVER (PARTITION BY h.CHR_MaDon, h.CHR_ActionType ORDER BY h.CHR_Updatedate DESC) AS rn
                    FROM [COST_MANAGEMENT].[dbo].[BaoGia_History_Request_of_Quotation] h
                    INNER JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] q ON q.ID = h.ID_RequestQuote
                    INNER JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] s ON q.CHR_SectionCode = s.CHR_CodeSection
                    WHERE (@MaDon IS NULL OR q.CHR_MaDon = @MaDon)
                      AND (@MaNcc IS NULL OR q.CHR_MaNCC LIKE '%' + @MaNcc + '%')
                      AND (@ChungLoai IS NULL OR q.NVCHR_ChungLoai LIKE '%' + @ChungLoai + '%')
                      AND (@Section IS NULL OR q.CHR_SectionCode LIKE '%' + @Section + '%')
                      AND (@NguoiYeuCau IS NULL OR q.CHR_CreateBy LIKE '%' + @NguoiYeuCau + '%')
                      AND (@MaHang IS NULL OR q.CHR_MaHangNoiBo LIKE '%' + @MaHang + '%')
                      AND (@Step IS NULL OR q.ID_StepBaoGia = @Step)
                      AND (s.CHR_UserAdid = @Adid)
                )
                SELECT 
                    CHR_MaDon AS maDon,
                    ID_RequestQuote,
                    MAX(CASE WHEN CHR_ActionType = 'INSERT' THEN CHR_UpdateBy END) AS userInsert,
                    MAX(CASE WHEN CHR_ActionType = 'INSERT' THEN CHR_Updatedate END) AS timeInsert,
                    MAX(CASE WHEN CHR_ActionType = 'QLSC' THEN NVCHR_UpdateName END) AS userChief,
                    MAX(CASE WHEN CHR_ActionType = 'QLSC' THEN CHR_Updatedate END) AS timeChief,
                    MAX(CASE WHEN CHR_ActionType = 'QLTC' THEN NVCHR_UpdateName END) AS userSection,
                    MAX(CASE WHEN CHR_ActionType = 'QLTC' THEN CHR_Updatedate END) AS timeSection,
                    MAX(CASE WHEN CHR_ActionType = 'PIC' THEN NVCHR_UpdateName END) AS userPIC,
                    MAX(CASE WHEN CHR_ActionType = 'PIC' THEN CHR_Updatedate END) AS timePIC,
                    MAX(CASE WHEN CHR_ActionType = 'QLSC_1' THEN NVCHR_UpdateName END) AS userPur,
                    MAX(CASE WHEN CHR_ActionType = 'QLSC_1' THEN CHR_Updatedate END) AS timePur
                FROM Ranked
            ";

            var statusSql = "1=1";
            switch (status)
            {
                case "RETURN":
                    statusSql = "ID_Status LIKE '%RETURN%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DONE":
                    statusSql = "ID_Status = 'DONE' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "APPROVAL":
                    statusSql = "ID_Status LIKE 'APPROVAL%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "WAIT":
                    statusSql = "ID_Status LIKE '%WAIT%' AND ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DELETE":
                    statusSql = "ID_Status LIKE 'DELETE'";
                    break;
                default:
                    statusSql = "ID_Status NOT LIKE 'DELETE'";
                    break;
            }

            sql += " WHERE (" + statusSql + ")";
            sql += @"
                GROUP BY CHR_MaDon, ID_RequestQuote
                ORDER BY maDon DESC
            ";

            var parameters = new
            {
                MaDon = string.IsNullOrEmpty(MaDon) ? null : MaDon,
                MaNcc = string.IsNullOrEmpty(MaNcc) ? null : MaNcc,
                Section = string.IsNullOrEmpty(Section) ? null : Section,
                NguoiYeuCau = string.IsNullOrEmpty(nguoiYeuCau) ? null : nguoiYeuCau,
                MaHang = string.IsNullOrEmpty(MaHang) ? null : MaHang,
                Step = step,
                ChungLoai = string.IsNullOrEmpty(chungLoai) ? null : chungLoai,
                Adid = user
            };

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return data;
        }
        // Lấy thông tin lịch sử báo giá theo mã hàng nội bộ và số đơn
        public async Task<List<dynamic>> GetHistoryByMaterialCode(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, string? chungLoai)
        {
            var sql = @"
                WITH CTE_MaxStep AS (
                    SELECT 
                        q.*,
                        d.BIT_Select,
                        d.NVCHR_ReasonPick,
                        d.NVCHR_File,
                        ROW_NUMBER() OVER (
                            PARTITION BY q.CHR_MaDon, q.CHR_MaHangNoiBo
                            ORDER BY q.ID_StepBaoGia DESC
                        ) AS rn
                    FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] q
                    INNER JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] s
                        ON q.CHR_SectionCode = s.CHR_CodeSection
                    LEFT JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Detail_of_Quotation] d
                        ON q.ID = d.ID_RequestQuote
                    WHERE (@MaDon IS NULL OR q.CHR_MaDon = @MaDon)
                      AND (@MaNcc IS NULL OR q.CHR_MaNCC LIKE '%' + @MaNcc + '%')
                      AND (@ChungLoai IS NULL OR q.NVCHR_ChungLoai LIKE '%' + @ChungLoai + '%')
                      AND (@Section IS NULL OR q.CHR_SectionCode LIKE '%' + @Section + '%')
                      AND (@NguoiYeuCau IS NULL OR q.CHR_CreateBy LIKE '%' + @NguoiYeuCau + '%')
                      AND (@MaHang IS NULL OR q.CHR_MaHangNoiBo LIKE '%' + @MaHang + '%')
                      AND (@Step IS NULL OR q.ID_StepBaoGia = @Step)
                      AND (s.CHR_UserAdid = @Adid)
            ";

            var statusSql = "1=1";
            switch (status)
            {
                case "RETURN":
                    statusSql = "q.ID_Status LIKE '%RETURN%' AND q.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DONE":
                    statusSql = "q.ID_Status = 'DONE' AND q.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "APPROVAL":
                    statusSql = "q.ID_Status LIKE 'APPROVAL%' AND q.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "WAIT":
                    statusSql = "q.ID_Status LIKE '%WAIT%' AND q.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DELETE":
                    statusSql = "q.ID_Status LIKE 'DELETE'";
                    break;
                default:
                    statusSql = "q.ID_Status NOT LIKE 'DELETE'";
                    break;
            }

            sql += " AND (" + statusSql + ")";
            sql += @"
                )
                SELECT 
                    CHR_MaDon,
                    CHR_MaHangNoiBo,
                    BIT_IsTemplate,
                    BIT_LayBaoGia,
                    CHR_CreateBy,
                    CHR_Gap,
                    CHR_MaHangNCC,
                    CHR_MaNCC,
                    CHR_MaThietBi,
                    CHR_NameEN,
                    CHR_Phanloai,
                    CHR_SectionCode,
                    CHR_SectionName,
                    DTM_Deadline,
                    DTM_KyHan,
                    DTM_NgayMuonNhan,
                    ID_Status,
                    NVCHR_ChungLoai,
                    NVCHR_DonVi,
                    NVCHR_FileThietKe,
                    NVCHR_LyDo,
                    NVCHR_NameVN,
                    NVCHR_NhaSanXuat,
                    NVCHR_TenNCC,
                    INT_SoLuong,
                    ID_StepBaoGia,
                    NVCHR_ReasonQuotation,
                    CHR_LinkFile,
                    BIT_Select,
                    NVCHR_ReasonPick,
                    NVCHR_File
                FROM CTE_MaxStep
                WHERE rn = 1
                ORDER BY CTE_MaxStep.CHR_MaDon, CTE_MaxStep.CHR_MaHangNoiBo
            ";

            var parameters = new
            {
                MaDon = string.IsNullOrEmpty(MaDon) ? null : MaDon,
                MaNcc = string.IsNullOrEmpty(MaNcc) ? null : MaNcc,
                Section = string.IsNullOrEmpty(Section) ? null : Section,
                NguoiYeuCau = string.IsNullOrEmpty(nguoiYeuCau) ? null : nguoiYeuCau,
                MaHang = string.IsNullOrEmpty(MaHang) ? null : MaHang,
                Step = step,
                ChungLoai = string.IsNullOrEmpty(chungLoai) ? null : chungLoai,
                Adid = user
            };

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return data;
        }
    }
}
