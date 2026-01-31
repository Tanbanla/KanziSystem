using System;
using System.Collections.Generic;

namespace PRJ_WAREHOUSE_BIVN.Models_Auto;

public partial class TM_PO_NHAPKHO_MOLD_STATUS
{
    public int ID_PO_ER_DETAIL { get; set; }

    public string CHR_PO_ER { get; set; } = null!;

    public string? CHR_RQ_CODE { get; set; }

    public string? CHR_GOOD_NAME { get; set; }

    public int? INT_QTY_PO { get; set; }

    public int? INT_QTY_RECEIVED { get; set; }

    public int? INT_STATUS_PROCESS { get; set; }

    public string? CHR_PROJECT_CODE { get; set; }

    public string? CHR_GOOD_CODE { get; set; }

    public string? CHR_DONVI { get; set; }

    public double? FLT_PRICE_VND { get; set; }

    public double? FLT_PRICE_USD { get; set; }

    public double? FLT_EXCHANCERATE { get; set; }

    public string? CHR_ACOUNT { get; set; }

    public string? CHR_PURPOSE { get; set; }

    public int? INT_PHANLOAI_HANGHOA { get; set; }

    public string? CHR_SEC_CODE { get; set; }

    public DateTime? DTM_DATE_CONFIRM { get; set; }

    public string? CHR_USER_CONFIRM { get; set; }

    public int? INT_STATUS_IN_STOCK { get; set; }

    public string? CHR_MA_HANG_HOA { get; set; }
}
