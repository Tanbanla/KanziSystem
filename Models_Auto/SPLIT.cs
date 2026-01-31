using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class SPLIT
{
    public int Id { get; set; }

    public DateOnly? DateSplit { get; set; }

    public DateTime? DateUpdate { get; set; }
}
