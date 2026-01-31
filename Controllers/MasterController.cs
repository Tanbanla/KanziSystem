using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.Master;
using System.Linq;
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
        private readonly ILogger<MasterController> _logger;

        public MasterController(IMasterApproverSendMailService approverService, IBaoGiaStepService baoGiaStepService,INhomViTriService nhomViTriService,
            ITmSectionService tmSectionService, IEmployeeWorkingService employeeWorkingService, ILogger<MasterController> logger)
        {
            _approverService = approverService;
            _baoGiaStepService = baoGiaStepService;
            _tmSectionService = tmSectionService;
            _employeeWorkingService = employeeWorkingService;
            _nhomViTriService = nhomViTriService;
            _logger = logger;
        }

        public IActionResult Masters()
        {
            return View();
        }
        [HttpPost]
        public JsonResult load_vender(VENDER vder)
        {
            List<VENDER> dt = Vender_process._listVender(vder);
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
    }
}
