using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class WorkflowController: BaseAuthController
    {
        private readonly IBaoGiaStepService _baoGiaStepService;

        public WorkflowController(IBaoGiaStepService baoGiaStepService)
        {
            _baoGiaStepService = baoGiaStepService;
        }

        public IActionResult DasboadWorkFlow()
        {
            return View();
        }

        public IActionResult WorkflowStages() => View();
        public IActionResult WorkflowSteps() => View();
        public IActionResult WorkflowRolePermissions() => View();
        public IActionResult WorkflowUserPermissions() => View();
        public IActionResult WorkflowTasks() => View();
        public IActionResult WorkflowRequestDetail() => View();
        public IActionResult workflow()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowSteps()
        {
            var response = await _baoGiaStepService.GetAll();
            if (response == null || !response.Success)
            {
                return BadRequest(new { success = false, message = response?.Message ?? "Không thể tải danh sách bước xử lý." });
            }

            var steps = (response.Data ?? new List<BaoGia_StepDTO>())
                .Where(step => !string.Equals(step.CHR_Status, "Inactive", StringComparison.OrdinalIgnoreCase))
                .OrderBy(step => step.INT_StepNumber ?? int.MaxValue)
                .ThenBy(step => step.ID)
                .Select(step => new
                {
                    id = step.ID,
                    name = step.CHR_StepName,
                    nameEn = step.CHR_StepNameEN,
                    nameJp = step.CHR_StepNameJP,
                    number = step.INT_StepNumber
                });

            return Ok(new { success = true, data = steps });
        }
    }
}
