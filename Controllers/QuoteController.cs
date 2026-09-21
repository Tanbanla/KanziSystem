using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
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
        private readonly IBaoGiaRequestTypeService _baoGiaRequestTypeService;
        private readonly IBaoGiaWFDefinitionService _baoGiaWorkflowDefinitionService;

        private readonly IStringLocalizer<QuoteController> _localizer;

        public QuoteController(ILogger<QuoteController> logger, ITmNccNewService tmNccNewService, IConfiguration configuration,
            IBaoGiaService baoGiaService, IMaterialService materialService, ITmSectionService tmSectionService, IExchangeRateService exchangeRateService,
           IDepartmentService deparmentService,  IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaStepService baoGiaStepService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaDetailService baoGiaDetailService, IBaoGiaRequestTypeService baoGiaRequestTypeService,
            ITmCategoryService tmCategoryService, IBaoGiaNccCategoryService baoGiaNccCategoryService, ITmEmployeeAgentService tmEmployeeAgentService,
            IWebHostEnvironment env, ISendMailService sendMailService, IServiceScopeFactory serviceScopeFactory, IMasterApproverSendMailService approverService,
            IStringLocalizer<QuoteController> localizer, IBaoGiaWFDefinitionService baoGiaWorkflowDefinitionService,
            IFileImportService fileImportService)
        {
            _logger = logger;
            _configuration = configuration;
            _tmNccNewService = tmNccNewService;
            _baoGiaService = baoGiaService;
            _materialService = materialService;
            _tmSectionService = tmSectionService;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaDetailService = baoGiaDetailService;
            _tmCategoryService = tmCategoryService;
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
            _baoGiaWorkflowDefinitionService = baoGiaWorkflowDefinitionService;
            _baoGiaRequestTypeService = baoGiaRequestTypeService;
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
        public async Task<IActionResult> QuoteV2()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 1, 500);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var requestTypesResp = await _baoGiaRequestTypeService.GetAllAsync();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";

            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                NguoiThaoTac = GetCurrentUserId() ?? "",
                RequestTypes = (List<BaoGia_RequestTypeDTO>)(requestTypesResp.Data ?? new List<BaoGia_RequestTypeDTO>())
            };
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
            // kiểm tra số lượng nhà cung cấp cho mỗi sản phẩm (MaHangNoiBo, MaHangNCC) không vượt quá 5
            var violatingGroups = danhSachBaoGia
                .Where(d => d != null && d.BIT_LayBaoGia == true)
                .GroupBy(d => (MaHangNoiBo: (d.CHR_MaHangNoiBo ??  string.Empty).Trim(), MaHangNcc: (d.CHR_MaHangNCC ?? string.Empty).Trim()
                , CHR_NameEN: (d.CHR_NameEN ?? string.Empty).Trim(), CHR_MaThietBi: (d.CHR_MaThietBi ?? string.Empty).Trim()
                ))
                .Select(g => new
                {
                    Key = g.Key,
                    DistinctSupplierCount = g.Select(x => (x.CHR_MaNCC ?? string.Empty).Trim())
                                            .Where(s => !string.IsNullOrEmpty(s))
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .Count()
                })
                .Where(x => x.DistinctSupplierCount > 5)
                .ToList();

            if (violatingGroups.Any())
            {
                var messages = violatingGroups.Select(v =>
                {
                    var mhnb = string.IsNullOrEmpty(v.Key.MaHangNoiBo) ? "(no internal code)" : v.Key.MaHangNoiBo;
                    var mhncc = string.IsNullOrEmpty(v.Key.MaHangNcc) ? "(no supplier product code)" : v.Key.MaHangNcc;
                    return $"Sản phẩm '{mhnb}' / '{mhncc}' có {v.DistinctSupplierCount} nhà cung cấp (vượt quá 5).";
                });

                return BadRequest("Lỗi ràng buộc nhà cung cấp: " + string.Join("; ", messages));
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
                        _logger.LogWarning(saveRes?.Message, "Failed saving link {Link}", src);
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
        // tạo mã đơn
        private static string? generateMaDonRequest(string? section) {
            try {
                var now = DateTime.Now;
                var utc = now.ToUniversalTime();
                var nowVN = utc.AddHours(7);
                var yyyy = nowVN.Year;
                var MM = nowVN.Month.ToString().PadLeft(2, '0');
                var dd = nowVN.Day.ToString().PadLeft(2, '0');
                var sec = (section ?? "").Trim().Replace("[^a-zA-Z0-9_-]", "_");
                if (string.IsNullOrEmpty(sec))
                {
                    sec = "GEN";
                }
                return $"RQ_{sec}_{yyyy}_{MM}_{dd}";
            } catch (Exception e) {
                Console.WriteLine("Error generating MaDonRequest: " + e.Message);
            }
                return null;
        }
        // Inser dữ liệu vào DB
        [HttpPost]
        public async Task<IActionResult> InsertQuotation([FromBody] List<InsertBaoGiaModel> items)
        {
            if (items == null || !items.Any())
            {
                return BadRequest("Danh sách báo giá trống");
            }

            try {
                var listInser = await ConvertModelToDTO(items);
                var currentUserId = GetCurrentUserId() ?? string.Empty;
                var createDate = DateTime.Now;

                if (listInser.Count == 0)
                {
                    return BadRequest("Không có nhà cung cấp hợp lệ để tạo yêu cầu báo giá.");
                }

                // Gọi service để insert danh sách báo giá
                var result = await _baoGiaService.NhapDanhSachBaoGiaAsync(listInser);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                var insertedList = result.Data ?? new List<BaoGia_Request_of_QuotationDTO>();
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
                    //_ = Task.Run(async () =>
                    //{
                    //    using (var scope = _serviceScopeFactory.CreateScope())
                    //    {
                    //        try
                    //        {
                    //            var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                    //            foreach (var item in SectionApporve)
                    //            {
                    //                await sendMailService.SendMailAsync(item.CHR_UserApproval + "@brothergroup.net", currentUserId + "@brothergroup.net", 11, "ApprovalQuote/Index", item.CHR_Gap == "false" ? false : true, item.CHR_SectionCode ?? "", item.CHR_MaDon ?? "", currentUserId);
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            _logger.LogError(ex, "Lỗi khi gửi mail phê duyệt");
                    //        }
                    //    }
                    //});
                }

                return Ok(listInser);

            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xử lý dữ liệu: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] List<InsertBaoGiaModel> items)
        {
            if (items == null || !items.Any()) return BadRequest("Danh sách báo giá trống");
            try
            {
                // lay thong tin WorkflowDefinition
                var workflowRespAsync = await _baoGiaWorkflowDefinitionService.GetAllAsync();
                if(!workflowRespAsync.Success|| workflowRespAsync.Data == null) 
                {
                    return BadRequest("Lỗi khi lấy thông tin WorkflowDefinition");
                }
                var workflowDefinitions = workflowRespAsync.Data;
                var list = await ConvertModelToDTO(items);

                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "TemplateQuationN.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: TemplateQuationN.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                int row = 10;
                foreach (var rq in list)
                {
                    // Map fields into template columns similar to ExportSelection
                    ws.Cell(row, 1).SetValue(row - 9); // status placeholder
                    ws.Cell(row, 2).SetValue(rq?.CHR_SectionCode ?? string.Empty);
                    ws.Cell(row, 3).SetValue(rq?.CHR_SectionName ?? string.Empty);
                    ws.Cell(row, 4).SetValue(rq?.CHR_Phanloai ?? string.Empty);
                    ws.Cell(row, 5).SetValue(workflowDefinitions.Where(w => w.WorkflowID == rq?.WorkflowID).Select(w => w.WorkflowName).FirstOrDefault() ?? string.Empty);
                    ws.Cell(row, 6).SetValue(rq?.CHR_MaThietBi ?? string.Empty);
                    ws.Cell(row, 7).SetValue(rq?.CHR_MaHangNoiBo ?? string.Empty);
                    ws.Cell(row, 8).SetValue(rq?.CHR_MaHangNCC ?? string.Empty);
                    ws.Cell(row, 9).SetValue(rq?.NVCHR_NameVN ?? string.Empty);
                    ws.Cell(row, 10).SetValue(rq?.CHR_NameEN ?? string.Empty);
                    ws.Cell(row, 11).SetValue(rq?.INT_SoLuong.HasValue == true ? rq.INT_SoLuong.Value : 0);
                    ws.Cell(row, 12).SetValue(rq?.NVCHR_DonVi ?? string.Empty);
                    ws.Cell(row, 13).SetValue(rq?.NVCHR_ChungLoai ?? string.Empty);
                    ws.Cell(row, 14).SetValue(rq?.NVCHR_HinhDang ?? string.Empty);
                    ws.Cell(row, 15).SetValue(rq?.NVCHR_ChatLieu ?? string.Empty);
                    ws.Cell(row, 16).SetValue(rq?.NVCHR_ThanhPhan ?? string.Empty);
                    ws.Cell(row, 17).SetValue(rq?.NVCHR_KichThuoc ?? string.Empty);
                    ws.Cell(row, 18).SetValue(rq?.NVCHR_DongMay ?? string.Empty);
                    ws.Cell(row, 19).SetValue(rq?.NVCHR_TinhNang ?? string.Empty);
                    ws.Cell(row, 20).SetValue(rq?.NVCHR_Rohs ?? string.Empty);
                    ws.Cell(row, 21).SetValue(rq?.NVCHR_COCQ ?? string.Empty);
                    ws.Cell(row, 22).SetValue(rq?.NVCHR_MSDS ?? string.Empty);
                    ws.Cell(row, 23).SetValue(rq?.NVCHR_AnToan ?? string.Empty);
                    ws.Cell(row, 24).SetValue(rq?.NVCHR_FileThietKe ?? string.Empty);
                    ws.Cell(row, 25).SetValue(rq?.CHR_LinkFile ?? string.Empty);
                    ws.Cell(row, 26).SetValue(rq?.NVCHR_NhaSanXuat ?? string.Empty);
                    ws.Cell(row, 27).SetValue(rq?.CHR_LinkImage ?? string.Empty);
                    ws.Cell(row, 28).SetValue(rq?.CHR_MaNCC ?? string.Empty);
                    ws.Cell(row, 29).SetValue(rq?.NVCHR_TenNCC ?? string.Empty);
                    ws.Cell(row, 30).SetValue(rq?.BIT_LayBaoGia == false ? "X" : "O");
                    ws.Cell(row, 31).SetValue(rq?.NVCHR_LyDo ?? string.Empty);
                    ws.Cell(row, 32).SetValue(rq?.DTM_NgayMuonNhan.HasValue == true ? rq.DTM_NgayMuonNhan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, 33).SetValue(rq?.DTM_KyHan.HasValue == true ? rq.DTM_KyHan.Value.ToString("dd/MM/yyyy") : string.Empty);
                    ws.Cell(row, 34).SetValue(rq?.CHR_Gap == "false" ? "X" : "O");
                    ws.Cell(row, 35).SetValue(rq?.NVCHR_UserRequest ?? string.Empty);
                    ws.Cell(row, 36).SetValue(rq?.NVCHR_ReasonQuotation ?? string.Empty);
                    ws.Cell(row, 37).SetValue(rq?.NVCHR_DiaDiemNH ?? string.Empty);
                    ws.Cell(row, 38).SetValue(rq?.NVCHR_NguoiNhan ?? string.Empty);
                    ws.Cell(row, 39).SetValue(rq?.CHR_SDT ?? string.Empty);
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
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
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
                var validationErrors = ValidateImportedWorksheet(ws, items);
                if (validationErrors.Count > 0)
                {
                    const int errorColumn = 40;
                    ws.Cell(9, errorColumn).Value = "Thông tin lỗi";
                    ws.Cell(9, errorColumn).Style.Font.Bold = true;
                    ws.Cell(9, errorColumn).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    foreach (var error in validationErrors)
                    {
                        var errorCell = ws.Cell(error.Row, errorColumn);
                        errorCell.Value = error.Message;
                        errorCell.Style.Font.FontColor = XLColor.Red;
                        errorCell.Style.Alignment.WrapText = true;
                    }

                    ws.Column(errorColumn).Width = 55;
                    using var errorStream = new MemoryStream();
                    workbook.SaveAs(errorStream);
                    Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    Response.Headers.Append("X-Import-Validation-Errors", validationErrors.Count.ToString());
                    return File(errorStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }

                var result = ConvertDTOToModel(items);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        private List<(int Row, string Message)> ValidateImportedWorksheet(
            IXLWorksheet ws,
            List<BaoGia_Request_of_QuotationDTO> items)
        {
            var errors = new List<(int Row, string Message)>();
            const int startRow = 10;
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow - 1;

            for (var row = startRow; row <= lastRow; row++)
            {
                if (ws.Row(row).IsEmpty())
                    continue;

                var sectionCode = ws.Cell(row, 2).GetString().Trim();
                if (sectionCode == null || sectionCode == "") break;

                var internalCode = ws.Cell(row, 7).GetString().Trim();
                var supplierItemCode = ws.Cell(row, 8).GetString().Trim();
                var rowItems = items
                    .Where(item => string.Equals(item.CHR_SectionCode?.Trim(), sectionCode, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(item.CHR_MaHangNoiBo?.Trim(), internalCode, StringComparison.OrdinalIgnoreCase)
                            || (string.IsNullOrWhiteSpace(internalCode)
                                && string.Equals(item.CHR_MaHangNCC?.Trim(), supplierItemCode, StringComparison.OrdinalIgnoreCase))))
                    .ToList();
                var rowErrors = new List<string>();

                if (!string.IsNullOrWhiteSpace(internalCode))
                {
                    if (!rowItems.Any(item => !string.IsNullOrWhiteSpace(item.CHR_MaNCC)
                        && !string.IsNullOrWhiteSpace(item.NVCHR_TenNCC)))
                        rowErrors.Add("Hàng có mã nội bộ bắt buộc phải có mã và tên nhà cung cấp.");
                }
                else
                {
                    if (!rowItems.Any(item => !string.IsNullOrWhiteSpace(item.CHR_MaHangNCC)))
                        rowErrors.Add("Hàng chưa có mã nội bộ bắt buộc phải có mã hàng nhà cung cấp.");
                    if (!rowItems.Any(item => !string.IsNullOrWhiteSpace(item.CHR_Phanloai)))
                        rowErrors.Add("Hàng chưa có mã nội bộ bắt buộc phải có phân loại hàng.");
                    if (!rowItems.Any(item => !string.IsNullOrWhiteSpace(item.NVCHR_ChungLoai)))
                        rowErrors.Add("Hàng chưa có mã nội bộ bắt buộc phải có chủng loại.");
                }

                if (!rowItems.Any(item => item.INT_SoLuong.HasValue))
                    rowErrors.Add("Số lượng là bắt buộc.");
                if (!rowItems.Any(item => !string.IsNullOrWhiteSpace(item.NVCHR_DonVi)))
                    rowErrors.Add("Đơn vị là bắt buộc.");
                if (string.IsNullOrWhiteSpace(sectionCode))
                    rowErrors.Add("Phòng ban là bắt buộc.");
                if (!rowItems.Any(item => item.DTM_KyHan.HasValue))
                    rowErrors.Add("Kỳ hạn lựa chọn nhà cung cấp là bắt buộc.");
                if (rowItems.Any(item => item.BIT_LayBaoGia == false
                    && string.IsNullOrWhiteSpace(item.NVCHR_LyDo)))
                    rowErrors.Add("Nhà cung cấp từ chối lấy báo giá thì phải nhập lý do.");

                if (rowErrors.Count > 0)
                    errors.Add((row, string.Join(" ", rowErrors)));
            }

            return errors;
        }

        // Convert dữ liệu từ model sang DTO
        private async Task<List<BaoGia_Request_of_QuotationDTO>> ConvertModelToDTO(List<InsertBaoGiaModel> items)
        {
            var list = new List<BaoGia_Request_of_QuotationDTO>();
            var currentUserId = GetCurrentUserId() ?? string.Empty;
            var createDate = DateTime.Now;

            // thong tin workflowID
            var workflowRespAsync = await _baoGiaWorkflowDefinitionService.GetWorkflowIDs();
            var wfREsult = workflowRespAsync.Data;

            var madon = generateMaDonRequest(items.FirstOrDefault()?.CHR_SectionCode ?? string.Empty);

            foreach (var item in items)
            {
                var vendors = item.Vendors ?? new List<VendorQuoteModel>();
                var selectedVendors = vendors
                    .Where(v => v != null && v.BIT_LayBaoGia && !string.IsNullOrWhiteSpace(v.MaNcc))
                    .Select(v => v.MaNcc!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                if (selectedVendors > 5)
                {
                    throw new Exception($"Mặt hàng '{item.CHR_MaHangNoiBo ?? item.CHR_NameEN}' có hơn 5 nhà cung cấp được chọn.");
                }

                if (vendors.Count == 0)
                {
                    throw new Exception($"Mặt hàng '{item.CHR_MaHangNoiBo ?? item.CHR_NameEN}' chưa có nhà cung cấp.");
                }

                foreach (var vendor in vendors)
                {
                    if (vendor == null || string.IsNullOrWhiteSpace(vendor.MaNcc))
                    {
                        continue;
                    }

                    list.Add(new BaoGia_Request_of_QuotationDTO
                    {
                        CHR_MaDon = madon,
                        CHR_CreateBy = currentUserId,
                        DTM_CreateDate = createDate,
                        CHR_Gap = item.CHR_Gap,
                        CHR_MaHangNoiBo = item.CHR_MaHangNoiBo,
                        CHR_MaThietBi = item.CHR_MaThietBi,
                        CHR_NameEN = item.CHR_NameEN,
                        CHR_Phanloai = item.CHR_Phanloai,
                        CHR_SectionCode = item.CHR_SectionCode,
                        CHR_SectionName = item.CHR_SectionName,
                        DTM_Deadline = item.DTM_Deadline,
                        DTM_KyHan = item.DTM_KyHan,
                        DTM_NgayMuonNhan = item.DTM_NgayMuonNhan,
                        NVCHR_AnToan = item.NVCHR_AnToan,
                        NVCHR_COCQ = item.NVCHR_COCQ,
                        NVCHR_ChatLieu = item.NVCHR_ChatLieu,
                        NVCHR_ChungLoai = item.NVCHR_ChungLoai,
                        NVCHR_DonVi = item.NVCHR_DonVi,
                        NVCHR_DongMay = item.NVCHR_DongMay,
                        NVCHR_FileThietKe = item.NVCHR_FileThietKe,
                        NVCHR_HinhDang = item.NVCHR_HinhDang,
                        NVCHR_KichThuoc = item.NVCHR_KichThuoc,
                        NVCHR_MSDS = item.NVCHR_MSDS,
                        NVCHR_NameVN = item.NVCHR_NameVN,
                        NVCHR_Rohs = item.NVCHR_Rohs,
                        NVCHR_ThanhPhan = item.NVCHR_ThanhPhan,
                        NVCHR_TinhNang = item.NVCHR_TinhNang,
                        CHR_UserApproval = item.CHR_UserApproval,
                        NVCHR_UserRequest = item.NVCHR_UserRequest,
                        INT_SoLuong = item.INT_SoLuong,
                        NVCHR_ReasonQuotation = item.NVCHR_ReasonQuotation,
                        CHR_MaHangNCC = item.CHR_MaHangNCC,
                        CHR_MaNCC = vendor.MaNcc.Trim(),
                        NVCHR_TenNCC = vendor.TenNcc,
                        NVCHR_NhaSanXuat = vendor.NhaSanXuat,
                        BIT_LayBaoGia = vendor.BIT_LayBaoGia,
                        NVCHR_LyDo = vendor.NVCHR_LyDo,
                        CHR_LinkImage = item.CHR_LinkImage,
                        NVCHR_DiaDiemNH = item.NVCHR_DiaDiemNH,
                        NVCHR_NguoiNhan = item.NVCHR_NguoiNhan,
                        CHR_SDT = item.CHR_SDT,
                        WorkflowID = wfREsult?.FirstOrDefault(w => w.FlowCode == item.WfSection && w.CHR_Code == item.WfType)?.WorkflowID ?? 1
                    });
                }
            }

            return list;
        }

        // Convert dữ liệu từ DTO sang model, gom các dòng theo từng mặt hàng
        private List<InsertBaoGiaModel> ConvertDTOToModel(List<BaoGia_Request_of_QuotationDTO> items)
        {
            if (items == null || items.Count == 0)
            {
                return new List<InsertBaoGiaModel>();
            }

            return items
                .Where(item => item != null)
                .GroupBy(item => new
                {
                    item.CHR_MaDon,
                    item.CHR_MaHangNoiBo,
                    item.CHR_MaHangNCC,
                    item.CHR_MaThietBi,
                    item.CHR_NameEN,
                    item.CHR_Phanloai,
                    item.CHR_SectionCode,
                    item.CHR_SectionName,
                    item.DTM_Deadline,
                    item.DTM_KyHan,
                    item.DTM_NgayMuonNhan
                })
                .Select(group =>
                {
                    var first = group.First();

                    return new InsertBaoGiaModel
                    {
                        CHR_CreateBy = first.CHR_CreateBy,
                        CHR_Gap = first.CHR_Gap,
                        CHR_MaHangNoiBo = first.CHR_MaHangNoiBo,
                        CHR_MaHangNCC = first.CHR_MaHangNCC,
                        CHR_MaThietBi = first.CHR_MaThietBi,
                        CHR_NameEN = first.CHR_NameEN,
                        CHR_Phanloai = first.CHR_Phanloai,
                        CHR_SectionCode = first.CHR_SectionCode,
                        CHR_SectionName = first.CHR_SectionName,
                        DTM_Deadline = first.DTM_Deadline,
                        DTM_KyHan = first.DTM_KyHan,
                        DTM_NgayMuonNhan = first.DTM_NgayMuonNhan,
                        NVCHR_AnToan = first.NVCHR_AnToan,
                        NVCHR_COCQ = first.NVCHR_COCQ,
                        NVCHR_ChatLieu = first.NVCHR_ChatLieu,
                        NVCHR_ChungLoai = first.NVCHR_ChungLoai,
                        NVCHR_DonVi = first.NVCHR_DonVi,
                        NVCHR_DongMay = first.NVCHR_DongMay,
                        NVCHR_FileThietKe = first.NVCHR_FileThietKe,
                        NVCHR_HinhDang = first.NVCHR_HinhDang,
                        NVCHR_KichThuoc = first.NVCHR_KichThuoc,
                        NVCHR_MSDS = first.NVCHR_MSDS,
                        NVCHR_NameVN = first.NVCHR_NameVN,
                        NVCHR_Rohs = first.NVCHR_Rohs,
                        NVCHR_ThanhPhan = first.NVCHR_ThanhPhan,
                        NVCHR_TinhNang = first.NVCHR_TinhNang,
                        CHR_UserApproval = first.CHR_UserApproval,
                        NVCHR_UserRequest = first.NVCHR_UserRequest,
                        INT_SoLuong = first.INT_SoLuong.HasValue
                            ? Convert.ToInt32(first.INT_SoLuong.Value)
                            : null,
                        NVCHR_ReasonQuotation = first.NVCHR_ReasonQuotation,
                        CHR_LinkImage = first.CHR_LinkImage,
                        NVCHR_DiaDiemNH = first.NVCHR_DiaDiemNH,
                        NVCHR_NguoiNhan = first.NVCHR_NguoiNhan,
                        CHR_SDT = first.CHR_SDT,
                        Vendors = group
                            .Where(item => !string.IsNullOrWhiteSpace(item.CHR_MaNCC))
                            .Select(item => new VendorQuoteModel
                            {
                                MaNcc = item.CHR_MaNCC,
                                TenNcc = item.NVCHR_TenNCC,
                                NhaSanXuat = item.NVCHR_NhaSanXuat,
                                BIT_LayBaoGia = item.BIT_LayBaoGia ?? false,
                                NVCHR_LyDo = item.NVCHR_LyDo
                            })
                            .ToList()
                    };
                })
                .ToList();
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
        // Upload Excel để nhập danh sách yêu cầu báo giá

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
                SectionCode = ws.Cell(row, 2).GetString().Trim(),
                SectionName = ws.Cell(row, 3).GetString(),
                Phanloai = ws.Cell(row, 4).GetString(),
                TypeQuation = ws.Cell(row, 5).GetString(),
                MaThietBi = ws.Cell(row, 6).GetString(),
                MaHangNoiBo = ws.Cell(row, 7).GetString().Trim(),
                MaHangNCC = ws.Cell(row, 8).GetString(),
                NameVN = ws.Cell(row, 9).GetString(),
                NameEN = ws.Cell(row, 10).GetString(),
                SoLuong = ws.Cell(row, 11).GetString(),
                DonVi = ws.Cell(row, 12).GetString(),
                ChungLoai = ws.Cell(row, 13).GetString().Trim(),
                HinhDang = ws.Cell(row, 14).GetString(),
                ChatLieu = ws.Cell(row, 15).GetString(),
                ThanhPhan = ws.Cell(row, 16).GetString(),
                KichThuoc = ws.Cell(row, 17).GetString(),
                DongMay = ws.Cell(row, 18).GetString(),
                TinhNang = ws.Cell(row, 19).GetString(),
                Rohs = ws.Cell(row, 20).GetString(),
                COCQ = ws.Cell(row, 21).GetString(),
                MSDS = ws.Cell(row, 22).GetString(),
                AnToan = ws.Cell(row, 23).GetString(),
                FileThietKe = ws.Cell(row, 24).GetString(),
                CHR_LinkFile = ws.Cell(row, 25).GetString(),
                NhaSanXuat = ws.Cell(row, 26).GetString(),
                CHR_LinkImage = ws.Cell(row, 27).GetString(),
                MaNCC = ws.Cell(row, 28).GetString(),
                TenNCC = ws.Cell(row, 29).GetString(),
                LayBaoGia = ws.Cell(row, 30).GetString(),
                LyDo = ws.Cell(row, 31).GetString(),
                NgayMuonNhan = ws.Cell(row, 32).GetString(),
                KyHan = ws.Cell(row, 33).GetString(),
                Gap = ws.Cell(row, 34).GetString(),
                UserRequest = ws.Cell(row, 35).GetString(),
                ReasonQuote = ws.Cell(row, 36).GetString(),
                NVCHR_DiaDiemNH = ws.Cell(row, 37).GetString(),
                NVCHR_NguoiNhan = ws.Cell(row, 38).GetString(),
                CHR_SDT = ws.Cell(row, 39).GetString()
            };
        }

        private async Task<List<BaoGia_Request_of_QuotationDTO>> ProcessRowData(ExcelRowData rowData)
        {
            var items = new List<BaoGia_Request_of_QuotationDTO>();

            // Case 1: Có mã hàng nội bộ
            if (!string.IsNullOrEmpty(rowData.MaHangNoiBo?.Trim()))
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
                CHR_MaHangNCC = material.Code_Suppiler,//string.IsNullOrEmpty(rowData.MaHangNCC) ? material.Code_Suppiler : rowData.MaHangNCC,
                NVCHR_NameVN = (rowData.NameVN == "" || rowData.NameVN == null) ? material.Material_Name_VN : rowData.NameVN,
                CHR_NameEN = (rowData.NameEN == "" || rowData.NameEN == null) ? material.Material_Name_EN : rowData.NameEN,
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
                CHR_LinkFile = rowData.CHR_LinkFile,
                CHR_LinkImage = rowData.CHR_LinkImage,
                NVCHR_DiaDiemNH = rowData.NVCHR_DiaDiemNH,
                NVCHR_NguoiNhan = rowData.NVCHR_NguoiNhan,
                CHR_SDT = rowData.CHR_SDT
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
                CHR_LinkFile = rowData.CHR_LinkFile,
                CHR_LinkImage = rowData.CHR_LinkImage,
                NVCHR_DiaDiemNH = rowData.NVCHR_DiaDiemNH,
                NVCHR_NguoiNhan = rowData.NVCHR_NguoiNhan,
                CHR_SDT = rowData.CHR_SDT
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
                BIT_IsTemplate = src.BIT_IsTemplate,
                CHR_LinkFile = src.CHR_LinkFile,
                CHR_LinkImage = src.CHR_LinkImage,
                NVCHR_ReasonQuotation = src.NVCHR_ReasonQuotation,
                NVCHR_DiaDiemNH = src.NVCHR_DiaDiemNH,
                NVCHR_NguoiNhan = src.NVCHR_NguoiNhan,
                CHR_SDT = src.CHR_SDT
            };
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
        // Check nhà cung cấp có theo chủng loại hàng hay không
        [HttpPost]
        public async Task<IActionResult> CheckNCCByCategory([FromBody] List<CheckSupplierByCategoryModel> request)
        {
            if (request == null || request.Count == 0)
            {
                return BadRequest("Danh sách yêu cầu không được để trống");
            }
            var result = await _baoGiaNccCategoryService.CheckSupperlierByCategory(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            if(result.Data.Count > 0)
            {
                return BadRequest("Các nhà cung cấp không tồn tại chủng loại: " + string.Join(", ", result.Data.Select(d => $"{d.MaDon}-{d.ChungLoai}")));
            }
            return Ok();
        }
    }
}
