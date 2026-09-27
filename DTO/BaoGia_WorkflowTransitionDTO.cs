namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowTransitionDTO
{
    public int TransitionID { get; set; }
    public int WorkflowID { get; set; }
    public int FromWorkflowStepID { get; set; }
    public int ToWorkflowStepID { get; set; }
    public string ActionCode { get; set; } = null!;
    public string? ConditionExpression { get; set; }
    public bool IsActive { get; set; }
}
