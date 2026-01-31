using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_QR_CODE
{
    public int ID { get; set; }

    public string? CHR_PO { get; set; }

    public string CHR_PROJECT_CODE { get; set; } = null!;

    public string CHR_GOOD_NAME { get; set; } = null!;

    public string CHR_NUM_CAV { get; set; } = null!;

    public string? CHR_USER_PRINT { get; set; }

    public DateOnly DTM_PRINT { get; set; }
}
