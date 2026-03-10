using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Controllers;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.Text;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaConfirmNameRepository : BaseRepository<BaoGia_Confirm_Name_Quotation, int>, IBaoGiaConfirmNameRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        public BaoGiaConfirmNameRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration
        ) : base(context, options, configuration)
        {
            _context = context;
        }
        //search thông tin xác nhận tên hàng
        public async Task<ListRequest<dynamic>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, int pageIndex, int pageSize)
        {
            // Tách phần FROM/WHERE để dùng chung cho truy vấn dữ liệu và truy vấn đếm
            var baseFrom = @"
                FROM BaoGia_Confirm_Name_Quotation c
                INNER JOIN BaoGia_Request_of_Quotation r ON c.NVCHR_Note = r.CHR_MaHangNCC
                WHERE 1 = 1
            ";

            var whereBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            // Thêm điều kiện tìm kiếm
            if (!string.IsNullOrWhiteSpace(TenHang))
            {
                var kw = TenHang.Trim();
                whereBuilder.Append(" AND (ISNULL(c.VCHR_TenHaiQuan, '') LIKE @TenHang OR ISNULL(r.NVCHR_NameVN, '') LIKE @TenHang)");
                parameters.Add("@TenHang", $"%{kw}%");
            }

            if (!string.IsNullOrWhiteSpace(SoDon))
            {
                var md = SoDon.Trim();
                whereBuilder.Append(" AND ISNULL(ID_RequestQuote, '') LIKE @SoDon");
                parameters.Add("@SoDon", $"%{md}%");
            }
            if (!string.IsNullOrWhiteSpace(section))
            {
                var se = section.Trim();
                whereBuilder.Append(" AND ISNULL(r.CHR_SectionCode, '') LIKE @Section");
                parameters.Add("@Section", $"%{se}%");
            }
            if (!string.IsNullOrWhiteSpace(TrangThai))
            {
                whereBuilder.Append(" AND c.CHR_Status = @TrangThai");
                parameters.Add("@TrangThai", TrangThai.Trim());
            }

            // Phân trang
            var PageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var PageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            // Truy vấn tổng (không có phân trang)
            var countSql = "SELECT COUNT(1) " + baseFrom + whereBuilder.ToString();
            var total = await _conn.ExecuteScalarAsync<long>(countSql, parameters);

            // Thực hiện truy vấn dữ liệu với phân trang
            var selectSql = new StringBuilder();
            selectSql.Append("SELECT c.* ,r.CHR_SectionCode,r.CHR_SectionName,r.CHR_Phanloai, r.CHR_MaThietBi, r.CHR_MaHangNoiBo, r.CHR_NameEN,r.CHR_MaHangNCC,r.INT_SoLuong,");
            selectSql.Append(" r.NVCHR_DonVi, r.NVCHR_ChungLoai, r.NVCHR_HinhDang,r.NVCHR_ChatLieu, r.NVCHR_ThanhPhan,r.NVCHR_KichThuoc,r.NVCHR_DongMay, r.NVCHR_TinhNang ");
            selectSql.Append(baseFrom);
            selectSql.Append(whereBuilder.ToString());
            selectSql.Append(@" ORDER BY c.DTM_CreateDate ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

            parameters.Add("@Offset", (PageIndex - 1) * PageSize);
            parameters.Add("@PageSize", PageSize);

            var data = await _conn.QueryAsync<dynamic>(selectSql.ToString(), parameters);

            var result = new ListRequest<dynamic>
            {
                Data = data.ToList(),
                TotalCount = total
            };

            return result;
        }
        // Luu thong tin
        public async Task<bool> SaveConfirmNameAsync(int? Id, string? TenHaiQuan, string? MaHangNoiBo, string? Role, string User)
        {

            var role = string.IsNullOrWhiteSpace(Role) ? "UserPUR" : Role.Trim();
            var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == Id);
            if (row == null) return false;

            var now = DateTime.Now;
            var user = User ?? "SYSTEM";

            // Role enforcement
            if (role.Equals("UserShip", StringComparison.OrdinalIgnoreCase))
            {
                row.VCHR_TenHaiQuan = TenHaiQuan ?? row.VCHR_TenHaiQuan;
                row.VCHR_UserShip = user;
                row.DTM_UserShip = now;
            }
            else if (role.Equals("UserAcc", StringComparison.OrdinalIgnoreCase))
            {
                row.VCHR_MaHangNoiBo = MaHangNoiBo ?? row.VCHR_MaHangNoiBo;
                row.VCHR_UserAcc = user;
                row.DTM_UserAcc = now;
            }
            else // UserPUR
            {
                if (TenHaiQuan != null) row.VCHR_TenHaiQuan = TenHaiQuan;
                if (MaHangNoiBo != null) row.VCHR_MaHangNoiBo = MaHangNoiBo;
                row.VCHR_UserPUR = user;
                row.DTM_UserPUR = now;
            }

            if (!string.Equals(row.CHR_Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(row.CHR_Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                row.CHR_Status = "Confirming"; // đang xác nhận
            }
            row.DTM_UpdateDate = now;
            row.VCHR_UpdateBy = user;

            await _context.SaveChangesAsync();
            return true;
        }
        // Them thong tin
        public async Task<bool> AddConfirmNameAsync(BaoGia_Confirm_Name_Quotation confirmName)
        {
            await _context.BaoGia_Confirm_Name_Quotations.AddAsync(confirmName);
            await _context.SaveChangesAsync();
            return true;
        }
        // Approve ConfirmName
        public async Task<bool> ApproveConfirmNameAsync(int id, string approvedBy)
        {
            if (id <= 0) return false;
            var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == id);
            if (row == null) return false;

            var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.CHR_MaHangNCC == row.NVCHR_Note)
                .ToListAsync();
            if (rq == null) return false;

            var now = DateTime.Now; var user = approvedBy ?? "SYSTEM";
            row.CHR_Status = "Confirmed";
            row.VCHR_UserPUR = user;
            row.DTM_UserPUR = now;
            row.VCHR_UpdateBy = user;
            row.DTM_UpdateDate = now;

            foreach (var r in rq)
            {
                // Update request: map TenHaiQuan -> NVCHR_NameVN, and MaHangNoiBo -> CHR_MaHangNoiBo
                if (!string.IsNullOrWhiteSpace(row.VCHR_TenHaiQuan))
                    r.NVCHR_NameVN = row.VCHR_TenHaiQuan;
                if (!string.IsNullOrWhiteSpace(row.VCHR_MaHangNoiBo))
                    r.CHR_MaHangNoiBo = row.VCHR_MaHangNoiBo;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        // Reject ConfirmName
        public async Task<bool> RejectConfirmNameAsync(int id, string reason, string rejectedBy)
        {
            if (id <= 0) return false;
            var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == id);
            if (row == null) return false;
            var now = DateTime.Now; var user = rejectedBy ?? "SYSTEM";
            row.CHR_Status = "Rejected";
            row.NVCHR_LyDo = reason;
            row.VCHR_UserPUR = user;
            row.DTM_UserPUR = now;
            row.VCHR_UpdateBy = user;
            row.DTM_UpdateDate = now;
            await _context.SaveChangesAsync();
            return true;
        }
        // Luu thong tin
        public async Task<bool> AddListAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames)
        {
            await _context.BaoGia_Confirm_Name_Quotations.AddRangeAsync(confirmNames);
            await _context.SaveChangesAsync();
            return true;
        }
        // luu thong tin nhap file
        public async Task<bool> SaveFromFileAsync(List<BaoGia_Confirm_Name_Quotation> confirmNames, string user, string? Role)
        {
            var role = string.IsNullOrWhiteSpace(Role) ? "UserPUR" : Role.Trim();
            if (confirmNames == null) return false;

            var now = DateTime.Now;
            foreach (var i in confirmNames)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID_RequestQuote == i.ID_RequestQuote);
                if (row == null) continue;
                // Role enforcement
                if (role.Equals("UserShip", StringComparison.OrdinalIgnoreCase))
                {
                    row.VCHR_TenHaiQuan = i.VCHR_TenHaiQuan;
                    row.VCHR_UserShip = user;
                    row.DTM_UserShip = now;
                }
                else if (role.Equals("UserAcc", StringComparison.OrdinalIgnoreCase))
                {
                    row.VCHR_MaHangNoiBo = i.VCHR_MaHangNoiBo;
                    row.VCHR_UserAcc = user;
                    row.DTM_UserAcc = now;
                }
                else // UserPUR
                {
                    row.VCHR_TenHaiQuan = i.VCHR_TenHaiQuan;
                    row.VCHR_MaHangNoiBo = i.VCHR_MaHangNoiBo;
                    row.NVCHR_LyDo = i.NVCHR_LyDo;
                    row.NVCHR_Note = i.NVCHR_Note;
                    row.VCHR_UserPUR = user;
                    row.DTM_UserPUR = now;
                }

                if (!string.Equals(row.CHR_Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(row.CHR_Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    row.CHR_Status = "Confirming"; // đang xác nhận
                }
                row.DTM_UpdateDate = now;
                row.VCHR_UpdateBy = user;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SaveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {

            var role = string.IsNullOrWhiteSpace(Role) ? "UserPUR" : Role.Trim();
            if (saveConfirms == null || !saveConfirms.Any()) return false;

            foreach (var item in saveConfirms)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.Id);
                if (row == null) return false;

                var now = DateTime.Now;

                // Role enforcement
                if (role.Equals("UserShip", StringComparison.OrdinalIgnoreCase))
                {
                    row.VCHR_TenHaiQuan = item.TenHaiQuan ?? row.VCHR_TenHaiQuan;
                    row.VCHR_UserShip = user;
                    row.DTM_UserShip = now;
                }
                else if (role.Equals("UserAcc", StringComparison.OrdinalIgnoreCase))
                {
                    row.VCHR_MaHangNoiBo = item.MaHangNoiBo ?? row.VCHR_MaHangNoiBo;
                    row.VCHR_UserAcc = user;
                    row.DTM_UserAcc = now;
                }
                else // UserPUR
                {
                    if (item.TenHaiQuan != null) row.VCHR_TenHaiQuan = item.TenHaiQuan;
                    if (item.MaHangNoiBo != null) row.VCHR_MaHangNoiBo = item.MaHangNoiBo;
                    row.VCHR_UserPUR = user;
                    row.DTM_UserPUR = now;
                }

                if (!string.Equals(row.CHR_Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(row.CHR_Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    row.CHR_Status = "Confirming"; // đang xác nhận
                }
                row.DTM_UpdateDate = now;
                row.VCHR_UpdateBy = user;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        // Approvers 
        public async Task<bool> ApproveConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            if (saveConfirms == null || !saveConfirms.Any()) return false;
            if (Role != "UserPUR") return false;
            foreach (var item in saveConfirms)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.Id);
                if (row == null) return false;
                var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.CHR_MaHangNCC == row.NVCHR_Note)
                .ToListAsync();
                if (rq == null) return false;
                var now = DateTime.Now;
                if(item.pheDuyet == true)
                {
                    row.CHR_Status = "Confirmed";
                    row.VCHR_UserPUR = user;
                    row.DTM_UserPUR = now;
                    row.VCHR_UpdateBy = user;
                    row.DTM_UpdateDate = now;
                    foreach (var r in rq)
                    {
                        // Update request: map TenHaiQuan -> NVCHR_NameVN, and MaHangNoiBo -> CHR_MaHangNoiBo
                        if (!string.IsNullOrWhiteSpace(row.VCHR_TenHaiQuan))
                            r.NVCHR_NameVN = row.VCHR_TenHaiQuan;
                        if (!string.IsNullOrWhiteSpace(row.VCHR_MaHangNoiBo))
                            r.CHR_MaHangNoiBo = row.VCHR_MaHangNoiBo;
                    }
                }
                else
                {
                    row.CHR_Status = "Rejected";
                    row.NVCHR_LyDo = item.LyDo;
                    row.VCHR_UserPUR = user;
                    row.DTM_UserPUR = now;
                    row.VCHR_UpdateBy = user;
                    row.DTM_UpdateDate = now;
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
// 03/03/2026
// k có mã nhà cung cấp, cũng đăng xin báo giá dc
// - thêm mã đơn yêu cầu vào file nhập báo giá nhà cung cấp

// 06/03/2026
// -- dong đầu tiên k nhảy chủng loại hàng
// - nhảy trùng nhà cung cấp (Không phải trung mà là 2 thằng 2 PIC)

// -- kiểm tra tất cả rồi mới out lỗi, thời gian gian lỗi vẫn cho up, blane vẫn cho up thông tin 
// -- lưu lịch sử khi nhập bao giá 

// 03/09/2026
// - màn hình nhập upload dữ liệu ncc
// - màn hình xác nhận tên, gom theo mà linh kiện nhà cung, thêm phê duyệt nhiều
// - mail chỉnh sửa cho gửi , 
// - cho phép sửa đến hết khi đẩy đi 
// - tạo lựa chọn trong file báo giá , accept, lỗi lấy thông tin gửi nhà cung cấp 
// - xử lý trường hợp nhà cung cấp từ chối báo giá.