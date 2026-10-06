using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;
using PRJ_WAREHOUSE_BIVN.View_Models.QuoteResult;
using System.Globalization;
using Path = System.IO.Path;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{

    public class QuoteResultsController : BaseAuthController
    {

        private readonly ILogger<QuoteResultsController> _logger;
        private readonly IBaoGiaHistoryService _baoGiaHistoryService;
        private readonly IWebHostEnvironment _env;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaStatusService _baoGiaStatusService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IMaterialService _materialService;
        private readonly IConfiguration _configuration;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
        private readonly ITmPriceMasterService _tmPriceMasterService;
        private readonly IFileImportService _fileImportService;

        private readonly ITmCategoryService _tmCategoryService;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IStringLocalizer<QuoteResultsController> _localizer;

        public QuoteResultsController(IWebHostEnvironment env, IBaoGiaHistoryService baoGiaHistoryService, IBaoGiaService baoGiaService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaStepService baoGiaStepService, ILogger<QuoteResultsController> logger, IServiceScopeFactory serviceScopeFactory,
            IMasterApproverSendMailService approverService, IMaterialService materialService, IConfiguration configuration, IBaoGiaDetailService baoGiaDetailService
            , ITmCategoryService tmCategoryService, IDepartmentService deparmentService, ITmNccNewService tmNccNewService, IStringLocalizer<QuoteResultsController> localizer,
            ITmPriceMasterService tmPriceMasterService, IFileImportService fileImportService
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
            _baoGiaDetailService = baoGiaDetailService;
            _tmPriceMasterService = tmPriceMasterService;
            _fileImportService = fileImportService;
        }
        // Search Infor table tab supplierQuoteBody
        [HttpPost]
        public async Task<IActionResult> SearchSupplierQuoteBody([FromBody] SearchQuotationResultsModel search)
        {
            var result = await _baoGiaService.GetThongTinBaoGiaChiTietAsync(
                search,
                GetCurrentUserId() ?? "",
                GetRolesUser() ?? "");

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);

        }
        [HttpPost]
        public async Task<IActionResult> GetLatestSelectedPrices([FromBody] List<string> maHangNoiBo)
        {
            var result = await _baoGiaDetailService.GetLatestSelectedPricesAsync(maHangNoiBo ?? new List<string>());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
        // Search Infor table tab Master Quote Info
        [HttpPost]
        public async Task<IActionResult> SearchMasterQuoteInfo([FromBody] SearchQuoteResultViewModel search)
        {
            // Search Infor table tab Master Quote Info
            var result = await _baoGiaDetailService.SearchMasterQuoteInfoAsync(search);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        // Count In table tab Master Quote Info
        [HttpPost]
        public async Task<IActionResult> CountMasterQuoteInfo([FromBody] SearchQuoteResultViewModel search)
        {
            // Count In table tab Master Quote Info
            var result = await _baoGiaDetailService.CountMasterQuoteInfoAsync(search);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExportMasterQuote([FromBody] SearchQuoteResultViewModel search)
        {
            search ??= new SearchQuoteResultViewModel();
            search.PageIndex = 0;
            search.PageSize = 0;

            var result = await _baoGiaDetailService.SearchMasterQuoteInfoAsync(search);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var root = _env.WebRootPath ?? _env.ContentRootPath;
            var templatePath = Path.Combine(root, "template", "MasterGia.xlsx");
            if (!System.IO.File.Exists(templatePath))
            {
                return BadRequest("Không tìm thấy file mẫu MasterGia.xlsx.");
            }

            using var templateStream = System.IO.File.OpenRead(templatePath);
            using var workbook = new XLWorkbook(templateStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return BadRequest("Không tìm thấy worksheet trong file mẫu MasterGia.xlsx.");
            }

            const int firstDataRow = 3;
            var rows = result.Data ?? new List<dynamic>();
            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var excelRow = firstDataRow + index;
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 1), GetDynamicValue(row, "UploadDate"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 2), GetDynamicValue(row, "PICUpload"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 3), GetDynamicValue(row, "QuotationRequestNumber"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 4), GetDynamicValue(row, "EquipmentCode"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 5), GetDynamicValue(row, "VendorCode"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 6), GetDynamicValue(row, "VendorName"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 7), GetDynamicValue(row, "BIVNPartCode"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 8), GetDynamicValue(row, "VendorGoodCode"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 9), GetDynamicValue(row, "PartNameVN"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 10), GetDynamicValue(row, "PartNameEN"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 11), GetDynamicValue(row, "Quantity"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 12), GetDynamicValue(row, "Unit"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 13), GetDynamicValue(row, "OtherRequirement"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 14), GetDynamicValue(row, "MakerOrigin"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 15), GetDynamicValue(row, "UnitPriceSupplier"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 16), GetDynamicValue(row, "Currency"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 17), GetDynamicValue(row, "UnitPriceUSD"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 18), GetDynamicValue(row, "LeadTime"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 19), GetDynamicValue(row, "MOQ"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 20), GetDynamicValue(row, "Remark"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 21), GetDynamicValue(row, "DeliveryTerm"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 22), GetDynamicValue(row, "Place"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 23), GetDynamicValue(row, "ShipmentMethod"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 24), GetDynamicValue(row, "VatPercent"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 25), GetDynamicValue(row, "PaymentTerm"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 26), GetDynamicValue(row, "PriceEffectiveDate"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 27), GetDynamicValue(row, "ExpiryDate"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 28), GetDynamicValue(row, "FixVendor") is true ? "O" : "X");
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 29), GetDynamicValue(row, "AdjustmentReason"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 30), GetDynamicValue(row, "QuotationDetail"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 31), GetDynamicValue(row, "QtnLink"));
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 32), GetDynamicValue(row, "QtnExcelLink"));
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MasterGia_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        private static object? GetDynamicValue(object row, string name)
        {
            if (row is IDictionary<string, object> values && values.TryGetValue(name, out var value))
            {
                return value;
            }

            return row.GetType().GetProperty(name)?.GetValue(row);
        }

        [HttpPost]
        public async Task<IActionResult> GetPriceMasterHistory([FromBody] string internalPartCode)
        {
            if (string.IsNullOrWhiteSpace(internalPartCode))
            {
                return BadRequest("Mã hàng nội bộ không được để trống.");
            }

            var result = await _tmPriceMasterService.GetByInternalPartCode(internalPartCode.Trim());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExportPriceMasterHistory([FromBody] PriceMasterHistoryExportRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.InternalPartCode))
            {
                return BadRequest("Mã hàng nội bộ không được để trống.");
            }

            var result = await _tmPriceMasterService.GetByInternalPartCode(request.InternalPartCode.Trim());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var rows = result.Data ?? new List<TM_PRICE_MASTERDTO>();
            if (!string.IsNullOrWhiteSpace(request.VendorCode))
            {
                rows = rows.Where(x => (x.CHR_VENDOR_CODE ?? string.Empty).Contains(request.VendorCode.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.QuotationRequestNo))
            {
                rows = rows.Where(x => (x.CHR_QUOTATION_REQUEST_NO ?? string.Empty).Contains(request.QuotationRequestNo.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (request.From.HasValue)
            {
                rows = rows.Where(x => x.DTM_UPLOAD.Date >= request.From.Value.Date).ToList();
            }
            if (request.To.HasValue)
            {
                rows = rows.Where(x => x.DTM_UPLOAD.Date <= request.To.Value.Date).ToList();
            }

            var root = _env.WebRootPath ?? _env.ContentRootPath;
            var templatePath = Path.Combine(root, "template", "MasterGia.xlsx");
            if (!System.IO.File.Exists(templatePath))
            {
                return BadRequest("Không tìm thấy file mẫu MasterGia.xlsx.");
            }

            using var templateStream = System.IO.File.OpenRead(templatePath);
            using var workbook = new XLWorkbook(templateStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return BadRequest("Không tìm thấy worksheet trong file mẫu MasterGia.xlsx.");
            }

            const int firstDataRow = 3;
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var excelRow = firstDataRow + rowIndex;
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 1), row.DTM_UPLOAD);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 2), row.CHR_UPLOAD_USERID);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 3), row.CHR_QUOTATION_REQUEST_NO);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 4), row.CHR_EQUIPMENT_CODE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 5), row.CHR_VENDOR_CODE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 6), row.NVCHR_VENDOR_NAME);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 7), row.CHR_INTERNAL_PART_CODE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 8), row.CHR_VENDOR_PART_CODE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 9), row.NVCHR_PART_NAME_VN);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 10), row.NVCHR_PART_NAME_EN);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 11), row.DEC_QUANTITY);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 12), row.NVCHR_UNIT);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 13), row.NVCHR_OTHER_REQUIREMENT);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 14), row.NVCHR_MAKER_ORIGIN);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 15), row.DEC_UNIT_PRICE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 16), row.CHR_CURRENCY);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 17), row.DEC_UNIT_PRICE_USD);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 18), row.INT_LEAD_TIME_DAY);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 19), row.DEC_MOQ);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 20), row.NVCHR_REMARK);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 21), row.NVCHR_DELIVERY_TERM);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 22), row.NVCHR_PLACE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 23), row.NVCHR_SHIPMENT_METHOD);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 24), row.DEC_VAT_PERCENT);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 25), row.NVCHR_PAYMENT_TERM);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 26), row.DTM_PRICE_EFFECTIVE);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 27), row.DTM_PRICE_EXPIRATION);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 28), row.BIT_FIX_VENDOR ? "O" : "X");
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 29), row.NVCHR_ADJUSTMENT_REASON);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 30), row.INT_QUOTATION_DETAIL);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 31), row.NVCHR_QTN_LINK);
                SetPriceMasterExportCell(worksheet.Cell(excelRow, 32), row.NVCHR_QTN_EXCEL_LINK);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"MasterGia_{request.InternalPartCode.Trim()}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private static void SetPriceMasterExportCell(IXLCell cell, object? value)
        {
            if (value is DateTime dateValue)
            {
                cell.Value = dateValue;
                cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
            }
            else if (value is decimal decimalValue)
            {
                cell.Value = decimalValue;
            }
            else if (value is int intValue)
            {
                cell.Value = intValue;
            }
            else
            {
                cell.Value = value?.ToString() ?? string.Empty;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadPriceMasterFile([FromForm] IFormFile file, [FromForm] string kind = "quotation")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn file cần tải lên.");
            }

            var result = await _fileImportService.SaveQuotationFileAsync(file, kind);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ImportPriceMaster([FromForm] IFormFile file)
        {
            const int startRow = 3;
            const int columnCount = 32;
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel cần nhập.");
            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Chỉ hỗ trợ file Excel định dạng .xlsx.");

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return BadRequest("Không tìm thấy worksheet trong file.");

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? startRow - 1;
                if (lastRow < startRow)
                    return BadRequest("File không có dữ liệu từ dòng 3.");

                var validRows = new List<TM_PRICE_MASTER>();
                var parsedRows = new List<(int ExcelRow, string[] Values, TM_PRICE_MASTER Entity)>();
                var invalidRows = new List<PriceMasterImportRow>();
                var duplicateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (var rowNumber = startRow; rowNumber <= lastRow; rowNumber++)
                {
                    var values = Enumerable.Range(1, columnCount)
                        .Select(column => worksheet.Cell(rowNumber, column).GetFormattedString().Trim())
                        .ToArray();
                    if (values.All(string.IsNullOrWhiteSpace)) continue;

                    var errors = new List<string>();
                    var entity = ParsePriceMasterRow(values, rowNumber, GetCurrentUserId(), errors);
                    if (entity != null)
                    {
                        // Tạm thời bỏ qua trùng
                        //var key = $"{entity.CHR_QUOTATION_REQUEST_NO}|{entity.CHR_INTERNAL_PART_CODE}|{entity.CHR_VENDOR_CODE}|{entity.DEC_MOQ}";
                        //if (!duplicateKeys.Add(key))
                        //    errors.Add("Trùng mã đơn, mã hàng nội bộ, mã nhà cung cấp và MOQ trong file.");
                    }

                    if (errors.Count > 0 || entity == null)
                    {
                        invalidRows.Add(new PriceMasterImportRow
                        {
                            ExcelRow = rowNumber,
                            Values = values,
                            Errors = string.Join("; ", errors)
                        });
                    }
                    else
                    {
                        validRows.Add(entity);
                        parsedRows.Add((rowNumber, values, entity));
                    }
                }

                if (invalidRows.Count > 0)
                {
                    Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    Response.Headers.Append("X-Import-Validation-Errors", invalidRows.Count.ToString(CultureInfo.InvariantCulture));
                    return BuildPriceMasterImportErrorFile(invalidRows, contentType);
                }

                invalidRows.AddRange(await SaveImportedPriceMasterFilesAsync(parsedRows));
                if (invalidRows.Count > 0)
                {
                    Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    Response.Headers.Append("X-Import-Validation-Errors", invalidRows.Count.ToString(CultureInfo.InvariantCulture));
                    return BuildPriceMasterImportErrorFile(invalidRows, contentType);
                }

                var insertResult = await _tmPriceMasterService.InsertImportedAsync(validRows);
                if (!insertResult.Success)
                    return BadRequest($"Không thể lưu dữ liệu master giá: {insertResult.Message}");

                return Ok(new { message = "Import master giá thành công.", totalRows = validRows.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import master giá từ Excel");
                return BadRequest($"Lỗi đọc file Excel: {ex.Message}");
            }
        }

        private static TM_PRICE_MASTER? ParsePriceMasterRow(string[] values, int rowNumber, string? currentUserId, List<string> errors)
        {
            string Text(int index) => values[index - 1];
            string? OptionalText(int index) => string.IsNullOrWhiteSpace(Text(index)) ? null : Text(index);

            decimal? DecimalValue(int index, string name)
            {
                var value = Text(index);
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ||
                    decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out result)) return result;
                errors.Add($"Cột {name} không phải số hợp lệ.");
                return null;
            }

            int? IntValue(int index, string name)
            {
                var value = Text(index);
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ||
                    int.TryParse(value, NumberStyles.Integer, CultureInfo.GetCultureInfo("vi-VN"), out result)) return result;
                errors.Add($"Cột {name} không phải số nguyên hợp lệ.");
                return null;
            }

            DateTime? DateValue(int index, string name)
            {
                var value = Text(index);
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (DateTime.TryParse(value, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.AllowWhiteSpaces, out var result) ||
                    DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result)) return result;
                errors.Add($"Cột {name} không phải ngày hợp lệ.");
                return null;
            }

            var uploadDate = DateValue(1, "Ngày upload") ?? DateTime.Now;
            var vendorCode = Text(5);
            var vendorName = Text(6);
            var internalCode = Text(7);
            if (string.IsNullOrWhiteSpace(internalCode)) errors.Add("Mã hàng nội bộ không được để trống.");
            if (string.IsNullOrWhiteSpace(vendorCode)) errors.Add("Mã nhà cung cấp không được để trống.");
            if (string.IsNullOrWhiteSpace(vendorName)) errors.Add("Tên nhà cung cấp không được để trống.");

            var effectiveDate = DateValue(26, "Ngày hiệu lực");
            var expirationDate = DateValue(27, "Ngày hết hạn");
            if (effectiveDate.HasValue && expirationDate.HasValue && expirationDate < effectiveDate)
                errors.Add("Ngày hết hạn phải lớn hơn hoặc bằng ngày hiệu lực.");

            var fixedVendor = Text(28);
            var isFixedVendor = string.IsNullOrWhiteSpace(fixedVendor) ||
                fixedVendor.Equals("O", StringComparison.OrdinalIgnoreCase) ||
                fixedVendor.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                fixedVendor.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
                fixedVendor.Equals("Có", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(fixedVendor) && !isFixedVendor &&
                !fixedVendor.Equals("X", StringComparison.OrdinalIgnoreCase) &&
                !fixedVendor.Equals("N", StringComparison.OrdinalIgnoreCase) &&
                !fixedVendor.Equals("FALSE", StringComparison.OrdinalIgnoreCase) &&
                !fixedVendor.Equals("Không", StringComparison.OrdinalIgnoreCase))
                errors.Add("Quyết định NCC chỉ nhận O/X, Có/Không hoặc TRUE/FALSE.");

            var quantity = DecimalValue(11, "Số lượng");
            var unitPrice = DecimalValue(15, "Đơn giá nhà cung cấp");
            var unitPriceUsd = DecimalValue(17, "Đơn giá USD");
            var leadTime = IntValue(18, "Thời gian giao hàng");
            var moq = DecimalValue(19, "MOQ");
            var vat = DecimalValue(24, "VAT");
            var quotationDetail = IntValue(30, "Số chi tiết đơn yêu cầu báo giá");
            if (errors.Count > 0) return null;

            return new TM_PRICE_MASTER
            {
                DTM_UPLOAD = uploadDate,
                CHR_UPLOAD_USERID = string.IsNullOrWhiteSpace(Text(2)) ? currentUserId : Text(2),
                CHR_QUOTATION_REQUEST_NO = OptionalText(3),
                CHR_EQUIPMENT_CODE = OptionalText(4),
                CHR_VENDOR_CODE = vendorCode,
                NVCHR_VENDOR_NAME = vendorName,
                CHR_INTERNAL_PART_CODE = internalCode,
                CHR_VENDOR_PART_CODE = OptionalText(8),
                NVCHR_PART_NAME_VN = OptionalText(9),
                NVCHR_PART_NAME_EN = OptionalText(10),
                DEC_QUANTITY = quantity,
                NVCHR_UNIT = OptionalText(12),
                NVCHR_OTHER_REQUIREMENT = OptionalText(13),
                NVCHR_MAKER_ORIGIN = OptionalText(14),
                DEC_UNIT_PRICE = unitPrice,
                CHR_CURRENCY = OptionalText(16),
                DEC_UNIT_PRICE_USD = unitPriceUsd,
                INT_LEAD_TIME_DAY = leadTime,
                DEC_MOQ = moq,
                NVCHR_REMARK = OptionalText(20),
                NVCHR_DELIVERY_TERM = OptionalText(21),
                NVCHR_PLACE = OptionalText(22),
                NVCHR_SHIPMENT_METHOD = OptionalText(23),
                DEC_VAT_PERCENT = vat,
                NVCHR_PAYMENT_TERM = OptionalText(25),
                DTM_PRICE_EFFECTIVE = effectiveDate,
                DTM_PRICE_EXPIRATION = expirationDate,
                BIT_FIX_VENDOR = isFixedVendor,
                NVCHR_ADJUSTMENT_REASON = OptionalText(29),
                INT_QUOTATION_DETAIL = quotationDetail,
                NVCHR_QTN_LINK = OptionalText(31),
                NVCHR_QTN_EXCEL_LINK = OptionalText(32),
                CHR_CRT_USERID = currentUserId,
                DTM_CREATE = DateTime.Now
            };
        }

        private static FileContentResult BuildPriceMasterImportErrorFile(IReadOnlyCollection<PriceMasterImportRow> rows, string contentType)
        {
            var headers = new[]
            {
                "Excel row", "Ngày upload", "PIC upload", "Số đơn yêu cầu báo giá", "Mã thiết bị", "Vendor code", "Vendor name",
                "Mã hàng nội bộ", "Mã hàng NCC", "Tên hàng tiếng Việt", "Tên hàng tiếng Anh", "Số lượng", "Đơn vị", "Yêu cầu khác",
                "Nhà sản xuất/Xuất xứ", "Đơn giá nhà cung cấp", "Đơn vị tiền", "Đơn giá USD", "Thời gian giao hàng", "MOQ", "Lý do tăng giá",
                "Điều kiện giao hàng", "Nơi giao hàng", "Phương thức giao hàng", "VAT (%)", "Phương thức thanh toán", "Ngày hiệu lực",
                "Ngày hết hạn", "Fix Vendor", "Lý do điều chỉnh", "Số chi tiết đơn yêu cầu báo giá", "Link QTN", "Link QTN Excel", "Lỗi chi tiết"
            };
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Du lieu loi");
            for (var column = 0; column < headers.Length; column++)
            {
                worksheet.Cell(1, column + 1).Value = headers[column];
                worksheet.Cell(1, column + 1).Style.Font.Bold = true;
                worksheet.Cell(1, column + 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            }

            var outputRow = 2;
            foreach (var row in rows)
            {
                worksheet.Cell(outputRow, 1).Value = row.ExcelRow;
                for (var column = 0; column < row.Values.Length; column++)
                    worksheet.Cell(outputRow, column + 2).Value = row.Values[column];
                worksheet.Cell(outputRow, headers.Length).Value = row.Errors;
                outputRow++;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return new FileContentResult(stream.ToArray(), contentType)
            {
                FileDownloadName = $"ImportMasterGia_Errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
        }

        private async Task<List<PriceMasterImportRow>> SaveImportedPriceMasterFilesAsync(
            IReadOnlyCollection<(int ExcelRow, string[] Values, TM_PRICE_MASTER Entity)> rows)
        {
            var errors = new List<PriceMasterImportRow>();
            var savedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                await SaveFileAsync(row.Entity.NVCHR_QTN_LINK, value => row.Entity.NVCHR_QTN_LINK = value);
                await SaveFileAsync(row.Entity.NVCHR_QTN_EXCEL_LINK, value => row.Entity.NVCHR_QTN_EXCEL_LINK = value);
            }

            return errors;

            async Task SaveFileAsync(string? sourcePath, Action<string> setValue)
            {
                if (string.IsNullOrWhiteSpace(sourcePath)) return;

                var normalizedPath = sourcePath.Trim().Trim('"', '\'');
                if (!savedFiles.TryGetValue(normalizedPath, out var savedPath))
                {
                    var saveResult = await _fileImportService.SaveFileFromPathAsync(normalizedPath, isDelete: false);
                    if (saveResult == null || !saveResult.Success || string.IsNullOrWhiteSpace(saveResult.Data))
                    {
                        return;
                    }

                    savedPath = saveResult.Data;
                    savedFiles[normalizedPath] = savedPath;
                }

                setValue(savedPath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPriceMaster([FromBody] TM_PRICE_MASTERDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CHR_INTERNAL_PART_CODE) || string.IsNullOrWhiteSpace(dto.CHR_VENDOR_CODE))
            {
                return BadRequest("Mã hàng nội bộ và mã nhà cung cấp không được để trống.");
            }

            var now = DateTime.Now;
            var entity = new TM_PRICE_MASTER
            {
                DTM_UPLOAD = now,
                CHR_UPLOAD_USERID = GetCurrentUserId(),
                CHR_QUOTATION_REQUEST_NO = dto.CHR_QUOTATION_REQUEST_NO,
                INT_QUOTATION_DETAIL = dto.INT_QUOTATION_DETAIL,
                CHR_EQUIPMENT_CODE = dto.CHR_EQUIPMENT_CODE,
                CHR_INTERNAL_PART_CODE = dto.CHR_INTERNAL_PART_CODE.Trim(),
                CHR_VENDOR_PART_CODE = dto.CHR_VENDOR_PART_CODE,
                NVCHR_PART_NAME_VN = dto.NVCHR_PART_NAME_VN,
                NVCHR_PART_NAME_EN = dto.NVCHR_PART_NAME_EN,
                NVCHR_UNIT = dto.NVCHR_UNIT,
                NVCHR_OTHER_REQUIREMENT = dto.NVCHR_OTHER_REQUIREMENT,
                NVCHR_MAKER_ORIGIN = dto.NVCHR_MAKER_ORIGIN,
                DEC_QUANTITY = dto.DEC_QUANTITY,
                CHR_VENDOR_CODE = dto.CHR_VENDOR_CODE.Trim(),
                NVCHR_VENDOR_NAME = dto.NVCHR_VENDOR_NAME ?? string.Empty,
                DEC_UNIT_PRICE = dto.DEC_UNIT_PRICE,
                CHR_CURRENCY = dto.CHR_CURRENCY,
                DEC_UNIT_PRICE_USD = dto.DEC_UNIT_PRICE_USD,
                INT_LEAD_TIME_DAY = dto.INT_LEAD_TIME_DAY,
                DEC_MOQ = dto.DEC_MOQ,
                NVCHR_REMARK = dto.NVCHR_REMARK,
                NVCHR_DELIVERY_TERM = dto.NVCHR_DELIVERY_TERM,
                NVCHR_PLACE = dto.NVCHR_PLACE,
                NVCHR_SHIPMENT_METHOD = dto.NVCHR_SHIPMENT_METHOD,
                DEC_VAT_PERCENT = dto.DEC_VAT_PERCENT,
                NVCHR_PAYMENT_TERM = dto.NVCHR_PAYMENT_TERM,
                DTM_PRICE_EFFECTIVE = dto.DTM_PRICE_EFFECTIVE,
                DTM_PRICE_EXPIRATION = dto.DTM_PRICE_EXPIRATION,
                BIT_FIX_VENDOR = dto.BIT_FIX_VENDOR,
                NVCHR_ADJUSTMENT_REASON = dto.NVCHR_ADJUSTMENT_REASON,
                NVCHR_QTN_LINK = dto.NVCHR_QTN_LINK,
                NVCHR_QTN_EXCEL_LINK = dto.NVCHR_QTN_EXCEL_LINK,
                CHR_CRT_USERID = GetCurrentUserId(),
                DTM_CREATE = now
            };

            var result = await _tmPriceMasterService.AddAsyncV2(entity);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
        // thông tin hàng hóa
        [HttpPost]
        public async Task<IActionResult> GetMaterialInfo([FromBody] string internalPartCode)
        {
            if (string.IsNullOrWhiteSpace(internalPartCode))
            {
                return BadRequest("Mã hàng nội bộ không được để trống.");
            }
            var result = await _materialService.GetByMaHangAsync(internalPartCode.Trim());
            if (result == null)
            {
                return NotFound($"Không tìm thấy thông tin hàng hóa với mã: {internalPartCode}");
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePriceMaster([FromBody] TM_PRICE_MASTERDTO dto)
        {
            if (dto == null || dto.ID <= 0 || string.IsNullOrWhiteSpace(dto.CHR_INTERNAL_PART_CODE))
            {
                return BadRequest("Dữ liệu master giá không hợp lệ.");
            }

            var entity = new TM_PRICE_MASTER
            {
                ID = dto.ID,
                DTM_UPLOAD = dto.DTM_UPLOAD,
                CHR_UPLOAD_USERID = dto.CHR_UPLOAD_USERID,
                CHR_QUOTATION_REQUEST_NO = dto.CHR_QUOTATION_REQUEST_NO,
                INT_QUOTATION_DETAIL = dto.INT_QUOTATION_DETAIL,
                CHR_EQUIPMENT_CODE = dto.CHR_EQUIPMENT_CODE,
                CHR_INTERNAL_PART_CODE = dto.CHR_INTERNAL_PART_CODE,
                CHR_VENDOR_PART_CODE = dto.CHR_VENDOR_PART_CODE,
                NVCHR_PART_NAME_VN = dto.NVCHR_PART_NAME_VN,
                NVCHR_PART_NAME_EN = dto.NVCHR_PART_NAME_EN,
                NVCHR_UNIT = dto.NVCHR_UNIT,
                NVCHR_OTHER_REQUIREMENT = dto.NVCHR_OTHER_REQUIREMENT,
                NVCHR_MAKER_ORIGIN = dto.NVCHR_MAKER_ORIGIN,
                DEC_QUANTITY = dto.DEC_QUANTITY,
                CHR_VENDOR_CODE = dto.CHR_VENDOR_CODE,
                NVCHR_VENDOR_NAME = dto.NVCHR_VENDOR_NAME,
                DEC_UNIT_PRICE = dto.DEC_UNIT_PRICE,
                CHR_CURRENCY = dto.CHR_CURRENCY,
                DEC_UNIT_PRICE_USD = dto.DEC_UNIT_PRICE_USD,
                INT_LEAD_TIME_DAY = dto.INT_LEAD_TIME_DAY,
                DEC_MOQ = dto.DEC_MOQ,
                NVCHR_REMARK = dto.NVCHR_REMARK,
                NVCHR_DELIVERY_TERM = dto.NVCHR_DELIVERY_TERM,
                NVCHR_PLACE = dto.NVCHR_PLACE,
                NVCHR_SHIPMENT_METHOD = dto.NVCHR_SHIPMENT_METHOD,
                DEC_VAT_PERCENT = dto.DEC_VAT_PERCENT,
                NVCHR_PAYMENT_TERM = dto.NVCHR_PAYMENT_TERM,
                DTM_PRICE_EFFECTIVE = dto.DTM_PRICE_EFFECTIVE,
                DTM_PRICE_EXPIRATION = dto.DTM_PRICE_EXPIRATION,
                BIT_FIX_VENDOR = dto.BIT_FIX_VENDOR,
                NVCHR_ADJUSTMENT_REASON = dto.NVCHR_ADJUSTMENT_REASON,
                NVCHR_QTN_LINK = dto.NVCHR_QTN_LINK,
                NVCHR_QTN_EXCEL_LINK = dto.NVCHR_QTN_EXCEL_LINK,
               // BIT_USE_LEAVE_RATE = dto.BIT_USE_LEAVE_RATE,
                CHR_CRT_USERID = dto.CHR_CRT_USERID,
                DTM_CREATE = dto.DTM_CREATE,
                CHR_UPD_USERID = GetCurrentUserId(),
                DTM_UPDATE = DateTime.Now
            };

            var result = await _tmPriceMasterService.UpdateAsync(entity);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        // Save pick supplier
        [HttpPost]
        public async Task<IActionResult> SavePickSupplier([FromBody] SaveQuotationResultsModel vm)
        {
            var check = vm.listPick.Where(c => (c.BIT_Select == true || c.BIT_Select == false) && c.NVCHR_ReasonPick == "").ToList();
            if (check.Any())
            {
                return BadRequest(_localizer["ReasonRequiredForItems"].Value);
            }
            var allRowsData = new List<BaoGiaImportModel>();
            var currentUserId = GetCurrentUserId();
            foreach (var item in vm.listPick)
            {
                allRowsData.Add(new BaoGiaImportModel
                {
                    ID = item.ID,
                    NVCHR_ReasonPick = item.NVCHR_ReasonPick,
                    MaDon = item.NVCHR_NameNCC,
                    MaHangNoiBo = item.CHR_MaHangNCC
                });
            }
            // Kiểm tra đơn hàng + mã hàng nội bộ đều đã báo giá hết chưa, nếu chưa thì báo lỗi
            var requestCheck = await _baoGiaService.CheckPermissionSelectSupplierAsync(allRowsData);

            if (requestCheck.Data.Count > 0 && requestCheck.Success)
            {
                var errorMessage = _localizer["CannotSelectSupplierForItems", string.Join(", ", requestCheck.Data.Select(c => $"Vender: {c.MaDon} - Bivn Code: {c.MaHangNoiBo}").Distinct())].Value;
                return BadRequest(errorMessage);
            }

            var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(vm.listPick, vm.UserApproverNext, currentUserId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            var req = result.Data;
            var userSend = vm.UserApproverNext;

            _ = Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                        await sendMailService.SendMailAsync(userSend + "@brothergroup.net", "", 14, "QuoteResults/Quotation_Results", req.CHR_Gap == "false" ? false : true, req.CHR_SectionCode ?? "", req.CHR_MaDon ?? "", currentUserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi mail xác nhận tên mới");
                    }
                }

            });
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> GetListApprovel([FromBody] SearchApprovalModel sr)
        {
            var result = await _approverService.GetApproverByStepAndSectionAsync(sr.Step ?? 2, sr.SectionCost ?? "");
            if (!result.Success)
            {
                return BadRequest(_localizer["ApproverListError", result.Message].Value);
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
        // Xuất dữ liệu để lựa chọn nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> ExportFileExcelQuotationResult([FromBody] SearchQuotationResultsModel search)
        {
            try
            {
                search.PageIndex = 0;
                search.PageSize = 0;
                var result = await _baoGiaService.GetThongTinBaoGiaChiTietAsync(
                    search,
                    GetCurrentUserId() ?? "",
                    GetRolesUser() ?? "");
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if (result.Data == null)
                {
                    return BadRequest(_localizer["NoDataToExport"].Value);
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
                    return BadRequest(_localizer["TemplateNotFound", "TemplateQuotationResults.xlsx"].Value);
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest(_localizer["WorksheetNotFound"].Value);
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
                    ws.Cell(rowStart, col++).SetValue(item.ShortName ?? item.NVCHR_TenNCC ?? string.Empty);
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
        public async Task<IActionResult> ImportApprovalQuotianExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(_localizer["InvalidFile"].Value);

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest(_localizer["WorksheetNotFound"].Value);
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
                    // case chưa chọn kết quả phê duyệt
                    if (resultApproval != "OK" && resultApproval != "NG")
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
                            Errors = _localizer["ValidApprovalResultRequired"].Value
                        });
                        continue;
                    }

                    // case chưa nhập lý do từ chối 
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
                            Errors = _localizer["ApprovalReasonRequiredWhenNG"].Value
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
                        errorWs.Cell(i + 2, 5).Value = row.NVCHR_LyDo;
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
                    return BadRequest(_localizer["NoValidDataReceived"].Value);
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
                        return BadRequest(_localizer["ApproverListError", approverResult.Message].Value);
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
                    return BadRequest(_localizer["ApproverListError", result.Message].Value);
                }
                var approvers = result.Data.FirstOrDefault();
                return await PheDuyetBaoGia(listApproval, approvers?.CHR_UserAdid ?? "khanhmf");
            }
            catch (Exception ex)
            {
                return BadRequest(_localizer["ReadFileError", ex.Message].Value);
            }
        }
        // Xử lý aproval
        public async Task<IActionResult> PheDuyetBaoGia(List<ApproverDTO> listCofirm, string UserApproverNext)
        {
            if (listCofirm == null || !listCofirm.Any())
            {
                return BadRequest(_localizer["NoApprovalData"].Value);
            }
            try
            {
                var currentUserId = GetCurrentUserId();
                // Check user phê duyệt có hợp lệ không
                var checkPermission = await _approverService.CheckUserApprovalPermissionAsync(currentUserId, listCofirm.Select(c => c.Id).ToList());
                if (!checkPermission.Success || !checkPermission.Data)
                {
                    return BadRequest(_localizer["NoApprovalPermission"].Value);
                }

                // Lưu lịch sử phê duyệt
                var baoGia = await _baoGiaService.UpdateApprover(listCofirm, UserApproverNext, currentUserId);
                if (!baoGia.Success)
                {
                    return BadRequest(_localizer["ApprovalError", baoGia.Message].Value);
                }
                var req = baoGia.Data;
                var userSend = UserApproverNext;
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
                                var baoGiaService = scope.ServiceProvider.GetRequiredService<IBaoGiaService>();
                                var nccNewService = scope.ServiceProvider.GetRequiredService<ITmNccNewService>();

                                var checkSendMail = true;

                                var listConfirm = new List<BaoGia_Confirm_Name_QuotationDTO>();
                                var listNotNeedConfirmName = new List<int>();


                                //lấy dữ liệu không cần xác nhận tên hải quan
                                var RepNotConfirmNameAsync = await nccNewService.ListNotConfirmName();
                                var NotConfirmNames = RepNotConfirmNameAsync.Data.ToList();

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


                                        if (NotConfirmNames.Contains(detailsResult.Data.CHR_CodeNCC))
                                        {
                                            listNotNeedConfirmName.Add(material.ID);
                                            continue;
                                        }
                                        // dữ liệu xác nhận tên
                                        var cf = new BaoGia_Confirm_Name_QuotationDTO();
                                        cf.ID_RequestQuote = material.ID;
                                        cf.DTM_CreateDate = DateTime.Now;
                                        cf.VCHR_CreateBy = "System";
                                        cf.VCHR_TenRecomment = detailsResult.Data.NVCHR_TenHangHQ ?? material.NVCHR_NameVN ?? "";
                                        cf.CHR_Status = "Confirmed";
                                        cf.CHR_StatusACC = "Confirmed";
                                        cf.CHR_StatusShip = "Confirming";
                                        cf.NVCHR_Note = material.CHR_MaHangNCC;
                                        listConfirm.Add(cf);
                                        checkSendMail = false;
                                    }
                                }
                                if (string.IsNullOrEmpty(userSend))
                                {
                                    var approverNext = await sendMailService.SendMailToRequesterAsync("", 11);
                                    userSend = approverNext?.Data ?? "";
                                    // update ngươì phê duyệt
                                    if (!string.IsNullOrEmpty(userSend))
                                    {
                                        var item = new UpdateHistoryResult
                                        {
                                            sectionCode = userSend.Replace("@brothergroup.net", "") ?? "",
                                            listUpdate = listOk.Select(c => c.ID).ToList(),
                                            isReturn = false
                                        };
                                        await baoGiaService.UpdateUserApprovalHistory(item);
                                    }
                                }
                                else
                                {
                                    userSend = userSend + "@brothergroup.net";
                                }
                                // gửi mail thông báo phê duyệt thành công
                                if (checkSendMail)
                                {
                                    await sendMailService.SendMailAsync(userSend, "", 14, "QuoteResults/Quotation_Results",
                                    listOk.FirstOrDefault()?.CHR_Gap == "false" ? false : true, listOk.FirstOrDefault()?.CHR_SectionCode ?? "",
                                    listOk.FirstOrDefault()?.CHR_MaDon ?? "", currentUserId);
                                }
                                // Send mail confirm name
                                if (listConfirm.Any())
                                {
                                    // lưu thông tin xác nhận tên
                                    var NeedConfirmName = await baoGiaConfirmNameService.AddListAsync(listConfirm);
                                    if (!NeedConfirmName.Success)
                                    {
                                        _logger.LogError("Lỗi khi kiểm tra đơn hàng đã được xác nhận: " + NeedConfirmName.Message);
                                    }
                                    //gửi mail thông báo có yêu cầu xác nhận tên mới
                                    var emailResult = await sendMailService.SendMailToConfirmItemAsync(13, 17, "Material/ConfirmName", true, "", "", currentUserId);

                                }
                                // sen mail done
                                if (listNotNeedConfirmName.Any())
                                {
                                    // gửi mail thông báo hoàn thành
                                    var listDone = await baoGiaConfirmNameService.DoneConfirmNameAsync(listNotNeedConfirmName);
                                    if (!listDone.Success)
                                    {
                                        _logger.LogError("Lỗi khi kiểm tra đơn hàng đã được xác nhận: " + listDone.Message);
                                        return;
                                    }
                                    foreach (var item in listDone.Data)
                                    {
                                        // gửi mail thông báo đơn đã hoàn thành xác nhận tên hải quan
                                        var emailDone = await sendMailService.SendMailAsync(
                                            item.UserCreate + "@brothergroup.net",
                                            string.Empty,
                                            18,
                                            "SelectQuote/SelectQuoteSection",
                                            true,
                                            item.Section,
                                            item.MaDon,
                                            item.UserCreate
                                        );
                                    }
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
                if (listNG.Any())
                {
                    // danh sach PIC
                    var result = await _approverService.GetApproverByStepAndSectionAsync(4, "3110");
                    if (!result.Success)
                    {
                        return BadRequest(_localizer["CannotGetPICInfo", result.Message].Value);
                    }
                    var dataPic = result.Data;
                    _ = Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
                                string emailList = string.Join("; ", dataPic.Select(x => x.CHR_UserAdid + "@brothergroup.net"));

                                await sendMailService.SendMailAsync(emailList, "", 15, "QuoteResults/Quotation_Results",
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
                return BadRequest(_localizer["ApprovalError", ex.Message].Value);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmImportedApprovals([FromBody] ConfirmImportedApprovalsRequest req)
        {
            if (req == null || req.Items == null || !req.Items.Any())
                return BadRequest(_localizer["NoItemsProvided"].Value);

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
                return await PheDuyetBaoGia(listApproval, approverAdid == string.Empty ? "khanhmf" : approverAdid);
            }
            catch (Exception ex)
            {
                return BadRequest(_localizer["ProcessApprovalsError", ex.Message].Value);
            }
        }
        // Nhập lựa chọn báo giá file excel
        [HttpPost]
        public async Task<IActionResult> ImportQuotianExcel([FromForm] ImportPickSupplier vm)
        {
            if (vm.fileSend == null || vm.fileSend.Length == 0)
                return BadRequest(_localizer["InvalidFile"].Value);

            var items = new List<dynamic>();
            var errorRows = new List<dynamic>();
            try
            {
                using var stream = vm.fileSend.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest(_localizer["WorksheetNotFound"].Value);
                var isErrors = false;

                int startRow = 4;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                var allRowsData = new List<BaoGiaImportModel>();
                var validationRows = new List<(BaoGiaImportModel Row, string Link, DateTime? Expiry)>();

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
                    var quoteLink = ws.Cell(r, 47).GetString()?.Trim() ?? string.Empty;
                    DateTime? expiry = DateTime.TryParse(ws.Cell(r, 49).GetString(), out var expiryValue)
                        ? expiryValue
                        : null;

                    var bitSelect = ws.Cell(r, 51).GetString();
                    var reason = ws.Cell(r, 52).GetString();

                    var reasonRemark = ws.Cell(r, 53).GetString();

                    var rowData = new BaoGiaImportModel
                    {
                        Row = r,
                        MaDon = maDon,
                        ID = int.Parse(id),
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
                    };
                    allRowsData.Add(rowData);
                    validationRows.Add((rowData, quoteLink, expiry));
                }

                var warningRows = validationRows
                    .GroupBy(x => $"{x.Row.MaDon}|{x.Row.MaHangNoiBo}")
                    .SelectMany(group =>
                    {
                        var quoted = group.Any(x => (x.Row.DonGiaUSD ?? 0) > 0 || (x.Row.DonGiaVND ?? 0) > 0);
                        var selected = group.Where(x => (x.Row.BIT_Select ?? string.Empty).Contains("O")).ToList();
                        var result = new List<object>();
                        if (quoted && selected.Count == 0)
                        {
                            result.Add(new { Row = group.First().Row.Row, MaDon = group.First().Row.MaDon, MaHangNoiBo = group.First().Row.MaHangNoiBo, VendorCode = group.First().Row.CodeVender, Reason = "Có NCC báo giá nhưng toàn bộ NCC đang tick X, chưa chọn NCC nào." });
                        }
                        foreach (var item in selected)
                        {
                            if ((item.Row.DonGiaUSD ?? 0) <= 0 && (item.Row.DonGiaVND ?? 0) <= 0)
                                result.Add(new { Row = item.Row.Row, MaDon = item.Row.MaDon, MaHangNoiBo = item.Row.MaHangNoiBo, VendorCode = item.Row.CodeVender, Reason = "Đã chọn NCC nhưng giá báo giá bằng 0." });
                            if (item.Expiry.HasValue && item.Expiry.Value.Date < DateTime.Today)
                                result.Add(new { Row = item.Row.Row, MaDon = item.Row.MaDon, MaHangNoiBo = item.Row.MaHangNoiBo, VendorCode = item.Row.CodeVender, Reason = "Đã chọn NCC nhưng báo giá đã hết hiệu lực." });
                            if (string.IsNullOrWhiteSpace(item.Link))
                                result.Add(new { Row = item.Row.Row, MaDon = item.Row.MaDon, MaHangNoiBo = item.Row.MaHangNoiBo, VendorCode = item.Row.CodeVender, Reason = "Đã chọn NCC nhưng link báo giá đang để trống." });
                        }
                        return result;
                    })
                    .ToList();

                if (warningRows.Any())
                {
                    var confirmationItems = allRowsData.Select(item => new BaoGia_Detail_of_QuotationDTO
                    {
                        ID = item.ID,
                        BIT_Select = (item.BIT_Select ?? string.Empty).Contains("O"),
                        NVCHR_ReasonPick = item.NVCHR_ReasonPick ?? string.Empty,
                        NVCHR_Note = item.NVCHR_Note ?? string.Empty
                    }).ToList();
                    return Ok(new
                    {
                        RequiresWarning = true,
                        Warnings = warningRows,
                        Items = confirmationItems,
                        UserNextApproval = vm.userNextApproval
                    });
                }

                // Kiểm tra đơn hàng + mã hàng nội bộ đều đã báo giá hết chưa, nếu chưa thì báo lỗi
                var requestCheck = await _baoGiaService.CheckPermissionSelectSupplierAsync(allRowsData);

                if (requestCheck.Data.Count > 0 && requestCheck.Success)
                {
                    foreach (var item in requestCheck.Data)
                    {
                        errorRows.Add(new
                        {
                            Row = item.Row,
                            MaDon = item.MaDon,
                            ID = item.ID,
                            MaHangNoiBo = item.MaHangNoiBo,
                            VendorCode = item.CodeVender,
                            BIT_Select = item.BIT_Select,
                            DonGiaUSD = item.DonGiaUSD,
                            NVCHR_ReasonPick = item.NVCHR_ReasonPick,
                            Errors = _localizer["NotAllSuppliersQuoted", item.MaDon, item.MaHangNoiBo].Value
                        });
                    }
                    isErrors = true;
                }
                else
                {
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
                        int id = rowData.ID;
                        var NVCHR_Note = rowData.NVCHR_Note;

                        var vendorCodesForSameProduct = allRowsData
                            .Where(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O") && x.MaThietBi == maThietBi)
                            .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                            .Where(v => !string.IsNullOrEmpty(v))
                            .Distinct()
                            .Count();

                        if (vendorCodesForSameProduct >= 2 && bitSelect.Contains("O"))
                        {
                            errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct].Value);
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
                            errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory].Value);
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
                                    (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender && x.MaThietBi == maThietBi)
                            .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                            .Distinct()
                            .Count();

                        if (duplicatePrice > 1)
                        {
                            errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC].Value);
                        }

                        // check lựa chọn nhà cung cấp bắt buộc phải lựa chọn O và X
                        if (!bitSelect.Contains("O") && !bitSelect.Contains("X"))
                        {
                            errors.Add(_localizer["InvalidSelection", 52].Value);
                        }
                        // check reason pick
                        if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason))
                        {
                            errors.Add(_localizer["SelectedVendorNoReasonColumn", 52].Value);
                        }
                        // check reason remark
                        if (bitSelect.Contains("X") && string.IsNullOrEmpty(reason))
                        {
                            errors.Add(_localizer["RejectedVendorNoRemarkColumn", 53].Value);
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

                    var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(dtoList, vm.userNextApproval, GetCurrentUserId());
                    if (!result.Success)
                    {
                        return BadRequest(result.Message);
                    }
                    return Ok(new { message = "Import successful", totalRows = items.Count });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(_localizer["ReadFileError", ex.Message].Value);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmImportedSupplier([FromBody] ConfirmImportedSupplierRequest req)
        {
            if (req?.Items == null || !req.Items.Any())
                return BadRequest(_localizer["NoValidDataReceived"].Value);

            var result = await _baoGiaDetailService.UpdatePickSupplierDetailAsync(
                req.Items, req.UserNextApproval ?? string.Empty, GetCurrentUserId());
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = "Import successful", totalRows = req.Items.Count });
        }

        [HttpPost]
        public async Task<IActionResult> SearchInputQuote([FromBody] SearchInputQuote searchModel)
        {
            if (searchModel == null) return BadRequest(_localizer["SearchInputMissing"].Value);
            var result = await _baoGiaDetailService.SearchBaoGiaAsync(searchModel.idRequestQuote, searchModel.maDon,
                searchModel.maVatTu, searchModel.maNcc, searchModel.section, GetCurrentUserId(), searchModel.dayMM, searchModel.status, GetRolesUser(), searchModel.pageSize, searchModel.pageIndex);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        // MARK: lấy thông tin lựa chọn báo giá 
        [HttpPost]
        public async Task<IActionResult> GetThongTinBaoGiaGomNhom([FromBody] ThongTinBaoGiaGomNhomModel model)
        {
            var result = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync(model, GetCurrentUserId(), GetRolesUser());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        // Xuất dữ liệu để lựa chọn phê duyệt NCC

        [HttpPost]
        public async Task<IActionResult> ExportFileExcelApproverResult([FromBody] List<string> model)
        {
            try
            {
                var result = await _baoGiaService.GetExportApprovalInfoAsync(model, GetCurrentUserId());
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                if (result.Data == null)
                {
                    return BadRequest(_localizer["NoDataToExport"].Value);
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
                var errorGroups = new HashSet<string>();

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
                        errors.Add(_localizer["VendorCodeMultipleSelected", maHangNB, vendorCodesForSameProduct].Value);
                    }

                    var hasAnySelect = allRowsData.Any(x => x.MaHangNoiBo == maHangNB && x.BIT_Select.Contains("O"));
                    if (!hasAnySelect && !string.IsNullOrEmpty(maHangNB))
                    {
                        errors.Add(_localizer["VendorCodeNotSelected", maHangNB].Value);
                    }

                    var vendorsForEquipmentAndCategory = allRowsData
                        .Where(x => (x.MaThietBi == maThietBi && x.ChungLoaiHang == chungLoaiHang && x.BIT_Select.Contains("O")) && !string.IsNullOrEmpty(maThietBi))
                        .Select(x => !string.IsNullOrEmpty(x.MaHangNCC_Vendor) ? x.MaHangNCC_Vendor : x.MaHangNCC_BIVN)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Distinct()
                        .Count();

                    if (vendorsForEquipmentAndCategory > 1 && bitSelect.Contains("O"))
                    {
                        errors.Add(_localizer["EquipmentCategoryMultipleVendors", maThietBi, chungLoaiHang, vendorsForEquipmentAndCategory].Value);
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
                                errors.Add(_localizer["SelectedPriceNotLowest", currentPriceUSD.ToString("N2"), minPriceUSD.ToString("N2")].Value);
                            }
                        }
                    }

                    var duplicatePrice = allRowsData
                        .Where(x => x.MaHangNoiBo == maHangNB &&
                               ((!string.IsNullOrEmpty(x.MaHangNCC_Vendor) && x.MaHangNCC_Vendor == maHangNCC) ||
                                (!string.IsNullOrEmpty(x.MaHangNCC_BIVN) && x.MaHangNCC_BIVN == maHangNCC)) && x.CodeVender == codeVender && x.MaThietBi == maThietBi)
                        .Select(x => new { x.DonGiaUSD, x.DonGiaVND })
                        .Distinct()
                        .Count();

                    if (duplicatePrice > 1)
                    {
                        errors.Add(_localizer["DuplicatePriceForVendor", maHangNB, maHangNCC].Value);
                    }

                    if (bitSelect.Contains("O") && string.IsNullOrEmpty(reason))
                    {
                        errors.Add(_localizer["SelectedVendorNoReason"].Value);
                    }

                    // Lưu lỗi vào dictionary với key là ID
                    if (errors.Any())
                    {
                        errorDetails[id] = errors;
                        // Thêm nhóm (MaDon|MaHangNoiBo|MaThietBi)
                        string groupKey = $"{maDon}|{maHangNB}|{maThietBi}";
                        errorGroups.Add(groupKey);
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
                    return BadRequest(_localizer["TemplateNotFound", "TemplateQuotationResults.xlsx"].Value);
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest(_localizer["WorksheetNotFound"].Value);
                }
                int rowStart = 4;
                foreach (var item in dataList)
                {
                    var enUs = new CultureInfo("en-US");
                    int col = 1;

                    // Xác định nhóm của dòng hiện tại
                    string groupKey = $"{item.CHR_MaDon ?? ""}|{item.CHR_MaHangNoiBo ?? ""}|{item.CHR_MaThietBi ?? ""}";
                    // Nếu nhóm có lỗi, tô toàn bộ dòng màu đỏ
                    if (errorGroups.Contains(groupKey))
                    {
                        ws.Row(rowStart).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

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
                    ws.Cell(rowStart, col++).SetValue(item.ShortName ?? item.NVCHR_TenNCC ?? string.Empty);
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
                return BadRequest(_localizer["ExportError", ex.Message].Value);
            }
        }

        // lấy thông tin theo chi tiêt mã đơn phe duyet
        [HttpPost]
        public async Task<IActionResult> GetSupplierApprovalInfor([FromBody] string maDon)
        {
            if (string.IsNullOrWhiteSpace(maDon))
            {
                return BadRequest(_localizer["OrderCodeRequired"].Value);
            }
            var result = await _baoGiaService.GetSupplierApprovalInfoAsync(maDon, GetCurrentUserId());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmApprover([FromBody] ConfirmApproverModel model)
        {
            if (model == null || !model.listCofirm.Any())
            {
                return BadRequest(_localizer["NoApprovalData"].Value);
            }
            return await PheDuyetBaoGia(model.listCofirm, model.UserApproverNext);
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
                    return BadRequest(_localizer["NoDataToExport"].Value);
                }
                var Status = await _baoGiaStatusService.GetListStatusAsync();
                if (Status == null || !Status.Success)
                {
                    return BadRequest(_localizer["StatusListFetchError"].Value);
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
                    return BadRequest(_localizer["TemplateNotFound", "FileSelectionQuote.xlsx"].Value);
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest(_localizer["WorksheetNotFound"].Value);
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
                return BadRequest(_localizer["ExportError", ex.Message].Value);
            }
        }
        private string GetApprovalStatus(int currentStep, int requiredStep, string? reason)
        {
            if (currentStep <= requiredStep) return "";
            return string.IsNullOrEmpty(reason) ? "OK" : "NG";
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
            var madons = await _baoGiaService.GetMaDonByAdidAsync(GetCurrentUserId() ?? "", step, GetRolesUser() ?? "USER");
            return madons.Data ?? new List<string>();
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
    }
}

public class ConfirmImportedSupplierRequest
{
    public List<BaoGia_Detail_of_QuotationDTO> Items { get; set; } = new();
    public string? UserNextApproval { get; set; }
}
