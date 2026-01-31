using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class GET_CODE_REQUEST_BY_MATERIAL_CODE
{
    public string Code_Request { get; set; } = null!;

    public string? Material_Code { get; set; }

    public string? Status { get; set; }

    public string? Type { get; set; }
}
