using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_DEPT_ESTIMATE_DEADLINE
{
    public string Cost_Center { get; set; } = null!;

    public string? Name { get; set; }

    public string? Time { get; set; }

    public string Date { get; set; } = null!;

    public DateTime? TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public int Id_Change { get; set; }

    public bool? Active { get; set; }
}
