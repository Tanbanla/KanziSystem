using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class MATEIAL_REUSE
{
    public int Id_reuse { get; set; }

    public string Material_Code { get; set; } = null!;

    public string? Amount { get; set; }

    public string? Group_Code { get; set; }
}
