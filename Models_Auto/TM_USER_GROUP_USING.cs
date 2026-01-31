using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_USER_GROUP_USING
{
    public string CHR_USERID { get; set; } = null!;

    public string? CHR_CODE_GROUP_NAME { get; set; }

    public string? CHR_GROUP_NAME { get; set; }

    public int? INT_EDIT_RIGHT { get; set; }
}
