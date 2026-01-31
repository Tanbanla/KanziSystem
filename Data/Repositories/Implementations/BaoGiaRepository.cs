using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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
        public async Task<BaoGia_Request_of_Quotation> GetByMaBaoGiaAsync(string maBaoGia)
        {
            var sql = "SELECT * FROM BaoGia_Request_of_Quotation WHERE Ma_Bao_Gia = @MaBaoGia";
            var parameters = new { MaBaoGia = maBaoGia };
            return await _conn.QueryFirstOrDefaultAsync<BaoGia_Request_of_Quotation>(sql, parameters);
        }
        // Tìm kiếm thông tin báo giá và phân trang
        public async Task<List<BaoGia_Request_of_Quotation>> SearchAsync(string? MaDon, string? MaNcc, string? Section, string? nguoiYeuCau, string? MaHang, string? status, int? step,int pageIndex, int pageSize, DateTime? date)
        {
            var sql = @"
                SELECT *
                FROM BaoGia_Request_of_Quotation
                WHERE (@MaDon IS NULL OR CHR_MaDon LIKE '%' + @MaDon + '%')
                  AND (@MaNcc IS NULL OR CHR_MaNCC LIKE '%' + @MaNcc + '%')
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
                Date = date
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
                FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] AS r
                WHERE 1 = 1");

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
                    FROM [COST_MANAGEMENT].[dbo].[BaoGia_Detail_of_Quotation] AS d
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
                    rr.CHR_MaHangNoiBo,
                    rr.INT_SoLuong,
                    rr.NVCHR_DonVi,
                    rr.CHR_Phanloai,
                    rr.NVCHR_NameVN,
                    rr.NVCHR_ChungLoai,
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
    }
}
