using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class REMAINDER
{
    public int Id_Remainder { get; set; }

    public string Dept { get; set; } = null!;

    public string AccountCode { get; set; } = null!;

    public DateOnly Month { get; set; }

    public string Kind { get; set; } = null!;

    public double? First { get; set; }

    public double? Last { get; set; }

    public string? Note { get; set; }

    public string Group_Code { get; set; } = null!;
}
