using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class DAY_OFF
{
    public int Id { get; set; }

    public DateOnly? Dayoff { get; set; }

    public string? Description { get; set; }
}
