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
        private readonly ITmUserService _tmUserService;
        private readonly IMaterialService _materialService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MaterialController> _logger;
        public MaterialController(IBaoGiaConfirmNameService confirmNameService, INhomViTriService nhomViTriService,
            IBaoGiaService baoGiaService, IWebHostEnvironment env, ISendMailService sendMailService, 
            ITmUserService tmUserService, IMaterialService materialService, IServiceScopeFactory serviceScopeFactory, ILogger<MaterialController> logger)
        {
            _confirmNameService = confirmNameService;
            _nhomViTriService = nhomViTriService;
            _baoGiaService = baoGiaService;
            _sendMailService = sendMailService;
            _logger = logger;
            _materialService = materialService;
            _tmUserService = tmUserService;
            _env = env;
            _materialService = materialService;
            _serviceScopeFactory = serviceScopeFactory;
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
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            //(Request.Query["role"].ToString() ?? string.Empty).Trim();
            if (!role.Success)
            {
                return BadRequest(role.Message);
            }
            ViewBag.Role = role.Data;
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

        [HttpPost]
        public async Task<IActionResult> Create_Material([FromBody] MATERIALDTO model)  
        {
            if (!ModelState.IsValid)
            {
                return View(model);  
            }

            var result = await _materialService.InsertMaterial(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Material created successfully.";
                return RedirectToAction("Material");  
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create material.");
                return View(model);
            }
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
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            // kiểm tra điều kiện  
            foreach (var req in reqs)
            {
                if (role.Data == "UserShip" && string.IsNullOrWhiteSpace(req.TenHaiQuan))
                {
                    return BadRequest("Tên hải quan không được để trống");
                }
                if ((role.Data == "UserAcc") && string.IsNullOrWhiteSpace(req.MaHangNoiBo))
                {
                    return BadRequest("Mã hàng nội bộ không được để trống");
                }
                var checkAsync = await _materialService.CheckMaHangExistsAsync(req.MaHangNoiBo);
                if (!checkAsync.Success)
                {
                    return BadRequest(checkAsync.Message);
                }
                if (checkAsync.Data)
                {
                    return BadRequest($"Mã hàng nội bộ '{req.MaHangNoiBo}' đã tồn tại trong hệ thống");
                }
            }
            var result = await _confirmNameService.SaveConfirmNameListAsync(reqs, GetCurrentUserId(), role.Data);
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
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var result = await _confirmNameService.ApproveConfirmNameListAsync(reqs, GetCurrentUserId(), role.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Success);
        }
        // Reject in select row (User ACC)
        [HttpPost]
        public async Task<IActionResult> RejectAccSelectedConfirmName([FromBody] List<ConfirmNameDTO> reqs)
        {
            var user = GetCurrentUserId();
            var reject = reqs.Select(d => d.LyDo).FirstOrDefault();
            var role = await _tmUserService.GetRoleAsync(user);
            var result = await _confirmNameService.RejectAccConfirmNameListAsync(reqs, user, role.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            _ = Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                        var listConfirm = new List<BaoGia_Confirm_Name_QuotationDTO>();
                        //PhuongThuy.VuThi@brother-bivn.com.vn;nguyenduy.khanh@brother-bivn.com.vn;nguyenthilan.huong2@brother-bivn.com.vn
                        // gửi mail thông báo có yêu cầu xác nhận tên mới 
                        var emailResult = await sendMailService.SendMailAsync(
                            "nguyenduy.khanh@brother-bivn.com.vn",
                            string.Empty,
                            21,
                            "http://172.26.248.62:8057/Material/ConfirmName",
                            true,
                            reject,
                            string.Empty,
                            user);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                    }
                }
            });
            return Ok(result.Success);
        }
        // Approve (agree) and update base request
        [HttpPost]
        public async Task<IActionResult> ApproveConfirmName([FromBody] ConfirmNameActionRequest req)
        {
            var user = GetCurrentUserId();
            var result = await _confirmNameService.ApproveConfirmNameAsync(req.Id, user);
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
            var user = GetCurrentUserId();
            var result = await _confirmNameService.RejectConfirmNameAsync(req.Id, req.LyDo ?? "", user);
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

            var roleAsync = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var role = roleAsync.Success ? roleAsync.Data : string.Empty;
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
                                ws.Cell(r, 25).SetValue("Mã hàng nội bộ không được để trống");
                                hasErrors = true;
                                continue;
                            }
                            // kiểm tra mã hàng đã tồn tại trong hệ thống chưa
                            var checkMaHang = await _materialService.CheckMaHangExistsAsync(mahang);
                            if (checkMaHang.Data)
                            {
                                ws.Cell(r, 25).SetValue($"Mã hàng nội bộ '{mahang}' đã tồn tại trong hệ thống");
                                hasErrors = true;
                                continue;
                            }
                            var checkList = item.Where(x => x.VCHR_MaHangNoiBo == mahang).ToList();
                            if (checkList.Any())
                            {
                                ws.Cell(r, 25).SetValue($"Mã hàng nội bộ '{mahang}' đã tồn tại trong file");
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
                            // kiểm tra mã hàng đã tồn tại trong hệ thống chưa
                            var checkMaHangPUR = await _materialService.CheckMaHangExistsAsync(mahangPUR);
                            if (checkMaHangPUR.Data)
                            {
                                ws.Cell(r, 25).SetValue($"Mã hàng nội bộ '{mahangPUR}' đã tồn tại trong hệ thống");
                                hasErrors = true;
                                continue;
                            }
                            var checkListPUR = item.Where(x => x.VCHR_MaHangNoiBo == mahangPUR).ToList();
                            if (checkListPUR.Any())
                            {
                                ws.Cell(r, 25).SetValue($"Mã hàng nội bộ '{mahangPUR}' đã tồn tại trong file");
                                hasErrors = true;
                                continue;
                            }
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
