using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class MATERIAL_MATONG
{
    public int Id_MaTong { get; set; }

    public string MaTong { get; set; } = null!;

    public string? TenMatongVn { get; set; }

    public string? TenMatongEN { get; set; }

    public string? TenMatongJp { get; set; }

    public string? Khoi { get; set; }
}
