using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;
using System.Text.RegularExpressions;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaRepository: BaseRepository<BaoGia_Request_of_Quotation , int>, IBaoGiaRepository 
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        : base(context, options, configuration) {
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
        public async Task<List<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step,int pageIndex, int pageSize, DateTime? date, string? chungLoai)
        {
            var sql = @"
                SELECT *
                FROM BaoGia_Request_of_Quotation
                WHERE (@MaDon IS NULL OR CHR_MaDon LIKE '%' + @MaDon + '%')
                  AND (@MaNcc IS NULL OR CHR_MaNCC LIKE '%' + @MaNcc + '%')
                  AND (@ChungLoai IS NULL OR NVCHR_ChungLoai LIKE '%' + @ChungLoai + '%')
                  AND (@Section IS NULL OR CHR_SectionCode LIKE '%' + @Section + '%')
                  AND (@NguoiYeuCau IS NULL OR CHR_CreateBy LIKE '%' + @NguoiYeuCau + '%')
                  AND (@MaHang IS NULL OR CHR_MaHangNoiBo LIKE '%' + @MaHang + '%')
                  AND (@status IS NULL OR ID_Status = @status)
                  AND (@Step IS NULL OR ID_StepBaoGia = @Step)
                  AND (@Date IS NULL OR CAST(DTM_CreateDate AS DATE) = CAST(@Date AS DATE))
            ";
            if (pageSize > 0 && pageIndex > 0)
            {
                sql += @"
                    ORDER BY DTM_CreateDate
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                ";
            }
            else
            {
                sql += @"
                    ORDER BY DTM_CreateDate
                ";
            }
            var parameters = new
            {
                MaDon = string.IsNullOrEmpty(MaDon) ? null : MaDon,
                MaNcc = string.IsNullOrEmpty(MaNcc) ? null : MaNcc,
                Section = string.IsNullOrEmpty(Section) ? null : Section,
                NguoiYeuCau = string.IsNullOrEmpty(nguoiYeuCau) ? null : nguoiYeuCau,
                MaHang = string.IsNullOrEmpty(MaHang) ? null : MaHang,
                status = string.IsNullOrEmpty(status) ? null : status,
                Step = step,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize,
                Date = date,
                ChungLoai = string.IsNullOrEmpty(chungLoai) ? null : chungLoai
            };
            return (await _conn.QueryAsync<BaoGia_Request_of_Quotation>(sql, parameters)).ToList();
        }
        // Nhap bao gia
        public async Task<bool> NhapBaoGiaAsync(BaoGia_Request_of_Quotation baoGia)
        {
            try
            {
                _context.BaoGia_Request_of_Quotations.Add(baoGia);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NhapBaoGiaAsync: {ex.Message}");
                return false;
            }
        }
        // Nhap danh sach
        public async Task<List<BaoGia_Request_of_Quotation>> NhapDanhSachBaoGiaAsync(List<BaoGia_Request_of_Quotation> danhSachBaoGia)
        {
            try
            {
                await _context.BaoGia_Request_of_Quotations.AddRangeAsync(danhSachBaoGia);
                await _context.SaveChangesAsync();

                return danhSachBaoGia;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NhapDanhSachBaoGiaAsync: {ex.Message}");
                return new List<BaoGia_Request_of_Quotation>();
            }
        }
        // Update thông tin bao gia
        public async Task<bool> CapNhatThongTinBaoGiaAsync(BaoGia_Request_of_Quotation baoGia)
        {
            try
            {
                _context.BaoGia_Request_of_Quotations.Update(baoGia);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Error in CapNhatThongTinBaoGiaAsync: {ex.Message}");
                return false;
            }
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
        public async Task<List<dynamic>> GetThongTinBaoGiaGomNhomAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize)
        {
            var sql = new StringBuilder(@"
            WITH rq AS (
                SELECT r.*
                FROM [BaoGia_Request_of_Quotation] AS r
                WHERE 1 = 1 and r.ID_StepBaoGia < 9 and  r.ID_StepBaoGia > 5");
            var parameters = new DynamicParameters();

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
                    rr.DTM_NgayMuonNhan,
                    CASE WHEN grp.CompletedCount = grp.ExpectedCount AND grp.ExpectedCount > 0 THEN N'Chưa chọn NCC' ELSE N'Đang chờ' END AS [Status]
                FROM rq rr
                LEFT JOIN grp ON grp.CHR_MaDon = rr.CHR_MaDon AND grp.CHR_MaHangNoiBo = rr.CHR_MaHangNoiBo");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" ORDER BY rr.DTM_NgayMuonNhan DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
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
        public async Task<ListRequest<dynamic>> SearchThongTinNhapBaoGiaAsync(string? maDon, string? section, string? maHang, int pageIndex, int pageSize)
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
                WHERE 1 = 1 
                    AND r.ID_StepBaoGia > 5 ");

            var parameters = new DynamicParameters();

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
        public async Task<ListRequest<dynamic>> GetThongTinBaoGiaChiTietAsync(string? maDon, string? section, string? maHang, string? maNCC, int pageIndex, int pageSize)
        {
            var sql = new StringBuilder(@"
            WITH StatusCheck AS (
                SELECT 
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
                        OVER (PARTITION BY r.CHR_MaDon, r.CHR_MaHangNoiBo) AS HasDifferentStep
                FROM BaoGia_Request_of_Quotation r
                LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
                WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia < 9");

            var parameters = new DynamicParameters();

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
                d.[INT_SoLuong],
                d.[NVCHR_DonVi],
                d.[NVCHR_NhaSanXuat],
                d.[DTM_EffectiveDate],
                d.[DTM_ExpiryDate],
                d.[NVCHR_Note],
                d.[NVCHR_File],
                d.[NVCHR_MOQ],
                d.[DTM_LeadTime],
                d.[DTM_ShipTime],
                d.[NVCHR_Packing],
                CASE 
                    WHEN sc.NeedConfirmName > 0 THEN 'WAIT_CONFIRM_NAME'
                    WHEN sc.HasDifferentStep = 0 THEN 'WAIT_PICK_NCC'
                    ELSE 'WAIT_NCC'
                END AS status
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            INNER JOIN StatusCheck sc ON r.id = sc.id
            WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia < 9");

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

            sql.Append(" ORDER BY r.DTM_CreateDate, r.CHR_MaHangNoiBo, r.NVCHR_NameVN");

            if (pageSize > 0 && pageIndex > 0)
            {
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageIndex - 1) * pageSize);
                parameters.Add("PageSize", pageSize);
            }

            var data = (await _conn.QueryAsync<dynamic>(sql.ToString(), parameters)).ToList();

            // For total count, we need a separate query
            var countSql = new StringBuilder(@"
            SELECT COUNT(r.id)
            FROM BaoGia_Request_of_Quotation r
            LEFT JOIN BaoGia_Detail_of_Quotation d ON r.id = d.ID_RequestQuote
            WHERE r.ID_StepBaoGia > 5 AND r.ID_StepBaoGia < 9");

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

            var totalCount = await _conn.ExecuteScalarAsync<int>(countSql.ToString(), parameters);

            return new ListRequest<dynamic>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
    }
}
