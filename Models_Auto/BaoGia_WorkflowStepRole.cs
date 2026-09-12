using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowStepRole
{
    public int WorkflowStepID { get; set; }

    public string RoleCode { get; set; } = null!;

    public bool CanView { get; set; }

    public bool CanProcess { get; set; }

    public bool CanApprove { get; set; }

    public bool CanReject { get; set; }

    public virtual BaoGia_WorkflowRole RoleCodeNavigation { get; set; } = null!;

    public virtual BaoGia_WorkflowDefinitionStep WorkflowStep { get; set; } = null!;
}
