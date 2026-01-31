using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class IM_LOG
{
    public int Log_Id { get; set; }

    public string? SoPO { get; set; }

    public string? PO_Detail_Id { get; set; }

    public string? Hanhdong { get; set; }

    public DateTime? Thogian { get; set; }

    public string? Nguoicapnhat { get; set; }

    public string? Loai { get; set; }

    public string? Ghichu { get; set; }
}
