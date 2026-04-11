using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ESTIMATE
{
    public int Id_Est { get; set; }

    public string Cost_Center { get; set; } = null!;

    public DateOnly Month { get; set; }

    public double? Money_Year { get; set; }

    public double? Money_Change { get; set; }

    public double? Money_ACC { get; set; }

    public string Kind { get; set; } = null!;

    public virtual DEPARTMENT_1 Cost_CenterNavigation { get; set; } = null!;
}
