using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Material;
using static PRJ_WAREHOUSE_BIVN.View_Models.Material.MaterialVM;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class MaterialController : BaseAuthController
    {
        private readonly IBaoGiaConfirmNameService _confirmNameService;
        private readonly INhomViTriService _nhomViTriService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IWebHostEnvironment _env;
        private readonly ISendMailService _sendMailService;
        public MaterialController(IBaoGiaConfirmNameService confirmNameService, INhomViTriService nhomViTriService, IBaoGiaService baoGiaService, IWebHostEnvironment env, ISendMailService sendMailService)
        {
            _confirmNameService = confirmNameService;
            _nhomViTriService = nhomViTriService;
            _baoGiaService = baoGiaService;
            _sendMailService = sendMailService;
            _env = env;
        }
        // MARK: Confirm Name actions use EF context directly
        public IActionResult Material()
        {
            return View();
        }
        public IActionResult Creat_Material()
        {
            return View();
        }
        [HttpPost]
        public JsonResult load_material(PARAS para)
        {
            List<PARAS> dt = MATERIA.material_process(para);
            dt = dt.GroupBy(x => x.Material_Code).Select(g => g.First()).ToList();
            return Json(dt);
        }
        // MARk: Màn hình xác nhận tên
        public async Task<IActionResult> ConfirmName()
        {
            // Determine role from query or default to UserPUR
            var role = (Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(role)) role = "UserPUR"; // UserShip | UserAcc | UserPUR
            ViewBag.Role = role;
            var vitris = await LoadNhomViTriDataAsync();
            var vm = new MaterialVM
            {
                vitris = vitris
            };
            return View(vm);
        }
        private async Task<List<ACC_NHOMVITRIDTO>> LoadNhomViTriDataAsync()
        {
            var nhomViTri = await _nhomViTriService.GetAllNhomViTriAsync();
            return nhomViTri.Data ?? new List<ACC_NHOMVITRIDTO>();
        }
        // Search confirm list
        [HttpPost]
        public async Task<IActionResult> SearchConfirmName([FromBody] ConfirmNameSearchRequest req)
        {
            var result = await _confirmNameService.SearchAsync(req.TenHang, req.SoDon, req.TrangThai, req.Section, req.pageIndex, req.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        // Save inline changes by role
        [HttpPost]
        public async Task<IActionResult> SaveConfirmName([FromBody] ConfirmNameSaveRequest req)
        {
            var result = await _confirmNameService.SaveConfirmNameAsync(req.Id, req.TenHaiQuan, req.MaHangNoiBo, req.Role, GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // Save in select row
        [HttpPost]
        public async Task<IActionResult> SaveSelectedConfirmName([FromBody] List<ConfirmNameDTO> reqs)
        {
            var role = (Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(role)) role = "UserShip"; // UserShip | UserAcc | UserPUR
            var result = await _confirmNameService.SaveConfirmNameListAsync(reqs, GetCurrentUserId(), role);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // Approve in select row
        [HttpPost]
        public async Task<IActionResult> ApproveSelectedConfirmName([FromBody] List<ConfirmNameDTO> reqs)
        {
            var role = (Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(role)) role = "UserPUR"; // UserShip | UserAcc | UserPUR
            var result = await _confirmNameService.ApproveConfirmNameListAsync(reqs, GetCurrentUserId(), role);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // Approve (agree) and update base request
        [HttpPost]
        public async Task<IActionResult> ApproveConfirmName([FromBody] ConfirmNameActionRequest req)
        {
            var result = await _confirmNameService.ApproveConfirmNameAsync(req.Id, GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }

        // Reject with reason
        [HttpPost]
        public async Task<IActionResult> RejectConfirmName([FromBody] ConfirmNameRejectRequest req)
        {
            var result = await _confirmNameService.RejectConfirmNameAsync(req.Id, req.LyDo ?? "", GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // tìm kiếm theo ID đơn báo giá 
        [HttpPost]
        public async Task<IActionResult> SearchID([FromBody] int id)
        {
            var result = await _baoGiaService.GetByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // Nhap bang excel
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            var role = "UserPUR";//(Request.Query["role"].ToString() ?? string.Empty).Trim();
            var item = new List<BaoGia_Confirm_Name_Quotation>();
            var hasErrors = false;
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // Dữ liệu bắt đầu từ dòng 4
                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "")
                    {
                        break; // Nếu cột 2 (trạng thái) trống, dừng đọc tiếp
                    }
                    if (ws.Cell(r, 3).GetString() == "")
                    {
                        ws.Cell(r, 25).SetValue("Số đơn yêu cầu không được để trống");
                        hasErrors = true;
                        continue;
                    }
                    var a = int.Parse(ws.Cell(r, 3).GetString());
                    switch (role)
                    {
                        case "UserShip":
                            var tenHaiQuan = ws.Cell(r, 22).GetString();
                            if (string.IsNullOrWhiteSpace(tenHaiQuan))
                            {
                                ws.Cell(r, 25).SetValue("Tên hải quan không được để trống");
                                hasErrors = true;
                                continue;
                            }
                            item.Add(new BaoGia_Confirm_Name_Quotation
                            {
                                ID = int.Parse(ws.Cell(r, 3).GetString()),
                                VCHR_TenHaiQuan = tenHaiQuan,
                                VCHR_UserShip = GetCurrentUserId(),
                                DTM_UserShip = DateTime.Now
                            });
                            break;
                        case "UserAcc":
                            var mahang = ws.Cell(r, 9).GetString();
                            if (string.IsNullOrWhiteSpace(mahang))
                            {
                                ws.Cell(r, 25).SetValue("Tên hải quan không được để trống");
                                hasErrors = true;
                                continue;
                            }
                            item.Add(new BaoGia_Confirm_Name_Quotation
                            {
                                ID = int.Parse(ws.Cell(r, 3).GetString()),
                                VCHR_MaHangNoiBo = mahang,
                                VCHR_UserAcc = GetCurrentUserId(),
                                DTM_UserAcc = DateTime.Now
                            });
                            break;
                        case "UserPUR":
                            var tenHaiQuanPUR = ws.Cell(r, 22).GetString();
                            var mahangPUR = ws.Cell(r, 9).GetString();
                            //if (string.IsNullOrWhiteSpace(tenHaiQuanPUR) || string.IsNullOrWhiteSpace(mahangPUR))
                            //{
                            //    ws.Cell(r, 25).SetValue("Tên hải quan và mã hàng nội bộ không được để trống");
                            //    hasErrors = true;
                            //    continue;
                            //}
                            item.Add(new BaoGia_Confirm_Name_Quotation
                            {
                                ID = int.Parse(ws.Cell(r, 3).GetString()),
                                VCHR_TenHaiQuan = tenHaiQuanPUR,
                                VCHR_MaHangNoiBo = mahangPUR,
                                VCHR_UserPUR = GetCurrentUserId(),
                                DTM_UserPUR = DateTime.Now
                            });
                            break;
                        default:
                            ws.Cell(r, 25).SetValue("Bạn không có quyền update file");
                            break;
                    }

                }
                if (hasErrors)
                {
                    // Trả về file Excel với lỗi
                    using var outStream = new MemoryStream();
                    workbook.SaveAs(outStream);
                    var bytes = outStream.ToArray();
                    var fileName = $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(bytes, contentType, fileName);
                }
                if (!item.Any() || item == null)
                {
                    return BadRequest("Không có dữ liệu hợp lệ để lưu");
                }
                await _confirmNameService.SaveFromFileAsync(item, GetCurrentUserId(), role);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }

            return Ok(item);
        }
        // Xuất file Excel table
        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ConfirmNameSearchRequest req)
        {
            try
            {

                var result = await _confirmNameService.SearchAsync(req.TenHang, req.SoDon, req.TrangThai, req.Section, req.pageIndex, req.pageSize);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if(result.Data.Data == null)
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }
                var items = result.Data.Data;
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = System.IO.Path.Combine(root, "template", "TemplateCofirmName.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateCofirmName.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 4;
                int idx = 1;
                foreach (var rq in items)
                {
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, 2).SetValue(rq.CHR_Status ?? "");
                    ws.Cell(row, 3).SetValue(rq.ID ?? 0);
                    ws.Cell(row, 4).SetValue(idx);
                    ws.Cell(row, 5).SetValue(rq.CHR_SectionCode ?? "");
                    ws.Cell(row, 6).SetValue(rq.CHR_SectionName ?? "");
                    ws.Cell(row, 7).SetValue(rq.CHR_Phanloai ?? "");
                    ws.Cell(row, 8).SetValue(rq.CHR_MaThietBi ?? "");
                    ws.Cell(row, 9).SetValue(rq.VCHR_MaHangNoiBo ?? "");
                    ws.Cell(row, 10).SetValue(rq.CHR_MaHangNCC ?? "");
                    ws.Cell(row, 11).SetValue(rq.VCHR_TenRecomment ?? "");
                    ws.Cell(row, 12).SetValue(rq.CHR_NameEN ?? "");
                    ws.Cell(row, 13).SetValue(rq.INT_SoLuong ?? "");
                    ws.Cell(row, 14).SetValue(rq.NVCHR_DonVi ?? "");
                    ws.Cell(row, 15).SetValue(rq.NVCHR_ChungLoai ?? "");
                    ws.Cell(row, 16).SetValue(rq.NVCHR_HinhDang ?? "");
                    ws.Cell(row, 17).SetValue(rq.NVCHR_ChatLieu ?? "");
                    ws.Cell(row, 18).SetValue(rq.NVCHR_ThanhPhan ?? "");
                    ws.Cell(row, 19).SetValue(rq.NVCHR_KichThuoc ?? "");
                    ws.Cell(row, 20).SetValue(rq.NVCHR_DongMay ?? "");
                    ws.Cell(row, 21).SetValue(rq.NVCHR_TinhNang ?? "");
                    ws.Cell(row, 22).SetValue(rq.VCHR_TenHaiQuan ?? "");
                    ws.Cell(row, 23).SetValue(rq.VCHR_UserShip ?? "");
                    ws.Cell(row, 24).SetValue(rq.VCHR_UserAcc ?? "");
                    row++;
                    idx++;
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"TableConfirmName_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }
    }
}
