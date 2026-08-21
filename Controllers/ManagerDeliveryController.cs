using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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
    public class NCC_NG
    {
        public string mancc { get; set; }
        public string tenncc { get; set; }
        public string giogiao { get; set; }
        public string soPO { get; set; }
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
    public class ManagerDeliveryController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> ImportExcelTiendo(IFormFile file)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "File không hợp lệ hoặc bị rỗng." });
            }

            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                return Json(new { success = false, message = "Vui lòng chọn đúng định dạng file Excel (.xlsx hoặc .xls)" });
            }

            try
            {
                // Thiết lập License cho EPPlus (Bắt buộc với bản mới)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Đọc file trực tiếp từ bộ nhớ
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        // Lấy Sheet đầu tiên
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null) return Json(new { success = false, message = "Không tìm thấy Sheet dữ liệu." });

                        int rowCount = worksheet.Dimension.Rows;

                        // Duyệt từ dòng số 2 (bỏ qua dòng Header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            // 1. Lấy khóa chính ID (Trong EPPlus, Cột đầu tiên là 1)
                            string idDetailPo = worksheet.Cells[row, 1].Text?.Trim();
                            string soPO = worksheet.Cells[row, 4].Text?.Trim();
                            if (string.IsNullOrEmpty(idDetailPo)) continue;

                            // 2. Lấy dữ liệu các cột theo index (Đã cộng thêm 1 so với code dùng mảng 0-index cũ)
                            string ngayGuiPo = worksheet.Cells[row, 11].Text?.Trim(); // Cột 10 Excel cũ -> Index 11
                            string ngayNccXacnhanGh = worksheet.Cells[row, 13].Text?.Trim(); // Cột 12 Excel cũ -> Index 13
                            string ngayGhChinhThuc = worksheet.Cells[row, 14].Text?.Trim();
                            string lichGiao = worksheet.Cells[row, 15].Text?.Trim();
                            string anhHuongSx = worksheet.Cells[row, 16].Text?.Trim();
                            string cuaGh = worksheet.Cells[row, 17].Text?.Trim();
                            string congNhanHang = worksheet.Cells[row, 18].Text?.Trim();
                            string nguoiNhanHang = worksheet.Cells[row, 19].Text?.Trim();
                            string soDntt = worksheet.Cells[row, 20].Text?.Trim();
                            string soHoaDon = worksheet.Cells[row, 21].Text?.Trim();

                            // 3. Cập nhật vào DB bằng Dapper
                            string query = $@"
                                IF EXISTS (SELECT 1 FROM [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] WHERE Id_Detail_PO = '{idDetailPo}')
                                BEGIN
                                    UPDATE [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                                    SET
                                        SoPO = '{soPO}',
                                        Ngay_gui_PO = '{ngayGuiPo}',
                                        Ngay_NCC_xacnhanGH = '{ngayNccXacnhanGh}',
                                        Ngay_GHchinhthuc = '{ngayGhChinhThuc}',
                                        Lichgiao = '{lichGiao}',
                                        Anh_huong_SX = '{anhHuongSx}',
                                        Cua_GH = '{cuaGh}',
                                        Cong_Nhanhang = '{congNhanHang}',
                                        Nguoi_Nhanhang = '{nguoiNhanHang}',
                                        So_DNTT = '{soDntt}',
                                        So_hoadon = '{soHoaDon}'
                                    WHERE Id_Detail_PO = '{idDetailPo}';
                                END ELSE BEGIN                              
                                    INSERT INTO [COST_MANAGEMENT].[dbo].[PE_THEODOITIENDO] 
                                    (
                                        SoPO,Id_Detail_PO,Ngay_gui_PO, Ngay_NCC_xacnhanGH, Ngay_GHchinhthuc, Lichgiao, Anh_huong_SX, Cua_GH, Cong_Nhanhang, Nguoi_Nhanhang, So_DNTT, So_hoadon
                                    )
                                    VALUES  
                                    (
                                        '{soPO}','{idDetailPo}', '{ngayGuiPo}', '{ngayNccXacnhanGh}', '{ngayGhChinhThuc}', '{lichGiao}', '{anhHuongSx}', '{cuaGh}', '{congNhanHang}', '{nguoiNhanHang}', '{soDntt}', '{soHoaDon}'
                                    );
                                END ";
                            db.GET_DATA_FROM_SQL_TEST(query);
                        }

                    }
                }

                return Json(new { success = true, message = $"Cập nhật thành công !" });
            }
            catch (Exception ex)
            {
                // Log lại exception nếu hệ thống bạn có bộ ghi log
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
                           )) AS Using3Thang
                ";

            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            var dt = sql.GET_DATA_FROM_SQL_TEST(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                double sl_po = Convert.ToDouble(dt.Rows[0]["SoLuongPO"]);
                double stock = Convert.ToDouble(dt.Rows[0]["Stock"]);
                double using_ngay = Convert.ToDouble(dt.Rows[0]["UsingNgay"]);
                double using3thang = Convert.ToDouble(dt.Rows[0]["Using3Thang"]);

                double canhbao = stock + sl_po - using_ngay;
                var trangThaiCanhBao = "AT";

                if (canhbao < 0)
                {
                    trangThaiCanhBao = "IN";
                }

                if (stock > using3thang)
                {
                    trangThaiCanhBao = "OUT";
                }

                return trangThaiCanhBao;
            }

            return "";
        }
        public IActionResult ManageDelivery(int page = 1, string searchTerm = "", string reqMonth = "", string tab = "ngoai", string impactStatus = "")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();
            // update lịch giao và ảnh hưởng sx

            sql.GET_DATA_FROM_SQL_TEST(@"UPDATE b
                              SET 
                                  -- Xét điều kiện cho cột Lichgiao
                                  b.Lichgiao = CASE 
                                                  WHEN b.Ngay_NCC_xacnhanGH IS NOT NULL
                                                       AND MONTH(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = MONTH(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                                       AND YEAR(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = YEAR(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                                  THEN 'OK'
                                                  ELSE 'NG'
                                               END,          
                                  -- Cột Anh_huong_SX chỉ được update thành 'No' nếu Lịch giao là OK, ngược lại giữ nguyên giá trị cũ (hoặc tuỳ chỉnh theo logic của bạn)
                                  b.Anh_huong_SX = CASE 
                                                      WHEN b.Ngay_NCC_xacnhanGH IS NOT NULL
                                                           AND MONTH(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = MONTH(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                                           AND YEAR(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = YEAR(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                                                      THEN 'No'
                                                      ELSE b.Anh_huong_SX 
                                                   END
                              FROM PE_THEODOITIENDO b
                              JOIN [COST_MANAGEMENT].[dbo].[PO] a ON a.PO_Detail_Id = b.Id_Detail_PO
                              WHERE a.Ngaygiaohangdukien IS NOT NULL;");

            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");
            var khoi = "";
            if (checkus == "PUR") { khoi = "AND Group_Code = 'PUR'"; }
            ;
            if (checkus == "GA") { khoi = "AND Group_Code = 'GA'"; }
            ;

            // TỐI ƯU 1: ÉP LỌC DANH MỤC TRỰC TIẾP Ở SQL ĐỂ GIẢM 50% RAM
            string tabCondition = tab == "trong" ? "AND a.Danhmuc = 'IN'" : "AND (a.Danhmuc IS NULL OR a.Danhmuc = 'OUT')";

            string query = $@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
            LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
            WHERE Ngayphathanh >= '2026-07-01' {khoi} {tabCondition} ORDER BY Ngayphathanh DESC";

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
            List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                PoDetailViewModel po = new PoDetailViewModel();
                po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString()!);
                po.Ngayyc = lst.Rows[i]["Ngaytao"].ToString()!.Split(' ')[0];
                po.Ngayycgiao = lst.Rows[i]["Ngaygiaohangdukien"].ToString()!.Split(' ')[0];
                po.SoPO = lst.Rows[i]["SoPO"].ToString();
                po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                po.Mahang = lst.Rows[i]["Mahang"].ToString();
                po.Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!);
                po.Donvi = lst.Rows[i]["Dovi"].ToString();
                po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                po.DNphathanhpo = lst.Rows[i]["Nguoilamdon"].ToString()?.ToLower();
                po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();
                po.MaNhacungcap = lst.Rows[i]["MaNCC"].ToString();

                object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";

                object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";

                po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                po.lichgiao = lst.Rows[i]["Lichgiao"].ToString();
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

                // CHÚ Ý: ĐÃ XÓA GỌI HÀM pullin_pullout TẠI ĐÂY (Để tính sau khi phân trang)
                listPo.Add(po);
            }

            // Lọc theo Search (Chạy trên RAM)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                listPo = listPo.Where(x =>
                    (x.SoPO?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Tentiengviet?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Nhacungcap?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Mahang?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(reqMonth))
            {
                listPo = listPo.Where(x => {
                    if (DateTime.TryParse(x.Ngayycgiao, out DateTime dt))
                    {
                        return dt.ToString("yyyy-MM") == reqMonth;
                    }
                    return false;
                }).ToList();
            }
            if (!string.IsNullOrEmpty(impactStatus))
            {
                if (impactStatus == "WAIT")
                {
                    // Điều kiện: Lịch giao là NG và cột Ảnh hưởng SX đang rỗng (null hoặc rỗng)
                    listPo = listPo.Where(x =>
                        !string.IsNullOrEmpty(x.lichgiao) && x.lichgiao.Trim().ToUpper() == "NG" &&
                        string.IsNullOrEmpty(x.anhuongsx)
                    ).ToList();
                }
                else
                {
                    // Lọc bình thường khi value là "NG" hoặc "OK"
                    listPo = listPo.Where(x =>
                        !string.IsNullOrEmpty(x.lichgiao) &&
                        x.lichgiao.Trim().Equals(impactStatus, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }
            // Phân trang
            int pageSize = 100;
            int totalRecords = listPo.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedList = listPo.OrderByDescending(x => x.PO_Detail_Id)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            // TỐI ƯU 2: CHỈ TÍNH TOÁN CẢNH BÁO CHO NHỮNG DÒNG ĐƯỢC HIỂN THỊ TRÊN TRANG (Tối đa 100 dòng)
            if (tab == "trong")
            {
                foreach (var po in pagedList)
                {
                    string ngayUuTien = !string.IsNullOrEmpty(po.Ngay_GHchinhthuc) ? po.Ngay_GHchinhthuc :
                                        !string.IsNullOrEmpty(po.ngaynccxngiao) ? po.ngaynccxngiao :
                                        po.Ngayycgiao!;

                    po.canhbao = pullin_pullout(po.Mahang!, ngayUuTien);
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentTab = tab;
            ViewBag.ReqMonth = reqMonth;
            ViewBag.ImpactStatus = impactStatus;

            TempData["Tongsoluong"] = totalRecords;

            return View(pagedList);
        }
        // Model nhận dữ liệu từ AJAX
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

                // Nếu ngày để trống -> giữ nguyên giá trị cột cũ (cho UPDATE)
                string updateSendDate = string.IsNullOrEmpty(model.SendDate) ? "[Ngay_gui_PO]" : $"'{model.SendDate}'";
                string updateConfirmDate = string.IsNullOrEmpty(model.ConfirmDate) ? "[Ngay_NCC_xacnhanGH]" : $"'{model.ConfirmDate}'";

                // Nếu ngày để trống -> truyền giá trị NULL (cho INSERT)
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
                    -- 1. Cập nhật thông tin ngày tháng cho những dòng đã tồn tại
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
                sql.GET_DATA_FROM_SQL_TEST(sqlQuery);

                return Json(new { success = true, message = "Cập nhật tiến độ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
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
                            [Nguoi_Nhanhang] = N'{model.NguoiNhanHang}'
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
                            [Nguoi_Nhanhang] = N'{model.NguoiNhanHang}'
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
                sql.GET_DATA_FROM_SQL_TEST(sqlQuery);

                return Json(new { success = true, message = "Cập nhật dữ liệu nhận hàng thực tế thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // Tạo Model để nhận dữ liệu JSON gửi lên
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
                sql.GET_DATA_FROM_SQL_TEST(sqlQuery);

                return Json(new { success = true, message = "Cập nhật dữ liệu thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult ExportExcel(string searchTerm = "", string reqMonth = "", string tab = "ngoai", string impactStatus = "")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");
            var khoi = "";
            if (checkus == "PUR") { khoi = "AND Group_Code = 'PUR'"; }
            if (checkus == "GA") { khoi = "AND Group_Code = 'GA'"; }

            string tabCondition = tab == "trong" ? "AND a.Danhmuc = 'IN'" : "AND (a.Danhmuc IS NULL OR a.Danhmuc = 'OUT')";
            string query = $@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                      LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
                      WHERE Ngayphathanh >= '2026-07-01' {khoi} {tabCondition} ORDER BY Ngayphathanh DESC";

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
            List<PoDetailViewModel> listPo = new List<PoDetailViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                PoDetailViewModel po = new PoDetailViewModel();

                po.PO_Detail_Id = int.Parse(lst.Rows[i]["PO_Detail_Id"].ToString()!);
                po.Ngayyc = lst.Rows[i]["Ngaytao"].ToString()!.Split(' ')[0];
                po.Ngayycgiao = lst.Rows[i]["Ngaygiaohangdukien"].ToString()!.Split(' ')[0];
                po.SoPO = lst.Rows[i]["SoPO"].ToString();
                po.Tentiengviet = lst.Rows[i]["Tentiengviet"].ToString();
                po.Mahang = lst.Rows[i]["Mahang"].ToString();
                po.Soluong = double.Parse(lst.Rows[i]["Soluong"].ToString()!);
                po.Donvi = lst.Rows[i]["Dovi"].ToString();
                po.Nhacungcap = lst.Rows[i]["TenNCC"].ToString();
                po.DNphathanhpo = lst.Rows[i]["Nguoilamdon"].ToString()?.ToLower();
                po.DNphongban = lst.Rows[i]["Nguoixacnhan"].ToString();
                po.MaNhacungcap = lst.Rows[i]["MaNCC"].ToString();

                object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";

                object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";

                // XỬ LÝ LOGIC LỊCH GIAO (OK/NG)
                po.lichgiao = "";
                if (!string.IsNullOrEmpty(po.Ngayycgiao) && !string.IsNullOrEmpty(po.ngaynccxngiao))
                {
                    if (DateTime.TryParse(po.Ngayycgiao, out DateTime dtNgayYcGiao) && DateTime.TryParse(po.ngaynccxngiao, out DateTime dtNgayNccXacNhan))
                    {
                        if (dtNgayYcGiao.Month == dtNgayNccXacNhan.Month && dtNgayYcGiao.Year == dtNgayNccXacNhan.Year)
                        {
                            po.lichgiao = "OK";
                        }
                        else
                        {
                            po.lichgiao = "NG";
                        }
                    }
                }

                if (po.lichgiao == "OK")
                {
                    po.anhuongsx = "No";
                }
                else
                {
                    po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                }

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

            // 2. Lọc theo Tab (Danh mục)
            if (tab == "trong")
            {
                listPo = listPo.Where(x => !string.IsNullOrEmpty(x.Danhmuc) && x.Danhmuc == "IN").ToList();
            }
            else
            {
                listPo = listPo.Where(x => string.IsNullOrEmpty(x.Danhmuc) || x.Danhmuc == "OUT").ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                listPo = listPo.Where(x =>
                    (x.SoPO?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Tentiengviet?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Nhacungcap?.ToLower().Contains(searchLower) ?? false) ||
                    (x.Mahang?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(reqMonth))
            {
                listPo = listPo.Where(x => {
                    if (DateTime.TryParse(x.Ngayycgiao, out DateTime dt))
                        return dt.ToString("yyyy-MM") == reqMonth;
                    return false;
                }).ToList();
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(impactStatus))
            {
                if (impactStatus == "WAIT")
                {
                    listPo = listPo.Where(x =>
                        !string.IsNullOrEmpty(x.lichgiao) && x.lichgiao.Trim().ToUpper() == "NG" &&
                        string.IsNullOrEmpty(x.anhuongsx)
                    ).ToList();
                }
                else
                {
                    listPo = listPo.Where(x =>
                        !string.IsNullOrEmpty(x.lichgiao) &&
                        x.lichgiao.Trim().Equals(impactStatus, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }

            if (!listPo.Any())
            {
                return Content("Không có dữ liệu phù hợp với điều kiện lọc để xuất Excel.");
            }

            // 5. Mở file mẫu và xuất Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "Nhaptiendo.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return Content("Không tìm thấy file mẫu Excel tại hệ thống (data/Nhaptiendo.xlsx).");
            }

            FileInfo templateFile = new FileInfo(templatePath);

            using (var package = new ExcelPackage(templateFile))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Chọn lọc đúng các cột cần hiển thị trên Excel theo thứ tự của bảng ManageDelivery
                var exportData = listPo.Select(x => new
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

                // Đổ dữ liệu từ Dòng 2, Ô A2 (false = không ghi lại Tiêu đề cột để giữ nguyên Header của file mẫu)
                worksheet.Cells["A2"].LoadFromCollection(exportData, false);


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
        public IActionResult ImportExcel(IFormFile fileExcel, string us)
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
                        // Bỏ cấu hình UseHeaderRow = true để tự quản lý dòng thủ công
                        var result = reader.AsDataSet();
                        var dataTable = result.Tables[0];

                        // Nếu file không có đủ ít nhất 3 dòng (Tiêu đề gộp, Cột tháng, Dữ liệu) thì bỏ qua
                        if (dataTable.Rows.Count < 3)
                            return Json(new { success = false, message = "File Excel không đúng định dạng" });

                        // Lấy dòng header chứa tháng năm (Dòng thứ 2 -> index = 1)
                        var headerRow = dataTable.Rows[0];

                        // Map cột với Tháng/Năm tương ứng. VD: Cột index 1 -> (Tháng 5, Năm 2026)
                        var columnMappings = new Dictionary<int, (int Thang, int Nam)>();

                        for (int i = 1; i < dataTable.Columns.Count; i++) // Bỏ qua cột 0 (Mã code)
                        {
                            string headerText = headerRow[i]?.ToString()!;
                            if (!string.IsNullOrEmpty(headerText))
                            {
                                columnMappings.Add(i, (DateTime.Parse(headerText).Month, DateTime.Parse(headerText).Year));
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
                                string maVatTu = row[0]?.ToString()!;

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

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
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
                sql.GET_DATA_FROM_SQL_TEST(sqlQuery);

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

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
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

                item.Tansuatgiaohang = row["Tansuatgiaohang"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Tansuatgiaohang"].ToString()) ? Convert.ToDouble(row["Tansuatgiaohang"]) : null;
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
                    item.DiemGoiHang = "CHƯA DÙNG";
                }
                else if (tile < 70)
                {
                    item.DiemGoiHang = "ĐỐI ỨNG GẤP";
                }
                else if (tile >= 70 && tile <= 100)
                {
                    item.DiemGoiHang = "GỌI HÀNG";
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

            var demsoluong = sQL.GET_DATA_FROM_SQL_TEST(@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
            LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO left join REQUEST as c on a.Code_Request = c.Code_Request
            WHERE Ngayphathanh >= '2026-07-01' AND a.Group_Code = 'PUR' and b.Lichgiao = 'NG' and (b.Anh_huong_SX <> 'No' or b.Anh_huong_SX is null) 
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

            var demsoluong = sQL.GET_DATA_FROM_SQL_TEST($@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
            LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO left join REQUEST as c on a.Code_Request = c.Code_Request
            WHERE Ngay_GHchinhthuc >= '{ngaythang}' AND a.Group_Code = 'PUR' 
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

                var lst = sql.GET_DATA_FROM_SQL_TEST(query);

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
                        worksheet.Cells[row, 8].Value = r["Tansuatgiaohang"] != DBNull.Value ? Convert.ToDouble(r["Tansuatgiaohang"]) : (object)"-";
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
    }
}

