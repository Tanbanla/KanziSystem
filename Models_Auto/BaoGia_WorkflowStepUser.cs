using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowStepUser
{
    public int WorkflowStepID { get; set; }

    public string UserADID { get; set; } = null!;

    public bool CanView { get; set; }

    public bool CanProcess { get; set; }

    public bool CanApprove { get; set; }

    public bool CanReject { get; set; }

    public bool IsActive { get; set; }

    public virtual BaoGia_WorkflowDefinitionStep WorkflowStep { get; set; } = null!;
}
