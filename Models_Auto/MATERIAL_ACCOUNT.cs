using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class MATERIAL_ACCOUNT
{
    public string Material_Code { get; set; } = null!;

    public string Account_Code { get; set; } = null!;

    public string Mucdich { get; set; } = null!;

    public string? Phongbanchiuchiphi { get; set; }

    public string? Group_Code { get; set; }
}
