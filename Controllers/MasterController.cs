using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Master;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class MasterController : BaseAuthController
    {
        private readonly IMasterApproverSendMailService _approverService;
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly INhomViTriService _nhomViTriService;
        private readonly ITmSectionService _tmSectionService;
        private readonly IEmployeeWorkingService _employeeWorkingService;
        private readonly ITmNccNewService _tmNccNewService;
        private readonly IBaoGiaNccCategoryService _baoGiaNccCategoryService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly IMaterialService _materialService;
        private readonly ITmEmployeeAgentService _employeeAgentService;
        private readonly ITmUserService _tmUserService;
        private readonly IDepartmentService _departmentService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        private readonly ILogger<MasterController> _logger;

        public MasterController(IMasterApproverSendMailService approverService, IBaoGiaStepService baoGiaStepService, INhomViTriService nhomViTriService,
            ITmSectionService tmSectionService, IEmployeeWorkingService employeeWorkingService, ITmNccNewService tmNccNewService,
            IBaoGiaNccCategoryService baoGiaNccCategoryService, IDepartmentService departmentService, IConfiguration configuration, IWebHostEnvironment env
            , ILogger<MasterController> logger, ITmCategoryService tmCategoryService, IMaterialService materialService, ITmEmployeeAgentService employeeAgentService, ITmUserService tmUserService)
        {
            _approverService = approverService;
            _baoGiaStepService = baoGiaStepService;
            _tmSectionService = tmSectionService;
            _employeeWorkingService = employeeWorkingService;
            _nhomViTriService = nhomViTriService;
            _tmNccNewService = tmNccNewService;
            _baoGiaNccCategoryService = baoGiaNccCategoryService;
            _logger = logger;
            _tmCategoryService = tmCategoryService;
            _materialService = materialService;
            _employeeAgentService = employeeAgentService;
            _tmUserService = tmUserService;
            _departmentService = departmentService;
            _configuration = configuration;
            _env = env;
        }

        public IActionResult Masters()
        {
            return View();
        }
        public IActionResult master_vender()
        {
            return View();
        }
        public IActionResult master_material()
        {
            return View();
        }
        [HttpPost]
        public JsonResult load_vender(VENDER vder)
        {
            List<VENDER> dt = Vender_process._listVender(vder);
            return Json(dt);
        }
        [HttpPost]
        public JsonResult load_warehouse()
        {
            List<Models.MST_WAREHOUSE> dt = Models.MST_WAREHOUSE.warehouse_process();
            return Json(dt);
        }
        [HttpPost]
        public JsonResult load_fac()
        {
            List<Models.MST_WAREHOUSE> dt = Models.MST_WAREHOUSE.warehouse_process();
            var dup = dt.Select(x => x.CHR_FACTORY).Distinct();
            return Json(dup);
        }
        [HttpPost]
        public JsonResult load_sec(string fac)
        {
            List<Models.MST_WAREHOUSE> dt = Models.MST_WAREHOUSE.warehouse_process();
            dt = dt.Where(x => x.CHR_FACTORY == fac).ToList();
            var dup = dt.Select(x => x.CHR_DEPT_USE).Distinct();
            return Json(dup);
        }
        [HttpPost]
        public JsonResult load_wh(string fac, string sec)
        {
            List<Models.MST_WAREHOUSE> dt = Models.MST_WAREHOUSE.warehouse_process();
            dt = dt.Where(x => x.CHR_FACTORY == fac && x.CHR_DEPT_USE == sec).ToList();
            return Json(dt);
        }
        public JsonResult load_ma(string us)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();

            string khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");
            List<string> materials = MST_INVENTORY._getname_material(khoi, "");
            var checklistData = new List<object>();

            foreach (var item in materials)
            {
                checklistData.Add(new
                {                  
                    Ma = item.Split(":")[0],
                    Ten = item.Split(":")[1]
                });
            }
            return Json(checklistData);
        }
        public JsonResult load_kho_theo_ma(string manguyenlieu)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var kho = db.GET_DATA_FROM_SQL("select Kho, Hientai from [KHO] where [MaNguyenLieu] = '" + manguyenlieu + "'");
            var kh = new List<object>();
            for (int i = 0; i < kho.Rows.Count; i++)
            {
                kh.Add(new
                {
                    Ma = kho.Rows[i]["Kho"].ToString()!,
                    Ten = kho.Rows[i]["Hientai"].ToString()!
                });
            }
            return Json(kh);
        }
        //public JsonResult load_soluonghientai(string manguyenlieu, string us, string khochuyen)
        //{
        //    SQL_Connect_DB20 db = new SQL_Connect_DB20();
        //    string khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + us + "'");
        //    var sl = db.ReturnString("SELECT [Hientai] FROM [KHO] WHERE [MaNguyenLieu] = '" + manguyenlieu) + "' AND [Group_Code] = '" + khoi + "' AND [Kho] = '" + khochuyen + "'");
        //}
        public PartialViewResult _modal()
        {
            return PartialView();
        }
        // MARK: Master Approver Send Mail
        public async Task<IActionResult> SendApprover()
        {
            var vm = await LoadDataSendApprover();
            var nhomViTris = await GetNhomViTriList();
            vm.NhomViTris = nhomViTris;
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            return View(vm);

        }
        // Lấy dữ liệu lúc loading màn hình SendApprover
        private async Task<SendApproverVM> LoadDataSendApprover()
        {
            var vm = new SendApproverVM();
            var sectionsTask = _tmSectionService.GetAllSectionsAsync();
            var stepsTask = _baoGiaStepService.GetAll();

            await Task.WhenAll(sectionsTask, stepsTask);
            // Phòng ban
            var sectionsResp = sectionsTask.Result;
            try
            {
                vm.SectionCodes = sectionsResp.Success ? sectionsResp.Data : new List<TM_SECTIONDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sections");
            }
            // Các Bước báo giá
            var stepsResp = stepsTask.Result;
            try
            {
                vm.baoGiaSteps = stepsResp.Success ? stepsResp.Data.OrderBy(c=>c.INT_StepNumber).ToList() : new List<BaoGia_StepDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading steps");
            }
            return vm;
        }
        // Lay thong tin phong ban
        public async Task<List<DEPARTMENTDTO>> GetNhomViTriList()
        {
            try
            {
                var result = await _departmentService.GetAllDepartmentAsync();
                return (List<DEPARTMENTDTO>)result.Data;
            }
            catch (Exception ex)
            {
                return new List<DEPARTMENTDTO>();
            }
        }
        // API: lấy dữ liệu theo điều kiện (section, adid, bước)
        [HttpPost]
        public async Task<JsonResult> GetApprovers([FromBody] GetApproversRequestDTO req)
        {
            var resp = await _approverService.GetByConditionAsync(req?.SectionCode, req?.Adid, req?.IdStep, req?.PageIndex ?? 1, req?.PageSize ?? 1000);
            if (resp == null || !resp.Success)
            {
                return Json(new { success = false, message = resp?.Message ?? "Error" });
            }
            var data = resp.Data ?? new List<BaoGia_Master_Approver_Send_MailDTO>();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<JsonResult> SaveApprover([FromBody] BaoGia_Master_Approver_Send_MailDTO obj)
        {
            if (obj == null)
                return Json(new { success = false, message = "Invalid data" });
            obj.CHR_CreateBy = GetCurrentUserId() ?? "system";
            obj.CHR_CreateDate = obj.CHR_CreateDate ?? System.DateTime.Now;
            var resp = await _approverService.SaveMasterApproverSendMailAsync(obj);
            return Json(new { success = resp.Success, message = resp.Message });
        }

        [HttpPost]
        public async Task<JsonResult> UpdateApprover([FromBody] BaoGia_Master_Approver_Send_MailDTO obj)
        {
            if (obj == null || obj.ID == 0)
                return Json(new { success = false, message = "Invalid data" });
            obj.CHR_UpdateBy = GetCurrentUserId() ?? "system";
            obj.CHR_UpdateDate = System.DateTime.Now;
            var resp = await _approverService.UpdateMasterApproverSendMailAsync(obj);
            return Json(new { success = resp.Success, message = resp.Message });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteApprover([FromBody] DeleteApproverRequest request)
        {
            if (request.Id == 0)
                return Json(new { success = false, message = "Invalid id" });
            var userAction = GetCurrentUserId() ?? "system";
            var resp = await _approverService.DeleteMasterApproverSendMailAsync(request.Id, userAction);
            return Json(new { success = resp.Success, message = resp.Message });
        }
        // Lấy thông tin nhân viên theo ADID or MNV
        [HttpGet]
        public async Task<JsonResult> GetEmployeeWorkingByIdAsync(string adidOrMnv)
        {
            var resp = await _employeeWorkingService.GetEmployeeWorkingByIdAsync(adidOrMnv);
            if (resp == null || !resp.Success)
            {
                return Json(new { success = false, message = resp?.Message ?? "Error" });
            }
            var data = resp.Data ?? new List<dynamic>();
            return Json(new { success = true, data });
        }
        // MARK: Quản lý thông tin nhà cung cấp
        public async Task<IActionResult> SupplierMana()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            return View();
        }
        // Lấy danh sách chủng loại hàng
        [HttpGet]
        public async Task<IActionResult> GetCategoryList()
        {
            var resp = await _tmCategoryService.GetListCategory();
            if (resp == null || !resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }
        // tìm kiếm thông tin nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> SearchSupplier([FromBody] SearchSupplierRequestDTO req)
        {
            var resp = await _tmNccNewService.GetNccNewPaging(req?.CodeNcc, req?.NameNcc, req?.PageIndex ?? 1, req?.PageSize ?? 10);
            if (resp == null || !resp.Success)
            {
                return BadRequest(resp);
            }

            return Ok(resp);
        }
        // Thêm thông tin nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] IM_NCC_NEWDTO supplierDto)
        {
            supplierDto.nguoi_cap_nhat = GetCurrentUserId() ?? "system";
            var result = await _tmNccNewService.AddNccNew(supplierDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        // Update thông tin nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> UpdateSupplier([FromBody] IM_NCC_NEWDTO supplierDto)
        {
            supplierDto.nguoi_cap_nhat = GetCurrentUserId() ?? "system";
            var result = await _tmNccNewService.UpdateNccNew(supplierDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        // Xóa thông tin nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> DeleteSupplier([FromBody] DeleteSupplierRequestDTO req)
        {
            var userAction = GetCurrentUserId() ?? "system";
            var result = await _tmNccNewService.DeleteNccNewByCode(req.Id, userAction);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        // Thông tin chi tiết loại hàng nhà cung cấp 
        [HttpGet]
        public async Task<IActionResult> GetSupplierDetail(string codeNcc)
        {
            var resp = await _baoGiaNccCategoryService.GetBaoGiaNccCategoryByMaNCC(codeNcc);
            if (resp == null || !resp.Success)
            {
                return BadRequest(resp);
            }
            return Ok(resp);
        }
        // thêm thông tin loại hàng nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> AddSupplierDetail([FromBody] BaoGia_NCC_CategoryDTO categoryDto)
        {
            categoryDto.CHR_CreateBy = GetCurrentUserId() ?? "system";
            categoryDto.DTM_CreateBy = System.DateTime.Now;
            var result = await _baoGiaNccCategoryService.AddBaoGiaNccCategory(categoryDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        // xóa thông tin loại hàng nhà cung cấp
        [HttpGet]
        public async Task<IActionResult> DeleteSupplierDetail(int req)
        {
            var userAction = GetCurrentUserId() ?? "system";
            var result = await _baoGiaNccCategoryService.DeleteBaoGiaNccCategory(req);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        // nhập thông tin loại hàng nhà cung cấp từ file excel
        [HttpPost]
        public async Task<IActionResult> ImportSupplierDetail([FromForm] ImportSupplierDetailDTO insertFile)
        {
            if (insertFile.FileExcel == null || insertFile.FileExcel.Length == 0)
            {
                return BadRequest("File không hợp lệ");
            }

            var items = new List<BaoGia_NCC_CategoryDTO>();
            var user = GetCurrentUserId() ?? "system";
            try
            {
                using var stream = insertFile.FileExcel.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // lấy dữ liệu từ dòng 3
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột trong bảng ở giao diện
                    var dto = new BaoGia_NCC_CategoryDTO
                    {
                        Id = 0,
                        CHR_MaNCC = ws.Cell(r, 2).GetString().Trim(),
                        NVCHR_TenNCC = ws.Cell(r, 4).GetString().Trim(),
                        NVCHR_ChungLoai = ws.Cell(r, 1).GetString().Trim(),
                        NVCHR_SanXuat = ws.Cell(r, 3).GetString().Trim(),
                        CHR_Status = "Active",
                        CHR_CreateBy = user,
                        DTM_CreateBy = DateTime.Now,
                        CHR_Mail = ws.Cell(r, 6).GetString().Trim(),
                        CHR_PIC = "SDT: " +ws.Cell(r, 7).GetString().Trim()+  ",Name: " + ws.Cell(r, 8).GetString().Trim()
                    };
                    // Lọc trùng dữ liệu trong file excel trước khi thêm vào danh sách, tránh trường hợp file có nhiều
                    if (items.Where(x => x.CHR_MaNCC == dto.CHR_MaNCC && x.NVCHR_ChungLoai == dto.NVCHR_ChungLoai && x.NVCHR_SanXuat == dto.NVCHR_SanXuat).Any())
                    {
                        //break;
                    }
                    else
                    {
                        items.Add(dto);
                    }
                }
                if (items.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                await _baoGiaNccCategoryService.AddListBaoGiaNccCategory(items);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }

        }
        // Nhập file cập nhật thông tin phòng ban
        [HttpPost]
        public async Task<IActionResult> ImportSectionExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Không nhận được dữ liệu từ file");
            }
            var listSections = new List<ACC_NHOMVITRIDTO>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                int startRow = 1;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int i = startRow; i <= lastRow; i++)
                {
                    var cellVal = ws.Cell(i, 1).GetString();
                    if (string.IsNullOrWhiteSpace(cellVal))
                    {
                        break;
                    }
                    var dto = new ACC_NHOMVITRIDTO
                    {
                        LoaiVitri = "",
                        Mahangmuctheovitri = cellVal.Trim(),
                        Tenhangmuctheovitri = ws.Cell(i, 2).GetString().Trim(),
                        Model = ""
                    };
                    listSections.Add(dto);
                }
                if (listSections.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                var result = await _nhomViTriService.InsertNhomViTriListAsync(listSections);
                if (!result.Success)
                {
                    return BadRequest("Error Insert database: " + result.Message);
                }
                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Nhập file cập nhật thông tin các mặt hàng
        [HttpPost]
        public async Task<IActionResult> UpdateMaterialInfo([FromForm] ImportSupplierDetailDTO insertFile)
        {

            if (insertFile.FileExcel == null || insertFile.FileExcel.Length == 0)
            {
                return BadRequest("File không hợp lệ");
            }

            var items = new List<MATERIALDTO>();
            try
            {
                using var stream = insertFile.FileExcel.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // lấy dữ liệu từ dòng 3
                int startRow = 3;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break;
                    }
                    // cac truong con lai doi cap nhat
                    // Map theo thứ tự cột trong bảng ở giao diện
                    var dto = new MATERIALDTO
                    {
                        Material_Code = ws.Cell(r, 2).GetString().Trim(),
                        Category_VN = ws.Cell(r, 6).GetString(),
                        Code_Suppiler = ws.Cell(r, 3).GetString(),
                        Material_Name_VN = ws.Cell(r, 5).GetString(),
                        Material_Name_EN = ws.Cell(r, 4).GetString(),
                        Group_Code = ws.Cell(r, 7).GetString(),
                        Shape = ws.Cell(r, 8).GetString(),
                        Material = ws.Cell(r, 9).GetString(),
                        Composition = ws.Cell(r, 10).GetString(),
                        Dimension = ws.Cell(r, 11).GetString(),
                        UsedFor = ws.Cell(r, 12).GetString(),
                        Purpose = ws.Cell(r, 13).GetString(),
                    };

                    items.Add(dto);
                }
                if (items.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                await _materialService.UpdateListThongTinNoList(items);
                //await _materialService.UpdateListThongTin(items);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // update short name supperlier to file excel
        [HttpPost]
        public async Task<IActionResult> UpdateShortNameSupplier([FromForm] ImportSupplierDetailDTO insertFile)
        {

            if (insertFile.FileExcel == null || insertFile.FileExcel.Length == 0)
            {
                return BadRequest("File không hợp lệ");
            }

            var items = new List<IM_NCC_NEW>();
            try
            {
                using var stream = insertFile.FileExcel.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // lấy dữ liệu từ dòng 2
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break;
                    }
                    var dto = new IM_NCC_NEW
                    {
                          Ma = ws.Cell(r, 2).GetString(),
                          ShortName = ws.Cell(r, 3).GetString()
                    };

                    items.Add(dto);
                }
                if (items.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                await _tmNccNewService.UpdateShortNames(items);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Nhập danh sách loại hàng nhà cung cấp
        [HttpPost]
        public async Task<IActionResult> AddListSupplierDetail([FromForm] InsertFileExcelSupplierRequestDTO insertFile)
        {
            if (insertFile.FileExcel == null || insertFile.FileExcel.Length == 0)
                return BadRequest("File không hợp lệ");

            var items = new List<BaoGia_NCC_CategoryDTO>();
            var user = GetCurrentUserId() ?? "system";
            try
            {
                using var stream = insertFile.FileExcel.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                // Dữ liệu bắt đầu từ dòng 2
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột trong bảng ở giao diện
                    var dto = new BaoGia_NCC_CategoryDTO
                    {
                        Id = 0,
                        CHR_MaNCC = insertFile.maNCC,
                        NVCHR_TenNCC = insertFile.tenNCC,
                        NVCHR_ChungLoai = ws.Cell(r, 1).GetString().Trim(),
                        NVCHR_SanXuat = ws.Cell(r, 2).GetString().Trim(),
                        CHR_Status = "Active",
                        CHR_CreateBy = user,
                        DTM_CreateBy = DateTime.Now,
                        CHR_Mail = ws.Cell(r, 4).GetString().Trim(),
                        CHR_PIC = ws.Cell(r, 3).GetString().Trim()
                    };
                    items.Add(dto);
                }
                if (items.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                await _baoGiaNccCategoryService.AddListBaoGiaNccCategory(items);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }

        }

        // Xuất Excel theo dữ liệu đang hiển thị (theo filter / paging)
        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] SearchSupplierRequestDTO req)
        {
            var resp = await _tmNccNewService.GetNccNewPaging(req?.CodeNcc, req?.NameNcc, req?.PageIndex ?? 1, req?.PageSize ?? 1000);
            if (resp == null || !resp.Success)
            {
                return BadRequest(resp);
            }
            var list = resp.Data as System.Collections.IEnumerable;
            try
            {
                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.Worksheets.Add("Suppliers");
                // headers - export all columns from IM_NCC_NEW
                var headers = new[] {
                    "ID",
                    "Mã",
                    "Tên",
                    "Địa chỉ",
                    "Số điện thoại",
                    "Fax",
                    "Khu vực",
                    "Ghi chú",
                    "Hình thức thanh toán",
                    "Điều kiện thanh toán",
                    "Mã số thuế",
                    "Nhân viên kinh doanh",
                    "Nhân viên kế toán",
                    "Cần phải xác nhận làm thủ tục hải quan",
                    "Nhóm",
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                }

                int r = 2;
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        object GetProp(object src, params string[] names)
                        {
                            if (src == null) return null;
                            var t = src.GetType();
                            foreach (var n in names)
                            {
                                var p = t.GetProperty(n, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                if (p != null) return p.GetValue(src);
                            }
                            return null;
                        }
                        var id = GetProp(item, "Ncc_Id");
                        var ma = GetProp(item, "Ma");
                        var ten = GetProp(item, "Ten");
                        var diachi = GetProp(item, "Diachi");
                        var sdt = GetProp(item, "Sodienthoai");
                        var fax = GetProp(item, "Fax");
                        var khuvuc = GetProp(item, "Khuvuc");
                        var ghichu = GetProp(item, "Ghichu");
                        var hinhthucmotk = GetProp(item, "Hinhthucmotk");
                        var dieukien = GetProp(item, "Dieukienthanhtoan");
                        var masothue = GetProp(item, "Masothue");
                        var nvkd = GetProp(item, "Nhanvienkinhdoand");
                        var nvkt = GetProp(item, "Nhanvienketoan");
                        var canphaixacnhan = GetProp(item, "Canphaixacnhanlamthutuchaiquan");
                        var nhom = GetProp(item, "nhom");

                        ws.Cell(r, 1).Value = (id ?? string.Empty).ToString();
                        ws.Cell(r, 2).Value = (ma ?? string.Empty).ToString();
                        ws.Cell(r, 3).Value = (ten ?? string.Empty).ToString();
                        ws.Cell(r, 4).Value = (diachi ?? string.Empty).ToString();
                        ws.Cell(r, 5).Value = (sdt ?? string.Empty).ToString();
                        ws.Cell(r, 6).Value = (fax ?? string.Empty).ToString();
                        ws.Cell(r, 7).Value = (khuvuc ?? string.Empty).ToString();
                        ws.Cell(r, 8).Value = (ghichu ?? string.Empty).ToString();
                        ws.Cell(r, 9).Value = (hinhthucmotk ?? string.Empty).ToString();
                        ws.Cell(r, 10).Value = (dieukien ?? string.Empty).ToString();
                        ws.Cell(r, 11).Value = (masothue ?? string.Empty).ToString();
                        ws.Cell(r, 12).Value = (nvkd ?? string.Empty).ToString();
                        ws.Cell(r, 13).Value = (nvkt ?? string.Empty).ToString();
                        ws.Cell(r, 14).Value = (canphaixacnhan ?? string.Empty).ToString();
                        ws.Cell(r, 15).Value = (nhom ?? string.Empty).ToString();
                        r++;
                    }
                }

                ws.Columns().AdjustToContents();
                using var ms = new System.IO.MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                var fileName = $"Suppliers_{System.DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error exporting suppliers to excel");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // Import dữ liệu nhà cung cấp từ file excel
        [HttpPost]
        public async Task<IActionResult> ImportSupplierExcel([FromForm] IFormFile importRequest)
        {
            if (importRequest == null || importRequest.Length == 0)
                return BadRequest("File không hợp lệ");
            var suppliers = new List<IM_NCC_NEWDTO>();
            var user = GetCurrentUserId() ?? "system";
            try
            {
                using var stream = importRequest.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                // Dữ liệu bắt đầu từ dòng 2
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 2).GetString() == "" || ws.Cell(r, 2).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột trong bảng ở giao diện
                    var dto = new IM_NCC_NEWDTO
                    {
                        Ncc_Id = 0,
                        Ma = ws.Cell(r, 2).GetString().Trim(),
                        Ten = ws.Cell(r, 3).GetString().Trim(),
                        Diachi = ws.Cell(r, 4).GetString().Trim(),
                        Sodienthoai = ws.Cell(r, 5).GetString().Trim(),
                        Fax = ws.Cell(r, 6).GetString().Trim(),
                        Khuvuc = ws.Cell(r, 7).GetString().Trim(),
                        Hinhthucmotk = ws.Cell(r, 8).GetString().Trim(),
                        Dieukienthanhtoan = ws.Cell(r, 9).GetString().Trim(),
                        Masothue = ws.Cell(r, 10).GetString().Trim(),
                        Nhanvienkinhdoand = ws.Cell(r, 11).GetString().Trim(),
                        Nhanvienketoan = ws.Cell(r, 12).GetString().Trim(),
                        Canphaixacnhanlamthutuchaiquan = ws.Cell(r, 13).GetString().Trim(),
                        nhom = ws.Cell(r, 14).GetString().Trim(),
                        nguoi_cap_nhat = user
                    };
                    suppliers.Add(dto);
                }
                if (suppliers.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                await _tmNccNewService.AddListNccNew(suppliers);
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }

        public JsonResult del_warehouse(string id, string tenkho)
        {
            var del = Models.MST_WAREHOUSE.delete_wh(id, tenkho);
            return Json(del);
        }
        public JsonResult load_section()
        {
            List<string> sec = Models.SECTION._load_sec();
            return Json(sec);
        }
        public JsonResult insert_wh(string CHR_WAREHOUSE, string CHR_DEPT_USE, string CHR_FACTORY, string CHR_NOTE, string CHR_USER)
        {
            var ins = Models.MST_WAREHOUSE.Insert_warehouse(CHR_WAREHOUSE, CHR_DEPT_USE, CHR_FACTORY, CHR_NOTE, CHR_USER);
            return Json(ins);
        }

        // MARK: Màn hình quản lý chủng loại hàng
        public async Task<IActionResult> Category()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            return View();
        }
        // API tìm kiếm chủng loại hàng theo tên
        [HttpPost]
        public async Task<JsonResult> SearchCategoryByName([FromBody] SearchCatergory searchCatergory)
        {
            var resp = await _tmCategoryService.SearchCategoryByName(searchCatergory.Name, searchCatergory.pageIndex, searchCatergory.pageSize);
            if (resp == null || !resp.Success)
            {
                return Json(new { success = false, message = resp?.Message ?? "Error" });
            }
            var data = resp.Data ?? new ListRequest<TM_Category>();
            return Json(new { success = true, data });
        }
        // API thêm mới chủng loại hàng
        [HttpPost]
        public async Task<JsonResult> AddCategory([FromBody] TM_CategoryDTO req)
        {
            // Kiểm tra dữ liệu đầu vào
            if (req == null || string.IsNullOrEmpty(req.NVCHR_Category))
            {
                return Json(new { success = false, message = "Invalid data" });
            }
            req.CHR_CreateBy = GetCurrentUserId() ?? "system";
            req.DTM_CreateBy = System.DateTime.Now;
            var resp = await _tmCategoryService.AddCategory(req);
            return Json(new { success = resp.Success, message = resp.Message, data = resp.Data });
        }
        // API xoa thong tin chung loai hang
        [HttpPost]
        public async Task<JsonResult> DeleteCategory([FromBody] int id)
        {
            if (id == 0)
            {
                return Json(new { success = false, message = "Invalid id" });
            }
            var resp = await _tmCategoryService.DeleteCategory(id);
            return Json(new { success = resp.Success, message = resp.Message });
        }
        // Import dữ liệu chủng loại hàng từ file excel
        [HttpPost]
        public async Task<IActionResult> ImportCategory([FromForm] IFormFile importRequest)
        {
            if (importRequest == null || importRequest.Length == 0)
                return BadRequest("File không hợp lệ");
            var categories = new List<TM_CategoryDTO>();
            var user = GetCurrentUserId() ?? "system";
            try
            {
                using var stream = importRequest.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                // Dữ liệu bắt đầu từ dòng 2
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 1).GetString() == "" || ws.Cell(r, 1).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột
                    var dto = new TM_CategoryDTO
                    {
                        CHR_CreateBy = user,
                        DTM_CreateBy = System.DateTime.Now,
                        NVCHR_Category = ws.Cell(r, 1).GetString().Trim()
                    };
                    categories.Add(dto);
                }
                if (categories.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                //service có AddListCategory
                var result = await _tmCategoryService.AddListCategory(categories);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        public JsonResult chuyenkho(string mahang, string khohientai, string tonkho, string phongban, string denkho, string soluong, string nguoichuyen, string khoi)
        {
            string manguyenlieu = mahang.Split('-')[0].Trim();
            var ck = Models.MST_WAREHOUSE.Chuyenkho(manguyenlieu, khohientai, tonkho, phongban, denkho, soluong, nguoichuyen, khoi);
            return Json(ck);
        }
        public JsonResult Tainhap(string malinhkien, string soluong, string kho, string vitri, string thoigian, string giatien, string ghichu, string khoi, string nguoichuyen, string phongban)
        {
            var tainhaphang = Models.MST_WAREHOUSE.TaiNhapkho(malinhkien.Split(':')[0], soluong, kho, vitri, thoigian, giatien, ghichu, khoi, nguoichuyen, phongban);
            return Json(tainhaphang);
        }
        public JsonResult Get_location()
        {
            var get = Models.MST_WAREHOUSE.Get_location();
            return Json(get);
        }
        [HttpPost]
        public JsonResult Load_tainhap(MST_INVENTORY para)
        {
            SQL_Connect_DB20 db = new SQL_Connect_DB20();
            var khoi = db.ReturnString("SELECT [Group_Code] FROM [COST_MANAGEMENT].[dbo].[GROUP_MEMBER] WHERE [CHR_USERID] = '" + para.UserName + "'");

            List<MST_INVENTORY> dt = MST_INVENTORY.inventory_process(para);
            dt = dt.Where(x => x.QTY_RE_IMPORT > 0).ToList();
            if (para.Group_Code != null)
            {
                dt = dt.Where(x => x.Group_Code == para.Group_Code && x.Group_Code == khoi).ToList();
            }
            if (para.MaNguyenLieu != null)
            {
                dt = dt.Where(x => x.MaNguyenLieu == para.MaNguyenLieu && x.Group_Code == khoi).ToList();
            }
            if (para.Material_Name != null)
            {
                dt = dt.Where(x => x.MaNguyenLieu!.Contains(para.MaNguyenLieu!) && x.Group_Code == khoi).ToList();
            }
            if (para.Kho != null)
            {
                dt = dt.Where(x => x.Kho == para.Kho && x.Group_Code == khoi).ToList();
            }
           
            return Json(dt);
        }
        public JsonResult del_tainhap(string id)
        {
            var Del_Tainhap = Models.MST_WAREHOUSE.Del_Tainhap(id);
            return Json(Del_Tainhap);
        }
        public JsonResult edit_tainhap(string id, string soluong, string donvi, string giatien, string kho)
        {
            var edit = Models.MST_WAREHOUSE.edit_tainhap(id, soluong, donvi, giatien, kho);
            return Json(edit);
        }
        public JsonResult _truyxuatlylich(string malinhkien, string kho)
        {
            var log = Models.KHO_NHAPXUAT._truyxuat(malinhkien,kho);
            return Json(log);
        }
        // Nhp file dang cho cac user tu file
        [HttpPost]
        public async Task<IActionResult> UploadFileApprovelUser([FromForm]IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File not Data");
            var stepAsync = await _baoGiaStepService.GetStepsApproverAsync();
            if (stepAsync == null || !stepAsync.Success || stepAsync.Data == null || stepAsync.Data.Count == 0)
            {
                return BadRequest("Không tìm thấy thông tin bước phê duyệt");
            }
            var listStep = stepAsync.Data.Where(c=> c.INT_StepNumber == 2);
            var listInsert = new List<BaoGia_Master_Approver_Send_Mail>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                // Dữ liệu bắt đầu từ dòng 2
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 1).GetString() == "" || ws.Cell(r, 1).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột
                    var seticon = ws.Cell(r, 4).GetString().Trim();
                    var mailUser = ws.Cell(r, 3).GetString().Trim();
                    // lay thong tin user
                    var userInfo = await _employeeAgentService.GetInforEmployeeByMail(mailUser);
                    if (userInfo == null || !userInfo.Success || userInfo.Data == null)
                    {
                        continue; // bỏ qua nếu không tìm thấy thông tin user
                    }
                    // lay thong tin vi tri
                    //var sectionCode = await _employeeWorkingService.GetCodeSec(seticon); await _employeeWorkingService.GetCodeCenterBySec(seticon);//
                    var listCodeCenter = await _departmentService.GetDepartmentBySectionAsync(seticon);
                    if (listCodeCenter == null || !listCodeCenter.Success || listCodeCenter.Data == null || listCodeCenter.Data.Count == 0)
                    {
                        continue; // bỏ qua nếu không tìm thấy thông tin phòng ban
                    }
                    foreach (var item in listCodeCenter.Data)
                    {
                        //foreach (var step in listStep)
                        //{
                            // nếu tên step trùng với tên step trong file thì mới tạo bản ghi, tránh trường hợp file có nhiều dòng cùng 1 user nhưng các step khác nhau
                            var dto = new BaoGia_Master_Approver_Send_Mail
                            {
                                ID = 0,
                                ID_BaoGiaStep = 1,
                                CHR_UserAdid = userInfo.Data.CHR_EMPLOYEE_ADID,
                                CHR_CodeSection = item.Cost_Center,
                                CHR_NameSection = "",
                                NVCHR_UserName = userInfo.Data.CHR_EMPLOYEE_NAME,
                                NVCHR_Position = userInfo.Data.CHR_POSITION_NAME,
                                NVCHR_StepName = "Tạo yêu cầu báo giá",
                                CHR_CreateBy = GetCurrentUserId() ?? "system",
                                CHR_CreateDate = DateTime.Now,
                                CHR_Status = "ON",
                                CHR_UpdateBy = null,
                                CHR_UpdateDate = null
                            };
                            listInsert.Add(dto);
                        //}
                    }
                }
                if (listInsert.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                var result = await _approverService.InsertMasterApproverSendMailAsync(listInsert);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Tạo tài khoản cho user theo file excel
        [HttpPost]
        public async Task<IActionResult> UploadFileCreateUser([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File not Data");
            var stepAsync = await _baoGiaStepService.GetStepsApproverAsync();
            if (stepAsync == null || !stepAsync.Success || stepAsync.Data == null || stepAsync.Data.Count == 0)
            {
                return BadRequest("Không tìm thấy thông tin bước phê duyệt");
            }
            var listInsert = new List<TM_USER>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                // Dữ liệu bắt đầu từ dòng 1
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 1).GetString() == "" || ws.Cell(r, 1).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột
                    var seticon = ws.Cell(r, 4).GetString().Trim();
                    var mailUser = ws.Cell(r, 3).GetString().Trim();
                    // lay thong tin user
                    var userInfo = await _employeeAgentService.GetInforEmployeeByMail(mailUser);
                    if (userInfo == null || !userInfo.Success || userInfo.Data == null)
                    {
                        continue; // bỏ qua nếu không tìm thấy thông tin user
                    }
                    var infor = userInfo.Data;
                    var check = listInsert.Where(x => x.CHR_USERID == infor.CHR_EMPLOYEE_ADID).FirstOrDefault();
                    if (check != null) continue; // tránh trường hợp file có nhiều dòng cùng 1 user 
                    var user = new TM_USER
                    {
                        CHR_USERID = infor.CHR_EMPLOYEE_ADID,
                        VCHR_PASSWORD = "123456",
                        DTM_CREATE = DateTime.Now,
                        CHR_CRT_USERID = GetCurrentUserId() ?? "system",
                        INT_LOCK = 0,
                        INT_LOCK_DAY = 30,
                        INT_USERID_COMMON = 0,
                        phan_quyen = 0,
                        FULLNAME = infor.CHR_EMPLOYEE_NAME,
                        CHR_EMPLOYEE_ID = infor.CHR_EMPLOYEE_ID,
                        CHR_SECTION = infor.CHR_SEC_CODE,
                        cho_phep_hoat_dong = true,
                        thoi_gian_cap_nhat = DateTime.Now
                    };
                    listInsert.Add(user);

                }
                if (listInsert.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                var result = await _tmUserService.InsertListUserAsync(listInsert);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Ham cap nhat thong tin phong ban tu file excel
        [HttpPost]
        public async Task<IActionResult> UploadFileUpdateDepartment([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File not Data");
            var stepAsync = await _baoGiaStepService.GetStepsApproverAsync();
            if (stepAsync == null || !stepAsync.Success || stepAsync.Data == null || stepAsync.Data.Count == 0)
            {
                return BadRequest("Không tìm thấy thông tin bước phê duyệt");
            }
            var listUpdate = new List<DEPARTMENT>();
            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");
                // Dữ liệu bắt đầu từ dòng 1
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
                for (int r = startRow; r <= lastRow; r++)
                {
                    if (ws.Cell(r, 1).GetString() == "" || ws.Cell(r, 1).GetString() == null)
                    {
                        break;
                    }
                    // Map theo thứ tự cột
                    var Section = ws.Cell(r, 3).GetString().Trim();
                    var CodeCost = ws.Cell(r, 1).GetString().Trim();
                    var  item = new DEPARTMENT
                    {
                        CHR_Section_Code = Section,
                        Cost_Center = CodeCost
                    };
                    listUpdate.Add(item);

                }
                if (listUpdate.Count == 0)
                {
                    return BadRequest("File không có dữ liệu hợp lệ");
                }
                var result = await _departmentService.UpdateSectionAsync(listUpdate);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi đọc file: {ex.Message}");
            }
        }
        // Xuất dữ liệu master nhà cung cấp
        [HttpGet]
        public async Task<IActionResult> ExportExcelMasterVendor()
        {
            try
            {
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "NccMaster.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: NccMaster.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                var dataAsync = await _tmNccNewService.ExportMasterVender();
                if (dataAsync == null || !dataAsync.Success || dataAsync.Data == null)
                {
                    return BadRequest("Error exporting master vendor data: " + dataAsync?.Message);
                }
                int startRow = 2;
                foreach (var item in dataAsync.Data)
                {
                    ws.Cell(startRow, 1).Value = item.NVCHR_ChungLoai ?? string.Empty;
                    ws.Cell(startRow, 2).Value = item.CHR_MaNCC ?? string.Empty;
                    ws.Cell(startRow, 3).Value = string.IsNullOrEmpty(item.ShortName) ? (item.NVCHR_SanXuat ?? string.Empty): item.ShortName;
                    ws.Cell(startRow, 4).Value = item.NVCHR_TenNCC ?? string.Empty;
                    ws.Cell(startRow, 5).Value = item.Diachi ?? string.Empty;
                    ws.Cell(startRow, 6).Value = item.CHR_Mail ?? string.Empty;
                    ws.Cell(startRow, 7).Value = item.CHR_PIC ?? string.Empty;
                    startRow++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"ExportMasterVendor_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //  Export master material
        [HttpGet]
        public async Task<IActionResult> ExportExcelMasterMaterial()
        {
            try
            {
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var templatePath = Path.Combine(root, "template", "MaterialMaster.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest("Không tìm thấy file template: MaterialMaster.xlsx");
                }

                using var fs = System.IO.File.OpenRead(templatePath);
                using var workbook = new ClosedXML.Excel.XLWorkbook(fs);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    return BadRequest("Không tìm thấy worksheet trong template");
                }
                var dataAsync = await _materialService.GetAllAsync();
                if (dataAsync == null || !dataAsync.Success || dataAsync.Data == null)
                {
                    return BadRequest("Error exporting master material data: " + dataAsync?.Message);
                }
                int startRow = 3;
                foreach (var item in dataAsync.Data)
                {
                    ws.Cell(startRow, 1).Value = item.GetLoaiHang() ?? string.Empty;
                    ws.Cell(startRow, 2).Value = item.Material_Code ?? string.Empty;
                    ws.Cell(startRow, 3).Value = item.Code_Suppiler ?? string.Empty;
                    ws.Cell(startRow, 4).Value = item.Material_Name_EN ?? string.Empty;
                    ws.Cell(startRow, 5).Value = item.Material_Name_VN ?? string.Empty;
                    ws.Cell(startRow, 6).Value = item.Category_VN ?? string.Empty;
                    ws.Cell(startRow, 7).Value = item.Group_Code ?? string.Empty;
                    ws.Cell(startRow, 8).Value = item.Shape ?? string.Empty;
                    ws.Cell(startRow, 9).Value = item.Material ?? string.Empty;
                    ws.Cell(startRow, 10).Value = item.Composition ?? string.Empty;
                    ws.Cell(startRow, 11).Value = item.Dimension ?? string.Empty;
                    ws.Cell(startRow, 12).Value = item.UsedFor ?? string.Empty;
                    ws.Cell(startRow, 13).Value = item.Purpose ?? string.Empty;
                    startRow++;
                }
                using var outStream = new MemoryStream();
                workbook.SaveAs(outStream);
                var bytes = outStream.ToArray();
                var fileName = $"ExportMasterMaterial_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // Import Excel Material
        [HttpPost]
        public async Task<IActionResult> ImportExcelMaterial([FromForm] ImportSupplierDetailDTO insertFile)
        {
            if (insertFile?.FileExcel == null)
                return BadRequest("File không hợp lệ");

            try
            {
                using var stream = insertFile.FileExcel.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null) return BadRequest("Không tìm thấy worksheet");

                var rows = new List<(string phanLoai, string codeSupplier, string nameVN, string nameEN, string category)>();
                int startRow = 2;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? startRow;

                for (int r = startRow; r <= lastRow; r++)
                {
                    var phanLoai = ws.Cell(r, 1).GetString();
                    if (string.IsNullOrWhiteSpace(phanLoai)) continue;

                    var nameVN = ws.Cell(r, 4).GetString();
                    var codeSupplier = ws.Cell(r, 3).GetString();

                    rows.Add((
                        phanLoai: phanLoai,
                        codeSupplier: codeSupplier,
                        nameVN: nameVN,
                        nameEN: ws.Cell(r, 5).GetString(),
                        category: ws.Cell(r, 6).GetString()
                    ));
                }

                if (!rows.Any())
                    return BadRequest("File không có dữ liệu hợp lệ");

                var materialNews = new List<MATERIALDTO>();
                var groups = rows.GroupBy(r => GetMaterialType(r.phanLoai));

                foreach (var group in groups)
                {
                    var latestCode = await _materialService.MaterialCodeLater(group.Key);
                    var currentNumber = ExtractNumberFromCode(latestCode.Data) + 1;

                    foreach (var row in group)
                    {
                        var newCode = GenerateMaterialCode(group.Key, currentNumber);

                        // Kiểm tra trùng trong file
                        if (materialNews.Any(m => m.Material_Code == newCode))
                            continue;

                        materialNews.Add(new MATERIALDTO
                        {
                            Material_Code = newCode,
                            Material_Name_VN = row.nameVN,
                            Material_Name_EN = row.nameEN,
                            Code_Suppiler = row.codeSupplier,
                            Category_VN = row.category,
                            Shape = "",
                            Material = "",
                            Composition = "",
                            Dimension = "",
                            UsedFor = "",
                            Purpose = "",
                            CHR_MaterialOutSide = "OUT",
                            Unit = "",
                        });
                        currentNumber++;
                    }
                }

                if (!materialNews.Any())
                    return BadRequest("Không có dữ liệu hợp lệ để import");

                await _materialService.UpdateListThongTinNoList(materialNews);
                return Ok(new { success = true, count = materialNews.Count });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        // Xác định mã loại vật liệu dựa trên phân loại (I hoặc O)
        private string GetMaterialType(string phanLoai)
        {
            if (string.IsNullOrEmpty(phanLoai)) return "O";

            var upperPhanLoai = phanLoai.ToUpper().Trim();

            if (upperPhanLoai == "I")
                return "I";

            return "O";
        }

        // Tạo mã material với prefix A hoặc I
        private string GenerateMaterialCode(string type, int number)
        {
            return $"{type}{number:D8}";
        }

        // Extract số từ mã material
        private int ExtractNumberFromCode(string materialCode)
        {
            if (string.IsNullOrEmpty(materialCode)) return 0;
            var match = Regex.Match(materialCode, @"\d+");
            return match.Success && int.TryParse(match.Value, out int number) ? number : 0;
        }
    }
}
