using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ESTIMATE_DEADLINE_CHANGE
{
    public int Id_Change { get; set; }

    public string Cost_Center { get; set; } = null!;

    public string? Time { get; set; }

    public string Date { get; set; } = null!;

    public DateTime? TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public virtual DEPARTMENT_1 Cost_CenterNavigation { get; set; } = null!;
}
