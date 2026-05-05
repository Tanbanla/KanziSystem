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
    public class SelectQuoteSectionController : BaseAuthController
    {
        private readonly ILogger<SelectQuoteSectionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IDepartmentService _deparmentService;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<SelectQuoteSectionController> _localizer;
        private readonly IMaterialService _materialService;
        private readonly ITmNccNewService _tmNccNewService;

        public SelectQuoteSectionController(ILogger<SelectQuoteSectionController> logger, IConfiguration configuration,
            IBaoGiaService baoGiaService, IDepartmentService deparmentService, IWebHostEnvironment env,
            IStringLocalizer<SelectQuoteSectionController> localizer, IMaterialService materialService, ITmNccNewService tmNccNewService)
        {
            _logger = logger;
            _configuration = configuration;
            _baoGiaService = baoGiaService;
            _deparmentService = deparmentService;
            _env = env;
            _localizer = localizer;
            _materialService = materialService;
            _tmNccNewService = tmNccNewService;
        }

        // MARK: - Select Quote Section
        public async Task<IActionResult> SelectQuoteSection()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await LoadMaterialsAsync();
            var nccNews = await LoadNhaCungCapDataAsync();
            var madons = await LoadMadonAsync(13);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials,
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            return View(vm);
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

        private async Task<List<string>> LoadMadonAsync(int step)
        {
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "", step);
            return madons.Data ?? new List<string>();
        }
    }
}
