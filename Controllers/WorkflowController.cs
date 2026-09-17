using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class WorkflowController: BaseAuthController
    {
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IBaoGiaWorkflowStageService _baoGiaWorkflowStageService;
        private readonly IBaoGiaWorkflowStepService _baoGiaWorkflowStepService;

        public WorkflowController(IBaoGiaStepService baoGiaStepService, IBaoGiaWorkflowStageService baoGiaWorkflowStageService, IBaoGiaWorkflowStepService baoGiaWorkflowStepService)
        {
            _baoGiaStepService = baoGiaStepService;
            _baoGiaWorkflowStageService = baoGiaWorkflowStageService;
            _baoGiaWorkflowStepService = baoGiaWorkflowStepService;
        }

        public IActionResult DasboadWorkFlow()
        {
            return View();
        }
        // MARK: - Workflow Stages
        public IActionResult WorkflowStages()
        {
             return View();
        }

        // MARK: - Workflow Roles
        public IActionResult WorkflowSteps() => View();

        // MARK: - Workflow Role Permissions
        public IActionResult WorkflowRolePermissions() => View();
        // MARK: - Workflow User Permissions
        public IActionResult WorkflowUserPermissions() => View();
        // MARK: - Workflow Tasks
        public IActionResult WorkflowTasks() => View();
        // MARK: - Workflow Request Detail
        public IActionResult WorkflowRequestDetail() => View();
        // MARK: - Workflow
        public IActionResult workflow()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowStages()
        {
            var stagesTask = _baoGiaWorkflowStageService.GetAllAsync();
            var stepsTask = _baoGiaWorkflowStepService.GetAllAsync();
            await Task.WhenAll(stagesTask, stepsTask);

            var response = stagesTask.Result;
            var stepsResponse = stepsTask.Result;
            if (response == null || !response.Success || stepsResponse == null || !stepsResponse.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = response?.Message ?? stepsResponse?.Message ?? "Không thể tải danh sách workflow."
                });
            }

            var stepCountsByStage = (stepsResponse.Data ?? Enumerable.Empty<BaoGia_WorkflowStepDTO>())
                .GroupBy(step => step.StageID)
                .ToDictionary(group => group.Key, group => group.Count());

            var stages = (response.Data ?? Enumerable.Empty<BaoGia_WorkflowStageDTO>())
                .OrderBy(stage => stage.StageOrder)
                .ThenBy(stage => stage.StageID)
                .Select(stage => new
                {
                    id = stage.StageID,
                    code = stage.StageCode,
                    name = stage.StageName,
                    order = stage.StageOrder,
                    isActive = stage.IsActive,
                    stepCount = stepCountsByStage.GetValueOrDefault(stage.StageID)
                });

            return Ok(new { success = true, data = stages });
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflowStage([FromBody] BaoGia_WorkflowStageDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.StageCode) || string.IsNullOrWhiteSpace(model.StageName))
            {
                return BadRequest(new { success = false, message = "Mã và tên bước lớn là bắt buộc." });
            }

            var response = await _baoGiaWorkflowStageService.AddWFStageAsync(new BaoGia_WorkflowStageDTO
            {
                StageCode = model.StageCode.Trim(), StageName = model.StageName.Trim(),
                StageOrder = model.StageOrder, IsActive = model.IsActive
            });

            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể thêm bước lớn." })
                : Ok(new { success = true, data = response.Data });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWorkflowStage(int id, [FromBody] BaoGia_WorkflowStageDTO model)
        {
            if (model == null || id <= 0 || string.IsNullOrWhiteSpace(model.StageCode) || string.IsNullOrWhiteSpace(model.StageName))
            {
                return BadRequest(new { success = false, message = "Thông tin bước lớn không hợp lệ." });
            }

            var response = await _baoGiaWorkflowStageService.UpdateWFStageAsync(new BaoGia_WorkflowStageDTO
            {
                StageID = id, StageCode = model.StageCode.Trim(), StageName = model.StageName.Trim(),
                StageOrder = model.StageOrder, IsActive = model.IsActive
            });

            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể cập nhật bước lớn." })
                : Ok(new { success = true, data = response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWorkflowStage(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Mã bước lớn không hợp lệ." });
            }

            var response = await _baoGiaWorkflowStageService.DeleteWFStageSoftAsync(id);
            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể xóa bước lớn." })
                : Ok(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowSteps()
        {
            var response = await _baoGiaStepService.GetAllAsync();
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

        [HttpGet]
        public async Task<IActionResult> GetWorkflowStepCatalog()
        {
            var response = await _baoGiaWorkflowStepService.GetAllAsync();
            if (response == null || !response.Success)
            {
                return BadRequest(new { success = false, message = response?.Message ?? "Không thể tải danh sách bước nhỏ." });
            }

            var steps = (response.Data ?? Enumerable.Empty<BaoGia_WorkflowStepDTO>())
                .OrderBy(step => step.StageID)
                .ThenBy(step => step.StepOrder)
                .ThenBy(step => step.StepCode)
                .Select(step => new
                {
                    id = step.StepID,
                    stageId = step.StageID,
                    stepOrder = step.StepOrder,
                    code = step.StepCode,
                    name = step.StepName,
                    nameEn = step.StepNameEN,
                    description = step.Description,
                    durationHours = step.DefaultDurationHours,
                    isActive = step.IsActive
                });

            return Ok(new { success = true, data = steps });
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflowStep([FromBody] BaoGia_WorkflowStepDTO model)
        {
            if (model == null || model.StageID <= 0 || model.StepOrder <= 0 || string.IsNullOrWhiteSpace(model.StepCode) || string.IsNullOrWhiteSpace(model.StepName))
            {
                return BadRequest(new { success = false, message = "Bước lớn, thứ tự, mã và tên bước là bắt buộc." });
            }

            var response = await _baoGiaWorkflowStepService.CreateWFStep(new BaoGia_WorkflowStepDTO
            {
                StageID = model.StageID,
                StepOrder = model.StepOrder,
                StepCode = model.StepCode.Trim(),
                StepName = model.StepName.Trim(),
                StepNameEN = string.IsNullOrWhiteSpace(model.StepNameEN) ? null : model.StepNameEN.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                DefaultDurationHours = model.DefaultDurationHours,
                IsActive = model.IsActive
            });

            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể thêm bước nhỏ." })
                : Ok(new { success = true, data = response.Data });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWorkflowStep(int id, [FromBody] BaoGia_WorkflowStepDTO model)
        {
            if (model == null || id <= 0 || model.StageID <= 0 || model.StepOrder <= 0 || string.IsNullOrWhiteSpace(model.StepCode) || string.IsNullOrWhiteSpace(model.StepName))
            {
                return BadRequest(new { success = false, message = "Thông tin bước nhỏ không hợp lệ." });
            }

            var response = await _baoGiaWorkflowStepService.UpdateWFStep(new BaoGia_WorkflowStepDTO
            {
                StepID = id,
                StageID = model.StageID,
                StepOrder = model.StepOrder,
                StepCode = model.StepCode.Trim(),
                StepName = model.StepName.Trim(),
                StepNameEN = string.IsNullOrWhiteSpace(model.StepNameEN) ? null : model.StepNameEN.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                DefaultDurationHours = model.DefaultDurationHours,
                IsActive = model.IsActive
            });

            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể cập nhật bước nhỏ." })
                : Ok(new { success = true, data = response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWorkflowStep(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Mã bước nhỏ không hợp lệ." });
            }

            var response = await _baoGiaWorkflowStepService.DeleteWFStep(id);
            return response == null || !response.Success
                ? BadRequest(new { success = false, message = response?.Message ?? "Không thể xóa bước nhỏ." })
                : Ok(new { success = true });
        }
    }
}
