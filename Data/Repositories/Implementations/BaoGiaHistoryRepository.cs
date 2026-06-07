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
            ";


            var whereClauses = new List<string>();
            var parameters = new Dapper.DynamicParameters();

            if (!string.IsNullOrEmpty(MaDon))
            {
                whereClauses.Add("q.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", MaDon);
            }
            if (!string.IsNullOrEmpty(MaNcc))
            {
                whereClauses.Add("q.CHR_MaNCC LIKE @MaNcc");
                parameters.Add("MaNcc", "%" + MaNcc + "%");
            }
            if (!string.IsNullOrEmpty(chungLoai))
            {
                whereClauses.Add("q.NVCHR_ChungLoai LIKE @ChungLoai");
                parameters.Add("ChungLoai", "%" + chungLoai + "%");
            }
            if (!string.IsNullOrEmpty(Section))
            {
                whereClauses.Add("q.CHR_SectionCode LIKE @Section");
                parameters.Add("Section", "%" + Section + "%");
            }
            if (!string.IsNullOrEmpty(nguoiYeuCau))
            {
                whereClauses.Add("q.CHR_CreateBy LIKE @NguoiYeuCau");
                parameters.Add("NguoiYeuCau", "%" + nguoiYeuCau + "%");
            }
            if (!string.IsNullOrEmpty(MaHang))
            {
                whereClauses.Add("q.CHR_MaHangNoiBo LIKE @MaHang");
                parameters.Add("MaHang", "%" + MaHang + "%");
            }
            if (step.HasValue)
            {
                whereClauses.Add("q.ID_StepBaoGia = @Step");
                parameters.Add("Step", step);
            }
            if (!string.IsNullOrEmpty(user))
            {
                whereClauses.Add("s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
            }

            if (!string.IsNullOrEmpty(status))
            {
                string statusSql = status switch
                {
                    "RETURN" => "q.ID_Status LIKE '%RETURN%' AND q.ID_Status NOT LIKE 'DELETE'",
                    "DONE" => "q.ID_Status = 'DONE' AND q.ID_Status NOT LIKE 'DELETE'",
                    "APPROVAL" => "q.ID_Status LIKE 'APPROVAL%' AND q.ID_Status NOT LIKE 'DELETE'",
                    "WAIT" => "q.ID_Status LIKE '%WAIT%' AND q.ID_Status NOT LIKE 'DELETE'",
                    "DELETE" => "q.ID_Status LIKE 'DELETE'",
                    _ => "q.ID_Status NOT LIKE 'DELETE'"
                };
                whereClauses.Add("(" + statusSql + ")");
            }
            else
            {
                whereClauses.Add("q.ID_Status NOT LIKE 'DELETE'");
            }

            if (whereClauses.Any())
            {
                sql += " WHERE " + string.Join(" AND ", whereClauses);
            }
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

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return data;
        }
        // tính tổng số đơn đến hạn
        public async Task<List<dynamic>> GetCountQuotation(string user)
        {
            var sql = @"
                SELECT 
                    SUM(CASE WHEN CAST(r.DTM_KyHan AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS DenHanLuaChon,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) = 1 THEN 1 ELSE 0 END) AS ConMotNgayHetHan,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) > 1 THEN 1 ELSE 0 END) AS ConLai,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) < 0 THEN 1 ELSE 0 END) AS QuaHan
                FROM [BaoGia_Request_of_Quotation] as r
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                WHERE r.BIT_LayBaoGia = 1
                  AND r.ID_StepBaoGia > 2 AND r.ID_StepBaoGia < 12
                  AND r.DTM_KyHan IS NOT NULL
                  AND s.CHR_UserAdid = @Adid
            ";

            var parameters = new { Adid = user };
            var result = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return result;
        }
        // Lấy thông tin lịch sử báo giá
        public async Task<ListRequest<dynamic>> GetHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? dateTo, DateTime? dateFrom, string? chungLoai)
        {
            var sql = @"
                WITH VendorHistory AS (
                    SELECT 
                        h.ID_RequestQuote,
                        h.CHR_MaDon,
                        h.NVCHR_NewValue AS VendorName,
                        ROW_NUMBER() OVER (
                            PARTITION BY h.ID_RequestQuote, h.CHR_MaDon 
                            ORDER BY h.CHR_Updatedate
                        ) AS VendorRank
                    FROM BaoGia_History_Request_of_Quotation h
                    WHERE h.CHR_ActionType = 'Vendor'
                      AND h.NVCHR_NewValue IS NOT NULL
                ),
                VendorPivot AS (
                    SELECT 
                        ID_RequestQuote,
                        CHR_MaDon,
                        MAX(CASE WHEN VendorRank = 1 THEN VendorName END) AS Vendor1,
                        MAX(CASE WHEN VendorRank = 2 THEN VendorName END) AS Vendor2,
                        MAX(CASE WHEN VendorRank = 3 THEN VendorName END) AS Vendor3,
                        MAX(CASE WHEN VendorRank = 4 THEN VendorName END) AS Vendor4,
                        MAX(CASE WHEN VendorRank = 5 THEN VendorName END) AS Vendor5
                    FROM VendorHistory
                    GROUP BY ID_RequestQuote, CHR_MaDon
                )
                SELECT 
                    r.CHR_MaDon AS [SoDon],
                    r.CHR_MaHangNoiBo AS [MaNoiBo],
                    r.CHR_MaHangNCC AS [MaHangNCC],
                    r.CHR_NameEN AS [TenEN],
                    vp.Vendor1,
                    vp.Vendor2,
                    vp.Vendor3,
                    vp.Vendor4,
                    vp.Vendor5,
                    r.DTM_Deadline AS [DeadLineSelectVendor],
                    r.NVCHR_UserRequest AS [PIC],
                    r.CHR_UserApproval AS [QLSC],
                    NULL AS [QLTC],
                    NULL AS [PUR_PIC],
                    NULL AS [PUR_QLSC]
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN VendorPivot vp 
                    ON vp.ID_RequestQuote = r.ID 
                    AND vp.CHR_MaDon = r.CHR_MaDon
            ";

            var whereClauses = new List<string>();
            var parameters = new Dapper.DynamicParameters();

            if (!string.IsNullOrEmpty(MaDon))
            {
                whereClauses.Add("r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", MaDon);
            }
            if (!string.IsNullOrEmpty(MaNcc))
            {
                whereClauses.Add("r.CHR_MaNCC LIKE @MaNcc");
                parameters.Add("MaNcc", "%" + MaNcc + "%");
            }
            if (!string.IsNullOrEmpty(chungLoai))
            {
                whereClauses.Add("r.NVCHR_ChungLoai LIKE @ChungLoai");
                parameters.Add("ChungLoai", "%" + chungLoai + "%");
            }
            if (!string.IsNullOrEmpty(Section))
            {
                whereClauses.Add("r.CHR_SectionCode LIKE @Section");
                parameters.Add("Section", "%" + Section + "%");
            }
            if (!string.IsNullOrEmpty(nguoiYeuCau))
            {
                whereClauses.Add("r.CHR_CreateBy LIKE @NguoiYeuCau");
                parameters.Add("NguoiYeuCau", "%" + nguoiYeuCau + "%");
            }
            if (!string.IsNullOrEmpty(MaHang))
            {
                whereClauses.Add("r.CHR_MaHangNoiBo LIKE @MaHang");
                parameters.Add("MaHang", "%" + MaHang + "%");
            }
            if (step.HasValue)
            {
                whereClauses.Add("r.ID_StepBaoGia = @Step");
                parameters.Add("Step", step);
            }
            if (!string.IsNullOrEmpty(user))
            {
                whereClauses.Add("s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
                // ensure join to master approver exists in WHERE via alias s - add join clause by referencing table in WHERE
                // but since we don't have an explicit join here, include check by joining in where using EXISTS on BaoGia_Master_Approver_Send_Mail
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            var statusSql = "1=1";
            switch (status)
            {
                case "RETURN":
                    statusSql = "r.ID_Status LIKE '%RETURN%' AND r.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DONE":
                    statusSql = "r.ID_Status = 'DONE' AND r.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "APPROVAL":
                    statusSql = "r.ID_Status LIKE 'APPROVAL%' AND r.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "WAIT":
                    statusSql = "r.ID_Status LIKE '%WAIT%' AND r.ID_Status NOT LIKE 'DELETE'";
                    break;
                case "DELETE":
                    statusSql = "r.ID_Status LIKE 'DELETE'";
                    break;
                default:
                    statusSql = "r.ID_Status NOT LIKE 'DELETE'";
                    break;
            }
            whereClauses.Add("(" + statusSql + ")");

            // date range filters (use DTM_CreateDate)
            if (dateFrom.HasValue)
            {
                whereClauses.Add("CAST(r.DTM_CreateDate AS DATE) >= CAST(@DateFrom AS DATE)");
                parameters.Add("DateFrom", dateFrom.Value.Date);
            }
            if (dateTo.HasValue)
            {
                whereClauses.Add("CAST(r.DTM_CreateDate AS DATE) <= CAST(@DateTo AS DATE)");
                parameters.Add("DateTo", dateTo.Value.Date);
            }

            if (whereClauses.Any())
            {
                sql += " WHERE " + string.Join(" AND ", whereClauses);
            }

            // add grouping
            sql += @"
                GROUP BY 
                    r.CHR_MaDon,
                    r.CHR_MaHangNoiBo,
                    r.CHR_MaHangNCC,
                    r.CHR_NameEN,
                    vp.Vendor1,
                    vp.Vendor2,
                    vp.Vendor3,
                    vp.Vendor4,
                    vp.Vendor5,
                    r.DTM_Deadline,
                    r.NVCHR_UserRequest,
                    r.CHR_UserApproval
            ";

            // build count sql
            var countSql = "WITH VendorHistory AS (" +
                " SELECT h.ID_RequestQuote, h.CHR_MaDon, h.NVCHR_NewValue AS VendorName, ROW_NUMBER() OVER (PARTITION BY h.ID_RequestQuote, h.CHR_MaDon ORDER BY h.CHR_Updatedate) AS VendorRank FROM BaoGia_History_Request_of_Quotation h WHERE h.CHR_ActionType = 'Vendor' AND h.NVCHR_NewValue IS NOT NULL" +
                " ), VendorPivot AS ( SELECT ID_RequestQuote, CHR_MaDon, MAX(CASE WHEN VendorRank = 1 THEN VendorName END) AS Vendor1, MAX(CASE WHEN VendorRank = 2 THEN VendorName END) AS Vendor2, MAX(CASE WHEN VendorRank = 3 THEN VendorName END) AS Vendor3, MAX(CASE WHEN VendorRank = 4 THEN VendorName END) AS Vendor4, MAX(CASE WHEN VendorRank = 5 THEN VendorName END) AS Vendor5 FROM VendorHistory GROUP BY ID_RequestQuote, CHR_MaDon ) SELECT COUNT(*) FROM (" +
                " SELECT r.CHR_MaDon FROM BaoGia_Request_of_Quotation r LEFT JOIN VendorPivot vp ON vp.ID_RequestQuote = r.ID AND vp.CHR_MaDon = r.CHR_MaDon";

            if (whereClauses.Any())
            {
                countSql += " WHERE " + string.Join(" AND ", whereClauses);
            }
            countSql += " GROUP BY r.CHR_MaDon, r.CHR_MaHangNoiBo, r.CHR_MaHangNCC, r.CHR_NameEN, vp.Vendor1, vp.Vendor2, vp.Vendor3, vp.Vendor4, vp.Vendor5, r.DTM_Deadline, r.NVCHR_UserRequest, r.CHR_UserApproval ) T";

            // add ordering and paging to main query
            if (pageSize > 0 && pageIndex > 0)
            {
                sql += @"
                    ORDER BY r.CHR_MaDon, r.CHR_MaHangNoiBo
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            else
            {
                sql += @"
                    ORDER BY r.CHR_MaDon, r.CHR_MaHangNoiBo
                ";
            }

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            var totalCount = (await _conn.ExecuteScalarAsync<long>(countSql, parameters));

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
    }
}
