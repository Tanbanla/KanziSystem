using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class ACCREPORT_COMMON
{
    public string? Manhomhang { get; set; }

    public string? Tennhomhang { get; set; }

    public string? MaTong { get; set; }

    public string? TenMatongVn { get; set; }

    public string? Unit { get; set; }

    public string Phong { get; set; } = null!;

    public double? Soluong { get; set; }

    public string? Khoi { get; set; }

    public string? Ngaynhaokho { get; set; }
}
