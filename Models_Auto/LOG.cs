using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class LOG
{
    public int Id { get; set; }

    public DateTime? DateCreation { get; set; }

    public string? Who { get; set; }

    public string? Explain { get; set; }
}
