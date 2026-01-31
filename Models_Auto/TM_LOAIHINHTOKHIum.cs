using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_LOAIHINHTOKHIum
{
    public int ID { get; set; }

    public string? Kind { get; set; }

    public string? Value { get; set; }

    public int? Order { get; set; }

    public string? SoTk { get; set; }

    public string? TEnTk { get; set; }

    public string? Condition { get; set; }

    public bool? CoverChiPhi { get; set; }
}
