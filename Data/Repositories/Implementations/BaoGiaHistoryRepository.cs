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
        public async Task<List<dynamic>> GetCountQuotation(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang,string? user)
        {
            var sql = @"
                SELECT 
                    SUM(CASE WHEN CAST(r.DTM_KyHan AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS DenHanLuaChon,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) = 1 THEN 1 ELSE 0 END) AS ConMotNgayHetHan,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) > 1 THEN 1 ELSE 0 END) AS ConLai,
                    SUM(CASE WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(r.DTM_KyHan AS DATE)) < 0 THEN 1 ELSE 0 END) AS QuaHan
                FROM [BaoGia_Request_of_Quotation] as r
                WHERE r.BIT_LayBaoGia = 1
                  AND r.ID_StepBaoGia > 2 AND r.ID_StepBaoGia < 12
                  AND r.DTM_KyHan IS NOT NULL
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
            if (!string.IsNullOrEmpty(user))
            {
                parameters.Add("Adid", user);
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            if (whereClauses.Any())
            {
                sql += " AND " + string.Join(" AND ", whereClauses);
            }

            var result = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return result;
        }
        // Lấy thông tin lịch sử báo giá
        public async Task<ListRequest<dynamic>> GetHistoryAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang, string? status, string? user, int pageIndex, int pageSize, DateTime? dateTo, DateTime? dateFrom, string? chungLoai)
        {
            var sql = string.Empty;

            var whereClauses = new List<string>();
            var parameters = new Dapper.DynamicParameters();

            whereClauses.Add("r.BIT_LayBaoGia = 1");

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
            if (!string.IsNullOrEmpty(user))
            {
                parameters.Add("Adid", user);
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            var statusSql = "1=1";
            switch (status)
            {
                case "RETURN":
                    statusSql = "r.ID_Status LIKE 'RETURN%'";
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
                    statusSql = "r.ID_Status NOT LIKE 'DELETE' AND (r.ID_Status NOT LIKE 'RETURN%' or r.ID_StepBaoGia = 8)";
                    break;
            }
            whereClauses.Add("(" + statusSql + ")");

            // date range filters (use DTM_CreateDate)
            if (dateFrom.HasValue)
            {
                whereClauses.Add("r.DTM_CreateDate >= @DateFrom");
                parameters.Add("DateFrom", dateFrom.Value.Date);
            }
            if (dateTo.HasValue)
            {
                whereClauses.Add("r.DTM_CreateDate < @DateTo");
                parameters.Add("DateTo", dateTo.Value.Date.AddDays(1));
            }

            var whereSql = whereClauses.Any()
                ? " WHERE " + string.Join(" AND ", whereClauses)
                : string.Empty;

            var cteSql = @"
                ;WITH FILTERED AS (
                    SELECT
                        r.ID,
                        r.CHR_MaDon,
                        r.CHR_MaHangNCC,
                        r.CHR_MaHangNoiBo,
                        r.CHR_NameEN,
                        r.DTM_KyHan,
                        r.CHR_CreateBy,
                        r.ID_StepBaoGia,
                        r.CHR_MaNCC,
                        r.CHR_UserApproval
                    FROM BaoGia_Request_of_Quotation r
            " + whereSql + @"
                ),
                MAIN AS (
                    SELECT
                        f.CHR_MaDon,
                        f.CHR_MaHangNoiBo,
                        MAX(f.CHR_MaHangNCC) AS CHR_MaHangNCC,
                        MAX(f.CHR_NameEN) AS CHR_NameEN,
                        MAX(f.DTM_KyHan) AS DTM_KyHan,
                        MAX(f.CHR_CreateBy) AS CHR_CreateBy,
                        MAX(f.ID_StepBaoGia) AS Step,
                        MAX(f.CHR_UserApproval) as UserNext
                    FROM FILTERED f
                    GROUP BY
                        f.CHR_MaDon,
                        f.CHR_MaHangNoiBo
                ),
                NCC_ROW AS (
	                SELECT
		                CHR_MaDon,
		                CHR_MaHangNoiBo,
		                ShortName,
						CHR_Status,
		                MAX(ID_StepBaoGia) AS ID_StepBaoGia,
		                ROW_NUMBER() OVER (
			                PARTITION BY CHR_MaDon, CHR_MaHangNoiBo
			                ORDER BY ShortName
		                ) AS rn
	                FROM (
		                SELECT DISTINCT
			                f.CHR_MaDon,
			                f.CHR_MaHangNoiBo,
			                ISNULL(n.ShortName, f.CHR_MaNCC) AS ShortName,
			                f.ID_StepBaoGia,
							d.CHR_Status
		                FROM FILTERED f
		                LEFT JOIN IM_NCC_NEW n ON f.CHR_MaNCC = n.Ma
					 LEFT JOIN BaoGia_Detail_of_Quotation d
                            ON d.ID_RequestQuote = f.ID
	                ) t
	                GROUP BY 
		                CHR_MaDon,
		                CHR_MaHangNoiBo,
		                ShortName,
						CHR_Status
                ),
                NCC_PIVOT AS (
                    SELECT
                        CHR_MaDon,
                        CHR_MaHangNoiBo,
                        MAX(CASE WHEN rn = 1 THEN ShortName END) AS NCC_1,
                        MAX(CASE WHEN rn = 2 THEN ShortName END) AS NCC_2,
                        MAX(CASE WHEN rn = 3 THEN ShortName END) AS NCC_3,
                        MAX(CASE WHEN rn = 4 THEN ShortName END) AS NCC_4,
                        MAX(CASE WHEN rn = 5 THEN ShortName END) AS NCC_5,
		                MAX(CASE WHEN rn = 1 and ID_StepBaoGia >6 THEN 1 END) AS BitNCC_1,
                        MAX(CASE WHEN rn = 2 and ID_StepBaoGia >6 THEN 1 END) AS BitNCC_2,
                        MAX(CASE WHEN rn = 3 and ID_StepBaoGia >6 THEN 1 END) AS BitNCC_3,
                        MAX(CASE WHEN rn = 4 and ID_StepBaoGia >6 THEN 1 END) AS BitNCC_4,
                        MAX(CASE WHEN rn = 5 and ID_StepBaoGia >6 THEN 1 END) AS BitNCC_5,
						MAX(CASE WHEN rn = 1 THEN CHR_Status END) AS Status_1,
                        MAX(CASE WHEN rn = 2 THEN CHR_Status END) AS Status_2,
                        MAX(CASE WHEN rn = 3 THEN CHR_Status END) AS Status_3,
                        MAX(CASE WHEN rn = 4 THEN CHR_Status END) AS Status_4,
                        MAX(CASE WHEN rn = 5 THEN CHR_Status END) AS Status_5
                    FROM NCC_ROW
                    GROUP BY CHR_MaDon, CHR_MaHangNoiBo
                ),
                HIS_RAW AS (
                    SELECT
                        f.CHR_MaDon,
                        f.CHR_MaHangNoiBo,
                        h.CHR_ActionType,
                        h.NVCHR_UpdateName,
                        TRY_CONVERT(DATETIME, h.CHR_Updatedate) AS ApproveTime
                    FROM BaoGia_History_Request_of_Quotation h
                    INNER JOIN FILTERED f
                        ON f.ID = h.ID_RequestQuote
                    WHERE h.CHR_ActionType IN (
                        'QLSC','QLTC','PIC',
                        'QLSC_1','PIC_PICK_NCC',
                        'QLSC_PICK_NCC','QLTC_PICK_NCC',
                        'DEFT_PICK_NCC'
                    )
                ),
                HIS AS (
                    SELECT
                        CHR_MaDon,
                        CHR_MaHangNoiBo,
		                MAX(CASE WHEN CHR_ActionType = 'PIC' THEN NVCHR_UpdateName END) AS PIC_Approve,
		                MAX(CASE WHEN CHR_ActionType = 'PIC' THEN ApproveTime END) AS PIC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC' THEN NVCHR_UpdateName END) AS QLSC_Approve,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC' THEN ApproveTime END) AS QLSC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'QLTC' THEN NVCHR_UpdateName END) AS QLTC_Approve,
		                MAX(CASE WHEN CHR_ActionType = 'QLTC' THEN ApproveTime END) AS QLTC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC_1' THEN NVCHR_UpdateName END) AS QLSC1_Approve,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC_1' THEN ApproveTime END) AS QLSC1_Time,
		                MAX(CASE WHEN CHR_ActionType = 'PIC_PICK_NCC' THEN NVCHR_UpdateName END) AS PIC_PickNCC,
		                MAX(CASE WHEN CHR_ActionType = 'PIC_PICK_NCC' THEN ApproveTime END) AS PIC_PickNCC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC_PICK_NCC' THEN NVCHR_UpdateName END) AS QLSC_PickNCC,
		                MAX(CASE WHEN CHR_ActionType = 'QLSC_PICK_NCC' THEN ApproveTime END) AS QLSC_PickNCC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'QLTC_PICK_NCC' THEN NVCHR_UpdateName END) AS QLTC_PickNCC,
		                MAX(CASE WHEN CHR_ActionType = 'QLTC_PICK_NCC' THEN ApproveTime END) AS QLTC_PickNCC_Time,
		                MAX(CASE WHEN CHR_ActionType = 'DEFT_PICK_NCC' THEN NVCHR_UpdateName END) AS DEFT_PickNCC,
		                MAX(CASE WHEN CHR_ActionType = 'DEFT_PICK_NCC' THEN ApproveTime END) AS DEFT_PickNCC_Time
                    FROM HIS_RAW
                    GROUP BY CHR_MaDon, CHR_MaHangNoiBo
                ),
                PICK AS (
                    SELECT *
                    FROM (
                        SELECT
                            f.CHR_MaDon,
                            f.CHR_MaHangNoiBo,
                            ISNULL(n.ShortName, n.Ma) AS NCC_DuocChon,
                            d.NVCHR_ReasonPick,
                            d.NVCHR_File,
                            f.ID_StepBaoGia AS PickStep,
                            ROW_NUMBER() OVER (
                                PARTITION BY f.CHR_MaDon, f.CHR_MaHangNoiBo
                                ORDER BY d.ID DESC
                            ) AS rn
                        FROM FILTERED f
                        INNER JOIN BaoGia_Detail_of_Quotation d
                            ON d.ID_RequestQuote = f.ID
                            AND d.BIT_Select = 1
                        LEFT JOIN IM_NCC_NEW n
                            ON f.CHR_MaNCC = n.Ma
                    ) t
                    WHERE rn = 1
                )   
            ";

            sql = cteSql + @"
                SELECT
                    m.CHR_MaDon,
                    m.CHR_MaHangNoiBo,
                    m.CHR_MaHangNCC,
                    m.CHR_NameEN,
                    m.DTM_KyHan,
                    m.CHR_CreateBy,
					m.UserNext,
					ISNULL(p.PickStep, m.Step) AS Step,
					s.CHR_StepName,
					s.CHR_StepNameEN,
					s.CHR_StepNameJP,
                    ncc.NCC_1,
                    ncc.NCC_2,
                    ncc.NCC_3,
                    ncc.NCC_4,
                    ncc.NCC_5,
					ncc.BitNCC_1,
                    ncc.BitNCC_2,
                    ncc.BitNCC_3,
                    ncc.BitNCC_4,
                    ncc.BitNCC_5,
					ncc.Status_1,
					ncc.Status_2,
					ncc.Status_3,
					ncc.Status_4,
					ncc.Status_5,
                    h.PIC_Approve, h.PIC_Time,
                    h.QLSC_Approve, h.QLSC_Time,
                    h.QLTC_Approve, h.QLTC_Time,
                    h.QLSC1_Approve, h.QLSC1_Time,
                    h.PIC_PickNCC, h.PIC_PickNCC_Time,
                    h.QLSC_PickNCC, h.QLSC_PickNCC_Time,
                    h.QLTC_PickNCC, h.QLTC_PickNCC_Time,
                    h.DEFT_PickNCC, h.DEFT_PickNCC_Time,
                    p.NCC_DuocChon,
                    p.NVCHR_ReasonPick,
                    p.NVCHR_File
                FROM MAIN m
                LEFT JOIN NCC_PIVOT ncc
                    ON m.CHR_MaDon = ncc.CHR_MaDon
                    AND m.CHR_MaHangNoiBo = ncc.CHR_MaHangNoiBo
                LEFT JOIN HIS h
                    ON m.CHR_MaDon = h.CHR_MaDon
                    AND m.CHR_MaHangNoiBo = h.CHR_MaHangNoiBo
                LEFT JOIN PICK p
                    ON m.CHR_MaDon = p.CHR_MaDon
                    AND m.CHR_MaHangNoiBo = p.CHR_MaHangNoiBo
                LEFT JOIN BaoGia_Step s
                    ON ISNULL(p.PickStep, m.Step) = s.INT_StepNumber
            ";

            var countSql = cteSql + @"
                SELECT COUNT(1)
                FROM MAIN
            ";

            if (pageSize > 0 && pageIndex > 0)
            {
                sql += @"
                    ORDER BY m.CHR_MaHangNoiBo
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            else
            {
                sql += @"
                    ORDER BY m.CHR_MaHangNoiBo
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

        // Tính tổng theo trạng thái đơn
        public async Task<List<dynamic>> GetCountStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau,
        string? MaHang,string? user)
        {
            var sql = @"
            WITH DonHang AS (
                 SELECT 
                     r.CHR_MaDon,
                     MAX(CASE WHEN r.ID_StepBaoGia = 1 THEN 1 ELSE 0 END) AS PICSection,
                     MAX(CASE WHEN r.ID_StepBaoGia = 2 THEN 1 ELSE 0 END) AS QLSCSection,
                     MAX(CASE WHEN r.ID_StepBaoGia = 3 THEN 1 ELSE 0 END) AS QLTCSection,
                     MAX(CASE WHEN r.ID_StepBaoGia = 4 THEN 1 ELSE 0 END) AS PICPur,
                     MAX(CASE WHEN r.ID_StepBaoGia = 5 THEN 1 ELSE 0 END) AS QLSCPur
                 FROM BaoGia_Request_of_Quotation r
                 WHERE r.BIT_LayBaoGia = 1 and r.ID_StepBaoGia < 6
                     AND r.ID_Status NOT LIKE 'DELETE'
                     AND r.ID_Status NOT LIKE 'RETURN%'
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
            if (!string.IsNullOrEmpty(user))
            {
                parameters.Add("Adid", user);
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            if (whereClauses.Any())
            {
                sql += " AND " + string.Join(" AND ", whereClauses);
            }

            sql += @"
                GROUP BY r.CHR_MaDon, r.BIT_LayBaoGia
            )

            SELECT 
                SUM(PICSection) AS PICSection,
                SUM(QLSCSection) AS QLSCSection,
                SUM(QLTCSection) AS QLTCSection,
                SUM(PICPur) AS PICPur,
                SUM(QLSCPur) AS QLSCPur
            FROM DonHang;";

            var result = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return result;
        }
        // Tính tình trạng xử lý đơn hàng
        public async Task<List<dynamic>> GetProcessingStatus(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user)
        {
            var sql = @"
            WITH DonHang AS (
                    SELECT 
                            r.CHR_MaDon,
                            CASE WHEN COUNT(CASE WHEN r.ID_StepBaoGia = 13 THEN 1 END) = COUNT(*) THEN 1 ELSE 0 END AS IsCompleted,
                            CASE WHEN COUNT(CASE WHEN r.ID_StepBaoGia > 11  and r.ID_StepBaoGia <13  THEN 1 END) = COUNT(*) THEN 1 ELSE 0 END AS IsProcessing,
                            CASE WHEN COUNT(CASE WHEN r.ID_StepBaoGia < 11 THEN 1 END) = COUNT(*) THEN 1 ELSE 0 END AS IsChuaXuLy
                        FROM BaoGia_Request_of_Quotation r
                    WHERE r.BIT_LayBaoGia = 1
                        AND r.ID_Status NOT LIKE 'DELETE'
                        AND (r.ID_Status NOT LIKE 'RETURN%' or r.ID_StepBaoGia = 8)
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
            if (!string.IsNullOrEmpty(user))
            {
                parameters.Add("Adid", user);
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            if (whereClauses.Any())
            {
                sql += " AND " + string.Join(" AND ", whereClauses);
            }

            sql += @"
                GROUP BY r.CHR_MaDon, r.BIT_LayBaoGia
            )

            SELECT 
                SUM(IsCompleted) AS SoDonHoanThanh,
                SUM(IsProcessing) AS SoDonDangXuLy,
                SUM(IsChuaXuLy) AS SoDonChuaXuLy
            FROM DonHang;";

            var result = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return result;
        }
        // Tính các đơn hàng đang chờ chọn nhà cung cấp
        public async Task<List<dynamic>> GetWaitingForSupplier(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? user)
        {
            var sql = @"
            WITH Base AS (
                SELECT DISTINCT
                    CHR_MaDon,
                    CHR_MaHangNoiBo,
                    REPLACE(LTRIM(RTRIM(CHR_MaHangNCC)), ' ', '') AS CHR_MaHangNCC_Clean,
                    ID_StepBaoGia
                FROM BaoGia_Request_of_Quotation r
                WHERE BIT_LayBaoGia = 1
		            AND ID_StepBaoGia > 2
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
            if (!string.IsNullOrEmpty(user))
            {
                parameters.Add("Adid", user);
                whereClauses.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @Adid)");
            }

            if (whereClauses.Any())
            {
                sql += " AND " + string.Join(" AND ", whereClauses);
            }

            sql += @"
            )
            SELECT
                -- Cần lấy quotation
                (SELECT COUNT(DISTINCT CHR_MaDon + '|' + CHR_MaHangNoiBo + '|' + CHR_MaHangNCC_Clean) FROM Base) AS IsNeed,

                -- Yêu cầu lựa chọn
                (SELECT COUNT(DISTINCT CHR_MaDon + '|' + CHR_MaHangNoiBo) FROM Base ) AS IsNeedPick,--WHERE ID_StepBaoGia > 5

                -- Đã chọn NCC
                (SELECT COUNT(DISTINCT CHR_MaDon + '|' + CHR_MaHangNoiBo) FROM Base WHERE ID_StepBaoGia > 9) AS IsPicked,

                -- Chờ lựa chọn
                (SELECT COUNT(DISTINCT CHR_MaDon + '|' + CHR_MaHangNoiBo) FROM Base WHERE ID_StepBaoGia > 6 AND ID_StepBaoGia < 9) AS IsPicking;";

            var result = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return result;
        }

        // Lấy lịch sử của đơn hành
        public async Task<List<BaoGia_History_Request_of_Quotation>> GetOrderHistoryAsync(string? maDon, string? maHang, string? maHangNCC)
        {

            var sql = @"
                SELECT 
                    h.*
                FROM BaoGia_History_Request_of_Quotation as h
                LEFT JOIN BaoGia_Request_of_Quotation as r ON h.ID_RequestQuote = r.ID
                WHERE 1=1 ";

            var whereClauses = new List<string>();
            var parameters = new Dapper.DynamicParameters();
            // Dieu kien loc theo maDon, maHang, maHangNCC
            if (!string.IsNullOrEmpty(maDon))
            {
                whereClauses.Add("r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                whereClauses.Add("r.CHR_MaHangNoiBo = @Mahang");
                parameters.Add("Mahang", maHang);
            }
            if (!string.IsNullOrEmpty(maHangNCC))
            {
                whereClauses.Add("r.CHR_MaHangNCC = @MaHangNCC");
                parameters.Add("MaHangNCC", maHangNCC);
            }

            if (whereClauses.Any())
            {
                sql += " AND " + string.Join(" AND ", whereClauses);
            }

            var result = (await _conn.QueryAsync<BaoGia_History_Request_of_Quotation>(sql, parameters)).ToList();
            return result;
        }

    }
}
