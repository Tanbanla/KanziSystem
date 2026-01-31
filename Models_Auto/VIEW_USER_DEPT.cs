using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_USER_DEPT
{
    public string CHR_USERID { get; set; } = null!;

    public string Cost_Center { get; set; } = null!;

    public string? Name { get; set; }

    public bool? Active { get; set; }
}
