using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class CHECK
{
    public DateOnly? NgayCheck { get; set; }

    public string? KindCheck { get; set; }

    public int Id { get; set; }
}
