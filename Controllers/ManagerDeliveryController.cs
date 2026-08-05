using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PRJ_WAREHOUSE_BIVN.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

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

    public class ManagerDeliveryController : Controller
    {
        public IActionResult ManageDelivery(int page = 1, string searchTerm = "", string reqMonth = "", string tab = "ngoai")
        {
            
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            sql.GET_DATA_FROM_SQL_TEST(@"UPDATE b
                        SET b.Anh_huong_SX = 'No'
                        FROM PE_THEODOITIENDO b
                        JOIN [COST_MANAGEMENT].[dbo].[PO] a ON a.PO_Detail_Id = b.Id_Detail_PO
                        WHERE a.Ngaygiaohangdukien IS NOT NULL 
                          AND b.Ngay_NCC_xacnhanGH IS NOT NULL
                          AND MONTH(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = MONTH(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                          AND YEAR(TRY_CAST(a.Ngaygiaohangdukien AS DATE)) = YEAR(TRY_CAST(b.Ngay_NCC_xacnhanGH AS DATE))
                          AND (b.Anh_huong_SX IS NULL OR b.Anh_huong_SX <> 'No');");

            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");
            var khoi = "";
            if(checkus == "PUR") { khoi = "AND Group_Code = 'PUR'";  };
            if(checkus == "GA") { khoi = "AND Group_Code = 'GA'";  };
            string query = $@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                     LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
                     WHERE Ngayphathanh >= '2026-07-01' {khoi} ORDER BY Ngayphathanh DESC";

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
                // Đối với DataTable, nên check thêm DBNull.Value để tránh lỗi null
                object valNgayGui = lst.Rows[i]["Ngay_gui_PO"];
                po.ngayguiPO = (valNgayGui != null && valNgayGui != DBNull.Value) ? Convert.ToDateTime(valNgayGui).ToString("yyyy-MM-dd") : "";

                object valNgayNcc = lst.Rows[i]["Ngay_NCC_xacnhanGH"];
                po.ngaynccxngiao = (valNgayNcc != null && valNgayNcc != DBNull.Value) ? Convert.ToDateTime(valNgayNcc).ToString("yyyy-MM-dd") : "";

                po.anhuongsx = lst.Rows[i]["Anh_huong_SX"].ToString();
                if(po.anhuongsx == "No")
                {
                    po.lichgiao = "OK";
                }
                else
                {
                    po.lichgiao = "NG";
                }
                po.trangthai = "";
                po.LuongvekhoKhonhap = lst.Rows[i]["LuongvekhoKhonhap"].ToString();
                po.Danhmuc = lst.Rows[i]["Danhmuc"].ToString();
                object valNgayGH = lst.Rows[i]["Ngay_GHchinhthuc"];
                po.Ngay_GHchinhthuc = (valNgayGH != null && valNgayGH != DBNull.Value)
                                      ? Convert.ToDateTime(valNgayGH).ToString("yyyy-MM-dd")
                                      : "";
                po.Gio_GH = lst.Rows[i]["Gio_GH"].ToString();
                po.Cua_GH = lst.Rows[i]["Cua_GH"].ToString();
                po.Cong_Nhanhang = lst.Rows[i]["Cong_Nhanhang"].ToString();
                po.Nguoi_Nhanhang = lst.Rows[i]["Nguoi_Nhanhang"].ToString();
                po.SL_Thucte = lst.Rows[i]["SL_Thucte"].ToString();
                po.So_DNTT = lst.Rows[i]["So_DNTT"].ToString();
                po.So_hoadon = lst.Rows[i]["So_hoadon"].ToString();

                listPo.Add(po);
            }

            // 1. Lọc theo Tab (Danh mục)
            // Lưu ý: Thay đổi chuỗi "Trong danh mục" khớp với dữ liệu thực tế của bạn
            if (tab == "trong")
                    {
                        listPo = listPo.Where(x => !string.IsNullOrEmpty(x.Danhmuc) && x.Danhmuc == "IN").ToList();
                    }
                    else // Mặc định là 'ngoai'
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
            if (!string.IsNullOrEmpty(reqMonth))
            {
                listPo = listPo.Where(x => {
                    // Parse an toàn ngày giao hàng để so sánh định dạng yyyy-MM
                    if (DateTime.TryParse(x.Ngayycgiao, out DateTime dt))
                    {
                        return dt.ToString("yyyy-MM") == reqMonth;
                    }
                    return false;
                }).ToList();
            }
            // 3. Phân trang
            int pageSize = 100;
            int totalRecords = listPo.Count; // Tổng số bản ghi sau khi đã lọc (Tab + Search)
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedList = listPo.OrderByDescending(x => x.PO_Detail_Id)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            // 4. Truyền dữ liệu sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentTab = tab;
            ViewBag.ReqMonth = reqMonth;

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
        public IActionResult ExportExcel(string searchTerm = "", string reqMonth = "", string tab = "ngoai")
        {
            SQL_Connect_DB20 sql = new SQL_Connect_DB20();

            // 1. Phân quyền theo nhóm User y hệt ManageDelivery
            var us = User.FindFirst("UserId")?.Value;
            var checkus = sql.ReturnString($"select [Group_Code] from [GROUP_MEMBER] where CHR_USERID = '{us}'");
            var khoi = "";
            if (checkus == "PUR") { khoi = "AND Group_Code = 'PUR'"; };
            if (checkus == "GA") { khoi = "AND Group_Code = 'GA'"; };

            string query = $@"SELECT * FROM [COST_MANAGEMENT].[dbo].[PO] as a 
                      LEFT JOIN PE_THEODOITIENDO as b ON a.PO_Detail_Id = b.Id_Detail_PO 
                      WHERE Ngayphathanh >= '2026-07-01' {khoi} 
                      ORDER BY Ngayphathanh DESC";

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

            // 3. Lọc theo từ khóa tìm kiếm
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

            // 4. Lọc theo tháng yêu cầu
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
                item.Nguoiupdate = "System"; // Thay bằng User đăng nhập
                string query = "";

                if (item.ID == 0) // Thêm mới
                {
                    query = @"INSERT INTO PE_Using (MaVatTu, Thang, Nam, Soluong, Ngayupdate, Nguoiupdate) 
                              VALUES (@MaVatTu, @Thang, @Nam, @Soluong, @Ngayupdate, @Nguoiupdate)";
                }
                else // Cập nhật
                {
                    query = @"UPDATE PE_Using 
                              SET MaVatTu=@MaVatTu, Thang=@Thang, Nam=@Nam, Soluong=@Soluong, Ngayupdate=@Ngayupdate, Nguoiupdate=@Nguoiupdate 
                              WHERE ID=@ID";
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
            string query = @"
                 SELECT a.Material_Code, b.* FROM [COST_MANAGEMENT].[dbo].MATERIAL as a left join Giaohang_Master as b on a.Material_Code = b.Mahang
                 where a.CHR_MaterialOutSide = 'IN' ORDER BY [Mahang] ASC";

            var lst = sql.GET_DATA_FROM_SQL_TEST(query);
            List<GiaoHangMasterViewModel> model = new List<GiaoHangMasterViewModel>();

            for (int i = 0; i < lst.Rows.Count; i++)
            {
                var row = lst.Rows[i];
                var item = new GiaoHangMasterViewModel();

                item.Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0;
                item.Mahang = row["Mahang"]?.ToString();
                item.Tenhang = row["Tenhang"]?.ToString();
                item.Vendor = row["Vendor"]?.ToString();
                item.Maker = row["Maker"]?.ToString();
                item.MOQ = row["MOQ"]?.ToString();

                item.Tansuatgiaohang = row["Tansuatgiaohang"] != DBNull.Value ? Convert.ToDouble(row["Tansuatgiaohang"]) : null;
                item.Leadtimegiaohang = row["Leadtimegiaohang"] != DBNull.Value ? Convert.ToDouble(row["Leadtimegiaohang"]) : null;
                item.Songaytonkhoantoan = row["songaytonkhoantoan"] != DBNull.Value ? Convert.ToDouble(row["songaytonkhoantoan"]) : null;
                item.Donvi = row["donvi"]?.ToString();
                item.Soluongtonkhoantoan = row["soluongtonkhoantoan"] != DBNull.Value ? Convert.ToDouble(row["soluongtonkhoantoan"]) : null;

                // Tính toán ví dụ TiLeTonKhoAnToan (Chuẩn 24 ngày = 100%)
                if (item.Songaytonkhoantoan.HasValue && item.Songaytonkhoantoan > 0)
                {
                    item.TiLeTonKhoAnToan = (item.Songaytonkhoantoan.Value / 24.0) * 100;
                }

                // --- DỮ LIỆU GIẢ LẬP / HOẶC THAY BẰNG DỮ LIỆU THẬT TỪ DB CỦA BẠN ---
                // item.UsingThangHienTai = ... ; 
                // item.StockHienTai = ... ;
                // item.SoNgaySuDungHienTai = ... ;
                // item.TiLeTonKhoHienTai = ... ;

                // LOGIC PHÂN LOẠI ĐIỂM GỌI HÀNG (Ví dụ theo % tồn kho hoặc dữ liệu hình ảnh)
                // Bạn có thể gán trực tiếp theo điều kiện nghiệp vụ:
                double tile = item.TiLeTonKhoHienTai ?? 100;
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

    }
}

