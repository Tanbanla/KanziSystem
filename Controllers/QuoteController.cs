using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.Common;
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

        //private string GetApprovalStatus(int currentStep, int requiredStep, string? reason)
        //{
        //    if (currentStep <= requiredStep) return "";
        //    return string.IsNullOrEmpty(reason) ? "OK" : "NG";
        //}
 


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
                INT_SoLuong = ConvertHelper.ParseDouble(rowData.SoLuong),
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
                BIT_LayBaoGia = ConvertHelper.ParseBool(rowData.LayBaoGia),
                NVCHR_LyDo = rowData.LyDo,
                DTM_NgayMuonNhan = ConvertHelper.ParseDate(rowData.NgayMuonNhan),
                DTM_KyHan = ConvertHelper.ParseDate(rowData.KyHan),
                CHR_Gap = ConvertHelper.ParseBool(rowData.Gap) == false ? "false" : "true",
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
                CHR_Phanloai = ConvertHelper.ParsePhanloai(rowData.Phanloai),
                CHR_MaThietBi = rowData.MaThietBi,
                CHR_MaHangNoiBo = rowData.MaHangNoiBo,
                CHR_MaHangNCC = rowData.MaHangNCC,
                NVCHR_NameVN = rowData.NameVN,
                CHR_NameEN = rowData.NameEN,
                INT_SoLuong = ConvertHelper.ParseDouble(rowData.SoLuong),
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
                BIT_LayBaoGia = ConvertHelper.ParseBool(rowData.LayBaoGia),
                NVCHR_LyDo = rowData.LyDo,
                DTM_NgayMuonNhan = ConvertHelper.ParseDate(rowData.NgayMuonNhan),
                DTM_KyHan = ConvertHelper.ParseDate(rowData.KyHan),
                CHR_Gap = ConvertHelper.ParseBool(rowData.Gap) == false ? "false" : "true",
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

                dto.BIT_LayBaoGia = ConvertHelper.ParseBool(rowData.LayBaoGia);

                items.Add(dto);
                first = false;
            }

            return items;
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
        // Nhập file excel
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
                            ID_StepBaoGia = ConvertHelper.ParseInt(ws.Cell(r, 35).GetString()),
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
    }
}
