using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models_Auto;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;
using PRJ_WAREHOUSE_BIVN.View_Models.WorkFlowModel;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class WorkflowController: BaseAuthController
    {
        private readonly IBaoGiaStepService _baoGiaStepService;
        private readonly IBaoGiaWorkflowStageService _baoGiaWorkflowStageService;
        private readonly IBaoGiaWorkflowStepService _baoGiaWorkflowStepService;
        private readonly COST_MANAGEMENTContext _context;

        public WorkflowController(IBaoGiaStepService baoGiaStepService, IBaoGiaWorkflowStageService baoGiaWorkflowStageService, IBaoGiaWorkflowStepService baoGiaWorkflowStepService, COST_MANAGEMENTContext context)
        {
            _baoGiaStepService = baoGiaStepService;
            _baoGiaWorkflowStageService = baoGiaWorkflowStageService;
            _baoGiaWorkflowStepService = baoGiaWorkflowStepService;
            _context = context;
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
        public IActionResult WorkflowDefinition()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowDefinitions(bool includeInactive = true)
        {
            var query = _context.BaoGia_WorkflowDefinitions
                .AsNoTracking()
                .Include(workflow => workflow.RequestType)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(workflow => workflow.IsActive);
            }

            var workflows = await query
                .OrderBy(workflow => workflow.RequestType.NVCHR_Name)
                .ThenBy(workflow => workflow.FlowCode)
                .Select(workflow => new
                {
                    id = workflow.WorkflowID,
                    requestTypeId = workflow.RequestTypeID,
                    requestTypeCode = workflow.RequestType.CHR_Code,
                    requestTypeName = workflow.RequestType.NVCHR_Name,
                    flowCode = workflow.FlowCode,
                    workflowName = workflow.WorkflowName,
                    isActive = workflow.IsActive,
                    stepCount = workflow.BaoGia_WorkflowDefinitionSteps.Count(step => step.IsEnabled)
                })
                .ToListAsync();

            return Ok(new { success = true, data = workflows });
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowRequestTypes()
        {
            var requestTypes = await _context.BaoGia_RequestTypes
                .AsNoTracking()
                .OrderBy(requestType => requestType.NVCHR_Name)
                .Select(requestType => new
                {
                    id = requestType.ID,
                    code = requestType.CHR_Code,
                    name = requestType.NVCHR_Name
                })
                .ToListAsync();

            return Ok(new { success = true, data = requestTypes });
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflowDefinition([FromBody] BaoGia_WorkflowDefinitionDTO model)
        {
            if (model == null || model.RequestTypeID <= 0 || string.IsNullOrWhiteSpace(model.FlowCode) || string.IsNullOrWhiteSpace(model.WorkflowName))
            {
                return BadRequest(new { success = false, message = "Loại yêu cầu, mã luồng và tên workflow là bắt buộc." });
            }

            var flowCode = model.FlowCode.Trim();
            var workflowName = model.WorkflowName.Trim();
            if (flowCode.Length > 20 || workflowName.Length > 300)
            {
                return BadRequest(new { success = false, message = "Mã luồng tối đa 20 ký tự và tên workflow tối đa 300 ký tự." });
            }

            var requestTypeExists = await _context.BaoGia_RequestTypes.AnyAsync(requestType => requestType.ID == model.RequestTypeID);
            if (!requestTypeExists)
            {
                return BadRequest(new { success = false, message = "Loại yêu cầu không tồn tại." });
            }

            var duplicate = await _context.BaoGia_WorkflowDefinitions.AnyAsync(workflow =>
                workflow.RequestTypeID == model.RequestTypeID && workflow.FlowCode == flowCode);
            if (duplicate)
            {
                return BadRequest(new { success = false, message = "Mã luồng đã tồn tại trong loại yêu cầu này." });
            }

            var entity = new BaoGia_WorkflowDefinition
            {
                RequestTypeID = model.RequestTypeID,
                FlowCode = flowCode,
                WorkflowName = workflowName,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            _context.BaoGia_WorkflowDefinitions.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = new { id = entity.WorkflowID } });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWorkflowDefinition(int id, [FromBody] BaoGia_WorkflowDefinitionDTO model)
        {
            if (id <= 0 || model == null || model.RequestTypeID <= 0 || string.IsNullOrWhiteSpace(model.FlowCode) || string.IsNullOrWhiteSpace(model.WorkflowName))
            {
                return BadRequest(new { success = false, message = "Thông tin workflow không hợp lệ." });
            }

            var flowCode = model.FlowCode.Trim();
            var workflowName = model.WorkflowName.Trim();
            if (flowCode.Length > 20 || workflowName.Length > 300)
            {
                return BadRequest(new { success = false, message = "Mã luồng tối đa 20 ký tự và tên workflow tối đa 300 ký tự." });
            }

            var entity = await _context.BaoGia_WorkflowDefinitions.FirstOrDefaultAsync(workflow => workflow.WorkflowID == id);
            if (entity == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy workflow." });
            }

            var requestTypeExists = await _context.BaoGia_RequestTypes.AnyAsync(requestType => requestType.ID == model.RequestTypeID);
            var duplicate = await _context.BaoGia_WorkflowDefinitions.AnyAsync(workflow =>
                workflow.WorkflowID != id && workflow.RequestTypeID == model.RequestTypeID && workflow.FlowCode == flowCode);
            if (!requestTypeExists || duplicate)
            {
                return BadRequest(new { success = false, message = !requestTypeExists ? "Loại yêu cầu không tồn tại." : "Mã luồng đã tồn tại trong loại yêu cầu này." });
            }

            entity.RequestTypeID = model.RequestTypeID;
            entity.FlowCode = flowCode;
            entity.WorkflowName = workflowName;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = User.Identity?.Name ?? "system";
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWorkflowDefinition(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Mã workflow không hợp lệ." });
            }

            var entity = await _context.BaoGia_WorkflowDefinitions.FirstOrDefaultAsync(workflow => workflow.WorkflowID == id);
            if (entity == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy workflow." });
            }

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = User.Identity?.Name ?? "system";
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
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



        [HttpGet]
        public async Task<IActionResult> GetWorkflowRolePermissions(int? workflowId)
        {
            var workflows = await _context.BaoGia_WorkflowDefinitions
                .AsNoTracking()
                .Where(workflow => workflow.IsActive)
                .OrderBy(workflow => workflow.WorkflowName)
                .Select(workflow => new
                {
                    id = workflow.WorkflowID,
                    code = workflow.FlowCode,
                    name = workflow.WorkflowName
                })
                .ToListAsync();

            var selectedWorkflowId = workflowId ?? workflows.FirstOrDefault()?.id;
            if (selectedWorkflowId == null)
            {
                return Ok(new { success = true, data = Array.Empty<object>(), workflows, roles = Array.Empty<object>(), workflowId = (int?)null });
            }

            if (!workflows.Any(workflow => workflow.id == selectedWorkflowId))
            {
                return BadRequest(new { success = false, message = "Workflow không tồn tại hoặc đã bị ngừng hoạt động." });
            }

            var roles = await _context.BaoGia_WorkflowRoles
                .AsNoTracking()
                .Where(role => role.IsActive)
                .OrderBy(role => role.RoleCode)
                .Select(role => new { code = role.RoleCode, name = role.RoleName })
                .ToListAsync();

            var stages = await _context.BaoGia_WorkflowStages
                .AsNoTracking()
                .Where(stage => stage.IsActive)
                .OrderBy(stage => stage.StageOrder)
                .ThenBy(stage => stage.StageID)
                .Select(stage => new
                {
                    id = stage.StageID,
                    code = stage.StageCode,
                    name = stage.StageName,
                    order = stage.StageOrder
                })
                .ToListAsync();

            var rows = await _context.BaoGia_WorkflowDefinitionSteps
                .AsNoTracking()
                .Where(definitionStep => definitionStep.WorkflowID == selectedWorkflowId
                    && definitionStep.IsEnabled
                    && definitionStep.Step.Stage.IsActive)
                .OrderBy(definitionStep => definitionStep.Step.Stage.StageOrder)
                .ThenBy(definitionStep => definitionStep.StepOrder)
                .ThenBy(definitionStep => definitionStep.WorkflowStepID)
                .Select(definitionStep => new
                {
                    id = definitionStep.WorkflowStepID,
                    order = definitionStep.StepOrder,
                    code = definitionStep.Step.StepCode,
                    name = definitionStep.Step.StepName,
                    description = definitionStep.Step.Description,
                    stageId = definitionStep.Step.StageID,
                    stageCode = definitionStep.Step.Stage.StageCode,
                    stageName = definitionStep.Step.Stage.StageName,
                    stageOrder = definitionStep.Step.Stage.StageOrder,
                    permissions = definitionStep.BaoGia_WorkflowStepRoles.Select(permission => new
                    {
                        roleCode = permission.RoleCode,
                        canView = permission.CanView,
                        canProcess = permission.CanProcess,
                        canApprove = permission.CanApprove,
                        canReject = permission.CanReject
                    })
                })
                .ToListAsync();

            return Ok(new { success = true, data = rows, workflows, roles, stages, workflowId = selectedWorkflowId });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWorkflowRolePermissions([FromBody] WorkflowRolePermissionsRequest? request)
        {
            if (request == null || request.WorkflowId <= 0 || request.Rows == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu phân quyền không hợp lệ." });
            }

            var workflowExists = await _context.BaoGia_WorkflowDefinitions
                .AsNoTracking()
                .AnyAsync(workflow => workflow.WorkflowID == request.WorkflowId && workflow.IsActive);
            if (!workflowExists)
            {
                return BadRequest(new { success = false, message = "Workflow không tồn tại hoặc đã bị ngừng hoạt động." });
            }

            var definitionStepIds = await _context.BaoGia_WorkflowDefinitionSteps
                .Where(step => step.WorkflowID == request.WorkflowId && step.IsEnabled)
                .Select(step => step.WorkflowStepID)
                .ToListAsync();
            var roleCodes = await _context.BaoGia_WorkflowRoles
                .Where(role => role.IsActive)
                .Select(role => role.RoleCode)
                .ToListAsync();
            var roleCodeSet = roleCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var validRows = request.Rows
                .Where(row => definitionStepIds.Contains(row.WorkflowStepId) && roleCodeSet.Contains(row.RoleCode))
                .GroupBy(row => new { row.WorkflowStepId, row.RoleCode })
                .Select(group => group.Last())
                .ToList();

            var stepIds = definitionStepIds.ToHashSet();
            var existingPermissions = await _context.BaoGia_WorkflowStepRoles
                .Where(permission => stepIds.Contains(permission.WorkflowStepID) && roleCodes.Contains(permission.RoleCode))
                .ToListAsync();

            foreach (var row in validRows)
            {
                var permission = existingPermissions.FirstOrDefault(item => item.WorkflowStepID == row.WorkflowStepId && item.RoleCode == row.RoleCode);
                var hasPermission = row.CanView || row.CanProcess || row.CanApprove || row.CanReject;
                if (permission == null)
                {
                    if (hasPermission)
                    {
                        _context.BaoGia_WorkflowStepRoles.Add(new BaoGia_WorkflowStepRole
                        {
                            WorkflowStepID = row.WorkflowStepId,
                            RoleCode = row.RoleCode,
                            CanView = row.CanView,
                            CanProcess = row.CanProcess,
                            CanApprove = row.CanApprove,
                            CanReject = row.CanReject
                        });
                    }
                    continue;
                }

                if (hasPermission)
                {
                    permission.CanView = row.CanView;
                    permission.CanProcess = row.CanProcess;
                    permission.CanApprove = row.CanApprove;
                    permission.CanReject = row.CanReject;
                }
                else
                {
                    _context.BaoGia_WorkflowStepRoles.Remove(permission);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã lưu ma trận phân quyền." });
        }

        [HttpGet]
        public async Task<IActionResult> SearchWorkflowUsers(string? query)
        {
            var normalizedQuery = query?.Trim();

            var userRoleQuery = _context.BaoGia_RoleUsers
                .AsNoTracking()
                .Select(permission => permission.UserAdid)
                .Distinct();

            var usersQuery = _context.TM_USERs
                .AsNoTracking()
                .Where(user => userRoleQuery.Contains(user.CHR_USERID));

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                usersQuery = usersQuery.Where(user =>
                    EF.Functions.Like(user.CHR_USERID, $"%{normalizedQuery}%")
                    || (user.FULLNAME != null && EF.Functions.Like(user.FULLNAME, $"%{normalizedQuery}%"))
                    || (user.CHR_SECTION != null && EF.Functions.Like(user.CHR_SECTION, $"%{normalizedQuery}%")));
            }

            var users = await usersQuery
                .OrderBy(user => user.CHR_USERID)
                .Take(30)
                .Select(user => new
                {
                    adid = user.CHR_USERID,
                    name = user.FULLNAME,
                    department = user.CHR_SECTION
                })
                .ToListAsync();

            return Ok(new { success = true, data = users });
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflowUserPermissions(string userADID, int? workflowId)
        {
            var normalizedUser = userADID?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUser))
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn người dùng." });
            }

            var user = await _context.TM_USERs
                .AsNoTracking()
                .Where(item => item.CHR_USERID == normalizedUser)
                .Select(item => new { adid = item.CHR_USERID, name = item.FULLNAME, department = item.CHR_SECTION })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Người dùng không tồn tại hoặc đã bị ngừng hoạt động." });
            }

            var workflows = await _context.BaoGia_WorkflowDefinitions
                .AsNoTracking()
                .Where(workflow => workflow.IsActive)
                .OrderBy(workflow => workflow.WorkflowName)
                .Select(workflow => new { id = workflow.WorkflowID, code = workflow.FlowCode, name = workflow.WorkflowName })
                .ToListAsync();
            var selectedWorkflowId = workflowId ?? workflows.FirstOrDefault()?.id;
            if (selectedWorkflowId == null)
            {
                return Ok(new { success = true, user, workflows, data = Array.Empty<object>(), workflowId = (int?)null });
            }

            if (!workflows.Any(workflow => workflow.id == selectedWorkflowId))
            {
                return BadRequest(new { success = false, message = "Workflow không tồn tại hoặc đã bị ngừng hoạt động." });
            }

            var rows = await _context.BaoGia_WorkflowDefinitionSteps
                .AsNoTracking()
                .Where(definitionStep => definitionStep.WorkflowID == selectedWorkflowId
                    && definitionStep.IsEnabled
                    && definitionStep.Step.IsActive
                    && definitionStep.Step.Stage.IsActive)
                .OrderBy(definitionStep => definitionStep.Step.Stage.StageOrder)
                .ThenBy(definitionStep => definitionStep.StepOrder)
                .ThenBy(definitionStep => definitionStep.WorkflowStepID)
                .Select(definitionStep => new
                {
                    id = definitionStep.WorkflowStepID,
                    order = definitionStep.StepOrder,
                    code = definitionStep.Step.StepCode,
                    name = definitionStep.Step.StepName,
                    description = definitionStep.Step.Description,
                    stageCode = definitionStep.Step.Stage.StageCode,
                    stageName = definitionStep.Step.Stage.StageName,
                    permissions = definitionStep.BaoGia_WorkflowStepUsers
                        .Where(permission => permission.UserADID == normalizedUser && permission.IsActive)
                        .Select(permission => new
                        {
                            canView = permission.CanView,
                            canProcess = permission.CanProcess,
                            canApprove = permission.CanApprove,
                            canReject = permission.CanReject
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { success = true, user, workflows, data = rows, workflowId = selectedWorkflowId });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWorkflowUserPermissions([FromBody] WorkflowUserPermissionsRequest? request)
        {
            if (request == null || request.WorkflowId <= 0 || string.IsNullOrWhiteSpace(request.UserADID) || request.Rows == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu phân quyền theo user không hợp lệ." });
            }

            var normalizedUser = request.UserADID.Trim();
            var userExists = await _context.TM_USERs
                .AsNoTracking()
                .AnyAsync(user => user.CHR_USERID == normalizedUser);
            var workflowExists = await _context.BaoGia_WorkflowDefinitions
                .AsNoTracking()
                .AnyAsync(workflow => workflow.WorkflowID == request.WorkflowId && workflow.IsActive);
            if (!userExists || !workflowExists)
            {
                return BadRequest(new { success = false, message = "User hoặc workflow không tồn tại hoặc đã bị ngừng hoạt động." });
            }

            var stepIds = await _context.BaoGia_WorkflowDefinitionSteps
                .Where(step => step.WorkflowID == request.WorkflowId && step.IsEnabled)
                .Select(step => step.WorkflowStepID)
                .ToListAsync();
            var stepIdSet = stepIds.ToHashSet();
            var validRows = request.Rows
                .Where(row => stepIdSet.Contains(row.WorkflowStepId))
                .GroupBy(row => row.WorkflowStepId)
                .Select(group => group.Last())
                .ToList();
            var existingPermissions = await _context.BaoGia_WorkflowStepUsers
                .Where(permission => stepIdSet.Contains(permission.WorkflowStepID) && permission.UserADID == normalizedUser)
                .ToListAsync();

            foreach (var row in validRows)
            {
                var permission = existingPermissions.FirstOrDefault(item => item.WorkflowStepID == row.WorkflowStepId);
                var hasPermission = row.CanView || row.CanProcess || row.CanApprove || row.CanReject;
                if (permission == null)
                {
                    if (hasPermission)
                    {
                        _context.BaoGia_WorkflowStepUsers.Add(new BaoGia_WorkflowStepUser
                        {
                            WorkflowStepID = row.WorkflowStepId,
                            UserADID = normalizedUser,
                            CanView = row.CanView,
                            CanProcess = row.CanProcess,
                            CanApprove = row.CanApprove,
                            CanReject = row.CanReject,
                            IsActive = true
                        });
                    }
                    continue;
                }

                if (hasPermission)
                {
                    permission.CanView = row.CanView;
                    permission.CanProcess = row.CanProcess;
                    permission.CanApprove = row.CanApprove;
                    permission.CanReject = row.CanReject;
                    permission.IsActive = true;
                }
                else
                {
                    _context.BaoGia_WorkflowStepUsers.Remove(permission);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã lưu quyền riêng theo user." });
        }
    }
}
