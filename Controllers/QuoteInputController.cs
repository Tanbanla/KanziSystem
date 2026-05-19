using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Quote;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [Route("QuoteInput")]
    public class QuoteInputController : BaseAuthController
    {
        private readonly IConfiguration _configuration;
        private readonly IMaterialService _materialService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IDepartmentService _deparmentService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IBaoGiaService _baoGiaService;
        private readonly IBaoGiaDetailService _baoGiaDetailService;
        private readonly ISendMailService _sendMailService;

        public QuoteInputController(
            IConfiguration configuration,
            IMaterialService materialService,
            ITmNccNewService tmNccNewService,
            IDepartmentService deparmentService,
            ITmCategoryService tmCategoryService,
            IBaoGiaService baoGiaService,
            IBaoGiaDetailService baoGiaDetailService,
            ISendMailService sendMailService)
        {
            _configuration = configuration;
            _materialService = materialService;
            _tmNccNewService = tmNccNewService;
            _deparmentService = deparmentService;
            _tmCategoryService = tmCategoryService;
            _baoGiaService = baoGiaService;
            _baoGiaDetailService = baoGiaDetailService;
            _sendMailService = sendMailService;
        }

        [HttpGet("InputQuote")]
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
            return View("~/Views/Quote/InputQuote/InputQuote.cshtml", vm);
        }

        [HttpPost("SearchInputQuote")]
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

        [HttpPost("SearchInputQuoteBySoDon")]
        public async Task<IActionResult> SearchInputQuoteBySoDon([FromBody] ThongTinBaoGiaGomNhomModel mod)
        {
            var result = await _baoGiaService.SearchThongTinNhapBaoGiaAsync(mod.maDon, mod.section, mod.maHang, GetCurrentUserId(), mod.pageIndex, mod.pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("InsertInputQuote")]
        public async Task<IActionResult> InsertInputQuote([FromBody] InsertInputQuoteModel model)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dtoList = new List<BaoGia_Detail_of_QuotationDTO>();
                foreach (var item in model.baoGiaDetail)
                {
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
                await _sendMailService.SendMailToSupplierByRequestCodeAsync(model.MaDon);
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi chuyển đổi dữ liệu: {ex.Message}");
            }
        }

        private async Task<List<string>> LoadCategoryDataAsync()
        {
            var categoryS = await _tmCategoryService.GetListCategory();
            return categoryS.Data ?? new List<string>();
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
    }
}
