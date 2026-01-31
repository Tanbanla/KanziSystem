using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_PROCESS
{
    public string Id_Process { get; set; } = null!;

    public string? Id_WF { get; set; }

    public string? Code_Request { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? Create_User { get; set; }

    public string? Create_UserName { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual REQUEST? Code_RequestNavigation { get; set; }

    public virtual WF_WORKFOLLOWLIST? Id_WFNavigation { get; set; }

    public virtual ICollection<WF_HISTORY> WF_HISTORies { get; set; } = new List<WF_HISTORY>();

    public virtual ICollection<WF_PROCESS_STEP> WF_PROCESS_STEPs { get; set; } = new List<WF_PROCESS_STEP>();
}
