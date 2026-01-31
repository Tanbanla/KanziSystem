using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ESTIMATE_CHANGE
{
    public int Id_Change { get; set; }

    public int? Id_Est { get; set; }

    public string? Cost_Center { get; set; }

    public DateOnly? Month { get; set; }

    public double? Old_Money { get; set; }

    public double? Money { get; set; }

    public string? Reason { get; set; }

    public DateTime? DateCreation { get; set; }

    public string? UserChange { get; set; }

    public string? Kind { get; set; }

    public string? NamThang { get; set; }
}
