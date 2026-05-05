using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System;
using System.Collections.Immutable;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class HistoryQuoteController : BaseAuthController
    {
        private readonly ILogger<HistoryQuoteController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IDepartmentService _deparmentService;
        private readonly ISendMailService _sendMailService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IStringLocalizer<HistoryQuoteController> _localizer;
        private readonly IMaterialService _materialService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IWebHostEnvironment _env;

        public HistoryQuoteController(ILogger<HistoryQuoteController> logger, IConfiguration configuration,
            IBaoGiaService baoGiaService, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStatusService baoGiaStatusService,
            IBaoGiaStepService baoGiaStepService, IMasterApproverSendMailService approverService, IDepartmentService deparmentService,
            ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IStringLocalizer<HistoryQuoteController> localizer,
            IMaterialService materialService, ITmNccNewService tmNccNewService, ITmCategoryService tmCategoryService, IWebHostEnvironment env)
        {
            _logger = logger;
            _configuration = configuration;
            _baoGiaService = baoGiaService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaStepService = baoGiaStepService;
            _approverService = approverService;
            _deparmentService = deparmentService;
            _sendMailService = sendMailService;
            _serviceScopeFactory = serviceScopeFactory;
            _localizer = localizer;
            _materialService = materialService;
            _tmNccNewService = tmNccNewService;
            _tmCategoryService = tmCategoryService;
            _env = env;
        }

        // MARK: - HistoryQuote
        public async Task<IActionResult> HistoryQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await LoadMaterialsAsync();
            var nccNews = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var statusData = await _baoGiaStatusService.GetListStatusAsync();
            var madons = await LoadMadonAsync(13);
            var role = GetRolesUser();
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials,
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                DanhSachCategory = categorys,
                DanhSachStatus = statusData.Data ?? new List<BaoGia_StatusDTO>(),
                NguoiThaoTac = GetCurrentUserId() ?? "",
                Role = role
            };

            return View(vm);
        }

        // tìm kiếm đơn báo giá
        [HttpPost]
        public async Task<IActionResult> SearchBaoGia([FromBody] SearchBaoGiaViewModel searchModel)
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

        // tìm kiếm theo ID đơn báo giá 
        public async Task<IActionResult> SearchID([FromBody] int id)
        {
            var result = await _baoGiaService.GetByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
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

        // Xuất file lịch sử báo giá 
        [HttpPost]
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
                0,//searchModel.PageIndex,
                0,//searchModel.PageSize,
                searchModel.Date,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            try
            {
                // lấy thong tin status
                var Status = await _baoGiaStatusService.GetListStatusAsync();
                if (Status == null || !Status.Success)
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
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 7;
                foreach (var rq in result.Data.Data)
                {
                    int col = 1;
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, col++).SetValue(row - 6); // status placeholder
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
                    var statusName = Status.Data.Where(s => s.VCHR_CodeStatus == rq.ID_Status).Select(s => s.NVCHR_TenStatus).FirstOrDefault() ?? string.Empty;
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

        // Xuất file quản lý tiến độ màn hình lịch sử báo giá
        [HttpPost]
        public async Task<IActionResult> ExportManagerHistory([FromBody] SearchBaoGiaViewModel searchModel)
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
                0,//searchModel.PageIndex,
                0,//searchModel.PageSize,
                searchModel.Date,
                searchModel.ChungLoai
             );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            try
            {
                // lấy thong tin status
                var Status = await _baoGiaStatusService.GetListStatusAsync();
                if (Status == null || !Status.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }
                // Lấy thông tin step
                var Steps = await _baoGiaStepService.GetAll();
                if (Steps == null || !Steps.Success)
                {
                    return BadRequest("Lỗi lấy danh sách step");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateExportHistoryNew.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateExportHistoryNew.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 4;
                foreach (var rq in result.Data.Data)
                {
                    int col = 1;
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, col++).SetValue(row - 3); // status placeholder
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
                    ws.Cell(row, col++).SetValue(rq?.NVCHR_UserRequest ?? string.Empty);
                    var history = await _baoGiaHistoryService.GetByRequestQuoteIdAsync(rq.ID);
                    var reson = rq.ID_Status.Contains("RETURN") ? (history.Data.Capacity > 0 ? history.Data.OrderByDescending(h => h.CHR_Updatedate).FirstOrDefault()?.NVCHR_LyDo : string.Empty) : "";
                    var statusName = Status.Data.Where(s => s.VCHR_CodeStatus == rq.ID_Status).Select(s => s.NVCHR_TenStatus).FirstOrDefault() ?? string.Empty;
                    ws.Cell(row, col++).SetValue(statusName);
                    var stepName = Steps.Data.Where(s => s.INT_StepNumber == rq.ID_StepBaoGia).Select(s => s.CHR_StepName).FirstOrDefault() ?? string.Empty;
                    ws.Cell(row, col++).SetValue(stepName);
                    ws.Cell(row, col++).SetValue(reson);
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
                    dto.CHR_CreateBy = ws.Cell(i, 34).GetString() ?? GetCurrentUserId() ?? string.Empty;
                    dto.DTM_UpdateLater = DateTime.Now;

                    listRequest.Add(dto);
                }

                if (!listRequest.Any()) return BadRequest("Không có dữ liệu hợp lệ để cập nhật");

                // Call service to update list of requests
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
                                    //await sendMailService.SendMailToRequesterAsync(item.CHR_MaDon ?? "", item.CHR_SectionCode ?? "", item.CHR_SectionName ?? "", item.CHR_Gap == "false" ? false : true, item.ID_StepBaoGia ?? 2);
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

        // Return PIC section
        [HttpPost]
        public async Task<IActionResult> ReturnQuotation([FromBody] string madon)
        {
            if (string.IsNullOrEmpty(madon)) return BadRequest("Vui lòng chọn mã đơn!");
            try
            {
                var user = GetCurrentUserId();
                var result = await _baoGiaService.TraLaiDonBaoGiaAsync(madon, user);
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

        // Private helper methods
        private async Task<List<DEPARTMENTDTO>> LoadNhomViTriDataAsync()
        {
            var nhomViTri = await _deparmentService.GetNhomViTriByDepartmentIdAsync(GetCurrentUserId() ?? "");
            return nhomViTri.Data ?? new List<DEPARTMENTDTO>();
        }

        private async Task<List<MATERIALDTO>> LoadMaterialsAsync()
        {
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            return materials.Data ?? new List<MATERIALDTO>();
        }

        private async Task<List<IM_NCC_NEWDTO>> LoadNhaCungCapDataAsync()
        {
            var nccNews = await _tmNccNewService.GetAllNccNew();
            return nccNews.Data ?? new List<IM_NCC_NEWDTO>();
        }

        private async Task<List<string>> LoadCategoryDataAsync()
        {
            var CategoryS = await _tmCategoryService.GetListCategory();
            return CategoryS.Data ?? new List<string>();
        }

        private async Task<List<string>> LoadMadonAsync(int step)
        {
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "", step);
            return madons.Data ?? new List<string>();
        }

        private static double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
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
