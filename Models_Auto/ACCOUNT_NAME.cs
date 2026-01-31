using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACCOUNT_NAME
{
    public string? Mucdich { get; set; }

    public string Material_Code { get; set; } = null!;

    public string? Phongbanchiuchiphi { get; set; }

    public string? Name { get; set; }

    public string? Name_Jp { get; set; }

    public string Account_Code { get; set; } = null!;

    public string? Account_Name_EN { get; set; }

    public string? Account_Name_VN { get; set; }

    public string Group_Code { get; set; } = null!;
}
