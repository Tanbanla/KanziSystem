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
    public class InputQuoteController : BaseAuthController
    {
        private readonly ILogger<InputQuoteController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
        private readonly IMaterialService _materialService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IBaoGiaNccCategoryService _baoGiaNccCategoryService;
        private readonly IWebHostEnvironment _env;
        private readonly ISendMailService _sendMailService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IStringLocalizer<InputQuoteController> _localizer;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmNccNewService _tmNccNewService;

        public InputQuoteController(ILogger<InputQuoteController> logger, IConfiguration configuration,
            IBaoGiaService baoGiaService, IBaoGiaDetailService baoGiaDetailService, IMaterialService materialService,
            ITmCategoryService tmCategoryService, IBaoGiaNccCategoryService baoGiaNccCategoryService,
            IWebHostEnvironment env, ISendMailService sendMailService, IExchangeRateService exchangeRateService,
            IServiceScopeFactory serviceScopeFactory, IStringLocalizer<InputQuoteController> localizer,
            IDepartmentService deparmentService, ITmNccNewService tmNccNewService)
        {
            _logger = logger;
            _configuration = configuration;
            _baoGiaService = baoGiaService;
            _baoGiaDetailService = baoGiaDetailService;
            _materialService = materialService;
            _tmCategoryService = tmCategoryService;
            _baoGiaNccCategoryService = baoGiaNccCategoryService;
            _env = env;
            _sendMailService = sendMailService;
            _exchangeRateService = exchangeRateService;
            _serviceScopeFactory = serviceScopeFactory;
            _localizer = localizer;
            _deparmentService = deparmentService;
            _tmNccNewService = tmNccNewService;
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

        // MARK: Màn hình Input Quote - Tìm kiếm báo giá theo các tiêu chí
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
                                NVCHR_ReasonPick = "Refuse"
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
                                if ( qty2 == null) errors.Add("Cột 14 không phải số hợp lệ");
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
                                DTM_EffectiveDate = ParseDate(ws.Cell(r,25).GetString()),
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
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                UserRequest = ws.Cell(row, 32).GetString()
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
                NVCHR_DonVi = material.Unit ?? rowData.DonVi,
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
                ID_Status = "CREATE"
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
                ID_Status = "CREATE"
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

                if (string.IsNullOrEmpty(dto.NVCHR_NhaSanXuat))
                    dto.NVCHR_NhaSanXuat = supplier.NVCHR_SanXuat;

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

        // Private helper methods
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
