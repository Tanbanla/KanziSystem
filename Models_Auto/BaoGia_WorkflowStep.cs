using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowStep
{
    public int StepID { get; set; }

    public int StageID { get; set; }

    public string StepCode { get; set; } = null!;

    public string StepName { get; set; } = null!;

    public string? StepNameEN { get; set; }

    public string? Description { get; set; }

    public int? DefaultDurationHours { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<BaoGia_WorkflowDefinitionStep> BaoGia_WorkflowDefinitionSteps { get; set; } = new List<BaoGia_WorkflowDefinitionStep>();

    public virtual BaoGia_WorkflowStage Stage { get; set; } = null!;
}
