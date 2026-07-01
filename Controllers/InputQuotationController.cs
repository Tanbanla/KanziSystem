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
                        if (ws.Cell(r, 16).GetString().ToLower().Contains("refuse") || ws.Cell(r, 17).GetString().ToLower().Contains("refuse"))
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
                                var checkRQ1 = await _baoGiaDetailService.GetIdDetailAsync(ConvertHelper.ParseInt(ws.Cell(r, 32).GetString()));
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
                                CHR_MaHangNCC = ws.Cell(r, 11).GetString(),
                                NVCHR_TenHangHQ = ws.Cell(r, 12).GetString(),
                                CHR_NameEN = ws.Cell(r, 13).GetString(),
                                INT_SoLuong = (int?)ConvertHelper.ParseDouble(ws.Cell(r, 14).GetString()),
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
                            // kiểm tra có điền giá không
                            if (
                                (string.IsNullOrEmpty(ws.Cell(r, 16).GetString()) && string.IsNullOrEmpty(ws.Cell(r, 17).GetString()))
                                || (ws.Cell(r, 16).GetString() == "0" && ws.Cell(r, 16).GetString() == "0")
                            )
                            {
                                break;
                            }

                            // Kiểm tra các cột bắt buộc (22,23,24)
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 22).GetString())) errors.Add("Cột 22 (VCHR_CamKet) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 23).GetString())) errors.Add("Cột 23 (Delivery Term) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 24).GetString())) errors.Add("Cột 24 (Payment Term) bắt buộc");

                            // So sánh tên mở thủ tục hải quan (cột 3 vs 27) và tên tiếng Anh (5 vs 28)
                            //if (!string.Equals(ws.Cell(r, 3).GetString(), ws.Cell(r, 27).GetString(), StringComparison.Ordinal)) errors.Add("Tên mở thủ tục hải quan khác nhau (cột 3 vs 27)");
                            //if (!string.Equals(ws.Cell(r, 5).GetString(), ws.Cell(r, 28).GetString(), StringComparison.Ordinal)) errors.Add("Tên tiếng Anh khác nhau (cột 5 vs 28)");

                            // So sánh số lượng (6 vs 14)
                            var qty1 = ConvertHelper.ParseInt(ws.Cell(r, 6).GetString());
                            var qty2 = ConvertHelper.ParseInt(ws.Cell(r, 14).GetString());
                            if (qty1 == null || qty2 == null)
                            {
                                if (qty1 == null) errors.Add("Cột 6 không phải số hợp lệ");
                                if (qty2 == null) errors.Add("Cột 14 không phải số hợp lệ");
                            }
                            //else if (qty1 != qty2) errors.Add("Số lượng khác nhau giữa cột 6 và 14");

                            // So sánh đơn vị (7 vs 15)
                            //if (!string.Equals(ws.Cell(r, 7).GetString(), ws.Cell(r, 15).GetString(), StringComparison.Ordinal)) errors.Add("Đơn vị khác nhau (cột 7 vs 15)");

                            // Ngày giao hàng (cột 21) so sánh với yêu cầu (cột 28)
                            var ship = ConvertHelper.ParseDate(ws.Cell(r, 21).GetString());
                            var reqDate = ConvertHelper.ParseDate(ws.Cell(r, 28).GetString());
                            if (ship == null) errors.Add("Cột 21 (DTM_ShipTime) không phải ngày hợp lệ");
                            if (reqDate == null) errors.Add("Cột 28 (DTM_NgayMuonNhan yêu cầu) không phải ngày hợp lệ");
                            //if (ship != null && reqDate != null && ship > reqDate) errors.Add("Thời gian giao hàng muộn hơn yêu cầu (cột 13 > cột 39)");

                            // MOQ (cột 18) <= Số lượng (cột 14)
                            var moq = ConvertHelper.ParseInt(ws.Cell(r, 18).GetString());
                            //if (moq != null && qty1 != null && moq > qty1) errors.Add("MOQ (cột 18) lớn hơn Số lượng (cột 14)");

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
                                var checkRQ = await _baoGiaDetailService.GetIdDetailAsync(ConvertHelper.ParseInt(ws.Cell(r, 32).GetString()));
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
                            double costUSD = ConvertHelper.ParseDouble(ws.Cell(r, 16).GetString()) ?? 0;
                            double costVND = ConvertHelper.ParseDouble(ws.Cell(r, 17).GetString()) ?? 0;

                            // Nếu không có lỗi, tạo DTO và thêm vào danh sách
                            var dto = new BaoGia_Detail_of_QuotationDTO
                            {
                                ID = idRequestQuote,
                                CHR_MaHangNCC = ws.Cell(r, 11).GetString(),
                                NVCHR_TenHangHQ = ws.Cell(r, 12).GetString(),
                                CHR_NameEN = ws.Cell(r, 13).GetString(),
                                INT_SoLuong = qty2,
                                NVCHR_DonVi = ws.Cell(r, 15).GetString(),
                                FL_USD = costUSD != 0 ? costUSD : ConvertHelper.ParseVNDtoUSD(costVND, true, exchangeRate),
                                FL_VND = costVND != 0 ? costVND : ConvertHelper.ParseVNDtoUSD(costUSD, false, exchangeRate),
                                NVCHR_MOQ = moq.ToString(),
                                NVCHR_Packing = ws.Cell(r, 19).GetString(),
                                DTM_LeadTime = ws.Cell(r, 20).GetString(),
                                DTM_ShipTime = ConvertHelper.ParseDate(ws.Cell(r, 21).GetString()),

                                VCHR_Rohs = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_COCQ = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_MSDS = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_AnToan = agree.Contains("Đồng ý (accept)") ? "OK" : "NG",
                                VCHR_CamKet = agree,
                                NVCHR_DeliveryTerm = ws.Cell(r, 23).GetString(),
                                NVCHR_PaymentTerm = ws.Cell(r, 24).GetString(),
                                DTM_EffectiveDate = ConvertHelper.ParseDate(ws.Cell(r, 25).GetString()),
                                DTM_ExpiryDate = ConvertHelper.ParseDate(ws.Cell(r, 26).GetString()),
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
                if (!items.Any())
                {
                    return BadRequest("Không có dữ liệu hợp lệ để cập nhật");
                }
                // lấy thông tin để lưu file nhập báo giá
                var listInforFile = items.Select(c => c.NVCHR_File).Distinct().ToList();

                var savedMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var src in listInforFile)
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
                        return BadRequest($"Lỗi khi lưu file từ đường dẫn: {src}. Chi tiết: {ex.Message}");
                    }
                }
                foreach (var dto in items)
                {
                    if (string.IsNullOrWhiteSpace(dto.NVCHR_File)) continue;
                    var checkKey = dto.NVCHR_File?.Trim().Trim('"', '\'') ?? dto.NVCHR_File;
                    if (savedMap.TryGetValue(checkKey, out var saved) && !string.IsNullOrWhiteSpace(saved))
                    {
                        dto.NVCHR_File = saved;
                    }

                }

                // Luu vào csdl
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
