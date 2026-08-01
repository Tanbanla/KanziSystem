using ClosedXML.Excel;
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
            var role = GetRolesUser();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";

            var vm = new ApprovalQuoteViewModel
            {
                listNhomVitri = nhomViTriList,
                listMaterial = materialList,
                listSoDon = soDonList,
                listStatusBaoGia = statusBaoGiaList,
                listStepBaoGia = stepBaoGiaList,
                Role = role
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
                    await EventApprovelOk(insertedList);
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
            var format = new[] { "A", "B", "E" }.Contains(type.ToUpper())
                ? "D6"
                : "D8";

            return $"{type}{number.ToString(format)}";
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
                var result = await _baoGiaService.UpdatePheDuyetDonBaoGiaAsync(updateModel);
                if (result.Success)
                {
                    var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                    await EventApprovelNG(insertedList);
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
        // Send mail Approval OK
        private async Task<IActionResult> EventApprovelOk(List<BaoGia_Request_of_QuotationDTO> insertedList)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
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
                // update kì hạn báo giá
                var listUpadte = insertedList.ToList();
                if (SectionApporve != null)
                {
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                
                                var baoGiaService = scope.ServiceProvider.GetRequiredService<IBaoGiaService>();
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                                //update kỳ hạn báo giá
                                await baoGiaService.UpdateDeadlineAsync(listUpadte);

                                // gửi mail thông báo phê duyệt cho requester và approver tiếp theo
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

                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail phê duyệt");
                            }
                        }
                    });
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
                                    //.Where(m => !string.IsNullOrEmpty(m.CHR_MaHangNCC))
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
                                        .GroupBy(m => string.IsNullOrEmpty(m.CHR_MaHangNCC) ? m.CHR_NameEN : m.CHR_MaHangNCC)
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
                                                var Group = "";
                                                switch (materialType)
                                                {
                                                    case "A":                                                   
                                                    case "E":
                                                        OutSide = "IN";
                                                        Group = "PUR";
                                                        break;
                                                    case "B":
                                                        OutSide = "IN";
                                                        Group = "GA";
                                                        break;
                                                    case "I":
                                                        OutSide = "OUT";
                                                        Group = "IT";
                                                        break;
                                                    default:
                                                        OutSide = "OUT";
                                                        Group = "PUR";
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
                                                    Unit = firstMaterial.NVCHR_DonVi,
                                                    Group_Code = Group
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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error Approval: "+ex.Message);
            }
        }
        // Send mail Approval NG
        private async Task<IActionResult> EventApprovelNG(List<BaoGia_Request_of_QuotationDTO> insertedList)
        {
            try {
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
                                "History/HistoryQuote", isGap,
                                sectionName, maDon, currentUserId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending email in UpdateQuotationNG");
                        }
                    }
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error Approval: " + ex.Message);
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
                var templatePath = Path.Combine(root, "template", "TemplateApprover.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateApprover.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int rowStart = 4;

                foreach (var item in updateModel)
                {
                    int col = 1;
                    ws.Cell(rowStart, col++).SetValue(rowStart-3);
                    ws.Cell(rowStart, col++).SetValue(item.ID);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_SectionCode);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_SectionName);
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
                    ws.Cell(rowStart, col++).SetValue(item?.DTM_NgayMuonNhan.HasValue == true ? item.DTM_NgayMuonNhan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item?.DTM_KyHan.HasValue == true ? item.DTM_KyHan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item?.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(rowStart, col++).SetValue(item?.NVCHR_UserRequest);
                    ws.Cell(rowStart, col++).SetValue(item?.NVCHR_ReasonQuotation);
                    ws.Cell(rowStart, col++).SetValue(item?.ID_StepBaoGia);
                    ws.Cell(rowStart, col++).SetValue(item?.ID_Status);
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
        // Nhập thông tin phê duyệt báo giá
        [HttpPost]
        public async Task<IActionResult> ImportExcel([FromForm] ImportPickSupplier vm)
        {
            if (vm.fileSend == null || vm.fileSend.Length == 0)
                return BadRequest("File không hợp lệ");

            var itemsOK = new List<BaoGia_Request_of_QuotationDTO>();
            var itemsNG = new List<BaoGia_Request_of_QuotationDTO>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = vm.fileSend.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                var isErrors = false;
                // Dữ liệu bắt đầu từ dòng 4
                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                // các dòng hợp lệ
                var allRows = new List<(int Row, string MaDon, bool BitSelect, string Reason, BaoGia_Request_of_QuotationDTO Dto)>();

                for (int r = startRow; r <= lastRow; r++)
                {
                    var errors = new List<string>();
                    var idRequest = ws.Cell(r, 2).GetString();
                    if (string.IsNullOrEmpty(idRequest))
                    {
                        break;
                    }
                    var bitSelect = ws.Cell(r, 38).GetString().Contains("NG");
                    var reason = ws.Cell(r, 39).GetString();
                    var maDon = ws.Cell(r, 3).GetString();

                    // Validate từng dòng
                    if (bitSelect && string.IsNullOrEmpty(reason))
                    {
                        isErrors = true;
                        errors.Add("Chưa chọn lý do từ chối");
                    }

                    if (errors.Any())
                    {
                        errorRows.Add(new
                        {
                            Row = r,
                            MaDon = maDon,
                            ID = idRequest,
                            BIT_Select = bitSelect,
                            NVCHR_ReasonPick = reason,
                            Errors = string.Join("; ", errors)
                        });
                    }
                    else
                    {
                        var dto = CreateDtoFromMaterial(ws, r);
                        allRows.Add((r, maDon, bitSelect, reason, dto));
                    }
                }

                if (isErrors)
                {
                    // Create error file
                    using var errorWorkbook = new ClosedXML.Excel.XLWorkbook();
                    var errorWs = errorWorkbook.Worksheets.Add("Errors");
                    errorWs.Cell(1, 1).Value = "Row";
                    errorWs.Cell(1, 2).Value = "MaDon";
                    errorWs.Cell(1, 3).Value = "ID";
                    errorWs.Cell(1, 4).Value = "BIT_Select";
                    errorWs.Cell(1, 5).Value = "NVCHR_ReasonPick";
                    errorWs.Cell(1, 6).Value = "Errors";
                    for (int i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i];
                        errorWs.Cell(i + 2, 1).Value = row.Row;
                        errorWs.Cell(i + 2, 2).Value = row.MaDon;
                        errorWs.Cell(i + 2, 3).Value = row.ID;
                        errorWs.Cell(i + 2, 4).Value = row.BIT_Select;
                        errorWs.Cell(i + 2, 5).Value = row.NVCHR_ReasonPick;
                        errorWs.Cell(i + 2, 6).Value = row.Errors;
                    }
                    using var errorStream = new MemoryStream();
                    errorWorkbook.SaveAs(errorStream);
                    var errorBytes = errorStream.ToArray();
                    var errorFileName = $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    const string errorContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(errorBytes, errorContentType, errorFileName);
                }
                else
                {
                    // Xử lý theo nhóm MaDon
                    var groupedByMaDon = allRows.GroupBy(x => x.MaDon);

                    foreach (var group in groupedByMaDon)
                    {
                        var maDon = group.Key;
                        var hasAnyNG = group.Any(x => x.BitSelect);
                        //var reasonForNG = group.Where(x => x.BitSelect && !string.IsNullOrEmpty(x.Reason)).Select(x => x.Reason).FirstOrDefault();

                        if (hasAnyNG)
                        {
                            // Nếu có ít nhất một NG, tất cả đều NG với lý do chung
                            foreach (var item in group)
                            {
                                var i = item.Dto;
                                i.ID_Status = GetStatusFromStepReturn(i.ID_StepBaoGia);
                                i.NVCHR_LyDo = item.Reason;
                                i.ID_StepBaoGia = 1;
                                itemsNG.Add(i);
                            }
                        }
                        else
                        {
                            // Tất cả OK
                            foreach (var item in group)
                            {
                                var i = item.Dto;
                                i.ID_StepBaoGia = i.ID_StepBaoGia + 1;
                                i.ID_Status = GetStatusFromStep(i.ID_StepBaoGia);
                                i.CHR_UserApproval = "nganng";
                                itemsOK.Add(i);
                            }
                        }
                    }

                    if(itemsOK.Any())
                    {
                        var updateResult = await _baoGiaService.UpdatePheDuyetDonBaoGiaAsync(itemsOK);
                        if (!updateResult.Success)
                        {
                            return BadRequest(updateResult.Message);
                        }
                        await EventApprovelOk(updateResult.Data);
                    }
                    if (itemsNG.Any())
                    {
                        var updateResult = await _baoGiaService.UpdatePheDuyetDonBaoGiaAsync(itemsNG);
                        if (!updateResult.Success)
                        {
                            return BadRequest(updateResult.Message);
                        }
                        await EventApprovelNG(updateResult.Data);
                    }
                    return Ok(new { message = "Import successful" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        private string GetStatusFromStepReturn(int? step)
        {
            switch (step)
            {
                case 2: return "RETURN_QLSC"; // Trả về QLSC phong ban
                case 3: return "RETURN_QLTC"; // Trả về QLTC phong ban
                case 4: return "RETURN_PIC";    // Trả về PIC phong mua hang
                case 5: return "RETURN_QLSC_1"; // Trả về QLSC phong ban mua hang
                default: return "CREATE";
            }
        }
        private string GetStatusFromStep(int? step)
        {
            switch (step)
            {
                case 5: return "APPROVAL5";
                default: return "WAIT_SEND_MAIL";
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
        private BaoGia_Request_of_QuotationDTO CreateDtoFromMaterial(IXLWorksheet ws, int row)
        {
            return new BaoGia_Request_of_QuotationDTO
            {
                ID = int.TryParse(ws.Cell(row, 2).GetString(), out int id) ? id : 0,
                CHR_MaDon = ws.Cell(row, 3).GetString(),
                CHR_SectionCode = ws.Cell(row, 4).GetString(),
                CHR_SectionName = ws.Cell(row, 5).GetString(),
                CHR_Phanloai = ParsePhanloai(ws.Cell(row, 6).GetString()),
                CHR_MaThietBi = ws.Cell(row, 7).GetString(),
                CHR_MaHangNoiBo = ws.Cell(row, 8).GetString(),
                CHR_MaHangNCC = ws.Cell(row, 9).GetString(),
                NVCHR_NameVN = ws.Cell(row, 10).GetString(),
                CHR_NameEN = ws.Cell(row, 11).GetString(),
                INT_SoLuong = ParseDouble(ws.Cell(row, 12).GetString()),
                NVCHR_DonVi = ws.Cell(row, 13).GetString(),
                NVCHR_ChungLoai = ws.Cell(row, 14).GetString(),
                NVCHR_HinhDang = ws.Cell(row, 15).GetString(),
                NVCHR_ChatLieu = ws.Cell(row, 16).GetString(),
                NVCHR_ThanhPhan = ws.Cell(row, 17).GetString(),
                NVCHR_KichThuoc = ws.Cell(row, 18).GetString(),
                NVCHR_DongMay = ws.Cell(row, 19).GetString(),
                NVCHR_TinhNang = ws.Cell(row, 20).GetString(),
                NVCHR_Rohs = ws.Cell(row, 21).GetString(),
                NVCHR_COCQ = ws.Cell(row, 22).GetString(),
                NVCHR_MSDS = ws.Cell(row, 23).GetString(),
                NVCHR_AnToan = ws.Cell(row, 24).GetString(),
                NVCHR_FileThietKe = ws.Cell(row, 25).GetString(),
                NVCHR_NhaSanXuat = ws.Cell(row, 26).GetString(),
                CHR_MaNCC = ws.Cell(row, 27).GetString(),
                NVCHR_TenNCC = ws.Cell(row, 28).GetString(),
                BIT_LayBaoGia = ParseBool(ws.Cell(row, 29).GetString()),
                NVCHR_LyDo = ws.Cell(row, 30).GetString(),
                DTM_NgayMuonNhan = ParseDate(ws.Cell(row, 31).GetString()   ),
                DTM_KyHan = ParseDate(ws.Cell(row, 32).GetString()),
                CHR_Gap = ParseBool(ws.Cell(row, 33).GetString()) == false ? "false" : "true",
                NVCHR_UserRequest = ws.Cell(row, 34).GetString(),
                ID_StepBaoGia = ParseInt(ws.Cell(row, 36).GetString()),
                ID_Status = ws.Cell(row, 37).GetString()
            };
        }
        private static string? ParsePhanloai(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "No list";
            if (s != "A" && s != "B" && s != "C" && s != "E" && s != "I") return "No list";
            return s;
        }
        private static double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }
        private static int? ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }
        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            return null;
        }
        private static bool? ParseBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var v = s.Trim().ToLowerInvariant();
            return v.ToUpper().Contains("O") ? true : false;
        }
    }
}
