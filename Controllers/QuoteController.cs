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
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IStringLocalizer<QuoteController> _localizer;

        public QuoteController(ILogger<QuoteController> logger, ITmNccNewService tmNccNewService, IConfiguration configuration,
            IBaoGiaService baoGiaService, IMaterialService materialService, ITmSectionService tmSectionService, IExchangeRateService exchangeRateService,
           IDepartmentService deparmentService, IBaoGiaNCCService baoGiaNCCService, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStepService baoGiaStepService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaDetailService baoGiaDetailService, IBaoGiaConfirmNameService baoGiaConfirmNameService,
            ITmCategoryService tmCategoryService, IBaoGiaNccCategoryService baoGiaNccCategoryService, ITmEmployeeAgentService tmEmployeeAgentService,
            IWebHostEnvironment env, ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IMasterApproverSendMailService approverService,
            IStringLocalizer<QuoteController> localizer)
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
        }
        // MARK: - Quote
        public async Task<IActionResult> Index()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";

            //var sendMail = await _sendMailService.SendMailToSupplierAsync();
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
                var enUs = new CultureInfo("en-US");
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
                    // System total
                    // System count
                    string key = $"{item.CHR_MaDon ?? ""}|{(string.IsNullOrEmpty(item.CHR_MaThietBi) ? item.ID.ToString() : item.CHR_MaThietBi)}|{item.CHR_MaNCC ?? ""}";
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
    }
}
