using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;


// removed unused OpenXML usings
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaRepository : BaseRepository<BaoGia_Request_of_Quotation, int>, IBaoGiaRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration)
        {
            _context = context;
        }

        // Lấy thông tin báo giá theo mã báo giá
        public async Task<List<BaoGia_Request_of_Quotation>> GetByMaBaoGiaAsync(string maBaoGia)
        {
            var sql = "SELECT * FROM BaoGia_Request_of_Quotation WHERE CHR_MaDon = @MaBaoGia";
            var parameters = new { MaBaoGia = maBaoGia };
            return (await _conn.QueryAsync<BaoGia_Request_of_Quotation>(sql, parameters)).ToList();
        }
        // Tìm kiếm thông tin báo giá và phân trang
        public async Task<ListRequest<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user, int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var sql = @"
                SELECT DISTINCT q.*
                FROM BaoGia_Request_of_Quotation as q
                INNER JOIN [BaoGia_Master_Approver_Send_Mail] as s ON q.CHR_SectionCode = s.CHR_CodeSection
                WHERE --(@MaDon IS NULL OR CHR_MaDon LIKE '%' + @MaDon + '%')
                  (@MaDon IS NULL OR CHR_MaDon = @MaDon)
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

                // append status filter
                sql += " AND (" + statusSql + ")";

            // Build count query first
            var countSql = @"
                SELECT COUNT(distinct q.ID)
                FROM BaoGia_Request_of_Quotation as q
				  inner join [BaoGia_Master_Approver_Send_Mail] as s 
				on q.CHR_SectionCode = s.CHR_CodeSection
                WHERE (@MaDon IS NULL OR CHR_MaDon LIKE '%' + @MaDon + '%')
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

            var data = (await _conn.QueryAsync<BaoGia_Request_of_Quotation>(sql, parameters)).ToList();
            var totalCount = await _conn.ExecuteScalarAsync<long>(countSql, parameters);

            return new ListRequest<BaoGia_Request_of_Quotation>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        // Nhap bao gia
        public async Task<bool> NhapBaoGiaAsync(BaoGia_Request_of_Quotation baoGia)
        {
            _context.BaoGia_Request_of_Quotations.Add(baoGia);
            await _context.SaveChangesAsync();
            return true;
        }
        // Nhap danh sach
        public async Task<List<BaoGia_Request_of_Quotation>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_Quotation> danhSachBaoGia)
        {
            if (danhSachBaoGia == null || !danhSachBaoGia.Any())
            {
                return new List<BaoGia_Request_of_Quotation>();
            }

            // Process each item to handle duplicate CHR_MaDon
            foreach (var baoGia in danhSachBaoGia)
            {
                if (!string.IsNullOrEmpty(baoGia.CHR_MaDon))
                {
                    // Check if CHR_MaDon already exists
                    var exists = await _context.BaoGia_Request_of_Quotations.AnyAsync(b => b.CHR_MaDon == baoGia.CHR_MaDon);
                    if (exists)
                    {
                        // Find the next available suffix
                        var baseMaDon = baoGia.CHR_MaDon;
                        var maxSuffix = await _conn.ExecuteScalarAsync<int?>(@"
                            SELECT MAX(CAST(SUBSTRING(CHR_MaDon, LEN(@BaseMaDon) + 2, LEN(CHR_MaDon) - LEN(@BaseMaDon) - 1) AS INT))
                            FROM BaoGia_Request_of_Quotation
                            WHERE CHR_MaDon LIKE @BaseMaDon + '_%' AND ISNUMERIC(SUBSTRING(CHR_MaDon, LEN(@BaseMaDon) + 2, LEN(CHR_MaDon) - LEN(@BaseMaDon) - 1)) = 1",
                            new { BaseMaDon = baseMaDon });

                        int nextSuffix = (maxSuffix ?? 0) + 1;
                        baoGia.CHR_MaDon = $"{baseMaDon}_{nextSuffix}";
                    }
                }
            }

            await _context.BaoGia_Request_of_Quotations.AddRangeAsync(danhSachBaoGia);
            await _context.SaveChangesAsync();
            return danhSachBaoGia;
        }
        // Update thông tin bao gia
        public async Task<bool> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_Quotation baoGia)
        {
            _context.BaoGia_Request_of_Quotations.Update(baoGia);
            await _context.SaveChangesAsync();
            return true;
        }
        // Lấy danh sách mã đơn báo giá 
        public async Task<List<string>> GetListMaDonBGAsync()
        {
            var sql = @"SELECT DISTINCT CHR_MaDon FROM BaoGia_Request_of_Quotation WHERE CHR_MaDon IS NOT NULL and ID_StepBaoGia <> 9";
            var maDons = await _conn.QueryAsync<string>(sql);
            return maDons.ToList();
        }
        // Cập nhâp danh sách mã đơn báo giá
        public async Task<List<BaoGia_Request_of_Quotation>> CapNhatDanhSachBGAsync(List<BaoGia_Request_of_Quotation> danhSachMaDonBG)
        {
            if (danhSachMaDonBG == null || !danhSachMaDonBG.Any())
            {
                return new List<BaoGia_Request_of_Quotation>();
            }
            foreach(var item in danhSachMaDonBG)
            {
                if(item.BIT_LayBaoGia == false)
                {
                    item.ID_Status = "NOT_QUOTATION";
                }
            }
            // Update range and persist changes
            _context.BaoGia_Request_of_Quotations.UpdateRange(danhSachMaDonBG);
            await _context.SaveChangesAsync();
            return danhSachMaDonBG;
        }
        // Cập nhật đơn báo giá
        public async Task<BaoGia_Request_of_Quotation> CapNhatDonBaoGiaAsync(BaoGia_Request_of_Quotation baogia)
        {
            if (baogia == null || baogia.ID == 0)
            {
                throw new ArgumentException("Invalid BaoGia_Request_of_Quotation object.");
            }

            // Lấy entity hiện có từ database
            var existingEntity = await _context.BaoGia_Request_of_Quotations
                .FirstOrDefaultAsync(x => x.ID == baogia.ID);

            if (existingEntity == null)
            {
                throw new ArgumentException($"BaoGia_Request_of_Quotation with ID {baogia.ID} not found.");
            }

            // Chỉ update các trường được chỉ định
            existingEntity.CHR_MaDon = baogia.CHR_MaDon;
            existingEntity.CHR_SectionCode = baogia.CHR_SectionCode;
            existingEntity.CHR_SectionName = baogia.CHR_SectionName;
            existingEntity.CHR_Phanloai = baogia.CHR_Phanloai;
            existingEntity.CHR_MaThietBi = baogia.CHR_MaThietBi;
            existingEntity.CHR_MaHangNoiBo = baogia.CHR_MaHangNoiBo;
            existingEntity.CHR_MaHangNCC = baogia.CHR_MaHangNCC;
            existingEntity.NVCHR_NameVN = baogia.NVCHR_NameVN;
            existingEntity.CHR_NameEN = baogia.CHR_NameEN;
            existingEntity.INT_SoLuong = baogia.INT_SoLuong;
            existingEntity.NVCHR_DonVi = baogia.NVCHR_DonVi;
            existingEntity.NVCHR_ChungLoai = baogia.NVCHR_ChungLoai;
            existingEntity.NVCHR_HinhDang = baogia.NVCHR_HinhDang;
            existingEntity.NVCHR_ChatLieu = baogia.NVCHR_ChatLieu;
            existingEntity.NVCHR_ThanhPhan = baogia.NVCHR_ThanhPhan;
            existingEntity.NVCHR_KichThuoc = baogia.NVCHR_KichThuoc;
            existingEntity.NVCHR_DongMay = baogia.NVCHR_DongMay;
            existingEntity.NVCHR_TinhNang = baogia.NVCHR_TinhNang;
            existingEntity.NVCHR_Rohs = baogia.NVCHR_Rohs;
            existingEntity.NVCHR_COCQ = baogia.NVCHR_COCQ;
            existingEntity.NVCHR_MSDS = baogia.NVCHR_MSDS;
            existingEntity.NVCHR_AnToan = baogia.NVCHR_AnToan;
            existingEntity.NVCHR_NhaSanXuat = baogia.NVCHR_NhaSanXuat;
            existingEntity.CHR_MaNCC = baogia.CHR_MaNCC;
            existingEntity.NVCHR_TenNCC = baogia.NVCHR_TenNCC;
            existingEntity.BIT_LayBaoGia = baogia.BIT_LayBaoGia;
            existingEntity.NVCHR_LyDo = baogia.NVCHR_LyDo;
            existingEntity.DTM_NgayMuonNhan = baogia.DTM_NgayMuonNhan;
            existingEntity.DTM_KyHan = baogia.DTM_KyHan;
            existingEntity.CHR_Gap = baogia.CHR_Gap;
            existingEntity.CHR_CreateBy = baogia.CHR_CreateBy;
            existingEntity.DTM_CreateDate = baogia.DTM_CreateDate;
            existingEntity.ID_Status = baogia.ID_Status;
            existingEntity.ID_StepBaoGia = baogia.ID_StepBaoGia;
            existingEntity.INT_SoLanUpdate = baogia.INT_SoLanUpdate;
            existingEntity.DTM_UpdateLater = baogia.DTM_UpdateLater;
            existingEntity.DTM_Deadline = baogia.DTM_Deadline;
            existingEntity.BIT_IsTemplate = baogia.BIT_IsTemplate;
            existingEntity.NVCHR_FileThietKe = baogia.NVCHR_FileThietKe;

            // Đánh dấu entity đã bị thay đổi
            _context.Entry(existingEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return existingEntity;
        }

        public async Task<ListRequest<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string? status, string user, int pageIndex, int pageSize)
        {
            var parameters = new DynamicParameters();

            // Build WHERE clause conditions
            var whereConditions = new List<string>();

            whereConditions.Add("r.ID_StepBaoGia BETWEEN 6 AND 11");
            whereConditions.Add("r.BIT_LayBaoGia = 1");

            if (!string.IsNullOrEmpty(user))
            {
                whereConditions.Add("(r.CHR_UserApproval = @Adid OR (r.ID_StepBaoGia = 11 AND m.CHR_UserAdid = @Adid))");
                parameters.Add("Adid", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                whereConditions.Add("r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                whereConditions.Add("r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }
            if (!string.IsNullOrEmpty(section))
            {
                whereConditions.Add("r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }

            var whereClause = whereConditions.Any() ? "AND " + string.Join(" AND ", whereConditions) : "";

            // Main query with pagination
            var sql = $@"
            WITH rq AS (
                SELECT DISTINCT
                    r.CHR_MaDon,
                    r.CHR_SectionName,
					MIN(CAST(r.DTM_NgayMuonNhan AS DATE)) AS DTM_NgayMuonNhan,
					MIN(CAST(r.DTM_KyHan AS DATE)) AS DTM_KyHan,
                    r.CHR_CreateBy,
                    r.ID_StepBaoGia
                FROM [BaoGia_Request_of_Quotation] r
                LEFT JOIN BaoGia_Master_Approver_Send_Mail m ON r.ID_StepBaoGia = m.ID_BaoGiaStep
                WHERE 1=1 {whereClause}
                GROUP BY r.CHR_MaDon, r.CHR_SectionName, r.CHR_CreateBy, r.ID_StepBaoGia
            )
            SELECT 
                rr.CHR_MaDon,
                rr.CHR_SectionName,
                rr.DTM_NgayMuonNhan,
                rr.DTM_KyHan,
                rr.CHR_CreateBy,
                rr.ID_StepBaoGia,
                CASE 
                    WHEN rr.ID_StepBaoGia = 6 THEN N'WAITING_NCC'
                    WHEN rr.ID_StepBaoGia = 7 THEN N'WAITING_PICK_NCC'
                    WHEN rr.ID_StepBaoGia IN (9,10,11) THEN N'WAITING_APPROVER'
                    ELSE 'NO'
                END AS [Status],
                STUFF((
                    SELECT DISTINCT '; ' + ISNULL(CHR_MaNCC, '')
                    FROM [BaoGia_Request_of_Quotation] bg
                    WHERE bg.CHR_MaDon = rr.CHR_MaDon AND ISNULL(bg.CHR_MaNCC, '') != ''
                    FOR XML PATH('')
                ), 1, 2, '') AS suppliesList
                --,STUFF((
                --    SELECT DISTINCT '; ' + ISNULL(NVCHR_ChungLoai, '')
                --    FROM [BaoGia_Request_of_Quotation] bg
                --    WHERE bg.CHR_MaDon = rr.CHR_MaDon AND ISNULL(bg.NVCHR_ChungLoai, '') != ''
                --    FOR XML PATH('')
                --), 1, 2, '') AS categoryList
            FROM rq rr
            {(string.IsNullOrEmpty(status) ? "" : "WHERE CASE WHEN rr.ID_StepBaoGia = 6 THEN N'WAITING_NCC' WHEN rr.ID_StepBaoGia = 7 THEN N'WAITING_PICK_NCC' WHEN rr.ID_StepBaoGia IN (9,10,11) THEN N'WAITING_APPROVER' ELSE 'NO' END = @Status")}
            ORDER BY rr.CHR_MaDon DESC
            {(pageSize > 0 && pageIndex > 0 ? "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY" : "")}";

            if (pageSize > 0 && pageIndex > 0)
            {
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            if (!string.IsNullOrEmpty(status))
            {
                parameters.Add("Status", status);
            }

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();

            // Count query (optimized)
            var countSql = $@"
            SELECT COUNT(DISTINCT CONCAT(r.CHR_MaDon, '|', r.CHR_SectionName))
            FROM [BaoGia_Request_of_Quotation] r
            LEFT JOIN BaoGia_Master_Approver_Send_Mail m ON r.ID_StepBaoGia = m.ID_BaoGiaStep
            WHERE 1=1 {whereClause}
            {(string.IsNullOrEmpty(status) ? "" : @" AND 
            CASE 
                WHEN r.ID_StepBaoGia = 6 THEN N'WAITING_NCC'
                WHEN r.ID_StepBaoGia = 7 THEN N'WAITING_PICK_NCC'
                WHEN r.ID_StepBaoGia IN (9,10,11) THEN N'WAITING_APPROVER'
                ELSE 'NO'
            END = @Status")}";

            var total = await _conn.ExecuteScalarAsync<int>(countSql, parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = total
            };
        }
        // Xuất báo giá
        public async Task<List<int>> ExportBaoGiaAsync(string? maDon)
        {
            var a = await _context.BaoGia_Request_of_Quotations
                .Where(c => c.CHR_MaDon == maDon).Select(c => c.ID).ToListAsync();
            return a;
        }
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public async Task<ListRequest<dynamic>> SearchThongTinNhapBaoGiaAsync(
            string? maDon,
            string? section,
            string? maHang,
            string? user,
            string? status,
            int pageIndex,
            int pageSize)
        {
            var cteBuilder = new StringBuilder();

            cteBuilder.Append(@"
            WITH BangTongHop AS
            (
                SELECT
                    r.CHR_MaDon,
                    CONVERT(DATE, r.DTM_CreateDate) AS DTM_CreateDate,
                    r.CHR_SectionName,
                    r.CHR_CreateBy,
                    r.CHR_MaHangNoiBo,
                    r.CHR_MaNCC,
                    r.ID_StepBaoGia
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Master_Approver_Send_Mail s
                    ON r.CHR_SectionCode = s.CHR_CodeSection
                WHERE r.ID_StepBaoGia > 5
            ");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(user))
            {
                cteBuilder.Append(" AND s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
            }

            if (!string.IsNullOrWhiteSpace(maDon))
            {
                cteBuilder.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }

            if (!string.IsNullOrWhiteSpace(maHang))
            {
                cteBuilder.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }

            if (!string.IsNullOrWhiteSpace(section))
            {
                cteBuilder.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }

            cteBuilder.Append(@"
                ),
                TongHop AS
                (
                    SELECT
                        CHR_MaDon,
                        MAX(DTM_CreateDate) AS DTM_CreateDate,
                        MAX(CHR_SectionName) AS CHR_SectionName,
                        MAX(CHR_CreateBy) AS CHR_CreateBy,
                        COUNT(DISTINCT CHR_MaHangNoiBo) AS SoLuongLinhKien,
                        MIN(ID_StepBaoGia) AS MinStep,
                        MAX(ID_StepBaoGia) AS MaxStep
                    FROM BangTongHop
                    GROUP BY CHR_MaDon
                )");

            var sql = new StringBuilder();
            sql.Append(cteBuilder);

            sql.Append(@"
                SELECT
                    t.CHR_MaDon,
                    t.DTM_CreateDate,
                    t.CHR_SectionName,
                    t.CHR_CreateBy,
                    t.SoLuongLinhKien,

                    STUFF
                    (
                        (
                            SELECT DISTINCT
                                ', ' + b.CHR_MaNCC
                            FROM BangTongHop b
                            WHERE b.CHR_MaDon = t.CHR_MaDon
                              AND ISNULL(b.CHR_MaNCC, '') <> ''
                            FOR XML PATH('')
                        ),
                        1,
                        2,
                        ''
                    ) AS DanhSachNCC,

                    CASE
                        WHEN t.MinStep = 6
                         AND t.MaxStep = 6
                        THEN 'Confirm'
                        ELSE 'Done'
                    END AS TrangThai

                FROM TongHop t
                WHERE 1 = 1
            ");

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Confirm", StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(@"
                        AND t.MinStep = 6
                        AND t.MaxStep = 6 ");
                }
                else if (status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(@"
                        AND NOT (t.MinStep = 6 AND t.MaxStep = 6) ");
                }
            }

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(@"
                    ORDER BY
                        CASE
                            WHEN t.MinStep = 6 AND t.MaxStep = 6 THEN 0
                            ELSE 1
                        END,
                        t.DTM_CreateDate DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY");

                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            else
            {
                sql.Append(@"
                ORDER BY
                    CASE
                        WHEN t.MinStep = 6 AND t.MaxStep = 6 THEN 0
                        ELSE 1
                    END,
                    t.DTM_CreateDate DESC");
            }

            var result = await _conn.QueryAsync<dynamic>(
                sql.ToString(),
                parameters);

            // COUNT
            var countSql = new StringBuilder();
            countSql.Append(cteBuilder);

            countSql.Append(@"
                SELECT COUNT(*)
                FROM TongHop t
                WHERE 1 = 1 ");

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Confirm", StringComparison.OrdinalIgnoreCase))
                {
                    countSql.Append(@"
                        AND t.MinStep = 6
                        AND t.MaxStep = 6 ");
                }
                else if (status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                {
                    countSql.Append(@"
                        AND NOT (t.MinStep = 6 AND t.MaxStep = 6) ");
                }
            }

            var total = await _conn.ExecuteScalarAsync<int>(
                countSql.ToString(),
                parameters);

            return new ListRequest<dynamic>
            {
                Data = result.ToList(),
                TotalCount = total
            };
        }
        // Lấy thông tin kèm chi tiết báo giá
        public async Task<ListRequest<dynamic>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize)
        {
            var sql = new StringBuilder(@"
             SELECT r.*,
                 d.[CHR_CodeNCC],
                 d.[NVCHR_NameNCC],
                 d.[CHR_MaHangNCC] as CodeEquipmentNCC,
                 d.[NVCHR_TenHangHQ],
                 d.[NVCHR_PaymentTerm],
                 d.[NVCHR_Warranty],
                 d.[NVCHR_DeliveryTerm],
                 d.[VCHR_Rohs],
                 d.[VCHR_COCQ],
                 d.[VCHR_MSDS],
                 d.[VCHR_AnToan],
                 d.[VCHR_CamKet],
                 d.[CHR_NameEN] as NameENByNCC,
                 d.[INT_SoLuong] as soluong,
                 d.[NVCHR_DonVi] as donvi,
                 d.[NVCHR_NhaSanXuat],
                 d.[DTM_EffectiveDate],
                 d.[DTM_ExpiryDate],
                 d.[NVCHR_Note],
                 d.[NVCHR_File],
                 d.[NVCHR_MOQ],
                 d.[DTM_LeadTime],
                 d.[DTM_ShipTime],
                 d.[NVCHR_Packing],
                 d.BIT_Select,
                 d.NVCHR_ReasonPick,
                 d.FL_USD,
                 d.FL_VND,
                 d.FL_Sum,
				 d.CHR_Status,
                 CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                 CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                 CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                 CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong or d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                 CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi or d.NVCHR_DonVi is null) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
		            CAST(CASE 
			            WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
			            WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'' )  THEN 1 
			            WHEN(r.NVCHR_Rohs ='') THEN 1
			            ELSE 0 
		            END AS BIT) AS IsMatch_Rohs,
		            CAST(CASE 
			            WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
			            WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1 
			            WHEN( R.NVCHR_COCQ ='') THEN 1
			            ELSE 0 
		            END AS BIT) AS IsMatch_COCQ,

		            CAST(CASE 
			            WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
			            WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1 
			            WHEN(r.NVCHR_MSDS ='') THEN 1
			            ELSE 0 
		            END AS BIT) AS IsMatch_MSDS,

		            CAST(CASE 
			            WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
			            WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1 
			            WHEN(r.NVCHR_AnToan ='') THEN 1
			            ELSE 0 
		            END AS BIT) AS IsMatch_AnToan,
                 CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) or d.DTM_ShipTime is null ) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                 CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' then 0 else 1 end as bit) As IsMatchCamKet,
	             st.NVCHR_TenStatus  as status,
                 n.ShortName
             FROM BaoGia_Request_of_Quotation r
             LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
             LEFT JOIN BaoGia_Status st on r.ID_Status = st.VCHR_CodeStatus
             left Join IM_NCC_NEW as n on r.CHR_MaNCC = n.Ma
             WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11 and r.BIT_LayBaoGia = 1 ");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @User)");
                parameters.Add("User", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }
            if (!string.IsNullOrEmpty(section))
            {
                sql.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }
            if (!string.IsNullOrEmpty(maNCC))
            {
                sql.Append(" AND r.CHR_MaNCC = @MaNCC");
                parameters.Add("MaNCC", maNCC);
            }
            // Filter by status 
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "WAIT_PICK_NCC":
                        sql.Append(" AND (r.ID_StepBaoGia >5 and r.ID_StepBaoGia <=7)");
                        break;
                    case "WAIT_APPROVAL":
                        sql.Append(" AND (r.ID_StepBaoGia >8 and r.ID_StepBaoGia <12)");
                        break;
                    case "RETURN_APPROVAL":
                        sql.Append(" AND (r.ID_StepBaoGia = 8)");
                        break;
                    default:
                        sql.Append(" AND 1=0"); 
                        break;
                }
            }

            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaDon ,r.CHR_MaThietBi, r.CHR_MaNCC ,r.CHR_MaHangNoiBo, r.NVCHR_NameVN");

            if (pageSize > 0 && pageIndex >= 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            var a = sql.ToString();
            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            // For total count, build a similar CTE so status filter and partition logic match the main query
            var countSql = new StringBuilder(@"
            SELECT COUNT(r.id)
                FROM BaoGia_Request_of_Quotation r
                WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11 and r.BIT_LayBaoGia = 1");
            if (!string.IsNullOrEmpty(user))
            {
                countSql.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @User)");
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                countSql.Append(" AND r.CHR_MaDon = @MaDon");
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                countSql.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
            }
            if (!string.IsNullOrEmpty(section))
            {
                countSql.Append(" AND r.CHR_SectionCode = @Section");
            }
            if (!string.IsNullOrEmpty(maNCC))
            {
                countSql.Append(" AND r.CHR_MaNCC = @MaNCC");
            }
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "WAIT_PICK_NCC":
                        countSql.Append(" AND (r.ID_StepBaoGia >5 and r.ID_StepBaoGia <=7)");
                        break;
                    case "WAIT_APPROVAL":
                        countSql.Append(" AND (r.ID_StepBaoGia >8 and r.ID_StepBaoGia <12)");
                        break;
                    case "RETURN_APPROVAL":
                        countSql.Append(" AND (r.ID_StepBaoGia = 8)");
                        break;
                    default:
                        countSql.Append(" AND 1=0");
                        break;
                }
            }
            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql.ToString(), parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        // lấy mã đơn theo Adid
        public async Task<List<string>> GetMaDonByAdidAsync(string adid, int step)
        {
            var sql = @"  SELECT DISTINCT CHR_MaDon FROM BaoGia_Request_of_Quotation as q
                  inner join [BaoGia_Master_Approver_Send_Mail] as s 
                  on q.CHR_SectionCode = s.CHR_CodeSection
                  WHERE s.CHR_UserAdid = @Adid AND ID_StepBaoGia < @Step  AND ID_STATUS <> 'DELETE'
                  order by CHR_MaDon";
            var parameters = new { Adid = adid , Step = step};
            var maDons = await _conn.QueryAsync<string>(sql, parameters);
            return maDons.ToList();
        }
        // update thông tin màn hình lịch sử báo giá
        public async Task<UpdateHistoryResult> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias)
        {
            var listResult = new List<int>();
            var sectionCode = "";
            bool isReturn = false;
            var listUpdate = new List<BaoGia_Request_of_Quotation>();
            var listOldData = await _context.BaoGia_Request_of_Quotations.Where(c => baoGias.Select(b => b.ID).Contains(c.ID)).ToListAsync();
            if (listOldData == null || !listOldData.Any())
            {
                throw new Exception("Dữ liệu không tồn tại để sửa lại");
            }
            var listHistory = new List<BaoGia_History_Request_of_Quotation>();
            foreach (var baoGia in baoGias)
            {
                var dto = listOldData.Find(c => c.ID == baoGia.ID);
                if (dto != null) {
                    if(dto.ID_StepBaoGia >= 6)
                    {
                       throw new Exception($"Đơn đã phê duyệt, không cập nhật");
                    }
                    if (sectionCode == "") {
                        sectionCode = baoGia.CHR_SectionCode;
                    }

                    dto.CHR_SectionCode = baoGia.CHR_SectionCode;
                    dto.CHR_SectionName = baoGia.CHR_SectionName;   
                    dto.CHR_Phanloai = baoGia.CHR_Phanloai;
                    dto.CHR_MaThietBi = baoGia.CHR_MaThietBi;
                    dto.CHR_MaHangNoiBo = baoGia.CHR_MaHangNoiBo ?? "";
                    dto.CHR_MaHangNCC = baoGia.CHR_MaHangNCC;
                    dto.NVCHR_NameVN = baoGia.NVCHR_NameVN;
                    dto.CHR_NameEN = baoGia.CHR_NameEN;
                    dto.INT_SoLuong = baoGia.INT_SoLuong;
                    dto.NVCHR_DonVi = baoGia.NVCHR_DonVi;
                    dto.NVCHR_ChungLoai = baoGia.NVCHR_ChungLoai;
                    dto.NVCHR_HinhDang = baoGia.NVCHR_HinhDang;
                    dto.NVCHR_ChatLieu = baoGia.NVCHR_ChatLieu;
                    dto.NVCHR_ThanhPhan = baoGia.NVCHR_ThanhPhan;
                    dto.NVCHR_KichThuoc = baoGia.NVCHR_KichThuoc;
                    dto.NVCHR_DongMay = baoGia.NVCHR_DongMay;
                    dto.NVCHR_TinhNang = baoGia.NVCHR_TinhNang;
                    dto.NVCHR_Rohs = baoGia.NVCHR_Rohs;
                    dto.NVCHR_COCQ = baoGia.NVCHR_COCQ;
                    dto.NVCHR_MSDS = baoGia.NVCHR_MSDS;
                    dto.NVCHR_AnToan = baoGia.NVCHR_AnToan;
                    dto.NVCHR_FileThietKe = baoGia.NVCHR_FileThietKe;
                    dto.NVCHR_NhaSanXuat = baoGia.NVCHR_NhaSanXuat;
                    dto.CHR_MaNCC = baoGia.CHR_MaNCC;
                    dto.NVCHR_TenNCC = baoGia.NVCHR_TenNCC;
                    dto.DTM_NgayMuonNhan = baoGia.DTM_NgayMuonNhan;
                    dto.DTM_KyHan = baoGia.DTM_KyHan;
                    dto.BIT_LayBaoGia = baoGia.BIT_LayBaoGia;
                    dto.NVCHR_LyDo = baoGia.NVCHR_LyDo;
                    dto.CHR_Gap = baoGia.CHR_Gap;
                    dto.NVCHR_UserRequest = baoGia.NVCHR_UserRequest;
                    dto.DTM_UpdateLater = DateTime.Now;
                    dto.INT_SoLanUpdate = (dto.INT_SoLanUpdate ?? 0) + 1;
                    if (dto.ID_Status.Contains("RETURN"))
                    {
                        dto.ID_StepBaoGia = 2;
                        dto.ID_Status = "APPROVAL2";
                        isReturn = true;
                    }
                    listUpdate.Add(dto);
                    var history = new BaoGia_History_Request_of_Quotation
                    {
                        ID_RequestQuote = dto.ID,
                        CHR_MaDon = dto.CHR_MaDon,
                        CHR_UpdateBy = dto.CHR_CreateBy,
                        NVCHR_UpdateName = "",
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = "",
                        CHR_OldData = System.Text.Json.JsonSerializer.Serialize(listOldData.FirstOrDefault(c => c.ID == dto.ID)),
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(dto),
                        NVCHR_LyDo = "",
                        CHR_ActionType = "UPDATE"
                    };
                    listHistory.Add(history);
                    listResult.Add(baoGia.ID);
                }
            }
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(listHistory);
            _context.BaoGia_Request_of_Quotations.UpdateRange(listUpdate);
            await _context.SaveChangesAsync();
            return new UpdateHistoryResult
            {
                listUpdate = listResult,
                sectionCode = sectionCode,
                isReturn = isReturn
            };
        }
        // Get thông tin đơn phê duyệt lựa chọn ncc
        public async Task<List<dynamic>> GetSupplierApprovalInfoAsync(string maDon, string user)
        {
            var sql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT 
                    distinct
                    r.id
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                WHERE r.ID_StepBaoGia BETWEEN 9 AND 11 and r.BIT_LayBaoGia = 1");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }

            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND r.CHR_UserApproval = @User");
                parameters.Add("User", user);
            }

            sql.Append(@"
            )
            SELECT r.*,
                d.[CHR_CodeNCC],
                d.[NVCHR_NameNCC],
                d.[CHR_MaHangNCC] as CodeEquipmentNCC,
                d.[NVCHR_TenHangHQ],
                d.[NVCHR_PaymentTerm],
                d.[NVCHR_Warranty],
                d.[NVCHR_DeliveryTerm],
                d.[VCHR_Rohs],
                d.[VCHR_COCQ],
                d.[VCHR_MSDS],
                d.[VCHR_AnToan],
                d.[VCHR_CamKet],
                d.[CHR_NameEN] as NameENByNCC,
                d.[INT_SoLuong] as soluong,
                d.[NVCHR_DonVi] as donvi,
                d.[NVCHR_NhaSanXuat],
                d.[DTM_EffectiveDate],
                d.[DTM_ExpiryDate],
                d.[NVCHR_Note],
                d.[NVCHR_File],
                d.[NVCHR_MOQ],
                d.[DTM_LeadTime],
                d.[DTM_ShipTime],
                d.[NVCHR_Packing],
                d.BIT_Select,
                d.NVCHR_ReasonPick,
                d.FL_USD,
                d.FL_VND,
                CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong or d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi or d.NVCHR_DonVi is null) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
                CAST(CASE 
                    WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
                    WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'' )  THEN 1 
                    WHEN(r.NVCHR_Rohs ='') THEN 1
                    ELSE 0 
                END AS BIT) AS IsMatch_Rohs,
                CAST(CASE 
                    WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
                    WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1 
                    WHEN( R.NVCHR_COCQ ='') THEN 1
                    ELSE 0 
                END AS BIT) AS IsMatch_COCQ,

                CAST(CASE 
                    WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
                    WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1 
                    WHEN(r.NVCHR_MSDS ='') THEN 1
                    ELSE 0 
                END AS BIT) AS IsMatch_MSDS,

                CAST(CASE 
                    WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
                    WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1 
                    WHEN(r.NVCHR_AnToan ='') THEN 1
                    ELSE 0 
                END AS BIT) AS IsMatch_AnToan,
                CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) or d.DTM_ShipTime is null ) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' then 0 else 1 end as bit) As IsMatchCamKet,
                n.ShortName
				FROM BaoGia_Request_of_Quotation r
				INNER JOIN StatusCheck sc ON r.id = sc.id
				LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
				LEFT JOIN IM_NCC_NEW n ON r.CHR_MaNCC = n.Ma");

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();
            return data;
        }

        public async Task<List<dynamic>> GetExportApprovalInfoAsync(List<string> listMaDon, string adid)
        {
            var sql = new StringBuilder(@"
               WITH StatusCheck AS (
                SELECT
                    r.id,
                    r.CHR_MaDon,
                    r.CHR_MaHangNoiBo,
                    -- Lấy từng action type thành các cột riêng
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQltc,
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQltc
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                LEFT JOIN BaoGia_History_Request_of_Quotation AS h ON h.ID_RequestQuote = r.id 
                    AND h.CHR_ActionType IN ('DEFT_PICK_NCC','QLSC_PICK_NCC','QLTC_PICK_NCC')
                WHERE r.ID_StepBaoGia >= 9  and r.ID_StepBaoGia <12 and r.BIT_LayBaoGia = 1");

            var parameters = new DynamicParameters();
            if(!string.IsNullOrEmpty(adid))
            {
                sql.Append(" AND r.CHR_UserApproval = @Adid");
                parameters.Add("Adid", adid);
            }
            if (listMaDon != null && listMaDon.Any())
            {
                sql.Append(" AND r.CHR_MaDon IN @MaDonList");
                parameters.Add("MaDonList", listMaDon);
            }

            sql.Append(@"
                GROUP BY r.id, r.CHR_MaDon, r.CHR_MaHangNoiBo
            )
            SELECT 
                r.*,
                d.[CHR_CodeNCC],
                d.[NVCHR_NameNCC],
                d.[CHR_MaHangNCC] AS CodeEquipmentNCC,
                d.[NVCHR_TenHangHQ],
                d.[NVCHR_PaymentTerm],
                d.[NVCHR_Warranty],
                d.[NVCHR_DeliveryTerm],
                d.[VCHR_Rohs],
                d.[VCHR_COCQ],
                d.[VCHR_MSDS],
                d.[VCHR_AnToan],
                d.[VCHR_CamKet],
                d.[CHR_NameEN] AS NameENByNCC,
                d.[INT_SoLuong] AS soluong,
                d.[NVCHR_DonVi] AS donvi,
                d.[NVCHR_NhaSanXuat],
                d.[DTM_EffectiveDate],
                d.[DTM_ExpiryDate],
                d.[NVCHR_Note],
                d.[NVCHR_File],
                d.[NVCHR_MOQ],
                d.[DTM_LeadTime],
                d.[DTM_ShipTime],
                d.[NVCHR_Packing],
                d.BIT_Select,
                d.NVCHR_ReasonPick,
                d.FL_USD,
                d.FL_VND,
                CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong OR d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi OR d.NVCHR_DonVi IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
                CAST(CASE
                    WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
                    WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'') THEN 1
                    WHEN (r.NVCHR_Rohs = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_Rohs,
                CAST(CASE
                    WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
                    WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1
                    WHEN (r.NVCHR_COCQ = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_COCQ,
                CAST(CASE
                    WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
                    WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1
                    WHEN (r.NVCHR_MSDS = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_MSDS,
                CAST(CASE
                    WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
                    WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1
                    WHEN (r.NVCHR_AnToan = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_AnToan,
                CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) OR d.DTM_ShipTime IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' THEN 0 ELSE 1 END AS BIT) AS IsMatchCamKet,
        
                -- Lấy từ CTE đã gộp
                sc.UserDeft,
                sc.UserQlsc,
                sc.UserQltc,
                sc.LyDoDeft,
                sc.LyDoQlsc,
                sc.LyDoQltc,
                n.ShortName
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            LEFT JOIN StatusCheck sc ON r.id = sc.id
            left Join IM_NCC_NEW as n on r.CHR_MaNCC = n.Ma
            WHERE r.ID_StepBaoGia >= 9  and r.ID_StepBaoGia <12 and r.BIT_LayBaoGia = 1");

            if (!string.IsNullOrEmpty(adid))
            {
                sql.Append(" AND r.CHR_UserApproval = @Adid");
                parameters.Add("Adid", adid);
            }
            if (listMaDon != null && listMaDon.Any())
            {
                sql.Append(" AND r.CHR_MaDon IN @MaDonList");
                parameters.Add("MaDonList", listMaDon);
            }
            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaDon, r.CHR_MaThietBi, r.CHR_MaNCC, r.CHR_MaHangNoiBo, r.NVCHR_NameVN");
            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();
            return data;
        }
        // Phê duyệt thông tin lựa chọn nhà cung cấp
        public async Task<List<BaoGia_Request_of_Quotation>> UpdateApprovarOK(string maDon, string userNext,string userUpdate)
        {
            if (string.IsNullOrEmpty(maDon))
            {
                throw new ArgumentNullException("Please select Reqeust Code");
            }
            var history = new List<BaoGia_History_Request_of_Quotation>();
            var data = await _context.BaoGia_Request_of_Quotations.Where(c => c.CHR_MaDon == maDon && c.ID_StepBaoGia >= 9 && c.ID_StepBaoGia <= 11).ToListAsync();
            foreach (var item in data)
            {
                item.ID_StepBaoGia = item.ID_StepBaoGia + 1;
                item.CHR_UserApproval = userNext;
                item.DTM_UpdateLater = DateTime.Now;
            
                var h = new BaoGia_History_Request_of_Quotation
                {
                    ID_RequestQuote = item.ID,
                    CHR_MaDon = item.CHR_MaDon ?? string.Empty,
                    CHR_UpdateBy = userUpdate,
                    NVCHR_UpdateName = userUpdate,
                    CHR_Updatedate = DateTime.Now,
                    CHR_ChangedColumns = null,
                    CHR_OldData = null,
                    CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                    NVCHR_LyDo = "",
                    CHR_ActionType = item.ID_StepBaoGia == 10 ? "QLSC_PICK_NCC" : (item.ID_StepBaoGia == 11 ? "QLTC_PICK_NCC" : "DEFT_PICK_NCC")
                };
                history.Add(h);
            }
            if (history.Any())
            {
                await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(history);
            }
            await _context.SaveChangesAsync();
            return data;
        }
        public async Task<List<BaoGia_Request_of_Quotation>> UpdateApprovarNG(string maDon, string Reason, string userUpdate)
        {
            if (string.IsNullOrEmpty(maDon))
            {
                throw new ArgumentNullException("Please select Reqeust Code");
            }
            var history = new List<BaoGia_History_Request_of_Quotation>();
            var data = await _context.BaoGia_Request_of_Quotations.Where(c => c.CHR_MaDon == maDon && c.ID_StepBaoGia >= 9 && c.ID_StepBaoGia <= 11).ToListAsync();
            foreach (var item in data)
            {
                if(item.ID_StepBaoGia == 9)
                {
                    item.ID_Status = "RETURN_QLSC_AFTER";
                }else if (item.ID_StepBaoGia == 10)
                {
                    item.ID_Status = "RETURN_QLTC_AFTER";
                }
                else
                {
                    item.ID_Status = "RETURN_TBP";
                }
                item.ID_StepBaoGia = 8;
                item.NVCHR_LyDo = Reason;
                item.DTM_UpdateLater = DateTime.Now;
                var h = new BaoGia_History_Request_of_Quotation
                {
                    ID_RequestQuote = item.ID,
                    CHR_MaDon = item.CHR_MaDon ?? string.Empty,
                    CHR_UpdateBy = userUpdate,
                    NVCHR_UpdateName = userUpdate,
                    CHR_Updatedate = DateTime.Now,
                    CHR_ChangedColumns = null,
                    CHR_OldData = null,
                    CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                    NVCHR_LyDo = Reason,
                    CHR_ActionType = item.ID_Status
                };
                history.Add(h);
            }
            if (history.Any())
            {
                await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(history);
            }
            await _context.SaveChangesAsync();
            return data;
        }
        // Search thông tin đơn báo giá đã hoàn thành lựa chọn nhà cung cấp
        public async Task<ListRequest<dynamic>> SearchRequestDone(string? maDon, string? section, string? maHang, string? maNCC, string user, int pageIndex, int pageSize)
        {
            var parameters = new DynamicParameters();
            var whereConditions = new List<string>
            {
                "r.BIT_LayBaoGia = 1"
            };

            if (!string.IsNullOrEmpty(user))
            {
                whereConditions.Add("EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @User)");
                parameters.Add("User", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                whereConditions.Add("r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                whereConditions.Add("r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }
            if (!string.IsNullOrEmpty(section))
            {
                whereConditions.Add("r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }
            if (!string.IsNullOrEmpty(maNCC))
            {
                whereConditions.Add("r.CHR_MaNCC = @MaNCC");
                parameters.Add("MaNCC", maNCC);
            }

            var whereClause = string.Join(" AND ", whereConditions);

            var sql = new StringBuilder(@"
            WITH RequestCompleted AS
            (
                SELECT
                    CHR_MaDon,
                    CHR_MaHangNoiBo
                FROM BaoGia_Request_of_Quotation
                WHERE BIT_LayBaoGia = 1
                GROUP BY
                    CHR_MaDon,
                    CHR_MaHangNoiBo
                HAVING MIN(ID_StepBaoGia) > 12
            )
            SELECT
                r.CHR_MaDon,
                r.CHR_SectionName AS PhongYeuCau,
                r.CHR_MaHangNoiBo,
                MAX(r.NVCHR_NameVN) AS TenHang,
                MAX(r.CHR_NameEN) AS TenHangEN,
                MAX(r.NVCHR_DonVi) AS DonVi,
                MAX(r.NVCHR_UserRequest) AS NguoiYeuCau,
                MAX(r.DTM_NgayMuonNhan) AS DTM_NgayMuonNhan,
                N'Done' AS TrangThai
            FROM BaoGia_Request_of_Quotation r
            INNER JOIN RequestCompleted rc
                ON rc.CHR_MaDon = r.CHR_MaDon
                AND rc.CHR_MaHangNoiBo = r.CHR_MaHangNoiBo
            LEFT JOIN BaoGia_Detail_of_Quotation d
                ON d.ID_RequestQuote = r.id
                AND d.BIT_Select = 1
            WHERE " + whereClause + @"
            GROUP BY
                r.CHR_MaDon,
                r.CHR_SectionName,
                r.CHR_MaHangNoiBo
            ORDER BY
                MAX(r.DTM_CreateDate) DESC");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            var countSql = @"
                WITH RequestCompleted AS
                (
                    SELECT
                        CHR_MaDon,
                        CHR_MaHangNoiBo
                    FROM BaoGia_Request_of_Quotation
                    WHERE BIT_LayBaoGia = 1
                    GROUP BY
                        CHR_MaDon,
                        CHR_MaHangNoiBo
                    HAVING MIN(ID_StepBaoGia) > 12
                )
                SELECT COUNT(*)
                FROM
                (
                    SELECT
                        r.CHR_MaDon,
                        r.CHR_SectionName,
                        r.CHR_MaHangNoiBo
                    FROM BaoGia_Request_of_Quotation r
                    INNER JOIN RequestCompleted rc
                        ON rc.CHR_MaDon = r.CHR_MaDon
                        AND rc.CHR_MaHangNoiBo = r.CHR_MaHangNoiBo
                    LEFT JOIN BaoGia_Detail_of_Quotation d
                        ON d.ID_RequestQuote = r.id
                        AND d.BIT_Select = 1
                    WHERE " + whereClause + @"
                    GROUP BY
                        r.CHR_MaDon,
                        r.CHR_SectionName,
                        r.CHR_MaHangNoiBo
                ) t;";

            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql, parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        public async Task<List<dynamic>> SearchRequestDoneDetail(string? maDon, string? maHang, string? user)
        {
            var sql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT
                    r.id,
                    r.CHR_MaDon,
                    r.CHR_MaHangNoiBo,
                    -- Lấy từng action type thành các cột riêng
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQltc,
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQltc
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_History_Request_of_Quotation AS h ON h.ID_RequestQuote = r.id 
                    AND h.CHR_ActionType IN ('DEFT_PICK_NCC','QLSC_PICK_NCC','QLTC_PICK_NCC')
                WHERE r.ID_StepBaoGia > 12 AND r.BIT_LayBaoGia = 1");

            var parameters = new DynamicParameters();

            // Thêm điều kiện lọc vào CTE StatusCheck
            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @User)");
                parameters.Add("User", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }

            sql.Append(@"
            GROUP BY r.id, r.CHR_MaDon, r.CHR_MaHangNoiBo
            )
            SELECT 
                r.*,
                d.[CHR_CodeNCC],
                d.[NVCHR_NameNCC],
                d.[CHR_MaHangNCC] AS CodeEquipmentNCC,
                d.[NVCHR_TenHangHQ],
                d.[NVCHR_PaymentTerm],
                d.[NVCHR_Warranty],
                d.[NVCHR_DeliveryTerm],
                d.[VCHR_Rohs],
                d.[VCHR_COCQ],
                d.[VCHR_MSDS],
                d.[VCHR_AnToan],
                d.[VCHR_CamKet],
                d.[CHR_NameEN] AS NameENByNCC,
                d.[INT_SoLuong] AS soluong,
                d.[NVCHR_DonVi] AS donvi,
                d.[NVCHR_NhaSanXuat],
                d.[DTM_EffectiveDate],
                d.[DTM_ExpiryDate],
                d.[NVCHR_Note],
                d.[NVCHR_File],
                d.[NVCHR_MOQ],
                d.[DTM_LeadTime],
                d.[DTM_ShipTime],
                d.[NVCHR_Packing],
                d.BIT_Select,
                d.NVCHR_ReasonPick,
                d.FL_USD,
                d.FL_VND,
                CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong OR d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi OR d.NVCHR_DonVi IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
                CAST(CASE
                    WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
                    WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'') THEN 1
                    WHEN (r.NVCHR_Rohs = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_Rohs,
                CAST(CASE
                    WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
                    WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1
                    WHEN (r.NVCHR_COCQ = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_COCQ,
                CAST(CASE
                    WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
                    WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1
                    WHEN (r.NVCHR_MSDS = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_MSDS,
                CAST(CASE
                    WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
                    WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1
                    WHEN (r.NVCHR_AnToan = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_AnToan,
                CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) OR d.DTM_ShipTime IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' THEN 0 ELSE 1 END AS BIT) AS IsMatchCamKet,
        
                -- Lấy từ CTE đã gộp
                sc.UserDeft,
                sc.UserQlsc,
                sc.UserQltc,
                sc.LyDoDeft,
                sc.LyDoQlsc,
                sc.LyDoQltc
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            INNER JOIN StatusCheck sc ON r.id = sc.id
            WHERE r.ID_StepBaoGia > 12");

            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaDon, r.CHR_MaThietBi, r.CHR_MaNCC, r.CHR_MaHangNoiBo, r.NVCHR_NameVN");

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            return data;
        }
        // update người phê duyệt cho đơn
        public async Task<List<BaoGia_Request_of_Quotation>> UpdateUserApprovalHistory(UpdateHistoryResult update)
        {
            if (update == null || update.listUpdate == null || update.listUpdate.Count == 0 || string.IsNullOrEmpty(update.sectionCode))
            {
                throw new Exception("Data error");
            }

            var updatedRecords = new List<BaoGia_Request_of_Quotation>();

            foreach (var id in update.listUpdate)
            {
                var data = await _context.BaoGia_Request_of_Quotations
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (data != null)
                {
                    data.CHR_UserApproval = update.sectionCode;
                    updatedRecords.Add(data);
                }
            }

            if (updatedRecords.Any())
            {
                await _context.SaveChangesAsync();
            }

            return updatedRecords;
        }
        // update ma hang noi bo
        public async Task<bool> UpdateCodeMaterialBIVN(List<ConfirmNameDTO> list)
        {
            if (!list.Any())
            {
                throw new Exception("Error List update");
            }

            // Lọc bỏ các item có Id null hoặc MaHangNoiBo null/empty
            var validList = list.Where(x => x.Id > 0 && !string.IsNullOrEmpty(x.MaHangNoiBo)).ToList();

            if (!validList.Any())
            {
                throw new Exception("No valid data to update");
            }

            var ids = validList.Select(x => x.Id).ToList();

            var requests = await _context.BaoGia_Request_of_Quotations
                .Where(x => ids.Contains(x.ID))
                .ToListAsync();

            var updateDict = validList.ToDictionary(x => x.Id, x => x.MaHangNoiBo);

            foreach (var request in requests)
            {
                if (updateDict.TryGetValue(request.ID, out var maHangNoiBo))
                {
                    request.CHR_MaHangNoiBo = maHangNoiBo;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        // Phê duyệt list lựa chọn nhà cung cấp
        public async Task<List<BaoGia_Request_of_Quotation>> UpdateApprover(List<ApproverDTO> dataApprovers, string userNext, string userUpdate)
        {
            // Kiểm tra tham số đầu vào
            if (dataApprovers == null || !dataApprovers.Any())
            {
                throw new ArgumentNullException(nameof(dataApprovers), "No approval data provided");
            }

            // Lấy tất cả ID cần xử lý
            var ids = dataApprovers.Select(a => a.Id).Distinct().ToList();

            // Load tất cả dữ liệu cần xử lý trong 1 query duy nhất
            var existingData = await _context.BaoGia_Request_of_Quotations
                .Where(c => ids.Contains(c.ID) && c.ID_StepBaoGia >= 9 && c.ID_StepBaoGia <= 11)
                .ToDictionaryAsync(c => c.ID, c => c);

            var historyList = new List<BaoGia_History_Request_of_Quotation>();
            var updatedEntities = new List<BaoGia_Request_of_Quotation>();

            foreach (var item in dataApprovers)
            {
                if (!existingData.TryGetValue(item.Id, out var data)) continue;

                if (data.ID_StepBaoGia < 7)
                {
                    continue;
                }

                if (item.IsApproved != false)
                {
                    data.ID_StepBaoGia++;
                    data.CHR_UserApproval = userNext;
                    data.DTM_UpdateLater = DateTime.Now;
                    if (data.ID_StepBaoGia >= 12)
                    {
                        data.ID_Status = "DONE";
                        data.ID_StepBaoGia = 13;
                    }

                    var actionType = data.ID_StepBaoGia == 10 ? "QLSC_PICK_NCC" :
                                    (data.ID_StepBaoGia == 11 ? "QLTC_PICK_NCC" : "DEFT_PICK_NCC");
                    historyList.Add(new BaoGia_History_Request_of_Quotation
                    {
                        ID_RequestQuote = item.Id,
                        CHR_MaDon = data.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = userUpdate,
                        NVCHR_UpdateName = userUpdate,
                        CHR_Updatedate = DateTime.Now,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                        CHR_ActionType = actionType
                    });
                    updatedEntities.Add(data);
                }
                else
                {
                    data.ID_Status = data.ID_StepBaoGia == 9 ? "RETURN_QLSC_AFTER" :
                                    (data.ID_StepBaoGia == 10 ? "RETURN_QLTC_AFTER" : "RETURN_TBP");
                    data.ID_StepBaoGia = 8;
                    data.NVCHR_LyDo = item.Reason;
                    data.DTM_UpdateLater = DateTime.Now;

                    historyList.Add(new BaoGia_History_Request_of_Quotation
                    {
                        ID_RequestQuote = data.ID,
                        CHR_MaDon = data.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = userUpdate,
                        NVCHR_UpdateName = userUpdate,
                        CHR_Updatedate = DateTime.Now,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                        NVCHR_LyDo = item.Reason,
                        CHR_ActionType = data.ID_Status
                    });
                    updatedEntities.Add(data);
                }
            }

            // Lưu thay đổi
            if (updatedEntities.Any())
            {
                _context.BaoGia_Request_of_Quotations.UpdateRange(updatedEntities);
            }

            if (historyList.Any())
            {
                await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
            }

            await _context.SaveChangesAsync();

            return updatedEntities;
        }
        // Xóa đơn xin báo giá
        public async Task<bool> DeleteDonXinBaoGiaAsync(string maDon, string reason, string userUpdate,string role)
        {
            if (string.IsNullOrEmpty(maDon))  throw new Exception("No valid data to update");

            var data = await _context.BaoGia_Request_of_Quotations.Where(c => c.CHR_MaDon == maDon).ToListAsync();
            if (!data.Any()) throw new Exception("No valid data to update");
            // kiểm tra nếu đơn đã được phê duyệt thì không cho xóa
            var isApproved = data.Any(d => d.ID_StepBaoGia > 5);
            if (isApproved && role != "UserPUR")
            {
                throw new Exception("Đơn đã được phê duyệt, không thể xóa. Vui lòng liên hệ PIC PUR để được hỗ trợ");
            }
            // xoa mem
            foreach (var item in data)
            {
                item.ID_Status  = "DELETE";
                item.ID_StepBaoGia = 0;
            }
            // Lưu lịch sử xóa
            var historyList = data.Select(item => new BaoGia_History_Request_of_Quotation
            {
                ID_RequestQuote = item.ID,
                CHR_MaDon = item.CHR_MaDon ?? string.Empty,
                CHR_UpdateBy = userUpdate,
                NVCHR_UpdateName = userUpdate,
                CHR_Updatedate = DateTime.Now,
                CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                NVCHR_LyDo = reason,
                CHR_ActionType = "DELETE"
            }).ToList();
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
            _context.BaoGia_Request_of_Quotations.UpdateRange(data);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa từng đơn
        public async Task<bool> DeleteDonBaoGiaAsync(int id, string reason, string userUpdate)
        {
            var data = await _context.BaoGia_Request_of_Quotations.FirstOrDefaultAsync(c => c.ID == id);
            if (data == null) throw new Exception("No valid data to update");
            // kiểm tra nếu đơn đã được phê duyệt thì không cho xóa
            if (data.ID_StepBaoGia > 5)
            {
               throw new Exception("Đơn đã được phê duyệt, không thể xóa. Vui lòng liên hệ PIC PUR để được hỗ trợ");
            }
            // xoa mem
            data.ID_Status = "DELETE";
            data.ID_StepBaoGia = 0;
            // Lưu lịch sử xóa
            var history = new BaoGia_History_Request_of_Quotation
            {
                ID_RequestQuote = data.ID,
                CHR_MaDon = data.CHR_MaDon ?? string.Empty,
                CHR_UpdateBy = userUpdate,
                NVCHR_UpdateName = userUpdate,
                CHR_Updatedate = DateTime.Now,
                CHR_NewData = System.Text.Json.JsonSerializer.Serialize(data),
                NVCHR_LyDo = reason,
                CHR_ActionType = "DELETE"
            };
            await _context.BaoGia_History_Request_of_Quotations.AddAsync(history);
            _context.BaoGia_Request_of_Quotations.Update(data);
            await _context.SaveChangesAsync();
            return true;
        }
        // Trả lại đơn báo giá
        public async Task<List<BaoGia_Request_of_Quotation>> TraLaiDonBaoGiaAsync(string maDon, string userUpdate, string reason)
        {
            if (string.IsNullOrEmpty(maDon)) throw new Exception("No valid data to update");

            var data = await _context.BaoGia_Request_of_Quotations.Where(c => c.CHR_MaDon == maDon).ToListAsync();

            if (!data.Any()) throw new Exception("Mã đơn báo giá không hợp lệ");

            foreach (var item in data)
            {
                item.ID_StepBaoGia = 1;
                item.ID_Status = "RETURN_PIC";
                item.DTM_UpdateLater = DateTime.Now;
            }
            // Lưu lịch sử trả lại
            var historyList = data.Select(item => new BaoGia_History_Request_of_Quotation
            {
                ID_RequestQuote = item.ID,
                CHR_MaDon = item.CHR_MaDon ?? string.Empty,
                CHR_UpdateBy = userUpdate,
                NVCHR_UpdateName = userUpdate,
                CHR_Updatedate = DateTime.Now,
                NVCHR_LyDo = reason,
                CHR_NewData = System.Text.Json.JsonSerializer.Serialize(item),
                CHR_ActionType = "RETURN_PIC"
            }).ToList();
            _context.BaoGia_Request_of_Quotations.UpdateRange(data);
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return data;
        }
        // lấy danh sách đơn yêu cầu hàng hóa
        public async Task<List<string>> GetMaDonYeuCauHangHoaAsync()
        {
              var data = await _context.BaoGia_Request_of_Quotations
                .Where(c => c.ID_StepBaoGia == 13 && c.BIT_LayBaoGia == true)
                .Select(c => c.CHR_MaDon)
                .Distinct()
                .ToListAsync();
            return data;
        }
        // update phê duyệt đơn báo giá
        public async Task<List<BaoGia_Request_of_Quotation>> UpdatePheDuyetDonBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias)
        {
            if (baoGias == null || !baoGias.Any())
            {
                throw new ArgumentNullException(nameof(baoGias), "No data to update");
            }

            var ids = baoGias.Select(b => b.ID).Where(id => id > 0).Distinct().ToList();
            if (!ids.Any())
            {
                throw new ArgumentException("No valid IDs provided in the input list.");
            }

            var existingDict = await _context.BaoGia_Request_of_Quotations
                .Where(c => ids.Contains(c.ID))
                .ToDictionaryAsync(c => c.ID);

            var now = DateTime.Now;
            foreach (var item in baoGias)
            {
                if (existingDict.TryGetValue(item.ID, out var data))
                {
                    data.ID_StepBaoGia = item.ID_StepBaoGia;
                    data.ID_Status = item.ID_Status;
                    data.CHR_UserApproval = item.CHR_UserApproval;
                    data.DTM_UpdateLater = now;
                    item.CHR_CreateBy = data.CHR_CreateBy;
                    if(data.BIT_LayBaoGia == false)
                    {
                        data.ID_Status = "NOT_QUOTATION";
                    }
                }
            }
            await _context.SaveChangesAsync();

            return baoGias;
        }
        // Check đơn return
        public async Task<bool> CheckDonReturnAsync(List<string> maDons)
        {
            if (maDons == null || !maDons.Any())
            {
                throw new ArgumentNullException(nameof(maDons), "No data to check");
            }

            var result = await _context.BaoGia_Request_of_Quotations
                .Where(c => maDons.Contains(c.CHR_MaDon))
                .GroupBy(c => c.CHR_MaDon)
                .Select(g => g.Max(x => x.ID_StepBaoGia))
                .AllAsync(maxStep => maxStep < 5);

            return result;
        }
        // Export history báo giá
        public async Task<List<dynamic>> ExportHistoryBaoGiaAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang,
            string? status, int? step, string? user, string? chungLoai, DateTime? to, DateTime? from)
        {
            var sql = @"
                WITH FilteredRequest AS (
                    SELECT q.*
                    FROM BaoGia_Request_of_Quotation q
            ";

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

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
                whereClauses.Add("EXISTS ( SELECT 1  FROM BaoGia_Master_Approver_Send_Mail s " +
                    "WHERE s.CHR_CodeSection = q.CHR_SectionCode and s.CHR_UserAdid = @Adid )");
                parameters.Add("Adid", user);
            }
            if (from.HasValue)
            {
                whereClauses.Add("q.DTM_CreateDate >= @From");
                parameters.Add("From", from.Value);
            }
            if (to.HasValue)
            {
                whereClauses.Add("q.DTM_CreateDate <= @To");
                parameters.Add("To", to.Value);
            }
            // status handling
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
                ),
                LatestDetail AS (
                    SELECT
                        d.ID_RequestQuote,
                        d.BIT_Select,
                        d.NVCHR_ReasonPick,
                        d.NVCHR_File,
                        ROW_NUMBER() OVER (PARTITION BY d.ID_RequestQuote ORDER BY d.ID DESC) AS rn
                    FROM BaoGia_Detail_of_Quotation d
                    INNER JOIN FilteredRequest q ON q.ID = d.ID_RequestQuote
                )
                SELECT
                    q.*, 
                    d.BIT_Select, 
                    d.NVCHR_ReasonPick, 
                    d.NVCHR_File,
                    v.ShortName
                FROM FilteredRequest q
                LEFT JOIN LatestDetail d ON d.ID_RequestQuote = q.ID AND d.rn = 1
                LEFT JOIN IM_NCC_NEW v ON q.CHR_MaNCC = v.Ma
                ORDER BY q.CHR_MaDon DESC";

            var data = (await _conn.QueryAsync<dynamic>(sql, parameters)).ToList();
            return data;
        }
        // update thời hạn lựa chọn nhà cung cấp
        public async Task<List<BaoGia_Request_of_Quotation>> UpdateDeadlineAsync(List<BaoGia_Request_of_Quotation> baoGias)
        {
            if (baoGias == null || !baoGias.Any())
            {
                throw new ArgumentNullException(nameof(baoGias), "No data to update");
            }
            // danh sach id cần update
            var ids = baoGias.Select(b => b.ID).Where(id => id > 0).Distinct().ToList();
            if (!ids.Any())
            {
                throw new ArgumentException("No valid IDs provided in the input list.");
            }

            var rq = await _context.BaoGia_Request_of_Quotations
                .Where(c => ids.Contains(c.ID))
                .ToListAsync();

            if (!rq.Any())
            {
                throw new ArgumentException("No data to update");
            }

            var now = DateTime.Now;
            foreach (var item in rq)
            {
                // Chỉ cập nhật khi đang ở bước 4 và có kỳ hạn
                if (item.ID_StepBaoGia == 4 && item.DTM_KyHan.HasValue)
                {
                    // Tính số ngày còn lại (dùng Date để bỏ phần giờ)
                    var daysLeft = (item.DTM_KyHan.Value.Date - now.Date).TotalDays;
                    if (daysLeft <= 5)
                    {
                        item.DTM_KyHan = item.DTM_KyHan.Value.AddDays(5);
                        item.DTM_UpdateLater = now;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Đồng bộ lại giá trị DTM_KyHan/DTM_UpdateLater cho danh sách đầu vào
            var rqDict = rq.ToDictionary(r => r.ID);
            foreach (var b in baoGias)
            {
                if (rqDict.TryGetValue(b.ID, out var updated))
                {
                    b.DTM_KyHan = updated.DTM_KyHan;
                    b.DTM_UpdateLater = updated.DTM_UpdateLater;
                }
            }
            return baoGias;
        }

        // Lấy danh sách NCC k cần xác nhận tên hàng
         public async Task<List<string>> GetListNccNotConfirmNameAsync()
        {
            var sql = "SELECT CHR_MaNcc FROM BaoGia_Vender_NotConfirm WHERE CHR_Status = 'ON'";
            var result = await _conn.QueryAsync<string>(sql);
            return result.ToList();
        }
        // kiểm tra đơn + mã hàng đã được quyền lựa chọn nhà cung cấp hay chưa
        public async Task<List<BaoGiaImportModel>> CheckPermissionSelectSupplierAsync(
            List<BaoGiaImportModel> baoGiaImportModels)
        {
            var dataCheck = baoGiaImportModels
                .Select(x => $"{x.MaDon}|{x.MaHangNoiBo}")
                .ToHashSet();

            var invalidKeys = await _context.BaoGia_Request_of_Quotations
                .Where(x => x.ID_StepBaoGia < 7 && x.BIT_LayBaoGia == true)
                .Select(x => new
                {
                    x.CHR_MaDon,
                    x.CHR_MaHangNoiBo
                })
                .ToListAsync();

            var invalidSet = invalidKeys
                .Where(x => dataCheck.Contains($"{x.CHR_MaDon}|{x.CHR_MaHangNoiBo}"))
                .Select(x => $"{x.CHR_MaDon}|{x.CHR_MaHangNoiBo}")
                .ToHashSet();

            return baoGiaImportModels
                .Where(x => invalidSet.Contains($"{x.MaDon}|{x.MaHangNoiBo}"))
                .ToList();
        }

        // check trạng thái đơn báo giá
        public async Task<List<int>> CheckStepAsync(List<int> ids, List<int> stepCheck)
        {
            var result = await _context.BaoGia_Request_of_Quotations
                .Where(x => ids.Contains(x.ID) && !stepCheck.Contains((int)x.ID_StepBaoGia))
                .Select(x => x.ID)
                .ToListAsync();
            return result;
        }
        // check infor báo giá theo mã đơn, mã hàng, mã ncc
        public async Task<List<BaoGia_Request_of_Quotation>> GetOrderInfoAsync(string? maDon, string? maHangNCC, string? maHangNB, string? NameEn)
        {
            var result = await _context.BaoGia_Request_of_Quotations
                .Where(x =>
                    (string.IsNullOrEmpty(maDon) || x.CHR_MaDon == maDon) &&
                    (string.IsNullOrEmpty(maHangNB) || x.CHR_MaHangNoiBo == maHangNB) &&
                    (string.IsNullOrEmpty(NameEn) || x.CHR_NameEN == NameEn))
                .OrderBy(x => x.CHR_MaNCC)
                .ThenBy(x => x.ID)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }
        // Export Excel báo giá Done
        public async Task<List<dynamic>> ExportExcelBaoGiaDoneAsync(string? maDon, string? section, string? maHang, string? maNCC, string user)
        {
            var sql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT
                    r.id,
                    r.CHR_MaDon,
                    r.CHR_MaHangNoiBo,
                    -- Lấy từng action type thành các cột riêng
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.CHR_UpdateBy END) AS UserQltc,
                    MAX(CASE WHEN h.CHR_ActionType = 'DEFT_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoDeft,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLSC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQlsc,
                    MAX(CASE WHEN h.CHR_ActionType = 'QLTC_PICK_NCC' THEN h.NVCHR_LyDo END) AS LyDoQltc
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                LEFT JOIN BaoGia_History_Request_of_Quotation AS h ON h.ID_RequestQuote = r.id 
                    AND h.CHR_ActionType IN ('DEFT_PICK_NCC','QLSC_PICK_NCC','QLTC_PICK_NCC')
                WHERE r.ID_StepBaoGia > 12 AND r.BIT_LayBaoGia = 1");

            var parameters = new DynamicParameters();

            // Thêm điều kiện lọc vào CTE StatusCheck
            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND s.CHR_UserAdid = @User");
                parameters.Add("User", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }
            if (!string.IsNullOrEmpty(section))
            {
                sql.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }
            if (!string.IsNullOrEmpty(maNCC))
            {
                sql.Append(" AND r.CHR_MaNCC = @MaNCC");
                parameters.Add("MaNCC", maNCC);
            }

            sql.Append(@"
            GROUP BY r.id, r.CHR_MaDon, r.CHR_MaHangNoiBo
            )
            SELECT 
                r.*,
                d.[CHR_CodeNCC],
                d.[NVCHR_NameNCC],
                d.[CHR_MaHangNCC] AS CodeEquipmentNCC,
                d.[NVCHR_TenHangHQ],
                d.[NVCHR_PaymentTerm],
                d.[NVCHR_Warranty],
                d.[NVCHR_DeliveryTerm],
                d.[VCHR_Rohs],
                d.[VCHR_COCQ],
                d.[VCHR_MSDS],
                d.[VCHR_AnToan],
                d.[VCHR_CamKet],
                d.[CHR_NameEN] AS NameENByNCC,
                d.[INT_SoLuong] AS soluong,
                d.[NVCHR_DonVi] AS donvi,
                d.[NVCHR_NhaSanXuat],
                d.[DTM_EffectiveDate],
                d.[DTM_ExpiryDate],
                d.[NVCHR_Note],
                d.[NVCHR_File],
                d.[NVCHR_MOQ],
                d.[DTM_LeadTime],
                d.[DTM_ShipTime],
                d.[NVCHR_Packing],
                d.BIT_Select,
                d.NVCHR_ReasonPick,
                d.FL_USD,
                d.FL_VND,
                CAST(CASE WHEN r.CHR_MaHangNCC = d.CHR_MaHangNCC THEN 1 ELSE 0 END AS BIT) AS IsMatch_MaHangNCC,
                CAST(CASE WHEN r.NVCHR_NameVN = d.NVCHR_TenHangHQ THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameVN,
                CAST(CASE WHEN r.CHR_NameEN = d.CHR_NameEN THEN 1 ELSE 0 END AS BIT) AS IsMatch_NameEN,
                CAST(CASE WHEN (r.INT_SoLuong = d.INT_SoLuong OR d.INT_SoLuong = 0) THEN 1 ELSE 0 END AS BIT) AS IsMatch_SoLuong,
                CAST(CASE WHEN (r.NVCHR_DonVi = d.NVCHR_DonVi OR d.NVCHR_DonVi IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_DonVi,
                CAST(CASE
                    WHEN r.NVCHR_Rohs = N'Need' AND (d.VCHR_Rohs = N'NG' OR d.VCHR_Rohs = N'No need') THEN 0
                    WHEN (r.NVCHR_Rohs = d.VCHR_Rohs OR d.VCHR_Rohs = N'OK' OR d.VCHR_Rohs = N'') THEN 1
                    WHEN (r.NVCHR_Rohs = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_Rohs,
                CAST(CASE
                    WHEN r.NVCHR_COCQ = N'Need' AND (d.VCHR_COCQ = N'NG' OR d.VCHR_COCQ = N'No need') THEN 0
                    WHEN (r.NVCHR_COCQ = d.VCHR_COCQ OR d.VCHR_COCQ = N'OK' OR d.VCHR_COCQ = N'') THEN 1
                    WHEN (r.NVCHR_COCQ = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_COCQ,
                CAST(CASE
                    WHEN r.NVCHR_MSDS = N'Need' AND (d.VCHR_MSDS = N'NG' OR d.VCHR_MSDS = N'No need') THEN 0
                    WHEN (r.NVCHR_MSDS = d.VCHR_MSDS OR d.VCHR_MSDS = N'OK' OR d.VCHR_MSDS = N'') THEN 1
                    WHEN (r.NVCHR_MSDS = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_MSDS,
                CAST(CASE
                    WHEN r.NVCHR_AnToan = N'Need' AND (d.VCHR_AnToan = N'NG' OR d.VCHR_AnToan = N'No need') THEN 0
                    WHEN (r.NVCHR_AnToan = d.VCHR_AnToan OR d.VCHR_AnToan = N'OK' OR d.VCHR_AnToan = N'') THEN 1
                    WHEN (r.NVCHR_AnToan = '') THEN 1
                    ELSE 0
                END AS BIT) AS IsMatch_AnToan,
                CAST(CASE WHEN (CAST(r.DTM_NgayMuonNhan AS DATE) = CAST(d.DTM_ShipTime AS DATE) OR d.DTM_ShipTime IS NULL) THEN 1 ELSE 0 END AS BIT) AS IsMatch_Ngay,
                CAST(CASE WHEN d.VCHR_CamKet != N'Đồng ý (accept)' THEN 0 ELSE 1 END AS BIT) AS IsMatchCamKet,
        
                -- Lấy từ CTE đã gộp
                sc.UserDeft,
                sc.UserQlsc,
                sc.UserQltc,
                sc.LyDoDeft,
                sc.LyDoQlsc,
                sc.LyDoQltc
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            INNER JOIN StatusCheck sc ON r.id = sc.id
            WHERE r.ID_StepBaoGia > 12");

            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaDon, r.CHR_MaThietBi, r.CHR_MaNCC, r.CHR_MaHangNoiBo, r.NVCHR_NameVN");

        

            var a = sql.ToString();
            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            return data;
        }
        // Search báo giá còn hiệu lực
        public async Task<ListRequest<dynamic>> SearchBaoGiaConHieuLucAsync(SearchQuotationResultsModel search, string? user)
        {
            var sql = new StringBuilder(@"
                SELECT r.NVCHR_NameVN as TenHang, r.CHR_NameEN as TenHangEN, r.CHR_MaDon, r.CHR_MaHangNoiBo, n.ShortName,d.*
                  FROM BaoGia_Detail_of_Quotation as d 
                  left join BaoGia_Request_of_Quotation as r
                  on d.ID_RequestQuote = r.id
                  left join IM_NCC_NEW as n 
                  on d.CHR_CodeNCC = n.Ma
                  where r.ID_StepBaoGia = 13 
                  and DTM_ExpiryDate > GETDATE()
                  and BIT_Select = 1
            ");
            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(search.MaDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", search.MaDon);
            }
            if (!string.IsNullOrEmpty(search.MaNcc))
            {
                sql.Append(" AND r.CHR_MaNCC LIKE @MaNcc");
                parameters.Add("MaNcc", "%" + search.MaNcc + "%");
            }
            if (!string.IsNullOrEmpty(search.MaVatTu))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo LIKE @MaVatTu");
                parameters.Add("MaVatTu", "%" + search.MaVatTu + "%");
            }
            if (!string.IsNullOrEmpty(search.Section))
            {
                sql.Append(" AND r.CHR_SectionCode LIKE @Section");
                parameters.Add("Section", "%" + search.Section + "%");
            }
            if (!string.IsNullOrEmpty(search.Status))
            {
                sql.Append(" AND r.ID_Status LIKE @Status");
                parameters.Add("Status", "%" + search.Status + "%");
            }
            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail s WHERE s.CHR_CodeSection = r.CHR_SectionCode AND s.CHR_UserAdid = @User)");
                parameters.Add("User", user);
            }

            // count query
            var baseSql = sql.ToString();

            var totalCount = await _conn.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM ({baseSql}) AS CountQuery",
                parameters);

            // pagination
            sql.Append("ORDER BY r.DTM_CreateDate desc, r.CHR_MaDon, r.CHR_MaThietBi, r.CHR_MaNCC, r.CHR_MaHangNoiBo, r.NVCHR_NameVN desc");

            if (search.PageSize > 0 && search.PageIndex > 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (search.PageIndex - 1) * search.PageSize);
                parameters.Add("PageSize", search.PageSize);
            }

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
    }
}
