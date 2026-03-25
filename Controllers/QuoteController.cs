using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Collections.Immutable;
using System.Drawing.Printing;
using System.IO;
using System.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class QuoteController : BaseAuthController
    {
        private readonly ILogger<QuoteController> _logger;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IMaterialService _materialService;
        private readonly ITmSectionService _tmSectionService;
        private readonly INhomViTriService _nhomViTriService;
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

        public QuoteController(ILogger<QuoteController> logger, ITmNccNewService tmNccNewService,
            IBaoGiaService baoGiaService, IMaterialService materialService, ITmSectionService tmSectionService,
            INhomViTriService nhomViTriService, IBaoGiaNCCService baoGiaNCCService, IBaoGiaHistoryService baoGiaHistoryService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaDetailService baoGiaDetailService, IBaoGiaConfirmNameService baoGiaConfirmNameService,
            ITmCategoryService tmCategoryService, IBaoGiaNccCategoryService baoGiaNccCategoryService, ITmEmployeeAgentService tmEmployeeAgentService,
            IWebHostEnvironment env, ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IMasterApproverSendMailService approverService)
        {
            _logger = logger;
            _tmNccNewService = tmNccNewService;
            _baoGiaService = baoGiaService;
            _materialService = materialService;
            _tmSectionService = tmSectionService;
            _nhomViTriService = nhomViTriService;
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
        }
        // MARK: - Quote
        public async Task<IActionResult> Index()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 1000);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var listApproval = await _approverService.GetApproverByStepAndSectionAsync(2,GetCurrentUserSection());
            //await _tmEmployeeAgentService.GetApproverBySection(GetCurrentUserSection() ?? "");

            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                ListApprovel = listApproval.Data,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            return View(vm);
        }
        // MARK: - Quotation Results
        public async Task<IActionResult> Quotation_Results()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 1000);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync();
            //var danhSach = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync("", "", "", GetCurrentUserId(), 1, 10);
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
                    search.PageIndex ?? 1,
                    search.PageSize ?? 10);
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
                    ws.Cell(rowStart, col++).SetValue(item.DTM_NgayMuonNhan ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_KyHan ?? string.Empty);
                    // Vendor input
                    ws.Cell(rowStart, col++).SetValue(item.CodeEquipmentNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_TenHangHQ ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.NameENByNCC ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.soluong ?? 0); // Vendor quantity
                    ws.Cell(rowStart, col++).SetValue(item.donvi ?? string.Empty); // Vendor unit
                    ws.Cell(rowStart, col++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty); // Vendor maker
                    ws.Cell(rowStart, col++).SetValue(item.FL_USD ?? 0.0);
                    ws.Cell(rowStart, col++).SetValue(item.FL_VND ?? 0.0);
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
                    ws.Cell(rowStart, col++).SetValue(item.DTM_EffectiveDate ?? string.Empty);
                    ws.Cell(rowStart, col++).SetValue(item.DTM_ExpiryDate ?? string.Empty);
                    // System count
                    string key = $"{item.CHR_MaDon ?? ""}|{item.CHR_MaThietBi ?? ""}|{item.CHR_MaNCC ?? ""}";
                    var tot = totals.ContainsKey(key) ? totals[key] : (0.0, 0.0);
                    string totalCell = "";
                    if (tot.Item1 != 0)
                    {
                        totalCell = tot.Item1.ToString("N0") + " VND";
                    }
                    else if (tot.Item2 != 0)
                    {
                        totalCell = tot.Item2.ToString("N0") + " USD";
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
        // Nhập lựa chọn báo giá file excel
        [HttpPost]
        public async Task<IActionResult> ImportQuotianExcel([FromForm] IFormFile file)
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
                    var id = ws.Cell(r, 3).GetString(); // Assuming ID is in column 3
                    var bitSelect = ws.Cell(r, 51).GetString();
                    var reason = ws.Cell(r, 52).GetString();

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
                        items.Add(new
                        {
                            ID = id,
                            BIT_Select = bitSelect.Contains("O"),
                            NVCHR_ReasonPick = reason,
                            CHR_MaDon = maDon
                        });
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
                    }).ToList();
                    var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(dtoList);
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
        public async Task<IActionResult> SavePickSupplier([FromBody] List<BaoGia_Detail_of_QuotationDTO> listPick)
        {
            var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(listPick);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            _ = Task.Run(() => {

            });
            return Ok(result.Data);
        }
        // MARK: - Input Quote
        public async Task<IActionResult> InputQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 1000);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync();
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
            var materials = await _materialService.SearchAsync("", "", "", 1, 1000);
            var nccNews = await LoadNhaCungCapDataAsync();
            var madons = await LoadMadonAsync();
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
            var materials = await _materialService.SearchAsync("", "", "", 1, 1000);
            var nccNews = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var statusData = await _baoGiaStatusService.GetListStatusAsync();
            var madons = await LoadMadonAsync();
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccNews,
                DanhSachMaDon = madons,
                DanhSachCategory = categorys,
                DanhSachStatus = statusData.Data ?? new List<BaoGia_StatusDTO>(),
                NguoiThaoTac = GetCurrentUserId() ?? ""
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
        private async Task<List<ACC_NHOMVITRIDTO>> LoadNhomViTriDataAsync()
        {
            //var nhomViTri = await _nhomViTriService.GetAllNhomViTriAsync();
            var nhomViTri = await _nhomViTriService.GetNhomViTriByDepartmentIdAsync(GetCurrentUserId() ?? "");
            return nhomViTri.Data ?? new List<ACC_NHOMVITRIDTO>();
        }
        private async Task<List<IM_NCC_NEWDTO>> LoadNhaCungCapDataAsync()
        {
            var nccNews = await _tmNccNewService.GetAllNccNew();
            return nccNews.Data ?? new List<IM_NCC_NEWDTO>();
        }
        private async Task<List<string>> LoadMadonAsync()
        {
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "");
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
            var result = await _baoGiaService.NhapDanhSachBaoGiaAsync(danhSachBaoGia);
            // result.Data contains inserted DTOs with IDs
            try
            {
                var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
                // Capture user info locally to avoid accessing HttpContext inside background tasks
                var currentUserId = GetCurrentUserId();
                var currentUserFullName = GetCurrentUserFullName();
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
                // xac nhan ten
                var MaterialsNew = insertedList
                    .Where(l => string.IsNullOrEmpty(l.CHR_MaHangNoiBo))
                    .DistinctBy(l => l.CHR_MaHangNCC)
                    .Select(l => (l.ID, l.NVCHR_NameVN, l.CHR_MaHangNCC))
                    .ToList();
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
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
                                var baoGiaConfirmNameService = scope.ServiceProvider.GetRequiredService<IBaoGiaConfirmNameService>();
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                var listConfirm = new List<BaoGia_Confirm_Name_QuotationDTO>();
                                foreach (var i in MaterialsNew)
                                {
                                    var cf = new BaoGia_Confirm_Name_QuotationDTO();
                                    cf.ID_RequestQuote = i.ID;
                                    cf.DTM_CreateDate = DateTime.Now;
                                    cf.VCHR_CreateBy = currentUserId;
                                    cf.VCHR_TenRecomment = i.NVCHR_NameVN;
                                    cf.CHR_Status = "Confirming";
                                    cf.CHR_StatusACC = "Confirming";
                                    cf.CHR_StatusShip = "Confirming";
                                    cf.NVCHR_Note = i.CHR_MaHangNCC;
                                    listConfirm.Add(cf);
                                }
                                await baoGiaConfirmNameService.AddListAsync(listConfirm);
                                // gửi mail thông báo có yêu cầu xác nhận tên mới
                                //var emailResult = await sendMailService.SendMailAsync(
                                //    "PhuongThuy.VuThi@brother-bivn.com.vn;nguyenduy.khanh@brother-bivn.com.vn;nguyenthilan.huong2@brother-bivn.com.vn",
                                //    string.Empty,
                                //    17,
                                //    "http://172.26.248.62:8057/Material/ConfirmName",
                                //    true,
                                //    string.Empty,
                                //    string.Empty,
                                //    currentUserId);
                                var emailResult = await sendMailService.SendMailToConfirmItemAsync(13, 17, "http://172.26.248.62:8057/Material/ConfirmName", true, "", "", currentUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                            }
                        }
                    });
                }
                // Gui mail phe duyet trong background
                var SectionApporve = insertedList
                    .DistinctBy(l => new { l.CHR_MaDon, l.CHR_SectionCode })
                    .Select(l => (l.CHR_SectionCode, l.CHR_SectionName, l.CHR_MaDon, l.CHR_Gap, l.ID_StepBaoGia))
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
                                    await sendMailService.SendMailToRequesterAsync(item.CHR_MaDon ?? "", item.CHR_SectionCode ?? "", item.CHR_SectionName ?? "", item.CHR_Gap == "false" ? false : true, item.ID_StepBaoGia ?? 2);
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
            return Ok(result.Data);
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
        // Lấy thông tin NCC 
        [HttpPost]
        public async Task<IActionResult> GetNhaCungCapByMaHang([FromBody] string maHang)
        {
            var result = await _baoGiaNCCService.GetBaoGiaNCCByMaHang(maHang);
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
                    ws.Cell(row, 32).SetValue(rq?.CHR_CreateBy ?? string.Empty);
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

            var items = new List<BaoGia_Request_of_QuotationDTO>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // Dữ liệu bắt đầu từ dòng 10
                int startRow = 10;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break; // kết thúc nếu gặp dòng trống ở cột Mã phòng ban
                    }
                    var dto = new BaoGia_Request_of_QuotationDTO();
                    var a = ws.Cell(r, 6).GetString();
                    // case 1 : nếu có mã hàng nội bộ, sẽ tự động check nhà cung cấp và nhân bản theo số lượng nhà cung cấp tìm được
                    if (ws.Cell(r, 6).GetString() != "" && ws.Cell(r, 6).GetString() != null)
                    {
                        // lấy thông tin theo mã hàng nội bộ đã có sẵn
                        var inforMateial = await _materialService.GetByMaHangAsync(ws.Cell(r, 6).GetString());

                        if (inforMateial.Success && inforMateial.Data != null)
                        {
                            var infor = inforMateial.Data;
                            var nmn = ParseDate(ws.Cell(r, 29).GetString());
                            var DT = ParseDate(ws.Cell(r, 30).GetString());
                            // Map theo thứ tự cột trong bảng ở giao diện
                            dto = new BaoGia_Request_of_QuotationDTO
                            {
                                CHR_SectionCode = ws.Cell(r, 2).GetString(), // Mã phòng ban (value)
                                CHR_SectionName = ws.Cell(r, 3).GetString(), // hiển thị có thể giống mã
                                CHR_Phanloai = infor.LoaiHang,
                                CHR_MaHangNoiBo = infor.Material_Code,
                                NVCHR_NameVN = infor.TenMoThuTuc,
                                CHR_NameEN = infor.Material_Name_EN,
                                INT_SoLuong = ParseDouble(ws.Cell(r, 10).GetString()),
                                NVCHR_DonVi = infor.Unit ?? ws.Cell(r, 11).GetString(),
                                NVCHR_ChungLoai = infor.Category_VN,
                                NVCHR_HinhDang = infor.Shape,
                                NVCHR_ChatLieu = infor.Material,
                                NVCHR_ThanhPhan = infor.Composition,
                                NVCHR_KichThuoc = infor.Dimension,
                                NVCHR_DongMay = infor.UsedFor,
                                NVCHR_TinhNang = infor.Purpose,
                                NVCHR_Rohs = ws.Cell(r, 19).GetString(),
                                NVCHR_COCQ = ws.Cell(r, 20).GetString(),
                                NVCHR_MSDS = ws.Cell(r, 21).GetString(),
                                NVCHR_AnToan = ws.Cell(r, 22).GetString(),
                                NVCHR_FileThietKe = ws.Cell(r, 23).GetString(),
                                NVCHR_NhaSanXuat = ws.Cell(r, 24).GetString(),

                                CHR_MaNCC = ws.Cell(r, 25).GetString(),
                                NVCHR_TenNCC = ws.Cell(r, 26).GetString(),

                                BIT_LayBaoGia = ParseBool(ws.Cell(r, 27).GetString()),
                                NVCHR_LyDo = ws.Cell(r, 28).GetString(),
                                DTM_NgayMuonNhan = ParseDate(ws.Cell(r, 29).GetString()),
                                DTM_KyHan = ParseDate(ws.Cell(r, 30).GetString()),
                                CHR_Gap = ws.Cell(r, 31).GetString(),
                                CHR_CreateBy = ws.Cell(r, 32).GetString() ?? GetCurrentUserId() ?? string.Empty,
                                DTM_CreateDate = DateTime.Now,
                                ID_Status = "CREATE"
                            };
                            // Đã có thông tin nhà cung cấp 
                            if (!string.IsNullOrEmpty(dto.CHR_MaNCC))
                            {
                                items.Add(dto);
                                continue;
                            }
                            // Nếu có mã hàng nội bộ, tự check nhà cung cấp và nhân bản theo NCC
                            //var suppliersResp = await _baoGiaNCCService.GetBaoGiaNCCByMaHang(dto.CHR_MaHangNoiBo ?? string.Empty);
                            var suppliersResp = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(dto.NVCHR_ChungLoai ?? "");
                            if (suppliersResp.Success && suppliersResp.Data != null && suppliersResp.Data.Count > 0)
                            {
                                var first = true;
                                foreach (var sup in suppliersResp.Data)
                                {
                                    var copy = first ? dto : CloneDto(dto);
                                    // k có mà thiết bị
                                    //dto.CHR_MaThietBi = "";

                                    //dto.CHR_MaHangNCC = sup.NVCHR_CodeByNCC;
                                    copy.CHR_MaNCC = sup.CHR_MaNCC;
                                    copy.NVCHR_TenNCC = sup.NVCHR_TenNCC;
                                    items.Add(copy);
                                    first = false;
                                }
                            }
                            else
                            {
                                items.Add(dto);
                            }
                        }
                    }
                    // case 2 : nếu không có mã hàng nội bộ, sẽ map theo thứ tự cột trong bảng ở giao diện, và lấy thông tin nhà cung cấp theo chủng loại hàng
                    else
                    {
                        // Map theo thứ tự cột trong bảng ở giao diện
                        dto = new BaoGia_Request_of_QuotationDTO
                        {
                            CHR_SectionCode = ws.Cell(r, 2).GetString(), // Mã phòng ban (value)
                            CHR_SectionName = ws.Cell(r, 3).GetString(), // hiển thị có thể giống mã
                            CHR_Phanloai = ParsePhanloai(ws.Cell(r, 4).GetString()),
                            CHR_MaThietBi = ws.Cell(r, 5).GetString(),
                            CHR_MaHangNoiBo = ws.Cell(r, 6).GetString(),
                            CHR_MaHangNCC = ws.Cell(r, 7).GetString(),
                            NVCHR_NameVN = ws.Cell(r, 8).GetString(),
                            CHR_NameEN = ws.Cell(r, 9).GetString(),
                            INT_SoLuong = ParseDouble(ws.Cell(r, 10).GetString()),
                            NVCHR_DonVi = ws.Cell(r, 11).GetString(),
                            NVCHR_ChungLoai = ws.Cell(r, 12).GetString(),
                            NVCHR_HinhDang = ws.Cell(r, 13).GetString(),
                            NVCHR_ChatLieu = ws.Cell(r, 14).GetString(),
                            NVCHR_ThanhPhan = ws.Cell(r, 15).GetString(),
                            NVCHR_KichThuoc = ws.Cell(r, 16).GetString(),
                            NVCHR_DongMay = ws.Cell(r, 17).GetString(),
                            NVCHR_TinhNang = ws.Cell(r, 18).GetString(),
                            NVCHR_Rohs = ws.Cell(r, 19).GetString(),
                            NVCHR_COCQ = ws.Cell(r, 20).GetString(),
                            NVCHR_MSDS = ws.Cell(r, 21).GetString(),
                            NVCHR_AnToan = ws.Cell(r, 22).GetString(),
                            NVCHR_FileThietKe = ws.Cell(r, 23).GetString(),

                            NVCHR_NhaSanXuat = ws.Cell(r, 24).GetString(),
                            CHR_MaNCC = ws.Cell(r, 25).GetString(),
                            NVCHR_TenNCC = ws.Cell(r, 26).GetString(),

                            BIT_LayBaoGia = ParseBool(ws.Cell(r, 27).GetString()),
                            NVCHR_LyDo = ws.Cell(r, 28).GetString(),
                            DTM_NgayMuonNhan = ParseDate(ws.Cell(r, 29).GetString()),
                            DTM_KyHan = ParseDate(ws.Cell(r, 30).GetString()),
                            CHR_Gap = ws.Cell(r, 31).GetString(),
                            CHR_CreateBy = ws.Cell(r, 32).GetString() ?? GetCurrentUserId() ?? string.Empty,
                            DTM_CreateDate = DateTime.Now,
                            ID_Status = "CREATE"
                        };

                        // Đã có thông tin nhà cung cấp 
                        if (!string.IsNullOrEmpty(dto.CHR_MaNCC))
                        {
                            items.Add(dto);
                            continue;
                        }
                        // lấy thông tin nhà cung cấp theo chủng loại hàng  _baoGiaNccCategoryService 
                        var suppliersResp = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByChungLoai(ws.Cell(r, 12).GetString() ?? "");
                        if (suppliersResp.Success && suppliersResp.Data != null && suppliersResp.Data.Count > 0)
                        {
                            var first = true;
                            foreach (var sup in suppliersResp.Data)
                            {
                                var copy = first ? dto : CloneDto(dto);
                                copy.CHR_MaNCC = sup.CHR_MaNCC;
                                copy.NVCHR_TenNCC = sup.NVCHR_TenNCC;
                                copy.NVCHR_NhaSanXuat = sup.NVCHR_SanXuat;

                                items.Add(copy);
                                first = false;
                            }
                        }
                        else
                        {
                            items.Add(dto);
                        }
                    }
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        private static string? ParsePhanloai(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "No list";
            if (s != "A" && s != "B" && s != "C" && s != "E") return "No list";
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
            if (string.IsNullOrWhiteSpace(s)) return null;
            var v = s.Trim().ToLowerInvariant();
            return v.ToUpper().Contains("O") ? true : false;
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
        public async Task<IActionResult> PheDuyetBaoGia([FromBody] int model)
        {
            try
            {
                var baoGia = await _baoGiaService.GetByIdAsync(model);
                if (!baoGia.Success || baoGia.Data == null)
                {
                    return BadRequest("Không tìm thấy báo giá");
                }
                baoGia.Data.ID_Status = "PENDING";
                var updateResult = await _baoGiaService.CapNhatThongTinBaoGiaAsync(baoGia.Data);
                if (!updateResult.Success)
                {
                    return BadRequest(updateResult.Message);
                }
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
            var materialsResp = await _materialService.SearchAsync("", "", "", 1, 1000);
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
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 15;
                    // Bắt đầu đọc từ dòng 15
                    for (int r = 15; r <= lastRow; r++)
                    {
                        var errors = new List<string>();

                        // Kiểm tra các cột bắt buộc (17,18,19)
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 17).GetString())) errors.Add("Cột 17 (VCHR_CamKet) bắt buộc");
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 18).GetString())) errors.Add("Cột 18 (NVCHR_DeliveryTerm) bắt buộc");
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 19).GetString())) errors.Add("Cột 19 (NVCHR_PaymentTerm) bắt buộc");

                        // So sánh tên mở thủ tục hải quan (cột 3 vs 26) và tên tiếng Anh (4 vs 27)
                        if (!string.Equals(ws.Cell(r, 3).GetString(), ws.Cell(r, 26).GetString(), StringComparison.Ordinal)) errors.Add("Tên mở thủ tục hải quan khác nhau (cột 3 vs 24)");
                        if (!string.Equals(ws.Cell(r, 4).GetString(), ws.Cell(r, 27).GetString(), StringComparison.Ordinal)) errors.Add("Tên tiếng Anh khác nhau (cột 4 vs 25)");

                        // So sánh số lượng (5 vs 28)
                        var qty1 = ParseInt(ws.Cell(r, 5).GetString());
                        var qty2 = ParseInt(ws.Cell(r, 28).GetString());
                        if (qty1 == null || qty2 == null)
                        {
                            if (qty1 == null) errors.Add("Cột 5 không phải số hợp lệ");
                            if (qty2 == null) errors.Add("Cột 28 không phải số hợp lệ");
                        }
                        else if (qty1 != qty2) errors.Add("Số lượng khác nhau giữa cột 5 và 28");

                        // So sánh đơn vị (6 vs 29)
                        if (!string.Equals(ws.Cell(r, 6).GetString(), ws.Cell(r, 29).GetString(), StringComparison.Ordinal)) errors.Add("Đơn vị khác nhau (cột 6 vs 29)");

                        // Ngày giao hàng (cột 12) so sánh với yêu cầu (cột 38)
                        var ship = ParseDate(ws.Cell(r, 12).GetString());
                        var reqDate = ParseDate(ws.Cell(r, 38).GetString());
                        if (ship == null) errors.Add("Cột 12 (DTM_ShipTime) không phải ngày hợp lệ");
                        if (reqDate == null) errors.Add("Cột 38 (DTM_NgayMuonNhan yêu cầu) không phải ngày hợp lệ");
                        if (ship != null && reqDate != null && ship > reqDate) errors.Add("Thời gian giao hàng muộn hơn yêu cầu (cột 12 > cột 38)");

                        // MOQ (cột 9) <= Số lượng (cột 5)
                        var moq = ParseInt(ws.Cell(r, 9).GetString());
                        if (moq != null && qty1 != null && moq > qty1) errors.Add("MOQ (cột 9) lớn hơn Số lượng (cột 5)");

                        // Kiểm tra các điều kiện Rohs/COCQ/MSDS/AnToan: nếu expected yêu cầu nhưng value thiếu -> lỗi
                        //if (CheckNotRequired(ws.Cell(r, 13).GetString())) errors.Add("Rohs không thỏa mãn (cột 13)");
                        //if (CheckNotRequired(ws.Cell(r, 14).GetString())) errors.Add("CO/CQ không thỏa mãn (cột 14)");
                        //if (CheckNotRequired(ws.Cell(r, 15).GetString())) errors.Add("MSDS không thỏa mãn (cột 15)");
                        //if (CheckNotRequired(ws.Cell(r, 16).GetString())) errors.Add("An toàn không thỏa mãn (cột 16)");

                        if (errors.Any())
                        {
                            ws.Cell(r, 40).SetValue(string.Join("; ", errors));
                            hasErrors = true;
                            continue;
                        }
                        // lấy Id của đơn lưu trong csdl
                        var checkRQ = await _baoGiaDetailService.GetIdOfQuotationAsync(ws.Cell(r, 22).GetString(),
                            ws.Cell(r, 23).GetString(), ws.Cell(r, 36).GetString(), ws.Cell(r, 26).GetString());
                        if (!checkRQ.Success || checkRQ.Data == null)
                        {
                            ws.Cell(r, 40).SetValue("Không tìm thấy đơn hàng tương ứng trong hệ thống");
                            hasErrors = true;
                            continue;
                        }
                        var idRequestQuote = checkRQ.Data.Value;
                        // Nếu không có lỗi, tạo DTO và thêm vào danh sách
                        var dto = new BaoGia_Detail_of_QuotationDTO
                        {
                            ID = idRequestQuote,
                            CHR_MaHangNCC = ws.Cell(r, 2).GetString(),
                            NVCHR_TenHangHQ = ws.Cell(r, 3).GetString(),
                            CHR_NameEN = ws.Cell(r, 4).GetString(),
                            INT_SoLuong = (int?)ParseDouble(ws.Cell(r, 5).GetString()),
                            NVCHR_DonVi = ws.Cell(r, 6).GetString(),
                            FL_USD = ParseDouble(ws.Cell(r, 7).GetString()),
                            FL_VND = ParseDouble(ws.Cell(r, 8).GetString()),
                            NVCHR_MOQ = ws.Cell(r, 9).GetString(),
                            NVCHR_Packing = ws.Cell(r, 10).GetString(),
                            DTM_LeadTime = ws.Cell(r, 11).GetString(),
                            DTM_ShipTime = ParseDate(ws.Cell(r, 12).GetString()),
                            VCHR_Rohs = ws.Cell(r, 13).GetString(),
                            VCHR_COCQ = ws.Cell(r, 14).GetString(),
                            VCHR_MSDS = ws.Cell(r, 15).GetString(),
                            VCHR_AnToan = ws.Cell(r, 16).GetString(),
                            VCHR_CamKet = ws.Cell(r, 17).GetString(),
                            NVCHR_DeliveryTerm = ws.Cell(r, 18).GetString(),
                            NVCHR_PaymentTerm = ws.Cell(r, 19).GetString(),
                            DTM_EffectiveDate = ParseDate(ws.Cell(r, 20).GetString()),
                            DTM_ExpiryDate = ParseDate(ws.Cell(r, 21).GetString()),
                            CHR_UpdateBy = GetCurrentUserId(),
                            NVCHR_File = "",
                        };

                        items.Add(dto);
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
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                searchModel.PageIndex,
                searchModel.PageSize,
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
                foreach (var rq in result.Data)
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

                // Dữ liệu bắt đầu từ dòng 7 (the same layout as ExportHistory)
                int startRow = 7;
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
    }
}
