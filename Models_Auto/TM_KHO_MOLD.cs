using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_KHO_MOLD
{
    public int ID_PO_ER_DETAIL { get; set; }

    public string CHR_PO_ER { get; set; } = null!;

    public string CHR_GOOD_CODE_BOOK { get; set; } = null!;

    public int INT_PHANLOAI_HANGHOA { get; set; }

    public string? CHR_PROJECT_CODE { get; set; }

    public string? CHR_KHO { get; set; }

    public int? INT_QTY { get; set; }

    public string? CHR_GOOD_NAME { get; set; }
}
