using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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
using Path = System.IO.Path;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class QuoteController : BaseAuthController
    {
        private readonly ILogger<QuoteController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IMaterialService _materialService;
        private readonly ITmSectionService _tmSectionService;
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
        private readonly IDepartmentService _deparmentService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IFileImportService _fileImportService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IStringLocalizer<QuoteController> _localizer;

        public QuoteController(ILogger<QuoteController> logger, ITmNccNewService tmNccNewService, IConfiguration configuration,
            IBaoGiaService baoGiaService, IMaterialService materialService, ITmSectionService tmSectionService, IExchangeRateService exchangeRateService,
           IDepartmentService deparmentService, IBaoGiaNCCService baoGiaNCCService, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStepService baoGiaStepService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaDetailService baoGiaDetailService, IBaoGiaConfirmNameService baoGiaConfirmNameService,
            ITmCategoryService tmCategoryService, IBaoGiaNccCategoryService baoGiaNccCategoryService, ITmEmployeeAgentService tmEmployeeAgentService,
            IWebHostEnvironment env, ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IMasterApproverSendMailService approverService,
            IStringLocalizer<QuoteController> localizer,
            IFileImportService fileImportService)
        {
            _logger = logger;
            _configuration = configuration;
            _tmNccNewService = tmNccNewService;
            _baoGiaService = baoGiaService;
            _materialService = materialService;
            _tmSectionService = tmSectionService;
            _baoGiaNCCService = baoGiaNCCService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaDetailService = baoGiaDetailService;
            _tmCategoryService = tmCategoryService;
            _baoGiaConfirmNameService = baoGiaConfirmNameService;
            _baoGiaNccCategoryService = baoGiaNccCategoryService;
            _tmEmployeeAgentService = tmEmployeeAgentService;
            _sendMailService = sendMailService;
            _env = env;
            _serviceScopeFactory = serviceScopeFactory;
            _approverService = approverService;
            _deparmentService = deparmentService;
            _exchangeRateService = exchangeRateService;
            _baoGiaStepService = baoGiaStepService;
            _localizer = localizer;
            _fileImportService = fileImportService;
        }
        // MARK: - Quote
        public async Task<IActionResult> Index()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";

            //var a = GetRolesUser();
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            // Load approver list for current user's section if available
            try
            {
                var section = GetCurrentUserSection() ?? string.Empty;
                var approverResp = await _approverService.GetApproverByStepAndSectionAsync(2, section);
                if (approverResp != null && approverResp.Success && approverResp.Data != null)
                {
                    vm.ListApprovel = approverResp.Data;
                }
            }
            catch
            {
                // ignore failures here; client JS can request approvers on-demand
            }
            return View(vm);
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
        // MARK: - Quotation Results
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
                NguoiThaoTac = GetCurrentUserId() ?? "",
                //DanhSachBaoGiaGomNhom = danhSach.Data.Data ?? new List<dynamic>()
            };
            return View(vm);
        }
        // Search Infor table tab supplierQuoteBody
        [HttpPost]
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
        // Xuất dữ liệu để lựa chọn nhà cung cấp
        [HttpPost]
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
                // Calculate totals for system columns
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
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                int rowStart = 4;
                foreach (var item in dataList)
                {
                    int col = 1;
                    // BIVN Input
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
                    // Vendor input
                    if (!item.IsMatch_MaHangNCC)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.CodeEquipmentNCC ?? string.Empty);

                    if (!item.IsMatch_NameVN)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                    if (!item.IsMatch_NameEN)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.NameENByNCC ?? string.Empty);
                    if (!item.IsMatch_SoLuong)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.soluong ?? 0); // Vendor quantity
                    if (!item.IsMatch_DonVi)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.donvi ?? string.Empty); // Vendor unit
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty); // Vendor maker
                    ws.Cell(rowStart, col++).SetValue(item.FL_USD ?? 0.0);
                    ws.Cell(rowStart, col++).SetValue(item.FL_VND ?? 0.0);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MOQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Packing ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_LeadTime ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ShipTime?.ToString("dd/MM/yyyy") ?? string.Empty);
                    if (!item.IsMatch_Rohs)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_Rohs ?? string.Empty);
                    if (!item.IsMatch_COCQ)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_COCQ ?? string.Empty);
                    if (!item.IsMatch_MSDS)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_MSDS ?? string.Empty);
                    if (!item.IsMatch_AnToan)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_AnToan ?? string.Empty);
                    if (!item.IsMatchCamKet)
                    {
                        ws.Cell(rowStart, col).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    ws.Cell(rowStart, col++).SetValue(item.VCHR_CamKet ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DeliveryTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_PaymentTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_File ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    // System count
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}";
                    var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                    string totalCell = "";

                    var enUs = new CultureInfo("en-US");
                    if (tot.Item1 != 0)
                    {
                        totalCell = tot.Item1.ToString("N0", enUs) + " VND";
                    }
                    else if (tot.Item2 != 0)
                    {
                        totalCell = Math.Round(tot.Item2, 4).ToString("0.0000", enUs) + " USD";
                    }
                    ws.Cell(rowStart, col++).SetValue(totalCell);
                    ws.Cell(rowStart, col++).SetValue(""); // BIT_Select placeholder
                    ws.Cell(rowStart, col++).SetValue(""); // NVCHR_ReasonPick placeholder
                    rowStart++;
                }

                // Thêm tổng vào Excel
                //int rowTotal = rowStart;
                //foreach (var total in totals)
                //{
                //    var items = total.Key.Split('|');
                //    string maDon = items[0];
                //    string maThietBi = items[1];
                //    string maNCC = items[2];

                //    // Tìm dòng tương ứng để cập nhật tổng
                //    var rowIndex = dataList.FindIndex(d => d.CHR_MaDon == maDon && d.CHR_MaThietBi == maThietBi && d.CHR_MaNCC == maNCC);
                //    if (rowIndex >= 0)
                //    {
                //        // Cập nhật tổng vào các ô tương ứng
                //        ws.Cell(rowIndex + 4, 8).SetValue(totals[total.Key].usd); // Tổng USD
                //        ws.Cell(rowIndex + 4, 9).SetValue(totals[total.Key].vnd); // Tổng VND
                //    }
                //}

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
        // Xuất dữ liệu để lựa chọn phê duyệt NCC
        [HttpPost]
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

                // Thu thập tất cả dữ liệu để kiểm tra lỗi
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

                // Kiểm tra lỗi cho từng dòng
                var errorDetails = new Dictionary<string, List<string>>();
                foreach (var rowData in allRowsData)
                {
                    var errors = new List<string>();
                    var maDon = rowData.MaDon;
                    var maHangNB = rowData.MaHangNoiBo;
                    var maThietBi = rowData.MaThietBi;
                    var chungLoaiHang = rowData.ChungLoaiHang;
                    var bitSelect = rowData.BIT_Select;
                    var reason = rowData.NVCHR_ReasonPick;
                    var tenHangEng = rowData.TenHangEng;
                    var tenHangVN = rowData.TenHangVN;
                    var codeVender = rowData.CodeVender;
                    var maHangNCC = !string.IsNullOrEmpty(rowData.MaHangNCC_Vendor)
                        ? rowData.MaHangNCC_Vendor
                        : rowData.MaHangNCC_BIVN;
                    var donGiaUSD = rowData.DonGiaUSD;
                    var donGiaVND = rowData.DonGiaVND;
                    string id = rowData.ID;

                    // Kiểm tra lỗi
                    var vendorCodesForSameProduct = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O") && x.MaThietBi == maThietBi)
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();

                    if (vendorCodesForSameProduct >= 2 && bitSelect.Contains("O"))
                    {
                        errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct]);
                    }

                    var hasAnySelect = allRowsData.Any(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O"));
                    if (!hasAnySelect && !string.IsNullOrEmpty(maHangNB))
                    {
                        errors.Add(_localizer["VendorCodeNotSelected", maHangNB]);
                    }

                    var vendorsForEquipmentAndCategory = allRowsData
                        .Where(x => (x.MaThietBi == maThietBi && x.ChungLoaiHang == chungLoaiHang && x.BIT_Select.Contains("O")) && !string.IsNullOrEmpty(maThietBi))
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();

                    if (vendorsForEquipmentAndCategory > 1 && bitSelect.Contains("O"))
                    {
                        errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory]);
                    }

                    var tenHangList = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB)
                        .Select(x => new { TenEng = x.TenHangEng, TenVN = x.TenHangVN })
                        .Distinct()
                        .ToList();

                    if (tenHangList.Count > 1)
                    {
                        errors.Add(_localizer["MaterialNameMismatch", maHangNB, string.Join(", ", tenHangList.Select(x => x.TenEng))]);
                    }

                    if (bitSelect.Contains("O"))
                    {
                        var allPricesForProduct = allRowsData
                            .Where(x => x.MaHangNoiBo == maHangNB && x.DonGiaUSD > 0)
                            .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                            .ToList();

                        if (allPricesForProduct.Any() && allPricesForProduct.Count > 1)
                        {
                            decimal minPriceUSD = (decimal)allPricesForProduct.Min(x => x.DonGiaUSD);
                            decimal currentPriceUSD = (decimal)donGiaUSD;

                            if (Math.Abs(currentPriceUSD - minPriceUSD) > 0.01m)
                            {
                                errors.Add(_localizer["SelectedPriceNotLowest", currentPriceUSD.ToString("N2"), minPriceUSD.ToString("N2")]);
                            }
                        }
                    }

                    var duplicatePrice = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB &&
                               ((!string.IsNullOrEmpty(x.MaHangNCC_Vendor) && x.MaHangNCC_Vendor == maHangNCC) ||
                                (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender)
                        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                        .Distinct()
                        .Count();

                    if (duplicatePrice > 1)
                    {
                        errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC]);
                    }

                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason))
                    {
                        errors.Add(_localizer["SelectedVendorNoReason"]);
                    }

                    // Lưu lỗi vào dictionary với key là ID
                    if (errors.Any())
                    {
                        errorDetails[id] = errors;
                    }
                }

                // Calculate totals for system columns
                var totals = new Dictionary<string, (double vnd, double usd)>();
                foreach (var item in dataList)
                {
                    string key = $"{item.CHR_MaDon ?? ""}|" +
                        $"{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}" +
                        $"|{(string.IsNullOrEmpty(item.CodeEquipmentNCC) ? item.ID.ToString() : item.CodeEquipmentNCC)}";
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
                    return BadRequest(_localizer["TemplateNotFound", "TemplateQuotationResults.xlsx"]);
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest(_localizer["WorksheetNotFound"]);
                }
                int rowStart = 4;
                foreach (var item in dataList)
                {
                    var enUs = new CultureInfo("en-US");
                    int col = 1;
                    // BIVN Input
                    ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(
                        item.ID_StepBaoGia == 9 ? "Chief/Expert Approval" : (item.ID_StepBaoGia == 10 ? "Section Manager Approval" : "Dept Manager Approval"));
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
                    // Vendor input
                    // Mã hàng NCC - tô màu nếu không khớp
                    var codeEquipmentCell = ws.Cell(rowStart, col++);
                    codeEquipmentCell.SetValue(item.CodeEquipmentNCC ?? string.Empty);
                    if (!item.IsMatch_MaHangNCC)
                    {
                        codeEquipmentCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Tên hàng tiếng Việt - tô màu nếu không khớp
                    var nameVNCell = ws.Cell(rowStart, col++);
                    nameVNCell.SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                    if (!item.IsMatch_NameVN)
                    {
                        nameVNCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Tên hàng tiếng Anh - tô màu nếu không khớp
                    var nameENCell = ws.Cell(rowStart, col++);
                    nameENCell.SetValue(item.NameENByNCC ?? string.Empty);
                    if (!item.IsMatch_NameEN)
                    {
                        nameENCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Số lượng - tô màu nếu không khớp
                    var quantityCell = ws.Cell(rowStart, col++);
                    quantityCell.SetValue(item.soluong ?? 0);
                    if (!item.IsMatch_SoLuong)
                    {
                        quantityCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Đơn vị - tô màu nếu không khớp
                    var unitCell = ws.Cell(rowStart, col++);
                    unitCell.SetValue(item.donvi ?? string.Empty);
                    if (!item.IsMatch_DonVi)
                    {
                        unitCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty); // Vendor maker
                    ws.Cell(rowStart, col++).SetValue((item.FL_USD ?? 0));
                    ws.Cell(rowStart, col++).SetValue((item.FL_VND ?? 0).ToString("N0", enUs));
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_MOQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Packing ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_LeadTime ?? string.Empty);

                    // Ngày giao hàng - tô màu nếu không khớp
                    var shipTimeCell = ws.Cell(rowStart, col++);
                    shipTimeCell.SetValue(item.DTM_ShipTime ?? string.Empty);
                    if (!item.IsMatch_Ngay)
                    {
                        shipTimeCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Rohs - tô màu nếu không khớp
                    var rohsCell = ws.Cell(rowStart, col++);
                    rohsCell.SetValue(item.VCHR_Rohs ?? string.Empty);
                    if (!item.IsMatch_Rohs)
                    {
                        rohsCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // CO/CQ - tô màu nếu không khớp
                    var cocqCell = ws.Cell(rowStart, col++);
                    cocqCell.SetValue(item.VCHR_COCQ ?? string.Empty);
                    if (!item.IsMatch_COCQ)
                    {
                        cocqCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // MSDS - tô màu nếu không khớp
                    var msdsCell = ws.Cell(rowStart, col++);
                    msdsCell.SetValue(item.VCHR_MSDS ?? string.Empty);
                    if (!item.IsMatch_MSDS)
                    {
                        msdsCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // An toàn - tô màu nếu không khớp
                    var anToanCell = ws.Cell(rowStart, col++);
                    anToanCell.SetValue(item.VCHR_AnToan ?? string.Empty);
                    if (!item.IsMatch_AnToan)
                    {
                        anToanCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Cam kết - tô màu nếu không khớp
                    var camKetCell = ws.Cell(rowStart, col++);
                    camKetCell.SetValue(item.VCHR_CamKet ?? string.Empty);
                    if (!item.IsMatchCamKet)
                    {
                        camKetCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_DeliveryTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_PaymentTerm ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_File ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                    // System count
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}" +
                        $"|{item.CHR_MaNCC ?? ""}|{(string.IsNullOrEmpty(item.CodeEquipmentNCC) ? item.ID.ToString() : item.CodeEquipmentNCC)}";
                    var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                    string totalCell = "";
                    if (tot.Item1 != 0)
                    {
                        totalCell = tot.Item1.ToString("N0", enUs) + " VND";
                    }
                    else if (tot.Item2 != 0)
                    {
                        totalCell = Math.Round(tot.Item2, 4).ToString("0.0000", enUs) + " USD";
                    }
                    ws.Cell(rowStart, col++).SetValue(totalCell);
                    ws.Cell(rowStart, col++).SetValue(item.BIT_Select == true ? "O" : "X");
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_ReasonPick ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_Note ?? string.Empty);
                    // Approval
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 9 ? item.UserQlsc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 9, item.LyDoQlsc));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 9 ? item.LyDoQlsc ?? "" : "");

                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 10 ? item.UserQltc ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 10, item.LyDoQltc));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 10 ? item.LyDoQltc ?? "" : "");

                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 11 ? item.UserDeft ?? "" : "");
                    ws.Cell(rowStart, col++).SetValue(GetApprovalStatus(item.ID_StepBaoGia, 11, item.LyDoDeft));
                    ws.Cell(rowStart, col++).SetValue(item.ID_StepBaoGia > 11 ? item.LyDoDeft ?? "" : "");

                    // Thêm cột "Lỗi chi tiết" vào cuối
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

        private string GetApprovalStatus(int currentStep, int requiredStep, string? reason)
        {
            if (currentStep <= requiredStep) return "";
            return string.IsNullOrEmpty(reason) ? "OK" : "NG";
        }
        // Nhập lựa chọn báo giá file excel
        [HttpPost]
        public async Task<IActionResult> ImportApprovalQuotianExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                var isErrors = false;
                // Dữ liệu bắt đầu từ dòng 4
                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    var errors = new List<string>();
                    var maDon = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrEmpty(maDon))
                    {
                        break;
                    }
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
                        BIT_Select = resultApproval == "NG" ? false : true,
                        NVCHR_LyDo = resonApproval,
                        ID_Step = status == "Chief/Expert Approval" ? 10 : (status == "Section Manager Approval" ? 11 : 11),
                        Errors = string.Join("; ", errors)
                    });
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
                if (items.Count == 0)
                {
                    return BadRequest("Không nhận được dữ liệu hợp lệ ");
                }

                // If any item is in Chief/Expert Approval step (mapped to 10) ask client to select next approver
                var requiresChiefSelection = items.Any(i => i.ID_Step == 10);

                var listApproval = new List<ApproverDTO>();
                var step = 0;
                foreach (var item in items)
                {
                    if (step == 0)
                    {
                        step = item.ID_Step;
                    }
                    listApproval.Add(new ApproverDTO
                    {
                        Id = int.Parse(item.ID.ToString()),
                        IsApproved = item.BIT_Select,
                        Reason = item.NVCHR_LyDo.ToString(),
                    });
                }

                if (requiresChiefSelection)
                {
                    var approverResult = await _approverService.GetApproverByStepAndSectionAsync(10, "");
                    if (!approverResult.Success)
                    {
                        return BadRequest("Error list Approver: " + approverResult.Message);
                    }
                    return Ok(new
                    {
                        RequiresSelection = true,
                        Step = 10,
                        Approvers = approverResult.Data,
                        Items = items
                    });
                }

                // lấy user phê duyệt theo step và section hiện tại
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

        [HttpPost]
        public async Task<IActionResult> ConfirmImportedApprovals([FromBody] ConfirmImportedApprovalsRequest req)
        {
            if (req == null || req.Items == null || !req.Items.Any())
                return BadRequest("No items provided");

            try
            {
                var listApproval = new List<ApproverDTO>();
                foreach (var it in req.Items)
                {
                    listApproval.Add(new ApproverDTO
                    {
                        Id = it.ID,
                        IsApproved = it.BIT_Select,
                        Reason = it.NVCHR_LyDo ?? string.Empty,
                    });
                }
                var approverAdid = req.SelectedApprover ?? "";
                return await PheDuyetBaoGia(listApproval, approverAdid == string.Empty ? "vuthipt" : approverAdid);
            }
            catch (Exception ex)
            {
                return BadRequest("Error processing approvals: " + ex.Message);
            }
        }
        // Nhập lựa chọn báo giá file excel
        [HttpPost]
        public async Task<IActionResult> ImportQuotianExcel([FromForm] ImportPickSupplier vm)
        {
            if (vm.fileSend == null || vm.fileSend.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = vm.fileSend.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
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
                    var tenHangVN_Vendor = ws.Cell(r, 29).GetString();
                    var tenHangEng_Vendor = ws.Cell(r, 30).GetString();
                    var soLuong_Vendor = ws.Cell(r, 31).GetDouble();
                    var donVi_Vendor = ws.Cell(r, 32).GetString();
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
                    var tenHangEng = rowData.TenHangEng;
                    var tenHangVN = rowData.TenHangVN;
                    var codeVender = rowData.CodeVender;
                    var maHangNCC = !string.IsNullOrEmpty(rowData.MaHangNCC_Vendor)
                        ? rowData.MaHangNCC_Vendor
                        : rowData.MaHangNCC_BIVN;
                    var donGiaUSD = rowData.DonGiaUSD;
                    var donGiaVND = rowData.DonGiaVND;
                    int row = rowData.Row;
                    string id = rowData.ID;
                    var NVCHR_Note = rowData.NVCHR_Note;

                    var vendorCodesForSameProduct = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O") && x.MaThietBi == maThietBi)
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();

                    if (vendorCodesForSameProduct >= 2 && bitSelect.Contains("O"))
                    {
                        errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct]);
                    }
                    // Kiểm tra nếu có chọn 'O' thì phải có ít nhất 1 dòng khác cùng mã hàng nội bộ cũng chọn 'O'
                    //var hasAnySelect = allRowsData.Any(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O"));
                    //if (!hasAnySelect && !string.IsNullOrEmpty(maHangNB))
                    //{
                    //    errors.Add($"Mã hàng nội bộ '{maHangNB}' chưa chọn bất kỳ Vendor Code nào (thiếu 'O' tại cột 51)");
                    //}

                    var vendorsForEquipmentAndCategory = allRowsData
                        .Where(x => (x.MaThietBi == maThietBi
                        && x.ChungLoaiHang == chungLoaiHang
                        && x.BIT_Select.Contains("O") && x.MaHangNCC_BIVN == maHangNCC)
                        && !string.IsNullOrEmpty(maThietBi))
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();

                    if (vendorsForEquipmentAndCategory > 1 && bitSelect.Contains("O"))
                    {
                        errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory]);
                    }

                    //var tenHangList = allRowsData
                    //    .Where(x => x.MaHangNoiBo == maHangNB)
                    //    .Select(x => new { TenEng = x.TenHangEng, TenVN = x.TenHangVN })
                    //    .Distinct()
                    //    .ToList();

                    //if (tenHangList.Count > 1)
                    //{
                    //    errors.Add(_localizer["MaterialNameMismatch", maHangNB, string.Join(", ", tenHangList.Select(x => x.TenEng))]);
                    //}

                    //if (bitSelect.Contains("O"))
                    //{
                    //    var allPricesForProduct = allRowsData
                    //        .Where(x => x.MaHangNoiBo == maHangNB && x.DonGiaUSD > 0 && x.DonGiaVND > 0)
                    //        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                    //        .ToList();

                    //    if (allPricesForProduct.Any() && allPricesForProduct.Count > 1)
                    //    {
                    //        decimal minPriceUSD = (decimal)allPricesForProduct.Min(x => x.DonGiaUSD);
                    //        decimal currentPriceUSD = (decimal)donGiaUSD;

                    //        if (Math.Abs(currentPriceUSD - minPriceUSD) > 0.01m)
                    //        {
                    //            errors.Add($"Đơn giá được chọn (USD: {currentPriceUSD:N2}) không phải giá thấp nhất (USD: {minPriceUSD:N2})");
                    //        }
                    //    }
                    //}

                    var duplicatePrice = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB &&
                               ((!string.IsNullOrEmpty(x.MaHangNCC_Vendor) && x.MaHangNCC_Vendor == maHangNCC) ||
                                (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender)
                        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                        .Distinct()
                        .Count();

                    if (duplicatePrice > 1)
                    {
                        errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC]);
                    }

                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason))
                    {
                        errors.Add(_localizer["SelectedVendorNoReasonColumn", 52]);
                    }

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
                        // check mã hàng nội bộ theo mã thiết bị
                        //var check = items.Where(c => c.CHR_MaDon == maDon
                        //&& c.CHR_MaHangNoiBo == maHangNB
                        //&& c.BIT_Select && c.CHR_MaThietBi == maThietBi).ToList();
                        //if (check.Any() && bitSelect.Contains("O"))
                        //{
                        //    isErrors = true;
                        //    errorRows.Add(new
                        //    {
                        //        Row = row,
                        //        MaDon = maDon,
                        //        ID = id,
                        //        MaHangNoiBo = maHangNB,
                        //        VendorCode = maHangNCC,
                        //        BIT_Select = bitSelect,
                        //        DonGiaUSD = donGiaUSD,
                        //        NVCHR_ReasonPick = reason,
                        //        Errors = "Trong 1 mã đơn, 1 hàng nội bộ chỉ được chọn 1 nhà báo giá (O)"
                        //    });
                        //}
                        //else
                        //{
                        //    items.Add(new
                        //    {
                        //        ID = id,
                        //        BIT_Select = bitSelect.Contains("O"),
                        //        NVCHR_ReasonPick = reason,
                        //        CHR_MaDon = maDon,
                        //        CHR_MaHangNoiBo = maHangNB,
                        //        NVCHR_Note = NVCHR_Note,
                        //        CHR_MaThietBi = maThietBi
                        //    });
                        //}
                        items.Add(new
                        {
                            ID = id,
                            BIT_Select = bitSelect.Contains("O"),
                            NVCHR_ReasonPick = reason,
                            CHR_MaDon = maDon,
                            CHR_MaHangNoiBo = maHangNB,
                            NVCHR_Note = NVCHR_Note,
                            CHR_MaThietBi = maThietBi
                        });
                    }
                }

                if (isErrors)
                {
                    using var errorWorkbook = new ClosedXML.Excel.XLWorkbook();
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
                else
                {
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
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ImportQuotianExcelOld([FromForm] ImportPickSupplier vm)
        {
            if (vm.fileSend == null || vm.fileSend.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<dynamic>();
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

                for (int r = startRow; r <= lastRow; r++)
                {
                    var errors = new List<string>();
                    var maDon = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrEmpty(maDon))
                    {
                        break;
                    }
                    var id = ws.Cell(r, 3).GetString();
                    var bitSelect = ws.Cell(r, 51).GetString();
                    var reason = ws.Cell(r, 52).GetString();
                    var maHangNB = ws.Cell(r, 5).GetString();

                    // Validate
                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason))
                    {
                        isErrors = true;
                        errors.Add("Hàng chưa được chọn nhà cung cấp nhưng đã có lý do từ chối");
                    }

                    if (errors.Any())
                    {
                        errorRows.Add(new
                        {
                            Row = r,
                            MaDon = maDon,
                            ID = id,
                            BIT_Select = bitSelect,
                            NVCHR_ReasonPick = reason,
                            Errors = string.Join("; ", errors)
                        });
                    }
                    else
                    {
                        var check = items.Where(c => c.CHR_MaDon == maDon && c.CHR_MaHangNoiBo == maHangNB && c.BIT_Select).ToList();
                        if (check.Any() && bitSelect.Contains("O"))
                        {
                            isErrors = true;
                            errorRows.Add(new
                            {
                                Row = r,
                                MaDon = maDon,
                                ID = id,
                                BIT_Select = bitSelect,
                                NVCHR_ReasonPick = reason,
                                Errors = string.Join("; ", " Trong 1 mã đơn , 1 hàng nội bộ chỉ dc chọn 1 nhà báo giá")
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
                                CHR_MaHangNoiBo = maHangNB
                            });
                        }
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
                    // Update database
                    var dtoList = items.Select(i => new BaoGia_Detail_of_QuotationDTO
                    {
                        ID = int.Parse(i.ID.ToString()),
                        BIT_Select = (bool)i.BIT_Select,
                        NVCHR_ReasonPick = i.NVCHR_ReasonPick.ToString(),
                        CHR_UpdateBy = GetCurrentUserId()
                    }).ToList();
                    var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(dtoList, vm.userNextApproval);
                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }
                    return Ok(new { message = "Import successful" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Save pick supplier
        [HttpPost]
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
                        await sendMailService.SendMailAsync(userSend + "@brothergroup.net", "", 14, "Quote/Quotation_Results", req.CHR_Gap == "false" ? false : true, req.CHR_SectionCode ?? "", req.CHR_MaDon ?? "", currentUserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                    }
                }

            });
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
        // MARK: - Select Quote Section
        public async Task<IActionResult> SelectQuoteSection()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccNews = await LoadNhaCungCapDataAsync();
            var madons = await LoadMadonAsync(13);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            return View(vm);
        }
        // MARK: - HistoryQuote
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

            return View(vm);
        }
        // MARK: Lấy các thông tin
        private async Task<List<TM_SECTIONDTO>> LoadSectionDataAsync()
        {
            var sections = await _tmSectionService.GetAllSectionsAsync();
            return sections.Data ?? new List<TM_SECTIONDTO>();
        }
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
        // lấy thông tin mặt hàng
        [HttpGet]
        public async Task<IActionResult> GetMaterialsByNameOrCode(string keyword)
        {
            var result = await _materialService.GetMaterialsByNameOrCodeAsync(keyword);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // Insert dữ liệu báo giá
        [HttpPost]
        public async Task<IActionResult> InsertBaoGia([FromBody] BaoGia_Request_of_QuotationDTO baoGia)
        {
            var result = await _baoGiaService.NhapBaoGiaAsync(baoGia);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // Insert danh sách báo giá
        [HttpPost]
        public async Task<IActionResult> InsertDanhSachBaoGia([FromBody] List<BaoGia_Request_of_QuotationDTO> danhSachBaoGia)
        {
            if (danhSachBaoGia == null || !danhSachBaoGia.Any())
            {
                return BadRequest("Not data empty");
            }
            var distinctLinks = danhSachBaoGia.Select(b => b.CHR_LinkFile)
                                               .Where(s => !string.IsNullOrWhiteSpace(s))
                                               .Distinct(StringComparer.OrdinalIgnoreCase)
                                               .ToList();

            var savedMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var src in distinctLinks)
            {
                if (savedMap.ContainsKey(src)) continue;
                try
                {
                    var saveRes = await _fileImportService.SaveFileFromPathAsync(src);
                    if (saveRes != null && saveRes.Success && !string.IsNullOrWhiteSpace(saveRes.Data))
                    {
                        savedMap[src] = saveRes.Data;
                    }
                    else
                    {

                        savedMap[src] = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed saving link {Link}", src);
                    savedMap[src] = null;
                }
            }

            foreach (var dto in danhSachBaoGia)
            {
                if (string.IsNullOrWhiteSpace(dto.CHR_LinkFile)) continue;
                var checkKey = dto.CHR_LinkFile?.Trim().Trim('"', '\'') ?? dto.CHR_LinkFile;
                if (savedMap.TryGetValue(checkKey, out var saved) && !string.IsNullOrWhiteSpace(saved))
                {
                    dto.CHR_LinkFile = saved;
                }

            }

            var result = await _baoGiaService.NhapDanhSachBaoGiaAsync(danhSachBaoGia);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            try
            {
                var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                var currentUserId = GetCurrentUserId();
                var currentUserFullName = GetCurrentUserFullName();
                var userAppproval = result.Data?.FirstOrDefault()?.CHR_UserApproval ?? "";
                var histories = insertedList.Select(b => new BaoGia_History_Request_of_QuotationDTO
                {
                    ID_RequestQuote = b.ID,
                    CHR_MaDon = b.CHR_MaDon ?? string.Empty,
                    CHR_UpdateBy = currentUserId ?? string.Empty,
                    NVCHR_UpdateName = currentUserFullName ?? string.Empty,
                    CHR_Updatedate = DateTime.Now,
                    CHR_ChangedColumns = null,
                    CHR_OldData = null,
                    CHR_NewData = System.Text.Json.JsonSerializer.Serialize(b),
                    NVCHR_LyDo = b.NVCHR_LyDo,
                    CHR_ActionType = "INSERT"
                }).ToList();

                if (histories.Any())
                {
                    await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                }
                // Gui mail phe duyet trong background
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
            }
            catch
            {
                return BadRequest(result.Message);
            }

            return Ok(danhSachBaoGia);
        }
        // Delete danh sách báo giá theo mã đơn
        [HttpPost]
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
        // update thông tin báo giá 
        [HttpPost]
        public async Task<IActionResult> UpdateBaoGias([FromBody] List<BaoGia_Request_of_QuotationDTO> baogia)
        {
            var result = await _baoGiaService.CapNhatDanhSachBGAsync(baogia);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            // result.Data contains inserted DTOs with IDs
            try
            {
                var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
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
                    CHR_ActionType = "UPDATE"
                }).ToList();

                if (histories.Any())
                {
                    await _baoGiaHistoryService.InsertHistoryListAsync(histories);
                }
            }
            catch
            {
                return BadRequest("Lỗi ghi lịch sử cập nhật báo giá");
            }

            return Ok(result.Data);
        }
        // update bao gia theo id
        [HttpPost]
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
            // result.Data contains inserted DTOs with IDs
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
        // Xóa dữ liệu báo giá
        [HttpPost]
        public async Task<IActionResult> DeleteBaoGia([FromBody] int id)
        {
            var result = await _baoGiaService.DeleteAsync(id);
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
        // Lấy thông tin NCC theo loại hàng 
        [HttpPost]
        public async Task<IActionResult> GetNCCByCategory([FromBody] string category)
        {
            var result = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(category);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
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
        // Download file dữ liệu đang có trong bảng 
        [HttpPost]
        public async Task<IActionResult> ExportTable([FromBody] List<BaoGia_Request_of_QuotationDTO> items)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemPlateQuote.xlsx");
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

                int row = 10;
                foreach (var rq in items)
                {
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, 1).SetValue(row - 9); // status placeholder
                    ws.Cell(row, 2).SetValue(rq?.CHR_SectionCode ?? string.Empty);
                    ws.Cell(row, 3).SetValue(rq?.CHR_SectionName ?? string.Empty);
                    ws.Cell(row, 4).SetValue(rq?.CHR_Phanloai ?? string.Empty);
                    ws.Cell(row, 5).SetValue(rq?.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(row, 6).SetValue(rq?.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(row, 7).SetValue(rq?.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, 8).SetValue(rq?.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(row, 9).SetValue(rq?.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, 10).SetValue(rq?.INT_SoLuong.HasValue == true ? rq.INT_SoLuong.Value : 0);
                    ws.Cell(row, 11).SetValue(rq?.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(row, 12).SetValue(rq?.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(row, 13).SetValue(rq?.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(row, 14).SetValue(rq?.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(row, 15).SetValue(rq?.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(row, 16).SetValue(rq?.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(row, 17).SetValue(rq?.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(row, 18).SetValue(rq?.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(row, 19).SetValue(rq?.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(row, 20).SetValue(rq?.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(row, 21).SetValue(rq?.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(row, 22).SetValue(rq?.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(row, 23).SetValue(rq?.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(row, 24).SetValue(rq?.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(row, 25).SetValue(rq?.CHR_MaNCC ?? string.Empty);
                    ws.Cell(row, 26).SetValue(rq?.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(row, 27).SetValue(rq?.BIT_LayBaoGia == false ? "X" : "O");
                    ws.Cell(row, 28).SetValue(rq?.NVCHR_LyDo ?? string.Empty);
                    ws.Cell(row, 29).SetValue(rq?.DTM_NgayMuonNhan.HasValue == true ? rq.DTM_NgayMuonNhan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, 30).SetValue(rq?.DTM_KyHan.HasValue == true ? rq.DTM_KyHan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, 31).SetValue(rq?.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(row, 32).SetValue(rq?.NVCHR_UserRequest ?? string.Empty);
                    ws.Cell(row, 33).SetValue(rq?.NVCHR_ReasonQuotation ?? string.Empty);
                    ws.Cell(row, 34).SetValue(rq?.CHR_LinkFile ?? string.Empty);
                    row++;
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"TableQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }
        // Upload Excel để nhập danh sách yêu cầu báo giá
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadQuoteExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                    return BadRequest("Không tìm thấy worksheet");

                var items = await ProcessExcelWorksheet(ws);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        private async Task<List<BaoGia_Request_of_QuotationDTO>> ProcessExcelWorksheet(IXLWorksheet ws)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();
            const int startRow = 10;
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

            for (int r = startRow; r <= lastRow; r++)
            {
                var sectionCode = ws.Cell(r, 2).GetString();
                if (string.IsNullOrEmpty(sectionCode))
                    break;

                var rowData = ExtractRowData(ws, r);
                var processedItems = await ProcessRowData(rowData);
                items.AddRange(processedItems);
            }

            return items;
        }

        private ExcelRowData ExtractRowData(IXLWorksheet ws, int row)
        {
            return new ExcelRowData
            {
                SectionCode = ws.Cell(row, 2).GetString(),
                SectionName = ws.Cell(row, 3).GetString(),
                Phanloai = ws.Cell(row, 4).GetString(),
                MaThietBi = ws.Cell(row, 5).GetString(),
                MaHangNoiBo = ws.Cell(row, 6).GetString(),
                MaHangNCC = ws.Cell(row, 7).GetString(),
                NameVN = ws.Cell(row, 8).GetString(),
                NameEN = ws.Cell(row, 9).GetString(),
                SoLuong = ws.Cell(row, 10).GetString(),
                DonVi = ws.Cell(row, 11).GetString(),
                ChungLoai = ws.Cell(row, 12).GetString()?.Trim(),
                HinhDang = ws.Cell(row, 13).GetString(),
                ChatLieu = ws.Cell(row, 14).GetString(),
                ThanhPhan = ws.Cell(row, 15).GetString(),
                KichThuoc = ws.Cell(row, 16).GetString(),
                DongMay = ws.Cell(row, 17).GetString(),
                TinhNang = ws.Cell(row, 18).GetString(),
                Rohs = ws.Cell(row, 19).GetString(),
                COCQ = ws.Cell(row, 20).GetString(),
                MSDS = ws.Cell(row, 21).GetString(),
                AnToan = ws.Cell(row, 22).GetString(),
                FileThietKe = ws.Cell(row, 23).GetString(),
                NhaSanXuat = ws.Cell(row, 24).GetString(),
                MaNCC = ws.Cell(row, 25).GetString(),
                TenNCC = ws.Cell(row, 26).GetString(),
                LayBaoGia = ws.Cell(row, 27).GetString(),
                LyDo = ws.Cell(row, 28).GetString(),
                NgayMuonNhan = ws.Cell(row, 29).GetString(),
                KyHan = ws.Cell(row, 30).GetString(),
                Gap = ws.Cell(row, 31).GetString(),
                UserRequest = ws.Cell(row, 32).GetString(),
                ReasonQuote = ws.Cell(row, 33).GetString(),
                CHR_LinkFile = ws.Cell(row, 34).GetString()
            };
        }

        private async Task<List<BaoGia_Request_of_QuotationDTO>> ProcessRowData(ExcelRowData rowData)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();

            // Case 1: Có mã hàng nội bộ
            if (!string.IsNullOrEmpty(rowData.MaHangNoiBo))
            {
                var processedItems = await ProcessRowWithExistingMaterial(rowData);
                items.AddRange(processedItems);
            }
            // Case 2: Không có mã hàng nội bộ
            else
            {
                var processedItems = await ProcessRowWithoutMaterial(rowData);
                items.AddRange(processedItems);
            }

            return items;
        }

        private async Task<List<BaoGia_Request_of_QuotationDTO>> ProcessRowWithExistingMaterial(ExcelRowData rowData)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();
            var materialResp = await _materialService.GetByMaHangAsync(rowData.MaHangNoiBo);

            if (!materialResp.Success || materialResp.Data == null)
                return items;

            var material = materialResp.Data;
            var dto = CreateDtoFromMaterial(rowData, material);

            // Đã có thông tin nhà cung cấp
            if (!string.IsNullOrEmpty(dto.CHR_MaNCC))
            {
                items.Add(dto);
                return items;
            }

            // Lấy nhà cung cấp theo chủng loại hàng
            var suppliers = await GetSuppliersByCategory(dto.NVCHR_ChungLoai ?? "");
            items.AddRange(CreateDtosWithSuppliers(dto, suppliers, rowData));

            if (!items.Any())
                items.Add(dto);

            return items;
        }

        private async Task<List<BaoGia_Request_of_QuotationDTO>> ProcessRowWithoutMaterial(ExcelRowData rowData)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();
            var dto = CreateDtoFromRowData(rowData);

            // Đã có thông tin nhà cung cấp
            if (!string.IsNullOrEmpty(dto.CHR_MaNCC))
            {
                items.Add(dto);
                return items;
            }

            // Lấy nhà cung cấp theo chủng loại hàng
            var suppliers = await GetSuppliersByCategory(rowData.ChungLoai ?? "");
            items.AddRange(CreateDtosWithSuppliers(dto, suppliers, rowData));

            if (!items.Any())
                items.Add(dto);

            return items;
        }

        private BaoGia_Request_of_QuotationDTO CreateDtoFromMaterial(ExcelRowData rowData, dynamic material)
        {
            var currentUserId = GetCurrentUserId() ?? string.Empty;

            return new BaoGia_Request_of_QuotationDTO
            {
                CHR_SectionCode = rowData.SectionCode,
                CHR_SectionName = rowData.SectionName,
                CHR_Phanloai = material.LoaiHang,
                CHR_MaThietBi = rowData.MaThietBi,
                CHR_MaHangNoiBo = material.Material_Code,
                CHR_MaHangNCC = string.IsNullOrEmpty(rowData.MaHangNCC) ? material.Code_Suppiler : rowData.MaHangNCC,
                NVCHR_NameVN = material.NameVI,
                CHR_NameEN = material.Material_Name_EN,
                INT_SoLuong = ParseDouble(rowData.SoLuong),
                NVCHR_DonVi = string.IsNullOrEmpty(material.Unit) ? rowData.DonVi : material.Unit,
                NVCHR_ChungLoai = material.Category_VN,
                NVCHR_HinhDang = rowData.HinhDang ?? material.Shape,
                NVCHR_ChatLieu = rowData.ChatLieu ?? material.Material,
                NVCHR_ThanhPhan = rowData.ThanhPhan ?? material.Composition,
                NVCHR_KichThuoc = rowData.KichThuoc ?? material.Dimension,
                NVCHR_DongMay = rowData.DongMay ?? material.UsedFor,
                NVCHR_TinhNang = rowData.TinhNang ?? material.Purpose,
                NVCHR_Rohs = rowData.Rohs,
                NVCHR_COCQ = rowData.COCQ,
                NVCHR_MSDS = rowData.MSDS,
                NVCHR_AnToan = rowData.AnToan,
                NVCHR_FileThietKe = rowData.FileThietKe,
                NVCHR_NhaSanXuat = rowData.NhaSanXuat,
                CHR_MaNCC = rowData.MaNCC,
                NVCHR_TenNCC = rowData.TenNCC,
                BIT_LayBaoGia = ParseBool(rowData.LayBaoGia),
                NVCHR_LyDo = rowData.LyDo,
                DTM_NgayMuonNhan = ParseDate(rowData.NgayMuonNhan),
                DTM_KyHan = ParseDate(rowData.KyHan),
                CHR_Gap = ParseBool(rowData.Gap) == false ? "false" : "true",
                NVCHR_UserRequest = rowData.UserRequest ?? currentUserId,
                CHR_CreateBy = currentUserId,
                DTM_CreateDate = DateTime.Now,
                ID_Status = "CREATE",
                NVCHR_ReasonQuotation = rowData.ReasonQuote,
                CHR_LinkFile = rowData.CHR_LinkFile
            };
        }

        private BaoGia_Request_of_QuotationDTO CreateDtoFromRowData(ExcelRowData rowData)
        {
            var currentUserId = GetCurrentUserId() ?? string.Empty;

            return new BaoGia_Request_of_QuotationDTO
            {
                CHR_SectionCode = rowData.SectionCode,
                CHR_SectionName = rowData.SectionName,
                CHR_Phanloai = ParsePhanloai(rowData.Phanloai),
                CHR_MaThietBi = rowData.MaThietBi,
                CHR_MaHangNoiBo = rowData.MaHangNoiBo,
                CHR_MaHangNCC = rowData.MaHangNCC,
                NVCHR_NameVN = rowData.NameVN,
                CHR_NameEN = rowData.NameEN,
                INT_SoLuong = ParseDouble(rowData.SoLuong),
                NVCHR_DonVi = rowData.DonVi,
                NVCHR_ChungLoai = rowData.ChungLoai,
                NVCHR_HinhDang = rowData.HinhDang,
                NVCHR_ChatLieu = rowData.ChatLieu,
                NVCHR_ThanhPhan = rowData.ThanhPhan,
                NVCHR_KichThuoc = rowData.KichThuoc,
                NVCHR_DongMay = rowData.DongMay,
                NVCHR_TinhNang = rowData.TinhNang,
                NVCHR_Rohs = rowData.Rohs,
                NVCHR_COCQ = rowData.COCQ,
                NVCHR_MSDS = rowData.MSDS,
                NVCHR_AnToan = rowData.AnToan,
                NVCHR_FileThietKe = rowData.FileThietKe,
                NVCHR_NhaSanXuat = rowData.NhaSanXuat,
                CHR_MaNCC = rowData.MaNCC,
                NVCHR_TenNCC = rowData.TenNCC,
                BIT_LayBaoGia = ParseBool(rowData.LayBaoGia),
                NVCHR_LyDo = rowData.LyDo,
                DTM_NgayMuonNhan = ParseDate(rowData.NgayMuonNhan),
                DTM_KyHan = ParseDate(rowData.KyHan),
                CHR_Gap = ParseBool(rowData.Gap) == false ? "false" : "true",
                NVCHR_UserRequest = rowData.UserRequest ?? currentUserId,
                CHR_CreateBy = currentUserId,
                DTM_CreateDate = DateTime.Now,
                ID_Status = "CREATE",
                NVCHR_ReasonQuotation = rowData.ReasonQuote,
                CHR_LinkFile = rowData.CHR_LinkFile
            };
        }

        private async Task<List<BaoGia_NCC_CategoryDTO>> GetSuppliersByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return new List<BaoGia_NCC_CategoryDTO>();

            var suppliersResp = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(category);
            return (suppliersResp.Success && suppliersResp.Data != null) ? suppliersResp.Data : new List<BaoGia_NCC_CategoryDTO>();
        }

        private List<BaoGia_Request_of_QuotationDTO> CreateDtosWithSuppliers(BaoGia_Request_of_QuotationDTO baseDto, List<BaoGia_NCC_CategoryDTO> suppliers, ExcelRowData rowData)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();
            var currentUserId = GetCurrentUserId() ?? string.Empty;

            var first = true;
            foreach (var supplier in suppliers)
            {
                var dto = first ? baseDto : CloneDto(baseDto);
                dto.NVCHR_UserRequest = rowData.UserRequest ?? currentUserId;
                dto.CHR_MaNCC = supplier.CHR_MaNCC;
                dto.NVCHR_TenNCC = supplier.NVCHR_TenNCC;

                //if (string.IsNullOrEmpty(dto.NVCHR_NhaSanXuat))
                //dto.NVCHR_NhaSanXuat = supplier.NVCHR_SanXuat;

                dto.BIT_LayBaoGia = ParseBool(rowData.LayBaoGia);

                items.Add(dto);
                first = false;
            }

            return items;
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
        private static string? ParseNameHQ(string Catergory, string Shape, string Material, string Composition, string Dimension, string UsedFor, string Purpose)
        {
            return Catergory + "có hình dáng " + Shape + " chất liệu " + Material + " thành phần hóa chất " + Composition + " có kích thước " + Dimension + " dùng để " + UsedFor + " cho " + Purpose;
        }

        private static BaoGia_Request_of_QuotationDTO CloneDto(BaoGia_Request_of_QuotationDTO src)
        {
            return new BaoGia_Request_of_QuotationDTO
            {
                CHR_MaDon = src.CHR_MaDon,
                CHR_MaThietBi = src.CHR_MaThietBi,
                CHR_Phanloai = src.CHR_Phanloai,
                CHR_MaHangNoiBo = src.CHR_MaHangNoiBo,
                CHR_MaHangNCC = src.CHR_MaHangNCC,
                NVCHR_NameVN = src.NVCHR_NameVN,
                CHR_NameEN = src.CHR_NameEN,
                INT_SoLuong = src.INT_SoLuong,
                NVCHR_DonVi = src.NVCHR_DonVi,
                NVCHR_ChungLoai = src.NVCHR_ChungLoai,
                NVCHR_HinhDang = src.NVCHR_HinhDang,
                NVCHR_ChatLieu = src.NVCHR_ChatLieu,
                NVCHR_ThanhPhan = src.NVCHR_ThanhPhan,
                NVCHR_KichThuoc = src.NVCHR_KichThuoc,
                NVCHR_DongMay = src.NVCHR_DongMay,
                NVCHR_TinhNang = src.NVCHR_TinhNang,
                NVCHR_Rohs = src.NVCHR_Rohs,
                NVCHR_COCQ = src.NVCHR_COCQ,
                NVCHR_MSDS = src.NVCHR_MSDS,
                NVCHR_AnToan = src.NVCHR_AnToan,
                NVCHR_FileThietKe = src.NVCHR_FileThietKe,
                NVCHR_NhaSanXuat = src.NVCHR_NhaSanXuat,
                NVCHR_LyDo = src.NVCHR_LyDo,
                DTM_NgayMuonNhan = src.DTM_NgayMuonNhan,
                DTM_KyHan = src.DTM_KyHan,
                CHR_Gap = src.CHR_Gap,
                CHR_SectionCode = src.CHR_SectionCode,
                CHR_SectionName = src.CHR_SectionName,
                CHR_CreateBy = src.CHR_CreateBy,
                DTM_CreateDate = src.DTM_CreateDate,
                ID_Status = src.ID_Status,
                ID_StepBaoGia = src.ID_StepBaoGia,
                INT_SoLanUpdate = src.INT_SoLanUpdate,
                DTM_UpdateLater = src.DTM_UpdateLater,
                DTM_Deadline = src.DTM_Deadline,
                BIT_IsTemplate = src.BIT_IsTemplate
            };
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
        // MARK : Màn hình Input Quote - Tìm kiếm báo giá theo các tiêu chí
        [HttpPost]
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
        // Tìm kiếm hiển thị danh sách nhập báo giá theo số đơn hàng
        [HttpPost]
        public async Task<IActionResult> SearchInputQuoteBySoDon([FromBody] ThongTinBaoGiaGomNhomModel mod)
        {
            var result = await _baoGiaService.SearchThongTinNhapBaoGiaAsync(mod.maDon, mod.section, mod.maHang, GetCurrentUserId(), mod.pageIndex, mod.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> InsertInputQuote([FromBody] InsertInputQuoteModel model)
        {
            try
            {
                // Convert List<dynamic> to List<BaoGia_Detail_of_QuotationDTO>
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dtoList = new List<BaoGia_Detail_of_QuotationDTO>();
                foreach (var item in model.baoGiaDetail)
                {
                    // Serialize dynamic to JSON then deserialize to DTO
                    var json = System.Text.Json.JsonSerializer.Serialize(item);
                    var dto = System.Text.Json.JsonSerializer.Deserialize<BaoGia_Detail_of_QuotationDTO>(json, options);
                    if (dto != null)
                        dtoList.Add(dto);
                }

                var result = await _baoGiaDetailService.InsertListBaoGiaDetailAsync(dtoList);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                // gửi mail thông báo có báo giá mới cho người yêu cầu báo giá
                var sendMail = await _sendMailService.SendMailToSupplierByRequestCodeAsync(model.MaDon);
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi chuyển đổi dữ liệu: {ex.Message}");
            }
        }
        // MARK: lấy thông tin lựa chọn báo giá 
        [HttpPost]
        public async Task<IActionResult> GetThongTinBaoGiaGomNhom([FromBody] ThongTinBaoGiaGomNhomModel model)
        {
            var result = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync(model.maDon, model.section, model.maHang, model.status, GetCurrentUserId(), model.pageIndex, model.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        // MARK: Phe duyet
        [HttpPost]
        public async Task<IActionResult> ConfirmApprover([FromBody] ConfirmApproverModel model)
        {
            if (model == null || !model.listCofirm.Any())
            {
                return BadRequest("Not data approver");
            }
            return await PheDuyetBaoGia(model.listCofirm, model.UserApproverNext);
        }
        // Xử lý aproval
        public async Task<IActionResult> PheDuyetBaoGia(List<ApproverDTO> listCofirm, string UserApproverNext)
        {
            if (listCofirm == null || !listCofirm.Any())
            {
                return BadRequest("Not data approver");
            }
            try
            {
                var baoGia = await _baoGiaService.UpdateApprover(listCofirm, UserApproverNext, GetCurrentUserId());
                if (!baoGia.Success)
                {
                    return BadRequest("Error Approval :" + baoGia.Message);
                }
                var req = baoGia.Data;
                var userSend = UserApproverNext;
                var currentUserId = GetCurrentUserId();
                var listOk = req.Where(c => c.ID_Status != null && !c.ID_Status.Contains("RETURN")).ToList();
                var listNG = req.Where(c => c.ID_Status != null && c.ID_Status.Contains("RETURN")).ToList();
                // Send mail Approval ok
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
                                        // dữ liệu xác nhận tên
                                        var cf = new BaoGia_Confirm_Name_QuotationDTO();
                                        cf.ID_RequestQuote = material.ID;
                                        cf.DTM_CreateDate = DateTime.Now;
                                        cf.VCHR_CreateBy = currentUserId;
                                        cf.VCHR_TenRecomment = material.NVCHR_NameVN;
                                        cf.CHR_Status = "";
                                        cf.CHR_StatusACC = "Confirmed";
                                        cf.CHR_StatusShip = "Confirming";
                                        cf.NVCHR_Note = material.CHR_MaHangNCC;
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
                                await sendMailService.SendMailAsync(userSend, "", 14, "Quote/Quotation_Results",
                                    listOk.FirstOrDefault()?.CHR_Gap == "false" ? false : true, listOk.FirstOrDefault()?.CHR_SectionCode ?? "",
                                    listOk.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                                // Send mail confirm name
                                if (listConfirm.Any())
                                {
                                    // bỏ gửi mail và lưu thông tin xác nhận tên
                                    await baoGiaConfirmNameService.AddListAsync(listConfirm);
                                    //gửi mail thông báo có yêu cầu xác nhận tên mới
                                    var emailResult = await sendMailService.SendMailToConfirmItemAsync(13, 17, "Material/ConfirmName", true, "", "", currentUserId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                            }
                        }
                    });
                }
                // send mail Approval return
                // danh sach PIC
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

                                await sendMailService.SendMailAsync("khanhmf@brothergroup.net;" + emailList, "", 15, "Quote/Quotation_Results",
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
        [HttpPost]
        public async Task<IActionResult> PheDuyetOKBaoGia([FromBody] ApprovalSelectModel model)
        {
            if (model == null)
            {
                return BadRequest("Not connect model approval");
            }
            return await BaoGiaApOK(model.maDon, model.UserApproverNext);
        }
        // funtion bao gia OK
        private async Task<IActionResult> BaoGiaApOK(string maDon, string UserApproverNext)
        {
            try
            {

                var baoGia = await _baoGiaService.UpdateApprovarOK(maDon, UserApproverNext, GetCurrentUserId());
                if (!baoGia.Success)
                {
                    return BadRequest("Error Approval :" + baoGia.Message);
                }
                var req = baoGia.Data;
                var userSend = UserApproverNext;
                var currentUserId = GetCurrentUserId();
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                            await sendMailService.SendMailAsync(userSend + "@brothergroup.net", "", 14, "Quote/Quotation_Results", req.FirstOrDefault()?.CHR_Gap == "false" ? false : true, req.FirstOrDefault()?.CHR_SectionCode ?? "", req.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                        }
                    }

                });
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi phê duyệt báo giá: {ex.Message}");
            }
        }
        // Từ chối lựa chọn
        [HttpPost]
        public async Task<IActionResult> PheDuyetNGBaoGia([FromBody] ApprovalSelectModel model)
        {
            if (model == null)
            {
                return BadRequest("Not connect model approval");
            }
            return await BaoGiaApNG(model.maDon, model.Reason);
        }
        // funtion bao gia ng
        private async Task<IActionResult> BaoGiaApNG(string maDon, string Reason)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                // danh sach PIC
                var result = await _approverService.GetApproverByStepAndSectionAsync(4, "3110");
                if (!result.Success)
                {
                    return BadRequest("Không lấy được thông tin PIC phụ trách: " + result.Message);
                }
                var baoGia = await _baoGiaService.UpdateApprovarNG(maDon, Reason, currentUserId);
                if (!baoGia.Success)
                {
                    return BadRequest("Error Approval :" + baoGia.Message);
                }
                var req = baoGia.Data;
                var dataPic = result.Data;
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                            string emailList = string.Join("; ", dataPic.Select(x => x.CHR_UserAdid + "@brothergroup.net"));

                            await sendMailService.SendMailAsync("khanhmf@brothergroup.net;" + emailList, "", 15, "Quote/Quotation_Results", req.FirstOrDefault()?.CHR_Gap == "false" ? false : true, req.FirstOrDefault()?.CHR_SectionCode ?? "", req.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                        }
                    }

                });
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi phê duyệt báo giá: {ex.Message}");
            }
        }
        // chon nha cung cap
        [HttpPost]
        public async Task<IActionResult> ChonNhaCungCapBaoGia([FromBody] List<dynamic> listUpdate)
        {
            var result = await _baoGiaDetailService.UpdateLuaChonNCCBaoGiaDetailAsync(listUpdate, GetCurrentUserId(), GetCurrentUserFullName());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        // Xuất danh sách lựa chọn theo template FileSelectionQuote.xlsx (bắt đầu từ dòng 4)
        [HttpPost]
        public async Task<IActionResult> ExportSelection([FromBody] List<SelectionExportItem> selections)
        {
            try
            {
                if (selections == null || !selections.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }
                var Status = await _baoGiaStatusService.GetListStatusAsync();
                if (Status == null || !Status.Success)
                {
                    return BadRequest("Lỗi lấy danh sách trạng thái");
                }
                List<BaoGia_StatusDTO> listStatus = Status.Data ?? new List<BaoGia_StatusDTO>();
                List<int> listIdExport = new List<int>();
                foreach (var item in selections)
                {
                    if (item.ID != null && item.ID != "")
                    {
                        if (int.TryParse(item.ID, out int id))
                        {
                            if (!listIdExport.Contains(id))
                            {
                                listIdExport.Add(id);
                            }
                        }
                    }
                    if (item.MaDon != null && item.MaDon != "")
                    {
                        var reqs = await _baoGiaService.ExportBaoGiaAsync(item.MaDon);
                        if (reqs.Success && reqs.Data != null && reqs.Data.Count > 0)
                        {
                            foreach (var r in reqs.Data)
                            {
                                if (!listIdExport.Contains(r))
                                {
                                    listIdExport.Add(r);
                                }
                            }
                        }
                    }
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "FileSelectionQuote.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: FileSelectionQuote.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 4;
                foreach (var item in listIdExport)
                {
                    // Lấy thông tin yêu cầu báo giá (SearchID)
                    var rqResp = await _baoGiaService.GetByIdAsync(item);
                    if (!rqResp.Success || rqResp.Data == null)
                    {
                        continue;
                    }
                    var rq = rqResp.Data;
                    var detailResp = await _baoGiaDetailService.GetByIdRequestQuoteAsync(rq.ID);
                    if (!detailResp.Success || detailResp.Data == null)
                    {
                        continue;
                    }
                    var d = detailResp.Data;

                    int col = 1;
                    // Các cột thông tin từ SearchID (thứ tự tùy theo template yêu cầu)
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

                    // Các cột thông tin tiếp theo từ chi tiết NCC (SearchInputQuote)
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

        // Xuất danh sách thông tin tự render theo mã mặt hàng đã chọn (Hàng đã có mã hàng NB)
        [HttpPost]
        public async Task<IActionResult> ExportAutoRender([FromBody] AutoRenderFile autoRenders)
        {
            try
            {
                if (autoRenders == null)
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemPlateQuote.xlsx");
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
                int row = 10;
                foreach (var item in autoRenders.selectedItemIds)
                {
                    var materialAsync = await _materialService.GetByMaHangAsync(item);
                    if (!materialAsync.Success || materialAsync.Data == null)
                    {
                        continue;
                    }
                    var m = materialAsync.Data;

                    //var supplierAs = await _baoGiaNCCService.GetBaoGiaNCCByMaHang(item);
                    // đổi sang dùng bảng BaoGiaNCCCategory
                    var supplierAs = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(m.Category_VN ?? ""); ;
                    if (!supplierAs.Success || supplierAs.Data == null)
                    {
                        continue;
                    }
                    var sp = supplierAs.Data;
                    foreach (var a in sp)
                    {
                        int col = 2;
                        ws.Cell(row, col++).SetValue(autoRenders.sectionCode);
                        ws.Cell(row, col++).SetValue(autoRenders.sectionName);
                        ws.Cell(row, col++).SetValue(m.LoaiHang);
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue(m.Material_Code);
                        ws.Cell(row, col++).SetValue("");// ma hang cua NCC a.NVCHR_CodeByNCC
                        ws.Cell(row, col++).SetValue(m.TenMoThuTuc);
                        ws.Cell(row, col++).SetValue(m.Material_Name_EN);
                        ws.Cell(row, col++).SetValue(0);
                        ws.Cell(row, col++).SetValue(m.Unit);
                        ws.Cell(row, col++).SetValue(m.Category_VN);
                        ws.Cell(row, col++).SetValue(m.Shape);
                        ws.Cell(row, col++).SetValue(m.Material);
                        ws.Cell(row, col++).SetValue(m.Composition);
                        ws.Cell(row, col++).SetValue(m.Dimension);
                        ws.Cell(row, col++).SetValue(m.UsedFor);
                        ws.Cell(row, col++).SetValue(m.Purpose);
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue(a.NVCHR_SanXuat);
                        ws.Cell(row, col++).SetValue(a.CHR_MaNCC);
                        ws.Cell(row, col++).SetValue(a.NVCHR_TenNCC);

                        ws.Cell(row, col + 5).SetValue(GetCurrentUserId());
                        row++;
                    }
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"AutoRenderQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
        }
        // Xuất file auto render cho hàng mới
        [HttpPost]
        public async Task<IActionResult> ExportRenderOutSide([FromBody] AutoRenderFile autoRenders)
        {
            try
            {
                if (autoRenders == null)
                {
                    return BadRequest("Không có dữ liệu để xuất");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemPlateQuote.xlsx");
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
                int row = 10;
                foreach (var item in autoRenders.selectedItemIds)
                {
                    var supplierAs = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(item); ;
                    if (!supplierAs.Success || supplierAs.Data == null)
                    {
                        continue;
                    }
                    var sp = supplierAs.Data;
                    foreach (var a in sp)
                    {
                        int col = 2;
                        ws.Cell(row, col++).SetValue(autoRenders.sectionCode);
                        ws.Cell(row, col++).SetValue(autoRenders.sectionName);
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue(0);
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue("");
                        ws.Cell(row, col++).SetValue(a.NVCHR_SanXuat);
                        ws.Cell(row, col++).SetValue(a.CHR_MaNCC);
                        ws.Cell(row, col++).SetValue(a.NVCHR_TenNCC);

                        ws.Cell(row, col + 5).SetValue(GetCurrentUserId());
                        row++;
                    }
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"AutoRenderQuote_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xuất file: {ex.Message}");
            }
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
        // Nhập file excel
        [HttpPost]
        public async Task<IActionResult> ImportExcelInputQuote([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file được tải lên");
                }

                // lấy tỷ giá tiền tệ
                var exchangRateAsyenc = await _exchangeRateService.GetExchangeRate();
                if (exchangRateAsyenc == null || !exchangRateAsyenc.Success)
                {
                    return BadRequest("Không thể lấy tỷ giá tiền tệ");
                }
                var exchangeRate = exchangRateAsyenc.Data;

                var items = new List<BaoGia_Detail_of_QuotationDTO>();
                var hasErrors = false;
                ClosedXML.Excel.XLWorkbook workbook = null;
                var fileBytes = new byte[file.Length];
                using (var stream = file.OpenReadStream())
                {
                    await stream.ReadAsync(fileBytes, 0, (int)file.Length);
                }
                using (var memoryStream = new MemoryStream(fileBytes))
                {
                    workbook = new ClosedXML.Excel.XLWorkbook(memoryStream);
                    var ws = workbook.Worksheets.FirstOrDefault();
                    if (ws == null)
                    {
                        return BadRequest("Không tìm thấy worksheet trong file");
                    }
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 13;
                    // Bắt đầu đọc từ dòng 13
                    for (int r = 13; r <= lastRow; r++)
                    {
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 1).GetString()))
                        {
                            break;
                        }
                        var errors = new List<string>();
                        if (ws.Cell(r, 16).GetString().Contains("Refuse"))
                        {
                            // lấy Id của đơn lưu trong csdl
                            var idRequestQuote1 = 0;
                            if (string.IsNullOrEmpty(ws.Cell(r, 32).GetString()))
                            {
                                var checkRQ1 = await _baoGiaDetailService.GetIdOfQuotationAsync(ws.Cell(r, 1).GetString(), ws.Cell(r, 4).GetString(),
                                ws.Cell(r, 3).GetString(), ws.Cell(r, 10).GetString(), ws.Cell(r, 2).GetString());
                                if (!checkRQ1.Success || checkRQ1.Data == null || checkRQ1.Data == 0)
                                {
                                    ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                    hasErrors = true;
                                    continue;
                                }
                                idRequestQuote1 = checkRQ1.Data.Value;
                            }
                            else
                            {
                                var checkRQ1 = await _baoGiaDetailService.GetIdDetailAsync(ParseInt(ws.Cell(r, 32).GetString()));
                                if (!checkRQ1.Success || checkRQ1.Data == 0)
                                {
                                    ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                    hasErrors = true;
                                    continue;
                                }
                                idRequestQuote1 = checkRQ1.Data;
                            }

                            if (idRequestQuote1 == 0)
                            {
                                ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                hasErrors = true;
                                continue;
                            }
                            var dto1 = new BaoGia_Detail_of_QuotationDTO
                            {
                                ID = idRequestQuote1,
                                CHR_MaHangNCC = ws.Cell(r, 10).GetString(),
                                NVCHR_TenHangHQ = ws.Cell(r, 12).GetString(),
                                CHR_NameEN = ws.Cell(r, 13).GetString(),
                                INT_SoLuong = (int?)ParseDouble(ws.Cell(r, 14).GetString()),
                                NVCHR_DonVi = ws.Cell(r, 15).GetString(),
                                FL_USD = null,
                                FL_VND = null,
                                NVCHR_MOQ = null,
                                NVCHR_Packing = null,
                                DTM_LeadTime = null,
                                DTM_ShipTime = null,
                                VCHR_Rohs = null,
                                VCHR_COCQ = null,
                                VCHR_MSDS = null,
                                VCHR_AnToan = null,
                                VCHR_CamKet = null,
                                NVCHR_DeliveryTerm = null,
                                NVCHR_PaymentTerm = null,
                                DTM_EffectiveDate = null,
                                DTM_ExpiryDate = null,
                                CHR_UpdateBy = null,
                                NVCHR_File = ws.Cell(r, 30).GetString(),
                                BIT_Select = false,
                                CHR_Status = "Refuse"
                            };

                            items.Add(dto1);
                        }
                        else
                        {
                            // Kiểm tra các cột bắt buộc (22,23,24)
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 22).GetString())) errors.Add("Cột 22 (VCHR_CamKet) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 23).GetString())) errors.Add("Cột 23 (Delivery Term) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 24).GetString())) errors.Add("Cột 24 (Payment Term) bắt buộc");

                            // So sánh tên mở thủ tục hải quan (cột 3 vs 27) và tên tiếng Anh (5 vs 28)
                            //if (!string.Equals(ws.Cell(r, 3).GetString(), ws.Cell(r, 27).GetString(), StringComparison.Ordinal)) errors.Add("Tên mở thủ tục hải quan khác nhau (cột 3 vs 27)");
                            //if (!string.Equals(ws.Cell(r, 5).GetString(), ws.Cell(r, 28).GetString(), StringComparison.Ordinal)) errors.Add("Tên tiếng Anh khác nhau (cột 5 vs 28)");

                            // So sánh số lượng (6 vs 14)
                            var qty1 = ParseInt(ws.Cell(r, 6).GetString());
                            var qty2 = ParseInt(ws.Cell(r, 14).GetString());
                            if (qty1 == null || qty2 == null)
                            {
                                if (qty1 == null) errors.Add("Cột 6 không phải số hợp lệ");
                                if (qty2 == null) errors.Add("Cột 14 không phải số hợp lệ");
                            }
                            //else if (qty1 != qty2) errors.Add("Số lượng khác nhau giữa cột 6 và 14");

                            // So sánh đơn vị (7 vs 15)
                            //if (!string.Equals(ws.Cell(r, 7).GetString(), ws.Cell(r, 15).GetString(), StringComparison.Ordinal)) errors.Add("Đơn vị khác nhau (cột 7 vs 15)");

                            // Ngày giao hàng (cột 21) so sánh với yêu cầu (cột 28)
                            var ship = ParseDate(ws.Cell(r, 21).GetString());
                            var reqDate = ParseDate(ws.Cell(r, 28).GetString());
                            if (ship == null) errors.Add("Cột 21 (DTM_ShipTime) không phải ngày hợp lệ");
                            if (reqDate == null) errors.Add("Cột 28 (DTM_NgayMuonNhan yêu cầu) không phải ngày hợp lệ");
                            //if (ship != null && reqDate != null && ship > reqDate) errors.Add("Thời gian giao hàng muộn hơn yêu cầu (cột 13 > cột 39)");

                            // MOQ (cột 18) <= Số lượng (cột 14)
                            var moq = ParseInt(ws.Cell(r, 18).GetString());
                            if (moq != null && qty1 != null && moq > qty1) errors.Add("MOQ (cột 18) lớn hơn Số lượng (cột 14)");

                            // Kiểm tra các điều kiện Rohs/COCQ/MSDS/AnToan: nếu expected yêu cầu nhưng value thiếu -> lỗi
                            //if (CheckNotRequired(ws.Cell(r, 13).GetString())) errors.Add("Rohs không thỏa mãn (cột 13)");
                            //if (CheckNotRequired(ws.Cell(r, 14).GetString())) errors.Add("CO/CQ không thỏa mãn (cột 14)");
                            //if (CheckNotRequired(ws.Cell(r, 15).GetString())) errors.Add("MSDS không thỏa mãn (cột 15)");
                            //if (CheckNotRequired(ws.Cell(r, 16).GetString())) errors.Add("An toàn không thỏa mãn (cột 16)");

                            if (errors.Any())
                            {
                                ws.Cell(r, 33).SetValue(string.Join("; ", errors));
                                ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                hasErrors = true;
                                continue;
                            }
                            var idRequestQuote = 0;
                            // lấy Id của đơn lưu trong csdl
                            if (string.IsNullOrEmpty(ws.Cell(r, 32).GetString()))
                            {
                                var checkRQ = await _baoGiaDetailService.GetIdOfQuotationAsync(ws.Cell(r, 1).GetString(), ws.Cell(r, 4).GetString(),
                                                               ws.Cell(r, 3).GetString(), ws.Cell(r, 10).GetString(), ws.Cell(r, 2).GetString());
                                if (!checkRQ.Success || checkRQ.Data == null)
                                {
                                    ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                    hasErrors = true;
                                    continue;
                                }
                                idRequestQuote = checkRQ.Data.Value;
                            }
                            else
                            {
                                var checkRQ = await _baoGiaDetailService.GetIdDetailAsync(ParseInt(ws.Cell(r, 32).GetString()));
                                if (!checkRQ.Success || checkRQ.Data == 0)
                                {
                                    ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                    ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                    hasErrors = true;
                                    continue;
                                }
                                idRequestQuote = checkRQ.Data;
                            }
                            if (idRequestQuote == 0)
                            {
                                ws.Cell(r, 33).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                                ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                hasErrors = true;
                                continue;
                            }
                            var agree = ws.Cell(r, 22).GetString();
                            //Tiền
                            double costUSD = ParseDouble(ws.Cell(r, 16).GetString()) ?? 0;
                            double costVND = ParseDouble(ws.Cell(r, 17).GetString()) ?? 0;

                            // Nếu không có lỗi, tạo DTO và thêm vào danh sách
                            var dto = new BaoGia_Detail_of_QuotationDTO
                            {
                                ID = idRequestQuote,
                                CHR_MaHangNCC = ws.Cell(r, 11).GetString(),
                                NVCHR_TenHangHQ = ws.Cell(r, 12).GetString(),
                                CHR_NameEN = ws.Cell(r, 13).GetString(),
                                INT_SoLuong = qty2,
                                NVCHR_DonVi = ws.Cell(r, 15).GetString(),
                                FL_USD = costUSD != 0 ? costUSD : ParseVNDtoUSD(costVND, true, exchangeRate),
                                FL_VND = costVND != 0 ? costVND : ParseVNDtoUSD(costUSD, false, exchangeRate),
                                NVCHR_MOQ = moq.ToString(),
                                NVCHR_Packing = ws.Cell(r, 19).GetString(),
                                DTM_LeadTime = ws.Cell(r, 20).GetString(),
                                DTM_ShipTime = ParseDate(ws.Cell(r, 21).GetString()),

                                VCHR_Rohs = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_COCQ = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_MSDS = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_AnToan = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_CamKet = agree,
                                NVCHR_DeliveryTerm = ws.Cell(r, 23).GetString(),
                                NVCHR_PaymentTerm = ws.Cell(r, 24).GetString(),
                                DTM_EffectiveDate = ParseDate(ws.Cell(r, 25).GetString()),
                                DTM_ExpiryDate = ParseDate(ws.Cell(r, 26).GetString()),
                                CHR_UpdateBy = GetCurrentUserId(),
                                NVCHR_File = ws.Cell(r, 30).GetString(),//fileUrl,
                            };

                            items.Add(dto);
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
                }

                // Cập nhật dữ liệu nếu không có lỗi
                var result = await _baoGiaDetailService.UpdateListThongTinNhapBaoGiaAsync(items);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                // Cập nhật trạng thái cho đơn hàng
                var listUpdateStatus = items.Select(c => c.ID).ToList();
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        try
                        {
                            var baoGiaDetailService = scope.ServiceProvider.GetRequiredService<IBaoGiaDetailService>();
                            var resultUpdateStatus = await baoGiaDetailService.UpdateStatusAsync(listUpdateStatus);
                            if (!resultUpdateStatus.Success)
                            {

                                _logger.LogError(resultUpdateStatus.Message, "Lỗi khi cập nhật trạng thái");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái");
                        }
                    }

                });


                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // Đổi tiền VND sang USD or USD sang VND
        private double? ParseVNDtoUSD(double input, bool isVNDToUSD, float exchangeRate)
        {
            double result = 0;
            if (input > 0)
            {
                if (isVNDToUSD)
                {
                    result = input / exchangeRate;
                }
                else
                {
                    result = input * exchangeRate;
                }
                return Math.Round(result, 4);
            }
            return null;
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

                // Lấy thông tin status
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

                // Lấy các đơn trả về có trạng thái RETURN để lấy lý do trả
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
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
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

                    // Lấy lý do trả (chỉ khi có RETURN)
                    var reason = (rq.ID_Status != null && rq.ID_Status.Contains("RETURN"))
                        ? (listReason.FirstOrDefault(c => c.Id == rq.ID)?.Reason ?? "")
                        : "";

                    // Lấy tên trạng thái
                    var statusName = Status.Data?
                        .FirstOrDefault(s => s.VCHR_CodeStatus == rq.ID_Status)?
                        .NVCHR_TenStatus ?? string.Empty;

                    // Lấy tên step
                    var stepName = Steps.Data?
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

                    // Lý do pick & file (chỉ khi có chọn)
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
                    dto.NVCHR_UserRequest = ws.Cell(i, 34).GetString() ?? GetCurrentUserId() ?? string.Empty;
                    dto.CHR_CreateBy = GetCurrentUserId() ?? string.Empty;
                    dto.DTM_UpdateLater = DateTime.Now;

                    listRequest.Add(dto);
                }

                if (!listRequest.Any()) return BadRequest("Không có dữ liệu hợp lệ để cập nhật");

                // check điều kiện update đơn
                var listMa = listRequest.Select(r => r.CHR_MaDon).Distinct().ToList();
                var checkUpdate = await _baoGiaService.CheckDonReturnAsync(listMa);
                if (!checkUpdate.Success || !checkUpdate.Data)
                {
                    return BadRequest("Không thể cập nhật đơn vì có đơn đang ở trạng thái RETURN");
                }

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
        // lấy thông tin theo chi tiêt mã đơn phe duyet
        [HttpPost]
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
        // check điều kiện k nhap
        private static bool CheckNotRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            return false;
        }
        // check điều kiện của Rohs, MSDS, an toàn, cam kết
        private static bool CheckCondition(string value, string expected)
        {
            if (string.IsNullOrWhiteSpace(expected))
            {
                return true;
            }

            var v = (value ?? string.Empty).Trim().ToLowerInvariant();
            var e = expected.Trim().ToLowerInvariant();
            if (e.Contains("need") && !e.Contains("no need") && !e.Contains("non-need"))
            {
                if (string.IsNullOrWhiteSpace(v)) return false;
                if (v.Contains("ok") || v == "ok") return true;
                if (v.Contains("ng") || v.Contains("no") || v.Contains("not")) return false;
                return false;
            }
            if (e.Contains("no need") || e.Contains("not need") || e.Contains("not required") || e.Contains("none"))
            {
                return true;
            }
            // Fallback: nếu expected chứa OK thì require OK
            if (e.Contains("ok"))
            {
                return v.Contains("ok");
            }
            return !string.IsNullOrWhiteSpace(v);
        }

        /// <summary>
        /// Màn hình kết quả phòng ban
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        public async Task<IActionResult> SearchQuoteSection([FromBody] SearchQuotationResultsModel mod)
        {
            if (mod == null) return BadRequest("No data view model search");
            try
            {
                var result = await _baoGiaService.SearchRequestDone(mod.MaDon, mod.Section, mod.MaVatTu, mod.MaNcc, GetCurrentUserId(), mod.PageIndex ?? 0, mod.PageSize ?? 20);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest("Error search: " + ex.Message);
            }
        }
        // Xuất file cho các nhóm đã chọn
        [HttpPost]
        public async Task<IActionResult> ExportSelectedGroups([FromBody] List<string> selectedMaDon)
        {
            try
            {
                var role = GetRolesUser();
                if (role == "UserPUR" && selectedMaDon.Count > 5)
                {
                    var maDonList = await _baoGiaService.GetMaDonYeuCauHangHoaAsync();
                    selectedMaDon = maDonList.Data.ToList();
                }
                if (selectedMaDon == null || !selectedMaDon.Any())
                {
                    return BadRequest("Không có nhóm nào được chọn");
                }
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateResults.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                var enUs = new CultureInfo("en-US");
                int rowStart = 4;
                foreach (var maDon in selectedMaDon)
                {
                    var result = await _baoGiaService.SearchRequestDone(maDon, "", "", "", GetCurrentUserId(), 0, 0);
                    if (!result.Success || result.Data == null) continue;
                    var dataList = result.Data.Data;
                    var totals = new Dictionary<string, (double vnd, double usd)>();
                    foreach (var item in dataList)
                    {
                        string key = $"{item.CHR_MaDon ?? ""}|{item.CHR_MaThietBi ?? ""}|{item.CHR_MaNCC ?? ""}";
                        double vnd = item.FL_VND ?? 0.0;
                        double usd = item.FL_USD ?? 0.0;
                        if (!totals.ContainsKey(key))
                        {
                            totals[key] = (0.0, 0.0);
                        }
                        var current = totals[key];
                        totals[key] = (current.Item1 + vnd, current.Item2 + usd);
                    }
                    foreach (var item in dataList)
                    {
                        int col = 1;
                        ws.Cell(rowStart, col++).SetValue(item.CHR_MaDon ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.ID_Status ?? string.Empty);
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
                        // Vendor input
                        ws.Cell(rowStart, col++).SetValue(item.CodeEquipmentNCC ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NameENByNCC ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.soluong ?? 0); // Vendor quantity
                        ws.Cell(rowStart, col++).SetValue(item.donvi ?? string.Empty); // Vendor unit
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty); // Vendor maker
                        ws.Cell(rowStart, col++).SetValue(item.FL_USD ?? 0.0);
                        ws.Cell(rowStart, col++).SetValue((item.FL_VND ?? 0).ToString("N0", enUs));
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_MOQ ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_Packing ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.DTM_LeadTime ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.DTM_ShipTime ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.VCHR_Rohs ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.VCHR_COCQ ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.VCHR_MSDS ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.VCHR_AnToan ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.VCHR_CamKet ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_DeliveryTerm ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_PaymentTerm ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_File ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                        ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty);
                        // System total
                        // System count
                        string key = $"{item.CHR_MaDon ?? ""}|{item.CHR_MaThietBi ?? ""}|{item.CHR_MaNCC ?? ""}";
                        var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                        string totalCell = "";
                        if (tot.Item1 != 0)
                        {
                            totalCell = tot.Item1.ToString("N0", enUs) + " VND";
                        }
                        else if (tot.Item2 != 0)
                        {
                            totalCell = Math.Round(tot.Item2, 4).ToString("0.0000", enUs) + " USD";
                        }
                        ws.Cell(rowStart, col++).SetValue(totalCell); // placeholder
                        ws.Cell(rowStart, col++).SetValue(item.BIT_Select == true ? "O" : "X"); // BIT_Select
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_ReasonPick); // Reason
                        col++;
                        // Approval
                        ws.Cell(rowStart, col++).SetValue(item.UserQlsc ?? "");
                        ws.Cell(rowStart, col++).SetValue((item.LyDoQlsc == null || item.LyDoQlsc == "") ? "OK" : "NG");
                        ws.Cell(rowStart, col++).SetValue(item.LyDoQlsc ?? "");
                        ws.Cell(rowStart, col++).SetValue(item.UserQltc ?? "");
                        ws.Cell(rowStart, col++).SetValue((item.LyDoQltc == null || item.LyDoQltc == "") ? "OK" : "NG");
                        ws.Cell(rowStart, col++).SetValue(item.LyDoQltc ?? "");
                        ws.Cell(rowStart, col++).SetValue(item.UserDeft ?? "");
                        ws.Cell(rowStart, col++).SetValue((item.LyDoDeft == null || item.LyDoDeft == "") ? "OK" : "NG");
                        ws.Cell(rowStart, col++).SetValue(item.LyDoDeft ?? "");
                        // user request
                        ws.Cell(rowStart, col++).SetValue(item.NVCHR_UserRequest ?? "");
                        rowStart++;
                    }
                }

                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"SelectedGroups_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadQuoteExcelBackup(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "File không hợp lệ" });

            var items = new List<BaoGia_Request_of_Quotation>();
            var errors = new List<string>();

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy worksheet" });

                // Dữ liệu bắt đầu từ dòng 10
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                int successCount = 0;

                for (int r = startRow; r <= lastRow; r++)
                {
                    try
                    {
                        // Kiểm tra dòng trống (cột Mã phòng ban hoặc Mã đơn hàng)
                        var maDon = ws.Cell(r, 2).GetString();
                        if (string.IsNullOrWhiteSpace(maDon))
                        {
                            break; // kết thúc nếu gặp dòng trống
                        }

                        var item = new BaoGia_Request_of_Quotation
                        {
                            // Cột 1: ID (bỏ qua vì tự động sinh)
                            CHR_MaDon = maDon,
                            CHR_MaThietBi = ws.Cell(r, 3).GetString(),
                            CHR_Phanloai = ws.Cell(r, 4).GetString(),
                            CHR_MaHangNoiBo = ws.Cell(r, 5).GetString(),
                            CHR_MaHangNCC = ws.Cell(r, 6).GetString(),
                            NVCHR_NameVN = ws.Cell(r, 7).GetString(),
                            CHR_NameEN = ws.Cell(r, 8).GetString(),

                            // Số lượng
                            INT_SoLuong = ws.Cell(r, 9).TryGetValue<int>(out var soLuong) ? soLuong : 0,

                            NVCHR_DonVi = ws.Cell(r, 10).GetString(),
                            NVCHR_ChungLoai = ws.Cell(r, 11).GetString(),
                            NVCHR_HinhDang = ws.Cell(r, 12).GetString(),
                            NVCHR_ChatLieu = ws.Cell(r, 13).GetString(),
                            NVCHR_ThanhPhan = ws.Cell(r, 14).GetString(),
                            NVCHR_KichThuoc = ws.Cell(r, 15).GetString(),
                            NVCHR_DongMay = ws.Cell(r, 16).GetString(),
                            NVCHR_TinhNang = ws.Cell(r, 17).GetString(),
                            NVCHR_Rohs = ws.Cell(r, 18).GetString(),
                            NVCHR_COCQ = ws.Cell(r, 19).GetString(),
                            NVCHR_MSDS = ws.Cell(r, 20).GetString(),
                            NVCHR_AnToan = ws.Cell(r, 21).GetString(),
                            NVCHR_FileThietKe = ws.Cell(r, 22).GetString(),
                            NVCHR_NhaSanXuat = ws.Cell(r, 23).GetString(),
                            CHR_MaNCC = ws.Cell(r, 24).GetString(),
                            NVCHR_TenNCC = ws.Cell(r, 25).GetString(),

                            // BIT_LayBaoGia (cột 26)
                            BIT_LayBaoGia = ws.Cell(r, 26).TryGetValue<bool>(out var layBaoGia) ? layBaoGia : true,

                            NVCHR_LyDo = ws.Cell(r, 27).GetString(),

                            // Ngày tháng
                            DTM_NgayMuonNhan = ws.Cell(r, 28).TryGetValue<DateTime>(out var ngayMuonNhan) ? ngayMuonNhan : (DateTime?)null,
                            DTM_KyHan = ws.Cell(r, 29).TryGetValue<DateTime>(out var kyHan) ? kyHan : (DateTime?)null,

                            CHR_Gap = ws.Cell(r, 30).GetString(),
                            CHR_SectionCode = ws.Cell(r, 31).GetString(),
                            CHR_SectionName = ws.Cell(r, 32).GetString(),
                            CHR_CreateBy = ws.Cell(r, 33).GetString(),

                            // DTM_CreateDate (cột 34) - nếu không có thì lấy ngày hiện tại
                            DTM_CreateDate = ws.Cell(r, 34).TryGetValue<DateTime>(out var createDate) ? createDate : DateTime.Now,

                            // Cột 35: ID_StepBaoGia (bỏ qua hoặc set mặc định)
                            ID_StepBaoGia = ParseInt(ws.Cell(r, 35).GetString()),
                            // Cột 36: ID_Status
                            ID_Status = ws.Cell(r, 36).GetString(),

                            // INT_SoLanUpdate (cột 37)
                            INT_SoLanUpdate = ws.Cell(r, 37).TryGetValue<int>(out var soLanUpdate) ? soLanUpdate : 0,

                            // DTM_UpdateLater (cột 38)
                            DTM_UpdateLater = ws.Cell(r, 38).TryGetValue<DateTime>(out var updateLater) ? updateLater : (DateTime?)null,

                            // DTM_Deadline (cột 39)
                            DTM_Deadline = ws.Cell(r, 39).TryGetValue<DateTime>(out var deadline) ? deadline : (DateTime?)null,

                            // BIT_IsTemplate (cột 40)
                            BIT_IsTemplate = ws.Cell(r, 40).TryGetValue<bool>(out var isTemplate) ? isTemplate : false,

                            CHR_UserApproval = ws.Cell(r, 41).GetString(),
                            NVCHR_UserRequest = ws.Cell(r, 42).GetString()
                        };

                        // Validate dữ liệu cơ bản
                        if (string.IsNullOrEmpty(item.CHR_MaDon))
                        {
                            errors.Add($"Dòng {r}: Mã đơn hàng không được để trống");
                            continue;
                        }

                        items.Add(item);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Dòng {r}: Lỗi xử lý - {ex.Message}");
                    }
                }

                if (items.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Không có dữ liệu hợp lệ để nhập", errors });
                }

                // Lưu vào database
                var a = await _baoGiaService.AddMultiAsync(items);
                if (!a.Success)
                {
                    return BadRequest(a.Message);
                }
                return Ok(new
                {
                    success = true,
                    message = $"Nhập thành công {successCount} dòng",
                    totalRows = items.Count,
                    errors = errors.Count > 0 ? errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi xử lý file: {ex.Message}" });
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
        // check NCC
        [HttpPost]
        public async Task<IActionResult> CheckNCC([FromBody] string maNcc, string catergory)
        {
            if (string.IsNullOrWhiteSpace(maNcc))
            {
                return BadRequest("Mã nhà cung cấp không được để trống");
            }
            var result = await _baoGiaNccCategoryService.CheckSupperlier(maNcc, catergory);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
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
                searchModel.Date,
                searchModel.ChungLoai
                );
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
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
    }
}
