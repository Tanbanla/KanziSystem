using ClosedXML.Excel;
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
        private readonly IBaoGiaService _baoGiaService;
        private readonly IWebHostEnvironment _env;
        private readonly ISendMailService _sendMailService;
        private readonly ITmUserService _tmUserService;
        private readonly IMaterialService _materialService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MaterialController> _logger;
        private readonly IDepartmentService _deparmentService;
        private readonly IConfiguration _configuration;
        public MaterialController(IBaoGiaConfirmNameService confirmNameService, IDepartmentService deparmentService,
            IBaoGiaService baoGiaService, IWebHostEnvironment env, ISendMailService sendMailService, IConfiguration configuration,
            ITmUserService tmUserService, IMaterialService materialService, IServiceScopeFactory serviceScopeFactory, ILogger<MaterialController> logger)
        {
            _confirmNameService = confirmNameService;
            _baoGiaService = baoGiaService;
            _sendMailService = sendMailService;
            _logger = logger;
            _materialService = materialService;
            _tmUserService = tmUserService;
            _env = env;
            _materialService = materialService;
            _serviceScopeFactory = serviceScopeFactory;
            _deparmentService = deparmentService;
            _configuration = configuration;
        }
        // MARK: Quản lý linh kiện
        public IActionResult Material()
        {
            return View();
        }
        // Search thông tin màn hình quản lý
        [HttpPost]
        public async Task<IActionResult> SearchMaterialView([FromBody] SearchMaterialVM search)
        {
            if(search == null)
            {
                return BadRequest("Invalid search parameters.");
            }
            try
            {
                var result = await _materialService.SearchDateByMaterialViewAsync(search);
                if(!result.Success)
                {
                    return BadRequest("Bug: "+result.Message);
                }
                return Ok(result.Data);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error searching material view.");
                return StatusCode(500, "Internal server error. Bug: "+ex.Message);
            }
        }
        // Xuất file excel Material View
        [HttpPost]
        public async Task<IActionResult> ExportMaterialViewToExcel([FromBody] SearchMaterialVM search)
        {
            if (search == null)
            {
                return BadRequest("Invalid search parameters.");
            }
            try
            {
                search.pageIndex = 0;
                search.pageSize = 0;
                var result = await _materialService.ExportMaterialViewToExcelAsync(search);
                if (!result.Success)
                {
                    return BadRequest("Bug: " + result.Message);
                }

                var (fileBytes, fileName, contentType) = result.Data;

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting material view to Excel.");
                return StatusCode(500, "Internal server error. Bug: " + ex.Message);
            }
        }
        // Delete Material
        [HttpGet]
        public async Task<IActionResult> DeleteMaterial(string codeMaterial)
        {
            if (string.IsNullOrEmpty(codeMaterial))
            {
                return BadRequest("Invalid material code.");
            }

            var result = await _materialService.DeleteMaterialAsync(codeMaterial);
            if (!result.Success)
            {
                return BadRequest("Bug: " + result.Message);
            }

            return Ok(result.Data);
        }
        // Update Material
        [HttpPost]
        public async Task<IActionResult> UpdateMaterial([FromBody] MATERIALDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state.");
            }
            var result = await _materialService.UpdateMaterialAsync(model);
            if (!result.Success)
            {
                return BadRequest("Bug: " + result.Message);
            }
            return Ok(result.Data);
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
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var madons = await LoadMadonAsync(13);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            if (!role.Success)
            {
                return BadRequest(role.Message);
            }
            if (role.Data == null || (role.Data != "UserPUR" && role.Data != "UserShip" && role.Data != "UserAcc"))
            {
                ViewBag.Role = "User";
            }
            else
            {
                ViewBag.Role = role.Data;
            }
            var vitris = await LoadNhomViTriDataAsync();
            var vm = new MaterialVM
            {
                vitris = vitris,
                confirmedCodes = await LoadConfirmedCodesAsync(),
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachMaDon = madons ?? new List<string>()
            };
            return View(vm);
        }

        public async Task<IActionResult> ConfirmNameV2()
        {
            // Determine role from query or default to UserPUR
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var madons = await LoadMadonAsync(13);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            if (!role.Success)
            {
                return BadRequest(role.Message);
            }
            if (role.Data == null || (role.Data != "UserPUR" && role.Data != "UserShip" && role.Data != "UserAcc"))
            {
                ViewBag.Role = "User";
            }
            else
            {
                ViewBag.Role = role.Data;
            }
            var vitris = await LoadNhomViTriDataAsync();
            var vm = new MaterialVM
            {
                vitris = vitris,
                confirmedCodes = await LoadConfirmedCodesAsync(),
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachMaDon = madons ?? new List<string>()
            };
            return View(vm);
        }



        private async Task<List<DEPARTMENTDTO>> LoadNhomViTriDataAsync()
        {
            var nhomViTri = await _deparmentService.GetNhomViTriByDepartmentIdAsync(GetCurrentUserId());
            return nhomViTri.Data ?? new List<DEPARTMENTDTO>();
        }
        private async Task<List<dynamic>> LoadConfirmedCodesAsync()
        {
            var result = await _confirmNameService.ExportCodeConfirmedAsync();
            return result.Success && result.Data != null ? result.Data : new List<dynamic>();
        }
        private async Task<List<string>> LoadMadonAsync(int step)
        {
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "", step);
            return madons.Data ?? new List<string>();
        }

        // Search confirm list
        [HttpPost]
        public async Task<IActionResult> SearchConfirmName([FromBody] ConfirmNameSearchRequest req)
        {
            req ??= new ConfirmNameSearchRequest();
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var user = (string.IsNullOrEmpty(role.Data)) ? GetCurrentUserId() : "";
            var result = await _confirmNameService.SearchAsync(req, user, role.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        // Count confirm name by role
        [HttpPost]
        public async Task<IActionResult> CountConfirmNameByRole([FromBody] ConfirmNameSearchRequest req)
        {
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var user = (string.IsNullOrEmpty(role.Data)) ? GetCurrentUserId() : "";
            req.TrangThai = "";
            var result = await _confirmNameService.GetCountCofirmNames(req, user, role.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetConfirmNameHistory(int confirmId)
        {
            var result = await _confirmNameService.GetConfirmNameHistoryAsync(confirmId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data ?? new List<ConfirmNameHistoryDTO>());
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

        [HttpPost]
        public async Task<IActionResult> ImportMaterials([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file được tải lên");
                }

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                {
                    return BadRequest("Chỉ hỗ trợ file excel");
                }

                try
                {
                    var details = new List<MATERIALDTO>();

                    using (var stream = file.OpenReadStream())
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        if (worksheet == null)
                        {
                            return BadRequest("Không tìm thấy worksheet trong file");
                        }


                        var rows = worksheet.RowsUsed().Skip(4);
                        if (!rows.Any())
                        {
                            return Json(new { success = false, message = "File trống không có dữ liệu" });
                        }

                        foreach (var row in rows)
                        {
                            var Material_Code = row.Cell(6).GetString().Trim();
                            var Material_Name_VN = row.Cell(7).GetString().Trim();
                            var Material_Name_EN = row.Cell(8).GetString().Trim();
                            var Account_Code = row.Cell(10).GetString().Trim();
                            var Account_Name_EN = row.Cell(11).GetString().Trim();
                            var Account_Name_VN = row.Cell(12).GetString().Trim();
                            var Unit = row.Cell(13).GetString().Trim();
                            var Currency = row.Cell(14).GetString().Trim();
                            var Group_Code = row.Cell(5).GetString().Trim();
                            var Category_VN = row.Cell(15).GetString().Trim();
                            var Shape = row.Cell(16).GetString().Trim();
                            var Material = row.Cell(17).GetString().Trim();
                            var Composition = row.Cell(18).GetString().Trim();
                            var Dimension = row.Cell(19).GetString().Trim();
                            var UsedFor = row.Cell(20).GetString().Trim();
                            var Purpose = row.Cell(21).GetString().Trim();

                            if (string.IsNullOrEmpty(Material_Code) || string.IsNullOrEmpty(Material_Name_VN))
                            {
                                continue;
                            }

                            details.Add(new MATERIALDTO
                            {
                                Material_Code = Material_Code,
                                Material_Name_VN = Material_Name_VN,
                                Material_Name_EN = Material_Name_EN,
                                Account_Code = Account_Code,
                                Account_Name_VN = Account_Name_VN,
                                Account_Name_EN = Account_Name_EN,
                                Unit = Unit,
                                Currency = Currency,
                                Group_Code = Group_Code,
                                Category_VN = Category_VN,
                                Shape = Shape,
                                Material = Material,
                                Composition = Composition,
                                Dimension = Dimension,
                                UsedFor = UsedFor,
                                Purpose = Purpose,
                            });
                        }
                    }

                    var result = await _materialService.UpdateListThongTin(details);
                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }
                    return Ok(result.Data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing quote details from Excel");
                    return Json(new { success = false, message = "An error occurred during import. Check the file format." });
                }
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
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
        public async Task<IActionResult> RejectShipSelectedConfirmName([FromBody] List<ConfirmNameDTO> reqs)
        {
            var user = GetCurrentUserId();
            var reject = reqs.Select(d => d.LyDo).FirstOrDefault();
            var role = await _tmUserService.GetRoleAsync(user);
            var result = await _confirmNameService.RejectShipConfirmNameListAsync(reqs, user, role.Data);
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
                        // gửi mail thông báo có yêu cầu xác nhận tên mới 
                        var emailResult = await sendMailService.SendMailAsync(
                            "nguyenduy.khanh@brother-bivn.com.vn",
                            string.Empty,
                            20,
                            "Material/ConfirmName",
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
            var user = GetCurrentUserId();
            var roleAsync = await _tmUserService.GetRoleAsync(user);
            var role = roleAsync.Success ? roleAsync.Data : string.Empty;
            var itemOK = new List<ConfirmNameInputExcel>();
            var itemNG = new List<ConfirmNameDTO>();
            var listDifferent = new List<ConfirmNameInputExcel>();
            var listUpdateRequest = new List<BaoGia_Request_of_QuotationDTO>();
            var listUpdateUserPur = new List<ConfirmNameInputExcel>();
            var listCheck = new List<int>();
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
                    if (ws.Cell(r, 3).GetString() == "")
                    {
                        break;
                    }
                    listCheck.Add(int.Parse(ws.Cell(r, 3).GetString()));
                    switch (role)
                    {
                        case "UserShip":
                            var tenHaiQuan = ws.Cell(r, 25).GetString();
                            var tenRecomment = ws.Cell(r, 13).GetString();
                            bool bitReturn = ws.Cell(r, 27).GetString().Trim().ToUpper() == "O";
                            var reasonReturn = ws.Cell(r, 28).GetString().Trim();
                            if (string.IsNullOrEmpty(reasonReturn) && bitReturn)
                            {
                                ws.Cell(r, 29).SetValue("Vui lòng nhập lý do trả lại");
                                hasErrors = true;
                                continue;
                            }
                            if (bitReturn)
                            {
                                itemNG.Add(new ConfirmNameDTO
                                {
                                    Id = int.Parse(ws.Cell(r, 3).GetString()),
                                    pheDuyet = bitReturn,
                                    LyDo = reasonReturn,
                                    PicShip = ws.Cell(r, 26).GetString()
                                });
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(tenHaiQuan))
                                {
                                    ws.Cell(r, 29).SetValue("Tên hải quan không được để trống");
                                    hasErrors = true;
                                    continue;
                                }
                                if (tenHaiQuan.Trim().ToUpper() != tenRecomment.Trim().ToUpper())
                                {
                                    listDifferent.Add(new ConfirmNameInputExcel
                                    {
                                        ID = int.Parse(ws.Cell(r, 3).GetString()),
                                        VCHR_TenHaiQuan = tenHaiQuan,
                                        DTM_UserShip = DateTime.Now,
                                        VCHR_UserShip = ws.Cell(r, 26).GetString()
                                    });
                                }
                                else
                                {
                                    itemOK.Add(new ConfirmNameInputExcel
                                    {
                                        ID = int.Parse(ws.Cell(r, 3).GetString()),
                                        VCHR_TenHaiQuan = tenHaiQuan,
                                        //VCHR_UserShip = GetCurrentUserId(),
                                        DTM_UserShip = DateTime.Now,
                                        VCHR_UserShip = ws.Cell(r, 26).GetString()
                                    });
                                }
                            }
                            break;
                        case "UserPUR":
                            var itemRequestPur = new ConfirmNameInputExcel
                            {
                                ID = int.Parse(ws.Cell(r, 3).GetString()),
                                CHR_NameEN = ws.Cell(r, 14).GetString(),
                                VCHR_TenRecomment = ws.Cell(r, 13).GetString(),
                                LinkQ = ws.Cell(r, 24).GetString(),
                            };
                            listUpdateUserPur.Add(itemRequestPur);
                            break;
                        default:
                            var itemRequest = new BaoGia_Request_of_QuotationDTO
                            {
                                ID = int.Parse(ws.Cell(r, 3).GetString()),
                                CHR_MaThietBi = ws.Cell(r, 9).GetString(),
                                CHR_MaHangNCC = ws.Cell(r, 11).GetString(),
                                NVCHR_HinhDang = ws.Cell(r, 18).GetString(),
                                NVCHR_ChatLieu = ws.Cell(r, 19).GetString(),
                                NVCHR_ThanhPhan = ws.Cell(r, 20).GetString(),
                                NVCHR_KichThuoc = ws.Cell(r, 21).GetString(),
                                NVCHR_DongMay = ws.Cell(r, 22).GetString(),
                                NVCHR_TinhNang = ws.Cell(r, 23).GetString()
                            };
                            listUpdateRequest.Add(itemRequest);
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
                if (listCheck.Count() != listCheck.Distinct().Count())
                {
                    return BadRequest("File có dữ liệu trùng lặp, vui lòng kiểm tra lại");
                }
                if (listCheck.Any())
                {
                    var checkResult = await _confirmNameService.CheckConfirmNameDoneAsync(listCheck);
                    if (!checkResult.Success)
                    {
                        return BadRequest("Bug check confirmName: " + checkResult.Message);
                    }
                    if(checkResult.Data.Count != 0)
                    {
                        var duplicateIds = string.Join(", ", checkResult.Data);
                        return BadRequest($"Các ID sau đã được xác nhận, vui lòng kiểm tra lại các số đơn: {duplicateIds}");
                    }
                }
                if (role == "UserShip")
                {
                    // gửi mail thông báo đã hoàn thành xác nhận tên đến PIC PUR
                    if (itemOK.Any())
                    {
                        // Lưu dữ liệu hợp lệ vào database
                        var rp =  await _confirmNameService.SaveFromFileAsync(itemOK, user, role);
                        if (!rp.Success || !rp.Data)
                        {
                            return BadRequest("Bug insert confirmName: "+ (rp.Message ?? "Unknown error"));
                        }

                        var listIdSendMail = itemOK.Select(d => d.ID).ToList();

                        // Send Mail PIC khi đơn hoàn thành xác nhận tên hải quan
                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    var listDone = await _confirmNameService.SearchSendMailConfirmNameAsync(listIdSendMail);
                                    if (!listDone.Success)
                                    {
                                        _logger.LogError("Lỗi khi kiểm tra đơn hàng đã được xác nhận: " + listDone.Message);
                                        return;
                                    }

                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                                    foreach (var item in listDone.Data)
                                    {
                                        // gửi mail thông báo đơn đã hoàn thành xác nhận tên hải quan
                                        var emailResult = await sendMailService.SendMailAsync(
                                            item.UserCreate + "@brothergroup.net",
                                            string.Empty,
                                            18,
                                            "SelectQuote/SelectQuoteSection",
                                            true,
                                            item.Section,
                                            item.MaDon,
                                            item.UserCreate);
                                    }

                                    // gửi mail thông báo có yêu cầu xác nhận tên mới 
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                                }
                            }
                        });
                    }
                    // Nếu có dữ liệu bị trả lại gửi đến PIC phòng ban để chỉnh sửa lại thông tin
                    if (itemNG.Any())
                    {
                        var rp = await _confirmNameService.RejectConfirmNameListAsync(itemNG, user, role);
                        if (!rp.Success || !rp.Data)
                        {
                            return BadRequest("Bug insert confirmName: " + (rp.Message ?? "Unknown error"));
                        }

                        var listIdSendMail = itemNG.Select(d => d.Id).ToList();
                        // Send Mail PIC phụ trách xác nhận tên mới (User Ship) khi có yêu cầu trả lại
                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {

                                    var listPIC = await _confirmNameService.SearchSendMailConfirmNameAsync(listIdSendMail);
                                    if (!listPIC.Success)
                                    {
                                        _logger.LogError("Lỗi khi kiểm tra đơn hàng đã được xác nhận: " + listPIC.Message);
                                        return;
                                    }

                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                                    // lọc trùng
                                    var distinctPIC = listPIC.Data.GroupBy(x => x.UserCreate).Select(g => g.First()).ToList();

                                    foreach (var item in distinctPIC)
                                    {
                                        var lydoNG = itemNG.FirstOrDefault(d => d.Id == item.ID)?.LyDo ?? string.Empty;
                                        // gửi mail thông báo chỉnh sửa thông tin xác nhận tên mới
                                        var emailResult = await sendMailService.SendMailAsync(
                                            item.UserCreate + "@brothergroup.net",
                                            string.Empty,
                                            20,
                                            "Material/ConfirmName",
                                            true,
                                            lydoNG ?? string.Empty,
                                            item.MaDon,
                                            item.UserCreate);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                                }
                            }
                        });
                    }
                    // với các dữ liệu tên bị lệch
                    if (listDifferent.Count() > 0)
                    {
                       var rp =  await _confirmNameService.UpdateRequestForPICPURAsync(listDifferent, user);
                        if (!rp.Success || !rp.Data)
                        {
                            return BadRequest("Bug insert confirmName: " + (rp.Message ?? "Unknown error"));
                        }

                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                    var approverService = scope.ServiceProvider.GetRequiredService<IMasterApproverSendMailService>();

                                    //danh sach PIC
                                    var result = await approverService.GetApproverByStepAndSectionAsync(4, "3110");
                                    if (!result.Success)
                                    {
                                        _logger.LogError("Không lấy được thông tin PIC phụ trách: " + result.Message);
                                    }
                                    var dataPic = result.Data;
                                    string emailList = string.Join("; ", dataPic.Select(x => x.CHR_UserAdid + "@brothergroup.net"));

                                    // gửi mail thông báo có yêu cầu xác nhận tên mới
                                    var emailResult = await sendMailService.SendMailAsync(
                                        emailList,
                                        string.Empty,
                                        21,
                                        "Material/ConfirmName",
                                        true,
                                        "",
                                        "",
                                        "");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                                }
                            }
                        });
                    }
                }

                // Lưu thông tin cập nhật vào request với role UserPUR và User
                if(role == "UserPUR" )
                {
                    if (listUpdateUserPur.Any())
                    {
                        var rp = await _confirmNameService.UpdateNameHQRolePICPURAsync(listUpdateUserPur, user);
                        if (!rp.Success || !rp.Data)
                        {
                            return BadRequest("Bug insert confirmName: " + (rp.Message ?? "Unknown error"));
                        }

                        var listIdSendMail = listUpdateUserPur.Select(d => d.ID).ToList();
                        // Send Mail PIC khi đơn hoàn thành xác nhận tên hải quan
                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    var listDone = await _confirmNameService.SearchSendMailConfirmNameAsync(listIdSendMail);
                                    if (!listDone.Success)
                                    {
                                        _logger.LogError("Lỗi khi kiểm tra đơn hàng đã được xác nhận: " + listDone.Message);
                                        return;
                                    }

                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                                    foreach (var item in listDone.Data)
                                    {
                                        // gửi mail thông báo đơn đã hoàn thành xác nhận tên hải quan
                                        var emailResult = await sendMailService.SendMailAsync(
                                            item.UserCreate + "@brothergroup.net",
                                            string.Empty,
                                            18,
                                            "SelectQuote/SelectQuoteSection",
                                            true,
                                            item.Section,
                                            item.MaDon,
                                            item.UserCreate);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                                }
                            }
                        });

                    }
                }
                else
                {
                    if (listUpdateRequest.Any())
                    {
                        var rp = await _confirmNameService.UpdateRequestFromFileAsync(listUpdateRequest, user);
                        if (!rp.Success || !rp.Data)
                        {
                            return BadRequest("Bug insert confirmName: " + (rp.Message ?? "Unknown error"));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }

            return Ok(itemOK);
        }
        // Xuất file Excel table
        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ConfirmNameSearchRequest req)
        {
            try
            {
                var roleAsync = await _tmUserService.GetRoleAsync(GetCurrentUserId());
                var role = roleAsync.Success ? roleAsync.Data : string.Empty;
                var user = (string.IsNullOrEmpty(role)) ? GetCurrentUserId() : "";
                var result = await _confirmNameService.ExportConfirmedMaterialNamesAsync(req.TenHang, req.SoDon, req.TrangThai, req.Section, role, user);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if (result.Data == null)
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }

                // hàm hiển thị trạng thái theo role
                static string GetStatusByRole(string CHR_Status, string CHR_StatusShip, string CHR_StatusAcc, string role)
                {
                    return role switch
                    {
                        "UserPUR" => CHR_StatusShip == "Confirming" ? "Chờ xác nhận tên - Ship" :
                            CHR_StatusAcc == "Confirming" ? "Chờ xác nhận tên - Phòng yêu cầu" :
                            CHR_Status == "Confirming" ? "Chờ xác nhận tên - Pur " : CHR_Status == "Confirmed" ? "Hoàn thành" : "Không xác định",
                        "UserShip" => CHR_StatusShip == "Confirming" ? "Chờ xác nhận tên - Ship" : CHR_StatusShip == "Confirmed" ? "Hoàn thành" : "Trả lại",
                        _=> CHR_StatusAcc == "Confirming" ? "Chờ bổ sung thông tin - phòng ban" : CHR_StatusAcc == "Confirmed" ? "Hoàn thành" : "",
                    };
                }

                var items = result.Data;
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
                    var maHangNb = rq.VCHR_MaHangNoiBo ?? (rq.CHR_MaHangNoiBo ?? "");
                    var a = GetStatusByRole(rq.CHR_Status, rq.CHR_StatusShip, rq.CHR_StatusACC, role);
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, 1).SetValue(rq.CHR_CreateBy ?? "");
                    ws.Cell(row, 2).SetValue(GetStatusByRole(rq.CHR_Status, rq.CHR_StatusShip, rq.CHR_StatusACC, role));
                    ws.Cell(row, 3).SetValue(rq.ID ?? 0);
                    ws.Cell(row, 4).SetValue(idx);
                    ws.Cell(row, 5).SetValue(rq.CHR_MaDon ?? "");
                    ws.Cell(row, 6).SetValue(rq.CHR_SectionCode ?? "");
                    ws.Cell(row, 7).SetValue(rq.CHR_SectionName ?? "");
                    ws.Cell(row, 8).SetValue(rq.CHR_Phanloai ?? "");
                    ws.Cell(row, 9).SetValue(rq.CHR_MaThietBi ?? "");
                    ws.Cell(row, 10).SetValue(maHangNb);
                    ws.Cell(row, 11).SetValue(rq.maHangNcc ?? "");
                    ws.Cell(row, 12).SetValue(rq.CHR_CodeNCC + " - " + rq.ShortName ?? "");
                    ws.Cell(row, 13).SetValue(rq.NVCHR_TenHangHQ ?? "");
                    ws.Cell(row, 14).SetValue(rq.NameENVendor ?? rq.NameEN ?? "");
                    ws.Cell(row, 15).SetValue(rq.INT_SoLuong ?? "");
                    ws.Cell(row, 16).SetValue(rq.NVCHR_DonVi ?? "");
                    ws.Cell(row, 17).SetValue(rq.NVCHR_ChungLoai ?? "");
                    ws.Cell(row, 18).SetValue(rq.NVCHR_HinhDang ?? "");
                    ws.Cell(row, 19).SetValue(rq.NVCHR_ChatLieu ?? "");
                    ws.Cell(row, 20).SetValue(rq.NVCHR_ThanhPhan ?? "");
                    ws.Cell(row, 21).SetValue(rq.NVCHR_KichThuoc ?? "");
                    ws.Cell(row, 22).SetValue(rq.NVCHR_DongMay ?? "");
                    ws.Cell(row, 23).SetValue(rq.NVCHR_TinhNang ?? "");
                    ws.Cell(row, 24).SetValue(rq.NVCHR_File ?? "");
                    ws.Cell(row, 25).SetValue(rq.VCHR_TenHaiQuan ?? "");
                    if (role == "UserPUR")
                    {
                        if (rq.VCHR_TenHaiQuan != rq.VCHR_TenRecomment)
                        {
                            ws.Cell(row, 25).Style.Fill.BackgroundColor = XLColor.DarkPink;
                        }
                    }
                    ws.Cell(row, 26).SetValue(rq.VCHR_UserShip ?? (rq.VCHR_UpdateBy ?? ""));

                    // check tra lai
                    bool isReturn = rq.CHR_StatusShip == "Rejected";
                    ws.Cell(row, 27).SetValue(isReturn ? "O" : "");
                    ws.Cell(row, 28).SetValue(isReturn ? rq.NVCHR_LyDo ?? "" : "");
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
        // Xuất các mã hàng đã được xác nhận thành file Excel
        [HttpPost]
        public async Task<IActionResult> ExportCodeCofirmed()
        {
            try
            {
                var data = await _confirmNameService.ExportCodeConfirmedAsync();
                if (!data.Success)
                {
                    return BadRequest("Error retrieving data: " + data.Message);
                }
                if (data.Data == null) return BadRequest("Not data");
                var items = data.Data;
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = System.IO.Path.Combine(root, "template", "ConfirmedCodeMaterial.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: ConfirmedCodeMaterial.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                int row = 2;
                int idx = 1;
                foreach (var rq in items)
                {
                    ws.Cell(row, 1).SetValue(idx);
                    ws.Cell(row, 2).SetValue(rq.Material_Code);
                    ws.Cell(row, 3).SetValue(rq.Material_Name_VN);
                    idx++;
                    row++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"ConfirmedCodeMaterial_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
