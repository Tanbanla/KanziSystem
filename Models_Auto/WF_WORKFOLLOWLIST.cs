using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_WORKFOLLOWLIST
{
    public string Id_WF { get; set; } = null!;

    public string? WF_Name { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? Create_User { get; set; }

    public bool? Locked { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<WF_PROCESS> WF_PROCESSes { get; set; } = new List<WF_PROCESS>();

    public virtual ICollection<WF_WORKFOLLOWSTEP> WF_WORKFOLLOWSTEPs { get; set; } = new List<WF_WORKFOLLOWSTEP>();
}
