using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.ApprovalQuote;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Text.RegularExpressions;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ApprovalQuoteController : BaseAuthController
    {
        private readonly ILogger<ApprovalQuoteController> _logger;
        private readonly IHistoryApproverServive _historyApproverServive;
        private readonly IMaterialService _materialService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly ISendMailService _sendMailService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDepartmentService _deparmentService;
        private readonly IWebHostEnvironment _env;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IConfiguration _configuration;
        public ApprovalQuoteController(ILogger<ApprovalQuoteController> logger, IConfiguration configuration,
            IHistoryApproverServive historyApproverServive, IMaterialService materialService, IMasterApproverSendMailService approverService,
            IBaoGiaService baoGiaService, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStatusService baoGiaStatusService, IDepartmentService departmentService
            , IBaoGiaStepService baoGiaStepService, ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment env)
        {
            _logger = logger;
            _historyApproverServive = historyApproverServive;
            _materialService = materialService;
            _env = env;
            _baoGiaService = baoGiaService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaStepService = baoGiaStepService;
            _sendMailService = sendMailService;
            _serviceScopeFactory = serviceScopeFactory;
            _deparmentService = departmentService;
            _approverService = approverService;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var nhomViTriList = await GetNhomViTriList();
            var materialList = await GetMaterialList();
            var soDonList = await GetSoDonList();
            var statusBaoGiaList = await GetStatusBaoGiaList();
            var stepBaoGiaList = await GetStepBaoGiaList();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";

            var vm = new ApprovalQuoteViewModel
            {
                listNhomVitri = nhomViTriList,
                listMaterial = materialList,
                listSoDon = soDonList,
                listStatusBaoGia = statusBaoGiaList,
                listStepBaoGia = stepBaoGiaList
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> GetListApprovel([FromBody] SearchApprovalModel sr)
        {
            var result = await _approverService.GetApproverByStepAndSectionAsync(sr.Step ?? 3, sr.SectionCost ?? "");
            if (!result.Success)
            {
                return BadRequest("Error list Approver: " + result.Message);
            }
            return Ok(result.Data);
        }
        // Lay thong tin phong ban
        public async Task<List<DEPARTMENTDTO>> GetNhomViTriList()
        {
            var result = await _deparmentService.GetNhomViTriByDepartmentIdAsync(GetCurrentUserId() ?? "");
            return result.Data;
        }
        // Lay thong tin hang hoa
        public async Task<List<MATERIALDTO>> GetMaterialList()
        {
            var result = await _materialService.SearchAsync("", "", "", 1, 1000);
            return result.Data;
        }
        // Lay so don
        public async Task<List<string>> GetSoDonList()
        {
            //var result = await _baoGiaService.GetListMaDonBGAsync();
            var adid = GetCurrentUserId() ?? string.Empty;
            var result = await _baoGiaService.GetMaDonByAdidAsync(adid, 6);
            return result.Data;
        }
        // Lay status bao gia
        public async Task<List<BaoGia_StatusDTO>> GetStatusBaoGiaList()
        {
            var result = await _baoGiaStatusService.GetAllAsync();
            return (List<BaoGia_StatusDTO>)result.Data;
        }
        // Lay step bao gia
        public async Task<List<BaoGia_StepDTO>> GetStepBaoGiaList()
        {
            var result = await _baoGiaStepService.GetStepsApproverAsync();
            return (List<BaoGia_StepDTO>)result.Data;
        }
        // Search đơn phê duyệt báo giá
        [HttpPost]
        public async Task<IActionResult> SearchApprovalQuote([FromBody] ApprovalQuoteSearchViewModel searchModel)
        {
            try
            {
                var adid = GetCurrentUserId() ?? string.Empty;
                var result = await _historyApproverServive.GetListApprover(
                    adid, searchModel.SoDon, searchModel.MaHang, searchModel.Section, searchModel.StatusApprover
                );
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Lưu thông tin phê duyệt báo giá
        [HttpPost]
        public async Task<IActionResult> SaveApprovalQuote([FromBody] List<BaoGia_History_Approver_of_QuotationDTO> saveModel)
        {
            try
            {
                var adid = GetCurrentUserId() ?? string.Empty;
                var result = await _historyApproverServive.AddHistoryListAsync(saveModel);
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Cập nhật thông tin đơn báo giá
        // case OK
        [HttpPost]
        public async Task<IActionResult> UpdateQuotationOK([FromBody] List<BaoGia_Request_of_QuotationDTO> updateModel)
        {
            try
            {
                var result = await _baoGiaService.CapNhatDanhSachBGAsync(updateModel);
                var currentUserId = GetCurrentUserId();
                if (result.Success)
                {
                    var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                    // Luu lich su phe duyet
                    var approverHistories = insertedList.Select(b => new BaoGia_History_Approver_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        ID_BaoGiaStep = b.ID_StepBaoGia - 1 ?? 0,
                        CHR_UserSendApprover = currentUserId ?? string.Empty,
                        DTM_UserSendApprover = DateTime.Now,
                        CHR_UserApprover = currentUserId ?? string.Empty,
                        DTM_UserApprover = DateTime.Now,
                        CHR_StatusFlag = "1",
                        BIT_SendMail = false,
                        NVCHR_ReturnReason = null,
                        BIT_Return = false,
                        CHR_SectionCodeSend = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameSend = GetCurrentUserDepartment() ?? string.Empty,
                        CHR_SectionCodeApprover = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameApprover = GetCurrentUserDepartment() ?? string.Empty
                    }).ToList();
                    if (approverHistories.Any())
                    {
                        await _historyApproverServive.AddHistoryListAsync(approverHistories);
                    }
                    // Luu lich su thay doi trang thai bao gia
                    var histories = insertedList.Select(b => new BaoGia_History_Request_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        CHR_MaDon = b.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = currentUserId ?? string.Empty,
                        NVCHR_UpdateName = currentUserId ?? string.Empty,
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = null,
                        CHR_OldData = null,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(b),
                        NVCHR_LyDo = b.NVCHR_LyDo,
                        CHR_ActionType = StatusOld(b.ID_StepBaoGia)
                    }).ToList();

                    if (histories.Any())
                    {
                        await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                    }
                    // Gui mail thông báo phê duyệt báo giá
                    var SectionApporve = insertedList
                     .DistinctBy(l => new { l.CHR_MaDon, l.CHR_SectionCode })
                     .Select(l => (l.CHR_SectionCode, l.CHR_SectionName, l.CHR_MaDon, l.CHR_Gap, l.ID_StepBaoGia, l.CHR_UserApproval))
                     .ToList();
                    if (SectionApporve != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                    foreach (var item in SectionApporve)
                                    {
                                        if (item.ID_StepBaoGia == 4)
                                        {
                                            await sendMailService.SendMailToRequesterAsync(item.CHR_MaDon ?? "", item.CHR_SectionCode ?? "", item.CHR_SectionName ?? "", item.CHR_Gap == "false" ? false : true, item.ID_StepBaoGia ?? 3);
                                        }
                                        else if (item.ID_StepBaoGia != 6)
                                        {
                                            await sendMailService.SendMailAsync(item.CHR_UserApproval + "@brothergroup.net", item.CHR_UserApproval + "@brothergroup.net", 11, "ApprovalQuote/Index", item.CHR_Gap == "false" ? false : true, item.CHR_SectionCode ?? "", item.CHR_MaDon ?? "", currentUserId);
                                        }
                                        //await sendMailService.SendMailToRequesterAsync(item.CHR_MaDon ?? "", item.CHR_SectionCode ?? "", item.CHR_SectionName ?? "", item.CHR_Gap == "false" ? false : true, item.ID_StepBaoGia ?? 3);

                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi gửi mail phê duyệt");
                                }
                            }
                        });
                    }
                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }
                    // Insert xác nhận tên và gửi mail trong background
                    var MaterialsNew = insertedList
                        .Where(l => string.IsNullOrEmpty(l.CHR_MaHangNoiBo) && l.ID_StepBaoGia >= 6 && l.BIT_LayBaoGia == true)
                        .ToList();

                    // Insert xác nhận tên và gửi mail trong background
                    if (MaterialsNew.Count > 0)
                    {
                        // Run background work without accessing controller/HttpContext inside the task
                        _ = Task.Run(async () =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    // service
                                    var baoGiaConfirmNameService = scope.ServiceProvider.GetRequiredService<IBaoGiaConfirmNameService>();
                                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                    var baoGiaService = scope.ServiceProvider.GetRequiredService<IBaoGiaService>();
                                    var materialService = scope.ServiceProvider.GetRequiredService<IMaterialService>();

                                    // Phân loại MaterialsNew theo CHR_Phanloai (A hoặc I)
                                    var materialsByPhanLoai = MaterialsNew
                                        .Where(m => !string.IsNullOrEmpty(m.CHR_MaHangNCC))
                                        .GroupBy(m => GetMaterialType(m.CHR_Phanloai))
                                        .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                                    var MaterialNews = new List<MATERIALDTO>();
                                    var confirmNames = new List<ConfirmNameDTO>();

                                    foreach (var phanLoaiGroup in materialsByPhanLoai)
                                    {
                                        var materialType = phanLoaiGroup.Key; 
                                        var materialsInGroup = phanLoaiGroup.Value;

                                        // Lấy số tiếp theo cho từng loại
                                        var latestCode = await materialService.MaterialCodeLater(materialType);
                                        var nextNumber = ExtractNumberFromCode(latestCode.Data);

                                        var processedSuppliers = new Dictionary<string, MATERIALDTO>(StringComparer.OrdinalIgnoreCase);

                                        var materialsBySupplier = materialsInGroup
                                            .GroupBy(m => m.CHR_MaHangNCC)
                                            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                                        foreach (var supplierGroup in materialsBySupplier)
                                        {
                                            var supplierCode = supplierGroup.Key;
                                            var materials = supplierGroup.Value;

                                            // Gọi service check material code
                                            var checkCode = await materialService.CheckMaterialCode(supplierCode, materials.First().NVCHR_ChungLoai);

                                            if (!checkCode.Success)
                                            {
                                                _logger.LogError(checkCode.Message, "Lỗi khi check Material code for {MaHangNCC}, Type: {Type}", supplierCode, materialType);
                                                continue;
                                            }

                                            string existingMaterialCode = checkCode.Data;

                                            if (!string.IsNullOrEmpty(existingMaterialCode))
                                            {
                                                // Trường hợp đã tồn tại material code
                                                foreach (var material in materials)
                                                {
                                                    confirmNames.Add(new ConfirmNameDTO
                                                    {
                                                        Id = material.ID,
                                                        MaHangNoiBo = existingMaterialCode
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                if (processedSuppliers.TryGetValue(supplierCode, out var existingMaterial))
                                                {
                                                    foreach (var material in materials)
                                                    {
                                                        confirmNames.Add(new ConfirmNameDTO
                                                        {
                                                            Id = material.ID,
                                                            MaHangNoiBo = existingMaterial.Material_Code
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    // Tạo material mới
                                                    nextNumber++;
                                                    var newMaterialCode = GenerateMaterialCode(materialType, nextNumber);
                                                    var firstMaterial = materials.First();

                                                    // Xác nhận loại hàng dựa trên CHR_Phanloai
                                                    //var typeMaterial = GetMaterialType(materialType);
                                                    var OutSide = "";
                                                    switch (materialType)
                                                    {
                                                        case "A":
                                                        case "B":
                                                        case "E":
                                                            OutSide = "IN";
                                                            break;
                                                        default:
                                                            OutSide = "OUT";
                                                            break;
                                                    }

                                                    var newMaterial = new MATERIALDTO
                                                    {
                                                        Material_Code = newMaterialCode,
                                                        Material_Name_VN = firstMaterial.NVCHR_NameVN,
                                                        Material_Name_EN = firstMaterial.CHR_NameEN,
                                                        Code_Suppiler = firstMaterial.CHR_MaHangNCC,
                                                        Category_VN = firstMaterial.NVCHR_ChungLoai,
                                                        Shape = firstMaterial.NVCHR_HinhDang,
                                                        Material = firstMaterial.NVCHR_ChatLieu,
                                                        Composition = firstMaterial.NVCHR_ThanhPhan,
                                                        Dimension = firstMaterial.NVCHR_KichThuoc,
                                                        UsedFor = firstMaterial.NVCHR_DongMay,
                                                        Purpose = firstMaterial.NVCHR_TinhNang,
                                                        CHR_MaterialOutSide = OutSide, 
                                                        Unit = firstMaterial.NVCHR_DonVi
                                                    };

                                                    MaterialNews.Add(newMaterial);
                                                    processedSuppliers[supplierCode] = newMaterial;

                                                    foreach (var material in materials)
                                                    {
                                                        confirmNames.Add(new ConfirmNameDTO
                                                        {
                                                            Id = material.ID,
                                                            MaHangNoiBo = newMaterialCode
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Send mail
                                    if (confirmNames.Any())
                                    {
                                        await baoGiaService.UpdateCodeMaterialBIVN(confirmNames);
                                    }
                                    if (MaterialNews.Any())
                                    {
                                        await materialService.UpdateListThongTinNoList(MaterialNews);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Lỗi khi xử lý material theo phân loại A/I");
                                }
                            }
                        });
                    }
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private string GetMaterialType(string phanLoai)
        {
            if (string.IsNullOrEmpty(phanLoai)) return "O";

            var upperPhanLoai = phanLoai.ToUpper().Trim();

            if (upperPhanLoai == "I")
                return "I";
            if (upperPhanLoai == "A")
                return "A";
            if (upperPhanLoai == "B")
                return "B";
            if (upperPhanLoai == "E")
                return "E";
            return "O";
        }

        // Tạo mã material với prefix A hoặc I
        private string GenerateMaterialCode(string type, int number)
        {
            return $"{type}{number:D8}";
        }

        // Extract số từ mã material
        private int ExtractNumberFromCode(string materialCode)
        {
            if (string.IsNullOrEmpty(materialCode)) return 0;
            var match = Regex.Match(materialCode, @"\d+");
            return match.Success && int.TryParse(match.Value, out int number) ? number : 0;
        }
        // case NG
        [HttpPost]
        public async Task<IActionResult> UpdateQuotationNG([FromBody] List<BaoGia_Request_of_QuotationDTO> updateModel)
        {
            try
            {
                var result = await _baoGiaService.CapNhatDanhSachBGAsync(updateModel);
                if (result.Success)
                {
                    var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                    // Capture user info before background task
                    var currentUserId = GetCurrentUserId();
                    var firstItem = insertedList.FirstOrDefault();
                    var isGap = firstItem?.CHR_Gap == "false" ? false : true;
                    var sectionName = firstItem?.CHR_SectionName;
                    var maDon = firstItem?.CHR_MaDon;
                    var userCreate = firstItem?.CHR_CreateBy;
                    // Luu lich su phe duyet
                    var approverHistories = insertedList.Select(b => new BaoGia_History_Approver_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        ID_BaoGiaStep = StepOld(b.ID_Status),
                        CHR_UserSendApprover = GetCurrentUserId() ?? string.Empty,
                        DTM_UserSendApprover = DateTime.Now,
                        CHR_UserApprover = GetCurrentUserId() ?? string.Empty,
                        DTM_UserApprover = DateTime.Now,
                        CHR_StatusFlag = "1",
                        BIT_SendMail = false,
                        NVCHR_ReturnReason = b.NVCHR_LyDo,
                        BIT_Return = true,
                        CHR_SectionCodeSend = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameSend = GetCurrentUserDepartment() ?? string.Empty,
                        CHR_SectionCodeApprover = GetCurrentUserSection() ?? string.Empty,
                        CHR_SectionNameApprover = GetCurrentUserDepartment() ?? string.Empty
                    }).ToList();
                    if (approverHistories.Any())
                    {
                        await _historyApproverServive.AddHistoryListAsync(approverHistories);
                    }
                    // Luu lich su thay doi trang thai bao gia
                    var histories = insertedList.Select(b => new BaoGia_History_Request_of_QuotationDTO
                    {
                        ID_RequestQuote = b.ID,
                        CHR_MaDon = b.CHR_MaDon ?? string.Empty,
                        CHR_UpdateBy = GetCurrentUserId() ?? string.Empty,
                        NVCHR_UpdateName = GetCurrentUserFullName() ?? string.Empty,
                        CHR_Updatedate = DateTime.Now,
                        CHR_ChangedColumns = null,
                        CHR_OldData = null,
                        CHR_NewData = System.Text.Json.JsonSerializer.Serialize(b),
                        NVCHR_LyDo = b.NVCHR_LyDo,
                        CHR_ActionType = b.ID_Status
                    }).ToList();

                    if (histories.Any())
                    {
                        await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                    }
                    // Gui mail thông báo trả về người tạo báo giá
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                var mail = userCreate + "@brothergroup.net";
                                var emailResult = await sendMailService.SendMailAsync(mail, mail, 12,
                                    "Quote/HistoryQuote", isGap,
                                    sectionName, maDon, currentUserId);
                                //var emailResult = await sendMailService.SendMailToConfirmItemAsync(12, 12, "Quote/HistoryQuote", true, "", "", currentUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending email in UpdateQuotationNG");
                            }
                        }
                    });

                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Export to Excel
        [HttpPost] //ApprovalQuoteSearchViewModel vm
        public async Task<IActionResult> ExportToExcel([FromBody] List<BaoGia_Request_of_QuotationDTO> updateModel)
        {
            try
            {
                if (updateModel == null || !updateModel.Any())
                {
                    return BadRequest("No data to export");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemPlateApproverQuote.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemPlateQuote.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int rowStart = 8;

                foreach (var item in updateModel)
                {
                    int col = 2;
                    ws.Cell(rowStart, col++).SetValue(item.CHR_SectionCode);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_SectionName);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_Phanloai);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaThietBi);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNoiBo);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNCC);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NameVN);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_NameEN);
                    ws.Cell(rowStart, col++).SetValue(item.INT_SoLuong);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DonVi);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChungLoai);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_HinhDang);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChatLieu);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ThanhPhan);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_KichThuoc);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DongMay);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TinhNang);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Rohs);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_COCQ);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MSDS);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_AnToan);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_FileThietKe);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaNCC);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenNCC);
                    ws.Cell(rowStart, col++).SetValue(item.BIT_LayBaoGia == false ? "X" : "O");
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_LyDo);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_NgayMuonNhan);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_KyHan);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_UserRequest);

                    rowStart++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"FileApproverQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error exporting to Excel: " + ex.Message);
            }
        }
        private int StepOld(string? status)
        {
            switch (status)
            {
                case "RETURN_QLSC": return 2; // Trả về QLSC phong ban
                case "RETURN_QLTC": return 3; // Trả về QLTC phong ban
                case "RETURN_PIC": return 4;    // Trả về PIC phong mua hang
                case "RETURN_QLSC_1": return 5; // Trả về QLSC phong ban mua hang
                default: return 1;
            }
        }
        private string StatusOld(int? step)
        {
            switch (step)
            {
                case 3: return "QLSC";
                case 4: return "QLTC";
                case 5: return "PIC";
                case 6: return "QLSC_1";
                default: return "CREATE";
            }
        }
    }
}
