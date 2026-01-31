using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class WF_HISTORY
{
    public int Id_History { get; set; }

    public string Id_Process { get; set; } = null!;

    public string? Action_Content { get; set; }

    public DateTime? Create_Date { get; set; }

    public string? Create_User { get; set; }

    public string? Create_UserName { get; set; }

    public virtual WF_PROCESS Id_ProcessNavigation { get; set; } = null!;
}
