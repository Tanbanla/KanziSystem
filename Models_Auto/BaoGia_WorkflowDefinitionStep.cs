using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowDefinitionStep
{
    public int WorkflowStepID { get; set; }

    public int WorkflowID { get; set; }

    public int StepID { get; set; }

    public int StepOrder { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsRequired { get; set; }

    public bool AllowSkip { get; set; }

    public bool IsFinalStep { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ICollection<BaoGia_WorkflowStepRole> BaoGia_WorkflowStepRoles { get; set; } = new List<BaoGia_WorkflowStepRole>();

    public virtual ICollection<BaoGia_WorkflowStepUser> BaoGia_WorkflowStepUsers { get; set; } = new List<BaoGia_WorkflowStepUser>();

    public virtual BaoGia_WorkflowStep Step { get; set; } = null!;

    public virtual BaoGia_WorkflowDefinition Workflow { get; set; } = null!;
}
