using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using PRJ_WAREHOUSE_BIVN.Common;
using System;
using System.Collections.Immutable;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using Path = System.IO.Path;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class InputQuotationController(IExchangeRateService exchangeRateService, IWebHostEnvironment env, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaService baoGiaService,
    IBaoGiaStatusService baoGiaStatusService, IBaoGiaStepService baoGiaStepService, ILogger<InputQuotationController> logger, IServiceScopeFactory serviceScopeFactory,
    IMasterApproverSendMailService approverService, IMaterialService materialService, IConfiguration configuration, IBaoGiaDetailService baoGiaDetailService
        , ITmCategoryService tmCategoryService, IDepartmentService deparmentService, ITmNccNewService tmNccNewService, IStringLocalizer<InputQuotationController> localizer
        , ISendMailService sendMailService, IFileImportService fileImportService
        ) : BaseAuthController
    {
        private readonly ILogger<InputQuotationController> _logger = logger;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService = baoGiaHistoryService;
        private readonly IWebHostEnvironment _env = env;
        private readonly IBaoGiaService _baoGiaService = baoGiaService;
        private readonly IBaoGiaStatusService _baoGiaStatusService = baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService = baoGiaStepService;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly IMasterApproverSendMailService _approverService = approverService;
        private readonly IMaterialService _materialService = materialService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IExchangeRateService _exchangeRateService = exchangeRateService;
        private readonly IBaoGiaDetailService _baoGiaDetailService = baoGiaDetailService;
        private readonly ISendMailService _sendMailService = sendMailService;
        private readonly IFileImportService _fileImportService = fileImportService;

        private readonly ITmCategoryService _tmCategoryService = tmCategoryService;
        private readonly IDepartmentService _deparmentService = deparmentService;
        private readonly ITmNccNewService _tmNccNewService = tmNccNewService;
        private readonly IStringLocalizer<InputQuotationController> _localizer = localizer;

        // Nhập file excel
        [HttpPost]
        public async Task<IActionResult> ImportExcelInputQuote([FromForm] IFormFile file, List<int> idChecks)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có file được tải lên");

                var exchangeRateResponse = await _exchangeRateService.GetExchangeRate();
                if (exchangeRateResponse == null || !exchangeRateResponse.Success)
                    return BadRequest("Không thể lấy tỷ giá tiền tệ");

                var exchangeRate = exchangeRateResponse.Data;
                var items = new List<BaoGia_Detail_of_QuotationDTO>();
                var hasErrors = false;

                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                    return BadRequest("Không tìm thấy worksheet trong file");

                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 13;

                for (int r = 13; r <= lastRow; r++)
                {
                    var col1Val = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrWhiteSpace(col1Val)) break;

                    var col16 = ws.Cell(r, 16).GetString();
                    var col17 = ws.Cell(r, 17).GetString();
                    int? qty2 = ConvertHelper.ParseInt(ws.Cell(r, 14).GetString());
                    var nameVN = ws.Cell(r, 12).GetString();
                    var nameEN = ws.Cell(r, 13).GetString();

                    if (qty2 == null)
                    {
                        ws.Cell(r, 35).SetValue("INT_SoLuong (cột 14) phải là số hợp lệ");
                        ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                        hasErrors = true;
                        continue;
                    }

                    bool isRefuse = col16.Contains("refuse", StringComparison.OrdinalIgnoreCase) ||
                                    col17.Contains("refuse", StringComparison.OrdinalIgnoreCase);

                    if (!isRefuse)
                    {
                        if ((string.IsNullOrEmpty(col16) && string.IsNullOrEmpty(col17)) || (col16 == "0" && col17 == "0"))
                            break;

                        var errors = new List<string>();
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 22).GetString())) errors.Add("Cột 22 (VCHR_CamKet) bắt buộc");
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 23).GetString())) errors.Add("Cột 23 (Delivery Term) bắt buộc");
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 24).GetString())) errors.Add("Cột 24 (Payment Term) bắt buộc");

                        var ship = ConvertHelper.ParseDate(ws.Cell(r, 21).GetString());
                        var reqDate = ConvertHelper.ParseDate(ws.Cell(r, 28).GetString());
                        if (ship == null) errors.Add("Cột 21 (DTM_ShipTime) không phải ngày hợp lệ");
                        if (reqDate == null) errors.Add("Cột 28 (DTM_NgayMuonNhan yêu cầu) không phải ngày hợp lệ");

                        if (errors.Any())
                        {
                            ws.Cell(r, 35).SetValue(string.Join("; ", errors));
                            ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                            hasErrors = true;
                            continue;
                        }
                        if (string.IsNullOrEmpty(nameEN) && string.IsNullOrEmpty(nameVN))
                        {
                            ws.Cell(r, 35).SetValue("Tên hàng không được để trống");
                            ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                            hasErrors = true;
                            continue;
                        }
                    }

                    int idRequestQuote = 0;
                    string reason = ws.Cell(r, 34).GetString();
                    var col32Val = ws.Cell(r, 32).GetString();

                    if (!string.IsNullOrEmpty(col32Val))
                    {
                        var checkRQ = await _baoGiaDetailService.GetIdDetailAsync(ConvertHelper.ParseInt(col32Val));
                        if (checkRQ.Success && checkRQ.Data != 0)
                            idRequestQuote = checkRQ.Data;
                    }
                    else
                    {
                        var checkRQ = await _baoGiaDetailService.GetIdOfQuotationAsync(
                            col1Val, ws.Cell(r, 4).GetString(), ws.Cell(r, 3).GetString(),
                            ws.Cell(r, 10).GetString(), ws.Cell(r, 2).GetString());

                        if (checkRQ.Success && checkRQ.Data.HasValue)
                            idRequestQuote = checkRQ.Data.Value;
                    }

                    if (idRequestQuote == 0)
                    {
                        ws.Cell(r, 35).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                        ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                        hasErrors = true;
                        continue;
                    }

                    // Kiểm tra đơn đã có lý do nêus up lại hay chưa
                    if (int.TryParse(col32Val, out var id) && idChecks.Contains(id) && reason == "")
                    {
                        ws.Cell(r, 35).SetValue("Đơn đã nhập trước đó! Voi lòng nhập lý do sửa vào cột 34");
                        ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                        hasErrors = true;
                        continue;
                    }

                    var agree = ws.Cell(r, 22).GetString();
                    double costUSD = ConvertHelper.ParseDouble(col16) ?? 0;
                    double costVND = ConvertHelper.ParseDouble(col17) ?? 0;

                    items.Add(new BaoGia_Detail_of_QuotationDTO
                    {
                        ID = idRequestQuote,
                        CHR_MaHangNCC = ws.Cell(r, 11).GetString(),
                        NVCHR_TenHangHQ = nameVN,
                        CHR_NameEN = nameEN,
                        INT_SoLuong = qty2,
                        NVCHR_DonVi = ws.Cell(r, 15).GetString(),
                        FL_USD = isRefuse ? null : (costUSD != 0 ? costUSD : ConvertHelper.ParseVNDtoUSD(costVND, true, exchangeRate)),
                        FL_VND = isRefuse ? null : (costVND != 0 ? costVND : ConvertHelper.ParseVNDtoUSD(costUSD, false, exchangeRate)),
                        NVCHR_MOQ = isRefuse ? null : ConvertHelper.ParseInt(ws.Cell(r, 18).GetString())?.ToString(),
                        NVCHR_Packing = isRefuse ? null : ws.Cell(r, 19).GetString(),
                        DTM_LeadTime = isRefuse ? null : ws.Cell(r, 20).GetString(),
                        DTM_ShipTime = isRefuse ? null : ConvertHelper.ParseDate(ws.Cell(r, 21).GetString()),
                        VCHR_Rohs = isRefuse ? null : (agree.Contains("Đồng ý (accept)") ? "OK" : "NG"),
                        VCHR_COCQ = isRefuse ? null : (agree.Contains("Đồng ý (accept)") ? "OK" : "NG"),
                        VCHR_MSDS = isRefuse ? null : (agree.Contains("Đồng ý (accept)") ? "OK" : "NG"),
                        VCHR_AnToan = isRefuse ? null : (agree.Contains("Đồng ý (accept)") ? "OK" : "NG"),
                        VCHR_CamKet = isRefuse ? null : agree,
                        NVCHR_DeliveryTerm = isRefuse ? null : ws.Cell(r, 23).GetString(),
                        NVCHR_PaymentTerm = isRefuse ? null : ws.Cell(r, 24).GetString(),
                        DTM_EffectiveDate = isRefuse ? null : ConvertHelper.ParseDate(ws.Cell(r, 25).GetString()),
                        DTM_ExpiryDate = isRefuse ? null : ConvertHelper.ParseDate(ws.Cell(r, 26).GetString()),
                        CHR_UpdateBy =  GetCurrentUserId(),
                        NVCHR_File = ws.Cell(r, 30).GetString()?.Trim(),
                        CHR_Status = isRefuse ? "Refuse" : null,
                        NVCHR_ReasonUpdate = reason
                    });
                }

                if (hasErrors)
                {
                    using var outStream = new MemoryStream();
                    workbook.SaveAs(outStream);
                    return File(outStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
                if (!items.Any())
                    return BadRequest("Không có dữ liệu hợp lệ để cập nhật");

                var uniqueFiles = items.Select(c => c.NVCHR_File)
                                       .Where(f => !string.IsNullOrWhiteSpace(f))
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToList();

                var savedMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var src in uniqueFiles)
                {
                    try
                    {
                        var saveRes = await _fileImportService.SaveFileFromPathAsync(src);
                        savedMap[src] = (saveRes != null && saveRes.Success) ? saveRes.Data : null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed saving link {Link}", src);
                        return BadRequest($"Lỗi khi lưu file từ đường dẫn: {src}. Chi tiết: {ex.Message}");
                    }
                }

                foreach (var dto in items.Where(d => !string.IsNullOrWhiteSpace(d.NVCHR_File)))
                {
                    if (savedMap.TryGetValue(dto.NVCHR_File, out var saved) && !string.IsNullOrWhiteSpace(saved))
                    {
                        dto.NVCHR_dataOld = dto.NVCHR_File; // Lưu giá trị cũ trước khi thay đổi
                        dto.NVCHR_File = saved;
                    }
                }

                var result = await _baoGiaDetailService.UpdateListThongTinNhapBaoGiaAsync(items);
                if (!result.Success)
                    return BadRequest(result.Message);

                var listUpdateStatus = items.Select(c => c.ID).Distinct().ToList();
                var resultUpdateStatus = await _baoGiaDetailService.UpdateStatusAsync(listUpdateStatus);
                if (!resultUpdateStatus.Success)
                {
                    _logger.LogError("Lỗi khi cập nhật trạng thái: {Message}", resultUpdateStatus.Message);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi Import Excel");
                return BadRequest(ex.Message);
            }
        }
        // Kiểm tra Step đơn
        [HttpPost]
        public async Task<IActionResult> CheckImportExcelInputQuote([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có file");

                var ids = new List<int>();

                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                var ws = workbook.Worksheets.FirstOrDefault();

                if (ws == null)
                    return BadRequest("Không tìm thấy worksheet");

                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 13;

                for (int r = 13; r <= lastRow; r++)
                {
                    var col1Val = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrWhiteSpace(col1Val))
                        break;

                    int idRequestQuote = int.Parse(ws.Cell(r, 32).GetString());

                    if (idRequestQuote > 0)
                    {
                        ids.Add(idRequestQuote);
                    }
                }

                ids = ids.Distinct().ToList();

                var duplicatedIds = await _baoGiaService.CheckStepAsync(
                    ids,
                    new List<int> { 6,7,8 });
                if (!duplicatedIds.Success)
                {
                    return BadRequest("Lỗi khi kiểm tra đơn. Chi tiết: "+duplicatedIds.Message);
                }


                return Ok(duplicatedIds.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // MARK : Màn hình Input Quote - Tìm kiếm báo giá theo các tiêu chí
        [HttpPost]
        public async Task<IActionResult> SearchInputQuote([FromBody] SearchInputQuote searchModel)
        {
            if (searchModel == null) return BadRequest("Không nhận Search Input");
            var result = await _baoGiaDetailService.SearchBaoGiaAsync(searchModel.idRequestQuote, searchModel.maDon,
                searchModel.maVatTu, searchModel.maNcc, searchModel.section, GetCurrentUserId(), searchModel.dayMM,
                searchModel.status,searchModel.pageSize, searchModel.pageIndex);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // export excel tab2
        [HttpPost]
        public async Task<IActionResult> ExportExcelTab2([FromBody] SearchInputQuote searchInputQuote)
        {
            if (searchInputQuote == null)
                return BadRequest("Không nhận Search Input");

            var result = await _baoGiaDetailService.SearchBaoGiaAsync(
                searchInputQuote.idRequestQuote,
                searchInputQuote.maDon,
                searchInputQuote.maVatTu,
                searchInputQuote.maNcc,
                searchInputQuote.section,
                GetCurrentUserId(),
                searchInputQuote.dayMM,
                searchInputQuote.status,
                searchInputQuote.pageSize,
                searchInputQuote.pageIndex);

            if (!result.Success)
                return BadRequest(result.Message);

            try
            {
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TmSendMailNew_Reason.xlsx");

                if (!System.IO.File.Exists(templatePath))
                    return BadRequest("Không tìm thấy file mẫu!");

                using var workbook = new XLWorkbook(templatePath);
                var ws = workbook.Worksheets.First();

                var data = result?.Data?.Data?.ToList();

                if (data == null || !data.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất Excel");
                }

                int startRow = 13;
                int row = startRow;

                foreach (var item in data)
                {
                    bool isRefuse = string.Equals(
                        item.CHR_Status,
                        "Refuse",
                        StringComparison.OrdinalIgnoreCase);

                    ws.Cell(row, 1).Value = item.CHR_MaDon ?? "";
                    ws.Cell(row, 2).Value = item.CHR_CodeNCC ?? "";
                    ws.Cell(row, 3).Value = item.NVCHR_NameNCC ?? "";
                    ws.Cell(row, 4).Value = item.CHR_MaVatTu ?? "";

                    ws.Cell(row, 10).Value = item.NVCHR_TenHangHQ ?? "";

                    ws.Cell(row, 11).Value = item.CHR_MaHangNCC ?? "";
                    ws.Cell(row, 12).Value = item.NVCHR_TenHangHQ ?? "";
                    ws.Cell(row, 13).Value = item.CHR_NameEN ?? "";

                    ws.Cell(row, 14).Value = item.INT_SoLuong ?? "";
                    ws.Cell(row, 15).Value = item.NVCHR_DonVi ?? "";

                    if (isRefuse)
                    {
                        var range = ws.Range(row, 1, row, 34);

                        range.Style.Fill.BackgroundColor = XLColor.LightPink;
                        range.Style.Font.FontColor = XLColor.DarkRed;
                        range.Style.Font.Bold = true;

                        ws.Cell(row, 16).Value = "Refuse";
                        ws.Cell(row, 17).Value = "Refuse";
                    }
                    else
                    {
                        ws.Cell(row, 16).Value = item.FL_USD ?? "";
                        ws.Cell(row, 17).Value = item.FL_VND ?? "";
                    }

                    ws.Cell(row, 18).Value = item.NVCHR_MOQ ?? "";
                    ws.Cell(row, 19).Value = item.NVCHR_Packing ?? "";

                    ws.Cell(row, 20).Value = item.DTM_LeadTime ?? "";


                    ws.Cell(row, 21).Value = item.DTM_ShipTime ?? "";

                    ws.Cell(row, 22).Value = item.VCHR_CamKet ?? "";
                    ws.Cell(row, 23).Value = item.NVCHR_DeliveryTerm ?? "";
                    ws.Cell(row, 24).Value = item.NVCHR_PaymentTerm ?? "";

                    ws.Cell(row, 25).Value = item.DTM_EffectiveDate ?? "";
                    ws.Cell(row, 26).Value = item.DTM_ExpiryDate ?? "";

                    ws.Cell(row, 28).Value = item.DTM_NgayMuonNhan ?? "";
                    ws.Cell(row, 29).Value = item.DTM_KyHan ?? "";

                    ws.Cell(row, 30).Value = item.NVCHR_File ?? "";

                    // Cột dùng để import update lại
                    ws.Cell(row, 32).Value = item.ID;

                    // Lý do sửa
                    ws.Cell(row, 34).Value = item.NVCHR_ReasonUpdate ?? "";

                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"InputQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportExcelTab2 Error");
                return BadRequest(ex.Message);
            }
        }

        // Tìm kiếm hiển thị danh sách nhập báo giá theo số đơn hàng
        [HttpPost]
        public async Task<IActionResult> SearchInputQuoteBySoDon([FromBody] ThongTinBaoGiaGomNhomModel mod)
        {
            var result = await _baoGiaService.SearchThongTinNhapBaoGiaAsync(mod.maDon, mod.section, mod.maHang, GetCurrentUserId(), mod.status, mod.pageIndex, mod.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
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
                searchModel.to,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        // Update thông tin chi tiết báo giá
        [HttpPost]
        public async Task<IActionResult> UpdateQuoteDetail([FromBody] List<BaoGia_Detail_of_QuotationDTO> details)
        {
            try
            {
                var result = await _baoGiaDetailService.UpdateListThongTinNhapBaoGiaAsync(details);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi chuyển đổi dữ liệu: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> InsertInputQuote([FromBody] InsertInputQuoteModel model)
        {
            try
            {
                // gửi mail thông báo có báo giá mới cho người yêu cầu báo giá
                var sendMail = await _sendMailService.SendMailToSupplierByRequestCodeAsync(model.MaDon);
                return Ok(sendMail.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi chuyển đổi dữ liệu: {ex.Message}");
            }
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

        // Tim kiem thong tin Material
        [HttpPost]
        public async Task<IActionResult> GetSearchMaterial([FromBody] MaterialSearch maHang)
        {
            var result = await _materialService.SearchAsync(maHang.MaHang, maHang.Name, maHang.NhomHang, maHang.PageIndex, maHang.PageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        // MARK: - Input Quote
        public async Task<IActionResult> InputQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync(9);

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                DanhSachMaDon = madons,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            return View(vm);
        }
        // MARK: Màn hình nhập thông tin chi tiết
        public async Task<IActionResult> InputQuoteDetail(string maDon)
        {
            // Load data for the detail page
            var request = await _baoGiaService.GetByMaBaoGiaAsync(maDon);
            if (!request.Success || request.Data == null || !request.Data.Any())
            {
                return NotFound("Request not found");
            }

            // Load supporting reference data
            var materialsResp = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categoriesAll = await LoadCategoryDataAsync();

            // Build distinct lists from the request data
            var listMaterial = request.Data.Select(d => d.CHR_MaHangNoiBo)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var listCategory = request.Data.Select(d => d.NVCHR_ChungLoai)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            // Build list of suppliers present in the request (group by code)
            var listNcc = request.Data
                .Where(d => !string.IsNullOrWhiteSpace(d.CHR_MaNCC) || !string.IsNullOrWhiteSpace(d.NVCHR_TenNCC))
                .GroupBy(d => d.CHR_MaNCC ?? d.NVCHR_TenNCC)
                .Select(g => new { MaNcc = g.Key, Ten = g.First().NVCHR_TenNCC })
                .ToList<dynamic>();
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                // per-request distilled lists
                listCategory = listCategory,
                listNcc = listNcc,
                listMaterial = listMaterial,

                NguoiThaoTac = GetCurrentUserId() ?? "",
                MaDonHienTai = maDon,
                // Add the specific request data
                CurrentRequest = request.Data
            };
            return View(vm);
        }
    }
}
