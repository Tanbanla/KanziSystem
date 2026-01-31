using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class VIEW_MATERIAL_REUSE
{
    public int Id_reuse { get; set; }

    public string Material_Code { get; set; } = null!;

    public string? Material_Name_VN { get; set; }

    public string? Unit { get; set; }

    public string? Unit_Note { get; set; }

    public string? Amount { get; set; }

    public string? Group_Code { get; set; }
}
