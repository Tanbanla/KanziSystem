using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class MAILED
{
    public int Id { get; set; }

    public string? Cost_Center { get; set; }

    public string? Kind { get; set; }

    public string? SendDate { get; set; }

    public string? Remain { get; set; }

    public DateOnly? SenDateReal { get; set; }
}
