using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class PoViewModel
    {
        public int Id { get; set; } // Số thứ tự hoặc ID
        public DateTime ReqDate { get; set; }
        public DateTime DeliveryReqDate { get; set; }
        public string? PoNumber { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? SupplierName { get; set; }
        public int LeadTime { get; set; }
        public string? Issuer { get; set; }
        public DateTime? SendPoDate { get; set; }
        public string? Follower { get; set; }
        public DateTime? ConfirmedDeliveryDate { get; set; }
        public string? ImpactToProduction { get; set; } // "Yes" hoặc "No"
        public string? Status { get; set; } // "Pending" hoặc "Received"
    }
    public class PoDetailViewModel
    {
        public int PO_Detail_Id { get; set; }
        public string? Ngayyc { get; set; }
        public string? Ngayycgiao { get; set; }
        public string? SoPO { get; set; }
        public string? Tentiengviet { get; set; }
        public string? Mahang { get; set; }
        public double Soluong { get; set; }
        public string? Donvi { get; set; }
        public string? Nhacungcap { get; set; }
        public string? MaNhacungcap { get; set; }
        public string? DNphathanhpo { get; set; }
        public string? ngayguiPO { get; set; } 
        public string? DNphongban { get; set; }
        public string? ngaynccxngiao { get; set; }
        public string? lichgiao { get; set; }
        public string? anhuongsx { get; set; }
        public string? trangthai { get; set; }
        public string? Danhmuc { get; set; }
        public string? LuongvekhoKhonhap { get; set; }
        public string? Ngay_GHchinhthuc { get; set; }
        public string? Gio_GH { get; set; }
        public string? Cua_GH { get; set; }
        public string? Cong_Nhanhang { get; set; }
        public string? Nguoi_Nhanhang { get; set; }   
        public string? SL_Thucte { get; set; }
        public string? So_DNTT { get; set; }
        public string? So_hoadon { get; set; }
        public string? canhbao { get; set; }
        public string? picpur { get; set; }
        public string? khoi { get; set; }
        public string? Dieuchinhlichgiao { get; set; }
        public string? Note { get; set; }
        public string? Code_Request { get; set; }
        public string? Good_Code { get; set; }
        public string? Phongchiuphi { get; set; }
    }
    public class UpdateDuKienModel
    {
        public string? PoNumber { get; set; }
        public int PoDetailId { get; set; } // Khóa chính của dòng detail
        public string? SendDate { get; set; }
        public string? ConfirmDate { get; set; }
        public string? Scope { get; set; }
    }
    public class UpdateChinhThucModel
    {
        public string PoNumber { get; set; }
        public int PoDetailId { get; set; }
        public string ActualDate { get; set; }  // YYYY-MM-DD từ input type="date"
        public string ActualTime { get; set; }  // HH:mm từ input type="time"
        public string CuaGiaoHang { get; set; }
        public string CongNhanHang { get; set; }
        public string NguoiNhanHang { get; set; }
        public string Scope { get; set; }       // "single" hoặc "full"
    }
    public class NccViewModel
    {
        public int Ncc_Id { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string Damnhiem { get; set; }
    }
    public class MaterialViewModel
    {
        public int Id_Material { get; set; }
        public string Material_Code { get; set; }
        public string Material_Name_VN { get; set; }
        public int LeadTime { get; set; }
    }
    public class UpdateThanhToanModel
    {
        public string PoNumber { get; set; }
        public int PoDetailId { get; set; }
        public double SlThucTe { get; set; }
        public string SoDntt { get; set; }
        public string SoHoaDon { get; set; }
        public string Scope { get; set; } // "full" hoặc "single"
    }
    public class UpdateAnhHuongSXModel
    {
        public int PoDetailId { get; set; }
        public string? ImpactStatus { get; set; } // Nhận các giá trị: "Yes", "No", "Wait"
    }
    public class UpdateDieuChinhLichGiaoModel
    {
        public string PoNumber { get; set; }
        public int PoDetailId { get; set; }
        public string DieuChinhDate { get; set; }
        public string Scope { get; set; }
    }
    public class NCC_NG
    {
        public string? mancc { get; set; }
        public string? tenncc { get; set; }
        public string? giogiao { get; set; }
        public string? soPO { get; set; }
    }
    public class NCC_NG_Detail
    {
        public string sopo { get; set; }
        public string mahang { get; set; }
        public string tenhang { get; set; }
        public string soluong { get; set; }
        public string donvi { get; set; }
        public string ngayycgiao { get; set; }
        public string ngaythuctegiao { get; set; }
        public string damnhiemxacnhananhhuong { get; set; }
        public string mancc { get; set; }
        public string tenncc { get; set; }
        public string ngaygiaochinhthuc { get; set; }
        public string giogiao { get; set; }
        public string cuagiao { get; set; }
        public string congnhanhang { get; set; }
        public string nguoinhanhang { get; set; }
    }
    public class LogChangeUsingViewModel
    {
        public int ID { get; set; }
        public string MaVatTu { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public double Soluong_Truoc { get; set; }
        public double Soluong_Sau { get; set; }
        public DateTime? Ngayupdate { get; set; }
        public string Nguoiupdate { get; set; }

        // Thuộc tính để hứng giá trị tính toán chênh lệch từ SQL
        public double ChenhLech { get; set; }
    }
    public class SplitPoRequest
    {
        public int PoDetailId { get; set; }
        public double Soluongmoi { get; set; }
    }
    public class UpdateInvoiceRequest
    {
        public List<string> ids { get; set; }
        public string loaiNhap { get; set; }
        public string duLieu { get; set; }
    }
    public class UpdateNoteModel
    {
        public int PoDetailId { get; set; }
        public string Note { get; set; }
    }
    public class ManagerDeliveryController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> ImportExcelTiendo(IFormFile excelFileInput)
        {
           
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (excelFileInput == null || excelFileInput.Length == 0)
            {
                return Json(new { success = false, message = "File không hợp lệ hoặc bị rỗng." });
            }

            string fileExtension = Path.GetExtension(excelFileInput.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                return Json(new { success = false, message = "Vui lòng chọn đúng định dạng file Excel (.xlsx hoặc .xls)" });
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await excelFileInput.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null) return Json(new { success = false, message = "Không tìm thấy Sheet dữ liệu." });

                        int rowCount = worksheet.Dimension.Rows;
                        List<string> errorRows = new List<string>();

                        // Hàm hỗ trợ format chuỗi ngày tháng thành dạng chuẩn SQL
                        string FormatSqlDate(string input)
                        {
                            if (string.IsNullOrWhiteSpace(input)) return "NULL";
                            return $"'{input.Replace("'", "''")}'";
                        }

                        // Hàm hỗ trợ format chuỗi văn bản (Chống lỗi dấu nháy đơn ' gây hỏng câu lệnh SQL)
                        string SafeString(string input)
                        {
                            if (string.IsNullOrWhiteSpace(input)) return "NULL";
                            return $"'{input.Replace("'", "''").Trim()}'";
                        }

                        bool IsValidDate(string dateStr)
                        {
                            if (string.IsNullOrWhiteSpace(dateStr)) return true;
                            return DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                        }

                        // KHỞI TẠO STRINGBUILDER ĐỂ GỘP QUERY
                        StringBuilder sqlBatch = new StringBuilder();
                        int batchSize = 500; // Số lượng dòng gộp lại để chạy 1 lần
                        int countProcess = 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            // Tối ưu: Dùng Value?.ToString() thay vì Text sẽ xử lý nhanh hơn trong EPPlus
                            string idDetailPo = worksheet.Cells[row, 1].Text?.ToString()?.Trim() ?? "";
                            string soPO = worksheet.Cells[row, 3].Text?.ToString()?.Trim() ?? "";

                            string rawNgaydieuchinh = worksheet.Cells[row, 14].Text?.ToString()?.Trim() ?? "";
                            string rawNgayGuiPo = worksheet.Cells[row, 11].Text?.ToString()?.Trim() ?? "";
                            string rawNgayNccXacnhanGh = worksheet.Cells[row, 15].Text?.ToString()?.Trim() ?? "";
                            string rawNgayGhChinhThuc = worksheet.Cells[row, 16].Text?.ToString()?.Trim() ?? "";
                          

                            if (!IsValidDate(rawNgayGuiPo) || !IsValidDate(rawNgayNccXacnhanGh) || !IsValidDate(rawNgayGhChinhThuc))
                            {
                                errorRows.Add($"- Dòng {row} (PO: {soPO})");
                                continue;
                            }

                            // Format SQL an toàn
                            string ngayGuiPo = FormatSqlDate(rawNgayGuiPo);
                            string ngayDieuchinh = FormatSqlDate(rawNgaydieuchinh);
                            string ngayNccXacnhanGh = FormatSqlDate(rawNgayNccXacnhanGh);
                            string ngayGhChinhThuc = FormatSqlDate(rawNgayGhChinhThuc);

                            string giogh = SafeString(worksheet.Cells[row, 17].Text?.ToString()!);
                            string lichGiao = SafeString(string.IsNullOrWhiteSpace(worksheet.Cells[row, 18].Text) ? null : worksheet.Cells[row, 18].Text.Trim());
                            string anhHuongSx = SafeString(string.IsNullOrWhiteSpace(worksheet.Cells[row, 19].Text) ? null : worksheet.Cells[row, 19].Text.Trim());
                            string cuaGh = SafeString(worksheet.Cells[row, 20].Text?.ToString()!);
                            string congNhanHang = SafeString(worksheet.Cells[row, 21].Text?.ToString()!);
                            string nguoiNhanHang = worksheet.Cells[row, 22].Text?.ToString()!;
                          
                            string Note = SafeString(worksheet.Cells[row, 23].Text?.ToString()!);

                            // Đưa câu lệnh vào bộ đệm (Không gọi DB ngay)
                            sqlBatch.AppendLine($@"
                                    IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE Id_Detail_PO = '{idDetailPo}')
                                    BEGIN
                                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                                        SET
                                            Ngay_gui_PO = {ngayGuiPo},
                                            Ngay_NCC_xacnhanGH = {ngayNccXacnhanGh},
                                            Ngay_GHchinhthuc = {ngayGhChinhThuc},
                                            Gio_GH = {giogh},
                                            Lichgiao = {lichGiao},
                                            Anh_huong_SX = {anhHuongSx},
                                            Cua_GH = {cuaGh},
                                            Cong_Nhanhang = {congNhanHang},
                                            Nguoi_Nhanhang = N'{nguoiNhanHang}',
                                            Dieuchinhlichgiao = {ngayDieuchinh},
                                            Note = {Note}
                                        WHERE Id_Detail_PO = '{idDetailPo}';
                                    END 
                                    ELSE 
                                    BEGIN                               
                                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                                        (
                                            SoPO, Id_Detail_PO, Ngay_gui_PO, Ngay_NCC_xacnhanGH, Ngay_GHchinhthuc, Gio_GH, 
                                            Lichgiao, Anh_huong_SX, Cua_GH, Cong_Nhanhang, Nguoi_Nhanhang, Dieuchinhlichgiao, Note
                                        )
                                        VALUES  
                                        (
                                            '{soPO}', '{idDetailPo}', {ngayGuiPo}, {ngayNccXacnhanGh}, {ngayGhChinhThuc}, {giogh},
                                            {lichGiao}, {anhHuongSx}, {cuaGh}, {congNhanHang}, N'{nguoiNhanHang}', {ngayDieuchinh}, {Note}
                                        );
                                    END;
                                ");
                            countProcess++;

                            // Thực thi SQL theo cụm (Batch) mỗi khi đủ 500 dòng
                            if (countProcess >= batchSize)
                            {
                                db.GET_DATA_FROM_SQL(sqlBatch.ToString());
                                sqlBatch.Clear(); // Dọn bộ đệm để chứa cụm tiếp theo
                                countProcess = 0;
                            }
                        }

                        // THỰC THI NHỮNG DÒNG CÒN LẠI (NẾU CÓ)
                        if (sqlBatch.Length > 0)
                        {
                            db.GET_DATA_FROM_SQL(sqlBatch.ToString());
                            sqlBatch.Clear();
                        }
                        // TRẢ VỀ THÔNG BÁO
                        if (errorRows.Count > 0)
                        {
                            string warningMsg = $"Import hoàn tất các dòng đúng.\nĐã bỏ qua {errorRows.Count} dòng do sai định dạng ngày (YYYY-MM-DD):\n";
                            if (errorRows.Count <= 10)
                            {
                                warningMsg += string.Join("\n", errorRows);
                            }
                            else
                            {
                                warningMsg += string.Join("\n", errorRows.Take(10)) + $"\n... và {errorRows.Count - 10} dòng khác.";
                            }
                            return Json(new { success = true, message = warningMsg });
                        }
                        tinhtoanlichgiao();
                        return Json(new { success = true, message = "Cập nhật thành công toàn bộ dữ liệu!" });
                    }
                }
              
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi đọc file: " + ex.Message });
            }
          
        }

        public string pullin_pullout(string manguyenlieu, string ngayyc)
        {
            // Nếu không có mã hàng hoặc ngày không đúng định dạng thì bỏ qua
            if (string.IsNullOrEmpty(manguyenlieu) || string.IsNullOrEmpty(ngayyc) || !DateTime.TryParse(ngayyc, out DateTime baseDate))
            {
                return "";
            }
            DateTime month1 = baseDate.AddMonths(1);
            DateTime month2 = baseDate.AddMonths(2);
            DateTime month3 = baseDate.AddMonths(3);

            // GOM 4 CÂU TRUY VẤN THÀNH 1 CÂU DUY NHẤT BẰNG SUBQUERY
            string query = $@"
                    SELECT 
                        (SELECT ISNULL(SUM(a.Soluong), 0) 
                         FROM [COST_MANAGEMENT].[dbo].[PO] AS a 
                         LEFT JOIN PE_THEODOITIENDO AS b ON a.PO_Detail_Id = b.Id_Detail_PO 
                         WHERE COALESCE(b.Ngay_GHchinhthuc, b.Ngay_NCC_xacnhanGH, a.Ngaygiaohangdukien) = '{ngayyc}' 
                           AND a.Mahang = '{manguyenlieu}' AND a.Danhmuc = 'IN') AS SoLuongPO,
                        (SELECT ISNULL(SUM(Hientai), 0) FROM KHO WHERE MaNguyenLieu = '{manguyenlieu}') AS Stock,             
                        (SELECT ISNULL(SUM(Soluong)/22.0, 0) 
                         FROM PE_Using 
                         WHERE Thang = '{baseDate.Month}' AND Nam = '{baseDate.Year}' AND MaVatTu = '{manguyenlieu}') AS UsingNgay,               
                        (SELECT ISNULL(SUM(Soluong), 0) 
                         FROM PE_Using 
                         WHERE MaVatTu = '{manguyenlieu}' 
                           AND (
                               (Thang = '{month1.Month}' AND Nam = '{month1.Year}') OR
                               (Thang = '{month2.Month}' AND Nam = '{month2.Year}') OR
                               (Thang = '{month3.Month}' AND Nam = '{month3.Year}')
                           )) AS Using3Thang ";

            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            var dt = sql.GET_DATA_FROM_SQL(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                double sl_po = Convert.ToDouble(dt.Rows[0]["SoLuongPO"]);
                double stock = Convert.ToDouble(dt.Rows[0]["Stock"]);
                double using_ngay = Convert.ToDouble(dt.Rows[0]["UsingNgay"]);
                double using3thang = Convert.ToDouble(dt.Rows[0]["Using3Thang"]);

                double canhbao = stock + sl_po - using_ngay;
                var trangThaiCanhBao = "";

                if (canhbao < 0)
                {
                    trangThaiCanhBao = "PullIn";
                }
                if (stock > using3thang)
                {
                    trangThaiCanhBao = "PullOut";
                }
                return trangThaiCanhBao;
            }

            return "";
        }

        public IActionResult ManageDelivery(int page = 1, string picpur = "", string searchTerm = "", string reqMonth = "", string tab = "", string impactStatus = "", string pullStatus = "", string sortColumn = "", string sortDirection = "asc", string mahang = "", string tenncc = "", string cost = "", string anhhuongsx = "")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            tinhtoanlichgiao();
            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");

            var khoi = "";
            if (checkus == "PUR") { khoi = "AND (a.Group_Code = 'PUR' OR a.Group_Code = 'PROD')"; }
            if (checkus == "GA") { khoi = "AND Group_Code = 'GA'"; }

            var get_sec = sql.ReturnString($"SELECT CHR_SECTION FROM [TM_USER] where CHR_USERID = '{us}'");
            var hientheophongban = "";
            if (get_sec == "3100" || get_sec == "1100") { }
            else { hientheophongban = @$" AND a.Phongchiuchiphi IN ( SELECT Cost_Center FROM [COST_MANAGEMENT].[dbo].[USER_DEPT] WHERE CHR_USERID = '{us}')"; }

            string tabCondition = "";
            if (string.IsNullOrEmpty(tab)) tab = "ngoai";
            if (tab == "trong") { tabCondition = "AND a.Danhmuc = 'IN'"; hientheophongban = ""; }
            if (tab == "ngoai") { tabCondition = "AND a.Danhmuc = 'OUT'"; }

            // --- XỬ LÝ ĐIỀU KIỆN TÌM KIẾM VÀ THÁNG ---
            string mainCondition = "1=1";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                reqMonth = ""; // Clear reqMonth khi dùng từ khóa
                string s = searchTerm.Replace("'", "''");
                mainCondition = $"(a.SoPO LIKE '%{s}%' OR a.Mahang LIKE '%{s}%' OR a.Tentiengviet LIKE N'%{s}%' OR a.TenNCC LIKE N'%{s}%')";
            }
            else
            {
                if (string.IsNullOrEmpty(reqMonth))
                {
                    mainCondition = $"Ngayphathanh >= '2024-01-01'";
                }
                else
                {
                    var monthParts = reqMonth.Split('-');
                    if (monthParts.Length == 2)
                    {
                        mainCondition = $"MONTH(Ngayphathanh) = '{monthParts[1]}' AND YEAR(Ngayphathanh) = '{monthParts[0]}'";
                    }
                }
            }

            if (!string.IsNullOrEmpty(tenncc)) { mainCondition += $" AND a.TenNCC LIKE N'%{tenncc.Replace("'", "''")}%'"; }
            if (!string.IsNullOrEmpty(cost)) { mainCondition += $" AND a.Phongchiuchiphi = N'{cost.Replace("'", "''")}'"; }
            if (!string.IsNullOrEmpty(anhhuongsx)) { mainCondition += $" AND b.Anh_huong_SX = N'{anhhuongsx.Replace("'", "''")}'"; }

            string query = $@"SELECT a.*, b.*, c.Damnhiem, d.User_Create FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                OUTER APPLY (
                    SELECT TOP 1 * FROM PE_THEODOITIENDO WHERE Id_Detail_PO = a.PO_Detail_Id 
                    ORDER BY Ngay_NCC_xacnhanGH DESC 
                ) as b
                OUTER APPLY (
                    SELECT TOP 1 Damnhiem FROM PE_DamnhiemNCC WHERE MaNCC = a.MaNCC
                ) as c
                LEFT JOIN REQUEST as d on a.Code_Request = d.Code_Request
                WHERE {mainCondition} {khoi} {tabCondition}
                AND Ngayphathanh >= '2024-01-01'
                AND TinhtrangPO <> 'HOANTHANH' AND TinhtrangPO <> 'HUY' AND Luongvekho is null
                {hientheophongban} ORDER BY a.Ngaytao DESC";

            var lst = sql.GET_DATA_FROM_SQL(query);

            List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

            // [BƯỚC 1]: ĐỔ DỮ LIỆU TỪ SQL VÀO LIST TRƯỚC
            if (lst != null)
            {
                for (int i = 0; i < lst.Rows.Count; i++)
                {
                    PoDetailViewModel po = new PoDetailViewModel();
                    po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString()!);
                    object valNgaytao = lst.Rows[i]["Ngaytao"];
                    po.Ngayyc = (valNgaytao != null && valNgaytao != DBNull.Value) ? Convert.ToDateTime(valNgaytao).ToString("yyyy-MM-dd") : "";

                    object valNgayyc = lst.Rows[i]["Ngaygiaohangdukien"];
                    po.Ngayycgiao = (valNgayyc != null && valNgayyc != DBNull.Value) ? Convert.ToDateTime(valNgayyc).ToString("yyyy-MM-dd") : "";

                    po.SoPO = lst.Rows[i]["SoPO"].ToString();
                    po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                    po.Mahang = lst.Rows[i]["Mahang"].ToString();
                    po.Soluong = double.TryParse(lst.Rows[i]["Soluong"].ToString(), out double sl) ? sl : 0;
                    po.Donvi = lst.Rows[i]["Dovi"].ToString();
                    po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                    po.DNphathanhpo = lst.Rows[i]["User_Create"].ToString()?.ToLower();
                    po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();
                    po.MaNhacungcap = lst.Rows[i]["MaNCC"].ToString();
                    po.Phongchiuphi = lst.Rows[i]["Phongchiuchiphi"].ToString();
                    po.picpur = lst.Rows[i]["Damnhiem"].ToString();

                    object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                    po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";

                    object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                    po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";

                    po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                    po.lichgiao = lst.Rows[i]["Lichgiao"].ToString();
                    po.trangthai = "";
                    po.LuongvekhoKhonhap = lst.Rows[i]["LuongvekhoKhonhap"].ToString();
                    po.Danhmuc = lst.Rows[i]["Danhmuc"].ToString();

                    object valNgaydieuchinh = lst.Rows[i]["Dieuchinhlichgiao"];
                    po.Dieuchinhlichgiao = (valNgaydieuchinh != null && valNgaydieuchinh != DBNull.Value) ? Convert.ToDateTime(valNgaydieuchinh).ToString("yyyy-MM-dd") : "";

                    po.Note = lst.Rows[i]["Note"].ToString();
                    po.Good_Code = lst.Rows[i]["Good_Code"].ToString();
                    po.Code_Request = lst.Rows[i]["Code_Request"].ToString();

                    object valNgayGH = lst.Rows[i]["Ngay_GHchinhthuc"];
                    po.Ngay_GHchinhthuc = (valNgayGH != null && valNgayGH != DBNull.Value) ? Convert.ToDateTime(valNgayGH).ToString("yyyy-MM-dd") : "";

                    po.Gio_GH = lst.Rows[i]["Gio_GH"].ToString();
                    po.Cua_GH = lst.Rows[i]["Cua_GH"].ToString();
                    po.Cong_Nhanhang = lst.Rows[i]["Cong_Nhanhang"].ToString();
                    po.Nguoi_Nhanhang = lst.Rows[i]["Nguoi_Nhanhang"].ToString();
                    po.SL_Thucte = lst.Rows[i]["SL_Thucte"].ToString();
                    po.So_DNTT = lst.Rows[i]["So_DNTT"].ToString();
                    po.So_hoadon = lst.Rows[i]["So_hoadon"].ToString();
                    po.khoi = lst.Rows[i]["Group_Code"].ToString();
                    po.canhbao = "";
                    listPo.Add(po);
                }
            }

            // [BƯỚC 2]: TÍNH TOÁN VÀ LỌC THEO PULL IN/PULL OUT NẾU LÀ TAB TRONG
            if (tab == "trong")
            {
                var uniqueItems = listPo.Select(x => x.Mahang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                if (uniqueItems.Any())
                {
                    string inClause = string.Join(",", uniqueItems.Select(x => $"'{x}'"));

                    var dictKho = new Dictionary<string, double>();
                    var dtKho = sql.GET_DATA_FROM_SQL($"SELECT MaNguyenLieu, ISNULL(SUM(Hientai), 0) AS Stock FROM KHO WHERE MaNguyenLieu IN ({inClause}) GROUP BY MaNguyenLieu");
                    if (dtKho != null)
                    {
                        foreach (System.Data.DataRow r in dtKho.Rows)
                            dictKho[r["MaNguyenLieu"].ToString()!] = Convert.ToDouble(r["Stock"]);
                    }

                    var dictUsing = new Dictionary<string, double>();
                    var dtUsing = sql.GET_DATA_FROM_SQL($"SELECT MaVatTu, Thang, Nam, ISNULL(SUM(Soluong), 0) AS Soluong FROM PE_Using WHERE MaVatTu IN ({inClause}) GROUP BY MaVatTu, Thang, Nam");
                    if (dtUsing != null)
                    {
                        foreach (System.Data.DataRow r in dtUsing.Rows)
                            dictUsing[$"{r["MaVatTu"]}_{r["Thang"]}_{r["Nam"]}"] = Convert.ToDouble(r["Soluong"]);
                    }

                    var dictPoSum = new Dictionary<string, double>();
                    var dtPoSum = sql.GET_DATA_FROM_SQL($@"
                    SELECT a.Mahang, 
                            COALESCE(b.Ngay_GHchinhthuc, b.Ngay_NCC_xacnhanGH, a.Ngaygiaohangdukien) AS Ngay,
                            ISNULL(SUM(a.Soluong), 0) AS SoLuongPO
                    FROM [COST_MANAGEMENT].[dbo].[PO] AS a 
                    LEFT JOIN PE_THEODOITIENDO AS b ON a.PO_Detail_Id = b.Id_Detail_PO 
                    WHERE a.Danhmuc = 'IN' AND a.Mahang IN ({inClause})
                    GROUP BY a.Mahang, COALESCE(b.Ngay_GHchinhthuc, b.Ngay_NCC_xacnhanGH, a.Ngaygiaohangdukien)");
                    if (dtPoSum != null)
                    {
                        foreach (System.Data.DataRow r in dtPoSum.Rows)
                        {
                            if (r["Ngay"] != DBNull.Value && DateTime.TryParse(r["Ngay"].ToString(), out DateTime dtParsed))
                            {
                                dictPoSum[$"{r["Mahang"]}_{dtParsed:yyyy-MM-dd}"] = Convert.ToDouble(r["SoLuongPO"]);
                            }
                        }
                    }

                    var filteredByPull = new List<PoDetailViewModel>();
                    foreach (var po in listPo)
                    {
                        string ngayUuTien = !string.IsNullOrEmpty(po.Ngay_GHchinhthuc) ? po.Ngay_GHchinhthuc :
                                            !string.IsNullOrEmpty(po.ngaynccxngiao) ? po.ngaynccxngiao :
                                            po.Ngayycgiao!;

                        if (!string.IsNullOrEmpty(po.Mahang) && DateTime.TryParse(ngayUuTien, out DateTime baseDate))
                        {
                            DateTime month1 = baseDate.AddMonths(1);
                            DateTime month2 = baseDate.AddMonths(2);
                            DateTime month3 = baseDate.AddMonths(3);

                            double stock = dictKho.ContainsKey(po.Mahang) ? dictKho[po.Mahang] : 0;
                            double sl_po = dictPoSum.ContainsKey($"{po.Mahang}_{baseDate:yyyy-MM-dd}") ? dictPoSum[$"{po.Mahang}_{baseDate:yyyy-MM-dd}"] : 0;

                            string uBase = $"{po.Mahang}_{baseDate.Month}_{baseDate.Year}";
                            double using_ngay = dictUsing.ContainsKey(uBase) ? dictUsing[uBase] / 22.0 : 0;

                            double using3thang = (dictUsing.ContainsKey($"{po.Mahang}_{month1.Month}_{month1.Year}") ? dictUsing[$"{po.Mahang}_{month1.Month}_{month1.Year}"] : 0) +
                                                 (dictUsing.ContainsKey($"{po.Mahang}_{month2.Month}_{month2.Year}") ? dictUsing[$"{po.Mahang}_{month2.Month}_{month2.Year}"] : 0) +
                                                 (dictUsing.ContainsKey($"{po.Mahang}_{month3.Month}_{month3.Year}") ? dictUsing[$"{po.Mahang}_{month3.Month}_{month3.Year}"] : 0);

                            double canhbao = stock + sl_po - using_ngay;
                            if (canhbao < 0) po.canhbao = "PullIn";
                            else if (stock > using3thang) po.canhbao = "PullOut";
                        }

                        if (string.IsNullOrEmpty(pullStatus) ||
                           (pullStatus == "in" && po.canhbao == "PullIn") ||
                           (pullStatus == "out" && po.canhbao == "PullOut"))
                        {
                            filteredByPull.Add(po);
                        }
                    }
                    listPo = filteredByPull;
                }
            }

            // Các bộ lọc phụ C# Memory
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                listPo = listPo.Where(x =>
                    (x.SoPO?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Tentiengviet?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Nhacungcap?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Mahang?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(reqMonth))
            {
                listPo = listPo.Where(x => {
                    if (DateTime.TryParse(x.Ngayycgiao, out DateTime dt)) return dt.ToString("yyyy-MM") == reqMonth;
                    return false;
                }).ToList();
            }

            if (!string.IsNullOrEmpty(impactStatus))
            {
                if (impactStatus == "WAIT")
                    listPo = listPo.Where(x => !string.IsNullOrEmpty(x.lichgiao) && x.lichgiao.Trim().ToUpper() == "NG" && string.IsNullOrEmpty(x.anhuongsx)).ToList();
                else
                    listPo = listPo.Where(x => x.lichgiao != null && x.lichgiao.Trim().Equals(impactStatus)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(picpur))
            {
                listPo = listPo.Where(x => x.picpur != null && x.picpur.Contains(picpur)).ToList();
            }

            if (!string.IsNullOrEmpty(mahang))
            {
                listPo = listPo.Where(x => !string.IsNullOrEmpty(x.Mahang) && x.Mahang.ToUpper().StartsWith(mahang.ToUpper())).ToList();
            }

            // Phân trang
            int pageSize = 100;
            int totalRecords = listPo.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedList = listPo.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // [BƯỚC 3]: ĐẨY ĐẦY ĐỦ CÁC BỘ LỌC RA VIEWBAG NỔI BẬT LÊN TRÊN DÒNG PHÂN TRANG
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentTab = tab;
            ViewBag.ReqMonth = reqMonth;
            ViewBag.ImpactStatus = impactStatus;
            ViewBag.PullStatus = pullStatus;
            ViewBag.PicPur = picpur;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.Mahang = mahang;
            ViewBag.TenNcc = tenncc;
            ViewBag.Cost = cost;
            ViewBag.AnhHuongSx = anhhuongsx;

            TempData["Tongsoluong"] = totalRecords;

            return View(pagedList);
        }

        [HttpPost]
        public IActionResult UpdateDuKien([FromBody] UpdateDuKienModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    return Json(new { success = false, message = "Số PO không hợp lệ." });
                }

                string sqlQuery = "";

                // THAY ĐỔI Ở ĐÂY: Nếu ngày để trống -> Ép hẳn thành từ khóa SQL 'NULL' để update xuống DB
                string updateSendDate = string.IsNullOrEmpty(model.SendDate) ? "NULL" : $"'{model.SendDate}'";
                string updateConfirmDate = string.IsNullOrEmpty(model.ConfirmDate) ? "NULL" : $"'{model.ConfirmDate}'";

                // Biến dùng cho INSERT (giữ nguyên logic truyền NULL nếu trống)
                string insertSendDate = string.IsNullOrEmpty(model.SendDate) ? "NULL" : $"'{model.SendDate}'";
                string insertConfirmDate = string.IsNullOrEmpty(model.ConfirmDate) ? "NULL" : $"'{model.ConfirmDate}'";

                if (model.Scope == "single")
                {
                    sqlQuery = $@"
                    IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {model.PoDetailId})
                    BEGIN
                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                        SET [Ngay_gui_PO] = {updateSendDate},
                            [Ngay_NCC_xacnhanGH] = {updateConfirmDate}
                        WHERE [Id_Detail_PO] = {model.PoDetailId}
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                            ([SoPO], [Id_Detail_PO], [Ngay_gui_PO], [Ngay_NCC_xacnhanGH])
                        VALUES 
                            ('{model.PoNumber}', {model.PoDetailId}, {insertSendDate}, {insertConfirmDate})
                    END";
                        }
                        else
                        {
                            sqlQuery = $@"
                    -- 1. Cập nhật thông tin ngày tháng cho những dòng đã tồn tại (nếu trống sẽ thành NULL)
                    UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                    SET [Ngay_gui_PO] = {updateSendDate},
                        [Ngay_NCC_xacnhanGH] = {updateConfirmDate}
                    WHERE [SoPO] = '{model.PoNumber}';

                    -- 2. Chèn vào nếu chưa tồn tại
                    INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                        ([SoPO], [Id_Detail_PO], [Ngay_gui_PO], [Ngay_NCC_xacnhanGH])
                    SELECT 
                        [SoPO], 
                        [PO_Detail_Id], 
                        {insertSendDate}, 
                        {insertConfirmDate}
                    FROM [COST_MANAGEMENT].[dbo].[PO]
                    WHERE [SoPO] = '{model.PoNumber}'
                      AND [PO_Detail_Id] NOT IN (
                          SELECT [Id_Detail_PO] 
                          FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                          WHERE [SoPO] = '{model.PoNumber}');";
                }

                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);
                tinhtoanlichgiao();
                return Json(new { success = true, message = "Cập nhật tiến độ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // Đặt giá trị mặc định sopo = "" để có thể gọi hàm cập nhật cho toàn bộ bảng nếu cần
        public void tinhtoanlichgiao(string sopo = "")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // Điều kiện WHERE động: Nếu truyền vào SoPO thì chỉ tính cho PO đó, nếu không thì tính hết
            string whereClause = string.IsNullOrEmpty(sopo)
                                 ? ""
                                 : $"WHERE a.SoPO = '{sopo}'";
            string query = $@"
                    UPDATE b SET 
                        b.Lichgiao = 
                        CASE 
                            -- 0. NẾU CHƯA CÓ NGÀY XÁC NHẬN TỪ NCC -> BẮT BUỘC LÀ NULL
                            WHEN b.Ngay_NCC_xacnhanGH IS NULL OR LTRIM(RTRIM(b.Ngay_NCC_xacnhanGH)) = '' THEN NULL

                            -- 1. XÉT ƯU TIÊN NGÀY GIAO CHÍNH THỨC TRƯỚC (NẾU CÓ)
                            WHEN b.Ngay_GHchinhthuc IS NOT NULL AND LTRIM(RTRIM(b.Ngay_GHchinhthuc)) <> '' THEN
                                CASE 
                                    -- 1.1 Hàng IN: Ngày chính thức phải BẰNG CHÍNH XÁC Ngày Đích (TargetDate)
                                    WHEN a.Danhmuc = 'IN' 
                                         AND TRY_CAST(b.Ngay_GHchinhthuc AS DATE) = v.TargetDate
                                    THEN 'OK'
            
                                    -- 1.2 Hàng OUT (hoặc các loại khác): Ngày chính thức chỉ cần CÙNG THÁNG/NĂM với Ngày Đích
                                    WHEN a.Danhmuc = 'OUT'
                                         AND MONTH(TRY_CAST(b.Ngay_GHchinhthuc AS DATE)) = MONTH(v.TargetDate)
                                         AND YEAR(TRY_CAST(b.Ngay_GHchinhthuc AS DATE)) = YEAR(v.TargetDate)
                                    THEN 'OK'
            
                                    -- Còn lại của ngày chính thức -> NG
                                    ELSE 'NG'
                                END

                            -- 2. NẾU KHÔNG CÓ NGÀY CHÍNH THỨC -> XÉT THEO NGÀY NCC XÁC NHẬN
                            WHEN b.Ngay_NCC_xacnhanGH IS NOT NULL AND LTRIM(RTRIM(b.Ngay_NCC_xacnhanGH)) <> '' THEN
                                CASE
                                    -- 2.1 Hàng IN: Bằng chính xác Ngày Đích -> OK
                                    WHEN a.Danhmuc = 'IN' 
                                         AND TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE) = v.TargetDate 
                                    THEN 'OK'

                                    -- 2.2 Hàng OUT: Trường hợp cùng tháng/năm -> OK
                                    WHEN a.Danhmuc = 'OUT' 		
                                         AND MONTH(v.TargetDate) = MONTH(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                         AND YEAR(v.TargetDate) = YEAR(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                    THEN 'OK'
            
                                    -- 2.3 Còn lại -> NG
                                    ELSE 'NG'
                                END

                            ELSE b.Lichgiao -- Giữ nguyên nếu không lọt vào case nào
                        END,        

                        b.Anh_huong_SX = 
                        CASE 
                            -- Nếu chưa có ngày xác nhận từ NCC -> Giữ nguyên trạng thái cũ
                            WHEN b.Ngay_NCC_xacnhanGH IS NULL OR LTRIM(RTRIM(b.Ngay_NCC_xacnhanGH)) = '' THEN b.Anh_huong_SX

                            -- Nếu có Ngày chính thức và thỏa mãn điều kiện OK -> 'No'
                            WHEN b.Ngay_GHchinhthuc IS NOT NULL AND LTRIM(RTRIM(b.Ngay_GHchinhthuc)) <> '' THEN
                                CASE 
                                    WHEN (a.Danhmuc = 'IN' AND TRY_CAST(b.Ngay_GHchinhthuc AS DATE) = v.TargetDate)
                                         OR (a.Danhmuc = 'OUT' AND MONTH(TRY_CAST(b.Ngay_GHchinhthuc AS DATE)) = MONTH(v.TargetDate) AND YEAR(TRY_CAST(b.Ngay_GHchinhthuc AS DATE)) = YEAR(v.TargetDate))
                                    THEN 'No'
                                    ELSE b.Anh_huong_SX
                                END

                            -- Nếu có ngày xác nhận NCC và thỏa mãn điều kiện OK -> 'No'
                            WHEN b.Ngay_NCC_xacnhanGH IS NOT NULL AND LTRIM(RTRIM(b.Ngay_NCC_xacnhanGH)) <> '' THEN
                                CASE 
                                    WHEN (a.Danhmuc = 'IN' AND TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE) = v.TargetDate)
                                         OR (a.Danhmuc = 'OUT' AND MONTH(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE)) = MONTH(v.TargetDate) AND YEAR(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE)) = YEAR(v.TargetDate))
                                    THEN 'No'
                                    ELSE b.Anh_huong_SX
                                END        

                            ELSE b.Anh_huong_SX 
                        END
                    FROM PE_THEODOITIENDO b
                    JOIN [COST_MANAGEMENT].[dbo].[PO] a ON a.PO_Detail_Id = b.Id_Detail_PO
        
                    -- CROSS APPLY tự động tạo v.TargetDate: 
                    -- Hàm COALESCE sẽ lấy Điều Chỉnh, nếu Điều Chỉnh rỗng thì nó lấy Dự Kiến.
                    CROSS APPLY (
                        SELECT TRY_CAST(COALESCE(
                            NULLIF(LTRIM(RTRIM(b.Dieuchinhlichgiao)), ''), 
                            NULLIF(LTRIM(RTRIM(a.Ngaygiaohangdukien)), '')
                        ) AS DATE) AS TargetDate
                    ) v
                    {whereClause};
                ";

            sql.GET_DATA_FROM_SQL(query);
        }
        [HttpPost]
        public IActionResult UpdateChinhThuc([FromBody] UpdateChinhThucModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    return Json(new { success = false, message = "Số PO không hợp lệ." });
                }

                // Kết hợp Ngày và Giờ giao hàng nếu cần thiết, hoặc để riêng
                string fullActualDate = model.ActualDate;
                if (!string.IsNullOrEmpty(model.ActualTime))
                {
                    fullActualDate += " " + model.ActualTime;
                }

                string sqlQuery = "";

                if (model.Scope == "single")
                {
                    // 1. Chỉ cập nhật hoặc thêm mới cho duy nhất 1 dòng chi tiết được chọn
                    sqlQuery = $@"
                    IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {model.PoDetailId})
                    BEGIN
                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                        SET [Ngay_GHchinhthuc] = '{model.ActualDate}',
                            [Gio_GH] = '{model.ActualTime}',
                            [Cua_GH] = N'{model.CuaGiaoHang}',
                            [Cong_Nhanhang] = N'{model.CongNhanHang}',
                            [Nguoi_Nhanhang] = N'{model.NguoiNhanHang}',
                            [Anh_huong_SX] = NULL
                        WHERE [Id_Detail_PO] = {model.PoDetailId}
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                            ([SoPO], [Id_Detail_PO], [Ngay_GHchinhthuc], [Gio_GH], [Cua_GH], [Cong_Nhanhang],[Nguoi_Nhanhang])
                        VALUES 
                            ('{model.PoNumber}', {model.PoDetailId}, '{model.ActualDate}', N'{model.ActualTime}', N'{model.CuaGiaoHang}', N'{model.CongNhanHang}', N'{model.NguoiNhanHang}')
                    END";
                }
                else
                {
                    sqlQuery = $@"
                        -- Bước 2.1: Cập nhật thông tin thực nhận cho những dòng đã tồn tại sẵn
                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                        SET [Ngay_GHchinhthuc] = '{model.ActualDate}',
                            [Gio_GH] = '{model.ActualTime}',
                            [Cua_GH] = N'{model.CuaGiaoHang}',
                            [Cong_Nhanhang] = N'{model.CongNhanHang}',
                            [Nguoi_Nhanhang] = N'{model.NguoiNhanHang}',
                            [Anh_huong_SX] = NULL   
                        WHERE [SoPO] = '{model.PoNumber}';

                        -- Bước 2.2: Chèn mới những dòng chi tiết thuộc PO này chưa có trong bảng tiến độ (Đã sửa lỗi lệch cột và sai biến)
                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                            ([SoPO], [Id_Detail_PO], [Ngay_GHchinhthuc], [Gio_GH], [Cua_GH], [Cong_Nhanhang], [Nguoi_Nhanhang])
                        SELECT 
                            [SoPO], 
                            [PO_Detail_Id], 
                            '{model.ActualDate}', 
                            '{model.ActualTime}', 
                            N'{model.CuaGiaoHang}', 
                            N'{model.CongNhanHang}', 
                            N'{model.NguoiNhanHang}'
                        FROM [COST_MANAGEMENT].[dbo].[PO]
                        WHERE [SoPO] = '{model.PoNumber}'
                          AND [PO_Detail_Id] NOT IN (
                              SELECT [Id_Detail_PO] 
                              FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                              WHERE [SoPO] = '{model.PoNumber}');";
                }

                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);
                tinhtoanlichgiao();
                return Json(new { success = true, message = "Cập nhật dữ liệu nhận hàng thực tế thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
           
        }
        // Tạo Model để nhận dữ liệu JSON gửi lên
        [HttpPost]
        public IActionResult UpdateDieuChinhLichGiao([FromBody] UpdateDieuChinhLichGiaoModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    return Json(new { success = false, message = "Số PO không hợp lệ." });
                }
                if (string.IsNullOrEmpty(model.DieuChinhDate))
                {
                    return Json(new { success = false, message = "Vui lòng nhập ngày điều chỉnh lịch giao." });
                }

                string sqlQuery = "";

                if (model.Scope == "single")
                {
                    // 1. Chỉ cập nhật hoặc thêm mới cho duy nhất 1 dòng chi tiết được chọn
                    sqlQuery = $@"
                        IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {model.PoDetailId})
                        BEGIN
                            UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                            SET [Dieuchinhlichgiao] = '{model.DieuChinhDate}'
                            WHERE [Id_Detail_PO] = {model.PoDetailId}
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                                ([SoPO], [Id_Detail_PO], [Dieuchinhlichgiao])
                            VALUES 
                                ('{model.PoNumber}', {model.PoDetailId}, '{model.DieuChinhDate}')
                        END";
                }
                else
                {
                    // 2. Cập nhật cho tất cả các mã hàng thuộc PO này
                    sqlQuery = $@"
                        -- Bước 2.1: Cập nhật ngày điều chỉnh cho những dòng đã tồn tại sẵn
                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                        SET [Dieuchinhlichgiao] = '{model.DieuChinhDate}'
                        WHERE [SoPO] = '{model.PoNumber}';

                        -- Bước 2.2: Chèn mới những dòng chi tiết thuộc PO này chưa có trong bảng tiến độ
                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                            ([SoPO], [Id_Detail_PO], [Dieuchinhlichgiao])
                        SELECT 
                            [SoPO], 
                            [PO_Detail_Id], 
                            '{model.DieuChinhDate}'
                        FROM [COST_MANAGEMENT].[dbo].[PO]
                        WHERE [SoPO] = '{model.PoNumber}'
                          AND [PO_Detail_Id] NOT IN (
                              SELECT [Id_Detail_PO] 
                              FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                              WHERE [SoPO] = '{model.PoNumber}'
                          );";
                }

                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);

                // Gọi hàm tính toán lại lịch giao OK/NG nếu hệ thống của bạn yêu cầu

                tinhtoanlichgiao();
                return Json(new { success = true, message = "Cập nhật ngày điều chỉnh lịch giao thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    
        public class UpdateThanhToanModel
        {
            public string PoNumber { get; set; }
            public int PoDetailId { get; set; }
            public double SlThucTe { get; set; }
            public string SoDntt { get; set; }
            public string SoHoaDon { get; set; }
            public string Scope { get; set; } // "full" hoặc "single"
        }
        // Action xử lý thanh toán
        [HttpPost]
        public IActionResult UpdateThanhToan([FromBody] UpdateThanhToanModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PoNumber))
                {
                    return Json(new { success = false, message = "Số PO không hợp lệ." });
                }

                string sqlQuery = "";

                if (model.Scope == "single")
                {
                    // 1. CHỈ CẬP NHẬT 1 MÃ: Sử dụng số lượng nhập từ giao diện (model.SlThucTe)
                    sqlQuery = $@"
                    IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {model.PoDetailId})
                    BEGIN
                        UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                        SET [SL_Thucte] = {model.SlThucTe},
                            [So_DNTT] = N'{model.SoDntt}',
                            [So_hoadon] = N'{model.SoHoaDon}'
                        WHERE [Id_Detail_PO] = {model.PoDetailId}
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                            ([SoPO], [Id_Detail_PO], [SL_Thucte], [So_DNTT], [So_hoadon])
                        VALUES 
                            ('{model.PoNumber}', {model.PoDetailId}, {model.SlThucTe}, N'{model.SoDntt}', N'{model.SoHoaDon}')
                    END";
                }
                else
                {
                    // 2. ÁP DỤNG FULL PO: SL_ThucTe = Số lượng của TỪNG MÃ trong bảng PO
                    sqlQuery = $@"
                    -- Bước 2.1: Cập nhật cho những dòng đã tồn tại (Lấy số lượng gốc từ bảng PO)
                    UPDATE T
                    SET T.[SL_Thucte] = P.[Soluong],
                        T.[So_DNTT] = N'{model.SoDntt}',
                        T.[So_hoadon] = N'{model.SoHoaDon}'
                    FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] T
                    INNER JOIN [COST_MANAGEMENT].[dbo].[PO] P ON T.[Id_Detail_PO] = P.[PO_Detail_Id]
                    WHERE T.[SoPO] = '{model.PoNumber}';

                    -- Bước 2.2: Chèn mới những dòng chưa có trong bảng PE_THEODOITIENDO
                    INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                        ([SoPO], [Id_Detail_PO], [SL_Thucte], [So_DNTT], [So_hoadon])
                    SELECT 
                        [SoPO], 
                        [PO_Detail_Id], 
                        [Soluong], -- Lấy linh động đúng số lượng của từng mã từ bảng PO
                        N'{model.SoDntt}', 
                        N'{model.SoHoaDon}'
                    FROM [COST_MANAGEMENT].[dbo].[PO]
                    WHERE [SoPO] = '{model.PoNumber}'
                      AND [PO_Detail_Id] NOT IN (
                          SELECT [Id_Detail_PO] 
                          FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                          WHERE [SoPO] = '{model.PoNumber}'
                      );";
                }

                // Thực thi SQL
                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);
        
                return Json(new { success = true, message = "Cập nhật dữ liệu thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpGet] // Hoặc [HttpGet] tùy thuộc vào form của bạn ở View đang dùng gì
        public IActionResult ExportExcel(string searchTerm = "", string picpur = "", string reqMonth = "", string tab = "ngoai", string impactStatus = "", string pullStatus = "", string mahang = "", string tenncc= "" , string cost = "", string anhhuongsx = "")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            tinhtoanlichgiao();
            // 2. PHÂN QUYỀN VÀ ĐIỀU KIỆN LỌC
            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");
            var khoi = "";
          
            if (checkus == "PUR") { khoi = "AND (a.Group_Code = 'PUR' OR a.Group_Code = 'PROD')"; }
            if (checkus == "GA") { khoi = "AND Group_Code = 'GA'"; }
            var get_sec = sql.ReturnString($"SELECT CHR_SECTION  FROM [TM_USER] where CHR_USERID = '{us}'");
            var hientheophongban = "";
            if (get_sec == "3100" || get_sec == "1100") { }
            else { hientheophongban = @$" AND a.Phongchiuchiphi IN ( SELECT Cost_Center FROM [COST_MANAGEMENT].[dbo].[USER_DEPT] WHERE CHR_USERID = '{us}')"; }
            string tabCondition = "";
            if (string.IsNullOrEmpty(tab))
            {
                tab = "ngoai";
            }
            if (tab == "trong") { tabCondition = "AND a.Danhmuc = 'IN'"; };
            if (tab == "ngoai") { tabCondition = "AND a.Danhmuc = 'OUT'"; };

            if (tab == "trong")
            {
                hientheophongban = "";
            }
            ;
            string mainCondition = "1=1";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                reqMonth = "";
                string s = searchTerm.Replace("'", "''");
                mainCondition = $"(a.SoPO LIKE '%{s}%' OR a.Mahang LIKE '%{s}%' OR a.Tentiengviet LIKE N'%{s}%' OR a.TenNCC LIKE N'%{s}%')";
            }
            else
            {             
                if (reqMonth == "")
                {
                    mainCondition = $"Ngayphathanh >= '2024-01-01'";
                }
                else
                {
                    mainCondition = $"MONTH(Ngayphathanh) = '{reqMonth}'";
                }
                  
            }
            if (!string.IsNullOrEmpty(tenncc)) { mainCondition += $" AND a.TenNCC LIKE N'%{tenncc.Replace("'", "''")}%'"; }
            if (!string.IsNullOrEmpty(cost)) { mainCondition += $" AND a.Phongchiuchiphi = N'{cost.Replace("'", "''")}'"; }
            if (!string.IsNullOrEmpty(anhhuongsx)) { mainCondition += $" AND b.Anh_huong_SX = N'{anhhuongsx.Replace("'", "''")}'"; }
            // Đã thêm điều kiện loại bỏ HOANTHANH và HUY giống ManageDelivery
            string query = $@"SELECT a.*, b.*, c.Damnhiem, d.User_Create
                    FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                    OUTER APPLY (
                        SELECT TOP 1 * FROM PE_THEODOITIENDO 
                        WHERE Id_Detail_PO = a.PO_Detail_Id 
                        ORDER BY Ngay_NCC_xacnhanGH DESC -- (Hoặc thay bằng cột ngày tạo để lấy dòng mới nhất)
                    ) as b
                    OUTER APPLY (
                        SELECT TOP 1 Damnhiem FROM PE_DamnhiemNCC 
                        WHERE MaNCC = a.MaNCC
                    ) as c
                    LEFT JOIN REQUEST as d on a.Code_Request = d.Code_Request
                    WHERE {mainCondition} {khoi} {tabCondition}
                    AND Ngayphathanh >= '2024-01-01'
                    AND TinhtrangPO <> 'HOANTHANH' AND TinhtrangPO <> 'HUY' AND Luongvekho is null
                    {hientheophongban} ORDER BY Ngayphathanh DESC";

            var lst = sql.GET_DATA_FROM_SQL(query);
            if (lst == null || lst.Rows.Count == 0)
            {
                return Content("Không có dữ liệu phù hợp với điều kiện lọc để xuất Excel.");
            }

            List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                PoDetailViewModel po = new PoDetailViewModel();
                po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString()!);
                object valNgaytao = lst.Rows[i]["Ngaytao"];
                po.Ngayyc = (valNgaytao != null && valNgaytao != DBNull.Value) ? Convert.ToDateTime(valNgaytao).ToString("yyyy-MM-dd") : "";

                object valNgayyc = lst.Rows[i]["Ngaygiaohangdukien"];
                po.Ngayycgiao = (valNgayyc != null && valNgayyc != DBNull.Value) ? Convert.ToDateTime(valNgayyc).ToString("yyyy-MM-dd") : "";

                po.SoPO = lst.Rows[i]["SoPO"].ToString();
                po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                po.Mahang = lst.Rows[i]["Mahang"].ToString();
                po.Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!);
                po.Donvi = lst.Rows[i]["Dovi"].ToString();
                po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                po.DNphathanhpo = lst.Rows[i]["User_Create"].ToString()?.ToLower();
                po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();
                po.MaNhacungcap = lst.Rows[i]["MaNCC"].ToString();
                po.Code_Request = lst.Rows[i]["Code_Request"].ToString();
                po.Good_Code = lst.Rows[i]["Good_Code"].ToString();
                po.picpur = lst.Rows[i]["Damnhiem"].ToString();
                object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";

                object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";

                po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                po.lichgiao = lst.Rows[i]["Lichgiao"].ToString();
                po.trangthai = "";
                po.LuongvekhoKhonhap = lst.Rows[i]["LuongvekhoKhonhap"].ToString();
                po.Danhmuc = lst.Rows[i]["Danhmuc"].ToString();
                object valNgaydieuchinh = lst.Rows[i]["Dieuchinhlichgiao"];
                po.Dieuchinhlichgiao = (valNgaydieuchinh != null && valNgaydieuchinh != DBNull.Value) ? Convert.ToDateTime(valNgaydieuchinh).ToString("yyyy-MM-dd") : "";

                po.Note = lst.Rows[i]["Note"].ToString();

                object valNgayGH = lst.Rows[i]["Ngay_GHchinhthuc"];
                po.Ngay_GHchinhthuc = (valNgayGH != null && valNgayGH != DBNull.Value) ? Convert.ToDateTime(valNgayGH).ToString("yyyy-MM-dd") : "";

                po.Gio_GH = lst.Rows[i]["Gio_GH"].ToString();
                po.Cua_GH = lst.Rows[i]["Cua_GH"].ToString();
                po.Cong_Nhanhang = lst.Rows[i]["Cong_Nhanhang"].ToString();
                po.Nguoi_Nhanhang = lst.Rows[i]["Nguoi_Nhanhang"].ToString();
                po.SL_Thucte = lst.Rows[i]["SL_Thucte"].ToString();
                po.So_DNTT = lst.Rows[i]["So_DNTT"].ToString();
                po.So_hoadon = lst.Rows[i]["So_hoadon"].ToString();
                po.khoi = lst.Rows[i]["Group_Code"].ToString();
                po.canhbao = ""; // Khởi tạo mặc định\
               
                listPo.Add(po);
            }

            // 4. XỬ LÝ LOGIC PULL IN / PULL OUT (Dành cho tab IN)
            if (tab == "trong" && listPo.Any())
            {
                var uniqueItems = listPo.Select(x => x.Mahang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                if (uniqueItems.Any())
                {
                    string inClause = string.Join(",", uniqueItems.Select(x => $"'{x}'"));

                    var dictKho = new Dictionary<string, double>();
                    var dtKho = sql.GET_DATA_FROM_SQL($"SELECT MaNguyenLieu, ISNULL(SUM(Hientai), 0) AS Stock FROM KHO WHERE MaNguyenLieu IN ({inClause}) GROUP BY MaNguyenLieu");
                    if (dtKho != null)
                    {
                        foreach (System.Data.DataRow r in dtKho.Rows)
                            dictKho[r["MaNguyenLieu"].ToString()!] = Convert.ToDouble(r["Stock"]);
                    }

                    var dictUsing = new Dictionary<string, double>();
                    var dtUsing = sql.GET_DATA_FROM_SQL($"SELECT MaVatTu, Thang, Nam, ISNULL(SUM(Soluong), 0) AS Soluong FROM PE_Using WHERE MaVatTu IN ({inClause}) GROUP BY MaVatTu, Thang, Nam");
                    if (dtUsing != null)
                    {
                        foreach (System.Data.DataRow r in dtUsing.Rows)
                            dictUsing[$"{r["MaVatTu"]}_{r["Thang"]}_{r["Nam"]}"] = Convert.ToDouble(r["Soluong"]);
                    }

                    var dictPoSum = new Dictionary<string, double>();
                    var dtPoSum = sql.GET_DATA_FROM_SQL($@"
                            SELECT a.Mahang, 
                                   COALESCE(b.Ngay_GHchinhthuc, b.Ngay_NCC_xacnhanGH, a.Ngaygiaohangdukien) AS Ngay,
                                   ISNULL(SUM(a.Soluong), 0) AS SoLuongPO
                            FROM [COST_MANAGEMENT].[dbo].[PO] AS a 
                            LEFT JOIN PE_THEODOITIENDO AS b ON a.PO_Detail_Id = b.Id_Detail_PO 
                            WHERE a.Danhmuc = 'IN' AND a.Mahang IN ({inClause})
                            GROUP BY a.Mahang, COALESCE(b.Ngay_GHchinhthuc, b.Ngay_NCC_xacnhanGH, a.Ngaygiaohangdukien)");
                    if (dtPoSum != null)
                    {
                        foreach (System.Data.DataRow r in dtPoSum.Rows)
                        {
                            if (r["Ngay"] != DBNull.Value && DateTime.TryParse(r["Ngay"].ToString(), out DateTime dtParsed))
                                dictPoSum[$"{r["Mahang"]}_{dtParsed:yyyy-MM-dd}"] = Convert.ToDouble(r["SoLuongPO"]);
                        }
                    }

                    var filteredByPull = new List<PoDetailViewModel>();
                    foreach (var po in listPo)
                    {
                        string ngayUuTien = !string.IsNullOrEmpty(po.Ngay_GHchinhthuc) ? po.Ngay_GHchinhthuc :
                                            !string.IsNullOrEmpty(po.ngaynccxngiao) ? po.ngaynccxngiao :
                                            po.Ngayycgiao!;

                        if (!string.IsNullOrEmpty(po.Mahang) && DateTime.TryParse(ngayUuTien, out DateTime baseDate))
                        {
                            DateTime month1 = baseDate.AddMonths(1);
                            DateTime month2 = baseDate.AddMonths(2);
                            DateTime month3 = baseDate.AddMonths(3);

                            double stock = dictKho.ContainsKey(po.Mahang) ? dictKho[po.Mahang] : 0;
                            double sl_po = dictPoSum.ContainsKey($"{po.Mahang}_{baseDate:yyyy-MM-dd}") ? dictPoSum[$"{po.Mahang}_{baseDate:yyyy-MM-dd}"] : 0;

                            string uBase = $"{po.Mahang}_{baseDate.Month}_{baseDate.Year}";
                            double using_ngay = dictUsing.ContainsKey(uBase) ? dictUsing[uBase] / 22.0 : 0;

                            double using3thang = (dictUsing.ContainsKey($"{po.Mahang}_{month1.Month}_{month1.Year}") ? dictUsing[$"{po.Mahang}_{month1.Month}_{month1.Year}"] : 0) +
                                                 (dictUsing.ContainsKey($"{po.Mahang}_{month2.Month}_{month2.Year}") ? dictUsing[$"{po.Mahang}_{month2.Month}_{month2.Year}"] : 0) +
                                                 (dictUsing.ContainsKey($"{po.Mahang}_{month3.Month}_{month3.Year}") ? dictUsing[$"{po.Mahang}_{month3.Month}_{month3.Year}"] : 0);

                            double canhbao = stock + sl_po - using_ngay;
                            if (canhbao < 0) po.canhbao = "PullIn";
                            else if (stock > using3thang) po.canhbao = "PullOut";
                        }

                        if (string.IsNullOrEmpty(pullStatus) ||
                           (pullStatus == "in" && po.canhbao == "PullIn") ||
                           (pullStatus == "out" && po.canhbao == "PullOut"))
                        {
                            filteredByPull.Add(po);
                        }
                    }
                    listPo = filteredByPull;
                }
            }

            // 5. LỌC CÁC TRƯỜNG HỢP CÒN LẠI (Search, ReqMonth, ImpactStatus)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                listPo = listPo.Where(x =>
                    (x.SoPO?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Tentiengviet?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Nhacungcap?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Mahang?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }
            if (!string.IsNullOrWhiteSpace(reqMonth))
            {
                listPo = listPo.Where(x => {
                    if (DateTime.TryParse(x.Ngayycgiao, out DateTime dt)) return dt.ToString("yyyy-MM") == reqMonth;
                    return false;
                }).ToList();
            }
            if (!string.IsNullOrEmpty(impactStatus))
            {
                if (impactStatus == "WAIT")
                {
                    listPo = listPo.Where(x => !string.IsNullOrEmpty(x.lichgiao) && x.lichgiao.Trim().ToUpper() == "NG" && string.IsNullOrEmpty(x.anhuongsx)).ToList();
                }
                else
                {
                    listPo = listPo.Where(x => x.lichgiao!.Trim().Equals(impactStatus)).ToList();
                }
            }
            if (!string.IsNullOrEmpty(mahang))
            {
                // Lọc những mã hàng bắt đầu bằng kí tự A hoặc E
                listPo = listPo.Where(x => !string.IsNullOrEmpty(x.Mahang) && x.Mahang.ToUpper().StartsWith(mahang.ToUpper())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(picpur))
            {
                // Chỉ lọc theo tháng khi ô tháng có giá trị
                listPo = listPo.Where(x => x.picpur!.Contains(picpur)).ToList();
            }
            if (!listPo.Any())
            {
                return Content("Không có dữ liệu phù hợp với điều kiện lọc để xuất Excel.");
            }

            // 6. XUẤT FILE EXCEL 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Nhaptiendo.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return Content("Không tìm thấy file mẫu Excel tại hệ thống (File/Nhaptiendo.xlsx).");
            }

            FileInfo templateFile = new FileInfo(templatePath);

            using (var package = new ExcelPackage(templateFile))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Dữ liệu được đưa ra Excel
                var exportData = listPo.Select(x => new
                {
                    x.PO_Detail_Id,
                    Ngayyc = string.IsNullOrEmpty(x.Ngayyc) ? (DateTime?)null : DateTime.Parse(x.Ngayyc),
                    x.SoPO,
                    x.Code_Request,
                    x.Good_Code,
                    x.Tentiengviet,
                    x.Mahang,
                    x.Soluong,
                    x.Donvi,
                    x.Nhacungcap,
                    ngayguiPO = string.IsNullOrEmpty(x.ngayguiPO) ? (DateTime?)null : DateTime.Parse(x.ngayguiPO),
                    x.picpur,
                    Ngayycgiao = string.IsNullOrEmpty(x.Ngayycgiao) ? (DateTime?)null : DateTime.Parse(x.Ngayycgiao),
                    Dieuchinhlichgiao = string.IsNullOrEmpty(x.Dieuchinhlichgiao) ? (DateTime?)null : DateTime.Parse(x.Dieuchinhlichgiao),
                    ngaynccxngiao = string.IsNullOrEmpty(x.ngaynccxngiao) ? (DateTime?)null : DateTime.Parse(x.ngaynccxngiao),    
                    Ngay_GHchinhthuc = string.IsNullOrEmpty(x.Ngay_GHchinhthuc) ? (DateTime?)null : DateTime.Parse(x.Ngay_GHchinhthuc),
                    x.Gio_GH,
                    x.lichgiao,
                    x.anhuongsx,
                    x.Cua_GH,
                    x.Cong_Nhanhang,
                    x.Nguoi_Nhanhang,
                    x.Note,
                    x.DNphathanhpo
                }).ToList();

                worksheet.Cells["A2"].LoadFromCollection(exportData, false);

                worksheet.Column(2).Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Column(11).Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Column(13).Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Column(14).Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Column(15).Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Column(16).Style.Numberformat.Format = "yyyy-MM-dd";
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"TienDo_PO_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        public IActionResult UsingManager()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetData()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                // Sử dụng chuỗi kết nối từ class db
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    string query = @"
                        SELECT 
                            a.Material_Code AS MaVatTu,
                            b.ID,
                            ISNULL(b.Thang, 0) AS Thang,
                            ISNULL(b.Nam, 0) AS Nam,
                            ISNULL(b.Soluong, 0) AS Soluong,
                            b.Nguoiupdate,
                            b.Ngayupdate
                        FROM MATERIAL as a 
                        LEFT JOIN PE_Using as b ON a.Material_Code = b.MaVatTu 
                        WHERE a.CHR_MaterialOutSide = 'IN' 
                          AND (a.Material_Code LIKE 'A%' OR a.Material_Code LIKE 'E%')";
                    // Thực thi và lấy dữ liệu
                    var data = cn.Query<PE_Using>(query).ToList();

                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có vấn đề về kết nối hoặc câu SQL
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult SaveData([FromBody] PE_Using item)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                item.Ngayupdate = DateTime.Now;
                item.Nguoiupdate = string.IsNullOrEmpty(item.Nguoiupdate) ? "System" : item.Nguoiupdate;

                // Sử dụng Dapper để thực hiện nhiều truy vấn an toàn
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    cn.Open();

                    // 1. Kiểm tra xem dữ liệu đã tồn tại chưa và lấy số lượng cũ
                    string checkQuery = "SELECT TOP 1 Soluong FROM PE_Using WHERE MaVatTu = @MaVatTu AND Thang = @Thang AND Nam = @Nam";

                    // (Dùng double? để hứng giá trị null nếu chưa có dữ liệu)
                    var oldItem = cn.QueryFirstOrDefault<double?>(checkQuery, new { item.MaVatTu, item.Thang, item.Nam });

                    double oldQty = oldItem ?? 0;
                    double newQty = item.Soluong;

                    // 2. Cập nhật hoặc Thêm mới dữ liệu vào PE_Using
                    if (oldItem.HasValue)
                    {
                        string updateQuery = @"UPDATE PE_Using 
                                       SET Soluong = @Soluong, Ngayupdate = @Ngayupdate, Nguoiupdate = @Nguoiupdate 
                                       WHERE MaVatTu = @MaVatTu AND Thang = @Thang AND Nam = @Nam";
                        cn.Execute(updateQuery, item);
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO PE_Using (MaVatTu, Thang, Nam, Soluong, Ngayupdate, Nguoiupdate) 
                                       VALUES (@MaVatTu, @Thang, @Nam, @Soluong, @Ngayupdate, @Nguoiupdate)";
                        cn.Execute(insertQuery, item);
                    }

                    // 3. GHI LOG: Chỉ ghi vào PE_LogChangeUsing khi số lượng thực sự có sự thay đổi
                    if (oldQty != newQty)
                    {
                        string logQuery = @"INSERT INTO [COST_MANAGEMENT].[dbo].[PE_LogChangeUsing] 
                                    (MaVatTu, Thang, Nam, Soluong_Truoc, Soluong_Sau, Ngayupdate, Nguoiupdate)
                                    VALUES (@MaVatTu, @Thang, @Nam, @Soluong_Truoc, @Soluong_Sau, @Ngayupdate, @Nguoiupdate)";

                        cn.Execute(logQuery, new
                        {
                            MaVatTu = item.MaVatTu,
                            Thang = item.Thang,
                            Nam = item.Nam,
                            Soluong_Truoc = oldQty,
                            Soluong_Sau = newQty,
                            Ngayupdate = item.Ngayupdate,
                            Nguoiupdate = item.Nguoiupdate
                        });
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DeleteData(int id)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            string query = "DELETE FROM PE_Using WHERE ID = @Id";
            bool result = db.EXECUTE_SQL(query, new { Id = id });
            return Json(new { success = result });
        }
        [HttpPost]
        public IActionResult ImportExcel(IFormFile excelFileInput, string us)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (excelFileInput == null || excelFileInput.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                using (var stream = new MemoryStream())
                {
                    excelFileInput.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Bỏ cấu hình UseHeaderRow = true để tự quản lý dòng thủ công
                        var result = reader.AsDataSet();
                        var dataTable = result.Tables[0];

                        // Nếu file không có đủ ít nhất 3 dòng (Tiêu đề gộp, Cột tháng, Dữ liệu) thì bỏ qua
                        if (dataTable.Rows.Count < 3)
                            return Json(new { success = false, message = "File Excel không đúng định dạng" });

                        // Lấy dòng header chứa tháng năm (Dòng thứ 2 -> index = 1)
                        var headerRow = dataTable.Rows[1];

                        var columnMappings = new Dictionary<int, (int Thang, int Nam)>();
        
                        for (int i = 2; i < dataTable.Columns.Count; i++)
                        {
                            string headerText = headerRow[i]?.ToString()!;
                            if (!string.IsNullOrEmpty(headerText))
                            {                            
                                if (DateTime.TryParse(headerText, out DateTime parsedDate))
                                {
                                    columnMappings.Add(i, (parsedDate.Month, parsedDate.Year));
                                }
                            }
                        }

                        // Kết nối Database qua class SQL_Connect_DB20 (có biến connectString)
                        using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                        {
                            if (cn.State != ConnectionState.Open) cn.Open();

                            // Duyệt từng dòng dữ liệu bắt đầu từ dòng số 3 (index = 2)
                            for (int i = 2; i < dataTable.Rows.Count; i++)
                            {
                                var row = dataTable.Rows[i];
                                string maVatTu = row[1]?.ToString()!;

                                if (string.IsNullOrEmpty(maVatTu)) continue;

                                // Chạy vòng lặp qua từng cột tháng đã parse được
                                foreach (var map in columnMappings)
                                {
                                    int colIndex = map.Key;
                                    int thang = map.Value.Thang;
                                    int nam = map.Value.Nam;

                                    decimal soLuong = 0;
                                    decimal.TryParse(row[colIndex]?.ToString(), out soLuong);

                                    // Dùng câu lệnh UPSERT: Nếu đã có dữ liệu thì Update, chưa có thì Insert
                                    string query = @"
                                                IF EXISTS (SELECT 1 FROM PE_Using WHERE MaVatTu = @MaVatTu AND Thang = @Thang AND Nam = @Nam)
                                                BEGIN
                                                    UPDATE PE_Using 
                                                    SET Soluong = @Soluong, Ngayupdate = GETDATE(), Nguoiupdate = @Nguoiupdate
                                                    WHERE MaVatTu = @MaVatTu AND Thang = @Thang AND Nam = @Nam
                                                END
                                                ELSE
                                                BEGIN
                                                    INSERT INTO PE_Using (MaVatTu, Thang, Nam, Soluong, Ngayupdate, Nguoiupdate)
                                                    VALUES (@MaVatTu, @Thang, @Nam, @Soluong, GETDATE(), @Nguoiupdate)
                                                END";

                                    // Thực thi lệnh SQL bằng Dapper
                                    cn.Execute(query, new
                                    {
                                        MaVatTu = maVatTu,
                                        Thang = thang,
                                        Nam = nam,
                                        Soluong = soLuong,
                                        Nguoiupdate = us
                                    });
                                }
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Import dữ liệu Excel thành công!" });
              
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Import: " + ex.Message });
            }
           
        }

        [HttpGet]
        public IActionResult GetNccData()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    string query = "SELECT Ncc_Id, Ma,Ten, Damnhiem FROM [COST_MANAGEMENT].[dbo].[IM_NCC_NEW] ORDER BY Ncc_Id DESC";

                    var data = cn.Query<NccViewModel>(query).ToList();

                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveNccData([FromBody] NccViewModel item)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                string query = "";

                if (item.Ncc_Id == 0) // Thêm mới
                {
                    query = @"INSERT INTO [COST_MANAGEMENT].[dbo].[IM_NCC_NEW] (Ma, Damnhiem) 
                      VALUES (@Ma, @Damnhiem)";
                }
                else // Cập nhật
                {
                    query = @"UPDATE [COST_MANAGEMENT].[dbo].[IM_NCC_NEW] 
                      SET Ma=@Ma, Damnhiem=@Damnhiem 
                      WHERE Ncc_Id=@Ncc_Id";
                }

                bool result = db.EXECUTE_SQL(query, item);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteNccData(int id)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            string query = "DELETE FROM [COST_MANAGEMENT].[dbo].[IM_NCC_NEW] WHERE Ncc_Id = @Id";
            bool result = db.EXECUTE_SQL(query, new { Id = id });
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult ImportExcelNcc(IFormFile fileExcel)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (fileExcel == null || fileExcel.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                using (var stream = new MemoryStream())
                {
                    fileExcel.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        var dataTable = result.Tables[0];

                        // Nếu file không có đủ ít nhất 2 dòng (Dòng tiêu đề và 1 dòng dữ liệu) thì bỏ qua
                        if (dataTable.Rows.Count < 2)
                            return Json(new { success = false, message = "File Excel không đúng định dạng hoặc không có dữ liệu" });

                        using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                        {
                            if (cn.State != ConnectionState.Open) cn.Open();

                            // Duyệt từng dòng dữ liệu bắt đầu từ dòng số 2 (index = 1, bỏ qua header)
                            for (int i = 1; i < dataTable.Rows.Count; i++)
                            {
                                var row = dataTable.Rows[i];

                                // Giả định Cột 0 là Mã NCC, Cột 1 là Đảm nhiệm
                                string maNcc = row[0]?.ToString()?.Trim();
                                string damNhiem = row[1]?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(maNcc)) continue;

                                // Dùng câu lệnh UPSERT: Kiểm tra theo Mã NCC
                                string query = @" UPDATE [COST_MANAGEMENT].[dbo].[IM_NCC_NEW] SET Damnhiem = @Damnhiem WHERE Ma = @Ma";

                                // Thực thi lệnh SQL bằng Dapper
                                cn.Execute(query, new
                                {
                                    Ma = maNcc,
                                    Damnhiem = damNhiem
                                });
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Import dữ liệu nhà cung cấp thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Import: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetMaterialData()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    string query = @"SELECT Material_Code 
                                    FROM MATERIAL 
                                    WHERE CHR_MaterialOutSide = 'IN' 
                                      AND (Material_Code LIKE 'A%' OR Material_Code LIKE 'E%')";

                    var data = cn.Query<MaterialViewModel>(query).ToList();

                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveMaterialData([FromBody] MaterialViewModel item)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                string query = "";

                if (item.Id_Material == 0) // Thêm mới
                {
                    query = @"INSERT INTO [COST_MANAGEMENT].[dbo].[MATERIAL] (Material_Code, Material_Name_VN, LeadTime) 
                      VALUES (@Material_Code, @Material_Name_VN, @LeadTime)";
                }
                else // Cập nhật
                {
                    query = @"UPDATE [COST_MANAGEMENT].[dbo].[MATERIAL] 
                      SET Material_Code=@Material_Code, Material_Name_VN=@Material_Name_VN, LeadTime=@LeadTime 
                      WHERE Id_Material=@Id_Material";
                }

                bool result = db.EXECUTE_SQL(query, item);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteMaterialData(int id)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            string query = "DELETE FROM [COST_MANAGEMENT].[dbo].[MATERIAL] WHERE Id_Material = @Id";
            bool result = db.EXECUTE_SQL(query, new { Id = id });
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult ImportExcelMaterial(IFormFile fileExcel)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (fileExcel == null || fileExcel.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                using (var stream = new MemoryStream())
                {
                    fileExcel.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        var dataTable = result.Tables[0];

                        if (dataTable.Rows.Count < 2)
                            return Json(new { success = false, message = "File Excel không đúng định dạng hoặc không có dữ liệu" });

                        using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                        {
                            if (cn.State != ConnectionState.Open) cn.Open();

                            // Duyệt dòng từ 2 (index = 1, bỏ qua header)
                            for (int i = 1; i < dataTable.Rows.Count; i++)
                            {
                                var row = dataTable.Rows[i];

                                // Giả định file có cấu trúc: Cột 0 (Mã), Cột 1 (Tên VN), Cột 2 (LeadTime)
                                string code = row[0]?.ToString()?.Trim();

                                int leadTime = 0;
                                int.TryParse(row[2]?.ToString()?.Trim(), out leadTime);

                                if (string.IsNullOrEmpty(code)) continue;

                                string query = @"UPDATE [COST_MANAGEMENT].[dbo].[MATERIAL] SET LeadTime = @LeadTime WHERE Material_Code = @Material_Code";

                                cn.Execute(query, new
                                {
                                    Material_Code = code,
                                    LeadTime = leadTime
                                });
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Import dữ liệu Material thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Import: " + ex.Message });
            }
        }

        public ActionResult QuanLyTienDo()
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            // Khuyến nghị: Trong thực tế nên lọc WHERE ngày tháng và điều kiện cơ bản trực tiếp ở SQL để tối ưu hiệu suất
            string query = @"select * from  PE_THEODOITIENDO as a left join [PO] as b on a.Id_Detail_PO  = b.PO_Detail_Id
                            WHERE Ngayphathanh >= '2026-06-01' ORDER BY Id_Detail_PO DESC";

            var lst = sql.GET_DATA_FROM_SQL(query);
            List<PE_THEODOITIENDO> listPo = new List<PE_THEODOITIENDO>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                PE_THEODOITIENDO po = new PE_THEODOITIENDO();

                po.SoPO = lst.Rows[i]["SoPO"]?.ToString() ?? "";
                po.Id_Detail_PO = lst.Rows[i]["Id_Detail_PO"]?.ToString() ?? "";
                po.Ngay_gui_PO = lst.Rows[i]["Ngay_gui_PO"]?.ToString() ?? "";
                po.Anh_huong_SX = lst.Rows[i]["Anh_huong_SX"]?.ToString() ?? "";
                po.Ngay_NCC_xacnhanGH = lst.Rows[i]["Ngay_NCC_xacnhanGH"]?.ToString();
                po.Ngay_GHchinhthuc = lst.Rows[i]["Ngay_GHchinhthuc"]?.ToString();
                po.Gio_GH = lst.Rows[i]["Gio_GH"]?.ToString() ?? "";
                po.Cua_GH = lst.Rows[i]["Cua_GH"]?.ToString() ?? "";
                po.Cong_Nhanhang = lst.Rows[i]["Cong_Nhanhang"]?.ToString() ?? "";
                po.Nguoi_Nhanhang = lst.Rows[i]["Nguoi_Nhanhang"]?.ToString() ?? "";
                po.MaNCC = lst.Rows[i]["MaNCC"]?.ToString();
                po.TenNCC = lst.Rows[i]["TenNCC"]?.ToString();
                po.Mahang = lst.Rows[i]["Mahang"]?.ToString();
                po.Tentiengviet = lst.Rows[i]["Tentiengviet"]?.ToString();
                // Xử lý an toàn cho SL_Thucte (nếu rỗng/null thì về 0)
                decimal slThucTe = 0;
                if (lst.Rows[i]["SL_Thucte"] != DBNull.Value)
                {
                    decimal.TryParse(lst.Rows[i]["SL_Thucte"].ToString(), out slThucTe);
                }
                po.SL_Thucte = slThucTe;

                po.So_DNTT = lst.Rows[i]["So_DNTT"]?.ToString() ?? "";
                po.So_hoadon = lst.Rows[i]["So_hoadon"]?.ToString() ?? "";
                po.Soluongantoan = lst.Rows[i]["Soluongantoan"]?.ToString();

                listPo.Add(po);
            }
            return View(listPo);

        }

        [HttpPost]
        public IActionResult UpdateAnhHuongSX([FromBody] UpdateAnhHuongSXModel model)
        {
            try
            {
                if (model.PoDetailId <= 0)
                {
                    return Json(new { success = false, message = "ID chi tiết PO không hợp lệ." });
                }

                // Xử lý giá trị chuỗi an toàn để tránh lỗi cú pháp SQL
                string statusVal = string.IsNullOrEmpty(model.ImpactStatus) ? "NULL" : $"N'{model.ImpactStatus}'";

                string sqlQuery = $@" UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] SET [Anh_huong_SX] = {statusVal}
                                      WHERE [Id_Detail_PO] = {model.PoDetailId};";

                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);

                return Json(new { success = true, message = "Xác nhận ảnh hưởng sản xuất thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public IActionResult Giaonhanhang_Master()
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // SQL Query: Trong thực tế, bạn có thể LEFT JOIN với bảng Tồn kho (Stock) và Sử dụng (Using) 
            // để có số liệu UsingThangHienTai và StockHienTai chính xác.
            string query = @"SELECT 
                            a.Material_Code, 
                            a.Material_Name_VN,
                            ISNULL(c.Hientai, 0) AS Hientai,
                            b.*, 
                            d.Soluong 
                        FROM [COST_MANAGEMENT].[dbo].MATERIAL as a 
                        LEFT JOIN Giaohang_Master as b 
                            ON a.Material_Code = b.Mahang 
                      
                        LEFT JOIN (
                            SELECT 
                                MaNguyenLieu, 
                                SUM(Hientai) AS Hientai 
                            FROM KHO
                            GROUP BY MaNguyenLieu
                        ) as c 
                            ON a.Material_Code = c.MaNguyenLieu 

                        LEFT JOIN PE_Using as d 
                            ON a.Material_Code = d.MaVatTu  
                            AND d.Thang = '7' 
                            AND d.Nam = '2026'

                        WHERE a.CHR_MaterialOutSide = 'IN' 
                          AND (a.Material_Code LIKE N'A%' OR a.Material_Code LIKE N'E%') 
                        ORDER BY a.[Material_Code] ASC;";

            var lst = sql.GET_DATA_FROM_SQL(query);
            List<GiaoHangMasterViewModel> model = new List<GiaoHangMasterViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                var row = lst.Rows[i];
                var item = new GiaoHangMasterViewModel();

                item.Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0;
                item.Mahang = row["Material_Code"]?.ToString();
                item.Tenhang = row["Material_Name_VN"]?.ToString();
                item.Vendor_Code = row["Vendor_Code"]?.ToString();
                item.Vendor = row["Vendor"]?.ToString();
                item.Maker = row["Maker"]?.ToString();
                item.MOQ = row["MOQ"]?.ToString();

                item.Tansuatgiaohang = row["Tansuatgiaohang"].ToString();
                item.Leadtimegiaohang = row["Leadtimegiaohang"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Leadtimegiaohang"].ToString()) ? Convert.ToDouble(row["Leadtimegiaohang"]) : null;
                item.Songaytonkhoantoan = row["songaytonkhoantoan"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["songaytonkhoantoan"].ToString()) ? Convert.ToDouble(row["songaytonkhoantoan"]) : null;
                item.Donvi = row["donvi"]?.ToString();
                item.Soluongtonkhoantoan = row["soluongtonkhoantoan"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["soluongtonkhoantoan"].ToString()) ? Convert.ToDouble(row["soluongtonkhoantoan"]) : null;

                // Tính toán ví dụ TiLeTonKhoAnToan (Chuẩn 24 ngày = 100%)
                if (item.Songaytonkhoantoan.HasValue && item.Songaytonkhoantoan > 0)
                {
                    item.TiLeTonKhoAnToan = (item.Songaytonkhoantoan.Value / 24.0) * 100;
                }

                // 1. Ép kiểu an toàn cho Hientai (StockHienTai)
                if (double.TryParse(row["Hientai"]?.ToString(), out double stockHienTai))
                {
                    item.StockHienTai = stockHienTai;
                }
                else
                {
                    item.StockHienTai = 0;
                }

                // 2. Ép kiểu an toàn cho Soluong (UsingThangHienTai)
                if (float.TryParse(row["Soluong"]?.ToString(), out float usingThangHienTai))
                {
                    item.UsingThangHienTai = usingThangHienTai;
                }
                else
                {
                    item.UsingThangHienTai = 0;
                }

                // Tính toán điểm gọi hàng
                double tile = item.StockHienTai ?? 100;

                if (item.StockHienTai == 0 && item.UsingThangHienTai == 0)
                {
                    item.DiemGoiHang = "Chưa có kế hoạch sử dụng";
                }
                else if (tile < 70)
                {
                    item.DiemGoiHang = "NG";
                }
                else if (tile >= 70 && tile <= 100)
                {
                    item.DiemGoiHang = "Cần gọi hàng";
                }
                else
                {
                    item.DiemGoiHang = "OK";
                }

                model.Add(item);
            }

            return View(model);
        }

        public JsonResult NCC_NG()
        {
            SQL_Connect_DB20 sQL = new SQL_Connect_DB20();

            var demsoluong = sQL.GET_DATA_FROM_SQL(@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
            LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO left join REQUEST as c on a.Code_Request = c.Code_Request
            WHERE Ngayphathanh >= '2024-01-01' AND (a.Group_Code = 'PUR' OR a.Group_Code = 'PROD') and b.Lichgiao = 'NG' and (b.Anh_huong_SX <> 'No' or b.Anh_huong_SX is null) 
            ORDER BY Ngayphathanh DESC");

            List<NCC_NG_Detail> ncc_ng_dt = new List<NCC_NG_Detail>();
            List<NCC_NG> ncc_ng = new List<NCC_NG>();

            for (int i = 0; i < demsoluong.Rows.Count; i++)
            {
                var sopo = demsoluong.Rows[i]["SoPO"].ToString();
                var Mahang = demsoluong.Rows[i]["Mahang"].ToString();
                var tenhang = demsoluong.Rows[i]["Tentiengviet"].ToString();
                var soluong = demsoluong.Rows[i]["Soluong"].ToString();
                var donvi = demsoluong.Rows[i]["Dovi"].ToString();
                var ngayycgiao = demsoluong.Rows[i]["Ngaygiaohangdukien"].ToString();
                var ngaynccgiao = demsoluong.Rows[i]["Ngay_NCC_xacnhanGH"].ToString();
                var danhmuc = demsoluong.Rows[i]["danhmuc"].ToString();
                var mancc = demsoluong.Rows[i]["MaNCC"].ToString();
                var tenncc = demsoluong.Rows[i]["TenNCC"].ToString();
                var damnhiemxacnhananhhuong = "";

                if (danhmuc == "OUT")
                {
                    damnhiemxacnhananhhuong = demsoluong.Rows[i]["User_Create"].ToString();
                }
                if (danhmuc == "IN")
                {
                    damnhiemxacnhananhhuong = "PR1-MC";
                }

                ncc_ng_dt.Add(new NCC_NG_Detail
                {
                    sopo = sopo!,
                    mahang = Mahang!,
                    tenhang = tenhang!,
                    soluong = soluong!,
                    donvi = donvi!,
                    ngayycgiao = ngayycgiao!,
                    ngaythuctegiao = ngaynccgiao!,
                    damnhiemxacnhananhhuong = damnhiemxacnhananhhuong!,
                    mancc = mancc!,
                    tenncc = tenncc!
                });
            }

            // Gộp theo MaNCC và TenNCC
            ncc_ng = ncc_ng_dt
                .GroupBy(x => new { x.mancc, x.tenncc })
                .Select(g => new NCC_NG
                {
                    mancc = g.Key.mancc,
                    tenncc = g.Key.tenncc
                }).ToList();


            return Json(new { ncc_ng = ncc_ng, ncc_ng_dt = ncc_ng_dt });
        }

        public JsonResult NCC_Giaohang(string ngaythang)
        {
            if (ngaythang == "" || ngaythang == null)
            {
                ngaythang = DateTime.Now.ToString("yyyy-MM-05");
            }
            SQL_Connect_DB20 sQL = new SQL_Connect_DB20();

            var demsoluong = sQL.GET_DATA_FROM_SQL($@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
            LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO left join REQUEST as c on a.Code_Request = c.Code_Request
            WHERE Ngay_GHchinhthuc >= '{ngaythang}' AND (a.Group_Code = 'PUR' OR a.Group_Code = 'PROD') 
            ORDER BY Ngayphathanh DESC");

            List<NCC_NG_Detail> ncc_ng_dt = new List<NCC_NG_Detail>();
            List<NCC_NG> ncc_ng = new List<NCC_NG>();

            for (int i = 0; i < demsoluong.Rows.Count; i++)
            {
                var sopo = demsoluong.Rows[i]["SoPO"].ToString();
                var Mahang = demsoluong.Rows[i]["Mahang"].ToString();
                var tenhang = demsoluong.Rows[i]["Tentiengviet"].ToString();
                var soluong = demsoluong.Rows[i]["Soluong"].ToString();
                var donvi = demsoluong.Rows[i]["Dovi"].ToString();
                var ngayycgiao = demsoluong.Rows[i]["Ngaygiaohangdukien"].ToString();
                var ngaynccgiao = demsoluong.Rows[i]["Ngay_NCC_xacnhanGH"].ToString();
                var danhmuc = demsoluong.Rows[i]["danhmuc"].ToString();
                var mancc = demsoluong.Rows[i]["MaNCC"].ToString();
                var tenncc = demsoluong.Rows[i]["TenNCC"].ToString();
                var ngaygiaochinhthuc = demsoluong.Rows[i]["Ngay_GHchinhthuc"].ToString();
                var giogiao = demsoluong.Rows[i]["Gio_GH"].ToString();
                var cuagiao = demsoluong.Rows[i]["Cua_GH"].ToString();
                var congnhanhang = demsoluong.Rows[i]["Cong_Nhanhang"].ToString();
                var nguoinhanhang = demsoluong.Rows[i]["Nguoi_Nhanhang"].ToString();
                var damnhiemxacnhananhhuong = "";

                if (danhmuc == "OUT")
                {
                    damnhiemxacnhananhhuong = demsoluong.Rows[i]["User_Create"].ToString();
                }
                if (danhmuc == "IN")
                {
                    damnhiemxacnhananhhuong = "PR1-MC";
                }

                ncc_ng_dt.Add(new NCC_NG_Detail
                {
                    sopo = sopo!,
                    mahang = Mahang!,
                    tenhang = tenhang!,
                    soluong = soluong!,
                    donvi = donvi!,
                    ngayycgiao = ngayycgiao!,
                    ngaythuctegiao = ngaynccgiao!,
                    damnhiemxacnhananhhuong = damnhiemxacnhananhhuong!,
                    mancc = mancc!,
                    tenncc = tenncc!,
                    ngaygiaochinhthuc = ngaygiaochinhthuc!,
                    giogiao = giogiao!,
                    cuagiao = cuagiao!,
                    congnhanhang = congnhanhang!,
                    nguoinhanhang = nguoinhanhang!
                });
            }

            // Gộp theo MaNCC và TenNCC
            ncc_ng = ncc_ng_dt
                .GroupBy(x => new { x.mancc, x.tenncc, x.giogiao, x.sopo })
                .Select(g => new NCC_NG
                {
                    mancc = g.Key.mancc,
                    tenncc = g.Key.tenncc,
                    giogiao = g.Key.giogiao,
                    soPO = g.Key.sopo
                }).ToList();

            return Json(new { ncc_ng = ncc_ng, ncc_ng_dt = ncc_ng_dt });
        }
   
        [HttpGet]
        public IActionResult LogChangeHistory()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    // Lấy 1000 dòng gần nhất, tính toán luôn độ chênh lệch
                    string query = @"
                SELECT TOP (1000) 
                    [ID], 
                    [MaVatTu], 
                    [Thang], 
                    [Nam], 
                    [Soluong_Truoc], 
                    [Soluong_Sau], 
                    [Ngayupdate], 
                    [Nguoiupdate],
                    ([Soluong_Sau] - [Soluong_Truoc]) AS ChenhLech
                FROM [COST_MANAGEMENT].[dbo].[PE_LogChangeUsing]
                ORDER BY Ngayupdate DESC";

                    var data = cn.Query<LogChangeUsingViewModel>(query).ToList();

                    return View(data);
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về danh sách rỗng để không bị chết trang
                return View(new List<LogChangeUsingViewModel>());
            }
        }

        [HttpGet]
        public IActionResult ExportLogChangeExcel()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                {
                    // Lấy dữ liệu (Giống hệt hàm hiển thị)
                    string query = @"
                SELECT TOP (1000) 
                    [ID], 
                    [MaVatTu], 
                    [Thang], 
                    [Nam], 
                    [Soluong_Truoc], 
                    [Soluong_Sau], 
                    [Ngayupdate], 
                    [Nguoiupdate],
                    ([Soluong_Sau] - [Soluong_Truoc]) AS ChenhLech
                FROM [COST_MANAGEMENT].[dbo].[PE_LogChangeUsing]
                ORDER BY Ngayupdate DESC";

                    var data = cn.Query<LogChangeUsingViewModel>(query).ToList();

                    // Khởi tạo EPPlus
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("LogChangeHistory");

                        // 1. Tạo dòng tiêu đề
                        worksheet.Cells[1, 1].Value = "STT";
                        worksheet.Cells[1, 2].Value = "Mã Vật Tư";
                        worksheet.Cells[1, 3].Value = "Tháng";
                        worksheet.Cells[1, 4].Value = "Năm";
                        worksheet.Cells[1, 5].Value = "Số Lượng Trước";
                        worksheet.Cells[1, 6].Value = "Số Lượng Sau";
                        worksheet.Cells[1, 7].Value = "Chênh Lệch";
                        worksheet.Cells[1, 8].Value = "Người Cập Nhật";
                        worksheet.Cells[1, 9].Value = "Thời Gian";

                        // Format dòng tiêu đề (Bôi đậm, nền màu nhạt)
                        using (var range = worksheet.Cells[1, 1, 1, 9])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        // 2. Đổ dữ liệu vào các dòng
                        int row = 2;
                        int stt = 1;
                        foreach (var item in data)
                        {
                            worksheet.Cells[row, 1].Value = stt++;
                            worksheet.Cells[row, 2].Value = item.MaVatTu;
                            worksheet.Cells[row, 3].Value = item.Thang;
                            worksheet.Cells[row, 4].Value = item.Nam;
                            worksheet.Cells[row, 5].Value = item.Soluong_Truoc;
                            worksheet.Cells[row, 6].Value = item.Soluong_Sau;
                            worksheet.Cells[row, 7].Value = item.ChenhLech;
                            worksheet.Cells[row, 8].Value = item.Nguoiupdate;

                            // Format ngày tháng tránh lỗi hiển thị trên Excel
                            worksheet.Cells[row, 9].Value = item.Ngayupdate.HasValue ? item.Ngayupdate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";

                            row++;
                        }

                        // Tự động căn chỉnh độ rộng các cột cho đẹp
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // 3. Chuyển đổi thành dạng Stream để trả về trình duyệt tải xuống
                        var stream = new MemoryStream();
                        package.SaveAs(stream);
                        stream.Position = 0;

                        string fileName = $"LichSu_Using_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("Có lỗi xảy ra khi xuất Excel: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult ExportGiaonhanhangMaster()
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            try
            {
                // 1. LẤY DỮ LIỆU (Câu query giống hệt hàm Giaonhanhang_Master)
                string query = @"SELECT 
                            a.Material_Code, 
                            a.Material_Name_VN,
                            ISNULL(c.Hientai, 0) AS Hientai,
                            b.*, 
                            d.Soluong 
                        FROM [COST_MANAGEMENT].[dbo].MATERIAL as a 
                        LEFT JOIN Giaohang_Master as b 
                            ON a.Material_Code = b.Mahang 
                        LEFT JOIN (
                            SELECT 
                                MaNguyenLieu, 
                                SUM(Hientai) AS Hientai 
                            FROM KHO
                            GROUP BY MaNguyenLieu
                        ) as c 
                            ON a.Material_Code = c.MaNguyenLieu 
                        LEFT JOIN PE_Using as d 
                            ON a.Material_Code = d.MaVatTu  
                            AND d.Thang = '7' 
                            AND d.Nam = '2026'
                        WHERE a.CHR_MaterialOutSide = 'IN' 
                          AND (a.Material_Code LIKE N'A%' OR a.Material_Code LIKE N'E%') 
                        ORDER BY a.[Material_Code] ASC;";

                var lst = sql.GET_DATA_FROM_SQL(query);

                // Khởi tạo EPPlus
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("MasterGiaoHang");

                    // --- TẠO DÒNG HEADER 1 (GỘP Ô) ---
                    worksheet.Cells["A1:F1"].Merge = true;
                    worksheet.Cells["A1:F1"].Value = "I. THÔNG TIN VẬT TƯ & NCC";
                    worksheet.Cells["A1:F1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#e2efda"));

                    worksheet.Cells["G1:I1"].Merge = true;
                    worksheet.Cells["G1:I1"].Value = "II. QUY ĐỊNH GIAO HÀNG";
                    worksheet.Cells["G1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["G1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#fff2cc"));

                    worksheet.Cells["J1:L1"].Merge = true;
                    worksheet.Cells["J1:L1"].Value = "III. TIÊU CHUẨN AN TOÀN";
                    worksheet.Cells["J1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["J1:L1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));

                    worksheet.Cells["M1:Q1"].Merge = true;
                    worksheet.Cells["M1:Q1"].Value = "IV. THỰC TẾ & TRẠNG THÁI";
                    worksheet.Cells["M1:Q1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["M1:Q1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#fce4d6"));

                    // --- TẠO DÒNG HEADER 2 (CHI TIẾT CỘT) ---
                    string[] headers = {
                            "Mã hàng", "Tên hàng", "Đơn vị", "Mã NCC", "Tên NCC", "Maker", // Nhóm I
                            "MOQ", "Tần suất GH", "Leadtime GH",                           // Nhóm II
                            "Số ngày kho AT", "Tỉ lệ kho AT", "SL tồn kho AT",             // Nhóm III
                            "Stock hiện tại", "Using tháng HT", "Số ngày dùng HT", "Tỉ lệ tồn kho HT", "Điểm gọi hàng" // Nhóm IV
                        };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[2, i + 1].Value = headers[i];
                    }

                    // Định dạng chung cho 2 dòng Header (Đậm, căn giữa, viền)
                    using (var range = worksheet.Cells[1, 1, 2, 17])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    // --- ĐỔ DỮ LIỆU VÀ TÍNH TOÁN ---
                    int row = 3;
                    for (int i = 0; i < lst.Rows.Count; i++)
                    {
                        var r = lst.Rows[i];

                        // Đọc và tính toán lại các logic y hệt như View
                        double snTonKhoAT = r["songaytonkhoantoan"] != DBNull.Value && !string.IsNullOrWhiteSpace(r["songaytonkhoantoan"].ToString()) ? Convert.ToDouble(r["songaytonkhoantoan"]) : 0;
                        double tiLeKhoAT = (snTonKhoAT / 24.0) * 100;

                        double stockHienTai = double.TryParse(r["Hientai"]?.ToString(), out double st) ? st : 0;
                        double usingHienTai = double.TryParse(r["Soluong"]?.ToString(), out double us) ? us : 0;

                        string diemGoiHang = "OK";
                        if (stockHienTai == 0 && usingHienTai == 0) diemGoiHang = "CHƯA DÙNG";
                        else if (stockHienTai < 70) diemGoiHang = "ĐỐI ỨNG GẤP";
                        else if (stockHienTai >= 70 && stockHienTai <= 100) diemGoiHang = "GỌI HÀNG";

                        // Gán dữ liệu vào Excel
                        worksheet.Cells[row, 1].Value = r["Material_Code"]?.ToString();
                        worksheet.Cells[row, 2].Value = r["Material_Name_VN"]?.ToString();
                        worksheet.Cells[row, 3].Value = r["donvi"]?.ToString();
                        worksheet.Cells[row, 4].Value = r["Vendor_Code"]?.ToString();
                        worksheet.Cells[row, 5].Value = r["Vendor"]?.ToString();
                        worksheet.Cells[row, 6].Value = r["Maker"]?.ToString();

                        worksheet.Cells[row, 7].Value = r["MOQ"]?.ToString();
                        worksheet.Cells[row, 8].Value = r["Tansuatgiaohang"]?.ToString();
                        worksheet.Cells[row, 9].Value = r["Leadtimegiaohang"] != DBNull.Value ? Convert.ToDouble(r["Leadtimegiaohang"]) : (object)"-";

                        worksheet.Cells[row, 10].Value = snTonKhoAT > 0 ? snTonKhoAT : (object)"-";
                        worksheet.Cells[row, 11].Value = snTonKhoAT > 0 ? $"{tiLeKhoAT:N0}%" : "-";
                        worksheet.Cells[row, 12].Value = r["soluongtonkhoantoan"] != DBNull.Value ? Convert.ToDouble(r["soluongtonkhoantoan"]) : (object)"-";

                        worksheet.Cells[row, 13].Value = stockHienTai;
                        worksheet.Cells[row, 14].Value = usingHienTai;
                        worksheet.Cells[row, 15].Value = "-"; // Số ngày sử dụng HT (Trong mã C# cũ không thấy bạn tính toán biến này)
                        worksheet.Cells[row, 16].Value = "-"; // Tỉ lệ tồn kho HT
                        worksheet.Cells[row, 17].Value = diemGoiHang;

                        row++;
                    }

                    // Tự động căn chỉnh độ rộng cột
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Trả về file tải xuống
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"Master_GiaoHang_TonKho_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Content("Lỗi xuất Excel: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportExcelGiaoHang(IFormFile fileExcel, string us)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (fileExcel == null || fileExcel.Length == 0)
            return Json(new { success = false, message = "Vui lòng chọn file" });
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                using (var stream = new MemoryStream())
                {
                    fileExcel.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Cấu hình dòng đầu tiên là Header
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });
                        var dataTable = result.Tables[0];

                        using (SqlConnection cn = new SqlConnection(db.connectString_Test))
                        {
                            if (cn.State != ConnectionState.Open) cn.Open();

                            // Duyệt từng dòng dữ liệu trong DataTable
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                var row = dataTable.Rows[i];

                                string maHang = row[1]?.ToString()?.Trim()!;
                                if (string.IsNullOrEmpty(maHang)) continue;

                                string tenHang = row[2]?.ToString()?.Trim()!;
                                string vendorCode = row[3]?.ToString()?.Trim()!;
                                string vendor = row[4]?.ToString()?.Trim()!;
                                string maker = row[5]?.ToString()?.Trim()!;
                                string picBIVN = row[8]?.ToString()?.Trim()!;
                                string moq = row[9]?.ToString()?.Trim()!;
                                string tanSuat = row[10]?.ToString()?.Trim()!;
                                string leadTime = row[11]?.ToString()?.Trim()! == "-" ? "0" : row[11]?.ToString()?.Trim()!;
                                string tiLeAnToan = row[12]?.ToString()?.Trim()! == "-" ? "0" : row[12]?.ToString()?.Trim()!;
                                string soNgayAnToan = row[13]?.ToString()?.Trim()! == "-" ? "0" : row[13]?.ToString()?.Trim()!;
                                string tiLeToiDa = row[14]?.ToString()?.Trim()! == "-" ? "0" : row[14]?.ToString()?.Trim()! ; 
                                string donVi = row[15]?.ToString()?.Trim()! ;

                                // Câu lệnh UPSERT cho Giaohang_Master
                                string query = $@"
                                IF EXISTS (SELECT 1 FROM Giaohang_Master WHERE Mahang = '{maHang}')
                                BEGIN
                                    UPDATE Giaohang_Master 
                                    SET Tenhang = N'{tenHang}', Vendor_Code = N'{vendorCode}', Vendor = N'{vendor}', 
                                        Maker = N'{maker}', MOQ = N'{moq}', Tansuatgiaohang = N'{tanSuat}', 
                                        Leadtimegiaohang = '{leadTime}', Tiletonkhoantoan = '{tiLeAnToan}', 
                                        songaytonkhoantoan = '{soNgayAnToan}', donvi = N'{donVi}', 
                                        PicBIVN = N'{picBIVN}', [using] = 1, soluongtonkhoantoan = '{tiLeToiDa}'
                                    WHERE Mahang = '{maHang}'
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO Giaohang_Master (Mahang, Tenhang, Vendor_Code, Vendor, Maker, 
                                        MOQ, Tansuatgiaohang, Leadtimegiaohang, Tiletonkhoantoan, songaytonkhoantoan, 
                                        donvi, PicBIVN, [using], soluongtonkhoantoan)
                                    VALUES ('{maHang}', N'{tenHang}',N'{vendorCode}', N'{vendor}', N'{maker}', 
                                       N'{moq}', N'{tanSuat}', '{leadTime}', '{tiLeAnToan}', '{soNgayAnToan}', 
                                        N'{donVi}', N'{picBIVN}', 1, '{tiLeToiDa}')
                                END";

                                // Thực thi bằng Dapper
                                cn.Execute(query);
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Import Master Giao Hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Import: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMailListNG()
        {
            try
            {
                SQL_Connect_DB20 sQL_Connect = new SQL_Connect_DB20();
                var lst = sQL_Connect.GET_DATA_FROM_SQL($@"
                        SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                        LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
                        WHERE a.Ngayphathanh >= '2024-01-01' 
                            AND (a.Group_Code = 'PUR' OR a.Group_Code = 'PROD') 
                            AND (a.Danhmuc IS NULL OR a.Danhmuc = 'OUT') 
                            AND b.Lichgiao = 'NG' AND b.Anh_huong_SX IS NULL
                        ORDER BY a.Ngayphathanh DESC;");

                // 1. Kiểm tra xem có dữ liệu không trước khi chạy tiếp
                if (lst == null || lst.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Không có PO nào có trạng thái NG để gửi cảnh báo." });
                }

                List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

                for (int i = 0; i < lst.Rows.Count; i++)
                {
                    PoDetailViewModel po = new PoDetailViewModel();
                    po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString());
                    po.Ngayyc = lst.Rows[i]["Ngaytao"].ToString().Split(' ')[0];
                    po.Ngayycgiao = lst.Rows[i]["Ngaygiaohangdukien"].ToString().Split(' ')[0];
                    po.SoPO = lst.Rows[i]["SoPO"].ToString();
                    po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                    po.Mahang = lst.Rows[i]["Mahang"].ToString();
                    po.Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString());
                    po.Donvi = lst.Rows[i]["Dovi"].ToString();
                    po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                    po.DNphathanhpo = lst.Rows[i]["Nguoilamdon"].ToString()?.ToLower();
                    po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();
                    po.MaNhacungcap = lst.Rows[i]["MaNCC"].ToString();
                    object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                    po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";
                    object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                    po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";
                    po.lichgiao = lst.Rows[i]["Lichgiao"].ToString();
                    po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                    po.trangthai = "";
                    po.LuongvekhoKhonhap = lst.Rows[i]["LuongvekhoKhonhap"].ToString();
                    po.Danhmuc = lst.Rows[i]["Danhmuc"].ToString();
                    object valNgayGH = lst.Rows[i]["Ngay_GHchinhthuc"];
                    po.Ngay_GHchinhthuc = (valNgayGH != null && valNgayGH != DBNull.Value) ? Convert.ToDateTime(valNgayGH).ToString("yyyy-MM-dd") : "";
                    po.Gio_GH = lst.Rows[i]["Gio_GH"].ToString();
                    po.Cua_GH = lst.Rows[i]["Cua_GH"].ToString();
                    po.Cong_Nhanhang = lst.Rows[i]["Cong_Nhanhang"].ToString();
                    po.Nguoi_Nhanhang = lst.Rows[i]["Nguoi_Nhanhang"].ToString();
                    po.SL_Thucte = lst.Rows[i]["SL_Thucte"].ToString();
                    po.So_DNTT = lst.Rows[i]["So_DNTT"].ToString();
                    po.So_hoadon = lst.Rows[i]["So_hoadon"].ToString();
                    listPo.Add(po);
                }

                // --- BẮT ĐẦU XỬ LÝ GỬI THEO TỪNG MANCC ---
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Nhaptiendo.xlsx");
                FileInfo templateFile = new FileInfo(templatePath);

                // 2. Bắt lỗi không tìm thấy File Template
                if (!templateFile.Exists)
                {
                    return Json(new { success = false, message = "Lỗi: Không tìm thấy file template Excel trên Server." });
                }

                var listMaNcc = listPo.Select(x => x.MaNhacungcap).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                int successCount = 0;

                foreach (var maNcc in listMaNcc)
                {
                    var listPoByNcc = listPo.Where(x => x.MaNhacungcap == maNcc).ToList();
                    if (!listPoByNcc.Any()) continue;

                    using (var package = new ExcelPackage(templateFile))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) continue;

                        var exportData = listPoByNcc.Select(x => new
                        {
                            x.PO_Detail_Id,
                            x.Ngayyc,
                            x.Ngayycgiao,
                            x.SoPO,
                            x.Tentiengviet,
                            x.Mahang,
                            x.Soluong,
                            x.Donvi,
                            x.Nhacungcap,
                            x.DNphathanhpo,
                            x.ngayguiPO,
                            x.Danhmuc,
                            x.ngaynccxngiao,
                            x.Ngay_GHchinhthuc,
                            x.lichgiao,
                            x.anhuongsx,
                            x.Cua_GH,
                            x.Cong_Nhanhang,
                            x.Nguoi_Nhanhang,
                            x.So_DNTT,
                            x.So_hoadon
                        }).ToList();

                        worksheet.Cells["A2"].LoadFromCollection(exportData, false);

                        using (var stream = new MemoryStream())
                        {
                            package.SaveAs(stream);
                            stream.Position = 0;
                            string fileName = $"CanhBao_NG_{maNcc}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress("zzpsvvmi@brother-bivn.com.vn", "[IPCS] Hệ thống quản lý linh kiện gián tiếp");
                                mail.To.Add("maithi.tuyen@brother-bivn.com.vn");
                                mail.To.Add("nguyenthi.ngan2@brother-bivn.com.vn");
                                mail.Subject = $"(CẢNH BÁO) NCC {maNcc} có nguy cơ giao hàng chậm";
                                mail.Body = $@"
                            <div style='font-family: Arial, Helvetica, sans-serif; font-size: 14px; '>
                                <b>Kính gửi anh/chị,</b> <br/><br/>
                                Hệ thống phát hiện NCC <b>{maNcc}</b> có nguy cơ giao hàng chậm so với thời gian yêu cầu giao hàng của phòng ban.<br/>
                                Chi tiết vui lòng xem trong tệp đính kèm. <br/><br/>
                                <i>Vui lòng kiểm tra và xác nhận mức độ ảnh hưởng tới sản xuất trên hệ thống.<br/>
                                Đây là email tự động.</i>
                            </div>";

                                Attachment attachment = new Attachment(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                                mail.Attachments.Add(attachment);

                                using (SmtpClient smtp = new SmtpClient("smtp-auth.brothergroup.net", 25))
                                {
                                    // Điền đúng Credentials ở đây nhé
                                    smtp.Credentials = new NetworkCredential("ZZPSVVMI", "123456a@");
                                    smtp.EnableSsl = false;
                                    mail.IsBodyHtml = true;

                                    // 3. Sử dụng SendMailAsync
                                    await smtp.SendMailAsync(mail);
                                    successCount++;
                                }
                            }
                        }
                    }
                }

                return Json(new { success = true, message = $"Đã gửi email nhắc nhở kèm file Excel thành công cho {successCount} nhà cung cấp." });
            }
            catch (Exception ex)
            {
                // 4. Bắt toàn bộ lỗi (SQL, SMTP, File) và ném về cho Client để hiển thị hộp thoại Alert
                return Json(new { success = false, message = "Lỗi khi gửi email: " + ex.Message });
            }
        }

        public IActionResult ExportWarningPullInOut()
        {
            DataSet ds = new DataSet();
            SQL_Connect_DB20 db = new SQL_Connect_DB20();

            var lst_using = db.GET_DATA_FROM_SQL(@"DECLARE @NienDo INT = 2026;
              SELECT 
                  MaVatTu AS [Using], 
                  [Apr], [May], [Jun], [Jul], [Aug], [Sep], [Oct], [Nov], [Dec], [Jan], [Feb], [Mar]
              FROM 
              (
                  SELECT 
                      MaVatTu, 
                      Soluong, 
                      FORMAT(DATEFROMPARTS(Nam, Thang, 1), 'MMM', 'en-US') AS MonthStr
                  FROM [COST_MANAGEMENT].[dbo].[PE_Using]
                  WHERE (Nam = @NienDo AND Thang >= 4) 
                      OR (Nam = @NienDo + 1 AND Thang <= 3) 
              ) AS SourceTable
              PIVOT 
              (
                  SUM(Soluong)
                  FOR MonthStr IN ([Apr], [May], [Jun], [Jul], [Aug], [Sep], [Oct], [Nov], [Dec], [Jan], [Feb], [Mar])
              ) AS PivotTable
              ORDER BY [Using];");

            var lst_stock = db.GET_DATA_FROM_SQL("select MaNguyenLieu,'', sum(hientai) from KHO group by MaNguyenLieu");

            // ================== TẠO DICTIONARY CHỨA STOCK ==================
            Dictionary<string, float> dictStock = new Dictionary<string, float>();
            if (lst_stock != null)
            {
                foreach (DataRow row in lst_stock.Rows)
                {
                    string maVatTu = row["MaNguyenLieu"].ToString();
                    if (float.TryParse(row[2].ToString(), out float qty))
                    {
                        dictStock[maVatTu] = qty;
                    }
                }
            }

            // TÍNH TOÁN KHOẢNG THỜI GIAN 3 THÁNG ĐỂ TẠO CỘT NGÀY CHO SHEET WARNING
            DateTime firstDayCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime startDate = firstDayCurrentMonth.AddMonths(-1);
            DateTime endDate = firstDayCurrentMonth.AddMonths(2).AddDays(-1);

            // ================== TẠO BẢNG WARNING DẠNG MA TRẬN ==================
            DataTable dtWarning = new DataTable("WarningOutStock");
            dtWarning.Columns.Add("Mã vật tư", typeof(string));
            dtWarning.Columns.Add("Ngày cần cảnh báo", typeof(string)); // Trước 3 ngày

            // Thêm các cột ngày tương ứng từ startDate đến endDate
            for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
            {
                dtWarning.Columns.Add(d.ToString("dd/MM/yyyy"), typeof(string));
            }
            // ====================================================================

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Warning_PullInOut.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return Content("Không tìm thấy file mẫu Excel tại hệ thống (Data/Nhaptiendo.xlsx).");
            }

            FileInfo templateFile = new FileInfo(templatePath);

            using (var package = new ExcelPackage(templateFile))
            {
                // ------------------ SHEET 0: SUMMARY ------------------
                var worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells["A2"].LoadFromDataTable(lst_using, false);
                using (var range = worksheet.Cells[2, 2, worksheet.Dimension.Rows, 13])
                {
                    range.Style.Numberformat.Format = "#,##0.00";
                }

                // ------------------ SHEET 2: STOCK --------------------
                var worksheet_stock = package.Workbook.Worksheets[2];
                worksheet_stock.Cells["A2"].LoadFromDataTable(lst_stock, false);

                // ------------------ SHEET 1 & 3: USING DAY & PO -------
                var worksheet_usingday = package.Workbook.Worksheets[1];
                var worksheet_po = package.Workbook.Worksheets[3];

                // Lấy toàn bộ dữ liệu PO của CẢ 3 THÁNG 1 lần duy nhất
                string sqlPO = $@"SELECT Mahang, CONVERT(VARCHAR(10), b.Ngay_NCC_xacnhanGH, 120) as Ngay, SUM(a.Soluong) as Soluong 
                  FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                  LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
                  WHERE (Group_Code = 'PUR' OR Group_Code = 'PROD') 
                      AND a.Danhmuc = 'IN' 
                      AND TinhtrangPO <> 'HOANTHANH' 
                      AND TinhtrangPO <> 'HUY' 
                      AND CAST(b.Ngay_NCC_xacnhanGH AS DATE) >= '{startDate:yyyy-MM-dd}'
                      AND CAST(b.Ngay_NCC_xacnhanGH AS DATE) <= '{endDate:yyyy-MM-dd}'
                  GROUP BY Mahang, CONVERT(VARCHAR(10), b.Ngay_NCC_xacnhanGH, 120)";

                var dt_po_month = db.GET_DATA_FROM_SQL(sqlPO);
                CultureInfo engCulture = new CultureInfo("en-US");

                for (int i = 0; i < lst_using.Rows.Count; i++)
                {
                    string maHangCurrent = lst_using.Rows[i]["Using"].ToString()!;

                    worksheet_usingday.Cells[i + 2, 1].Value = maHangCurrent;
                    worksheet_po.Cells[i + 2, 1].Value = maHangCurrent;

                    float runningStock = dictStock.ContainsKey(maHangCurrent) ? dictStock[maHangCurrent] : 0f;
                    bool outOfStockWarningTriggered = false;

                    DataRow warningRow = dtWarning.NewRow();
                    warningRow["Mã vật tư"] = maHangCurrent;

                    int colIndex = 3;

                    for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                    {
                        string formattedDate = currentDate.ToString("yyyy-MM-dd");
                        bool isWeekend = (currentDate.DayOfWeek == DayOfWeek.Sunday);

                        string monthName = currentDate.ToString("MMM", engCulture);
                        float dulieu = 0;
                        if (lst_using.Columns.Contains(monthName) && lst_using.Rows[i][monthName] != DBNull.Value)
                        {
                            float.TryParse(lst_using.Rows[i][monthName].ToString(), out dulieu);
                        }

                        if (i == 0)
                        {
                            worksheet_usingday.Cells[1, colIndex].Value = currentDate.ToString("dd/MM/yyyy");
                            worksheet_po.Cells[1, colIndex].Value = currentDate.ToString("dd/MM/yyyy");

                            if (isWeekend)
                            {
                                worksheet_usingday.Cells[1, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet_usingday.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                                worksheet_po.Cells[1, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet_po.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            }
                        }

                        DataRow[] foundRows = dt_po_month.Select($"Mahang = '{maHangCurrent}' AND Ngay = '{formattedDate}'");
                        float poAmount = 0;
                        if (foundRows.Length > 0)
                        {
                            float.TryParse(foundRows[0]["Soluong"].ToString(), out poAmount);
                        }

                        float dailyUsage = isWeekend ? 0 : (dulieu / 22);

                        if (isWeekend)
                        {
                            worksheet_usingday.Cells[i + 2, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet_usingday.Cells[i + 2, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                            worksheet_po.Cells[i + 2, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet_po.Cells[i + 2, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                        else
                        {
                            worksheet_usingday.Cells[i + 2, colIndex].Value = dailyUsage;
                        }

                        if (poAmount > 0)
                        {
                            worksheet_po.Cells[i + 2, colIndex].Value = poAmount;
                        }

                        if (currentDate.Date >= DateTime.Now.Date)
                        {
                            runningStock = runningStock + poAmount - dailyUsage;

                            if (runningStock < 0)
                            {
                                if (!outOfStockWarningTriggered)
                                {
                                    DateTime warningDate = currentDate.AddDays(-3);
                                    string warningDateStr = warningDate.ToString("dd/MM/yyyy");

                                    warningRow["Ngày cần cảnh báo"] = warningDateStr;

                                    if (dtWarning.Columns.Contains(warningDateStr))
                                    {
                                        warningRow[warningDateStr] = Math.Round(runningStock, 2).ToString();
                                    }

                                    outOfStockWarningTriggered = true;
                                }
                            }
                        }

                        colIndex++;
                    }

                    if (outOfStockWarningTriggered)
                    {
                        dtWarning.Rows.Add(warningRow);
                    }
                }

                if (dtWarning.Rows.Count > 0)
                {
                    var worksheet_warning = package.Workbook.Worksheets.Add("PullInOut");
                    worksheet_warning.Cells["A1"].LoadFromDataTable(dtWarning, true);

                    // Format Header của bảng Warning cho có background màu vàng 
                    using (var range = worksheet_warning.Cells[1, 1, 1, dtWarning.Columns.Count])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }

                    // DUYỆT ĐỂ TÔ MÀU CAM VÀO CÁC Ô CHỨA DỮ LIỆU CẢNH BÁO (Bắt đầu từ cột thứ 3 trở đi)
                    for (int r = 0; r < dtWarning.Rows.Count; r++)
                    {
                        // Bắt đầu từ index 2 trong DataTable (tức là cột thứ 3: các ngày)
                        for (int c = 2; c < dtWarning.Columns.Count; c++)
                        {
                            if (!string.IsNullOrEmpty(dtWarning.Rows[r][c]?.ToString()))
                            {
                                // +2 cho Row vì dòng 1 là Header
                                // +1 cho Column vì DataTable index bắt đầu từ 0
                                var cell = worksheet_warning.Cells[r + 2, c + 1];
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                            }
                        }
                    }

                    worksheet_warning.Cells.AutoFitColumns(); // Tự động căn chỉnh độ rộng cột
                }

                // --- XUẤT FILE ---
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"TienDo_PO_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public IActionResult Quanlythanhtoan(string cmbKhoi, string txtPO, string cmbTinhTrang, DateTime? txtPhathanh1, DateTime? txtPhathanh2)
        {
            string userr = User.FindFirst("UserId")?.Value;
            SQL_Connect_DB20 db = new SQL_Connect_DB20();

            if (!txtPhathanh1.HasValue)
                txtPhathanh1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-2);
            if (!txtPhathanh2.HasValue)
                txtPhathanh2 = DateTime.Now;

            // 1. Tạo câu SQL cơ bản (lọc theo ngày và quyền user)
            string query = @"SELECT PO_Detail_Id,Id_Goc,[Benxacnhantruoc],SoPO,Mahang,   [Good_Code] as MaSanPham,Tentienganh,Tentiengviet,Soluong,Luongvethucte,Luongvekho,Luongvekho as Luongvekhocu,LuongvekhoNgaynhap,[InvoicePO],[InvoicePODenghithanhtoan],[InvoicePONgaynhap],[InvoicePONguoinhap],Dovi,Dongia,Dieukiengiaohang,Diadiemgiaohang,Phuongthucvanchuyen,Sotien,Vat,Maphongyeucau,Tenphongyeucau,Ngaygiaohangdukien,Noigiaodukien,Thoigianthanhtoan,Id_RequestDetail,Loaitien,Tygia,DoisangUSD,Danhmuc,Code_Request,[TinhtrangPO],[Tinhtrangtokhai]
                     FROM [IM_PO] 
                     WHERE [Ngayphathanh] >= '" + txtPhathanh1.Value.ToString("MM/dd/yyyy") + @"' 
                     AND [Ngayphathanh] <= '" + txtPhathanh2.Value.ToString("MM/dd/yyyy") + @"'  
                     AND [Group_Code] IN (SELECT [Group_Code] FROM [GROUP_MEMBER] WHERE [CHR_USERID] = '" + userr + @"' ) ";

            // 2. Thêm điều kiện tìm kiếm tự động (Dynamic Query)

            // Tìm theo Số PO (dùng LIKE để tìm gần đúng hoặc = để tìm chính xác)
            if (!string.IsNullOrEmpty(txtPO))
            {
                query += " AND [SoPO] LIKE '%" + txtPO.Trim() + "%' ";
            }

            // Tìm theo Tình trạng
            if (!string.IsNullOrEmpty(cmbTinhTrang))
            {
                query += " AND [TinhtrangPO] = '" + cmbTinhTrang + "' ";
            }

            // Tìm theo Khối (Cần sửa lại tên cột tương ứng trong database nếu không phải là Loaichiphi/Maphongban)
            if (!string.IsNullOrEmpty(cmbKhoi))
            {
                // Ví dụ: Giả sử thuộc khối lưu ở cột Group_Code hoặc Loaichiphi
                // query += " AND [Tên_Cột_Lưu_Khối_Của_Bạn] = '" + cmbKhoi + "' ";
            }

            // 3. Cuối cùng mới chèn lệnh ORDER BY
            query += " ORDER BY [Ngaytao] DESC";

            // Trả về DataTable
            DataTable lst = db.GET_DATA_FROM_SQL(query);

            var data = new List<Quanlythanhtoan>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                var row = lst.Rows[i];
                data.Add(new Quanlythanhtoan
                {
                    PO_Detail_Id = row["PO_Detail_Id"] != DBNull.Value ? Convert.ToInt64(row["PO_Detail_Id"]) : 0,
                    Id_Goc = row["Id_Goc"]?.ToString(),
                    Benxacnhantruoc = row["Benxacnhantruoc"]?.ToString(),
                    SoPO = row["SoPO"]?.ToString(),
                    Mahang = row["Mahang"]?.ToString(),
                    MaSanPham = row["MaSanPham"]?.ToString(), // Từ [Good_Code] as MaSanPham
                    Tentienganh = row["Tentienganh"]?.ToString(),
                    Tentiengviet = row["Tentiengviet"]?.ToString(),
                    Soluong = row["Soluong"] != DBNull.Value ? Convert.ToDecimal(row["Soluong"]) : 0,
                    Luongvethucte = row["Luongvethucte"] != DBNull.Value ? Convert.ToDecimal(row["Luongvethucte"]) : 0,
                    Luongvekho = row["Luongvekho"] != DBNull.Value ? Convert.ToDecimal(row["Luongvekho"]) : 0,
                    Luongvekhocu = row["Luongvekhocu"] != DBNull.Value ? Convert.ToDecimal(row["Luongvekhocu"]) : 0, // Từ Luongvekho as Luongvekhocu
                    LuongvekhoNgaynhap = row["LuongvekhoNgaynhap"] != DBNull.Value ? Convert.ToDateTime(row["LuongvekhoNgaynhap"]) : (DateTime?)null,

                    InvoicePO = row["InvoicePO"]?.ToString(),
                    InvoicePODenghithanhtoan = row["InvoicePODenghithanhtoan"]?.ToString(),
                    InvoicePONgaynhap = row["InvoicePONgaynhap"] != DBNull.Value ? Convert.ToDateTime(row["InvoicePONgaynhap"]) : (DateTime?)null,
                    InvoicePONguoinhap = row["InvoicePONguoinhap"]?.ToString(),

                    Dovi = row["Dovi"]?.ToString(),
                    Dongia = row["Dongia"] != DBNull.Value ? Convert.ToDecimal(row["Dongia"]) : 0,
                    Dieukiengiaohang = row["Dieukiengiaohang"]?.ToString(),
                    Diadiemgiaohang = row["Diadiemgiaohang"]?.ToString(),
                    Phuongthucvanchuyen = row["Phuongthucvanchuyen"]?.ToString(),
                    Sotien = row["Sotien"] != DBNull.Value ? Convert.ToDecimal(row["Sotien"]) : 0,
                    Vat = row["Vat"] != DBNull.Value ? Convert.ToDecimal(row["Vat"]) : 0,
                    Maphongyeucau = row["Maphongyeucau"]?.ToString(),
                    Tenphongyeucau = row["Tenphongyeucau"]?.ToString(),
                    Ngaygiaohangdukien = row["Ngaygiaohangdukien"] != DBNull.Value ? Convert.ToDateTime(row["Ngaygiaohangdukien"]) : (DateTime?)null,
                    Noigiaodukien = row["Noigiaodukien"]?.ToString(),
                    Thoigianthanhtoan = row["Thoigianthanhtoan"]?.ToString(),
                    Id_RequestDetail = row["Id_RequestDetail"]?.ToString(),
                    Loaitien = row["Loaitien"]?.ToString(),
                    Tygia = row["Tygia"] != DBNull.Value ? Convert.ToDecimal(row["Tygia"]) : 0,
                    DoisangUSD = row["DoisangUSD"] != DBNull.Value ? Convert.ToDecimal(row["DoisangUSD"]) : 0,
                    Danhmuc = row["Danhmuc"]?.ToString(),
                    Code_Request = row["Code_Request"]?.ToString(),
                    TinhtrangPO = row["TinhtrangPO"]?.ToString(),
                    Tinhtrangtokhai = row["Tinhtrangtokhai"]?.ToString()
                });
            }

            ViewBag.cmbKhoi = cmbKhoi;
            ViewBag.txtPO = txtPO;
            ViewBag.cmbTinhTrang = cmbTinhTrang;
            ViewBag.txtPhathanh1 = txtPhathanh1.Value.ToString("yyyy-MM-dd");
            ViewBag.txtPhathanh2 = txtPhathanh2.Value.ToString("yyyy-MM-dd");

            return View(data);
        }

        public IActionResult ExportExcel(string id)
        {
            // Implementation for generating and downloading Excel file
            return Content($"Exporting {id}");
        }

        [HttpGet]
        public IActionResult GetPoDetails(string soPo)
        {
            try
            {
                SQL_Connect_DB20 db = new SQL_Connect_DB20();

                // Lấy đầy đủ các cột theo thứ tự trên lưới grid trong ảnh
                string query = $@"
            SELECT 
                [PO_Detail_Id], [Id_Goc], [Tentiengviet], [Tentienganh], 
                [Good_Code], [Mahang], [Soluong], [Luongvekho], 
                [LuongvekhoNgaynhap], [Dovi], [Dongia], [Dieukiengiaohang], 
                [Diadiemgiaohang], [Phuongthucvanchuyen], [Sotien], [Vat], 
                [Maphongyeucau], [Tenphongyeucau], [Ngaygiaohangdukien], 
                [Noigiaodukien], [Thoigianthanhtoan], [Loaitien], [Tygia]
            FROM [COST_MANAGEMENT].[dbo].[IM_PO_DETAIL] 
            WHERE [SoPO] = '{soPo}'";

                System.Data.DataTable dt = db.GET_DATA_FROM_SQL(query);
                var detailList = new List<object>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        detailList.Add(new
                        {
                            MaHeThong = row["PO_Detail_Id"].ToString(),
                            DuocTachTu = row["Id_Goc"].ToString(),
                            TenTiengViet = row["Tentiengviet"].ToString(),
                            TenTiengAnh = row["Tentienganh"].ToString(),
                            MaHangHoa = row["Good_Code"].ToString(),
                            MaHang = row["Mahang"].ToString(),
                            SoLuong = row["Soluong"] != DBNull.Value ? Convert.ToDecimal(row["Soluong"]) : 0,
                            LuongVeKho = row["Luongvekho"] != DBNull.Value ? Convert.ToDecimal(row["Luongvekho"]) : 0,
                            NgayNhapKho = row["LuongvekhoNgaynhap"] != DBNull.Value ? Convert.ToDateTime(row["LuongvekhoNgaynhap"]).ToString("dd/MM/yyyy") : "",
                            DonVi = row["Dovi"].ToString(),
                            DonGiaUocTinh = row["Dongia"] != DBNull.Value ? Convert.ToDecimal(row["Dongia"]) : 0,
                            DieuKienGiaoHang = row["Dieukiengiaohang"].ToString(),
                            DiaDiemGiaoHang = row["Diadiemgiaohang"].ToString(),
                            PhuongThucVanChuyen = row["Phuongthucvanchuyen"].ToString(),
                            SoTien = row["Sotien"] != DBNull.Value ? Convert.ToDecimal(row["Sotien"]) : 0,
                            Vat = row["Vat"].ToString(),
                            PhongBanYeuCau = row["Maphongyeucau"].ToString(),
                            TenPhongYeuCau = row["Tenphongyeucau"].ToString(),
                            NgayGiaoHangDuKien = row["Ngaygiaohangdukien"] != DBNull.Value ? Convert.ToDateTime(row["Ngaygiaohangdukien"]).ToString("dd/MM/yyyy") : "",
                            NoiGiaoDuKien = row["Noigiaodukien"].ToString(),
                            ThoiHanThanhToan = row["Thoigianthanhtoan"].ToString(),
                            LoaiTien = row["Loaitien"].ToString(),
                            TyGia = row["Tygia"] != DBNull.Value ? Convert.ToDecimal(row["Tygia"]) : 0
                        });
                    }
                }

                return Json(new { success = true, data = detailList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult Thanhtoan()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SearchPO_Thanhtoan(string SoPO)
        {
            if (string.IsNullOrEmpty(SoPO))
            {
                return Json(new List<object>());
            }

            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var us = User.FindFirst("UserId")?.Value;

            // Xử lý khối an toàn (nếu us null)
            if (string.IsNullOrEmpty(us))
            {
                return Json(new { error = "Không lấy được thông tin User đăng nhập" });
            }

            var khoi = db.ReturnString($"SELECT [Group_Code] FROM [GROUP_MEMBER] WHERE CHR_USERID = '{us}'");

            // Xử lý cắt chuỗi nếu người dùng nhập nhiều PO (VD: 1604-0001,1604-0002)
            var listPO = SoPO.Split(',')
                             .Select(p => $"N'{p.Trim()}'")
                             .ToList();
            string poCondition = string.Join(",", listPO);
            if(khoi == "PUR")
            {
                khoi = "";
            }
            string sql = $@"
                    SELECT PO_Detail_Id, Id_Goc, [Benxacnhantruoc], SoPO, Mahang, [Good_Code] as MaSanPham,
                           Tentienganh, Tentiengviet, Soluong, Luongvethucte, Luongvekho, 
                           Luongvekho as Luongvekhocu, LuongvekhoNgaynhap, [InvoicePO], 
                           [InvoicePODenghithanhtoan], [InvoicePONgaynhap], [InvoicePONguoinhap], 
                           Dovi, Dongia, Dieukiengiaohang, Diadiemgiaohang, Phuongthucvanchuyen, 
                           Sotien, Vat, Maphongyeucau, Tenphongyeucau, Ngaygiaohangdukien, 
                           Noigiaodukien, Thoigianthanhtoan, Id_RequestDetail, Loaitien, 
                           Tygia, DoisangUSD, Danhmuc, Code_Request, [TinhtrangPO], [Tinhtrangtokhai] 
                    FROM [PO] 
                    WHERE SoPO IN ({poCondition}) 
                      AND [TinhtrangPO] NOT IN ('NEED','HUY') 
                     
                    ORDER BY [SoPO] DESC, Hienthi ASC";

            var lst_Po = db.GET_DATA_FROM_SQL(sql);
            System.Data.DataTable dt = db.GET_DATA_FROM_SQL(sql);
            var detailList = new List<object>();

            if (dt != null && dt.Rows.Count > 0)
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        detailList.Add(new
                        {
                            PO_Detail_Id = row["PO_Detail_Id"] != DBNull.Value ? Convert.ToInt64(row["PO_Detail_Id"]) : 0,
                            Id_Goc = row["Id_Goc"]?.ToString(),
                            Benxacnhantruoc = row["Benxacnhantruoc"]?.ToString(),
                            SoPO = row["SoPO"]?.ToString(),
                            Mahang = row["Mahang"]?.ToString(),
                            MaSanPham = row["MaSanPham"]?.ToString(), // Từ [Good_Code] as MaSanPham
                            Tentienganh = row["Tentienganh"]?.ToString(),
                            Tentiengviet = row["Tentiengviet"]?.ToString(),
                            Soluong = row["Soluong"] != DBNull.Value ? Convert.ToDecimal(row["Soluong"]) : 0,
                            Luongvethucte = row["Luongvethucte"] != DBNull.Value ? Convert.ToDecimal(row["Luongvethucte"]) : 0,
                            Luongvekho = row["Luongvekho"] != DBNull.Value ? Convert.ToDecimal(row["Luongvekho"]) : 0,
                            Luongvekhocu = row["Luongvekhocu"] != DBNull.Value ? Convert.ToDecimal(row["Luongvekhocu"]) : 0, // Từ Luongvekho as Luongvekhocu
                            LuongvekhoNgaynhap = row["LuongvekhoNgaynhap"] != DBNull.Value ? Convert.ToDateTime(row["LuongvekhoNgaynhap"]) : (DateTime?)null,

                            InvoicePO = row["InvoicePO"]?.ToString(),
                            InvoicePODenghithanhtoan = row["InvoicePODenghithanhtoan"]?.ToString(),
                            InvoicePONgaynhap = row["InvoicePONgaynhap"] != DBNull.Value ? Convert.ToDateTime(row["InvoicePONgaynhap"]) : (DateTime?)null,
                            InvoicePONguoinhap = row["InvoicePONguoinhap"]?.ToString(),

                            Dovi = row["Dovi"]?.ToString(),
                            Dongia = row["Dongia"] != DBNull.Value ? Convert.ToDecimal(row["Dongia"]) : 0,
                            Dieukiengiaohang = row["Dieukiengiaohang"]?.ToString(),
                            Diadiemgiaohang = row["Diadiemgiaohang"]?.ToString(),
                            Phuongthucvanchuyen = row["Phuongthucvanchuyen"]?.ToString(),
                            Sotien = row["Sotien"] != DBNull.Value ? Convert.ToDecimal(row["Sotien"]) : 0,
                            Vat = row["Vat"] != DBNull.Value ? Convert.ToDecimal(row["Vat"]) : 0,
                            Maphongyeucau = row["Maphongyeucau"]?.ToString(),
                            Tenphongyeucau = row["Tenphongyeucau"]?.ToString(),
                            Ngaygiaohangdukien = row["Ngaygiaohangdukien"] != DBNull.Value ? Convert.ToDateTime(row["Ngaygiaohangdukien"]) : (DateTime?)null,
                            Noigiaodukien = row["Noigiaodukien"]?.ToString(),
                            Thoigianthanhtoan = row["Thoigianthanhtoan"]?.ToString(),
                            Id_RequestDetail = row["Id_RequestDetail"]?.ToString(),
                            Loaitien = row["Loaitien"]?.ToString(),
                            Tygia = row["Tygia"] != DBNull.Value ? Convert.ToDecimal(row["Tygia"]) : 0,
                            DoisangUSD = row["DoisangUSD"] != DBNull.Value ? Convert.ToDecimal(row["DoisangUSD"]) : 0,
                            Danhmuc = row["Danhmuc"]?.ToString(),
                            Code_Request = row["Code_Request"]?.ToString(),
                            TinhtrangPO = row["TinhtrangPO"]?.ToString(),
                            Tinhtrangtokhai = row["Tinhtrangtokhai"]?.ToString()
                        });
                    }
                }
            }
                      // Trả về JSON cho Ajax gọi tới
            return Json(detailList);
        }
        [HttpPost]
        public JsonResult UpdateInvoice([FromBody] UpdateInvoiceRequest request) // <--- SỬA DÒNG NÀY
        {
            try
            {
                // Gán lại ra biến cho giống code cũ của bạn
                var ids = request.ids;
                var loaiNhap = request.loaiNhap;
                var duLieu = request.duLieu;

                if (ids == null || ids.Count == 0)
                {
                    return Json(new { success = false, error = "Không có mã hệ thống nào được chọn." });
                }

                SQL_Connect_DB20 db = new SQL_Connect_DB20();
                var us = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(us))
                {
                    return Json(new { success = false, error = "Không lấy được thông tin User đăng nhập." });
                }

                if (duLieu == null) { duLieu = ""; }

                foreach (var id in ids)
                {
                    string safeId = id.Replace("'", "''");
                    string sqlUpdate = "";
                    string sqlQuery = "";

                    if (loaiNhap == "Số Invoice")
                    {
                        sqlUpdate = $@"UPDATE [IM_PO_DETAIL] SET [InvoicePO] = N'{duLieu}',[InvoicePONgaynhap] = GETDATE(),[InvoicePONguoinhap] = '{us}' WHERE [PO_Detail_Id] = '{safeId}'";

                        // LƯU Ý: Ở ĐÂY BẠN VẪN ĐANG THIẾU DẤU NHÁY ĐƠN BAO QUANH '{safeId}'
                        sqlQuery = $@" UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] SET [So_hoadon] = N'{duLieu}' WHERE [Id_Detail_PO] = '{safeId}'";
                    }
                    else // "Đề nghị thanh toán"
                    {
                        sqlUpdate = $@"UPDATE [IM_PO_DETAIL] SET [InvoicePODenghithanhtoan] = N'{duLieu}',[InvoicePONgaynhap] = GETDATE(),[InvoicePONguoinhap] = '{us}' WHERE [PO_Detail_Id] = '{safeId}'";

                        // LƯU Ý: TƯƠNG TỰ, THÊM DẤU NHÁY ĐƠN '{safeId}' VÀO ĐÂY
                        sqlQuery = $@" UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] SET [So_DNTT] = N'{duLieu}' WHERE [Id_Detail_PO] = '{safeId}'";
                    }

                    // XIN NHẮC LẠI: Hàm thực thi lệnh UPDATE không nên dùng hàm GET_DATA... (vì nó trả về bảng). 
                    // Bạn nên đổi thành hàm ExecuteNonQuery của bạn.
                    db.GET_DATA_FROM_SQL(sqlUpdate);
                    db.GET_DATA_FROM_SQL(sqlQuery);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ResetInvoiceData(List<string> ids)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return Json(new { success = false, error = "Chưa có PO nào được chọn." });
                }

                // Lấy tên User đang đăng nhập (tùy vào hệ thống của bạn đang dùng Session hay Identity)
                string currentUser = User.FindFirst("UserId")?.Value;

                // Giả sử con.Excutesql là thư viện dùng ADO.NET của bạn
                foreach (var id in ids)
                {
                    // Escape hoặc dùng Parameter để chống SQL Injection
                    string sql = $@"
                            UPDATE [IM_PO_DETAIL] SET [InvoicePO] = NULL,
                                [InvoicePODenghithanhtoan] = NULL,
                                [BBGH] = NULL, 
                                [HD] = NULL,   
                                [InvoicePONgaynhap] = GETDATE(),
                                [InvoicePONguoinhap] = '{currentUser}' 
                            WHERE [PO_Detail_Id] = '{id.Trim()}'";

                    // Gọi hàm thực thi SQL của bạn
                    db.GET_DATA_FROM_SQL(sql);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult SplitPO([FromBody] SplitPoRequest request)
        {
            try
            {
                // 1. Validate dữ liệu đầu vào
                if (request == null || request.PoDetailId <= 0 || request.Soluongmoi <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ (PO Detail ID hoặc Số lượng tách <= 0)." });
                }

                SQL_Connect_DB20 sQL_Connect = new SQL_Connect_DB20();
                string sqlQuery = $@"
                        BEGIN TRY
                            BEGIN TRANSACTION;
                                -- 1. Khai báo biến để lưu ID mới được tạo
                                DECLARE @NewPoDetailId INT;

                                -- A. INSERT DÒNG MỚI (Số lượng = {request.Soluongmoi})
                                INSERT INTO IM_PO_DETAIL(
                                    [SoPO],[Tentienganh],[Tentiengviet],[Mahang],[Soluong],[Dovi],[Dongia],[Dieukiengiaohang],
                                    [Diadiemgiaohang],[Phuongthucvanchuyen],[Sotien],[Vat],[Maphongyeucau],[Tenphongyeucau],
                                    [Ngaygiaohangdukien],[Noigiaodukien],[Thoigianthanhtoan],[Code_Request],[Id_RequestDetail],
                                    [Loaitien],[Tygia],[DoisangUSD],[Danhmuc],[Id_Goc],[Hienthi],[Benxacnhantruoc],[Good_Code]
                                )
                                SELECT 
                                    [SoPO], [Tentienganh], [Tentiengviet], [Mahang], 
                                    {request.Soluongmoi}, 
                                    [Dovi], [Dongia], [Dieukiengiaohang],
                                    [Diadiemgiaohang], [Phuongthucvanchuyen],
                                    (ISNULL([Dongia], 0) * {request.Soluongmoi}),
                                    [Vat], [Maphongyeucau], [Tenphongyeucau],
                                    [Ngaygiaohangdukien], [Noigiaodukien], [Thoigianthanhtoan],
                                    [Code_Request], [Id_RequestDetail], [Loaitien], [Tygia],
                                    CASE 
                                        WHEN ISNULL([Tygia], 0) > 0 THEN ((ISNULL([Dongia], 0) * {request.Soluongmoi}) / [Tygia]) 
                                        ELSE 0 
                                    END,
                                    [Danhmuc], 
                                    [PO_Detail_Id], 
                                    ISNULL([Hienthi], 0) + 1, 
                                    'STOCK', 
                                    [Good_Code]
                                FROM IM_PO_DETAIL 
                                WHERE PO_Detail_Id = {request.PoDetailId};


                                -- 2. Lấy ID của dòng vừa INSERT thành công
                                SET @NewPoDetailId = SCOPE_IDENTITY();

                                -- 3. INSERT VÀO BẢNG PE_THEODOITIENDO
                                -- Trường hợp 1: Nếu PO cũ đã có dữ liệu trong PE_THEODOITIENDO -> Sao chép thông tin sang cho PO_Detail_Id mới
                                IF EXISTS (SELECT 1 FROM [PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {request.PoDetailId})
                                BEGIN
                                    INSERT INTO [PE_THEODOITIENDO] (
                                        [SoPO], [Id_PO], [Id_Detail_PO], [Ngay_gui_PO], 
                                        [Dieuchinhlichgiao], [Drawchinhlichgiao], [Note]
                                    )
                                    SELECT 
                                        [SoPO], [Id_PO], @NewPoDetailId, [Ngay_gui_PO],
                                        [Dieuchinhlichgiao], [Drawchinhlichgiao],
                                        N'Tách từ Id_Detail_PO: {request.PoDetailId}'
                                    FROM [PE_THEODOITIENDO]
                                    WHERE [Id_Detail_PO] = {request.PoDetailId};
                                END
                                -- Trường hợp 2: Nếu PO cũ chưa có trong PE_THEODOITIENDO -> Tạo dòng mới với các thông tin cơ bản từ IM_PO_DETAIL
                                ELSE
                                BEGIN
                                    INSERT INTO [PE_THEODOITIENDO] (
                                        [SoPO], [Id_Detail_PO], [Note]
                                    )
                                    SELECT 
                                        [SoPO], @NewPoDetailId, N'Tách mới từ PO_Detail_Id: {request.PoDetailId}'
                                    FROM IM_PO_DETAIL
                                    WHERE PO_Detail_Id = @NewPoDetailId;
                                END


                                -- B. UPDATE LẠI DÒNG GỐC (Trừ đi số lượng đã tách)
                                UPDATE IM_PO_DETAIL
                                SET 
                                    -- Đưa tính toán Sotien và DoisangUSD lên trước
                                    Sotien = (Soluong - {request.Soluongmoi}) * ISNULL(Dongia, 0),
                                    DoisangUSD = CASE 
                                                     WHEN ISNULL(Tygia, 0) > 0 THEN ((Soluong - {request.Soluongmoi}) * ISNULL(Dongia, 0)) / Tygia 
                                                     ELSE 0 
                                                 END,
                                    -- Cập nhật Soluong để ở cuối cùng
                                    Soluong = Soluong - {request.Soluongmoi}
                                WHERE PO_Detail_Id = {request.PoDetailId};


                                -- Xác nhận lưu dữ liệu
                                COMMIT TRANSACTION;

                            END TRY
                            BEGIN CATCH
                                -- Nếu có lỗi, huỷ bỏ toàn bộ các thay đổi chưa được commit
                                IF @@TRANCOUNT > 0
                                    ROLLBACK TRANSACTION;

                                -- Quăng lỗi ra ngoài cho code (ví dụ C#) bắt được exception
                                DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
                                RAISERROR(@ErrorMessage, 16, 1);
                            END CATCH;
                    ";
                bool isSuccess = sQL_Connect.EXECUTE_SQL(sqlQuery);

                if (isSuccess)
                {
                    return Json(new { success = true, message = "Tách PO thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Lỗi! Không thể thực thi câu lệnh SQL." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateNote([FromBody] UpdateNoteModel model)
        {
            try
            {
                if (model.PoDetailId <= 0)
                {
                    return Json(new { success = false, message = "ID chi tiết PO không hợp lệ." });
                }

                // Xử lý chuỗi để tránh lỗi SQL Injection khi người dùng nhập dấu nháy đơn (')
                string safeNote = string.IsNullOrEmpty(model.Note) ? "" : model.Note.Replace("'", "''");

                // Logic UPSERT: Update nếu đã có dòng theo dõi, Insert nếu chưa có
                string sqlQuery = $@"
                        IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE [Id_Detail_PO] = {model.PoDetailId})
                        BEGIN
                            UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO]
                            SET [Note] = N'{safeNote}'
                            WHERE [Id_Detail_PO] = {model.PoDetailId}
                        END
                        ELSE
                        BEGIN
                            -- Nếu chưa có dòng theo dõi tiến độ, thực hiện Insert lấy SoPO từ bảng PO gốc
                            INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] ([SoPO], [Id_Detail_PO], [Note])
                            SELECT [SoPO], {model.PoDetailId}, N'{safeNote}'
                            FROM [COST_MANAGEMENT].[dbo].[PO]
                            WHERE [PO_Detail_Id] = {model.PoDetailId}
                        END
                    ";

                // Thực thi câu lệnh SQL bằng class kết nối của bạn
                SQL_Connect_DB20 sql = new SQL_Connect_DB20();
                sql.GET_DATA_FROM_SQL(sqlQuery);

                return Json(new { success = true, message = "Cập nhật ghi chú thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        public IActionResult Master_NCC()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            List<NccModel> list = new List<NccModel>();
            string sql = "SELECT [Id], [TenNCC], [MaNCC], [Damnhiem] FROM [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC]";
           
            using (SqlConnection conn = new SqlConnection(db.connectString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new NccModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TenNCC = reader["TenNCC"].ToString(),
                            MaNCC = reader["MaNCC"].ToString(),
                            Damnhiem = reader["Damnhiem"].ToString()
                        });
                    }
                }
            }
            return View(list);
        }
        [HttpPost]
        public IActionResult CreateNCC([FromBody] NccModel model)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                if (string.IsNullOrEmpty(model.MaNCC)) return Json(new { success = false, message = "Mã NCC không được để trống." });

                string sqlQuery = @"INSERT INTO [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC] (TenNCC, MaNCC, Damnhiem) 
                                VALUES (@TenNCC, @MaNCC, @Damnhiem)";

                using (SqlConnection conn = new SqlConnection(db.connectString))
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@TenNCC", model.TenNCC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaNCC", model.MaNCC);
                    cmd.Parameters.AddWithValue("@Damnhiem", model.Damnhiem ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Thêm nhà cung cấp thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // --- API CẬP NHẬT (SỬA) ---
        [HttpPost]
        public IActionResult UpdateNCC([FromBody] NccModel model)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                if (model.Id <= 0) return Json(new { success = false, message = "ID không hợp lệ." });

                string sqlQuery = @"UPDATE [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC] 
                                SET TenNCC = @TenNCC, MaNCC = @MaNCC, Damnhiem = @Damnhiem 
                                WHERE Id = @Id";

                using (SqlConnection conn = new SqlConnection(db.connectString))
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@TenNCC", model.TenNCC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaNCC", model.MaNCC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Damnhiem", model.Damnhiem ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // --- API XÓA ---
        [HttpPost]
        public IActionResult DeleteNCC([FromBody] DeleteNccModel model)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                if (model.Id <= 0) return Json(new { success = false, message = "ID không hợp lệ." });

                string sqlQuery = "DELETE FROM [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC] WHERE Id = @Id";

                using (SqlConnection conn = new SqlConnection(db.connectString))
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Xóa nhà cung cấp thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // --- API IMPORT EXCEL ---
        [HttpPost]
        public IActionResult ImportExcel(IFormFile file)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            try
            {
                if (file == null || file.Length == 0) return Json(new { success = false, message = "Vui lòng chọn file Excel." });

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                int count = 0;

                using (var stream = new System.IO.MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        using (SqlConnection conn = new SqlConnection(db.connectString))
                        {
                            conn.Open();
                            // Chạy từ dòng 2 (bỏ dòng tiêu đề)
                            for (int row = 2; row <= rowCount; row++)
                            {
                                var tenNcc = worksheet.Cells[row, 2].Value?.ToString();
                                var maNcc = worksheet.Cells[row, 3].Value?.ToString();
                                var damNhiem = worksheet.Cells[row, 4].Value?.ToString();

                                if (!string.IsNullOrEmpty(maNcc))
                                {
                                    // Sử dụng IF EXISTS để kiểm tra xem MaNCC đã tồn tại chưa
                                    string sqlQuery = @"
                                        IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC] WHERE MaNCC = @MaNCC)
                                        BEGIN
                                            UPDATE [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC]
                                            SET TenNCC = @TenNCC,
                                                Damnhiem = @Damnhiem
                                            WHERE MaNCC = @MaNCC
                                        END
                                        ELSE
                                        BEGIN
                                            INSERT INTO [COST_MANAGEMENT].[dbo].[PE_DamnhiemNCC] (TenNCC, MaNCC, Damnhiem) 
                                            VALUES (@TenNCC, @MaNCC, @Damnhiem)
                                        END";

                                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                                    {
                                        // Gán tham số (thêm DBNull.Value nếu giá trị null để tránh lỗi SQL)
                                        cmd.Parameters.AddWithValue("@TenNCC", tenNcc ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@MaNCC", maNcc);
                                        cmd.Parameters.AddWithValue("@Damnhiem", damNhiem ?? (object)DBNull.Value);

                                        // Thực thi câu lệnh
                                        cmd.ExecuteNonQuery();
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
                return Json(new { success = true, message = $"Import thành công {count} dòng dữ liệu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
       
        public class DeleteNccModel
        {
            public int Id { get; set; }
        }

        public JsonResult List_DamnhiemNCC()
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var lt = db.GET_DATA_FROM_SQL("select distinct(Damnhiem) from PE_DamnhiemNCC");
            List<string> damnhiem = new List<string>();
            for(int i = 0; i < lt.Rows.Count; i++)
            {
                damnhiem.Add(lt.Rows[i][0].ToString()!);
            }
            return Json(damnhiem);
        }
        public JsonResult List_NCCtheodamnhiem(string tendamnhiem, string danhmuc)
        {
            if(danhmuc == "ngoai") { danhmuc = "OUT"; } else { danhmuc = "IN"; };
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var lt = db.GET_DATA_FROM_SQL(@$"SELECT a.TenNCC 
                FROM [COST_MANAGEMENT].[dbo].[PO] AS a
                OUTER APPLY (
                    SELECT TOP 1 Damnhiem 
                    FROM PE_DamnhiemNCC 
                    WHERE MaNCC = a.MaNCC
                ) AS c
                WHERE a.Group_Code = 'PUR' 
                  AND a.Danhmuc = '{danhmuc}'
                  AND a.Ngayphathanh >= '2024-01-01'
                  AND a.TinhtrangPO NOT IN ('HOANTHANH', 'HUY') 
                  AND a.Luongvekho IS NULL 
                  AND a.Luongvethucte IS NULL
                  AND c.Damnhiem = N'{tendamnhiem}'
                GROUP BY a.TenNCC");
            List<string> damnhiem = new List<string>();
            for (int i = 0; i < lt.Rows.Count; i++)
            {
                damnhiem.Add(lt.Rows[i][0].ToString()!);
            }
            return Json(damnhiem);
        }
    }
}

