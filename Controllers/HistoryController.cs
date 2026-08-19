using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Globalization;
using MiniExcel = MiniExcelLibs.MiniExcel;
using Path = System.IO.Path;
namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class HistoryController : BaseAuthController
    {

        private readonly ILogger<HistoryController> _logger;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IWebHostEnvironment _env;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IMaterialService _materialService;
        private readonly IConfiguration _configuration;
        private readonly IFileImportService _fileImportService;
        private readonly ITmUserService _tmUserService;

        private readonly ITmCategoryService _tmCategoryService;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IStringLocalizer<HistoryController> _localizer;

        public HistoryController(IWebHostEnvironment env, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaService baoGiaService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaStepService baoGiaStepService, ILogger<HistoryController> logger, IServiceScopeFactory serviceScopeFactory,
            IMasterApproverSendMailService approverService, IMaterialService materialService, IConfiguration configuration
            , ITmCategoryService tmCategoryService, IDepartmentService deparmentService, ITmNccNewService tmNccNewService,
            IStringLocalizer<HistoryController> localizer, IFileImportService fileImportService, ITmUserService tmUserService
            )
        {
            _env = env;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaService = baoGiaService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaStepService = baoGiaStepService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _approverService = approverService;
            _materialService = materialService;
            _configuration = configuration;
            _tmCategoryService = tmCategoryService;
            _deparmentService = deparmentService;
            _tmNccNewService = tmNccNewService;
            _localizer = localizer;
            _fileImportService = fileImportService;
            _tmUserService = tmUserService;
        }
        // tìm kiếm đơn báo giá
        [HttpPost]
        public async Task<IActionResult> SearchHistoryBaoGia([FromBody] SearchBaoGiaViewModel searchModel)
        {
            var result = await _baoGiaHistoryService.SearchHistoryAsync(
                searchModel.MaDon,
                searchModel.MaNcc,
                searchModel.Section,
                searchModel.NguoiYeuCau,
                searchModel.MaHang,
                searchModel.TrangThai,
                searchModel.Step,
                GetCurrentUserId() ?? "",
                searchModel.PageIndex,
                searchModel.PageSize,
                searchModel.to,
                searchModel.from,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        // Xuất file quản lý tiến độ màn hình lịch sử báo giá
        [HttpPost]
        public async Task<IActionResult> ExportManagerHistory([FromBody] SearchBaoGiaViewModel searchModel)
        {
            // Lấy thông tin dữ liệu lịch sử báo giá theo điều kiện tìm kiếm
            var result = await _baoGiaService.ExportHistoryBaoGiaAsync(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai, searchModel.to, searchModel.from);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            // Lấy thông tin lịch sử phê duyệt theo điều kiện tìm kiếm
            var historyApprover = await _baoGiaHistoryService.GetHistoryApprover(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai, searchModel.to, searchModel.from);
            if (!historyApprover.Success)
            {
                return BadRequest(historyApprover.Message);
            }
            // lấy thông tin trạng thái theo đơn báo giá và mã hàng nội bộ
            var historyByMaterial = await _baoGiaHistoryService.GetHistoryByMaterialCode(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai, searchModel.to, searchModel.from);
            if (!historyByMaterial.Success)
            {
                return BadRequest(historyByMaterial.Message);
            }
            var stepByRole = GetRolesUser() == "UserPUR" ? 8 : 12;

            try
            {
                var historyData = result.Data;
                if (historyData == null || !historyData.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }

                var historyMaterialData = historyByMaterial.Data ?? new List<dynamic>();

                // Lấy thông tin status
                var statusResult = await _baoGiaStatusService.GetListStatusAsync();
                if (statusResult == null || !statusResult.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }

                // Lấy thông tin step
                var stepsResult = await _baoGiaStepService.GetAll();
                if (stepsResult == null || !stepsResult.Success)
                {
                    return BadRequest("Lỗi lấy danh sách step");
                }

                // Chuẩn bị dictionary để tra cứu nhanh O(1) thay vì FirstOrDefault trong mỗi dòng.
                var statusMap = statusResult.Data?
                    .Where(s => !string.IsNullOrWhiteSpace(s.VCHR_CodeStatus))
                    .GroupBy(s => s.VCHR_CodeStatus)
                    .ToDictionary(g => g.Key!, g => g.First().NVCHR_TenStatus ?? string.Empty)
                    ?? new Dictionary<string, string>();

                var stepMap = stepsResult.Data?
                    .Where(s => s.INT_StepNumber.HasValue)
                    .GroupBy(s => s.INT_StepNumber!.Value)
                    .ToDictionary(g => g.Key, g => g.First().CHR_StepName ?? string.Empty)
                    ?? new Dictionary<int, string>();

                static bool IsReturnStatus(string? statusCode)
                    => !string.IsNullOrWhiteSpace(statusCode)
                       && statusCode.IndexOf("RETURN", StringComparison.OrdinalIgnoreCase) >= 0;

                static string ToDateString(DateTime? date)
                    => date?.ToString("dd/MM/yyyy") ?? string.Empty;

                string GetStatusName(string? statusCode)
                    => (!string.IsNullOrWhiteSpace(statusCode) && statusMap.TryGetValue(statusCode, out var statusName))
                        ? statusName
                        : string.Empty;

                string GetStepName(object? stepValue)
                {
                    if (stepValue == null) return string.Empty;

                    try
                    {
                        var stepNumber = Convert.ToInt32(stepValue);
                        return stepMap.TryGetValue(stepNumber, out var stepName) ? stepName : string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                static bool IsSelectedValue(object? selectValue)
                {
                    if (selectValue == null) return false;

                    try
                    {
                        return Convert.ToBoolean(selectValue);
                    }
                    catch
                    {
                        return false;
                    }
                }

                static bool IsStepLessOrEqualSix(object? stepValue)
                {
                    if (stepValue == null) return false;

                    try
                    {
                        return Convert.ToInt32(stepValue) <= 6;
                    }
                    catch
                    {
                        return false;
                    }
                }

                static string GetSelectMark(object? selectValue, object? stepValue)
                {
                    bool? isSelected = null;

                    try
                    {
                        if (selectValue != null)
                        {
                            isSelected = Convert.ToBoolean(selectValue);
                        }
                    }
                    catch
                    {
                        isSelected = null;
                    }

                    if (isSelected == null || (isSelected == false && IsStepLessOrEqualSix(stepValue)))
                    {
                        return string.Empty;
                    }

                    return isSelected == true ? "O" : "X";
                }

                // Lấy các đơn trả về có trạng thái RETURN để lấy lý do trả
                var listReason = new List<ReasonQuotition>();
                var returnIds = historyData
                    .Where(rq => rq != null && IsReturnStatus(rq?.ID_Status))
                    .Select(rq => rq.ID)
                    .Distinct()
                    .ToList();

                if (returnIds.Any())
                {
                    var reasons = await _baoGiaHistoryService.GetReasonsAsync(returnIds);
                    if (!reasons.Success)
                    {
                        return BadRequest("Lỗi lấy lý do trả");
                    }
                    listReason = reasons.Data ?? new List<ReasonQuotition>();
                }

                var reasonMap = listReason
                    .GroupBy(r => r.Id)
                    .ToDictionary(g => g.Key, g => g.First().Reason ?? string.Empty);

                string GetReason(object? idValue, string? statusCode)
                {
                    if (!IsReturnStatus(statusCode) || idValue == null)
                    {
                        return string.Empty;
                    }

                    try
                    {
                        var id = Convert.ToInt32(idValue);
                        return reasonMap.TryGetValue(id, out var reason) ? reason : string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateExportHistoryNew.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateExportHistoryNew.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheet(1);
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                var wsApprover = workbook.Worksheets.Count >= 2 ? workbook.Worksheet(2) : null;
                if (wsApprover == null)
                {
                    return BadRequest("Không tìm thấy sheet 2 trong template");
                }

                var wsMaterial = workbook.Worksheets.Count >= 3 ? workbook.Worksheet(3) : null;
                if (wsMaterial == null)
                {
                    return BadRequest("Không tìm thấy sheet 3 trong template");
                }

                var mainRows = new List<object[]>(historyData.Count);
                int stt = 1;

                foreach (var rq in historyData)
                {
                    if (rq == null) continue;

                    var selectMark = GetSelectMark(rq.BIT_Select, rq.ID_StepBaoGia);

                    var isSelected = rq.BIT_Select == true;
                    mainRows.Add(new object[]
                    {
                        stt++,
                        rq.CHR_MaDon ?? string.Empty,
                        rq.ID,
                        rq.CHR_SectionCode ?? string.Empty,
                        rq.CHR_SectionName ?? string.Empty,
                        rq.CHR_Phanloai ?? string.Empty,
                        rq.CHR_MaThietBi ?? string.Empty,
                        rq.CHR_MaHangNoiBo ?? string.Empty,
                        rq.CHR_MaHangNCC ?? string.Empty,
                        rq.NVCHR_NameVN ?? string.Empty,
                        rq.CHR_NameEN ?? string.Empty,
                        rq.INT_SoLuong ?? 0,
                        rq.NVCHR_DonVi ?? string.Empty,
                        rq.NVCHR_ChungLoai ?? string.Empty,
                        rq.NVCHR_HinhDang ?? string.Empty,
                        rq.NVCHR_ChatLieu ?? string.Empty,
                        rq.NVCHR_ThanhPhan ?? string.Empty,
                        rq.NVCHR_KichThuoc ?? string.Empty,
                        rq.NVCHR_DongMay ?? string.Empty,
                        rq.NVCHR_TinhNang ?? string.Empty,
                        rq.NVCHR_Rohs ?? string.Empty,
                        rq.NVCHR_COCQ ?? string.Empty,
                        rq.NVCHR_MSDS ?? string.Empty,
                        rq.NVCHR_AnToan ?? string.Empty,
                        rq.NVCHR_FileThietKe ?? string.Empty,
                        rq.NVCHR_NhaSanXuat ?? string.Empty,
                        rq.CHR_MaNCC ?? string.Empty,
                        rq.ShortName ?? string.Empty,
                        rq.BIT_LayBaoGia == false ? "X" : "O",
                        rq.NVCHR_LyDo ?? string.Empty,
                        ToDateString(rq.DTM_NgayMuonNhan),
                        ToDateString(rq.DTM_KyHan),
                        rq.CHR_Gap == "false" ? "X" : "O",
                        rq.NVCHR_UserRequest ?? string.Empty,
                        GetStatusName(rq.ID_Status),
                        GetStepName(rq.ID_StepBaoGia),
                        GetReason(rq.ID, rq.ID_Status),
                        rq.ID_StepBaoGia > stepByRole ? selectMark : string.Empty,
                        rq.ID_StepBaoGia > stepByRole ? rq.NVCHR_ReasonPick ?? string.Empty : string.Empty,
                        rq.ID_StepBaoGia > stepByRole ? rq.NVCHR_File ?? string.Empty : string.Empty
                    });
                }

                ws.Cell(4, 1).InsertData(mainRows);

                // Export dữ liệu lịch sử phê duyệt vào sheet 2
                var approverData = historyApprover.Data ?? Enumerable.Empty<dynamic>();
                var approverRows = new List<object[]>();
                int sttApprover = 1;

                foreach (var item in approverData)
                {
                    if (item == null) continue;

                    approverRows.Add(new object[]
                    {
                        sttApprover++,
                        item.maDon ?? string.Empty,
                        item.ID_RequestQuote,
                        item.userInsert ?? string.Empty,
                        item.timeInsert?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        item.userChief ?? string.Empty,
                        item.timeChief?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        item.userSection ?? string.Empty,
                        item.timeSection?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        item.userPIC ?? string.Empty,
                        item.timePIC?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty,
                        item.userPur ?? string.Empty,
                        item.timePur?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty
                    });
                }

                if (approverRows.Count > 0)
                {
                    wsApprover.Cell(4, 1).InsertData(approverRows);
                }

                // Export dữ liệu trạng thái theo mã hàng nội bộ vào sheet 3
                var materialRows = new List<object[]>();
                int sttMaterial = 1;

                foreach (var item in historyMaterialData)
                {
                    if (item == null) continue;

                    var selectMark = GetSelectMark(item.BIT_Select, item.ID_StepBaoGia);

                    var isSelected = IsSelectedValue(item.BIT_Select);

                    materialRows.Add(new object[]
                    {
                        sttMaterial++,
                        item.CHR_MaDon ?? string.Empty,
                        item.CHR_MaHangNoiBo ?? string.Empty,
                        item.CHR_SectionCode ?? string.Empty,
                        item.CHR_SectionName ?? string.Empty,
                        item.CHR_Phanloai ?? string.Empty,
                        item.CHR_MaThietBi ?? string.Empty,
                        item.CHR_MaHangNCC ?? string.Empty,
                        item.NVCHR_NameVN ?? string.Empty,
                        item.CHR_NameEN ?? string.Empty,
                        item.INT_SoLuong ?? 0,
                        item.NVCHR_DonVi ?? string.Empty,
                        item.NVCHR_ChungLoai ?? string.Empty,
                        item.NVCHR_File ?? string.Empty,
                        item.CHR_LinkFile ?? string.Empty,
                        item.NVCHR_NhaSanXuat ?? string.Empty,
                        item.CHR_MaNCC ?? string.Empty,
                        item.ShortName ?? string.Empty,
                        item.BIT_LayBaoGia == false ? "X" : "O",
                        item.NVCHR_LyDo ?? string.Empty,
                        ToDateString(item.DTM_NgayMuonNhan),
                        ToDateString(item.DTM_KyHan),
                        item.CHR_Gap == "false" ? "X" : "O",
                        item.NVCHR_UserRequest ?? string.Empty,
                        GetStatusName(item.ID_Status),
                        GetStepName(item.ID_StepBaoGia),
                        GetReason(item.ID, item.ID_Status),
                        selectMark,
                        isSelected ? item.NVCHR_ReasonPick ?? string.Empty : string.Empty,
                        isSelected ? item.NVCHR_File ?? string.Empty : string.Empty
                    });
                }

                if (materialRows.Count > 0)
                {
                    wsMaterial.Cell(4, 1).InsertData(materialRows);
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"HistoryManagerQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }
        // Nhập file chỉnh sửa thông tin lịch sử báo giá 
        [HttpPost]
        public async Task<IActionResult> ImportFileExcelEditHistory([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Not data import file");
            var listRequest = new List<BaoGia_Request_of_QuotationDTO>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // Dữ liệu bắt đầu từ dòng 4 (the same layout as ExportHistory)
                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int i = startRow; i <= lastRow; i++)
                {
                    // Stop if MaDon empty
                    var maDon = ws.Cell(i, 2).GetString();
                    if (string.IsNullOrWhiteSpace(maDon)) break;

                    // Read values according to the expected column layout
                    var dto = new BaoGia_Request_of_QuotationDTO();
                    try
                    {
                        dto.ID = ws.Cell(i, 3).GetValue<int>();
                    }
                    catch
                    {
                        // if cannot parse id, skip row
                        continue;
                    }
                    dto.CHR_MaDon = maDon;
                    dto.CHR_SectionCode = ws.Cell(i, 4).GetString() ?? string.Empty;
                    dto.CHR_SectionName = ws.Cell(i, 5).GetString() ?? string.Empty;
                    dto.CHR_Phanloai = ws.Cell(i, 6).GetString() ?? string.Empty;
                    dto.CHR_MaThietBi = ws.Cell(i, 7).GetString() ?? string.Empty;
                    dto.CHR_MaHangNoiBo = ws.Cell(i, 8).GetString() ?? "";
                    dto.CHR_MaHangNCC = ws.Cell(i, 9).GetString() ?? string.Empty;
                    dto.NVCHR_NameVN = ws.Cell(i, 10).GetString() ?? string.Empty;
                    dto.CHR_NameEN = ws.Cell(i, 11).GetString() ?? string.Empty;
                    dto.INT_SoLuong = ConvertHelper.ParseDouble(ws.Cell(i, 12).GetString());
                    dto.NVCHR_DonVi = ws.Cell(i, 13).GetString() ?? string.Empty;
                    dto.NVCHR_ChungLoai = ws.Cell(i, 14).GetString() ?? string.Empty;
                    dto.NVCHR_HinhDang = ws.Cell(i, 15).GetString() ?? string.Empty;
                    dto.NVCHR_ChatLieu = ws.Cell(i, 16).GetString() ?? string.Empty ;
                    dto.NVCHR_ThanhPhan = ws.Cell(i, 17).GetString() ?? string.Empty;
                    dto.NVCHR_KichThuoc = ws.Cell(i, 18).GetString() ?? string.Empty;
                    dto.NVCHR_DongMay = ws.Cell(i, 19).GetString() ?? string.Empty;
                    dto.NVCHR_TinhNang = ws.Cell(i, 20).GetString() ?? string.Empty;
                    dto.NVCHR_Rohs = ws.Cell(i, 21).GetString() ?? string.Empty;
                    dto.NVCHR_COCQ = ws.Cell(i, 22).GetString() ?? string.Empty;
                    dto.NVCHR_MSDS = ws.Cell(i, 23).GetString() ?? string.Empty;
                    dto.NVCHR_AnToan = ws.Cell(i, 24).GetString() ?? string.Empty;
                    dto.NVCHR_FileThietKe = ws.Cell(i, 25).GetString() ?? string.Empty;
                    dto.NVCHR_NhaSanXuat = ws.Cell(i, 26).GetString() ?? string.Empty;
                    dto.CHR_MaNCC = ws.Cell(i, 27).GetString() ?? string.Empty;
                    dto.NVCHR_TenNCC = ws.Cell(i, 28).GetString() ?? string.Empty;
                    dto.BIT_LayBaoGia = ConvertHelper.ParseBool(ws.Cell(i, 29).GetString());
                    dto.NVCHR_LyDo = ws.Cell(i, 30).GetString() ?? string.Empty;
                    dto.DTM_NgayMuonNhan = ConvertHelper.ParseDate(ws.Cell(i, 31).GetString());
                    dto.DTM_KyHan = ConvertHelper.ParseDate(ws.Cell(i, 32).GetString());
                    dto.CHR_Gap = ws.Cell(i, 33).GetString() == "X" ? "false" : "true";
                    dto.NVCHR_UserRequest = ws.Cell(i, 34).GetString() ?? GetCurrentUserId() ?? string.Empty;
                    dto.CHR_CreateBy = GetCurrentUserId() ?? string.Empty;
                    dto.DTM_UpdateLater = DateTime.Now;

                    listRequest.Add(dto);
                }

                if (!listRequest.Any()) return BadRequest("Không có dữ liệu hợp lệ để cập nhật");

                // check điều kiện update đơn
                var listMa = listRequest
                    .Select(r => r.CHR_MaDon)
                    .Where(ma => !string.IsNullOrWhiteSpace(ma))
                    .Select(ma => ma!)
                    .Distinct()
                    .ToList();
                var checkUpdate = await _baoGiaService.CheckDonReturnAsync(listMa);
                if (!checkUpdate.Data)
                {
                    return BadRequest("Không thể cập nhật đơn vì có đơn đã được phê duyệt");
                }
                // Call service to update list of requests
                var resultUpdate = await _baoGiaService.UpdateThongTinLichSuBaoGiaAsync(listRequest);
                if (!resultUpdate.Success)
                {
                    return BadRequest(resultUpdate.Message);
                }
                return Ok(resultUpdate.Data);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }
        // update thong tin nguoi phe duyet
        [HttpPost]
        public async Task<IActionResult> UpdateUserApprovalHistory([FromBody] UpdateHistoryResult vm)
        {
            if (vm == null)
            {
                return BadRequest("Data error");
            }
            try
            {
                // cập nhật thông tin người phê duyệt
                var result = await _baoGiaService.UpdateUserApprovalHistory(vm);
                if (!result.Success)
                {
                    return BadRequest("Error: " + result.Message);
                }
                var currentUserId = GetCurrentUserId();
                // Gui mail phe duyet trong background
                var SectionApporve = result.Data
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
                                    await sendMailService.SendMailAsync(item.CHR_UserApproval + "@brothergroup.net", currentUserId + "@brothergroup.net", 11, "ApprovalQuote/Index", item.CHR_Gap == "false" ? false : true, item.CHR_SectionCode ?? "", item.CHR_MaDon ?? "", currentUserId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail phê duyệt");
                            }
                        }
                    });
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }
        // Lấy dữ liệu màn hình lịch sử báo giá theo ID yêu cầu báo giá
        [HttpPost]
        public async Task<IActionResult> GetHistoryDataByID([FromBody] int idRequest)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetByRequestQuoteIdAsync(idRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc dữ liệu: {ex.Message}");
            }
        }
        // Lấy dữ liệu chi tiết theo số đơn
        [HttpPost]
        public async Task<IActionResult> GetHistoryDataBySoDon([FromBody] string soDon)
        {
            try
            {
                var result = await _baoGiaHistoryService.SearchBySoDonAsync(soDon);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc dữ liệu: {ex.Message}");
            }
        }
        // Delete theo ID request
        [HttpPost]
        public async Task<IActionResult> DeleteDanhSachBaoGiaByID([FromBody] DeleteQuotationByIdModel deleteQuotation)
        {
            var userRequest = GetCurrentUserId() ?? "";
            var result = await _baoGiaService.DeleteDonBaoGiaAsync(deleteQuotation.id, deleteQuotation.reason, userRequest);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // Delete danh sách báo giá theo mã đơn
        [HttpPost]
        public async Task<IActionResult> DeleteDanhSachBaoGiaByMaDon([FromBody] DeleteQuotationByMaDonModel deleteQuotation)
        {
            var userRequest = GetCurrentUserId() ?? "";
            var role = await _tmUserService.GetRoleAsync(GetCurrentUserId());
            var result = await _baoGiaService.DeleteDonXinBaoGiaAsync(deleteQuotation.maDon, deleteQuotation.reason, userRequest, role.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // tìm kiếm chi tiết đơn báo giá theo mã đơn
        [HttpPost]
        public async Task<IActionResult> GetByMaBaoGia([FromBody] string maDon)
        {
            var result = await _baoGiaService.GetByMaBaoGiaAsync(maDon);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // Return PIC section
        [HttpPost]
        public async Task<IActionResult> ReturnQuotation([FromBody] ReturnQuotationByMaDonModel vm)
        {
            if (string.IsNullOrEmpty(vm.maDon)) return BadRequest("Vui lòng chọn mã đơn!");
            try
            {
                var user = GetCurrentUserId();
                var result = await _baoGiaService.TraLaiDonBaoGiaAsync(vm.maDon, user, vm.reason);
                if (!result.Success || result.Data == null)
                {
                    return BadRequest("Error: " + result.Message);
                }
                var firstData = result.Data.FirstOrDefault();
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                            await sendMailService.SendMailAsync(firstData?.CHR_CreateBy + "@brothergroup.net", user + "@brothergroup.net", 12, "History/HistoryQuote", firstData?.CHR_Gap == "false" ? false : true, firstData?.CHR_SectionCode ?? "", firstData?.CHR_MaDon ?? "", user);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi gửi mail phê duyệt");
                        }
                    }
                });
                return Ok(result.Data);

            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

        }
        [HttpPost]
        public async Task<IActionResult> GetListApprovel([FromBody] SearchApprovalModel sr)
        {
            var result = await _approverService.GetApproverByStepAndSectionAsync(sr.Step ?? 2, sr.SectionCost ?? "");
            if (!result.Success)
            {
                return BadRequest("Error list Approver: " + result.Message);
            }
            return Ok(result.Data);
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
        // tìm kiếm thông tin đơn hàng, theo mã đơn, mã hàng nội bộ
        [HttpPost]
        public async Task<IActionResult> SearchOrderInfo([FromBody] SearchHistoryInfoByMaDonModel vm)
        {
            if(vm == null)
            {
                return BadRequest("Data error");
            }

            var result = await _baoGiaService.GetOrderInfoAsync(vm.MaDon, vm.MaHangNCC, vm.MaHang, vm.NameEn);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        // update bao gia theo id
        [HttpPost]
        public async Task<IActionResult> UpdateBaoGiaById([FromBody] BaoGia_Request_of_QuotationDTO baogia)
        {
            if (baogia == null)
            {
                return BadRequest("Error data update");
            }
            //
            var user = GetCurrentUserId();
            var roleAsync = await _tmUserService.GetRoleAsync(user);
            var role = roleAsync.Success ? roleAsync.Data : string.Empty;

            if(role != "UserPUR")
            {
                // Check trạng thái đơn
                var checkStatus = await _baoGiaService.CheckStepAsync([baogia.ID], [1, 2, 3, 4, 5]);

                if (!checkStatus.Success || checkStatus.Data.Count > 0)
                {
                    return BadRequest("Đơn đã được phê duyệt hoặc không hợp lệ để cập nhật");
                }
            } 

            baogia.DTM_UpdateLater = DateTime.Now;
            var result = await _baoGiaService.CapNhatDonBaoGiaAsync(baogia);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            try
            {
                var insertedList = result.Data;
                var histories = new BaoGia_History_Request_of_QuotationDTO
                {
                    ID_RequestQuote = insertedList.ID,
                    CHR_MaDon = insertedList.CHR_MaDon ?? string.Empty,
                    CHR_UpdateBy = user ?? string.Empty,
                    NVCHR_UpdateName = GetCurrentUserFullName() ?? string.Empty,
                    CHR_Updatedate = DateTime.Now,
                    CHR_ChangedColumns = null,
                    CHR_OldData = null,
                    CHR_NewData = System.Text.Json.JsonSerializer.Serialize(insertedList),
                    NVCHR_LyDo = insertedList.NVCHR_LyDo,
                    CHR_ActionType = "UPDATE"
                };

                if (histories != null)
                {
                    await _baoGiaHistoryService.InsertHistoryAsync(histories);
                }
            }
            catch
            {
                return BadRequest("Lỗi ghi lịch sử cập nhật báo giá");
            }
            return Ok(result.Data);
        }

        // MARK: Lấy các thông tin
        private async Task<List<string>> LoadCategoryDataAsync()
        {
            var CategoryS = await _tmCategoryService.GetListCategory();
            return CategoryS.Data ?? new List<string>();
        }
        private async Task<List<DEPARTMENTDTO>> LoadNhomViTriDataAsync()
        {
            //var nhomViTri = await _nhomViTriService.GetAllNhomViTriAsync();
            var nhomViTri = await _deparmentService.GetNhomViTriByDepartmentIdAsync(GetCurrentUserId() ?? "");
            return nhomViTri.Data ?? new List<DEPARTMENTDTO>();
        }
        private async Task<List<IM_NCC_NEWDTO>> LoadNhaCungCapDataAsync()
        {
            var nccNews = await _tmNccNewService.GetAllNccNew();
            return nccNews.Data ?? new List<IM_NCC_NEWDTO>();
        }
        private async Task<List<string>> LoadMadonAsync(int step)
        {
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "", step);
            return madons.Data ?? new List<string>();
        }
        // MARK: - HistoryQuote
        public async Task<IActionResult> HistoryQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccNews = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var statusData = await _baoGiaStatusService.GetListStatusAsync();
            var madons = await LoadMadonAsync(14);
            var role = GetRolesUser();
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                DanhSachCategory = categorys,
                DanhSachStatus = statusData.Data ?? new List<BaoGia_StatusDTO>(),
                NguoiThaoTac = GetCurrentUserId() ?? "",
                Role = role
            };

            return View(vm);
        }



        // MARK: - Index
        public async Task<IActionResult> Index()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccNews = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var statusData = await _baoGiaStatusService.GetListStatusAsync();
            var madons = await LoadMadonAsync(14);
            var role = GetRolesUser();
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                DanhSachCategory = categorys,
                DanhSachStatus = statusData.Data ?? new List<BaoGia_StatusDTO>(),
                NguoiThaoTac = GetCurrentUserId() ?? "",
                Role = role
            };

            return View(vm);
        }

        // Search của màn hình Index
        [HttpPost]
        public async Task<IActionResult> SearchHistory([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetHistoryAsync(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    searchModel.TrangThai,
                    GetCurrentUserId(),
                    searchModel.PageIndex,
                    searchModel.PageSize,
                    searchModel.to,
                    searchModel.from,
                    searchModel.ChungLoai
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }
        }
        // Lấy dữ liệu bảng Tab1
        [HttpPost]
        public async Task<IActionResult> GetHistoryTab1([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetWaitingForSupplier(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    GetCurrentUserId()
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }
        }
        // Lấy dữ liệu bảng Tab2
        [HttpPost]
        public async Task<IActionResult> GetCountQuotation([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetCountQuotation(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    GetCurrentUserId()
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }
        }
        // Lấy dữ liệu bảng Tab3
        [HttpPost]
        public async Task<IActionResult> GetCountStatus([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetCountStatus(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    GetCurrentUserId()
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }
        }
        // Lấy dữ liệu bảng Tab4
        [HttpPost]
        public async Task<IActionResult> GetProcessingStatus([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                var result = await _baoGiaHistoryService.GetProcessingStatus(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    GetCurrentUserId()
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }
        }
        // Xuất dữ liệu excel table màn hình Index
        [HttpPost]
        public async Task<IActionResult> ExportHistoryExcel([FromBody] SearchBaoGiaViewModel searchModel)
        {
            try
            {
                searchModel ??= new SearchBaoGiaViewModel();

                var result = await _baoGiaHistoryService.GetHistoryAsync(
                    searchModel.MaDon,
                    searchModel.MaNcc,
                    searchModel.Section,
                    searchModel.NguoiYeuCau,
                    searchModel.MaHang,
                    searchModel.TrangThai,
                    GetCurrentUserId(),
                    -1,
                    -1,
                    searchModel.to,
                    searchModel.from,
                    searchModel.ChungLoai
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                var stepByRole = GetRolesUser() == "UserPUR" ? 8 : 12;

                var rows = result.Data?.Data?.ToList() ?? new List<dynamic>();

                static IDictionary<string, object?> ToDictionary(dynamic item)
                {
                    if (item is IDictionary<string, object> dict)
                    {
                        return dict.ToDictionary(k => k.Key, v => (object?)v.Value, StringComparer.OrdinalIgnoreCase);
                    }

                    var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    if (item == null) return result;

                    foreach (var prop in item.GetType().GetProperties())
                    {
                        result[prop.Name] = prop.GetValue(item);
                    }

                    return result;
                }

                static string GetString(IDictionary<string, object?> item, params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (item.TryGetValue(key, out var value) && value != null)
                        {
                            var text = Convert.ToString(value)?.Trim();
                            if (!string.IsNullOrEmpty(text)) return text;
                        }
                    }

                    return string.Empty;
                }

                static int GetInt(IDictionary<string, object?> item, params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (item.TryGetValue(key, out var value) && value != null)
                        {
                            if (value is int i) return i;
                            if (int.TryParse(Convert.ToString(value), out var parsed)) return parsed;
                        }
                    }
                    return 0;
                }

                static DateTime? GetDateTime(IDictionary<string, object?> item, params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (!item.TryGetValue(key, out var value) || value == null) continue;

                        if (value is DateTime dt) return dt;
                        if (DateTime.TryParse(Convert.ToString(value), out var parsed)) return parsed;
                    }

                    return null;
                }
                static string BuildStatusText(
                    string lang,
                    int step,
                    bool isAllRefuse,
                    string defaultVi,
                    string defaultEn,
                    string defaultJa,
                    string purStatus,
                    string accStatus,
                    string shipStatus)
                {
                    if (step == 12)
                    {
                        if (string.Equals(purStatus, "Confirming", StringComparison.OrdinalIgnoreCase))
                        {
                            return lang switch
                            {
                                "en" => "Waiting for PUR name confirmation",
                                "ja" => "PURによる品名確認待ち",
                                _ => "Chờ xác nhận tên của PUR"
                            };
                        }

                        if (string.Equals(accStatus, "Confirming", StringComparison.OrdinalIgnoreCase))
                        {
                            return lang switch
                            {
                                "en" => "Waiting for Department confirmation",
                                "ja" => "部門による品名確認待ち",
                                _ => "Chờ xác nhận tên của phòng ban"
                            };
                        }

                        if (string.Equals(shipStatus, "Confirming", StringComparison.OrdinalIgnoreCase))
                        {
                            return lang switch
                            {
                                "en" => "Waiting for Ship confirmation",
                                "ja" => "Shipによる品名確認待ち",
                                _ => "Chờ xác nhận tên của Ship"
                            };
                        }

                        return lang switch
                        {
                            "en" => defaultEn,
                            "ja" => defaultJa,
                            _ => defaultVi
                        };
                    }

                    if (isAllRefuse)
                    {
                        return lang switch
                        {
                            "en" => "All suppliers refused to provide a quote.",
                            "ja" => "すべてのサプライヤーが見積もり提供を拒否した。",
                            _ => "Toàn bộ các NCC đã từ chối báo giá"
                        };
                    }

                    return lang switch
                    {
                        "en" => defaultEn,
                        "ja" => defaultJa,
                        _ => defaultVi
                    };
                }
                static bool? GetBool(IDictionary<string, object?> item, params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (!item.TryGetValue(key, out var value)) continue;

                        if (value == null) return null;

                        try
                        {
                            if (value is bool b) return b;
                            if (value is byte bt) return bt == 1;
                            if (value is short s) return s == 1;
                            if (value is int i) return i == 1;

                            var text = Convert.ToString(value)?.Trim();
                            if (string.IsNullOrEmpty(text)) continue;
                            if (text == "1") return true;
                            if (text == "0") return false;
                            if (bool.TryParse(text, out var parsed)) return parsed;
                        }
                        catch
                        {
                        }
                    }

                    return false;
                }

                static string BuildApprovalText(
                    string approver,
                    DateTime? approvedAt,
                    string? userNext,
                    int cellStep,
                    int currentStep)
                {

                    if (cellStep < currentStep)
                    {
                        if (!string.IsNullOrWhiteSpace(approver))
                        {
                            return approvedAt.HasValue
                                ? $"{approver}\n{approvedAt.Value:dd/MM/yyyy HH:mm}"
                                : approver;
                        }
                    }else if (cellStep == currentStep)
                    {
                        return string.IsNullOrWhiteSpace(userNext) ? string.Empty : userNext;
                    }

                    return string.Empty;
                }

                var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "vi";

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("History");

                const int totalColumns = 24;
                const int headerRow1 = 1;
                const int headerRow2 = 2;
                const int dataStartRow = 3;

                var baseHeaderColor = XLColor.FromHtml("#DDDDDD");
                var group1Color = XLColor.FromHtml("#E7B38C");
                var group2Color = XLColor.FromHtml("#9FB6C8");
                var group3Color = XLColor.FromHtml("#CFE3C6");
                var group4Color = XLColor.FromHtml("#FFF200");
                var overdueColor = XLColor.FromHtml("#E74C3C");

                string L(string key, string fallback)
                {
                    var val = _localizer[key].Value;
                    return string.IsNullOrWhiteSpace(val) || val == key ? fallback : val;
                }

                bool IsRefuse(string s) => (s ?? "").Trim().ToLower() == "refuse";

                ws.Cell(headerRow1, 1).Value = L("No", "No");
                ws.Cell(headerRow1, 2).Value = L("OrderNo", "Order No");
                ws.Cell(headerRow1, 3).Value = L("InternalCode", "Internal Code");
                ws.Cell(headerRow1, 4).Value = "Vendor's good code";
                ws.Cell(headerRow1, 5).Value = "Part name (English)";
                ws.Cell(headerRow1, 6).Value = "Category";
                ws.Cell(headerRow1, 7).Value = L("Supplier1", "Supplier 1");
                ws.Cell(headerRow1, 8).Value = L("Supplier2", "Supplier 2");
                ws.Cell(headerRow1, 9).Value = L("Supplier3", "Supplier 3");
                ws.Cell(headerRow1, 10).Value = L("Supplier4", "Supplier 4");
                ws.Cell(headerRow1, 11).Value = L("Supplier5", "Supplier 5");
                ws.Cell(headerRow1, 12).Value = L("ReasonForSupplierSelection", "Reason for supplier selection");
                ws.Cell(headerRow1, 13).Value = "Price (USD)";
                ws.Cell(headerRow1, 14).Value = L("SelectionDeadline", "Selection deadline");
                ws.Cell(headerRow1, 15).Value = L("RequesterUser", "Requester user");

                ws.Range(headerRow1, 16, headerRow1, 19).Merge().Value = L("ApprovalOrderGroup", "Approval order");
                ws.Range(headerRow1, 20, headerRow1, 23).Merge().Value = L("ApprovalSupplierGroup", "Approval supplier");
                ws.Cell(headerRow1, 24).Value = L("Status", "Status");

                ws.Cell(headerRow2, 16).Value = "QLSC";
                ws.Cell(headerRow2, 17).Value = "QLTC";
                ws.Cell(headerRow2, 18).Value = "PUR PIC";
                ws.Cell(headerRow2, 19).Value = "PUR QLSC";

                ws.Cell(headerRow2, 20).Value = "PUR PIC";
                ws.Cell(headerRow2, 21).Value = "QLSC";
                ws.Cell(headerRow2, 22).Value = "QLTC";
                ws.Cell(headerRow2, 23).Value = "QLCC";

                for (var col = 1; col <= 15; col++)
                {
                    ws.Range(headerRow1, col, headerRow2, col).Merge();
                }
                ws.Range(headerRow1, 24, headerRow2, 24).Merge();
                ws.Range(headerRow1, 1, headerRow2, 15).Style.Fill.BackgroundColor = baseHeaderColor;
                ws.Range(headerRow1, 16, headerRow2, 19).Style.Fill.BackgroundColor = group1Color;
                ws.Range(headerRow1, 20, headerRow2, 23).Style.Fill.BackgroundColor = group2Color;
                ws.Range(headerRow1, 24, headerRow2, 24).Style.Fill.BackgroundColor = group4Color;

                var headerRange = ws.Range(headerRow1, 1, headerRow2, totalColumns);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                for (var i = 0; i < rows.Count; i++)
                {
                    var excelRow = dataStartRow + i;
                    var item = ToDictionary(rows[i]);
                    var supplier1 = GetString(item, "NCC_1");
                    var supplier2 = GetString(item, "NCC_2");
                    var supplier3 = GetString(item, "NCC_3");
                    var supplier4 = GetString(item, "NCC_4");
                    var supplier5 = GetString(item, "NCC_5");

                    var bitNcc1 = GetBool(item, "BitNCC_1");
                    var bitNcc2 = GetBool(item, "BitNCC_2");
                    var bitNcc3 = GetBool(item, "BitNCC_3");
                    var bitNcc4 = GetBool(item, "BitNCC_4");
                    var bitNcc5 = GetBool(item, "BitNCC_5");

                    var status1 = GetString(item, "Status_1");
                    var status2 = GetString(item, "Status_2");
                    var status3 = GetString(item, "Status_3");
                    var status4 = GetString(item, "Status_4");
                    var status5 = GetString(item, "Status_5");

                    // count sl nha tu choi
                    var allRefuse = new[]
                    {
                        IsRefuse(status1) ,
                        IsRefuse(status2),
                        IsRefuse(status3) ,
                        IsRefuse(status4),
                        IsRefuse(status5)
                    }.Count(a => a == true);

                    // count sl nhà cung cấp

                    int countSL = new[]
                    {
                        supplier1,
                        supplier2,
                        supplier3,
                        supplier4,
                        supplier5
                    }.Count(s => !string.IsNullOrWhiteSpace(s));

                    var isAllRefuse = countSL == allRefuse;
                    ws.Cell(excelRow, 1).Value = i + 1;
                    ws.Cell(excelRow, 2).Value = GetString(item, "CHR_MaDon");
                    ws.Cell(excelRow, 3).Value = GetString(item, "CHR_MaHangNoiBo");
                    ws.Cell(excelRow, 4).Value = GetString(item, "CHR_MaHangNCC");
                    ws.Cell(excelRow, 5).Value = GetString(item, "CHR_NameEN");
                    ws.Cell(excelRow, 6).Value = GetString(item, "NVCHR_ChungLoai");

                    var deadline = GetDateTime(item, "DTM_KyHan");
                    var isOverdue = deadline.HasValue && deadline.Value.Date < DateTime.Today;

                    var step = GetInt(item, "Step", "INT_Step");
                    var selectedSupplier = GetString(item, "NCC_DuocChon");

                    void ApplySupplierStyle(int col, string supplierText, object bitValue, string statusText)
                    {
                        var cell = ws.Cell(excelRow, col);
                        cell.Value = supplierText ?? "";

                        if (step < 5 || bitValue == null)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.White;
                            return;
                        }

                        // RULE 1: ALL REFUSE -> đỏ
                        if (isAllRefuse)
                        {
                            cell.Style.Fill.BackgroundColor = overdueColor;
                            cell.Style.Font.FontColor = XLColor.White;
                            return;
                        }

                        // RULE 2
                        var raw = Convert.ToString(bitValue)?.Trim().ToLower();
                        var isRefuse = (statusText ?? "").Trim().ToLower() == "refuse";
                        var isValue = string.IsNullOrEmpty(supplierText);

                        var isSelected =
                            (bitValue is bool b && b) ||
                            (bitValue is int i && i == 1) ||
                            raw == "1" ||
                            raw == "true";

                        if (isSelected && !isRefuse)
                        {
                            cell.Style.Fill.BackgroundColor = group3Color; // xanh
                        }
                        else if (!isValue)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                        }
                        else
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.White;
                        }

                        var isPickedSupplier =
                            step > stepByRole
                            && !string.IsNullOrWhiteSpace(selectedSupplier)
                            && !string.IsNullOrWhiteSpace(supplierText)
                            && string.Equals(supplierText.Trim(), selectedSupplier.Trim(), StringComparison.OrdinalIgnoreCase);

                        if (isPickedSupplier)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#CFE2FF");
                            cell.Style.Font.FontColor = XLColor.FromHtml("#0D6EFD");
                        }
                    }
                    ApplySupplierStyle(7, supplier1, bitNcc1, status1);
                    ApplySupplierStyle(8, supplier2, bitNcc2, status2);
                    ApplySupplierStyle(9, supplier3, bitNcc3, status3);
                    ApplySupplierStyle(10, supplier4, bitNcc4, status4);
                    ApplySupplierStyle(11, supplier5, bitNcc5, status5);

                    ws.Cell(excelRow, 12).Value = GetString(item, "NVCHR_ReasonPick");

                    var price = GetString(item, "FL_USD");

                    if (string.IsNullOrWhiteSpace(price) || step < 11)
                    {
                        ws.Cell(excelRow, 13).Value = "";
                    }
                    else if (decimal.TryParse(price, out decimal value))
                    {
                        ws.Cell(excelRow, 13).Value = $"{value:0.####} USD";
                    }

                    if (deadline.HasValue)
                    {
                        ws.Cell(excelRow, 14).Value = deadline.Value;
                        ws.Cell(excelRow, 14).Style.DateFormat.Format = "dd/MM/yyyy";
                        if (isOverdue)
                        {
                            ws.Cell(excelRow, 14).Style.Fill.BackgroundColor = overdueColor;
                            ws.Cell(excelRow, 14).Style.Font.FontColor = XLColor.White;
                        }
                    }

                    ws.Cell(excelRow, 15).Value = GetString(item, "CHR_CreateBy");

                    var approvals = new[]
                    {
                        new { Approver = GetString(item, "QLSC_Approve"),      Time = GetDateTime(item, "QLSC_Time"),          Step = 2 },
                        new { Approver = GetString(item, "QLTC_Approve"),      Time = GetDateTime(item, "QLTC_Time"),          Step = 3 },
                        new { Approver = GetString(item, "PIC_Approve"),       Time = GetDateTime(item, "PIC_Time"),           Step = 4 },
                        new { Approver = GetString(item, "QLSC1_Approve"),     Time = GetDateTime(item, "QLSC1_Time"),         Step = 5 },
                        new { Approver = GetString(item, "PIC_PickNCC"),       Time = GetDateTime(item, "PIC_PickNCC_Time"),   Step = 7 },
                        new { Approver = GetString(item, "QLSC_PickNCC"),      Time = GetDateTime(item, "QLSC_PickNCC_Time"),  Step = 9 },
                        new { Approver = GetString(item, "QLTC_PickNCC"),      Time = GetDateTime(item, "QLTC_PickNCC_Time"),  Step = 10 },
                        new { Approver = GetString(item, "DEFT_PickNCC"),      Time = GetDateTime(item, "DEFT_PickNCC_Time"),  Step = 11 }
                    };

                    for (var offset = 0; offset < approvals.Length; offset++)
                    {
                        var approval = approvals[offset];
                        var col = 16 + offset;

                        var cellValue = BuildApprovalText(
                            approval.Approver,
                            approval.Time,
                            GetString(item, "UserNext"),
                            approval.Step,
                            step);

                        ws.Cell(excelRow, col).Value = cellValue;

                        if (approval.Step < step &&
                            !string.IsNullOrWhiteSpace(approval.Approver))
                        {
                            ws.Cell(excelRow, col).Style.Fill.BackgroundColor = group3Color;
                        }
                    }
                    var statusText = BuildStatusText(
                        currentLang,
                        step,
                        isAllRefuse,
                        GetString(item, "CHR_StepName"),
                        GetString(item, "CHR_StepNameEN", "CHR_StepName"),
                        GetString(item, "CHR_StepNameJP", "CHR_StepName"),
                        GetString(item, "CHR_Status"),
                        GetString(item, "CHR_StatusACC"),
                        GetString(item, "CHR_StatusShip")
                    );
                    ws.Cell(excelRow, 24).Value = statusText;
                    ws.Cell(excelRow, 24).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF9CC");
                }

                var lastRow = Math.Max(dataStartRow, dataStartRow + rows.Count - 1);
                var tableRange = ws.Range(dataStartRow, 1, lastRow, totalColumns);
                tableRange.Style.Alignment.WrapText = true;
                tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 6;
                ws.Column(2).Width = 16;
                ws.Column(3).Width = 16;
                ws.Column(4).Width = 18;
                ws.Column(5).Width = 30;
                ws.Column(6).Width = 16;
                ws.Column(7).Width = 14;
                ws.Column(8).Width = 14;
                ws.Column(9).Width = 14;
                ws.Column(10).Width = 14;
                ws.Column(11).Width = 28;
                ws.Column(12).Width = 15;
                ws.Column(13).Width = 14;
                ws.Column(14).Width = 16;
                ws.Column(15).Width = 20;
                ws.Column(16).Width = 20;
                ws.Column(17).Width = 20;
                ws.Column(18).Width = 20;
                ws.Column(19).Width = 20;
                ws.Column(20).Width = 20;
                ws.Column(21).Width = 20;
                ws.Column(22).Width = 20;
                ws.Column(23).Width = 20;
                ws.Column(24).Width = 20;

                ws.Row(headerRow1).Height = 24;
                ws.Row(headerRow2).Height = 24;
                ws.SheetView.FreezeRows(2);
                ws.Range(1, 1, lastRow, totalColumns).SetAutoFilter();

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"HistoryQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }
        // Xuất file quản lý màn hình Index bằng MiniExcel
        [HttpPost]
        public async Task<IActionResult> ExportManagerHistoryIndex([FromBody] SearchBaoGiaViewModel searchModel)
        {
            // Lấy thông tin dữ liệu lịch sử báo giá theo điều kiện tìm kiếm
            var result = await _baoGiaService.ExportHistoryBaoGiaAsync(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai, searchModel.to, searchModel.from);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var stepByRole = GetRolesUser() == "UserPUR" ? 8 : 12;
            try
            {
                var historyData = result.Data;
                if (historyData == null || !historyData.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }


                // Lấy thông tin status
                var statusResult = await _baoGiaStatusService.GetListStatusAsync();
                if (statusResult == null || !statusResult.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }

                // Lấy thông tin step
                var stepsResult = await _baoGiaStepService.GetAll();
                if (stepsResult == null || !stepsResult.Success)
                {
                    return BadRequest("Lỗi lấy danh sách step");
                }

                var statusMap = statusResult.Data?
                    .Where(s => !string.IsNullOrWhiteSpace(s.VCHR_CodeStatus))
                    .GroupBy(s => s.VCHR_CodeStatus)
                    .ToDictionary(g => g.Key!, g => g.First().NVCHR_TenStatus ?? string.Empty)
                    ?? new Dictionary<string, string>();

                var stepMap = stepsResult.Data?
                    .Where(s => s.INT_StepNumber.HasValue)
                    .GroupBy(s => s.INT_StepNumber!.Value)
                    .ToDictionary(g => g.Key, g => g.First().CHR_StepName ?? string.Empty)
                    ?? new Dictionary<int, string>();

                static bool IsReturnStatus(string? statusCode)
                    => !string.IsNullOrWhiteSpace(statusCode)
                       && statusCode.IndexOf("RETURN", StringComparison.OrdinalIgnoreCase) >= 0;

                static string ToDateString(DateTime? date)
                    => date?.ToString("dd/MM/yyyy") ?? string.Empty;

                string GetStatusName(string? statusCode)
                    => (!string.IsNullOrWhiteSpace(statusCode) && statusMap.TryGetValue(statusCode, out var statusName))
                        ? statusName
                        : string.Empty;

                string GetStepName(object? stepValue)
                {
                    if (stepValue == null) return string.Empty;

                    try
                    {
                        var stepNumber = Convert.ToInt32(stepValue);
                        return stepMap.TryGetValue(stepNumber, out var stepName) ? stepName : string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                static bool IsStepLessOrEqualSix(object? stepValue)
                {
                    if (stepValue == null) return false;

                    try
                    {
                        return Convert.ToInt32(stepValue) <= 6;
                    }
                    catch
                    {
                        return false;
                    }
                }

                static string GetSelectMark(object? selectValue, object? stepValue)
                {
                    bool? isSelected = null;

                    try
                    {
                        if (selectValue != null)
                        {
                            isSelected = Convert.ToBoolean(selectValue);
                        }
                    }
                    catch
                    {
                        isSelected = null;
                    }

                    if (isSelected == null || (isSelected == false && IsStepLessOrEqualSix(stepValue)))
                    {
                        return string.Empty;
                    }

                    return isSelected == true ? "O" : "X";
                }

                // Lấy các đơn trả về có trạng thái RETURN để lấy lý do trả
                var listReason = new List<ReasonQuotition>();
                var returnIds = historyData
                    .Where(rq => rq != null && IsReturnStatus(rq?.ID_Status))
                    .Select(rq => rq.ID)
                    .Distinct()
                    .ToList();

                if (returnIds.Any())
                {
                    var reasons = await _baoGiaHistoryService.GetReasonsAsync(returnIds);
                    if (!reasons.Success)
                    {
                        return BadRequest("Lỗi lấy lý do trả");
                    }
                    listReason = reasons.Data ?? new List<ReasonQuotition>();
                }

                var reasonMap = listReason
                    .GroupBy(r => r.Id)
                    .ToDictionary(g => g.Key, g => g.First().Reason ?? string.Empty);

                string GetReason(object? idValue, string? statusCode)
                {
                    if (!IsReturnStatus(statusCode) || idValue == null)
                    {
                        return string.Empty;
                    }

                    try
                    {
                        var id = Convert.ToInt32(idValue);
                        return reasonMap.TryGetValue(id, out var reason) ? reason : string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateExportHistoryNew.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateExportHistoryNew.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheet(1);
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                var mainRows = new List<object[]>(historyData.Count);
                int stt = 1;

                var allowShowFileGroups = historyData
                    .GroupBy(x => new
                    {
                        x.CHR_MaDon,
                        x.CHR_MaHangNoiBo
                    })
                    .Where(g => g.All(x => x.ID_StepBaoGia > 12))
                    .Select(g => $"{g.Key.CHR_MaDon}|{g.Key.CHR_MaHangNoiBo}")
                    .ToHashSet();

                foreach (var rq in historyData)
                {
                    if (rq == null) continue;

                    var groupKey = $"{rq.CHR_MaDon}|{rq.CHR_MaHangNoiBo}";
                    var canShowFile = allowShowFileGroups.Contains(groupKey);

                    var selectMark = GetSelectMark(rq.BIT_Select, rq.ID_StepBaoGia);

                    var isSelected = rq.BIT_Select == true;
                    mainRows.Add(new object[]
                    {
                        stt++,
                        rq.CHR_MaDon ?? string.Empty,
                        rq.ID,
                        rq.CHR_SectionCode ?? string.Empty,
                        rq.CHR_SectionName ?? string.Empty,
                        rq.CHR_Phanloai ?? string.Empty,
                        rq.CHR_MaThietBi ?? string.Empty,
                        rq.CHR_MaHangNoiBo ?? string.Empty,
                        rq.CHR_MaHangNCC ?? string.Empty,
                        rq.NVCHR_NameVN ?? string.Empty,
                        rq.CHR_NameEN ?? string.Empty,
                        rq.INT_SoLuong ?? 0,
                        rq.NVCHR_DonVi ?? string.Empty,
                        rq.NVCHR_ChungLoai ?? string.Empty,
                        rq.NVCHR_HinhDang ?? string.Empty,
                        rq.NVCHR_ChatLieu ?? string.Empty,
                        rq.NVCHR_ThanhPhan ?? string.Empty,
                        rq.NVCHR_KichThuoc ?? string.Empty,
                        rq.NVCHR_DongMay ?? string.Empty,
                        rq.NVCHR_TinhNang ?? string.Empty,
                        rq.NVCHR_Rohs ?? string.Empty,
                        rq.NVCHR_COCQ ?? string.Empty,
                        rq.NVCHR_MSDS ?? string.Empty,
                        rq.NVCHR_AnToan ?? string.Empty,
                        rq.NVCHR_FileThietKe ?? string.Empty,
                        rq.NVCHR_NhaSanXuat ?? string.Empty,
                        rq.CHR_MaNCC ?? string.Empty,
                        rq.ShortName?? rq.NVCHR_TenNCC ?? string.Empty,
                        rq.BIT_LayBaoGia == false ? "X" : "O",
                        rq.NVCHR_LyDo ?? string.Empty,
                        ToDateString(rq.DTM_NgayMuonNhan),
                        ToDateString(rq.DTM_KyHan),
                        rq.CHR_Gap == "false" ? "X" : "O",
                        rq.NVCHR_UserRequest ?? string.Empty,
                        GetStatusName(rq.ID_Status),
                        GetStepName(rq.ID_StepBaoGia),
                        GetReason(rq.ID, rq.ID_Status),
                        rq.ID_StepBaoGia > stepByRole ? selectMark : string.Empty,
                        rq.ID_StepBaoGia > stepByRole ? rq.NVCHR_ReasonPick ?? string.Empty : string.Empty,
                        canShowFile ? rq.NVCHR_File ?? string.Empty : string.Empty
                    });
                }

                ws.Cell(4, 1).InsertData(mainRows);

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"HistoryManagerQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }

        // Lấy thông tin lịch sử của đơn hàng
        [HttpPost]
        public async Task<IActionResult> GetHistoryApprover([FromBody] SearchHistoryInfoByMaDonModel searchModel)
        {
            if (searchModel == null)
            {
                return BadRequest("Dữ liệu tìm kiếm không hợp lệ");
            }

            try
            {
                var result = await _baoGiaHistoryService.GetOrderHistoryAsync(
                    searchModel.MaDon,
                    searchModel.MaHang,
                    searchModel.MaHangNCC
                );
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tìm kiếm: {ex.Message}");
            }

        }

        // Download thông tin file kết quả báo giá
        [HttpPost]
        public async Task<IActionResult> DownloadQuoteFile([FromBody] string urlFile)
        {
            if (string.IsNullOrEmpty(urlFile))
                return BadRequest("Không có đường dẫn file");

            try
            {
                var rqFile = await _fileImportService.GetFileToLinkAsync(urlFile);

                if (!rqFile.Success || rqFile.Data == null)
                    return BadRequest(urlFile);//+ rqFile.Message

                var file = rqFile.Data;

                return File(
                    file.OpenReadStream(),
                    file.ContentType ?? "application/octet-stream",
                    file.FileName
                );
            }
            catch (Exception ex)
            {
                return BadRequest("Chi tiết: " + ex.Message);
            }
        }
    }
}
