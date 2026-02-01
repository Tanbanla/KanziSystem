using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Working;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

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
        public QuoteController(ILogger<QuoteController> logger, ITmNccNewService tmNccNewService,
            IBaoGiaService baoGiaService, IMaterialService materialService, ITmSectionService tmSectionService,
            INhomViTriService nhomViTriService, IBaoGiaNCCService baoGiaNCCService, IBaoGiaHistoryService baoGiaHistoryService,
            IBaoGiaStatusService baoGiaStatusService, IBaoGiaDetailService baoGiaDetailService)
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
        }
        // MARK: - Quote
        public async Task<IActionResult> Index()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 0, 0);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();

            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                NguoiThaoTac = GetCurrentUserId() ?? ""
            };
            return View(vm);
        }
        // MARK: - Quotation Results
        public async Task<IActionResult> Quotation_Results()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 0, 0);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync();
            var danhSach = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync("", "", "", 1, 10);
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                DanhSachMaDon = madons,
                NguoiThaoTac = GetCurrentUserId() ?? "",
                DanhSachBaoGiaGomNhom = danhSach.Data
            };
            return View(vm);
        }
        // MARK: - Input Quote
        public async Task<IActionResult> InputQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 0, 0);
            var nccs = await LoadNhaCungCapDataAsync();
            var categorys = await LoadCategoryDataAsync();
            var madons = await LoadMadonAsync();
            var danhSach = await _baoGiaService.SearchAsync("", "", "", "", "", "", 6, 0, 0, null);
            var vm = new QuoteModel
            {
                DanhSachNhomViTri = nhomViTri,
                DanhSachVatTu = materials.Data ?? new List<MATERIALDTO>(),
                DanhSachNhaCungCap = nccs,
                DanhSachCategory = categorys,
                DanhSachMaDon = madons,
                NguoiThaoTac = GetCurrentUserId() ?? "",
                DanhSachYeuCauBaoGia = danhSach.Data ?? new List<BaoGia_Request_of_QuotationDTO>()
            };
            return View(vm);
        }
        // MARK: - Select Quote Section
        public IActionResult SelectQuoteSection()
        {
            return View();
        }
        // MARK: - HistoryQuote
        public async Task<IActionResult> HistoryQuote()
        {
            var nhomViTri = await LoadNhomViTriDataAsync();
            var materials = await _materialService.SearchAsync("", "", "", 0, 0);
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
        private async Task<List<dynamic>> LoadCategoryDataAsync()
        {
            var CategoryS = await _materialService.GetListMaterial();
            return CategoryS.Data ?? new List<dynamic>();
        }
        private async Task<List<ACC_NHOMVITRIDTO>> LoadNhomViTriDataAsync()
        {
            var nhomViTri = await _nhomViTriService.GetAllNhomViTriAsync();
            return nhomViTri.Data ?? new List<ACC_NHOMVITRIDTO>();
        }
        private async Task<List<IM_NCC_NEWDTO>> LoadNhaCungCapDataAsync()
        {
            var nccNews = await _tmNccNewService.GetAllNccNew();
            return nccNews.Data ?? new List<IM_NCC_NEWDTO>();
        }
        private async Task<List<string>> LoadMadonAsync()
        {
            var madons = await _baoGiaService.GetListMaDonBGAsync();
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
                    CHR_ActionType = "INSERT"
                }).ToList();

                if (histories.Any())
                {
                    await _baoGiaHistoryService.InsertHistoryListAsync(histories);
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
                searchModel.PageIndex,
                searchModel.PageSize,
                searchModel.Date
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
                    // Map theo thứ tự cột trong bảng ở giao diện
                    var dto = new BaoGia_Request_of_QuotationDTO
                    {
                        CHR_SectionCode = ws.Cell(r, 2).GetString(), // Mã phòng ban (value)
                        CHR_SectionName = ws.Cell(r, 3).GetString(), // hiển thị có thể giống mã
                        CHR_Phanloai = ws.Cell(r, 4).GetString(),
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

                    // Nếu có mã hàng nội bộ, tự check nhà cung cấp và nhân bản theo NCC
                    var suppliersResp = await _baoGiaNCCService.GetBaoGiaNCCByMaHang(dto.CHR_MaHangNoiBo ?? string.Empty);
                    if (suppliersResp.Success && suppliersResp.Data != null && suppliersResp.Data.Count > 0)
                    {
                        var first = true;
                        foreach (var sup in suppliersResp.Data)
                        {
                            var copy = first ? dto : CloneDto(dto);
                            copy.CHR_MaNCC = sup.CHR_MaNCC;
                            copy.NVCHR_TenNCC = sup.NVCHAR_TenNCC;
                            items.Add(copy);
                            first = false;
                        }
                    }
                    else
                    {
                        items.Add(dto);
                    }
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        private static double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
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
            return v == "O" || v == "true" || v == "1" ? true : v == "X" || v == "false" || v == "0" ? false : null;
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
            //int? idRequest, string? maDon, string? maVatTu, string? maNcc, string? section, int? PageSize, int? PageIndex
            var result = await _baoGiaDetailService.SearchBaoGiaAsync(searchModel.idRequestQuote, searchModel.maDon,
                searchModel.maVatTu, searchModel.maNcc, searchModel.section, searchModel.dayMM, searchModel.pageSize, searchModel.pageIndex);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> InsertInputQuote([FromBody] List<dynamic> baoGiaDetail)
        {
            try
            {
                // Convert List<dynamic> to List<BaoGia_Detail_of_QuotationDTO>
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dtoList = new List<BaoGia_Detail_of_QuotationDTO>();
                foreach (var item in baoGiaDetail)
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
            var result = await _baoGiaService.GetThongTinBaoGiaGomNhomAsync(model.maDon, model.section, model.maHang, model.pageIndex, model.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
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
            var result = await _baoGiaDetailService.UpdateLuaChonNCCBaoGiaDetailAsync(listUpdate, GetCurrentUserId() , GetCurrentUserFullName());
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
    }
}
