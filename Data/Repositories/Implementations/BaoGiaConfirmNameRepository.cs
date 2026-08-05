using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Controllers;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using System.Data;
using System.Text;

namespace PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations
{
    public class BaoGiaConfirmNameRepository : BaseRepository<BaoGia_Confirm_Name_Quotation, int>, IBaoGiaConfirmNameRepository
    {
        private readonly COST_MANAGEMENTContext _context;
        private readonly IFileImportService _fileImportService;
        public BaoGiaConfirmNameRepository(COST_MANAGEMENTContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration, IFileImportService fileImportService
        ) : base(context, options, configuration)
        {
            _context = context;
            _fileImportService = fileImportService;
        }
        //search thông tin xác nhận tên hàng
        public async Task<ListRequest<dynamic>> SearchAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user, int pageIndex, int pageSize)
        {
            // Tách phần FROM/WHERE để dùng chung cho truy vấn dữ liệu và truy vấn đếm
            var baseFrom = @"
                FROM BaoGia_Confirm_Name_Quotation c
                INNER JOIN BaoGia_Request_of_Quotation r 
                    ON c.ID_RequestQuote = r.ID
                    AND r.ID_Status NOT LIKE '%RETURN%'
                    AND r.ID_StepBaoGia >= 6
                    AND r.BIT_LayBaoGia = 1
                INNER JOIN BaoGia_Detail_of_Quotation d 
                    ON c.ID_RequestQuote = d.ID_RequestQuote
                INNER JOIN IM_NCC_NEW n 
                    ON n.Ma = r.CHR_MaNCC
                WHERE 1 = 1
            ";

            var whereBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            // Thêm điều kiện tìm kiếm
            if (!string.IsNullOrWhiteSpace(user))
            {
                var u = user.Trim();
                whereBuilder.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail m " +
                    "WHERE m.CHR_CodeSection = r.CHR_SectionCode AND m.CHR_UserAdid LIKE @User)");
                parameters.Add("@User", $"%{u}%");
            }
            if (!string.IsNullOrWhiteSpace(TenHang))
            {
                var kw = TenHang.Trim();
                whereBuilder.Append(" AND (ISNULL(c.VCHR_TenHaiQuan, '') LIKE @TenHang OR ISNULL(r.NVCHR_NameVN, '') LIKE @TenHang)");
                parameters.Add("@TenHang", $"%{kw}%");
            }

            if (!string.IsNullOrWhiteSpace(SoDon))
            {
                whereBuilder.Append(" AND r.CHR_MaDon = @SoDon");
                parameters.Add("@SoDon", SoDon.Trim());
            }
            if (!string.IsNullOrWhiteSpace(section))
            {
                var se = section.Trim();
                whereBuilder.Append(" AND ISNULL(r.CHR_SectionCode, '') LIKE @Section");
                parameters.Add("@Section", $"%{se}%");
            }
            if (!string.IsNullOrWhiteSpace(TrangThai))
            {
                if (string.Equals(role, "UserShip", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND c.CHR_StatusShip = @TrangThai");
                }
                //else if (string.Equals(role, "UserAcc", StringComparison.OrdinalIgnoreCase))
                //{
                //    whereBuilder.Append(" AND c.CHR_StatusAcc = @TrangThai");
                //}
                else if (string.Equals(role, "UserPUR", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND c.CHR_Status = @TrangThai");
                }
                else
                {
                    whereBuilder.Append(" AND c.CHR_StatusAcc = @TrangThai");
                }
                parameters.Add("@TrangThai", TrangThai.Trim());
            }

            // Phân trang
            var PageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var PageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 1000);

            // Truy vấn tổng (không có phân trang)
            var countSql = "SELECT COUNT(DISTINCT c.ID) " + baseFrom + whereBuilder.ToString();
            var total = await _conn.ExecuteScalarAsync<long>(countSql, parameters);

            // Thực hiện truy vấn dữ liệu với phân trang
            var selectSql = new StringBuilder();
            selectSql.Append("SELECT DISTINCT c.* ,r.CHR_SectionCode,r.CHR_SectionName,r.CHR_Phanloai, r.CHR_MaThietBi, r.CHR_MaHangNoiBo, r.CHR_NameEN,r.CHR_MaHangNCC,r.INT_SoLuong,");
            selectSql.Append(" r.NVCHR_DonVi, r.NVCHR_ChungLoai, r.NVCHR_HinhDang,r.NVCHR_ChatLieu, r.NVCHR_ThanhPhan,r.NVCHR_KichThuoc,r.NVCHR_DongMay, r.NVCHR_TinhNang,n.ShortName, d.CHR_CodeNCC, d.NVCHR_File ");
            selectSql.Append(baseFrom);
            selectSql.Append(whereBuilder.ToString());
            selectSql.Append(@" ORDER BY c.DTM_CreateDate ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

            parameters.Add("@Offset", (PageIndex - 1) * PageSize);
            parameters.Add("@PageSize", PageSize);

            var data = await _conn.QueryAsync<dynamic>(selectSql.ToString(), parameters);

            var a = selectSql.ToString();
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

            var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.CHR_MaHangNCC == row.NVCHR_Note)
            .ToListAsync();
            if (rq == null) return false;

            var now = DateTime.Now;
            var user = User ?? "SYSTEM";

            // Role enforcement
            if (role.Equals("UserShip", StringComparison.OrdinalIgnoreCase))
            {
                row.VCHR_TenHaiQuan = TenHaiQuan ?? row.VCHR_TenHaiQuan;
                row.VCHR_UserShip = user;
                row.DTM_UserShip = now;
                row.CHR_StatusShip = "Confirmed";
                foreach (var r in rq)
                {
                    // Update request: map TenHaiQuan -> NVCHR_NameVN, and MaHangNoiBo -> CHR_MaHangNoiBo
                    if (!string.IsNullOrWhiteSpace(row.VCHR_TenHaiQuan))
                        r.NVCHR_NameVN = row.VCHR_TenHaiQuan;
                }
            }
            else if (role.Equals("UserAcc", StringComparison.OrdinalIgnoreCase))
            {
                row.VCHR_MaHangNoiBo = MaHangNoiBo ?? row.VCHR_MaHangNoiBo;
                row.VCHR_UserAcc = user;
                row.DTM_UserAcc = now;
                row.CHR_StatusACC = "Confirmed";
                foreach (var r in rq)
                {
                    // Update request: map TenHaiQuan -> NVCHR_NameVN, and MaHangNoiBo -> CHR_MaHangNoiBo
                    if (!string.IsNullOrWhiteSpace(row.VCHR_MaHangNoiBo))
                        r.CHR_MaHangNoiBo = row.VCHR_MaHangNoiBo;
                }
            }
            else // UserPUR
            {
                if (TenHaiQuan != null) row.VCHR_TenHaiQuan = TenHaiQuan;
                if (MaHangNoiBo != null) row.VCHR_MaHangNoiBo = MaHangNoiBo;
                row.VCHR_UserPUR = user;
                row.DTM_UserPUR = now;
            }

            //if (!string.Equals(row.CHR_Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
            //    !string.Equals(row.CHR_Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            //{
            //    row.CHR_Status = "Confirming"; // đang xác nhận
            //}
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
        public async Task<List<BaoGia_Confirm_Name_Quotation>> AddListAsync(
            List<BaoGia_Confirm_Name_Quotation> confirmNames)
        {
            if (confirmNames == null || !confirmNames.Any())
            {
                throw new ArgumentException(
                    "The list of confirm names cannot be null or empty.",
                    nameof(confirmNames));
            }

            var requestIds = confirmNames
                .Select(x => x.ID_RequestQuote)
                .Distinct()
                .ToList();

            // Luôn update Request
            var requests = await _context.BaoGia_Request_of_Quotations
                .Where(x => requestIds.Contains(x.ID))
                .ToListAsync();

            foreach (var request in requests)
            {
                request.ID_StepBaoGia = 12;
                request.ID_Status = "WAIT_CONFIRM_NAME";
            }

            // Lấy các Request đã có Confirm Name
            var existingRequestIds = (await _context.BaoGia_Confirm_Name_Quotations
                .Where(x => requestIds.Contains(x.ID_RequestQuote))
                .Select(x => x.ID_RequestQuote)
                .ToListAsync())
                .ToHashSet();

            // Chỉ insert những Request chưa có
            var newConfirmNames = confirmNames
                .Where(x => !existingRequestIds.Contains(x.ID_RequestQuote))
                .ToList();

            if (newConfirmNames.Any())
            {
                await _context.BaoGia_Confirm_Name_Quotations.AddRangeAsync(newConfirmNames);
            }

            await _context.SaveChangesAsync();

            return newConfirmNames;
        }
        // luu thong tin nhap file
        public async Task<bool> SaveFromFileAsync(List<ConfirmNameInputExcel> confirmNames, string user, string? Role)
        {
            if (confirmNames == null) return false;

            var now = DateTime.Now;
            foreach (var i in confirmNames)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == i.ID);
                if (row == null) continue;


                var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.ID == row.ID_RequestQuote)
                .FirstOrDefaultAsync();
                if (rq == null) continue;

                // update rq
                rq.ID_StepBaoGia = 13;
                rq.ID_Status = "DONE";
                rq.NVCHR_NameVN = i.VCHR_TenHaiQuan;

                // update confirm name
                row.VCHR_TenHaiQuan = i.VCHR_TenHaiQuan;
                row.VCHR_UserShip = user;
                row.DTM_UserShip = now;
                row.CHR_StatusShip = "Confirmed";
                row.CHR_Status = "Confirmed";
                row.CHR_StatusACC = "Confirmed";
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

                var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.ID == row.ID_RequestQuote)
                .FirstOrDefaultAsync();
                if (rq == null) return false;

                var now = DateTime.Now;

                // Role enforcement
                if (role.Equals("UserShip", StringComparison.OrdinalIgnoreCase))
                {
                    row.VCHR_TenHaiQuan = item.TenHaiQuan ?? row.VCHR_TenHaiQuan;
                    row.VCHR_UserShip = user;
                    row.DTM_UserShip = now;
                    row.CHR_StatusShip = "Confirmed";
                    // Update request: map TenHaiQuan -> NVCHR_NameVN, and MaHangNoiBo -> CHR_MaHangNoiBo
                    if (!string.IsNullOrWhiteSpace(row.VCHR_TenHaiQuan))
                    {
                        rq.NVCHR_NameVN = row.VCHR_TenHaiQuan;
                        rq.ID_StepBaoGia = 13;
                        rq.ID_Status = "DONE";
                    }
                }
                else // UserPUR
                {
                    if (item.TenHaiQuan != null) row.VCHR_TenHaiQuan = item.TenHaiQuan;
                    if (item.MaHangNoiBo != null) row.VCHR_MaHangNoiBo = item.MaHangNoiBo;
                    row.CHR_Status = "";
                    row.CHR_StatusShip = "Confirming";
                    row.VCHR_UserPUR = user;
                    row.DTM_UserPUR = now;
                }

                //if (!string.Equals(row.CHR_Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                //    !string.Equals(row.CHR_Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                //{
                //    row.CHR_Status = "Confirming"; // đang xác nhận
                //}
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
                if (item.pheDuyet == true)
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
        // Rejects Acc
        public async Task<bool> RejectAccConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            if (saveConfirms == null || !saveConfirms.Any()) return false;
            if (Role != "UserAcc") return false;
            foreach (var item in saveConfirms)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.Id);
                if (row == null) return false;
                row.CHR_Status = "RejectedAcc";
                row.NVCHR_LyDo = item.LyDo;
                row.VCHR_UserAcc = user;
                row.DTM_UserAcc = DateTime.Now;
                row.VCHR_UpdateBy = user;
                row.DTM_UpdateDate = DateTime.Now;
                row.CHR_StatusACC = "RejectedAcc";
            }
            await _context.SaveChangesAsync();
            return true;
        }
        // Export Code Confirmed
        public async Task<List<dynamic>> ExportCodeConfirmedAsync()
        {
            var data = await _context.MATERIALs
                .Select(c => new { c.Material_Code, c.Material_Name_VN })
                .ToListAsync();

            return data.Cast<dynamic>().ToList();
        }
        // Từ chối xác nhận tên hàng
        public async Task<bool> RejectConfirmNameListAsync(List<ConfirmNameDTO> saveConfirms, string user, string? Role)
        {
            if (saveConfirms == null || !saveConfirms.Any()) return false;
            if (Role != "UserShip") return false;
            var historyList = new List<BaoGia_History_Request_of_Quotation>();
            foreach (var item in saveConfirms)
            {
                var row = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.Id);
                if (row == null) return false;
                var rq = await _context.BaoGia_Request_of_Quotations.Where(x => x.ID == row.ID_RequestQuote)
                .FirstOrDefaultAsync();
                if (rq == null) return false;
                var now = DateTime.Now;
                // Update infor data confirm name
                row.CHR_Status = "";
                row.NVCHR_LyDo = item.LyDo;
                row.DTM_UserShip = now;
                row.VCHR_UpdateBy = user;
                row.DTM_UpdateDate = now;
                row.CHR_StatusShip = "Rejected";
                row.CHR_StatusACC = "Confirming";
                row.VCHR_UserShip = item.PicShip;

                // Create history
                var history = new BaoGia_History_Request_of_Quotation
                {
                    ID_RequestQuote = rq.ID,
                    CHR_MaDon = rq.CHR_MaDon ?? "",
                    CHR_UpdateBy = user,
                    NVCHR_LyDo = item.LyDo,
                    CHR_ActionType = "RETURN_SHIP",
                    CHR_ChangedColumns = "CHR_Status, ID_StepBaoGia, ID_Status",
                    CHR_OldData = $"CHR_Status: {row.CHR_Status}, ID_StepBaoGia: {rq.ID_StepBaoGia}, ID_Status: {rq.ID_Status}",
                    CHR_NewData = $"CHR_Status: Rejected, ID_StepBaoGia: 6, ID_Status: RETURN_SHIP",
                    CHR_Updatedate = now
                };
                historyList.Add(history);
            }
            await _context.BaoGia_History_Request_of_Quotations.AddRangeAsync(historyList);
            await _context.SaveChangesAsync();
            return true;
        }
        // Check mã đơn đã xác nhận tên hàng đã hoàn thành hay chưa
        public async Task<List<ResultCheckCofirmName>> SearchSendMailConfirmNameAsync(List<int> listCheck)
        {
            if (listCheck == null || !listCheck.Any())
                throw new Exception("Invalid list of IDs.");

            string sql = @"
               SELECT DISTINCT r.CHR_MaDon as MaDon, r.CHR_SectionCode as Section, r.CHR_CreateBy as UserCreate, r.ID as ID
               FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] as r
               LEFT JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Confirm_Name_Quotation] as c 
                   ON r.ID = c.ID_RequestQuote
               WHERE c.ID IN @ListCheck";

            using (var connection = _conn)
            {
                var result = await connection.QueryAsync<ResultCheckCofirmName>(sql, new { ListCheck = listCheck });
                return result.ToList();
            }
        }
        // Cập nhật thông tin đơn báo giá sau khi trả lại
        public async Task<bool> UpdateRequestFromFileAsync(List<BaoGia_Request_of_Quotation> baoGia, string user)
        {
            if (baoGia == null || !baoGia.Any()) return false;
            bool hasChanges = false;
            foreach (var item in baoGia)
            {
                var confirmName = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.ID);
                if (confirmName == null) continue;
                var rq = await _context.BaoGia_Request_of_Quotations.FirstOrDefaultAsync(x => x.ID == confirmName.ID_RequestQuote);
                if (rq == null) continue;
                // Update infor request quote
                if (rq.NVCHR_HinhDang != item.NVCHR_HinhDang ||
                       rq.NVCHR_ChatLieu != item.NVCHR_ChatLieu ||
                       rq.NVCHR_ThanhPhan != item.NVCHR_ThanhPhan ||
                       rq.NVCHR_KichThuoc != item.NVCHR_KichThuoc ||
                       rq.NVCHR_DongMay != item.NVCHR_DongMay ||
                       rq.NVCHR_TinhNang != item.NVCHR_TinhNang ||
                       rq.CHR_MaThietBi != item.CHR_MaThietBi ||
                       rq.CHR_MaHangNCC != item.CHR_MaHangNCC)
                {
                    // Update infor request quote
                    rq.NVCHR_HinhDang = item.NVCHR_HinhDang;
                    rq.NVCHR_ChatLieu = item.NVCHR_ChatLieu;
                    rq.NVCHR_ThanhPhan = item.NVCHR_ThanhPhan;
                    rq.NVCHR_KichThuoc = item.NVCHR_KichThuoc;
                    rq.NVCHR_DongMay = item.NVCHR_DongMay;
                    rq.NVCHR_TinhNang = item.NVCHR_TinhNang;
                    rq.CHR_MaThietBi = item.CHR_MaThietBi;
                    rq.CHR_MaHangNCC = item.CHR_MaHangNCC;

                    hasChanges = true;
                }
                // update confirm name
                confirmName.CHR_Status = "Confirmed";
                confirmName.CHR_StatusACC = "Confirmed";
                confirmName.CHR_StatusShip = "Confirming";
                confirmName.DTM_UserAcc = DateTime.Now;
                confirmName.VCHR_UserAcc = user;
                hasChanges = true;
            }
            if (hasChanges)
            {
                int result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }
        // Cập nhật thông tin yêu cầu PIC PUR cần xác nhận lại báo giá
        public async Task<bool> UpdateRequestForPICPURAsync(List<ConfirmNameInputExcel> baoGia, string user)
        {
            if (baoGia == null || !baoGia.Any()) return false;
            foreach (var item in baoGia)
            {
                var confirmName = await _context.BaoGia_Confirm_Name_Quotations.FirstOrDefaultAsync(x => x.ID == item.ID);
                if (confirmName == null) continue;
                confirmName.CHR_Status = "Confirming";
                confirmName.DTM_UserShip = DateTime.Now;
                confirmName.VCHR_UserShip = user;
                confirmName.NVCHR_LyDo = "Tên xác nhận của Ship và nhà cung cấp khác nhau";
                confirmName.CHR_StatusShip = "Rejected";
                confirmName.VCHR_TenHaiQuan = item.VCHR_TenHaiQuan;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        //Export file ten hanh xac nhan
        public async Task<List<dynamic>> ExportConfirmedMaterialNamesAsync(string? TenHang, string? SoDon, string? TrangThai, string? section, string? role, string user)
        {
            var baseFrom = @"
                FROM BaoGia_Confirm_Name_Quotation c
                INNER JOIN BaoGia_Request_of_Quotation r 
                    ON c.ID_RequestQuote = r.ID
                    AND r.ID_Status NOT LIKE '%RETURN%'
                    AND r.ID_StepBaoGia >= 6
                    AND r.BIT_LayBaoGia = 1
                INNER JOIN BaoGia_Detail_of_Quotation d 
                    ON c.ID_RequestQuote = d.ID_RequestQuote
                INNER JOIN IM_NCC_NEW n 
                    ON n.Ma = r.CHR_MaNCC
                WHERE 1 = 1
            ";

            var whereBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            // Thêm điều kiện tìm kiếm
            if (!string.IsNullOrWhiteSpace(user))
            {
                var u = user.Trim();
                whereBuilder.Append(" AND EXISTS (SELECT 1 FROM BaoGia_Master_Approver_Send_Mail m " +
                    "WHERE m.CHR_CodeSection = r.CHR_SectionCode AND m.CHR_UserAdid LIKE @User)");
                parameters.Add("@User", $"%{u}%");
            }
            if (!string.IsNullOrWhiteSpace(TenHang))
            {
                var kw = TenHang.Trim();
                whereBuilder.Append(" AND (ISNULL(c.VCHR_TenHaiQuan, '') LIKE @TenHang OR ISNULL(r.NVCHR_NameVN, '') LIKE @TenHang)");
                parameters.Add("@TenHang", $"%{kw}%");
            }

            if (!string.IsNullOrWhiteSpace(SoDon))
            {
                var md = SoDon.Trim();
                whereBuilder.Append(" AND ISNULL(CHR_MaDon, '') LIKE @SoDon");
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
                if (string.Equals(role, "UserShip", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND c.CHR_StatusShip = @TrangThai");
                }
                else if (string.Equals(role, "UserAcc", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND c.CHR_StatusAcc = @TrangThai");
                }
                else if (string.Equals(role, "UserPUR", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND c.CHR_Status = @TrangThai");
                }
                else
                {
                    whereBuilder.Append(" AND c.CHR_StatusAcc = @TrangThai");
                }
                parameters.Add("@TrangThai", TrangThai.Trim());
            }

            var selectSql = new StringBuilder();
            selectSql.Append("SELECT DISTINCT c.*, r.CHR_MaDon ,r.CHR_CreateBy,r.CHR_SectionCode,r.CHR_SectionName,r.CHR_Phanloai, r.CHR_MaThietBi, r.CHR_MaHangNoiBo,r.CHR_NameEN as NameEN,d.CHR_MaHangNCC as maHangNcc,d.INT_SoLuong,");
            selectSql.Append(" d.NVCHR_DonVi,d.NVCHR_TenHangHQ, r.NVCHR_ChungLoai, r.NVCHR_HinhDang,r.NVCHR_ChatLieu, r.NVCHR_ThanhPhan,r.NVCHR_KichThuoc,r.NVCHR_DongMay, r.NVCHR_TinhNang,n.ShortName, d.CHR_CodeNCC, d.NVCHR_File ");
            selectSql.Append(baseFrom);
            selectSql.Append(whereBuilder.ToString());

            var data = await _conn.QueryAsync<dynamic>(selectSql.ToString(), parameters);
            if (data == null) return new List<dynamic>();

            return data.ToList();
        }
        // Update Name HQ role PIC PUR
        public async Task<List<int>> UpdateNameHQRolePICPURAsync(List<ConfirmNameInputExcel> baoGia, string user)
        {
            if (baoGia == null || baoGia.Count == 0) throw new Exception("Danh sách nhập không được để trống.");

            var ids = baoGia.Select(x => x.ID).ToList();

            // Load confirmName
            var confirmNames = await _context.BaoGia_Confirm_Name_Quotations
                .Where(x => ids.Contains(x.ID))
                .ToListAsync();

            if (!confirmNames.Any()) throw new  Exception("Không tìm thấy dữ liệu tương ứng trong hệ thống.");

            // Map 
            var confirmDict = confirmNames.ToDictionary(x => x.ID);

            var requestQuoteIds = confirmNames
                .Select(x => x.ID_RequestQuote)
                .Distinct()
                .ToList();

            // Load detail
            var details = await _context.BaoGia_Detail_of_Quotations
                .Where(x => requestQuoteIds.Contains(x.ID_RequestQuote))
                .ToListAsync();

            var detailDict = details
                .GroupBy(x => x.ID_RequestQuote)
                .ToDictionary(g => g.Key, g => g.First());

            // Load request quote
            var requestQuotes = await _context.BaoGia_Request_of_Quotations
                .Where(x => requestQuoteIds.Contains(x.ID))
                .ToListAsync();
            var requestDict = requestQuotes.GroupBy(x => x.ID)
                .ToDictionary(g => g.Key, g => g.First());

            // Cache link đã tồn tại trong DB để tránh lưu file trùng
            var inputLinks = baoGia
                .Select(x => x.LinkQ?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingLinkRows = await _context.BaoGia_Detail_of_Quotations
                .Where(x =>
                    (!string.IsNullOrEmpty(x.NVCHR_dataOld) && inputLinks.Contains(x.NVCHR_dataOld)) ||
                    (!string.IsNullOrEmpty(x.NVCHR_File) && inputLinks.Contains(x.NVCHR_File)))
                .Select(x => new { x.NVCHR_dataOld, x.NVCHR_File })
                .ToListAsync();

            var existingLinkMap = existingLinkRows
                .Where(x => !string.IsNullOrWhiteSpace(x.NVCHR_dataOld) && !string.IsNullOrWhiteSpace(x.NVCHR_File))
                .GroupBy(x => x.NVCHR_dataOld!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().NVCHR_File!, StringComparer.OrdinalIgnoreCase);

            bool hasChanges = false;
            var now = DateTime.Now;

            foreach (var item in baoGia)
            {
                if (!confirmDict.TryGetValue(item.ID, out var confirmName))
                    continue;

                bool rowChanged = false;

                // update detail
                if (detailDict.TryGetValue(confirmName.ID_RequestQuote, out var detail))
                {
                    if (!string.Equals(detail.NVCHR_TenHangHQ, item.VCHR_TenRecomment, StringComparison.Ordinal))
                    {
                        detail.NVCHR_TenHangHQ = item.VCHR_TenRecomment;
                        rowChanged = true;
                    }

                    if (!string.Equals(detail.CHR_NameEN, item.CHR_NameEN, StringComparison.Ordinal))
                    {
                        detail.CHR_NameEN = item.CHR_NameEN;
                        rowChanged = true;
                    }

                    var newLink = item.LinkQ?.Trim();
                    if (!string.IsNullOrWhiteSpace(newLink))
                    {
                        var sameAsCurrent =
                            string.Equals(detail.NVCHR_dataOld, newLink, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(detail.NVCHR_File, newLink, StringComparison.OrdinalIgnoreCase);

                        if (!sameAsCurrent)
                        {
                            if (existingLinkMap.TryGetValue(newLink, out var existedSavedPath) && !string.IsNullOrWhiteSpace(existedSavedPath))
                            {
                                detail.NVCHR_dataOld = newLink;
                                detail.NVCHR_File = existedSavedPath;
                                rowChanged = true;
                            }
                            else
                            {
                                var saveRes = await _fileImportService.SaveFileFromPathAsync(newLink);
                                if (!string.IsNullOrWhiteSpace(saveRes.Data))
                                {
                                    detail.NVCHR_dataOld = newLink;
                                    detail.NVCHR_File = saveRes.Data;
                                    existingLinkMap[newLink] = saveRes.Data;
                                    rowChanged = true;
                                }
                            }
                        }
                    }
                }

                // update request quote
                if (requestDict.TryGetValue(confirmName.ID_RequestQuote, out var requestQuote))
                {
                    if (!string.Equals(requestQuote.NVCHR_NameVN, item.VCHR_TenRecomment, StringComparison.Ordinal))
                    {
                        requestQuote.NVCHR_NameVN = item.VCHR_TenRecomment;
                        rowChanged = true;
                    }

                    if (requestQuote.ID_StepBaoGia != 13)
                    {
                        requestQuote.ID_StepBaoGia = 13;
                        rowChanged = true;
                    }

                    if (!string.Equals(requestQuote.ID_Status, "DONE", StringComparison.Ordinal))
                    {
                        requestQuote.ID_Status = "DONE";
                        rowChanged = true;
                    }

                    if (!string.Equals(requestQuote.CHR_NameEN, item.CHR_NameEN, StringComparison.Ordinal))
                    {
                        requestQuote.CHR_NameEN = item.CHR_NameEN;
                        rowChanged = true;
                    }

                }

                // update confirm name
                if (!string.Equals(confirmName.VCHR_TenRecomment, item.VCHR_TenRecomment, StringComparison.Ordinal))
                {
                    confirmName.VCHR_TenRecomment = item.VCHR_TenRecomment;
                    rowChanged = true;
                }

                if (!string.Equals(confirmName.CHR_Status, "Confirmed", StringComparison.Ordinal))
                {
                    confirmName.CHR_Status = "Confirmed";
                    rowChanged = true;
                }

                if (!string.Equals(confirmName.CHR_StatusShip, "Confirmed", StringComparison.Ordinal))
                {
                    confirmName.CHR_StatusShip = "Confirmed";
                    rowChanged = true;
                }

                if (!string.Equals(confirmName.VCHR_UserPUR, user, StringComparison.Ordinal))
                {
                    confirmName.VCHR_UserPUR = user;
                    rowChanged = true;
                }

                if (!string.Equals(confirmName.VCHR_UpdateBy, user, StringComparison.Ordinal))
                {
                    confirmName.VCHR_UpdateBy = user;
                    rowChanged = true;
                }

                if (!string.Equals(confirmName.CHR_NameEN, item.CHR_NameEN, StringComparison.Ordinal))
                {
                    confirmName.CHR_NameEN = item.CHR_NameEN;
                    rowChanged = true;
                }

                if (rowChanged)
                {
                    confirmName.DTM_UserPUR = now;
                    confirmName.DTM_UpdateDate = now;
                    hasChanges = true;
                }
            }

            if(hasChanges)
            {
                await _context.SaveChangesAsync();
            }
            return requestQuoteIds;
        }
        // Done
         public async Task<List<ResultCheckCofirmName>> DoneConfirmNameAsync(List<int> listDone)
        {
            if (listDone == null || !listDone.Any())
                throw new Exception("Invalid list of IDs.");
            var confirmNames = await _context.BaoGia_Request_of_Quotations
                .Where(x => listDone.Contains(x.ID))
                .ToListAsync();
            if (!confirmNames.Any())
                throw new Exception("No confirm names found.");
            // update
            foreach (var confirmName in confirmNames)
            {
                confirmName.ID_StepBaoGia = 13;
                confirmName.ID_Status = "DONE";
            }

            // get list
            string sql = @"
               SELECT DISTINCT r.CHR_MaDon as MaDon, r.CHR_SectionCode as Section, r.CHR_CreateBy as UserCreate, r.ID as ID
               FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] as r
               WHERE r.ID IN @ListCheck";

            var result = await _conn.QueryAsync<ResultCheckCofirmName>(sql, new { ListCheck = listDone });
            return result.ToList();
        }

        // Check đơn đã hoàn thành hay chưa
        public async Task<List<int>> CheckConfirmNameDoneAsync(List<int> listCheck)
        {
            if (listCheck == null || !listCheck.Any())
                throw new Exception("Invalid list of IDs.");

            string sql = @"
               SELECT c.ID
               FROM [COST_MANAGEMENT].[dbo].[BaoGia_Request_of_Quotation] as r
               LEFT JOIN [COST_MANAGEMENT].[dbo].[BaoGia_Confirm_Name_Quotation] as c 
                   ON r.ID = c.ID_RequestQuote
               WHERE r.ID_StepBaoGia = 13 and c.ID IN @ListCheck";

            using (var connection = _conn)
            {
                var result = await connection.QueryAsync<int>(sql, new { ListCheck = listCheck });
                return result.ToList();
            }
        }
    }
}
