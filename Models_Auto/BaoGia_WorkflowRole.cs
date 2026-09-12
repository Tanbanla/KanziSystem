using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class BaoGia_WorkflowRole
{
    public string RoleCode { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<BaoGia_WorkflowStepRole> BaoGia_WorkflowStepRoles { get; set; } = new List<BaoGia_WorkflowStepRole>();
}
