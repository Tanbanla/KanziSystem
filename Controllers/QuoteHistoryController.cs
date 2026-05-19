using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Globalization;
using Path = System.IO.Path;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [Route("QuoteHistory")]
    public class QuoteHistoryController : BaseAuthController
    {
        private readonly IConfiguration _configuration;
        private readonly IMaterialService _materialService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISendMailService _sendMailService;
        private readonly ILogger<QuoteHistoryController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<QuoteHistoryController> _localizer;

        public QuoteHistoryController(
            IConfiguration configuration,
            IMaterialService materialService,
            ITmNccNewService tmNccNewService,
            IDepartmentService deparmentService,
            ITmCategoryService tmCategoryService,
            IBaoGiaStatusService baoGiaStatusService,
            IBaoGiaService baoGiaService,
            IBaoGiaHistoryService baoGiaHistoryService,
            IBaoGiaStepService baoGiaStepService,
            IMasterApproverSendMailService approverService,
            IServiceScopeFactory serviceScopeFactory,
            ISendMailService sendMailService,
            ILogger<QuoteHistoryController> logger,
            IWebHostEnvironment env,
            IStringLocalizer<QuoteHistoryController> localizer)
        {
            _configuration = configuration;
            _materialService = materialService;
            _tmNccNewService = tmNccNewService;
            _deparmentService = deparmentService;
            _tmCategoryService = tmCategoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaService = baoGiaService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStepService = baoGiaStepService;
            _approverService = approverService;
            _serviceScopeFactory = serviceScopeFactory;
            _sendMailService = sendMailService;
            _logger = logger;
            _env = env;
            _localizer = localizer;
        }

        [HttpGet("HistoryQuote")]
        public async Task<IActionResult> HistoryQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccNews = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var statusData = await _baoGiaStatusService.GetListStatusAsync();
            var madons = await LoadMadonAsync(13);
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

            return View("~/Views/Quote/HistoryQuote/HistoryQuote.cshtml", vm);
        }

        [HttpPost("ExportHistory")]
        public async Task<IActionResult> ExportHistory([FromBody] SearchBaoGiaViewModel searchModel)
        {
            var result = await _baoGiaService.SearchAsync(
                searchModel.MaDon,
                searchModel.MaNcc,
                searchModel.Section,
                searchModel.NguoiYeuCau,
                searchModel.MaHang,
                searchModel.TrangThai,
                searchModel.Step,
                GetCurrentUserId() ?? "",
                0,
                0,
                searchModel.Date,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            try
            {
                var status = await _baoGiaStatusService.GetListStatusAsync();
                if (status == null || !status.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "HistoryQuote.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: HistoryQuote.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 7;
                foreach (var rq in result.Data.Data)
                {
                    int col = 1;
                    ws.Cell(row, col++).SetValue(row - 6);
                    ws.Cell(row, col++).SetValue(rq?.CHR_MaDon ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.ID);
                    ws.Cell(row, col++).SetValue(rq?.CHR_SectionCode ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_SectionName ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_Phanloai ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.INT_SoLuong.HasValue == true ? rq.INT_SoLuong.Value : 0);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_MaNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.BIT_LayBaoGia == false ? "X" : "O");
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_LyDo ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.DTM_NgayMuonNhan.HasValue == true ? rq.DTM_NgayMuonNhan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.DTM_KyHan.HasValue == true ? rq.DTM_KyHan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, col++).SetValue(rq?.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(row, col++).SetValue(rq?.CHR_CreateBy ?? string.Empty);
                    var history = await _baoGiaHistoryService.GetByRequestQuoteIdAsync(rq.ID);
                    var reson = history.Data.Capacity > 0 ? history.Data.OrderByDescending(h => h.CHR_Updatedate).FirstOrDefault()?.NVCHR_LyDo : string.Empty;
                    var statusName = status.Data.Where(s => s.VCHR_CodeStatus == rq.ID_Status).Select(s => s.NVCHR_TenStatus).FirstOrDefault() ?? string.Empty;
                    ws.Cell(row, col++).SetValue(statusName);
                    ws.Cell(row, col++).SetValue(reson);
                    row++;
                }

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

        [HttpPost("ExportManagerHistory")]
        public async Task<IActionResult> ExportManagerHistory([FromBody] SearchBaoGiaViewModel searchModel)
        {
            var result = await _baoGiaService.ExportHistoryBaoGiaAsync(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            try
            {
                var historyData = result.Data;
                if (historyData == null || !historyData.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }

                var status = await _baoGiaStatusService.GetListStatusAsync();
                if (status == null || !status.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }

                var steps = await _baoGiaStepService.GetAll();
                if (steps == null || !steps.Success)
                {
                    return BadRequest("Lỗi lấy danh sách step");
                }

                var listReason = new List<ReasonQuotition>();
                var returnIds = historyData
                    .Where(rq => rq != null && rq.ID_Status != null && rq.ID_Status.Contains("RETURN"))
                    .Select(rq => rq.ID)
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

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateExportHistoryNew.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateExportHistoryNew.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 4;
                int stt = 1;

                foreach (var rq in historyData)
                {
                    if (rq == null) continue;

                    int col = 1;
                    ws.Cell(row, col++).SetValue(stt++);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaDon ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.ID);
                    ws.Cell(row, col++).SetValue(rq.CHR_SectionCode ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_SectionName ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_Phanloai ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.INT_SoLuong ?? 0);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.BIT_LayBaoGia == false ? "X" : "O");
                    ws.Cell(row, col++).SetValue(rq.NVCHR_LyDo ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.DTM_NgayMuonNhan?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.DTM_KyHan?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(row, col++).SetValue(rq.NVCHR_UserRequest ?? string.Empty);

                    var reason = (rq.ID_Status != null && rq.ID_Status.Contains("RETURN"))
                        ? (listReason.FirstOrDefault(c => c.Id == rq.ID)?.Reason ?? "")
                        : "";

                    var statusName = status.Data?
                        .FirstOrDefault(s => s.VCHR_CodeStatus == rq.ID_Status)?
                        .NVCHR_TenStatus ?? string.Empty;

                    var stepName = steps.Data?
                        .FirstOrDefault(s => s.INT_StepNumber == rq.ID_StepBaoGia)?
                        .CHR_StepName ?? string.Empty;

                    ws.Cell(row, col++).SetValue(statusName);
                    ws.Cell(row, col++).SetValue(stepName);
                    ws.Cell(row, col++).SetValue(reason);
                    if (rq.BIT_Select == null)
                    {
                        ws.Cell(row, col++).SetValue("");
                    }
                    else
                    {
                        ws.Cell(row, col++).SetValue(rq.BIT_Select == true ? "O" : "X");
                    }

                    if (rq.BIT_Select == true)
                    {
                        ws.Cell(row, col++).SetValue(rq.NVCHR_ReasonPick ?? string.Empty);
                        ws.Cell(row, col++).SetValue(rq.NVCHR_File ?? string.Empty);
                    }
                    else
                    {
                        ws.Cell(row, col++).SetValue(string.Empty);
                        ws.Cell(row, col++).SetValue(string.Empty);
                    }

                    row++;
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

        [HttpPost("ImportFileExcelEditHistory")]
        public async Task<IActionResult> ImportFileExcelEditHistory([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Not data import file");
            var listRequest = new List<BaoGia_Request_of_QuotationDTO>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int i = startRow; i <= lastRow; i++)
                {
                    var maDon = ws.Cell(i, 2).GetString();
                    if (string.IsNullOrWhiteSpace(maDon)) break;

                    var dto = new BaoGia_Request_of_QuotationDTO();
                    try
                    {
                        dto.ID = ws.Cell(i, 3).GetValue<int>();
                    }
                    catch
                    {
                        continue;
                    }
                    dto.CHR_MaDon = maDon;
                    dto.CHR_SectionCode = ws.Cell(i, 4).GetString();
                    dto.CHR_SectionName = ws.Cell(i, 5).GetString();
                    dto.CHR_Phanloai = ws.Cell(i, 6).GetString();
                    dto.CHR_MaThietBi = ws.Cell(i, 7).GetString();
                    dto.CHR_MaHangNoiBo = ws.Cell(i, 8).GetString();
                    dto.CHR_MaHangNCC = ws.Cell(i, 9).GetString();
                    dto.NVCHR_NameVN = ws.Cell(i, 10).GetString();
                    dto.CHR_NameEN = ws.Cell(i, 11).GetString();
                    dto.INT_SoLuong = ParseDouble(ws.Cell(i, 12).GetString());
                    dto.NVCHR_DonVi = ws.Cell(i, 13).GetString();
                    dto.NVCHR_ChungLoai = ws.Cell(i, 14).GetString();
                    dto.NVCHR_HinhDang = ws.Cell(i, 15).GetString();
                    dto.NVCHR_ChatLieu = ws.Cell(i, 16).GetString();
                    dto.NVCHR_ThanhPhan = ws.Cell(i, 17).GetString();
                    dto.NVCHR_KichThuoc = ws.Cell(i, 18).GetString();
                    dto.NVCHR_DongMay = ws.Cell(i, 19).GetString();
                    dto.NVCHR_TinhNang = ws.Cell(i, 20).GetString();
                    dto.NVCHR_Rohs = ws.Cell(i, 21).GetString();
                    dto.NVCHR_COCQ = ws.Cell(i, 22).GetString();
                    dto.NVCHR_MSDS = ws.Cell(i, 23).GetString();
                    dto.NVCHR_AnToan = ws.Cell(i, 24).GetString();
                    dto.NVCHR_FileThietKe = ws.Cell(i, 25).GetString();
                    dto.NVCHR_NhaSanXuat = ws.Cell(i, 26).GetString();
                    dto.CHR_MaNCC = ws.Cell(i, 27).GetString();
                    dto.NVCHR_TenNCC = ws.Cell(i, 28).GetString();
                    dto.BIT_LayBaoGia = ParseBool(ws.Cell(i, 29).GetString());
                    dto.NVCHR_LyDo = ws.Cell(i, 30).GetString();
                    dto.DTM_NgayMuonNhan = ParseDate(ws.Cell(i, 31).GetString());
                    dto.DTM_KyHan = ParseDate(ws.Cell(i, 32).GetString());
                    dto.CHR_Gap = ws.Cell(i, 33).GetString() == "X" ? "false" : "true";
                    dto.NVCHR_UserRequest = ws.Cell(i, 34).GetString() ?? GetCurrentUserId() ?? string.Empty;
                    dto.CHR_CreateBy = GetCurrentUserId() ?? string.Empty;
                    dto.DTM_UpdateLater = DateTime.Now;

                    listRequest.Add(dto);
                }

                if (!listRequest.Any()) return BadRequest("Không có dữ liệu hợp lệ để cập nhật");

                var listMa = listRequest.Select(r => r.CHR_MaDon).Distinct().ToList();
                var checkUpdate = await _baoGiaService.CheckDonReturnAsync(listMa);
                if (!checkUpdate.Success || !checkUpdate.Data)
                {
                    return BadRequest("Không thể cập nhật đơn vì có đơn đang ở trạng thái RETURN");
                }

                var result = await _baoGiaService.UpdateThongTinLichSuBaoGiaAsync(listRequest);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpPost("SearchHistoryBaoGia")]
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
                searchModel.Date,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("GetByMaBaoGia")]
        public async Task<IActionResult> GetByMaBaoGia([FromBody] string maDon)
        {
            var result = await _baoGiaService.GetByMaBaoGiaAsync(maDon);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("GetHistoryDataByID")]
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

        [HttpPost("GetHistoryDataBySoDon")]
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

        [HttpPost("DeleteDanhSachBaoGiaByMaDon")]
        public async Task<IActionResult> DeleteDanhSachBaoGiaByMaDon([FromBody] DeleteQuotationByMaDonModel deleteQuotation)
        {
            var userRequest = GetCurrentUserId() ?? "";
            var result = await _baoGiaService.DeleteDonXinBaoGiaAsync(deleteQuotation.maDon, deleteQuotation.reason, userRequest);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("DeleteDanhSachBaoGiaByID")]
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

        [HttpPost("GetListApprovel")]
        public async Task<IActionResult> GetListApprovel([FromBody] SearchApprovalModel sr)
        {
            var result = await _approverService.GetApproverByStepAndSectionAsync(sr.Step ?? 2, sr.SectionCost ?? "");
            if (!result.Success)
            {
                return BadRequest("Error list Approver: " + result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("SearchID")]
        public async Task<IActionResult> SearchID([FromBody] int id)
        {
            var result = await _baoGiaService.GetByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("UpdateBaoGiaById")]
        public async Task<IActionResult> UpdateBaoGiaById([FromBody] BaoGia_Request_of_QuotationDTO baogia)
        {
            if (baogia.ID_StepBaoGia >= 6)
            {
                return BadRequest("Đơn đã phê duyệt không sửa");
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
                    CHR_UpdateBy = GetCurrentUserId() ?? string.Empty,
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

        [HttpPost("UpdateUserApprovalHistory")]
        public async Task<IActionResult> UpdateUserApprovalHistory([FromBody] UpdateHistoryResult vm)
        {
            if (vm == null)
            {
                return BadRequest("Data error");
            }
            try
            {
                var result = await _baoGiaService.UpdateUserApprovalHistory(vm);
                if (!result.Success)
                {
                    return BadRequest("Error: " + result.Message);
                }
                var currentUserId = GetCurrentUserId();
                var sectionApporve = result.Data
                    .DistinctBy(l => new { l.CHR_MaDon, l.CHR_SectionCode })
                    .Select(l => (l.CHR_SectionCode, l.CHR_SectionName, l.CHR_MaDon, l.CHR_Gap, l.ID_StepBaoGia, l.CHR_UserApproval))
                    .ToList();
                if (sectionApporve != null)
                {
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                foreach (var item in sectionApporve)
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

        [HttpPost("ReturnQuotation")]
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
                            await sendMailService.SendMailAsync(firstData.CHR_CreateBy + "@brothergroup.net", user + "@brothergroup.net", 12, "Quote/HistoryQuote", firstData.CHR_Gap == "false" ? false : true, firstData.CHR_SectionCode ?? "", firstData.CHR_MaDon ?? "", user);
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

        private async Task<List<string>> LoadCategoryDataAsync()
        {
            var categoryS = await _tmCategoryService.GetListCategory();
            return categoryS.Data ?? new List<string>();
        }

        private async Task<List<DEPARTMENTDTO>> LoadNhomViTriDataAsync()
        {
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

        private static double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
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
