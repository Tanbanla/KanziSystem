namespace PRJ_WAREHOUSE_BIVN.View_Models.WorkFlowModel
{
    public class WorkFlowModel
    {
    }

    public sealed class WorkflowRolePermissionsRequest
    {
        public int WorkflowId { get; set; }
        public List<WorkflowRolePermissionRequest> Rows { get; set; } = new();
    }

    public sealed class WorkflowRolePermissionRequest
    {
        public int WorkflowStepId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanProcess { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
    }

    public sealed class WorkflowUserPermissionsRequest
    {
        public int WorkflowId { get; set; }
        public string UserADID { get; set; } = string.Empty;
        public List<WorkflowUserPermissionRequest> Rows { get; set; } = new();
    }

    public sealed class WorkflowUserPermissionRequest
    {
        public int WorkflowStepId { get; set; }
        public bool CanView { get; set; }
        public bool CanProcess { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
    }
}
