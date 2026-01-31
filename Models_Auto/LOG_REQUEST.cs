using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class LOG_REQUEST
{
    public string? Code_Request { get; set; }

    public string? Action { get; set; }

    public string? Detail { get; set; }

    public string? User { get; set; }

    public DateTime? DateCreate { get; set; }
}
