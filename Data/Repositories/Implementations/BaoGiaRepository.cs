using Dapper;
// removed unused OpenXML usings
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;
using System.Text.RegularExpressions;

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
        public async Task<List<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step, string? user , int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var sql = @"
                SELECT distinct q.*
                FROM BaoGia_Request_of_Quotation as q
				  inner join [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] as s 
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
            ";

            // Build status SQL fragment and append it directly to the WHERE clause when needed
            var statusSql = "1=1";
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "RETURN":
                        statusSql = "ID_Status LIKE '%RETURN%'";
                        break;
                    case "DONE":
                        statusSql = "ID_Status = 'DONE'";
                        break;
                    case "APPROVAL":
                        statusSql = "ID_Status LIKE 'APPROVAL'";
                        break;
                    case "WAIT":
                        statusSql = "ID_Status LIKE '%WAIT%'";
                        break;
                    default:
                        break;
                }

                // append status filter
                sql += " AND (" + statusSql + ")";
            }

            // Add ordering and paging
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

            return (await _conn.QueryAsync<BaoGia_Request_of_Quotation>(sql, parameters)).ToList();
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
            _context.BaoGia_Request_of_Quotations.Update(baogia);
            await _context.SaveChangesAsync();
            return baogia;
        }
        // Lấy danh sách báo giá theo mã đơn
        public async Task<List<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, string user, int pageIndex, int pageSize)
        {
            var sql = new StringBuilder(@"
            WITH rq AS (
                SELECT r.*
                FROM [BaoGia_Request_of_Quotation] AS r
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                WHERE 1 = 1 and r.ID_StepBaoGia < 9 and  r.ID_StepBaoGia > 5");
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
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

            sql.Append(@"
                )
                , det AS (
                    SELECT d.ID_RequestQuote,
                           COUNT(*) AS DetailCount
                    FROM [BaoGia_Detail_of_Quotation] AS d
                    INNER JOIN rq ON rq.ID = d.ID_RequestQuote
                    GROUP BY d.ID_RequestQuote
                )
                , grp AS (
                    SELECT rr.CHR_MaDon, rr.CHR_MaHangNoiBo,
                           COUNT(*) AS ExpectedCount,
                           SUM(CASE WHEN ISNULL(det.DetailCount, 0) > 0 THEN 1 ELSE 0 END) AS CompletedCount
                    FROM rq rr
                    LEFT JOIN det ON det.ID_RequestQuote = rr.ID
                    GROUP BY rr.CHR_MaDon, rr.CHR_MaHangNoiBo
                )
                SELECT DISTINCT 
                    rr.CHR_MaDon,
                    rr.CHR_SectionName,
                    --rr.CHR_MaHangNoiBo,
                    --rr.INT_SoLuong,
                    --rr.NVCHR_DonVi,
                    --rr.CHR_Phanloai,
                    --rr.NVCHR_NameVN,
                    --rr.NVCHR_ChungLoai,
                    --rr.DTM_NgayMuonNhan,
                    CASE WHEN grp.CompletedCount = grp.ExpectedCount AND grp.ExpectedCount > 0 THEN N'Chưa chọn NCC' ELSE N'Đang chờ' END AS [Status]
                FROM rq rr
                LEFT JOIN grp ON grp.CHR_MaDon = rr.CHR_MaDon AND grp.CHR_MaHangNoiBo = rr.CHR_MaHangNoiBo");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" ORDER BY rr.CHR_MaDon DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            else
            {
                sql.Append(" ORDER BY rr.DTM_NgayMuonNhan DESC");
            }

            return (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();
        }
        // Xuất báo giá
        public async Task<List<int>> ExportBaoGiaAsync(string? maDon)
        {
            var a = await _context.BaoGia_Request_of_Quotations
                .Where(c => c.CHR_MaDon == maDon).Select(c => c.ID).ToListAsync();
            return a;
        }
        // Tìm kiến thông tin nhập báo nhập báo giá theo mã đơn yêu cầu
        public async Task<ListRequest<dynamic>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang,string? user, int pageIndex, int pageSize)
        {
            var cteBuilder = new StringBuilder();
            cteBuilder.Append(@"
            WITH BangTongHop AS (
                SELECT 
                    r.CHR_MaDon, 
                    CONVERT(DATE, r.DTM_CreateDate) AS DTM_CreateDate, 
                    r.CHR_SectionName, 
                    r.CHR_CreateBy,
                    r.CHR_MaHangNoiBo,
                    r.CHR_MaNCC,
                    r.ID_StepBaoGia
                FROM [BaoGia_Request_of_Quotation] r
	            left join [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] as s 
	            on r.CHR_SectionCode = s.CHR_CodeSection
                WHERE 1 = 1 
                    AND r.ID_StepBaoGia > 5 ");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(user))
            {
                cteBuilder.Append(" AND s.CHR_UserAdid = @Adid");
                parameters.Add("Adid", user);
            }
            if (!string.IsNullOrEmpty(maDon))
            {
                cteBuilder.Append(" AND r.CHR_MaDon = @MaDon");
                parameters.Add("MaDon", maDon);
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                cteBuilder.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
                parameters.Add("MaHang", maHang);
            }
            if (!string.IsNullOrEmpty(section))
            {
                cteBuilder.Append(" AND r.CHR_SectionCode = @Section");
                parameters.Add("Section", section);
            }

            // Build main select using the CTE
            var sql = new StringBuilder();
            sql.Append(cteBuilder.ToString());
            sql.Append(@"
            )
            SELECT DISTINCT 
                t1.CHR_MaDon, 
                t1.DTM_CreateDate, 
                t1.CHR_SectionName, 
                t1.CHR_CreateBy,
    
                -- Số lượng linh kiện
                (
                    SELECT COUNT(DISTINCT t2.CHR_MaHangNoiBo)
                    FROM BangTongHop t2
                    WHERE t2.CHR_MaDon = t1.CHR_MaDon
                ) AS SoLuongLinhKien,
    
                -- Danh sách nhà cung cấp
                STUFF((
                    SELECT DISTINCT ', ' + t2.CHR_MaNCC
                    FROM BangTongHop t2
                    WHERE t2.CHR_MaDon = t1.CHR_MaDon 
                        AND t2.CHR_MaNCC IS NOT NULL 
                        AND t2.CHR_MaNCC != ''
                    FOR XML PATH('')
                ), 1, 2, '') AS DanhSachNCC,
    
                -- Trạng thái
                CASE 
                    WHEN NOT EXISTS (
                        SELECT 1 
                        FROM BangTongHop t3
                        WHERE t3.CHR_MaDon = t1.CHR_MaDon 
                            AND t3.ID_StepBaoGia != 6
                    ) THEN 'Confirm'
                    ELSE 'Done'
                END AS TrangThai

            FROM BangTongHop t1");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" ORDER BY TrangThai,t1.DTM_CreateDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }
            else
            {
                sql.Append(" ORDER BY TrangThai,t1.DTM_CreateDate DESC");
            }

            var result = await _conn.QueryAsync<dynamic>(sql.ToString(), parameters);

            // Build count sql using same CTE and filters so total respects search
            var countSql = new StringBuilder();
            countSql.Append(cteBuilder.ToString());
            countSql.Append(@"
            )
            SELECT COUNT(DISTINCT CONCAT(CHR_MaDon, '|', CONVERT(DATE, DTM_CreateDate), '|', CHR_SectionName, '|', CHR_CreateBy)) FROM BangTongHop");

            var total = await _conn.ExecuteScalarAsync<int>(countSql.ToString(), parameters);

            return new ListRequest<dynamic>
            {
                Data = result.ToList(),
                TotalCount = total,
            };
        }
        // Lấy thông tin kèm chi tiết báo giá
        public async Task<ListRequest<dynamic>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, string? status, string user, int pageIndex, int pageSize)
        {
            var sql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT 
                    distinct
                    r.id,
                    r.CHR_MaDon,
                    r.CHR_MaHangNoiBo,
                    MAX(CASE 
                        WHEN r.CHR_MaHangNoiBo IS NULL 
                            OR r.NVCHR_NameVN IS NULL 
                            OR r.CHR_MaHangNoiBo = '' 
                            OR r.NVCHR_NameVN = ''
                        THEN 1 ELSE 0 
                    END) OVER (PARTITION BY r.CHR_MaDon, r.CHR_MaHangNoiBo) AS NeedConfirmName,
                    MAX(CASE WHEN r.ID_StepBaoGia != 7 THEN 1 ELSE 0 END) 
                        OVER (PARTITION BY r.CHR_MaDon, r.CHR_MaHangNoiBo) AS HasDifferentStep,
                     CASE WHEN r.ID_StepBaoGia >= 9 and r.ID_StepBaoGia <12 then 1 else 0 END as CofirmedName 
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(user))
            {
                sql.Append(" AND s.CHR_UserAdid= @User");
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
                CASE 
                    WHEN sc.NeedConfirmName > 0 THEN 'WAIT_CONFIRM_NAME'
                    WHEN sc.HasDifferentStep = 0 THEN 'WAIT_PICK_NCC'
		            WHEN SC.CofirmedName  = 1  THEN 'CONFIRMED'
                    ELSE 'WAIT_NCC'
                END AS status
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            INNER JOIN StatusCheck sc ON r.id = sc.id
            WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11");

            if (!string.IsNullOrEmpty(maDon))
            {
                sql.Append(" AND r.CHR_MaDon = @MaDon");
            }
            if (!string.IsNullOrEmpty(maHang))
            {
                sql.Append(" AND r.CHR_MaHangNoiBo = @MaHang");
            }
            if (!string.IsNullOrEmpty(section))
            {
                sql.Append(" AND r.CHR_SectionCode = @Section");
            }
            if (!string.IsNullOrEmpty(maNCC))
            {
                sql.Append(" AND r.CHR_MaNCC = @MaNCC");
            }
            // Filter by computed status coming from StatusCheck (WAIT_CONFIRM_NAME, WAIT_PICK_NCC, WAIT_NCC)
            if (!string.IsNullOrEmpty(status))
            {
                sql.Append(" AND (CASE WHEN sc.NeedConfirmName > 0 THEN 'WAIT_CONFIRM_NAME' WHEN sc.HasDifferentStep = 0 THEN 'WAIT_PICK_NCC' " +
                    "WHEN sc.CofirmedName  = 1  THEN 'CONFIRMED' ELSE 'WAIT_NCC' END) = @Status");
                parameters.Add("Status", status);
            }

            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaDon ,r.CHR_MaThietBi, r.CHR_MaNCC ,r.CHR_MaHangNoiBo, r.NVCHR_NameVN");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            // For total count, build a similar CTE so status filter and partition logic match the main query
            var countSql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT distinct r.id,
                       MAX(CASE 
                           WHEN r.CHR_MaHangNoiBo IS NULL 
                               OR r.NVCHR_NameVN IS NULL 
                               OR r.CHR_MaHangNoiBo = '' 
                               OR r.NVCHR_NameVN = ''
                           THEN 1 ELSE 0 END) OVER (PARTITION BY r.CHR_MaDon, r.CHR_MaHangNoiBo) AS NeedConfirmName,
                           MAX(CASE WHEN r.ID_StepBaoGia != 7 THEN 1 ELSE 0 END) OVER (PARTITION BY r.CHR_MaDon, r.CHR_MaHangNoiBo) AS HasDifferentStep,
                           CASE WHEN r.ID_StepBaoGia >= 9 and r.ID_StepBaoGia <12 then 1 else 0 END as CofirmedName 
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                LEFT JOIN [BaoGia_Master_Approver_Send_Mail] AS s ON r.CHR_SectionCode = s.CHR_CodeSection
                WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11");
            if (!string.IsNullOrEmpty(user))
            {
                countSql.Append(" AND s.CHR_UserAdid= @User");
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

            countSql.Append(@")
            SELECT COUNT(r.id)
            FROM BaoGia_Request_of_Quotation r
            INNER JOIN StatusCheck sc ON r.id = sc.id
            WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia <= 11");

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
                countSql.Append(" AND (CASE WHEN sc.NeedConfirmName > 0 THEN 'WAIT_CONFIRM_NAME' WHEN sc.HasDifferentStep = 0 THEN 'WAIT_PICK_NCC'" +
                    "WHEN sc.CofirmedName  = 1  THEN 'CONFIRMED' ELSE 'WAIT_NCC' END) = @Status");
            }

            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql.ToString(), parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        // lấy mã đơn theo Adid
        public async Task<List<string>> GetMaDonByAdidAsync(string adid)
        {
            var sql = @"  SELECT DISTINCT CHR_MaDon FROM BaoGia_Request_of_Quotation as q
                  inner join [COST_MANAGEMENT].[dbo].[BaoGia_Master_Approver_Send_Mail] as s 
                  on q.CHR_SectionCode = s.CHR_CodeSection
                  WHERE s.CHR_UserAdid = @Adid AND ID_StepBaoGia < 9";
            var parameters = new { Adid = adid };
            var maDons = await _conn.QueryAsync<string>(sql, parameters);
            return maDons.ToList();
        }
        // update thông tin màn hình lịch sử báo giá
        public async Task<bool> UpdateThongTinLichSuBaoGiaAsync(List<BaoGia_Request_of_Quotation> baoGias)
        {
            var listUpdate = new List<BaoGia_Request_of_Quotation>();
            var listOldData = await _context.BaoGia_Request_of_Quotations.Where(c => baoGias.Select(b => b.ID).Contains(c.ID) && c.ID_Status.Contains("RETURN")).ToListAsync();
            if(listOldData == null || !listOldData.Any())
            {
                throw new Exception("Dữ liệu không tồn tại hoặc không ở trạng thái RETURN để sửa lại");
            }
            var listHistory = new List<BaoGia_History_Request_of_Quotation>();
            foreach (var baoGia in baoGias)
            {
                var dto = listOldData.Find(c => c.ID == baoGia.ID);
                if (dto != null)
                {
                    dto.CHR_SectionCode = baoGia.CHR_SectionCode;
                    dto.CHR_SectionName = baoGia.CHR_SectionName;
                    dto.CHR_Phanloai = baoGia.CHR_Phanloai;
                    dto.CHR_MaThietBi = baoGia.CHR_MaThietBi;
                    dto.CHR_MaHangNoiBo = baoGia.CHR_MaHangNoiBo;
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
                    dto.CHR_CreateBy = baoGia.CHR_CreateBy;
                    dto.DTM_UpdateLater = DateTime.Now;
                    dto.INT_SoLanUpdate = (dto.INT_SoLanUpdate ?? 0) + 1;
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
                }
            }
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(listHistory);
            _context.BaoGia_Request_of_Quotations.UpdateRange(listUpdate);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
