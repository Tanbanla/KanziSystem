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
    [Route("QuoteQuotationResults")]
    public class QuoteQuotationResultsController : BaseAuthController
    {
        private readonly ILogger<QuoteQuotationResultsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMaterialService _materialService;
        private readonly ITmSectionService _tmSectionService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IDepartmentService _deparmentService;
        private readonly IBaoGiaNCCService _baoGiaNCCService;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
        private readonly IBaoGiaConfirmNameService _baoGiaConfirmNameService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IBaoGiaNccCategoryService _baoGiaNccCategoryService;
        private readonly IWebHostEnvironment _env;
        private readonly ISendMailService _sendMailService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITmEmployeeAgentService _tmEmployeeAgentService;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IFileImportService _fileImportService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IStringLocalizer<QuoteQuotationResultsController> _localizer;
        private readonly IBaoGiaService _baoGiaService;

        public QuoteQuotationResultsController(
            ILogger<QuoteQuotationResultsController> logger,
            IConfiguration configuration,
            IMaterialService materialService,
            ITmSectionService tmSectionService,
            ITmNccNewService tmNccNewService,
            IDepartmentService deparmentService,
            IBaoGiaNCCService baoGiaNCCService,
            IBaoGiaHistoryService baoGiaHistoryService,
            IBaoGiaStatusService baoGiaStatusService,
            IBaoGiaDetailService baoGiaDetailService,
            IBaoGiaConfirmNameService baoGiaConfirmNameService,
            ITmCategoryService tmCategoryService,
            IBaoGiaNccCategoryService baoGiaNccCategoryService,
            IWebHostEnvironment env,
            ISendMailService sendMailService,
            IServiceScopeFactory serviceScopeFactory,
            ITmEmployeeAgentService tmEmployeeAgentService,
            IMasterApproverSendMailService approverService,
            IExchangeRateService exchangeRateService,
            IFileImportService fileImportService,
            IBaoGiaStepService baoGiaStepService,
            IStringLocalizer<QuoteQuotationResultsController> localizer,
            IBaoGiaService baoGiaService)
        {
            _logger = logger;
            _configuration = configuration;
            _materialService = materialService;
            _tmSectionService = tmSectionService;
            _tmNccNewService = tmNccNewService;
            _baoGiaNCCService = baoGiaNCCService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaDetailService = baoGiaDetailService;
            _baoGiaConfirmNameService = baoGiaConfirmNameService;
            _deparmentService = deparmentService;
            _tmCategoryService = tmCategoryService;
            _baoGiaNccCategoryService = baoGiaNccCategoryService;
            _sendMailService = sendMailService;
            _env = env;
            _serviceScopeFactory = serviceScopeFactory;
            _approverService = approverService;
            _tmEmployeeAgentService = tmEmployeeAgentService;
            _exchangeRateService = exchangeRateService;
            _baoGiaStepService = baoGiaStepService;
            _localizer = localizer;
            _fileImportService = fileImportService;
            _baoGiaService = baoGiaService;
        }

        [HttpGet("Quotation_Results")]
        public async Task<IActionResult> Quotation_Results()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync(12);
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
            return View("~/Views/Quote/QuotationResults/Quotation_Results.cshtml", vm);
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

        [HttpPost("SearchSupplierQuoteBody")]
        public async Task<IActionResult> SearchSupplierQuoteBody([FromBody] SearchQuotationResultsModel search)
        {
            var result = await _baoGiaService.GetThongTinBaoGiaChiTietAsync(
                search.MaDon ?? "",
                search.Section ?? "",
                search.MaVatTu ?? "",
                search.MaNcc ?? "",
                search.Status ?? "",
                GetCurrentUserId() ?? "",
                search.PageIndex ?? 1,
                search.PageSize ?? 10);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("ExportFileExcelQuotationResult")]
        public async Task<IActionResult> ExportFileExcelQuotationResult([FromBody] SearchQuotationResultsModel search)
        {
            try
            {
                var result = await _baoGiaService.GetThongTinBaoGiaChiTietAsync(
                    search.MaDon ?? "",
                    search.Section ?? "",
                    search.MaVatTu ?? "",
                    search.MaNcc ?? "",
                    search.Status ?? "",
                    GetCurrentUserId() ?? "",
                    0,
                    0);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if (result.Data == null)
                {
                    return BadRequest("No data to export");
                }
                var dataList = result.Data.Data;
                var totals = new Dictionary<string, (double vnd, double usd)>();
                foreach (var item in dataList)
                {
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}";
                    double vnd = item.FL_VND * item.soluong ?? 0.0;
                    double usd = item.FL_USD * item.soluong ?? 0.0;
                    if (!totals.ContainsKey(key))
                    {
                        totals[key] = (0.0, 0.0);
                    }
                    var current = totals[key];
                    totals[key] = (current.Item1 + vnd, current.Item2 + usd);
                }

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateQuotationResults.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateQuotationResults.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                int rowStart = 4;
                foreach (var item in dataList)
                {
                    int col = 1;
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.status ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.ID ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_NameEN ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.INT_SoLuong ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_NgayMuonNhan?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_KyHan?.ToString("dd/MM/yyyy") ?? string.Empty);
                    if (!item.IsMatch_MaHangNCC) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.CodeEquipmentNCC ?? string.Empty);
                    if (!item.IsMatch_NameVN) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                    if (!item.IsMatch_NameEN) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.NameENByNCC ?? string.Empty);
                    if (!item.IsMatch_SoLuong) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.soluong ?? 0);
                    if (!item.IsMatch_DonVi) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.donvi ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.FL_USD ?? 0.0);
                    ws.Cell(rowStart, col++).SetValue(item.FL_VND ?? 0.0);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MOQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Packing ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_LeadTime ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ShipTime?.ToString("dd/MM/yyyy") ?? string.Empty);
                    if (!item.IsMatch_Rohs) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_Rohs ?? string.Empty);
                    if (!item.IsMatch_COCQ) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_COCQ ?? string.Empty);
                    if (!item.IsMatch_MSDS) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_MSDS ?? string.Empty);
                    if (!item.IsMatch_AnToan) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_AnToan ?? string.Empty);
                    if (!item.IsMatchCamKet) ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_CamKet ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DeliveryTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_PaymentTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_File ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}";
                    var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                    string totalCell = "";
                    var enUs = new CultureInfo("en-US");
                    if (tot.Item1 != 0) totalCell = tot.Item1.ToString("N0", enUs) + " VND";
                    else if (tot.Item2 != 0) totalCell = Math.Round(tot.Item2, 4).ToString("0.0000", enUs) + " USD";
                    ws.Cell(rowStart, col++).SetValue(totalCell);
                    ws.Cell(rowStart, col++).SetValue("");
                    ws.Cell(rowStart, col++).SetValue("");
                    rowStart++;
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"QuotationResults_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ExportFileExcelApproverResult")]
        public async Task<IActionResult> ExportFileExcelApproverResult([FromBody] List<string> model)
        {
            if (model == null || model.Count == 0)
            {
                return BadRequest(_localizer["PleaseSelectQuoteRequest"]);
            }
            try
            {
                var result = await _baoGiaService.GetExportApprovalInfoAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if (result.Data == null)
                {
                    return BadRequest(_localizer["NoDataToExport"]);
                }
                var dataList = result.Data;
                var allRowsData = new List<dynamic>();
                foreach (var item in dataList)
                {
                    allRowsData.Add(new
                    {
                        Item = item,
                        MaDon = item.CHR_MaDon ?? "",
                        ID = item.ID?.ToString() ?? "",
                        MaThietBi = item.CHR_MaThietBi ?? "",
                        MaHangNoiBo = item.CHR_MaHangNoiBo ?? "",
                        CodeVender = item.CHR_MaNCC ?? "",
                        MaHangNCC_Vendor = item.CodeEquipmentNCC ?? "",
                        MaHangNCC_BIVN = item.CHR_MaHangNCC ?? "",
                        TenHangVN = item.NVCHR_NameVN ?? "",
                        TenHangEng = item.CHR_NameEN ?? "",
                        ChungLoaiHang = item.NVCHR_ChungLoai ?? "",
                        DonGiaUSD = item.FL_USD ?? 0.0,
                        DonGiaVND = item.FL_VND ?? 0.0,
                        SoLuong = item.INT_SoLuong ?? 0,
                        DonVi = item.NVCHR_DonVi ?? "",
                        NhaSanXuat = item.NVCHR_NhaSanXuat ?? "",
                        BIT_Select = item.BIT_Select == true ? "O" : "X",
                        NVCHR_ReasonPick = item.NVCHR_ReasonPick ?? "",
                        NVCHR_Note = item.NVCHR_Note ?? ""
                    });
                }

                var errorDetails = new Dictionary<string, List<string>>();
                foreach (var rowData in allRowsData)
                {
                    var errors = new List<string>();
                    var maHangNB = rowData.MaHangNoiBo;
                    var maThietBi = rowData.MaThietBi;
                    var chungLoaiHang = rowData.ChungLoaiHang;
                    var bitSelect = rowData.BIT_Select;
                    var reason = rowData.NVCHR_ReasonPick;
                    var codeVender = rowData.CodeVender;
                    var maHangNCC = !string.IsNullOrEmpty(rowData.MaHangNCC_Vendor) ? rowData.MaHangNCC_Vendor : rowData.MaHangNCC_BIVN;
                    var donGiaUSD = rowData.DonGiaUSD;
                    string id = rowData.ID;

                    var vendorCodesForSameProduct = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O") && x.MaThietBi == maThietBi)
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();
                    if (vendorCodesForSameProduct >= 2 && bitSelect.Contains("O")) errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct]);

                    var hasAnySelect = allRowsData.Any(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O"));
                    if (!hasAnySelect && !string.IsNullOrEmpty(maHangNB)) errors.Add(_localizer["VendorCodeNotSelected", maHangNB]);

                    var vendorsForEquipmentAndCategory = allRowsData
                        .Where(x => (x.MaThietBi == maThietBi && x.ChungLoaiHang == chungLoaiHang && x.BIT_Select.Contains("O")) && !string.IsNullOrEmpty(maThietBi))
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();
                    if (vendorsForEquipmentAndCategory > 1 && bitSelect.Contains("O")) errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory]);

                    if (bitSelect.Contains("O"))
                    {
                        var allPricesForProduct = allRowsData.Where(x => x.MaHangNoiBo == maHangNB && x.DonGiaUSD > 0).Select(x => new { x.DonGiaUSD, x.DonGiaVND }).ToList();
                        if (allPricesForProduct.Any() && allPricesForProduct.Count > 1)
                        {
                            decimal minPriceUSD = (decimal)allPricesForProduct.Min(x => x.DonGiaUSD);
                            decimal currentPriceUSD = (decimal)donGiaUSD;
                            if (Math.Abs(currentPriceUSD - minPriceUSD) > 0.01m) errors.Add(_localizer["SelectedPriceNotLowest", currentPriceUSD.ToString("N2"), minPriceUSD.ToString("N2")]);
                        }
                    }

                    var duplicatePrice = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB &&
                               ((!string.IsNullOrEmpty(x.MaHangNCC_Vendor) && x.MaHangNCC_Vendor == maHangNCC) ||
                                (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender)
                        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                        .Distinct()
                        .Count();
                    if (duplicatePrice > 1) errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC]);
                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason)) errors.Add(_localizer["SelectedVendorNoReason"]);
                    if (errors.Any()) errorDetails[id] = errors;
                }

                var totals = new Dictionary<string, (double vnd, double usd)>();
                foreach (var item in dataList)
                {
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}|{(string.IsNullOrEmpty(item.CodeEquipmentNCC) ? item.ID.ToString() : item.CodeEquipmentNCC)}";
                    double vnd = item.FL_VND * item.soluong ?? 0.0;
                    double usd = item.FL_USD * item.soluong ?? 0.0;
                    if (!totals.ContainsKey(key)) totals[key] = (0.0, 0.0);
                    var current = totals[key];
                    totals[key] = (current.Item1 + vnd, current.Item2 + usd);
                }

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateQuotationResults.xlsx");
                if (!System.IO.File.Exists(templatePath)) return BadRequest(_localizer["TemplateNotFound", "TemplateQuotationResults.xlsx"]);
                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest(_localizer["WorksheetNotFound"]);
                int rowStart = 4;
                foreach (var item in dataList)
                {
                    var enUs = new CultureInfo("en-US");
                    int col = 1;
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia == 9 ? "Chief/Expert Approval" : (item.ID_StepBaoGia == 10 ? "Section Manager Approval" : "Dept Manager Approval"));
                    ws.Cell(rowStart, col++).SetValue(item.ID ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_NameEN ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.INT_SoLuong ?? 0);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_NgayMuonNhan?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_KyHan?.ToString("dd/MM/yyyy") ?? string.Empty);

                    var codeEquipmentCell = ws.Cell(rowStart, col++);
                    codeEquipmentCell.SetValue(item.CodeEquipmentNCC ?? string.Empty);
                    if (!item.IsMatch_MaHangNCC) codeEquipmentCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var nameVNCell = ws.Cell(rowStart, col++);
                    nameVNCell.SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                    if (!item.IsMatch_NameVN) nameVNCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var nameENCell = ws.Cell(rowStart, col++);
                    nameENCell.SetValue(item.NameENByNCC ?? string.Empty);
                    if (!item.IsMatch_NameEN) nameENCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var quantityCell = ws.Cell(rowStart, col++);
                    quantityCell.SetValue(item.soluong ?? 0);
                    if (!item.IsMatch_SoLuong) quantityCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var unitCell = ws.Cell(rowStart, col++);
                    unitCell.SetValue(item.donvi ?? string.Empty);
                    if (!item.IsMatch_DonVi) unitCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue((item.FL_USD ?? 0));
                    ws.Cell(rowStart, col++).SetValue((item.FL_VND ?? 0).ToString("N0", enUs));
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MOQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Packing ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_LeadTime ?? string.Empty);

                    var shipTimeCell = ws.Cell(rowStart, col++);
                    shipTimeCell.SetValue(item.DTM_ShipTime ?? string.Empty);
                    if (!item.IsMatch_Ngay) shipTimeCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var rohsCell = ws.Cell(rowStart, col++);
                    rohsCell.SetValue(item.VCHR_Rohs ?? string.Empty);
                    if (!item.IsMatch_Rohs) rohsCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var cocqCell = ws.Cell(rowStart, col++);
                    cocqCell.SetValue(item.VCHR_COCQ ?? string.Empty);
                    if (!item.IsMatch_COCQ) cocqCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var msdsCell = ws.Cell(rowStart, col++);
                    msdsCell.SetValue(item.VCHR_MSDS ?? string.Empty);
                    if (!item.IsMatch_MSDS) msdsCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var anToanCell = ws.Cell(rowStart, col++);
                    anToanCell.SetValue(item.VCHR_AnToan ?? string.Empty);
                    if (!item.IsMatch_AnToan) anToanCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    var camKetCell = ws.Cell(rowStart, col++);
                    camKetCell.SetValue(item.VCHR_CamKet ?? string.Empty);
                    if (!item.IsMatchCamKet) camKetCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DeliveryTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_PaymentTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_File ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty);

                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}|{(string.IsNullOrEmpty(item.CodeEquipmentNCC) ? item.ID.ToString() : item.CodeEquipmentNCC)}";
                    var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                    string totalCell = "";
                    if (tot.Item1 != 0) totalCell = tot.Item1.ToString("N0", enUs) + " VND";
                    else if (tot.Item2 != 0) totalCell = Math.Round(tot.Item2, 4).ToString("0.0000", enUs) + " USD";
                    ws.Cell(rowStart, col++).SetValue(totalCell);
                    ws.Cell(rowStart, col++).SetValue(item.BIT_Select == true ? "O" : "X");
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ReasonPick ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Note ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 9 ? item.UserQlsc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 9, item.LyDoQlsc));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 9 ? item.LyDoQlsc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 10 ? item.UserQltc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 10, item.LyDoQltc));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 10 ? item.LyDoQltc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 11 ? item.UserDeft ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 11, item.LyDoDeft));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 11 ? item.LyDoDeft ?? "" : "");
                    string itemId = item.ID?.ToString() ?? "";
                    if (errorDetails.ContainsKey(itemId) && errorDetails[itemId].Any())
                    {
                        ws.Cell(rowStart, col++).SetValue(string.Join("; ", errorDetails[itemId]));
                        ws.Row(rowStart).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    else
                    {
                        ws.Cell(rowStart, col++).SetValue("");
                    }

                    rowStart++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"QuotationResults_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(_localizer["ExportError", ex.Message]);
            }
        }

        [HttpPost("ImportApprovalQuotianExcel")]
        public async Task<IActionResult> ImportApprovalQuotianExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                var isErrors = false;
                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    var errors = new List<string>();
                    var maDon = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrEmpty(maDon)) break;
                    var idRequest = ws.Cell(r, 3).GetString();
                    var resultApproval = "";
                    var resonApproval = "";
                    var status = ws.Cell(r, 2).GetString();

                    switch (status)
                    {
                        case "Chief/Expert Approval":
                            resultApproval = ws.Cell(r, 55).GetString();
                            resonApproval = ws.Cell(r, 56).GetString();
                            break;
                        case "Section Manager Approval":
                            resultApproval = ws.Cell(r, 58).GetString();
                            resonApproval = ws.Cell(r, 59).GetString();
                            break;
                        default:
                            resultApproval = ws.Cell(r, 61).GetString();
                            resonApproval = ws.Cell(r, 62).GetString();
                            break;
                    }

                    if (resultApproval == "NG" && resonApproval == "")
                    {
                        isErrors = true;
                        errorRows.Add(new
                        {
                            Row = r,
                            MaDon = maDon,
                            ID = idRequest,
                            BIT_Select = false,
                            NVCHR_LyDo = resonApproval,
                            ID_Step = status,
                            Errors = "Lý do phê duyệt không được để trống khi kết quả là NG"
                        });
                        continue;
                    }
                    items.Add(new
                    {
                        Row = r,
                        MaDon = maDon,
                        ID = idRequest,
                        BIT_Select = resultApproval != "NG",
                        NVCHR_LyDo = resonApproval,
                        ID_Step = status == "Chief/Expert Approval" ? 10 : (status == "Section Manager Approval" ? 11 : 11),
                        Errors = string.Join("; ", errors)
                    });
                }
                if (isErrors)
                {
                    using var errorWorkbook = new XLWorkbook();
                    var errorWs = errorWorkbook.Worksheets.Add("Errors");
                    errorWs.Cell(1, 1).Value = "Row";
                    errorWs.Cell(1, 2).Value = "MaDon";
                    errorWs.Cell(1, 3).Value = "ID";
                    errorWs.Cell(1, 4).Value = "BIT_Select";
                    errorWs.Cell(1, 5).Value = "NVCHR_LyDo";
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
                if (items.Count == 0) return BadRequest("Không nhận được dữ liệu hợp lệ ");

                var listApproval = new List<ApproverDTO>();
                var step = 0;
                foreach (var item in items)
                {
                    if (step == 0) step = item.ID_Step;
                    listApproval.Add(new ApproverDTO
                    {
                        Id = int.Parse(item.ID.ToString()),
                        IsApproved = item.BIT_Select,
                        Reason = item.NVCHR_LyDo.ToString(),
                    });
                }

                var result = await _approverService.GetApproverByStepAndSectionAsync(step, "");
                if (!result.Success)
                {
                    return BadRequest("Error list Approver: " + result.Message);
                }
                var approvers = result.Data.FirstOrDefault();
                return await PheDuyetBaoGia(listApproval, approvers?.CHR_UserAdid ?? "vuthipt");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        [HttpPost("ImportQuotianExcel")]
        public async Task<IActionResult> ImportQuotianExcel([FromForm] ImportPickSupplier vm)
        {
            if (vm.fileSend == null || vm.fileSend.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = vm.fileSend.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                var isErrors = false;

                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                var allRowsData = new List<dynamic>();

                for (int r = startRow; r <= lastRow; r++)
                {
                    var maDon = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrEmpty(maDon)) break;

                    var id = ws.Cell(r, 3).GetString();
                    var maThietBi = ws.Cell(r, 4).GetString();
                    var maHangNB = ws.Cell(r, 5).GetString();
                    var maHangNCC_BIVN = ws.Cell(r, 6).GetString();
                    var tenHangVN = ws.Cell(r, 7).GetString();
                    var tenHangEng = ws.Cell(r, 8).GetString();
                    var soLuong = ws.Cell(r, 9).GetDouble();
                    var donVi = ws.Cell(r, 10).GetString();
                    var chungLoaiHang = ws.Cell(r, 11).GetString();
                    var codeVender = ws.Cell(r, 24).GetString();
                    var maHangNCC_Vendor = ws.Cell(r, 28).GetString();
                    var nhaSanXuat = ws.Cell(r, 33).GetString();
                    var donGiaUSD = ws.Cell(r, 34).GetDouble();
                    var donGiaVND = ws.Cell(r, 35).GetDouble();
                    var bitSelect = ws.Cell(r, 51).GetString();
                    var reason = ws.Cell(r, 52).GetString();
                    var reasonRemark = ws.Cell(r, 53).GetString();

                    allRowsData.Add(new
                    {
                        Row = r,
                        MaDon = maDon,
                        ID = id,
                        MaThietBi = maThietBi,
                        MaHangNoiBo = maHangNB,
                        CodeVender = codeVender,
                        MaHangNCC_BIVN = maHangNCC_BIVN,
                        MaHangNCC_Vendor = maHangNCC_Vendor,
                        TenHangVN = tenHangVN,
                        TenHangEng = tenHangEng,
                        ChungLoaiHang = chungLoaiHang,
                        DonGiaUSD = donGiaUSD,
                        DonGiaVND = donGiaVND,
                        SoLuong = soLuong,
                        DonVi = donVi,
                        NhaSanXuat = nhaSanXuat,
                        BIT_Select = bitSelect,
                        NVCHR_ReasonPick = reason,
                        NVCHR_Note = reasonRemark
                    });
                }

                foreach (var rowData in allRowsData)
                {
                    var errors = new List<string>();
                    var maDon = rowData.MaDon;
                    var maHangNB = rowData.MaHangNoiBo;
                    var maThietBi = rowData.MaThietBi;
                    var chungLoaiHang = rowData.ChungLoaiHang;
                    var bitSelect = rowData.BIT_Select;
                    var reason = rowData.NVCHR_ReasonPick;
                    var codeVender = rowData.CodeVender;
                    var maHangNCC = !string.IsNullOrEmpty(rowData.MaHangNCC_Vendor) ? rowData.MaHangNCC_Vendor : rowData.MaHangNCC_BIVN;
                    var donGiaUSD = rowData.DonGiaUSD;
                    int row = rowData.Row;
                    string id = rowData.ID;
                    var nvchrNote = rowData.NVCHR_Note;

                    var vendorCodesForSameProduct = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O") && x.MaThietBi == maThietBi)
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();
                    if (vendorCodesForSameProduct >= 2 && bitSelect.Contains("O")) errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct]);

                    var vendorsForEquipmentAndCategory = allRowsData
                        .Where(x => (x.MaThietBi == maThietBi && x.ChungLoaiHang == chungLoaiHang && x.BIT_Select.Contains("O") && x.MaHangNCC_BIVN == maHangNCC) && !string.IsNullOrEmpty(maThietBi))
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();
                    if (vendorsForEquipmentAndCategory > 1 && bitSelect.Contains("O")) errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory]);

                    var duplicatePrice = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB &&
                               ((!string.IsNullOrEmpty(x.MaHangNCC_Vendor) && x.MaHangNCC_Vendor == maHangNCC) ||
                                (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender)
                        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                        .Distinct()
                        .Count();
                    if (duplicatePrice > 1) errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC]);

                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason)) errors.Add(_localizer["SelectedVendorNoReasonColumn", 52]);

                    if (errors.Any())
                    {
                        isErrors = true;
                        errorRows.Add(new
                        {
                            Row = row,
                            MaDon = maDon,
                            ID = id,
                            MaHangNoiBo = maHangNB,
                            VendorCode = maHangNCC,
                            BIT_Select = bitSelect,
                            DonGiaUSD = donGiaUSD,
                            NVCHR_ReasonPick = reason,
                            Errors = string.Join("; ", errors)
                        });
                    }
                    else
                    {
                        items.Add(new
                        {
                            ID = id,
                            BIT_Select = bitSelect.Contains("O"),
                            NVCHR_ReasonPick = reason,
                            CHR_MaDon = maDon,
                            CHR_MaHangNoiBo = maHangNB,
                            NVCHR_Note = nvchrNote,
                            CHR_MaThietBi = maThietBi
                        });
                    }
                }

                if (isErrors)
                {
                    using var errorWorkbook = new XLWorkbook();
                    var errorWs = errorWorkbook.Worksheets.Add("Errors");
                    errorWs.Cell(1, 1).Value = "Row";
                    errorWs.Cell(1, 2).Value = "Số đơn";
                    errorWs.Cell(1, 3).Value = "ID";
                    errorWs.Cell(1, 4).Value = "Mã hàng nội bộ";
                    errorWs.Cell(1, 5).Value = "Vendor Code";
                    errorWs.Cell(1, 6).Value = "Đơn giá USD";
                    errorWs.Cell(1, 7).Value = "Lựa chọn (O/X)";
                    errorWs.Cell(1, 8).Value = "Lý do";
                    errorWs.Cell(1, 9).Value = "Lỗi chi tiết";
                    var headerRange = errorWs.Range(1, 1, 1, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    for (int i = 0; i < errorRows.Count; i++)
                    {
                        var row = errorRows[i];
                        errorWs.Cell(i + 2, 1).Value = row.Row;
                        errorWs.Cell(i + 2, 2).Value = row.MaDon;
                        errorWs.Cell(i + 2, 3).Value = row.ID;
                        errorWs.Cell(i + 2, 4).Value = row.MaHangNoiBo;
                        errorWs.Cell(i + 2, 5).Value = row.VendorCode;
                        errorWs.Cell(i + 2, 6).Value = row.DonGiaUSD;
                        errorWs.Cell(i + 2, 7).Value = row.BIT_Select;
                        errorWs.Cell(i + 2, 8).Value = row.NVCHR_ReasonPick;
                        errorWs.Cell(i + 2, 9).Value = row.Errors;
                    }
                    errorWs.Columns().AdjustToContents();
                    using var errorStream = new MemoryStream();
                    errorWorkbook.SaveAs(errorStream);
                    var errorBytes = errorStream.ToArray();
                    var errorFileName = $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    const string errorContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(errorBytes, errorContentType, errorFileName);
                }

                var dtoList = items.Select(i => new BaoGia_Detail_of_QuotationDTO
                {
                    ID = int.Parse(i.ID.ToString()),
                    BIT_Select = (bool)i.BIT_Select,
                    NVCHR_ReasonPick = i.NVCHR_ReasonPick?.ToString() ?? "",
                    CHR_UpdateBy = GetCurrentUserId(),
                    NVCHR_Note = i.NVCHR_Note?.ToString() ?? ""
                }).ToList();

                var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(dtoList, vm.userNextApproval);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(new { message = "Import successful", totalRows = items.Count });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        [HttpPost("SavePickSupplier")]
        public async Task<IActionResult> SavePickSupplier([FromBody] SaveQuotationResultsModel vm)
        {
            var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(vm.listPick, vm.UserApproverNext);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            var req = result.Data;
            var userSend = vm.UserApproverNext;
            var currentUserId = GetCurrentUserId();
            _ = Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                        await sendMailService.SendMailAsync(userSend + "@brothergroup.net", "", 14, "QuoteQuotationResults/Quotation_Results", req.CHR_Gap == "false" ? false : true, req.CHR_SectionCode ?? "", req.CHR_MaDon ?? "", currentUserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                    }
                }
            });
            return Ok(result.Data);
        }

        [HttpPost("SearchInputQuote")]
        public async Task<IActionResult> SearchInputQuote([FromBody] SearchInputQuote searchModel)
        {
            if (searchModel == null) return BadRequest("Không nhận Search Input");
            var result = await _baoGiaDetailService.SearchBaoGiaAsync(searchModel.idRequestQuote, searchModel.maDon,
                searchModel.maVatTu, searchModel.maNcc, searchModel.section, GetCurrentUserId(), searchModel.dayMM, searchModel.pageSize, searchModel.pageIndex);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("GetThongTinBaoGiaGomNhom")]
        public async Task<IActionResult> GetThongTinBaoGiaGomNhom([FromBody] ThongTinBaoGiaGomNhomModel model)
        {
            var result = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync(model.maDon, model.section, model.maHang, model.status, GetCurrentUserId(), model.pageIndex, model.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("ConfirmApprover")]
        public async Task<IActionResult> ConfirmApprover([FromBody] ConfirmApproverModel model)
        {
            if (model == null || !model.listCofirm.Any())
            {
                return BadRequest("Not data approver");
            }
            return await PheDuyetBaoGia(model.listCofirm, model.UserApproverNext);
        }

        [HttpPost("GetSupplierApprovalInfor")]
        public async Task<IActionResult> GetSupplierApprovalInfor([FromBody] string maDon)
        {
            if (string.IsNullOrWhiteSpace(maDon))
            {
                return BadRequest("Mã đơn không được để trống");
            }
            var result = await _baoGiaService.GetSupplierApprovalInfoAsync(maDon);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("ChonNhaCungCapBaoGia")]
        public async Task<IActionResult> ChonNhaCungCapBaoGia([FromBody] List<dynamic> listUpdate)
        {
            var result = await _baoGiaDetailService.UpdateLuaChonNCCBaoGiaDetailAsync(listUpdate, GetCurrentUserId(), GetCurrentUserFullName());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("ExportSelection")]
        public async Task<IActionResult> ExportSelection([FromBody] List<SelectionExportItem> selections)
        {
            try
            {
                if (selections == null || !selections.Any()) return BadRequest("Không có dữ liệu để xuất");
                var status = await _baoGiaStatusService.GetListStatusAsync();
                if (status == null || !status.Success) return BadRequest("Lỗi lấy danh sách trạng thái");
                List<BaoGia_StatusDTO> listStatus = status.Data ?? new List<BaoGia_StatusDTO>();
                List<int> listIdExport = new List<int>();
                foreach (var item in selections)
                {
                    if (!string.IsNullOrEmpty(item.ID) && int.TryParse(item.ID, out int id) && !listIdExport.Contains(id)) listIdExport.Add(id);
                    if (!string.IsNullOrEmpty(item.MaDon))
                    {
                        var reqs = await _baoGiaService.ExportBaoGiaAsync(item.MaDon);
                        if (reqs.Success && reqs.Data != null && reqs.Data.Count > 0)
                        {
                            foreach (var r in reqs.Data)
                            {
                                if (!listIdExport.Contains(r)) listIdExport.Add(r);
                            }
                        }
                    }
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "FileSelectionQuote.xlsx");
                if (!System.IO.File.Exists(templatePath)) return BadRequest("Không tìm thấy file template: FileSelectionQuote.xlsx");
                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet trong template");

                int row = 4;
                foreach (var item in listIdExport)
                {
                    var rqResp = await _baoGiaService.GetByIdAsync(item);
                    if (!rqResp.Success || rqResp.Data == null) continue;
                    var rq = rqResp.Data;
                    var detailResp = await _baoGiaDetailService.GetByIdRequestQuoteAsync(rq.ID);
                    if (!detailResp.Success || detailResp.Data == null) continue;
                    var d = detailResp.Data;
                    int col = 1;
                    ws.Cell(row, col++).SetValue(listStatus.Where(c => c.VCHR_CodeStatus == rq.ID_Status).Select(c => c.NVCHR_TenStatus).FirstOrDefault());
                    ws.Cell(row, col++).SetValue("OK");
                    ws.Cell(row, col++).SetValue(rq.CHR_MaDon ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.ID.ToString() ?? "");
                    ws.Cell(row, col++).SetValue(rq.CHR_MaThietBi ?? "");
                    ws.Cell(row, col++).SetValue(rq.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(rq.INT_SoLuong.HasValue ? rq.INT_SoLuong.Value : 0);
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
                    ws.Cell(row, col++).SetValue(rq.DTM_NgayMuonNhan.HasValue ? rq.DTM_NgayMuonNhan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, col++).SetValue(rq.DTM_KyHan.HasValue ? rq.DTM_KyHan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, col++).SetValue(d.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_TenHangHQ ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.INT_SoLuong.HasValue ? d.INT_SoLuong.Value : 0);
                    ws.Cell(row, col++).SetValue(d.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.FL_USD.HasValue ? d.FL_USD.Value : 0);
                    ws.Cell(row, col++).SetValue(d.FL_VND.HasValue ? d.FL_VND.Value : 0);
                    ws.Cell(row, col++).SetValue(d.NVCHR_MOQ ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_Packing ?? string.Empty);
                    ws.Cell(row, col++).SetValue(string.IsNullOrWhiteSpace(d.DTM_LeadTime) ? string.Empty : d.DTM_LeadTime);
                    ws.Cell(row, col++).SetValue(d.DTM_ShipTime.HasValue ? d.DTM_ShipTime.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, col++).SetValue(d.VCHR_Rohs ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.VCHR_COCQ ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.VCHR_MSDS ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.VCHR_AnToan ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.VCHR_CamKet ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_DeliveryTerm ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_PaymentTerm ?? string.Empty);
                    ws.Cell(row, col++).SetValue(d.NVCHR_File ?? string.Empty);
                    row++;
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"SelectionQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
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

        private async Task<IActionResult> PheDuyetBaoGia(List<ApproverDTO> listCofirm, string UserApproverNext)
        {
            if (listCofirm == null || !listCofirm.Any()) return BadRequest("Not data approver");
            try
            {
                var baoGia = await _baoGiaService.UpdateApprover(listCofirm, UserApproverNext, GetCurrentUserId());
                if (!baoGia.Success) return BadRequest("Error Approval :" + baoGia.Message);
                var req = baoGia.Data;
                var userSend = UserApproverNext;
                var currentUserId = GetCurrentUserId();
                var listOk = req.Where(c => c.ID_Status != null && !c.ID_Status.Contains("RETURN")).ToList();
                var listNG = req.Where(c => c.ID_Status != null && c.ID_Status.Contains("RETURN")).ToList();
                if (listOk.Any())
                {
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                var baoGiaConfirmNameService = scope.ServiceProvider.GetRequiredService<IBaoGiaConfirmNameService>();
                                var baoGiaDetailService = scope.ServiceProvider.GetRequiredService<IBaoGiaDetailService>();

                                var listConfirm = new List<BaoGia_Confirm_Name_QuotationDTO>();
                                foreach (var material in listOk)
                                {
                                    var detailsResult = await baoGiaDetailService.GetByIdRequestQuoteAsync(material.ID);
                                    if (detailsResult == null || !detailsResult.Success || detailsResult.Data == null)
                                    {
                                        _logger.LogError("Không lấy được thông tin chi tiết báo giá cho ID: " + material.ID + " Error: " + detailsResult?.Message);
                                        continue;
                                    }
                                    if (material.ID_StepBaoGia >= 12 && detailsResult.Data.BIT_Select == true)
                                    {
                                        var cf = new BaoGia_Confirm_Name_QuotationDTO
                                        {
                                            ID_RequestQuote = material.ID,
                                            DTM_CreateDate = DateTime.Now,
                                            VCHR_CreateBy = currentUserId,
                                            VCHR_TenRecomment = material.NVCHR_NameVN,
                                            CHR_Status = "",
                                            CHR_StatusACC = "Confirmed",
                                            CHR_StatusShip = "Confirming",
                                            NVCHR_Note = material.CHR_MaHangNCC
                                        };
                                        listConfirm.Add(cf);
                                    }
                                }
                                if (string.IsNullOrEmpty(userSend))
                                {
                                    var approverNext = await sendMailService.SendMailToRequesterAsync("", 11);
                                    userSend = approverNext?.Data ?? "";
                                }
                                else
                                {
                                    userSend = userSend + "@brothergroup.net";
                                }
                                await sendMailService.SendMailAsync(userSend, "", 14, "QuoteQuotationResults/Quotation_Results",
                                    listOk.FirstOrDefault()?.CHR_Gap == "false" ? false : true, listOk.FirstOrDefault()?.CHR_SectionCode ?? "",
                                    listOk.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                                if (listConfirm.Any())
                                {
                                    await baoGiaConfirmNameService.AddListAsync(listConfirm);
                                    await sendMailService.SendMailToConfirmItemAsync(13, 17, "Material/ConfirmName", true, "", "", currentUserId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                            }
                        }
                    });
                }

                var result = await _approverService.GetApproverByStepAndSectionAsync(4, "3110");
                if (!result.Success)
                {
                    return BadRequest("Không lấy được thông tin PIC phụ trách: " + result.Message);
                }
                var dataPic = result.Data;
                if (listNG.Any())
                {
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                string emailList = string.Join("; ", dataPic.Select(x => x.CHR_UserAdid + "@brothergroup.net"));

                                await sendMailService.SendMailAsync("khanhmf@brothergroup.net;" + emailList, "", 15, "QuoteQuotationResults/Quotation_Results",
                                    listNG.FirstOrDefault()?.CHR_Gap == "false" ? false : true,
                                    listNG.FirstOrDefault()?.CHR_SectionCode ?? "", listNG.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                            }
                        }
                    });
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(" Error Approval: " + ex.Message);
            }
        }

        private string GetApprovalStatus(int currentStep, int requiredStep, string? reason)
        {
            if (currentStep <= requiredStep) return "";
            return string.IsNullOrEmpty(reason) ? "OK" : "NG";
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
    }
}
