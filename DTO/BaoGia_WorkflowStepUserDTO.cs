using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class BaoGia_WorkflowStepUserDTO
{
    public int WorkflowStepID { get; set; }

    public string UserADID { get; set; } = null!;

    public bool CanView { get; set; }

    public bool CanProcess { get; set; }

    public bool CanApprove { get; set; }

    public bool CanReject { get; set; }

    public bool IsActive { get; set; }

}
