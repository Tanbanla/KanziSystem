using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using Path = System.IO.Path;
namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class HistoryController : BaseAuthController
    {

        private readonly ILogger<HistoryController> _logger;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IWebHostEnvironment _env;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IMaterialService _materialService;
        private readonly IConfiguration _configuration;

        private readonly ITmCategoryService _tmCategoryService;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IStringLocalizer<HistoryController> _localizer;

        public HistoryController(IWebHostEnvironment env,IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaService baoGiaService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaStepService baoGiaStepService, ILogger<HistoryController> logger, IServiceScopeFactory serviceScopeFactory,
            IMasterApproverSendMailService approverService, IMaterialService materialService, IConfiguration configuration
            ,ITmCategoryService tmCategoryService, IDepartmentService deparmentService, ITmNccNewService tmNccNewService, IStringLocalizer<HistoryController> localizer
            )
        {
            _env = env;
            _baoGiaHistoryService = baoGiaHistoryService;
            _baoGiaService = baoGiaService;
            _baoGiaStatusService = baoGiaStatusService;
            _baoGiaStepService = baoGiaStepService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _approverService = approverService;
            _materialService = materialService;
            _configuration = configuration;
            _tmCategoryService = tmCategoryService;
            _deparmentService = deparmentService;
            _tmNccNewService = tmNccNewService;
            _localizer = localizer;
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
        // Xuất file quản lý tiến độ màn hình lịch sử báo giá
        [HttpPost]
        public async Task<IActionResult> ExportManagerHistory([FromBody] SearchBaoGiaViewModel searchModel)
        {
            // Lấy thông tin dữ liệu lịch sử báo giá theo điều kiện tìm kiếm
            var result = await _baoGiaService.ExportHistoryBaoGiaAsync(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            // Lấy thông tin lịch sử phê duyệt theo điều kiện tìm kiếm
            var historyApprover = await _baoGiaHistoryService.GetHistoryApprover(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai);
            if (!historyApprover.Success)
            {
                return BadRequest(historyApprover.Message);
            }
            // lấy thông tin trạng thái theo đơn báo giá và mã hàng nội bộ
            var historyByMaterial = await _baoGiaHistoryService.GetHistoryByMaterialCode(searchModel.MaDon,
                searchModel.MaNcc, searchModel.Section,
                searchModel.NguoiYeuCau, searchModel.MaHang,
                searchModel.TrangThai, searchModel.Step, GetCurrentUserId(), searchModel.ChungLoai);
            if (!historyByMaterial.Success)
            {
                return BadRequest(historyByMaterial.Message);
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
                    .Where(rq => rq != null && rq?.ID_Status != null && rq?.ID_Status.Contains("RETURN"))
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
                var ws = workbook.Worksheet(1);
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }

                var wsApprover = workbook.Worksheets.Count >= 2 ? workbook.Worksheet(2) : null;
                if (wsApprover == null)
                {
                    return BadRequest("Không tìm thấy sheet 2 trong template");
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
                    if (rq.BIT_Select == null || (rq.BIT_Select == false && rq.ID_StepBaoGia <= 6))
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

                // Export dữ liệu lịch sử phê duyệt vào sheet 2
                var approverData = historyApprover.Data ?? new List<dynamic>();
                int rowApprover = 4;
                int sttApprover = 1;

                foreach (var item in approverData)
                {
                    if (item == null) continue;

                    int colApprover = 1;
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(sttApprover++); // No
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.maDon ?? string.Empty); // Mã đơn yêu cầu báo giá
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.ID_RequestQuote); // Số chi tiết đơn yêu cầu báo giá
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.userInsert ?? string.Empty); // PIC phòng ban yêu cầu
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.timeInsert?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty); // Thời gian tạo đơn yêu cầu báo giá
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.userChief ?? string.Empty); // QLSC Phê duyệt
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.timeChief?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty); // Thời gian QLSC phê duyệt
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.userSection ?? string.Empty); // QLTC Phê duyệt
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.timeSection?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty); // Thời gian QLTC phê duyệt
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.userPIC ?? string.Empty); // PIC phòng PUR tiếp nhận
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.timePIC?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty); // Thời gian PIC PUR tiếp nhận
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.userPur ?? string.Empty); // QLSC phòng PUR tiếp nhận
                    wsApprover.Cell(rowApprover, colApprover++).SetValue(item.timePur?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty); // Thời gian QLSC PUR phê duyệt

                    rowApprover++;
                }
                // Export dữ liệu trạng thái theo mã hàng nội bộ vào sheet 3
                var historyMaterialData = historyByMaterial.Data ?? new List<dynamic>();
                var wsMaterial = workbook.Worksheets.Count >= 3 ? workbook.Worksheet(3) : null;

                if (wsMaterial != null)
                {
                    int rowMaterial = 4;
                    int sttMaterial = 1;

                    foreach (var item in historyMaterialData)
                    {
                        if (item == null) continue;

                        int colMaterial = 1;
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(sttMaterial++); // No
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_MaDon ?? string.Empty); // Mã đơn yêu cầu báo giá
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_MaHangNoiBo ?? string.Empty); // Mã hàng nội bộ
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_SectionCode ?? string.Empty); // Mã phòng
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_SectionName ?? string.Empty); // Tên phòng
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_Phanloai ?? string.Empty); // Phan loại
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_MaThietBi ?? string.Empty); // Mã thiết bị
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_MaHangNCC ?? string.Empty); // Mã hàng NCC
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_NameVN ?? string.Empty); // Tên hàng Việt Nam
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_NameEN ?? string.Empty); // Tên hàng English
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.INT_SoLuong ?? 0); // Số lượng
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_DonVi ?? string.Empty); // Đơn vị
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_ChungLoai ?? string.Empty); // Chủng loại
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_File ?? string.Empty); // link Box
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_LinkFile ?? string.Empty); // file thiet ke
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_NhaSanXuat ?? string.Empty); // Nhà sản xuất 
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_MaNCC ?? string.Empty); // Mã NCC
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_TenNCC ?? string.Empty); // Tên NCC
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.BIT_LayBaoGia == false ? "X" : "O"); ; // Lý do
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_LyDo ?? string.Empty); // Lý do
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.DTM_NgayMuonNhan?.ToString("dd/MM/yyyy") ?? string.Empty); // Ngày muốn nhận
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.DTM_KyHan?.ToString("dd/MM/yyyy") ?? string.Empty); // Kỳ hạn
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.CHR_Gap == "false" ? "X" : "O"); // Gấp
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_UserRequest ?? string.Empty); // Người yêu cầu
                                                                                                                      // Lấy lý do trả (chỉ khi có RETURN)
                        var reason = (item.ID_Status != null && item.ID_Status.Contains("RETURN"))
                            ? (listReason.FirstOrDefault(c => c.Id == item.ID)?.Reason ?? "")
                            : "";

                        // Lấy tên trạng thái
                        var statusName = Status.Data?
                            .FirstOrDefault(s => s.VCHR_CodeStatus == item.ID_Status)?
                            .NVCHR_TenStatus ?? string.Empty;

                        // Lấy tên step
                        var stepName = Steps.Data?
                            .FirstOrDefault(s => s.INT_StepNumber == item.ID_StepBaoGia)?
                            .CHR_StepName ?? string.Empty;

                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(statusName);
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(stepName);
                        wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(reason);
                        if (item.BIT_Select == null || (item.BIT_Select == false && item.ID_StepBaoGia <= 6))
                        {
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue("");
                        }
                        else
                        {
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.BIT_Select == true ? "O" : "X");
                        }

                        // Lý do pick & file (chỉ khi có chọn)
                        if (item.BIT_Select == true)
                        {
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_ReasonPick ?? string.Empty);
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(item.NVCHR_File ?? string.Empty);
                        }
                        else
                        {
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(string.Empty);
                            wsMaterial.Cell(rowMaterial, colMaterial++).SetValue(string.Empty);
                        }

                        rowMaterial++;
                    }
                }
                else
                {
                    return BadRequest("Không tìm thấy sheet 3 trong template");
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
                    dto.INT_SoLuong = ConvertHelper.ParseDouble(ws.Cell(i, 12).GetString());
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
                    dto.BIT_LayBaoGia = ConvertHelper.ParseBool(ws.Cell(i, 29).GetString());
                    dto.NVCHR_LyDo = ws.Cell(i, 30).GetString();
                    dto.DTM_NgayMuonNhan = ConvertHelper.ParseDate(ws.Cell(i, 31).GetString());
                    dto.DTM_KyHan = ConvertHelper.ParseDate(ws.Cell(i, 32).GetString());
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
                if (checkUpdate.Data)
                {
                    return BadRequest("Không thể cập nhật đơn vì có đơn đang không ở trạng thái RETURN");
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
                            await sendMailService.SendMailAsync(firstData?.CHR_CreateBy + "@brothergroup.net", user + "@brothergroup.net", 12, "History/HistoryQuote", firstData?.CHR_Gap == "false" ? false : true, firstData?.CHR_SectionCode ?? "", firstData?.CHR_MaDon ?? "", user);
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
        // tìm kiếm theo ID đơn báo giá
        [HttpPost]
        public async Task<IActionResult> SearchID([FromBody] int id)
        {
            var result = await _baoGiaService.GetByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
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

    }
}
