using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class EXCHANGE_RATE
{
    public int Id { get; set; }

    public DateOnly DateApply { get; set; }

    public string Currency { get; set; } = null!;

    public string? Rate { get; set; }

    public string? Note { get; set; }
}
