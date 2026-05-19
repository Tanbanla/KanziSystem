using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [Route("QuoteInputDetail")]
    public class QuoteInputDetailController : BaseAuthController
    {
        private readonly IConfiguration _configuration;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IMaterialService _materialService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<QuoteInputDetailController> _logger;

        public QuoteInputDetailController(
            IConfiguration configuration,
            IBaoGiaService baoGiaService,
            IMaterialService materialService,
            ITmNccNewService tmNccNewService,
            ITmCategoryService tmCategoryService,
            IBaoGiaDetailService baoGiaDetailService,
            IExchangeRateService exchangeRateService,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<QuoteInputDetailController> logger)
        {
            _configuration = configuration;
            _baoGiaService = baoGiaService;
            _materialService = materialService;
            _tmNccNewService = tmNccNewService;
            _tmCategoryService = tmCategoryService;
            _baoGiaDetailService = baoGiaDetailService;
            _exchangeRateService = exchangeRateService;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        [HttpGet("InputQuoteDetail")]
        public async Task<IActionResult> InputQuoteDetail(string maDon)
        {
            var request = await _baoGiaService.GetByMaBaoGiaAsync(maDon);
            if (!request.Success || request.Data == null || !request.Data.Any())
            {
                return NotFound("Request not found");
            }

            await _materialService.SearchAsync("", "", "", 1, 500);
            await LoadNhaCungCapDataAsync();
            await LoadCategoryDataAsync();

            var listMaterial = request.Data.Select(d => d.CHR_MaHangNoiBo)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var listCategory = request.Data.Select(d => d.NVCHR_ChungLoai)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var listNcc = request.Data
                .Where(d => !string.IsNullOrWhiteSpace(d.CHR_MaNCC) || !string.IsNullOrWhiteSpace(d.NVCHR_TenNCC))
                .GroupBy(d => d.CHR_MaNCC ?? d.NVCHR_TenNCC)
                .Select(g => new { MaNcc = g.Key, Ten = g.First().NVCHR_TenNCC })
                .ToList<dynamic>();

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            var vm = new QuoteModel
            {
                listCategory = listCategory,
                listNcc = listNcc,
                listMaterial = listMaterial,
                NguoiThaoTac = GetCurrentUserId() ?? "",
                MaDonHienTai = maDon,
                CurrentRequest = request.Data
            };
            return View("~/Views/Quote/InputQuoteDetail/InputQuoteDetail.cshtml", vm);
        }

        [HttpPost("UpdateQuoteDetail")]
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

        [HttpPost("ImportExcelInputQuote")]
        public async Task<IActionResult> ImportExcelInputQuote([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file được tải lên");
                }

                var exchangRateAsyenc = await _exchangeRateService.GetExchangeRate();
                if (exchangRateAsyenc == null || !exchangRateAsyenc.Success)
                {
                    return BadRequest("Không thể lấy tỷ giá tiền tệ");
                }
                var exchangeRate = exchangRateAsyenc.Data;

                var items = new List<BaoGia_Detail_of_QuotationDTO>();
                var hasErrors = false;
                XLWorkbook workbook = null;
                var fileBytes = new byte[file.Length];
                using (var stream = file.OpenReadStream())
                {
                    await stream.ReadAsync(fileBytes, 0, (int)file.Length);
                }
                using (var memoryStream = new MemoryStream(fileBytes))
                {
                    workbook = new XLWorkbook(memoryStream);
                    var ws = workbook.Worksheets.FirstOrDefault();
                    if (ws == null)
                    {
                        return BadRequest("Không tìm thấy worksheet trong file");
                    }
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 13;
                    for (int r = 13; r <= lastRow; r++)
                    {
                        if (string.IsNullOrWhiteSpace(ws.Cell(r, 1).GetString()))
                        {
                            break;
                        }
                        var errors = new List<string>();
                        if (ws.Cell(r, 16).GetString().Contains("Refuse"))
                        {
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
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 22).GetString())) errors.Add("Cột 22 (VCHR_CamKet) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 23).GetString())) errors.Add("Cột 23 (Delivery Term) bắt buộc");
                            if (string.IsNullOrWhiteSpace(ws.Cell(r, 24).GetString())) errors.Add("Cột 24 (Payment Term) bắt buộc");

                            var qty1 = ParseInt(ws.Cell(r, 6).GetString());
                            var qty2 = ParseInt(ws.Cell(r, 14).GetString());
                            if (qty1 == null || qty2 == null)
                            {
                                if (qty1 == null) errors.Add("Cột 6 không phải số hợp lệ");
                                if (qty2 == null) errors.Add("Cột 14 không phải số hợp lệ");
                            }

                            var ship = ParseDate(ws.Cell(r, 21).GetString());
                            var reqDate = ParseDate(ws.Cell(r, 28).GetString());
                            if (ship == null) errors.Add("Cột 21 (DTM_ShipTime) không phải ngày hợp lệ");
                            if (reqDate == null) errors.Add("Cột 28 (DTM_NgayMuonNhan yêu cầu) không phải ngày hợp lệ");

                            var moq = ParseInt(ws.Cell(r, 18).GetString());
                            if (moq != null && qty1 != null && moq > qty1) errors.Add("MOQ (cột 18) lớn hơn Số lượng (cột 14)");

                            if (errors.Any())
                            {
                                ws.Cell(r, 33).SetValue(string.Join("; ", errors));
                                ws.Row(r).Style.Fill.BackgroundColor = XLColor.Yellow;
                                hasErrors = true;
                                continue;
                            }
                            var idRequestQuote = 0;
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
                            double costUSD = ParseDouble(ws.Cell(r, 16).GetString()) ?? 0;
                            double costVND = ParseDouble(ws.Cell(r, 17).GetString()) ?? 0;

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
                                NVCHR_File = ws.Cell(r, 30).GetString(),
                            };

                            items.Add(dto);
                        }
                    }

                    if (hasErrors)
                    {
                        using var outStream = new MemoryStream();
                        workbook.SaveAs(outStream);
                        var bytes = outStream.ToArray();
                        var fileName = $"ImportErrors_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(bytes, contentType, fileName);
                    }
                }

                var result = await _baoGiaDetailService.UpdateListThongTinNhapBaoGiaAsync(items);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
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

        private async Task<List<string>> LoadCategoryDataAsync()
        {
            var categoryS = await _tmCategoryService.GetListCategory();
            return categoryS.Data ?? new List<string>();
        }

        private async Task<List<IM_NCC_NEWDTO>> LoadNhaCungCapDataAsync()
        {
            var nccNews = await _tmNccNewService.GetAllNccNew();
            return nccNews.Data ?? new List<IM_NCC_NEWDTO>();
        }
    }
}
