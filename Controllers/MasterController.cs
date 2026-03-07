using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Master;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly IBaoGiaNCCService _baoGiaNCCService;
        private readonly ITmCategoryService _tmCategoryService;
        private readonly ILogger<MasterController> _logger;

        public MasterController(IMasterApproverSendMailService approverService, IBaoGiaStepService baoGiaStepService, INhomViTriService nhomViTriService,
            ITmSectionService tmSectionService, IEmployeeWorkingService employeeWorkingService, ITmNccNewService tmNccNewService,
            IBaoGiaNccCategoryService baoGiaNccCategoryService, IBaoGiaNCCService baoGiaNCCService
            , ILogger<MasterController> logger, ITmCategoryService tmCategoryService)
        {
            _approverService = approverService;
            _baoGiaStepService = baoGiaStepService;
            _tmSectionService = tmSectionService;
            _employeeWorkingService = employeeWorkingService;
            _nhomViTriService = nhomViTriService;
            _tmNccNewService = tmNccNewService;
            _baoGiaNCCService = baoGiaNCCService;
            _baoGiaNccCategoryService = baoGiaNccCategoryService;
            _logger = logger;
            _tmCategoryService = tmCategoryService;
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
            return View(vm);

        }
        // Lấy dữ liệu lúc loading màn hình SendApprover
        private async Task<SendApproverVM> LoadDataSendApprover()
        {
            var vm = new SendApproverVM();
            var sectionsTask = _tmSectionService.GetAllSectionsAsync();
            var stepsTask = _baoGiaStepService.GetStepsApproverAsync();

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
                vm.baoGiaSteps = stepsResp.Success ? stepsResp.Data : new List<BaoGia_StepDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading steps");
            }
            return vm;
        }
        // Lay thong tin phong ban
        public async Task<List<ACC_NHOMVITRIDTO>> GetNhomViTriList()
        {
            try
            {
                var result = await _nhomViTriService.GetAllAsync();
                return (List<ACC_NHOMVITRIDTO>)result.Data;
            }
            catch (Exception ex)
            {
                return new List<ACC_NHOMVITRIDTO>();
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
                int startRow = 3;
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
                        CHR_MaNCC = ws.Cell(r, 4).GetString().Trim(),
                        NVCHR_TenNCC = ws.Cell(r, 6).GetString().Trim(),
                        NVCHR_ChungLoai = ws.Cell(r, 2).GetString().Trim(),
                        NVCHR_SanXuat = ws.Cell(r, 3).GetString().Trim(),
                        CHR_Status = "Active",
                        CHR_CreateBy = user,
                        DTM_CreateBy = DateTime.Now,
                        CHR_Mail = ws.Cell(r, 8).GetString().Trim(),
                        CHR_PIC = ws.Cell(r, 10).GetString().Trim()
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
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile importRequest)
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

            return View();
        }
        // API tìm kiếm chủng loại hàng theo tên
        [HttpPost]
        public async Task<JsonResult> SearchCategoryByName([FromBody] string? req)
        {
            var resp = await _tmCategoryService.SearchCategoryByName(req ?? "");
            if (resp == null || !resp.Success)
            {
                return Json(new { success = false, message = resp?.Message ?? "Error" });
            }
            var data = resp.Data ?? new List<TM_CategoryDTO>();
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
                // Giả sử service có AddListCategory
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
            var tainhaphang = Models.MST_WAREHOUSE.TaiNhapkho(malinhkien.Split(':')[0], soluong, kho, vitri, thoigian, giatien, ghichu, khoi,nguoichuyen, phongban);
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
            List<MST_INVENTORY> dt = MST_INVENTORY.inventory_process(para);
            dt = dt.Where(x => x.QTY_RE_IMPORT > 0).ToList();
            if (para.Group_Code != null)
            {
                dt = dt.Where(x => x.Group_Code == para.Group_Code).ToList();
            }
            if (para.MaNguyenLieu != null)
            {
                dt = dt.Where(x => x.MaNguyenLieu == para.MaNguyenLieu).ToList();
            }
            if (para.Material_Name != null)
            {
                dt = dt.Where(x => x.MaNguyenLieu!.Contains(para.MaNguyenLieu!)).ToList();
            }
            if (para.Kho != null)
            {
                dt = dt.Where(x => x.Kho == para.Kho).ToList();
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
    }
}
