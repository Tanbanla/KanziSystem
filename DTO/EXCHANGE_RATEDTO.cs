using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.DTO;

public partial class EXCHANGE_RATEDTO
{
    public int Id { get; set; }

    public DateOnly DateApply { get; set; }

    public string Currency { get; set; } = null!;

    public string? Rate { get; set; }

    public string? Note { get; set; }
}
